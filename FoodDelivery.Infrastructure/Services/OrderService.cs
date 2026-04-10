using AutoMapper;
using FoodDelivery.Application.Common;
using FoodDelivery.Application.DTOs;
using FoodDelivery.Application.Interfaces;
using FoodDelivery.Domain.Constants;
using FoodDelivery.Domain.Entities;
using FoodDelivery.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace FoodDelivery.Infrastructure.Services;

public class OrderService : IOrderService
{
    private readonly FoodDeliveryDbContext _context;
    private readonly IMapper _mapper;

    public OrderService(FoodDeliveryDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<Result<OrderResponse>> CreateOrderAsync(Guid userId, CreateOrderRequest request, CancellationToken cancellationToken = default)
    {
        if (request.Items == null || !request.Items.Any())
            return Result<OrderResponse>.Failure("Giỏ hàng trống.");

        if (request.Items.Any(i => i.Quantity <= 0))
            return Result<OrderResponse>.Failure("Số lượng món ăn phải lớn hơn 0.");

        using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            var restaurant = await _context.Restaurants.FindAsync(new object[] { request.RestaurantId }, cancellationToken);
            if (restaurant == null)
                return Result<OrderResponse>.Failure("Cửa hàng không tồn tại.");

            // Group items by MenuItemId to handle duplicates
            var groupedItems = request.Items
                .GroupBy(i => i.MenuItemId)
                .Select(g => new { MenuItemId = g.Key, Quantity = g.Sum(i => i.Quantity) })
                .ToList();

            var itemIds = groupedItems.Select(i => i.MenuItemId).ToList();

            var menuItems = await _context.MenuItems
                .Where(m => m.RestaurantId == request.RestaurantId && itemIds.Contains(m.Id))
                .ToDictionaryAsync(m => m.Id, cancellationToken);

            if (menuItems.Count != groupedItems.Count)
                return Result<OrderResponse>.Failure("Một số món ăn không tồn tại hoặc không thuộc cửa hàng này.");

            var order = new Order
            {
                UserId = userId,
                RestaurantId = request.RestaurantId,
                Status = OrderStatus.Pending,
                CreatedAt = DateTime.UtcNow
            };

            decimal totalAmount = 0;

            foreach (var itemReq in groupedItems)
            {
                var menuItem = menuItems[itemReq.MenuItemId];
                var orderItem = new OrderItem
                {
                    MenuItemId = menuItem.Id,
                    Quantity = itemReq.Quantity,
                    UnitPrice = menuItem.Price
                };
                order.OrderItems.Add(orderItem);
                totalAmount += menuItem.Price * itemReq.Quantity;
            }

            order.TotalAmount = totalAmount;

            _context.Orders.Add(order);
            await _context.SaveChangesAsync(cancellationToken);

            await transaction.CommitAsync(cancellationToken);

            var response = _mapper.Map<OrderResponse>(order);
            return Result<OrderResponse>.Success(response);
        }
        catch (Exception)
        {
            await transaction.RollbackAsync(cancellationToken);
            return Result<OrderResponse>.Failure("Đã xảy ra lỗi trong quá trình tạo đơn hàng.");
        }
    }

    public async Task<Result<PaginatedList<OrderResponse>>> GetCustomerOrdersAsync(Guid userId, int pageIndex = 1, int pageSize = 10, CancellationToken cancellationToken = default)
    {
        var ordersQuery = _context.Orders
            .Include(o => o.OrderItems)
            .ThenInclude(oi => oi.MenuItem)
            .Where(o => o.UserId == userId)
            .OrderByDescending(o => o.CreatedAt);

        var paginatedOrders = await PaginatedList<Order>.CreateAsync(ordersQuery, pageIndex, pageSize, cancellationToken);
        var paginatedResponses = new PaginatedList<OrderResponse>(
            _mapper.Map<List<OrderResponse>>(paginatedOrders.Items),
            paginatedOrders.TotalCount,
            paginatedOrders.PageIndex,
            pageSize
        );

        return Result<PaginatedList<OrderResponse>>.Success(paginatedResponses);
    }

    public async Task<Result<PaginatedList<OrderResponse>>> GetRestaurantOrdersAsync(Guid ownerId, int pageIndex = 1, int pageSize = 10, CancellationToken cancellationToken = default)
    {
        var restaurant = await _context.Restaurants.FirstOrDefaultAsync(r => r.OwnerId == ownerId, cancellationToken);
        if (restaurant == null)
            return Result<PaginatedList<OrderResponse>>.Failure("Bạn chưa có cửa hàng.");

        var ordersQuery = _context.Orders
            .Include(o => o.OrderItems)
            .ThenInclude(oi => oi.MenuItem)
            .Where(o => o.RestaurantId == restaurant.Id)
            .OrderByDescending(o => o.CreatedAt);

        var paginatedOrders = await PaginatedList<Order>.CreateAsync(ordersQuery, pageIndex, pageSize, cancellationToken);
        var paginatedResponses = new PaginatedList<OrderResponse>(
            _mapper.Map<List<OrderResponse>>(paginatedOrders.Items),
            paginatedOrders.TotalCount,
            paginatedOrders.PageIndex,
            pageSize
        );

        return Result<PaginatedList<OrderResponse>>.Success(paginatedResponses);
    }

    public async Task<Result<OrderResponse>> UpdateOrderStatusAsync(Guid orderId, Guid userId, string newStatus, CancellationToken cancellationToken = default)
    {
        var order = await _context.Orders
            .Include(o => o.Restaurant)
            .Include(o => o.OrderItems)
            .ThenInclude(oi => oi.MenuItem)
            .FirstOrDefaultAsync(o => o.Id == orderId, cancellationToken);

        if (order == null)
            return Result<OrderResponse>.Failure("Đơn hàng không tồn tại.");

        var user = await _context.Users.FindAsync(new object[] { userId }, cancellationToken);
        if (user == null)
            return Result<OrderResponse>.Failure("Người dùng không tồn tại.");

        // Check permissions
        bool isCustomer = order.UserId == userId;
        bool isRestaurantOwner = order.Restaurant.OwnerId == userId;

        if (isCustomer)
        {
            if (newStatus == OrderStatus.Cancelled && order.Status == OrderStatus.Pending)
            {
                order.Status = newStatus;
            }
            else
            {
                return Result<OrderResponse>.Failure("Khách hàng chỉ có thể hủy đơn hàng đang chờ xử lý.");
            }
        }
        else if (isRestaurantOwner)
        {
            var validTransitions = new[] { OrderStatus.Preparing, OrderStatus.Delivering, OrderStatus.Completed, OrderStatus.Cancelled };
            if (!validTransitions.Contains(newStatus))
                return Result<OrderResponse>.Failure($"Trạng thái '{newStatus}' không hợp lệ.");

            order.Status = newStatus;
        }
        else
        {
            return Result<OrderResponse>.Failure("Bạn không có quyền cập nhật đơn hàng này.");
        }

        _context.Orders.Update(order);
        await _context.SaveChangesAsync(cancellationToken);

        return Result<OrderResponse>.Success(_mapper.Map<OrderResponse>(order));
    }
}

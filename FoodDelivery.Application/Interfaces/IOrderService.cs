using FoodDelivery.Application.Common;
using FoodDelivery.Application.DTOs;

namespace FoodDelivery.Application.Interfaces;

public interface IOrderService
{
    Task<Result<OrderResponse>> CreateOrderAsync(Guid userId, CreateOrderRequest request, CancellationToken cancellationToken = default);
    Task<Result<PaginatedList<OrderResponse>>> GetCustomerOrdersAsync(Guid userId, int pageIndex = 1, int pageSize = 10, CancellationToken cancellationToken = default);
    Task<Result<PaginatedList<OrderResponse>>> GetRestaurantOrdersAsync(Guid ownerId, int pageIndex = 1, int pageSize = 10, CancellationToken cancellationToken = default);
    Task<Result<OrderResponse>> UpdateOrderStatusAsync(Guid orderId, Guid userId, string newStatus, CancellationToken cancellationToken = default);
}

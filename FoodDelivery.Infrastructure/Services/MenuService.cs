using AutoMapper;
using FoodDelivery.Application.Common;
using FoodDelivery.Application.DTOs;
using FoodDelivery.Application.Interfaces;
using FoodDelivery.Domain.Entities;
using FoodDelivery.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;

namespace FoodDelivery.Infrastructure.Services;

public class MenuService : IMenuService
{
    private readonly FoodDeliveryDbContext _context;
    private readonly IMapper _mapper;
    private readonly IDistributedCache _cache;

    public MenuService(FoodDeliveryDbContext context, IMapper mapper, IDistributedCache cache)
    {
        _context = context;
        _mapper = mapper;
        _cache = cache;
    }

    public async Task<Result<List<MenuItemResponse>>> GetMenuItemsAsync(Guid restaurantId, CancellationToken cancellationToken = default)
    {
        var cacheKey = $"Menu_{restaurantId}";
        var cachedMenu = await _cache.GetStringAsync(cacheKey, cancellationToken);

        if (!string.IsNullOrEmpty(cachedMenu))
        {
            var menuResponse = JsonSerializer.Deserialize<List<MenuItemResponse>>(cachedMenu);
            if (menuResponse != null)
            {
                return Result<List<MenuItemResponse>>.Success(menuResponse);
            }
        }

        var items = await _context.MenuItems
            .Where(m => m.RestaurantId == restaurantId)
            .ToListAsync(cancellationToken);

        var mappedItems = _mapper.Map<List<MenuItemResponse>>(items);

        await _cache.SetStringAsync(cacheKey, JsonSerializer.Serialize(mappedItems), new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(30)
        }, cancellationToken);

        return Result<List<MenuItemResponse>>.Success(mappedItems);
    }

    public async Task<Result<MenuItemResponse>> CreateMenuItemAsync(Guid ownerId, CreateMenuItemRequest request, CancellationToken cancellationToken = default)
    {
        var restaurant = await _context.Restaurants
            .FirstOrDefaultAsync(r => r.OwnerId == ownerId, cancellationToken);

        if (restaurant == null)
            return Result<MenuItemResponse>.Failure("Bạn chưa có cửa hàng nào để thêm món ăn.");

        var menuItem = _mapper.Map<MenuItem>(request);
        menuItem.RestaurantId = restaurant.Id;

        _context.MenuItems.Add(menuItem);
        await _context.SaveChangesAsync(cancellationToken);

        await _cache.RemoveAsync($"Menu_{restaurant.Id}", cancellationToken);

        return Result<MenuItemResponse>.Success(_mapper.Map<MenuItemResponse>(menuItem));
    }

    public async Task<Result<MenuItemResponse>> UpdateMenuItemAsync(Guid id, Guid ownerId, UpdateMenuItemRequest request, CancellationToken cancellationToken = default)
    {
        var menuItem = await _context.MenuItems
            .Include(m => m.Restaurant)
            .FirstOrDefaultAsync(m => m.Id == id, cancellationToken);

        if (menuItem == null)
            return Result<MenuItemResponse>.Failure("Không tìm thấy món ăn.");

        if (menuItem.Restaurant.OwnerId != ownerId)
            return Result<MenuItemResponse>.Failure("Bạn không có quyền cập nhật món ăn này.");

        _mapper.Map(request, menuItem);

        _context.MenuItems.Update(menuItem);
        await _context.SaveChangesAsync(cancellationToken);

        await _cache.RemoveAsync($"Menu_{menuItem.RestaurantId}", cancellationToken);

        return Result<MenuItemResponse>.Success(_mapper.Map<MenuItemResponse>(menuItem));
    }

    public async Task<Result> DeleteMenuItemAsync(Guid id, Guid ownerId, CancellationToken cancellationToken = default)
    {
        var menuItem = await _context.MenuItems
            .Include(m => m.Restaurant)
            .FirstOrDefaultAsync(m => m.Id == id, cancellationToken);

        if (menuItem == null)
            return Result.Failure("Không tìm thấy món ăn.");

        if (menuItem.Restaurant.OwnerId != ownerId)
            return Result.Failure("Bạn không có quyền xoá món ăn này.");

        menuItem.IsDeleted = true;
        _context.MenuItems.Update(menuItem);
        await _context.SaveChangesAsync(cancellationToken);

        await _cache.RemoveAsync($"Menu_{menuItem.RestaurantId}", cancellationToken);

        return Result.Success();
    }
}

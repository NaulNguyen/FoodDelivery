using FoodDelivery.Application.Common;
using FoodDelivery.Application.DTOs;

namespace FoodDelivery.Application.Interfaces;

public interface IMenuService
{
    Task<Result<List<MenuItemResponse>>> GetMenuItemsAsync(Guid restaurantId, CancellationToken cancellationToken = default);
    Task<Result<MenuItemResponse>> CreateMenuItemAsync(Guid ownerId, CreateMenuItemRequest request, CancellationToken cancellationToken = default);
    Task<Result<MenuItemResponse>> UpdateMenuItemAsync(Guid id, Guid ownerId, UpdateMenuItemRequest request, CancellationToken cancellationToken = default);
    Task<Result> DeleteMenuItemAsync(Guid id, Guid ownerId, CancellationToken cancellationToken = default);
}

using FoodDelivery.Application.Common;
using FoodDelivery.Application.DTOs;

namespace FoodDelivery.Application.Interfaces;

public interface IRestaurantService
{
    Task<Result<RestaurantResponse>> CreateAsync(Guid ownerId, CreateRestaurantRequest request, CancellationToken cancellationToken = default);
    Task<Result<RestaurantResponse>> GetMyRestaurantAsync(Guid ownerId, CancellationToken cancellationToken = default);
    Task<Result<RestaurantResponse>> UpdateAsync(Guid id, Guid ownerId, UpdateRestaurantRequest request, CancellationToken cancellationToken = default);
    Task<Result> DeleteAsync(Guid id, Guid ownerId, CancellationToken cancellationToken = default);
}

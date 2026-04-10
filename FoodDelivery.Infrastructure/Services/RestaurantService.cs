using AutoMapper;
using FoodDelivery.Application.Common;
using FoodDelivery.Application.DTOs;
using FoodDelivery.Application.Interfaces;
using FoodDelivery.Domain.Entities;
using FoodDelivery.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace FoodDelivery.Infrastructure.Services;

public class RestaurantService : IRestaurantService
{
    private readonly FoodDeliveryDbContext _context;
    private readonly IMapper _mapper;

    public RestaurantService(FoodDeliveryDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<Result<RestaurantResponse>> CreateAsync(Guid ownerId, CreateRestaurantRequest request, CancellationToken cancellationToken = default)
    {
        var existingRestaurant = await _context.Restaurants
            .FirstOrDefaultAsync(r => r.OwnerId == ownerId, cancellationToken);

        if (existingRestaurant != null)
        {
            return Result<RestaurantResponse>.Failure("Chủ cửa hàng đã có một cửa hàng. Mỗi chủ chỉ được tạo một cửa hàng.");
        }

        var restaurant = _mapper.Map<Restaurant>(request);
        restaurant.OwnerId = ownerId;

        _context.Restaurants.Add(restaurant);
        await _context.SaveChangesAsync(cancellationToken);

        return Result<RestaurantResponse>.Success(_mapper.Map<RestaurantResponse>(restaurant));
    }

    public async Task<Result<RestaurantResponse>> GetMyRestaurantAsync(Guid ownerId, CancellationToken cancellationToken = default)
    {
        var restaurant = await _context.Restaurants
            .FirstOrDefaultAsync(r => r.OwnerId == ownerId, cancellationToken);

        if (restaurant == null)
        {
            return Result<RestaurantResponse>.Failure("Không tìm thấy cửa hàng.");
        }

        return Result<RestaurantResponse>.Success(_mapper.Map<RestaurantResponse>(restaurant));
    }

    public async Task<Result<RestaurantResponse>> UpdateAsync(Guid id, Guid ownerId, UpdateRestaurantRequest request, CancellationToken cancellationToken = default)
    {
        var restaurant = await _context.Restaurants
            .FirstOrDefaultAsync(r => r.Id == id && r.OwnerId == ownerId, cancellationToken);

        if (restaurant == null)
        {
            return Result<RestaurantResponse>.Failure("Không tìm thấy cửa hàng hoặc bạn không có quyền cập nhật.");
        }

        _mapper.Map(request, restaurant);

        _context.Restaurants.Update(restaurant);
        await _context.SaveChangesAsync(cancellationToken);

        return Result<RestaurantResponse>.Success(_mapper.Map<RestaurantResponse>(restaurant));
    }

    public async Task<Result> DeleteAsync(Guid id, Guid ownerId, CancellationToken cancellationToken = default)
    {
        var restaurant = await _context.Restaurants
            .FirstOrDefaultAsync(r => r.Id == id && r.OwnerId == ownerId, cancellationToken);

        if (restaurant == null)
        {
            return Result.Failure("Không tìm thấy cửa hàng hoặc bạn không có quyền xóa.");
        }

        restaurant.IsDeleted = true;
        _context.Restaurants.Update(restaurant);
        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}

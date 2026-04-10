using AutoMapper;
using FoodDelivery.Application.DTOs;
using FoodDelivery.Domain.Entities;

namespace FoodDelivery.Application.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // Restaurant Mappings
        CreateMap<Restaurant, RestaurantResponse>();
        CreateMap<CreateRestaurantRequest, Restaurant>();
        CreateMap<UpdateRestaurantRequest, Restaurant>();

        // Menu Mappings
        CreateMap<MenuItem, MenuItemResponse>();
        CreateMap<CreateMenuItemRequest, MenuItem>();
        CreateMap<UpdateMenuItemRequest, MenuItem>();

        // Order Mappings
        CreateMap<Order, OrderResponse>();
        CreateMap<OrderItem, OrderItemResponse>()
            .ForMember(dest => dest.MenuItemName, opt => opt.MapFrom(src => src.MenuItem.Name));
    }
}

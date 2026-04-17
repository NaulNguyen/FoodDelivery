using FoodDelivery.Application.Interfaces;
using FoodDelivery.Domain.Entities;
using FoodDelivery.Infrastructure.Data;
using FoodDelivery.Infrastructure.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FoodDelivery.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection") 
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

        services.AddDbContext<FoodDeliveryDbContext>(options =>
            options.UseMySql(connectionString, new MySqlServerVersion(new Version(8, 0, 34))));

        var redisConnectionString = configuration.GetConnectionString("Redis") 
            ?? "localhost:6379";

        services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = redisConnectionString;
            options.InstanceName = "FoodDelivery_";
        });

        services.AddIdentity<User, IdentityRole<Guid>>(options =>
        {
            // Cấu hình mật khẩu
            options.Password.RequireDigit = true;
            options.Password.RequiredLength = 8;
            options.Password.RequireNonAlphanumeric = false; // Theo ý bạn vừa chọn
            options.Password.RequireUppercase = true;

            // Cấu hình User
            options.User.RequireUniqueEmail = true; // Không cho trùng Email
        })
        .AddEntityFrameworkStores<FoodDeliveryDbContext>()
        .AddDefaultTokenProviders();

        services.AddScoped<IAuthService, AuthService>();// Cần thiết để tạo token reset mật khẩu, xác thực...
        services.AddScoped<IRestaurantService, RestaurantService>();
        services.AddScoped<IMenuService, MenuService>();
        services.AddScoped<IOrderService, OrderService>();

        return services;
    }
}
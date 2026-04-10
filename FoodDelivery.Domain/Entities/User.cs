using Microsoft.AspNetCore.Identity;

namespace FoodDelivery.Domain.Entities;

public class User : IdentityUser<Guid>
{
    // Chúng ta không cần khai báo lại Id, Username, Email, PasswordHash nữa
    // vì chúng đã có sẵn trong IdentityUser
    public string Role { get; set; } = "Customer"; // e.g., Customer, Driver, RestaurantOwner

    // Refresh Token fields
    public string? RefreshToken { get; set; }
    public DateTime? RefreshTokenExpiryTime { get; set; }

    // Navigation properties
    public ICollection<Order> Orders { get; set; } = new List<Order>();
    public ICollection<Restaurant> OwnedRestaurants { get; set; } = new List<Restaurant>();
}

namespace FoodDelivery.Domain.Entities;

public class MenuItem
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    
    // Soft Delete flag
    public bool IsDeleted { get; set; } = false;

    // Foreign Key
    public Guid RestaurantId { get; set; }
    public Restaurant Restaurant { get; set; } = null!;
}

namespace FoodDelivery.Domain.Entities;

public class Restaurant
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;

    // Soft Delete flag
    public bool IsDeleted { get; set; } = false;

    // Foreign Key
    public Guid OwnerId { get; set; }
    public User Owner { get; set; } = null!;

    // Navigation properties
    public ICollection<MenuItem> MenuItems { get; set; } = new List<MenuItem>();
    public ICollection<Order> Orders { get; set; } = new List<Order>();
}

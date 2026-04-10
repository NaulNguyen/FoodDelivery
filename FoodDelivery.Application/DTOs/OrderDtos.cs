namespace FoodDelivery.Application.DTOs;

public class CreateOrderRequest
{
    public Guid RestaurantId { get; set; }
    public List<OrderItemRequest> Items { get; set; } = new();
}

public class OrderItemRequest
{
    public Guid MenuItemId { get; set; }
    public int Quantity { get; set; }
}

public class UpdateOrderStatusRequest
{
    public string Status { get; set; } = string.Empty;
}

public class OrderResponse
{
    public Guid Id { get; set; }
    public DateTime CreatedAt { get; set; }
    public decimal TotalAmount { get; set; }
    public string Status { get; set; } = string.Empty;
    public Guid UserId { get; set; }
    public Guid RestaurantId { get; set; }
    public Guid? DriverId { get; set; }
    public List<OrderItemResponse> OrderItems { get; set; } = new();
}

public class OrderItemResponse
{
    public Guid Id { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public Guid MenuItemId { get; set; }
    public string MenuItemName { get; set; } = string.Empty;
}

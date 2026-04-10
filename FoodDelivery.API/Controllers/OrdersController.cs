using FoodDelivery.Application.DTOs;
using FoodDelivery.Application.Interfaces;
using FoodDelivery.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace FoodDelivery.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class OrdersController : ControllerBase
{
    private readonly IOrderService _orderService;

    public OrdersController(IOrderService orderService)
    {
        _orderService = orderService;
    }

    private Guid CurrentUserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    // Khách hàng đặt đồ ăn
    [HttpPost]
    [Authorize(Roles = AppRoles.Customer)]
    public async Task<IActionResult> PlaceOrder([FromBody] CreateOrderRequest request, CancellationToken cancellationToken)
    {
        var result = await _orderService.CreateOrderAsync(CurrentUserId, request, cancellationToken);
        if (!result.Succeeded)
            return BadRequest(new { errors = result.Errors });

        return Ok(result.Data);
    }

    // Khách hàng xem danh sách đơn hàng đã đặt
    [HttpGet("my-orders")]
    [Authorize(Roles = AppRoles.Customer)]
    public async Task<IActionResult> GetCustomerOrders([FromQuery] int pageIndex = 1, [FromQuery] int pageSize = 10, CancellationToken cancellationToken = default)
    {
        var result = await _orderService.GetCustomerOrdersAsync(CurrentUserId, pageIndex, pageSize, cancellationToken);
        if (!result.Succeeded)
            return BadRequest(new { errors = result.Errors });

        return Ok(result.Data);
    }

    // Chủ quán xem danh sách đơn hàng của quán mình
    [HttpGet("restaurant-orders")]
    [Authorize(Roles = AppRoles.RestaurantOwner)]
    public async Task<IActionResult> GetRestaurantOrders([FromQuery] int pageIndex = 1, [FromQuery] int pageSize = 10, CancellationToken cancellationToken = default)
    {
        var result = await _orderService.GetRestaurantOrdersAsync(CurrentUserId, pageIndex, pageSize, cancellationToken);
        if (!result.Succeeded)
            return BadRequest(new { errors = result.Errors });

        return Ok(result.Data);
    }

    // Khách hàng hoặc chủ quán cập nhật trạng thái đơn hàng
    [HttpPatch("{id}/status")]
    public async Task<IActionResult> UpdateOrderStatus(Guid id, [FromBody] UpdateOrderStatusRequest request, CancellationToken cancellationToken)
    {
        var result = await _orderService.UpdateOrderStatusAsync(id, CurrentUserId, request.Status, cancellationToken);
        if (!result.Succeeded)
            return BadRequest(new { errors = result.Errors });

        return Ok(result.Data);
    }

    // Tài xế được nhận đơn
    [HttpPut("{id}/accept")]
    [Authorize(Roles = AppRoles.Driver)]
    public IActionResult AcceptOrder(Guid id)
    {
        return Ok(new { message = $"Tài xế đã nhận đơn {id}" });
    }
}

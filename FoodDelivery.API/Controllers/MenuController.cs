using FoodDelivery.Application.DTOs;
using FoodDelivery.Application.Interfaces;
using FoodDelivery.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace FoodDelivery.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MenuController : ControllerBase
{
    private readonly IMenuService _menuService;

    public MenuController(IMenuService menuService)
    {
        _menuService = menuService;
    }

    private Guid CurrentUserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    // Bất kỳ ai cũng có thể xem thực đơn của một cửa hàng cụ thể
    [HttpGet("restaurant/{restaurantId}")]
    public async Task<IActionResult> GetMenu(Guid restaurantId, CancellationToken cancellationToken)
    {
        var result = await _menuService.GetMenuItemsAsync(restaurantId, cancellationToken);
        if (!result.Succeeded)
            return BadRequest(new { errors = result.Errors });

        return Ok(result.Data);
    }

    // Chỉ Chủ quán mới được thêm thực đơn
    [HttpPost]
    [Authorize(Roles = AppRoles.RestaurantOwner)]
    public async Task<IActionResult> AddMenuItem([FromBody] CreateMenuItemRequest request, CancellationToken cancellationToken)
    {
        var result = await _menuService.CreateMenuItemAsync(CurrentUserId, request, cancellationToken);
        if (!result.Succeeded)
            return BadRequest(new { errors = result.Errors });

        return Ok(result.Data);
    }

    // Chỉ Chủ quán mới được sửa thực đơn
    [HttpPut("{id}")]
    [Authorize(Roles = AppRoles.RestaurantOwner)]
    public async Task<IActionResult> UpdateMenuItem(Guid id, [FromBody] UpdateMenuItemRequest request, CancellationToken cancellationToken)
    {
        var result = await _menuService.UpdateMenuItemAsync(id, CurrentUserId, request, cancellationToken);
        if (!result.Succeeded)
            return BadRequest(new { errors = result.Errors });

        return Ok(result.Data);
    }

    // Chỉ Chủ quán mới được xoá thực đơn
    [HttpDelete("{id}")]
    [Authorize(Roles = AppRoles.RestaurantOwner)]
    public async Task<IActionResult> DeleteMenuItem(Guid id, CancellationToken cancellationToken)
    {
        var result = await _menuService.DeleteMenuItemAsync(id, CurrentUserId, cancellationToken);
        if (!result.Succeeded)
            return BadRequest(new { errors = result.Errors });

        return Ok(new { message = "Xóa món ăn thành công" });
    }
}

using FoodDelivery.Application.DTOs;
using FoodDelivery.Application.Interfaces;
using FoodDelivery.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace FoodDelivery.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = AppRoles.RestaurantOwner)]
public class RestaurantsController : ControllerBase
{
    private readonly IRestaurantService _restaurantService;

    public RestaurantsController(IRestaurantService restaurantService)
    {
        _restaurantService = restaurantService;
    }

    private Guid CurrentUserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpPost]
    public async Task<IActionResult> CreateRestaurant([FromBody] CreateRestaurantRequest request, CancellationToken cancellationToken)
    {
        var result = await _restaurantService.CreateAsync(CurrentUserId, request, cancellationToken);
        if (!result.Succeeded)
        {
            return BadRequest(new ProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Tạo cửa hàng thất bại",
                Detail = "Dữ liệu không hợp lệ",
                Extensions = { ["errors"] = result.Errors }
            });
        }

        return CreatedAtAction(nameof(GetMyRestaurant), null, result.Data);
    }

    [HttpGet("my-restaurant")]
    public async Task<IActionResult> GetMyRestaurant(CancellationToken cancellationToken)
    {
        var result = await _restaurantService.GetMyRestaurantAsync(CurrentUserId, cancellationToken);
        if (!result.Succeeded)
            return NotFound(new { errors = result.Errors });

        return Ok(result.Data);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateRestaurant(Guid id, [FromBody] UpdateRestaurantRequest request, CancellationToken cancellationToken)
    {
        var result = await _restaurantService.UpdateAsync(id, CurrentUserId, request, cancellationToken);
        if (!result.Succeeded)
            return BadRequest(new { errors = result.Errors });

        return Ok(result.Data);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteRestaurant(Guid id, CancellationToken cancellationToken)
    {
        var result = await _restaurantService.DeleteAsync(id, CurrentUserId, cancellationToken);
        if (!result.Succeeded)
            return BadRequest(new { errors = result.Errors });

        return Ok(new { message = "Xóa cửa hàng thành công" });
    }
}

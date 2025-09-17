using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using LeafSide.Application.Services.Abstract;
using LeafSide.Domain.Repositories;
using System.Security.Claims;

namespace LeafSide.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UserStatsController : ControllerBase
{
    private readonly IOrderRepository _orderRepository;
    private readonly ICartRepository _cartRepository;

    public UserStatsController(IOrderRepository orderRepository, ICartRepository cartRepository)
    {
        _orderRepository = orderRepository;
        _cartRepository = cartRepository;
    }

    [HttpGet("stats")]
    public async Task<IActionResult> GetUserStats()
    {
        try
        {
            Console.WriteLine("UserStatsController - GetUserStats called");
            Console.WriteLine($"UserStatsController - User claims: {string.Join(", ", User.Claims.Select(c => $"{c.Type}={c.Value}"))}");
            
            var userIdString = User.FindFirst("sub")?.Value;
            Console.WriteLine($"UserStatsController - User ID from sub claim: {userIdString}");
            
            if (string.IsNullOrEmpty(userIdString))
            {
                // Попробуем альтернативный способ получения ID
                userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                Console.WriteLine($"UserStatsController - User ID from NameIdentifier claim: {userIdString}");
            }
            
            if (string.IsNullOrEmpty(userIdString) || !Guid.TryParse(userIdString, out var userId))
            {
                Console.WriteLine("UserStatsController - Invalid user ID");
                return Unauthorized("Пользователь не найден");
            }

            // Получаем заказы пользователя
            var orders = await _orderRepository.GetByUserIdAsync(userId);
            var totalOrders = orders.Count();
            var totalBooksPurchased = orders.SelectMany(o => o.Items).Sum(oi => oi.Quantity);

            // Получаем корзину пользователя
            var cart = await _cartRepository.GetByUserIdAsync(userId);
            var itemsInCart = cart?.Items?.Count ?? 0;

            // TODO: Добавить избранное когда будет реализовано
            var favoritesCount = 0;

            var stats = new
            {
                totalOrders = totalOrders,
                totalBooksPurchased = totalBooksPurchased,
                itemsInCart = itemsInCart,
                favoritesCount = favoritesCount
                
            };

            Console.WriteLine($"UserStatsController - Returning stats: {System.Text.Json.JsonSerializer.Serialize(stats)}");
            return Ok(stats);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Ошибка при получении статистики", details = ex.Message });
        }
    }
}

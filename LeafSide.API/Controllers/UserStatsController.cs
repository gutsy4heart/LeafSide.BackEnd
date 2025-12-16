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
    private readonly IFavoriteService _favoriteService;

    public UserStatsController(IOrderRepository orderRepository, ICartRepository cartRepository, IFavoriteService favoriteService)
    {
        _orderRepository = orderRepository;
        _cartRepository = cartRepository;
        _favoriteService = favoriteService;
    }

    [HttpGet("stats")]
    public async Task<IActionResult> GetUserStats()
    {
        try
        {
            var userIdString = User.FindFirst("sub")?.Value;
            
            if (string.IsNullOrEmpty(userIdString))
            {
                // Попробуем альтернативный способ получения ID
                userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            }
            
            if (string.IsNullOrEmpty(userIdString) || !Guid.TryParse(userIdString, out var userId))
            {
                return Unauthorized("Пользователь не найден");
            }

            // Получаем заказы пользователя
            var orders = await _orderRepository.GetByUserIdAsync(userId);
            var totalOrders = orders.Count();
            var totalBooksPurchased = orders.SelectMany(o => o.Items).Sum(oi => oi.Quantity);

            // Получаем корзину пользователя
            var cart = await _cartRepository.GetByUserIdAsync(userId);
            var itemsInCart = cart?.Items?.Count ?? 0;

            // Получаем количество избранных книг
            var favoritesCount = await _favoriteService.GetCountByUserIdAsync(userId);

            var stats = new
            {
                totalOrders = totalOrders,
                totalBooksPurchased = totalBooksPurchased,
                itemsInCart = itemsInCart,
                favoritesCount = favoritesCount
                
            };

            return Ok(stats);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Ошибка при получении статистики", details = ex.Message });
        }
    }
}

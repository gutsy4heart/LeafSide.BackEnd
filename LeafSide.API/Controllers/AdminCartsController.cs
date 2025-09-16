using LeafSide.API.Responses.Cart;
using LeafSide.Application.Services.Abstract;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LeafSide.API.Controllers;

[ApiController]
[Route("api/admin/carts")]
[Authorize(Roles = "Admin")]
public class AdminCartsController : ControllerBase
{
    private readonly ICartService _cartService;

    public AdminCartsController(ICartService cartService)
    {
        _cartService = cartService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<AdminCartResponse>>> GetAll()
    {
        try
        {
            var carts = await _cartService.GetAllAsync();
            var response = carts.Select(cart => new AdminCartResponse
            {
                id = cart.Id,
                userId = cart.UserId,
                items = cart.Items.Select(item => new AdminCartItemResponse
                {
                    id = item.Id,
                    bookId = item.BookId,
                    quantity = item.Quantity,
                    priceSnapshot = item.PriceSnapshot
                }).ToList()
            });

            return Ok(response);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Ошибка при получении корзин", details = ex.Message });
        }
    }

    [HttpGet("user/{userId:guid}")]
    public async Task<ActionResult<AdminCartResponse>> GetByUserId(Guid userId)
    {
        try
        {
            var cart = await _cartService.GetOrCreateAsync(userId);
            var response = new AdminCartResponse
            {
                id = cart.Id,
                userId = cart.UserId,
                items = cart.Items.Select(i => new AdminCartItemResponse
                {
                    id = i.Id,
                    bookId = i.BookId,
                    quantity = i.Quantity,
                    priceSnapshot = i.PriceSnapshot
                }).ToList()
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }
}

public class AdminCartResponse
{
    public Guid id { get; set; }
    public Guid userId { get; set; }
    public List<AdminCartItemResponse> items { get; set; } = new();
}

public class AdminCartItemResponse
{
    public Guid id { get; set; }
    public Guid bookId { get; set; }
    public int quantity { get; set; }
    public decimal? priceSnapshot { get; set; }
}

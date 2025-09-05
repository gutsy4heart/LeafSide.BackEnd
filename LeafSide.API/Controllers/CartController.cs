using System.Security.Claims;
using LeafSide.API.Requests.Cart;
using LeafSide.API.Responses.Cart;
using LeafSide.Application.Services.Abstract;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LeafSide.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CartController : ControllerBase
{
    private readonly ICartService _cartService;

    public CartController(ICartService cartService)
    {
        _cartService = cartService;
    }

    private Guid GetUserId()
    {
        var sub = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
        return Guid.Parse(sub!);
    }

    [HttpGet]
    public async Task<ActionResult<CartResponse>> Get()
    {
        var userId = GetUserId();
        var cart = await _cartService.GetOrCreateAsync(userId);
        var response = new CartResponse
        {
            Id = cart.Id,
            Items = cart.Items.Select(i => new CartItemResponse
            {
                BookId = i.BookId,
                Quantity = i.Quantity,
                PriceSnapshot = i.PriceSnapshot
            })
        };
        return Ok(response);
    }

    [HttpPost("items")]
    public async Task<ActionResult<CartResponse>> AddOrUpdate(AddOrUpdateItemRequest request)
    {
        var userId = GetUserId();
        var cart = await _cartService.AddOrUpdateItemAsync(userId, request.BookId, request.Quantity);
        var response = new CartResponse
        {
            Id = cart.Id,
            Items = cart.Items.Select(i => new CartItemResponse
            {
                BookId = i.BookId,
                Quantity = i.Quantity,
                PriceSnapshot = i.PriceSnapshot
            })
        };
        return Ok(response);
    }

    [HttpDelete("items/{bookId:guid}")]
    public async Task<IActionResult> Remove(Guid bookId)
    {
        var userId = GetUserId();
        var ok = await _cartService.RemoveItemAsync(userId, bookId);
        if (!ok) return NotFound();
        return NoContent();
    }

    [HttpDelete]
    public async Task<IActionResult> Clear()
    {
        var userId = GetUserId();
        var ok = await _cartService.ClearAsync(userId);
        if (!ok) return NotFound();
        return NoContent();
    }
}



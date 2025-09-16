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
        try
        {
            var userId = GetUserId();
            Console.WriteLine($"CartController - Get called for userId: {userId}");
            
            var cart = await _cartService.GetOrCreateAsync(userId);
            Console.WriteLine($"CartController - Cart found with {cart.Items.Count} items");
            
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
            
            Console.WriteLine($"CartController - Response prepared with {response.Items.Count()} items");
            return Ok(response);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"CartController - Get error: {ex.Message}");
            return StatusCode(500, new { error = ex.Message });
        }
    }

    [HttpPost("items")]
    public async Task<ActionResult<CartResponse>> AddOrUpdate(AddOrUpdateItemRequest request)
    {
        try
        {
            Console.WriteLine($"CartController - AddOrUpdate called with BookId: {request.BookId}, Quantity: {request.Quantity}");
            
            var userId = GetUserId();
            Console.WriteLine($"CartController - UserId: {userId}");
            
            var cart = await _cartService.AddOrUpdateItemAsync(userId, request.BookId, request.Quantity);
            Console.WriteLine($"CartController - Cart updated, Items count: {cart.Items.Count}");
            
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
            
            Console.WriteLine($"CartController - Response prepared with {response.Items.Count()} items");
            return Ok(response);
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("Book not found"))
        {
            Console.WriteLine($"CartController - Book not found: {request.BookId}");
            return NotFound(new { error = "Книга не найдена" });
        }
        catch (ArgumentOutOfRangeException ex) when (ex.ParamName == "quantity")
        {
            Console.WriteLine($"CartController - Invalid quantity: {request.Quantity}");
            return BadRequest(new { error = "Количество должно быть больше 0" });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"CartController - Error: {ex.Message}");
            Console.WriteLine($"CartController - Stack trace: {ex.StackTrace}");
            return StatusCode(500, new { error = ex.Message });
        }
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



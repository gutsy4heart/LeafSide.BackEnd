using System.Security.Claims;
using LeafSide.API.Requests.Favorites;
using LeafSide.API.Responses.Favorites;
using LeafSide.API.Responses.Books;
using LeafSide.Application.Services.Abstract;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LeafSide.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class FavoritesController : ControllerBase
{
    private readonly IFavoriteService _favoriteService;

    public FavoritesController(IFavoriteService favoriteService)
    {
        _favoriteService = favoriteService;
    }

    private Guid GetUserId()
    {
        var sub = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
        return Guid.Parse(sub!);
    }

    private static BookResponse? MapBook(LeafSide.Domain.Entities.Book? book)
    {
        if (book is null) return null;

        return new BookResponse
        {
            Id = book.Id,
            Title = book.Title,
            Description = book.Description,
            Author = book.Author,
            Genre = book.Genre,
            Publishing = book.Publishing,
            Created = book.Created,
            ImageUrl = book.ImageUrl,
            Price = book.Price,
            Isbn = book.Isbn,
            Language = book.Language,
            PageCount = book.PageCount,
            IsAvailable = book.IsAvailable,
            CreatedAt = book.CreatedAt,
            UpdatedAt = book.UpdatedAt
        };
    }

    private static FavoriteResponse MapFavorite(LeafSide.Domain.Entities.Favorite favorite)
    {
        return new FavoriteResponse
        {
            Id = favorite.Id,
            BookId = favorite.BookId,
            CreatedAt = favorite.CreatedAt,
            Book = MapBook(favorite.Book)
        };
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<FavoriteResponse>>> Get()
    {
        try
        {
            var userId = GetUserId();
            var favorites = await _favoriteService.GetByUserIdAsync(userId);
            
            var response = favorites.Select(MapFavorite);
            
            return Ok(response);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }

    [HttpPost]
    public async Task<ActionResult<FavoriteResponse>> Add([FromBody] AddFavoriteRequest request)
    {
        try
        {
            var userId = GetUserId();
            var favorite = await _favoriteService.AddAsync(userId, request.BookId);
            
            return Ok(MapFavorite(favorite));
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("not found"))
        {
            return NotFound(new { error = "Книга не найдена" });
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("already in favorites"))
        {
            return Conflict(new { error = "Книга уже в избранном" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }

    [HttpDelete("{bookId:guid}")]
    public async Task<IActionResult> Remove(Guid bookId)
    {
        try
        {
            var userId = GetUserId();
            var removed = await _favoriteService.RemoveAsync(userId, bookId);
            
            if (!removed)
                return NotFound(new { error = "Книга не найдена в избранном" });
            
            return NoContent();
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }

    [HttpDelete]
    public async Task<IActionResult> Clear()
    {
        try
        {
            var userId = GetUserId();
            var removedCount = await _favoriteService.RemoveAllByUserIdAsync(userId);
            return Ok(new { removedCount });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }

    [HttpGet("{bookId:guid}/check")]
    public async Task<ActionResult<bool>> Check(Guid bookId)
    {
        try
        {
            var userId = GetUserId();
            var isFavorite = await _favoriteService.IsFavoriteAsync(userId, bookId);
            return Ok(new { isFavorite });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }

    [HttpGet("count")]
    public async Task<ActionResult<int>> GetCount()
    {
        try
        {
            var userId = GetUserId();
            var count = await _favoriteService.GetCountByUserIdAsync(userId);
            return Ok(new { count });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }
}

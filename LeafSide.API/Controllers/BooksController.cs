using LeafSide.Application.Services.Abstract;
using LeafSide.Domain.Entities;
using LeafSide.API.Requests.Books;
using LeafSide.API.Responses.Books;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace LeafSide.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BooksController : ControllerBase
{
    private readonly IBookService _bookService;

    public BooksController(IBookService bookService)
    {
        _bookService = bookService;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<IEnumerable<BookResponse>>> GetAll()
    {
        var books = await _bookService.GetAllAsync();
        var response = books.Select(b => new BookResponse
        {
            Id = b.Id,
            Title = b.Title,
            Description = b.Description,
            Author = b.Author,
            Genre = b.Genre,
            Publishing = b.Publishing,
            Created = b.Created,
            ImageUrl = b.ImageUrl,
            Price = b.Price,
            Isbn = b.Isbn,
            Language = b.Language,
            PageCount = b.PageCount,
            IsAvailable = b.IsAvailable,
            CreatedAt = b.CreatedAt,
            UpdatedAt = b.UpdatedAt
        });
        return Ok(response);
    }

    [HttpGet("{id:guid}")]
    [AllowAnonymous]
    public async Task<ActionResult<BookResponse>> GetById([FromRoute] Guid id)
    {
        var book = await _bookService.GetByIdAsync(id);
        if (book is null) return NotFound();
        var response = new BookResponse
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
        return Ok(response);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<BookResponse>> Create([FromForm] CreateBookRequest request)
    {
        var toCreate = new Book
        {
            Title = request.Title,
            Description = request.Description,
            Author = request.Author,
            Genre = request.Genre,
            Publishing = request.Publishing,
            Created = request.Created,
            ImageUrl = request.ImageUrl,
            Price = request.Price,
            Isbn = request.Isbn ?? string.Empty,
            Language = request.Language ?? "Russian",
            PageCount = request.PageCount,
            IsAvailable = request.IsAvailable,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        var created = await _bookService.AddAsync(toCreate);
        var response = new BookResponse
        {
            Id = created.Id,
            Title = created.Title,
            Description = created.Description,
            Author = created.Author,
            Genre = created.Genre,
            Publishing = created.Publishing,
            Created = created.Created,
            ImageUrl = created.ImageUrl,
            Price = created.Price,
            Isbn = created.Isbn,
            Language = created.Language,
            PageCount = created.PageCount,
            IsAvailable = created.IsAvailable,
            CreatedAt = created.CreatedAt,
            UpdatedAt = created.UpdatedAt
        };
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, response);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<BookResponse>> Update([FromRoute] Guid id, [FromForm] UpdateBookRequest request)
    {
        var existing = await _bookService.GetByIdAsync(id);
        if (existing is null) return NotFound();
        existing.Title = request.Title;
        existing.Description = request.Description;
        existing.Author = request.Author;
        existing.Genre = request.Genre;
        existing.Publishing = request.Publishing;
        existing.Created = request.Created;
        existing.ImageUrl = request.ImageUrl;
        existing.Price = request.Price;
        existing.Isbn = request.Isbn ?? existing.Isbn;
        existing.Language = request.Language ?? existing.Language;
        existing.PageCount = request.PageCount;
        existing.IsAvailable = request.IsAvailable;
        existing.UpdatedAt = DateTime.UtcNow;
        var updated = await _bookService.UpdateAsync(existing);
        if (updated is null) return NotFound();
        var response = new BookResponse
        {
            Id = updated.Id,
            Title = updated.Title,
            Description = updated.Description,
            Author = updated.Author,
            Genre = updated.Genre,
            Publishing = updated.Publishing,
            Created = updated.Created,
            ImageUrl = updated.ImageUrl,
            Price = updated.Price,
            Isbn = updated.Isbn,
            Language = updated.Language,
            PageCount = updated.PageCount,
            IsAvailable = updated.IsAvailable,
            CreatedAt = updated.CreatedAt,
            UpdatedAt = updated.UpdatedAt
        };
        return Ok(response);
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete([FromRoute] Guid id)
    {
        var deleted = await _bookService.DeleteAsync(id);
        if (!deleted) return NotFound();
        return NoContent();
    }
}



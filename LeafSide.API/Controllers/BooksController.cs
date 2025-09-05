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
            Price = b.Price
        });
        return Ok(response);
    }

    [HttpGet("{id:guid}")]
    [AllowAnonymous]
    public async Task<ActionResult<BookResponse>> GetById([FromForm]Guid id)
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
            Price = book.Price
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
            Price = request.Price
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
            Price = created.Price
        };
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, response);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<BookResponse>> Update([FromForm]Guid id, [FromForm] UpdateBookRequest request)
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
            Price = updated.Price
        };
        return Ok(response);
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete([FromForm]Guid id)
    {
        var deleted = await _bookService.DeleteAsync(id);
        if (!deleted) return NotFound();
        return NoContent();
    }
}



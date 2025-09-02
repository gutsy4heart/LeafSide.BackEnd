using LeafSide.Application.Services.Abstract;
using LeafSide.Domain.Entities;
using Microsoft.AspNetCore.Mvc;

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
    public async Task<ActionResult<IEnumerable<Book>>> GetAll()
    {
        var books = await _bookService.GetAllAsync();
        return Ok(books);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<Book>> GetById(Guid id)
    {
        var book = await _bookService.GetByIdAsync(id);
        if (book is null) return NotFound();
        return Ok(book);
    }

    [HttpPost]
    public async Task<ActionResult<Book>> Create(Book book)
    {
        var created = await _bookService.AddAsync(book);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<Book>> Update(Guid id, Book book)
    {
        if (id != book.Id) return BadRequest("Id mismatch");
        var updated = await _bookService.UpdateAsync(book);
        if (updated is null) return NotFound();
        return Ok(updated);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var deleted = await _bookService.DeleteAsync(id);
        if (!deleted) return NotFound();
        return NoContent();
    }
}



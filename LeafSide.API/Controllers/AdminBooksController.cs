using LeafSide.API.Requests.Admin;
using LeafSide.API.Responses.Admin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;

namespace LeafSide.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class AdminBooksController : ControllerBase
{
    private static readonly List<BookResponse> _books;
    [HttpGet("books")]
    public ActionResult<List<BookResponse>> GetAllBooks()
    {
        return Ok(_books);
    }

    [HttpGet("books/{bookId}")]
    public ActionResult<BookResponse> GetBook(string bookId)
    {
        var book = _books.FirstOrDefault(b => b.Id == bookId);
        if (book == null)
        {
            return NotFound("Книга не найдена");
        }
        return Ok(book);
    }

    [HttpPost("books")]
    public ActionResult<BookResponse> CreateBook([FromBody] CreateBookRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Title))
        {
            return BadRequest("Название книги обязательно");
        }

        if (string.IsNullOrWhiteSpace(request.Author))
        {
            return BadRequest("Автор книги обязателен");
        }

        if (request.Price < 0)
        {
            return BadRequest("Цена не может быть отрицательной");
        }

        if (request.PublishedYear < 1000 || request.PublishedYear > DateTime.Now.Year + 1)
        {
            return BadRequest("Некорректный год издания");
        }

        var book = new BookResponse
        {
            Id = (_books.Count + 1).ToString(),
            Title = request.Title,
            Author = request.Author,
            Description = request.Description ?? string.Empty,
            Isbn = request.Isbn ?? string.Empty,
            PublishedYear = request.PublishedYear,
            Genre = request.Genre ?? "Other",
            Language = request.Language ?? "Russian",
            PageCount = request.PageCount,
            Price = request.Price,
            CoverImageUrl = request.CoverImageUrl,
            IsAvailable = request.IsAvailable,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _books.Add(book);
        return CreatedAtAction(nameof(GetBook), new { bookId = book.Id }, book);
    }

    [HttpPut("books/{bookId}")]
    public ActionResult<BookResponse> UpdateBook(string bookId, [FromBody] UpdateBookRequest request)
    {
        var book = _books.FirstOrDefault(b => b.Id == bookId);
        if (book == null)
        {
            return NotFound("Книга не найдена");
        }

        if (!string.IsNullOrWhiteSpace(request.Title))
        {
            book.Title = request.Title;
        }

        if (!string.IsNullOrWhiteSpace(request.Author))
        {
            book.Author = request.Author;
        }

        if (request.Description != null)
        {
            book.Description = request.Description;
        }

        if (!string.IsNullOrWhiteSpace(request.Isbn))
        {
            book.Isbn = request.Isbn;
        }

        if (request.PublishedYear.HasValue)
        {
            if (request.PublishedYear.Value < 1000 || request.PublishedYear.Value > DateTime.Now.Year + 1)
            {
                return BadRequest("Некорректный год издания");
            }
            book.PublishedYear = request.PublishedYear.Value;
        }

        if (!string.IsNullOrWhiteSpace(request.Genre))
        {
            book.Genre = request.Genre;
        }

        if (!string.IsNullOrWhiteSpace(request.Language))
        {
            book.Language = request.Language;
        }

        if (request.PageCount.HasValue)
        {
            book.PageCount = request.PageCount.Value;
        }

        if (request.Price.HasValue)
        {
            if (request.Price.Value < 0)
            {
                return BadRequest("Цена не может быть отрицательной");
            }
            book.Price = request.Price.Value;
        }

        if (request.CoverImageUrl != null)
        {
            book.CoverImageUrl = request.CoverImageUrl;
        }

        if (request.IsAvailable.HasValue)
        {
            book.IsAvailable = request.IsAvailable.Value;
        }

        book.UpdatedAt = DateTime.UtcNow;
        return Ok(book);
    }

    [HttpDelete("books/{bookId}")]
    public IActionResult DeleteBook(string bookId)
    {
        var book = _books.FirstOrDefault(b => b.Id == bookId);
        if (book == null)
        {
            return NotFound("Книга не найдена");
        }

        _books.Remove(book);
        return Ok(new { message = "Книга успешно удалена" });
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeafSide.Application.Services.Abstract;
using LeafSide.Domain.Entities;
using LeafSide.Infrastructure.Data.Repostitory.Abstract;

namespace LeafSide.Application.Services;


public class BookServices : IBookService
{
    private readonly IBookRepository _bookRepository;

    public BookServices(IBookRepository bookRepository)
    {
        _bookRepository = bookRepository;
    }

    public Task<IEnumerable<Book>> GetAllAsync()
    {
        return _bookRepository.GetAllAsync();
    }

    public Task<Book?> GetByIdAsync(Guid id)
    {
        return _bookRepository.GetByIdAsync(id);
    }

    public Task<Book> AddAsync(Book book)
    {
        return _bookRepository.AddBookAsync(book);
    }

    public Task<Book?> UpdateAsync(Book book)
    {
        return _bookRepository.UpdateBookAsync(book);
    }

    public Task<bool> DeleteAsync(Guid id)
    {
        return _bookRepository.DeleteBookAsync(id);
    }
}

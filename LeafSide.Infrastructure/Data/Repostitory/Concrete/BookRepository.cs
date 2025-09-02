using LeafSide.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LeafSide.Infrastructure.Data.Repostitory.Concrete;

public class BookRepository
{
    private readonly AppDbContext _context;

    public BookRepository(AppDbContext context)
    {
        _context = context;
    }
    
    public async Task<IEnumerable<Book>> GetAllAsync()
    {
        return await _context.Books.AsNoTracking().ToListAsync();
    }
    
    public async Task<Book?> GetByIdAsync(int id)
    {
        return await _context.Books.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id);
    }
    
    public async Task<Book> AddBookAsync(Book book)
    {
        await _context.Books.AddAsync(book);
        await _context.SaveChangesAsync();
        return book;
    }
    
    public async Task<Book?> UpdateBookAsync(Book book)
    {
        var existingBook = await _context.Books
            .FirstOrDefaultAsync(x => x.Id == book.Id);
    
        if (existingBook is null) return null;
    
        _context.Entry(existingBook).CurrentValues.SetValues(book);
        await _context.SaveChangesAsync();
        return existingBook;
    }
    
    public async Task<bool> DeleteBookAsync(int id)
    {
        var book = await _context.Books
            .FirstOrDefaultAsync(x => x.Id == id);
    
        if (book is null) return false;
    
        _context.Books.Remove(book);
        await _context.SaveChangesAsync();
        return true;
    }
}

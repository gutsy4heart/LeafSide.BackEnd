using LeafSide.Domain.Entities;
using LeafSide.Domain.Repositories;
using LeafSide.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace LeafSide.Infrastructure.Data.Repostitory.Concrete;

public class FavoriteRepository : IFavoriteRepository
{
    private readonly AppDbContext _context;

    public FavoriteRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Favorite>> GetByUserIdAsync(Guid userId)
    {
        return await _context.Favorites
            .Include(f => f.Book)
            .Where(f => f.UserId == userId)
            .OrderByDescending(f => f.CreatedAt)
            .ToListAsync();
    }

    public async Task<Favorite?> GetByUserAndBookIdAsync(Guid userId, Guid bookId)
    {
        return await _context.Favorites
            .FirstOrDefaultAsync(f => f.UserId == userId && f.BookId == bookId);
    }

    public async Task<Favorite> AddAsync(Favorite favorite)
    {
        await _context.Favorites.AddAsync(favorite);
        await _context.SaveChangesAsync();
        return favorite;
    }

    public async Task<bool> RemoveAsync(Guid userId, Guid bookId)
    {
        var favorite = await _context.Favorites
            .FirstOrDefaultAsync(f => f.UserId == userId && f.BookId == bookId);
        
        if (favorite == null)
            return false;
        
        _context.Favorites.Remove(favorite);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> IsFavoriteAsync(Guid userId, Guid bookId)
    {
        return await _context.Favorites
            .AnyAsync(f => f.UserId == userId && f.BookId == bookId);
    }

    public async Task<int> GetCountByUserIdAsync(Guid userId)
    {
        return await _context.Favorites
            .CountAsync(f => f.UserId == userId);
    }
}


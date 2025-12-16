using LeafSide.Domain.Entities;
using LeafSide.Domain.Repositories;
using LeafSide.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace LeafSide.Infrastructure.Data.Repostitory.Concrete;

public class ReviewRepository : IReviewRepository
{
    private readonly AppDbContext _context;

    public ReviewRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Review>> GetByBookIdAsync(Guid bookId, bool onlyApproved = true)
    {
        var query = _context.Reviews
            .Where(r => r.BookId == bookId);
        
        if (onlyApproved)
        {
            query = query.Where(r => r.IsApproved);
        }
        
        return await query
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();
    }

    public async Task<Review?> GetByUserAndBookIdAsync(Guid userId, Guid bookId)
    {
        return await _context.Reviews
            .FirstOrDefaultAsync(r => r.UserId == userId && r.BookId == bookId);
    }

    public async Task<Review?> GetByIdAsync(Guid reviewId)
    {
        return await _context.Reviews
            .FirstOrDefaultAsync(r => r.Id == reviewId);
    }

    public async Task<Review> AddAsync(Review review)
    {
        await _context.Reviews.AddAsync(review);
        await _context.SaveChangesAsync();
        return review;
    }

    public async Task<Review> UpdateAsync(Review review)
    {
        review.UpdatedAt = DateTime.UtcNow;
        _context.Reviews.Update(review);
        await _context.SaveChangesAsync();
        return review;
    }

    public async Task<bool> DeleteAsync(Guid reviewId)
    {
        var review = await _context.Reviews
            .FirstOrDefaultAsync(r => r.Id == reviewId);
        
        if (review == null)
            return false;
        
        _context.Reviews.Remove(review);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<double> GetAverageRatingByBookIdAsync(Guid bookId, bool onlyApproved = true)
    {
        var query = _context.Reviews
            .Where(r => r.BookId == bookId);
        
        if (onlyApproved)
        {
            query = query.Where(r => r.IsApproved);
        }
        
        var average = await query
            .AverageAsync(r => (double?)r.Rating);
        
        return average ?? 0.0;
    }

    public async Task<int> GetReviewCountByBookIdAsync(Guid bookId, bool onlyApproved = true)
    {
        var query = _context.Reviews
            .Where(r => r.BookId == bookId);
        
        if (onlyApproved)
        {
            query = query.Where(r => r.IsApproved);
        }
        
        return await query.CountAsync();
    }

    public async Task<IEnumerable<Review>> GetPendingReviewsAsync()
    {
        return await _context.Reviews
            .Where(r => !r.IsApproved)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<Review>> GetAllReviewsAsync()
    {
        return await _context.Reviews
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();
    }
}


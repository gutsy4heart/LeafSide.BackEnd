using LeafSide.Domain.Entities;

namespace LeafSide.Domain.Repositories;

public interface IReviewRepository
{
    Task<IEnumerable<Review>> GetByBookIdAsync(Guid bookId, bool onlyApproved = true);
    Task<Review?> GetByUserAndBookIdAsync(Guid userId, Guid bookId);
    Task<Review?> GetByIdAsync(Guid reviewId);
    Task<Review> AddAsync(Review review);
    Task<Review> UpdateAsync(Review review);
    Task<bool> DeleteAsync(Guid reviewId);
    Task<double> GetAverageRatingByBookIdAsync(Guid bookId, bool onlyApproved = true);
    Task<int> GetReviewCountByBookIdAsync(Guid bookId, bool onlyApproved = true);
    Task<IEnumerable<Review>> GetPendingReviewsAsync();
    Task<IEnumerable<Review>> GetAllReviewsAsync();
}


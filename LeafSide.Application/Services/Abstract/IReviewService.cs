using LeafSide.Domain.Entities;

namespace LeafSide.Application.Services.Abstract;

public interface IReviewService
{
    Task<IEnumerable<Review>> GetBookReviewsAsync(Guid bookId, bool onlyApproved = true);
    Task<Review?> GetUserReviewForBookAsync(Guid userId, Guid bookId);
    Task<Review> CreateReviewAsync(Guid userId, Guid bookId, int rating, string? comment);
    Task<Review> UpdateReviewAsync(Guid userId, Guid reviewId, int rating, string? comment);
    Task<bool> DeleteReviewAsync(Guid userId, Guid reviewId);
    Task<double> GetAverageRatingAsync(Guid bookId, bool onlyApproved = true);
    Task<int> GetReviewCountAsync(Guid bookId, bool onlyApproved = true);
    Task<IEnumerable<Review>> GetPendingReviewsAsync();
    Task<bool> ApproveReviewAsync(Guid reviewId);
    Task<bool> RejectReviewAsync(Guid reviewId);
}


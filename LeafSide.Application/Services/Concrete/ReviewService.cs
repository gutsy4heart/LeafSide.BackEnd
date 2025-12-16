using LeafSide.Application.Services.Abstract;
using LeafSide.Domain.Entities;
using LeafSide.Domain.Repositories;

namespace LeafSide.Application.Services;

public class ReviewService : IReviewService
{
    private readonly IReviewRepository _reviewRepository;
    private readonly IBookRepository _bookRepository;

    public ReviewService(IReviewRepository reviewRepository, IBookRepository bookRepository)
    {
        _reviewRepository = reviewRepository;
        _bookRepository = bookRepository;
    }

    public async Task<IEnumerable<Review>> GetBookReviewsAsync(Guid bookId, bool onlyApproved = true)
    {
        return await _reviewRepository.GetByBookIdAsync(bookId, onlyApproved);
    }

    public async Task<Review?> GetUserReviewForBookAsync(Guid userId, Guid bookId)
    {
        return await _reviewRepository.GetByUserAndBookIdAsync(userId, bookId);
    }

    public async Task<Review> CreateReviewAsync(Guid userId, Guid bookId, int rating, string? comment)
    {
        // Валидация рейтинга
        if (rating < 1 || rating > 5)
        {
            throw new ArgumentException("Rating must be between 1 and 5");
        }

        // Проверяем, существует ли книга
        var book = await _bookRepository.GetByIdAsync(bookId);
        if (book == null)
        {
            throw new InvalidOperationException($"Book with ID {bookId} not found");
        }

        // Проверяем, не оставил ли пользователь уже отзыв
        var existingReview = await _reviewRepository.GetByUserAndBookIdAsync(userId, bookId);
        if (existingReview != null)
        {
            throw new InvalidOperationException("User has already reviewed this book. Use update instead.");
        }

        var review = new Review
        {
            UserId = userId,
            BookId = bookId,
            Rating = rating,
            Comment = comment,
            IsApproved = false, // Требует модерации
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        return await _reviewRepository.AddAsync(review);
    }

    public async Task<Review> UpdateReviewAsync(Guid userId, Guid reviewId, int rating, string? comment)
    {
        // Валидация рейтинга
        if (rating < 1 || rating > 5)
        {
            throw new ArgumentException("Rating must be between 1 and 5");
        }

        var review = await _reviewRepository.GetByIdAsync(reviewId);
        if (review == null)
        {
            throw new InvalidOperationException($"Review with ID {reviewId} not found");
        }

        // Проверяем, что пользователь может редактировать только свой отзыв
        if (review.UserId != userId)
        {
            throw new UnauthorizedAccessException("You can only update your own reviews");
        }

        review.Rating = rating;
        review.Comment = comment;
        review.UpdatedAt = DateTime.UtcNow;
        review.IsApproved = false; // После обновления требуется повторная модерация

        return await _reviewRepository.UpdateAsync(review);
    }

    public async Task<bool> DeleteReviewAsync(Guid userId, Guid reviewId)
    {
        var review = await _reviewRepository.GetByIdAsync(reviewId);
        if (review == null)
        {
            return false;
        }

        // Проверяем, что пользователь может удалять только свой отзыв
        if (review.UserId != userId)
        {
            throw new UnauthorizedAccessException("You can only delete your own reviews");
        }

        return await _reviewRepository.DeleteAsync(reviewId);
    }

    public async Task<double> GetAverageRatingAsync(Guid bookId, bool onlyApproved = true)
    {
        return await _reviewRepository.GetAverageRatingByBookIdAsync(bookId, onlyApproved);
    }

    public async Task<int> GetReviewCountAsync(Guid bookId, bool onlyApproved = true)
    {
        return await _reviewRepository.GetReviewCountByBookIdAsync(bookId, onlyApproved);
    }

    public async Task<IEnumerable<Review>> GetPendingReviewsAsync()
    {
        return await _reviewRepository.GetPendingReviewsAsync();
    }

    public async Task<bool> ApproveReviewAsync(Guid reviewId)
    {
        var review = await _reviewRepository.GetByIdAsync(reviewId);
        if (review == null)
        {
            return false;
        }

        review.IsApproved = true;
        review.UpdatedAt = DateTime.UtcNow;
        await _reviewRepository.UpdateAsync(review);
        return true;
    }

    public async Task<bool> RejectReviewAsync(Guid reviewId)
    {
        return await _reviewRepository.DeleteAsync(reviewId);
    }
}


using System.Security.Claims;
using LeafSide.API.Requests.Reviews;
using LeafSide.API.Responses.Reviews;
using LeafSide.Application.Services.Abstract;
using LeafSide.Infrastructure.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace LeafSide.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ReviewsController : ControllerBase
{
    private readonly IReviewService _reviewService;
    private readonly UserManager<AppUser> _userManager;

    public ReviewsController(IReviewService reviewService, UserManager<AppUser> userManager)
    {
        _reviewService = reviewService;
        _userManager = userManager;
    }

    private Guid GetUserId()
    {
        var sub = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
        return Guid.Parse(sub!);
    }

    [HttpGet("book/{bookId:guid}")]
    public async Task<ActionResult<IEnumerable<ReviewResponse>>> GetBookReviews(Guid bookId)
    {
        try
        {
            var reviews = await _reviewService.GetBookReviewsAsync(bookId, onlyApproved: true);
            
            var response = new List<ReviewResponse>();
            foreach (var review in reviews)
            {
                var user = await _userManager.FindByIdAsync(review.UserId.ToString());
                response.Add(new ReviewResponse
                {
                    Id = review.Id,
                    UserId = review.UserId,
                    BookId = review.BookId,
                    Rating = review.Rating,
                    Comment = review.Comment,
                    IsApproved = review.IsApproved,
                    CreatedAt = review.CreatedAt,
                    UpdatedAt = review.UpdatedAt,
                    UserName = user?.UserName
                });
            }
            
            return Ok(response);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }

    [HttpGet("book/{bookId:guid}/rating")]
    public async Task<ActionResult<BookRatingResponse>> GetBookRating(Guid bookId)
    {
        try
        {
            var averageRating = await _reviewService.GetAverageRatingAsync(bookId, onlyApproved: true);
            var reviewCount = await _reviewService.GetReviewCountAsync(bookId, onlyApproved: true);
            
            return Ok(new BookRatingResponse
            {
                BookId = bookId,
                AverageRating = averageRating,
                ReviewCount = reviewCount
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }

    [HttpGet("book/{bookId:guid}/my")]
    [Authorize]
    public async Task<ActionResult<ReviewResponse>> GetMyReview(Guid bookId)
    {
        try
        {
            var userId = GetUserId();
            var review = await _reviewService.GetUserReviewForBookAsync(userId, bookId);
            
            if (review == null)
                return NotFound(new { error = "Отзыв не найден" });
            
            var user = await _userManager.FindByIdAsync(review.UserId.ToString());
            return Ok(new ReviewResponse
            {
                Id = review.Id,
                UserId = review.UserId,
                BookId = review.BookId,
                Rating = review.Rating,
                Comment = review.Comment,
                IsApproved = review.IsApproved,
                CreatedAt = review.CreatedAt,
                UpdatedAt = review.UpdatedAt,
                UserName = user?.UserName
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }

    [HttpPost]
    [Authorize]
    public async Task<ActionResult<ReviewResponse>> Create([FromBody] CreateReviewRequest request)
    {
        try
        {
            var userId = GetUserId();
            var review = await _reviewService.CreateReviewAsync(userId, request.BookId, request.Rating, request.Comment);
            
            var user = await _userManager.FindByIdAsync(review.UserId.ToString());
            var response = new ReviewResponse
            {
                Id = review.Id,
                UserId = review.UserId,
                BookId = review.BookId,
                Rating = review.Rating,
                Comment = review.Comment,
                IsApproved = review.IsApproved,
                CreatedAt = review.CreatedAt,
                UpdatedAt = review.UpdatedAt,
                UserName = user?.UserName
            };
            
            return CreatedAtAction(nameof(GetMyReview), new { bookId = review.BookId }, response);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("not found"))
        {
            return NotFound(new { error = "Книга не найдена" });
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("already reviewed"))
        {
            return Conflict(new { error = "Вы уже оставили отзыв на эту книгу" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }

    [HttpPut("{reviewId:guid}")]
    [Authorize]
    public async Task<ActionResult<ReviewResponse>> Update(Guid reviewId, [FromBody] UpdateReviewRequest request)
    {
        try
        {
            var userId = GetUserId();
            var review = await _reviewService.UpdateReviewAsync(userId, reviewId, request.Rating, request.Comment);
            
            var user = await _userManager.FindByIdAsync(review.UserId.ToString());
            var response = new ReviewResponse
            {
                Id = review.Id,
                UserId = review.UserId,
                BookId = review.BookId,
                Rating = review.Rating,
                Comment = review.Comment,
                IsApproved = review.IsApproved,
                CreatedAt = review.CreatedAt,
                UpdatedAt = review.UpdatedAt,
                UserName = user?.UserName
            };
            
            return Ok(response);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("not found"))
        {
            return NotFound(new { error = "Отзыв не найден" });
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid(ex.Message);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }

    [HttpDelete("{reviewId:guid}")]
    [Authorize]
    public async Task<IActionResult> Delete(Guid reviewId)
    {
        try
        {
            var userId = GetUserId();
            var deleted = await _reviewService.DeleteReviewAsync(userId, reviewId);
            
            if (!deleted)
                return NotFound(new { error = "Отзыв не найден" });
            
            return NoContent();
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid(ex.Message);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }

    [HttpGet("pending")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<IEnumerable<ReviewResponse>>> GetPendingReviews()
    {
        try
        {
            var reviews = await _reviewService.GetPendingReviewsAsync();
            
            var response = new List<ReviewResponse>();
            foreach (var review in reviews)
            {
                var user = await _userManager.FindByIdAsync(review.UserId.ToString());
                response.Add(new ReviewResponse
                {
                    Id = review.Id,
                    UserId = review.UserId,
                    BookId = review.BookId,
                    Rating = review.Rating,
                    Comment = review.Comment,
                    IsApproved = review.IsApproved,
                    CreatedAt = review.CreatedAt,
                    UpdatedAt = review.UpdatedAt,
                    UserName = user?.UserName
                });
            }
            
            return Ok(response);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }

    [HttpPost("{reviewId:guid}/approve")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> ApproveReview(Guid reviewId)
    {
        try
        {
            var approved = await _reviewService.ApproveReviewAsync(reviewId);
            
            if (!approved)
                return NotFound(new { error = "Отзыв не найден" });
            
            return Ok(new { message = "Отзыв одобрен" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }

    [HttpPost("{reviewId:guid}/reject")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> RejectReview(Guid reviewId)
    {
        try
        {
            var rejected = await _reviewService.RejectReviewAsync(reviewId);
            
            if (!rejected)
                return NotFound(new { error = "Отзыв не найден" });
            
            return Ok(new { message = "Отзыв отклонен" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }
}


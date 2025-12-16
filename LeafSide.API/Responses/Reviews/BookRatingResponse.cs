namespace LeafSide.API.Responses.Reviews;

public class BookRatingResponse
{
    public Guid BookId { get; set; }
    public double AverageRating { get; set; }
    public int ReviewCount { get; set; }
}


namespace LeafSide.API.Requests.Admin;

public class UpdateBookRequest
{
    public string? Title { get; set; }
    public string? Author { get; set; }
    public string? Description { get; set; }
    public string? Isbn { get; set; }
    public int? PublishedYear { get; set; }
    public string? Genre { get; set; }
    public string? Language { get; set; }
    public int? PageCount { get; set; }
    public decimal? Price { get; set; }
    public string? CoverImageUrl { get; set; }
    public bool? IsAvailable { get; set; }
}

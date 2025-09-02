namespace LeafSide.API.Responses.Books;

public class BookResponse
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Author { get; set; } = string.Empty;
    public string Genre { get; set; } = string.Empty;
    public string Publishing { get; set; } = string.Empty;
    public string Created { get; set; } = string.Empty;
    public string ImageUrl { get; set; } = string.Empty;
    public decimal? Price { get; set; }
}



using LeafSide.API.Responses.Books;

namespace LeafSide.API.Responses.Favorites;

public class FavoriteResponse
{
    public Guid Id { get; set; }
    public Guid BookId { get; set; }
    public DateTime CreatedAt { get; set; }
    public BookResponse? Book { get; set; }
}


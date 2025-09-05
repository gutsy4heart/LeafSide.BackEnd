namespace LeafSide.API.Responses.Cart;

public class CartItemResponse
{
    public Guid BookId { get; set; }
    public int Quantity { get; set; }
    public decimal? PriceSnapshot { get; set; }
}

public class CartResponse
{
    public Guid Id { get; set; }
    public IEnumerable<CartItemResponse> Items { get; set; } = Array.Empty<CartItemResponse>();
}



namespace LeafSide.API.Requests.Cart;

public class AddOrUpdateItemRequest
{
    public Guid BookId { get; set; }
    public int Quantity { get; set; }
}



namespace LeafSide.Application.DTOs;

public class OrderItemRequest
{
    public Guid BookId { get; set; }
    public int Quantity { get; set; }
}

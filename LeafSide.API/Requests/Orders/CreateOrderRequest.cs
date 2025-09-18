using System.ComponentModel.DataAnnotations;

namespace LeafSide.API.Requests.Orders;

public class CreateOrderRequest
{
    [Required]
    public List<OrderItemRequest> Items { get; set; } = new();

    [Required]
    [Range(0.01, double.MaxValue, ErrorMessage = "Сумма заказа должна быть больше 0")]
    public decimal TotalAmount { get; set; }
}

public class OrderItemRequest
{
    [Required]
    public Guid BookId { get; set; }

    [Required]
    [Range(1, int.MaxValue, ErrorMessage = "Количество должно быть больше 0")]
    public int Quantity { get; set; }
}

using System.ComponentModel.DataAnnotations;

namespace LeafSide.API.Requests.Orders;

public class CreateOrderRequest
{
    [Required]
    public List<OrderItemRequest> Items { get; set; } = new();

    [Required]
    [Range(0.01, double.MaxValue, ErrorMessage = "Сумма заказа должна быть больше 0")]
    public decimal TotalAmount { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "Стоимость доставки не может быть отрицательной")]
    public decimal DeliveryFee { get; set; }

    [Required(ErrorMessage = "Имя клиента обязательно")]
    public string CustomerName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email клиента обязателен")]
    [EmailAddress(ErrorMessage = "Некорректный email")]
    public string CustomerEmail { get; set; } = string.Empty;

    public string? CustomerPhone { get; set; }

    [Required(ErrorMessage = "Адрес доставки обязателен")]
    public string ShippingAddress { get; set; } = string.Empty;

    [Required(ErrorMessage = "Способ доставки обязателен")]
    public string DeliveryMethod { get; set; } = "standard";

    [Required(ErrorMessage = "Способ оплаты обязателен")]
    public string PaymentMethod { get; set; } = "cashOnDelivery";

    public string? Notes { get; set; }
}

public class OrderItemRequest
{
    [Required]
    public Guid BookId { get; set; }

    [Required]
    [Range(1, int.MaxValue, ErrorMessage = "Количество должно быть больше 0")]
    public int Quantity { get; set; }
}

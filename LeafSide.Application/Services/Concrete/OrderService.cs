using LeafSide.Application.Services.Abstract;
using LeafSide.Domain.Entities;
using LeafSide.Domain.Repositories;

namespace LeafSide.Application.Services;

public class OrderService : IOrderService
{
    private readonly IOrderRepository _orderRepository;
    private readonly ICartRepository _cartRepository;
    private readonly IBookRepository _bookRepository;

    public OrderService(IOrderRepository orderRepository, ICartRepository cartRepository, IBookRepository bookRepository)
    {
        _orderRepository = orderRepository;
        _cartRepository = cartRepository;
        _bookRepository = bookRepository;
    }

    public async Task<Order?> GetByIdAsync(Guid id)
    {
        return await _orderRepository.GetByIdAsync(id);
    }

    public async Task<IEnumerable<Order>> GetAllAsync()
    {
        return await _orderRepository.GetAllAsync();
    }

    public async Task<IEnumerable<Order>> GetByUserIdAsync(Guid userId)
    {
        return await _orderRepository.GetByUserIdAsync(userId);
    }

    public async Task<Order> CreateFromCartAsync(Guid userId, string shippingAddress, string customerName, string customerEmail, string? customerPhone = null, string? notes = null)
    {
        var cart = await _cartRepository.GetByUserIdAsync(userId);
        if (cart == null || !cart.Items.Any())
            throw new InvalidOperationException("Cart is empty");

        var order = new Order
        {
            UserId = userId,
            Status = "Pending",
            ShippingAddress = shippingAddress,
            CustomerName = customerName,
            CustomerEmail = customerEmail,
            CustomerPhone = customerPhone,
            Notes = notes,
            Items = new List<OrderItem>()
        };

        decimal totalAmount = 0;

        foreach (var cartItem in cart.Items)
        {
            var book = await _bookRepository.GetByIdAsync(cartItem.BookId);
            if (book == null) continue;

            var orderItem = new OrderItem
            {
                BookId = cartItem.BookId,
                Quantity = cartItem.Quantity,
                UnitPrice = cartItem.PriceSnapshot ?? book.Price ?? 0,
                TotalPrice = (cartItem.PriceSnapshot ?? book.Price ?? 0) * cartItem.Quantity
            };

            order.Items.Add(orderItem);
            totalAmount += orderItem.TotalPrice;
        }

        order.TotalAmount = totalAmount;

        // Очищаем корзину после создания заказа
        await _cartRepository.ClearAsync(userId);

        return await _orderRepository.CreateAsync(order);
    }

    public async Task<Order> UpdateStatusAsync(Guid orderId, string status)
    {
        var order = await _orderRepository.GetByIdAsync(orderId);
        if (order == null)
            throw new InvalidOperationException("Order not found");

        order.Status = status;
        order.UpdatedAt = DateTime.UtcNow;

        return await _orderRepository.UpdateAsync(order);
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        return await _orderRepository.DeleteAsync(id);
    }

    public async Task<IEnumerable<Order>> GetByStatusAsync(string status)
    {
        return await _orderRepository.GetByStatusAsync(status);
    }
}

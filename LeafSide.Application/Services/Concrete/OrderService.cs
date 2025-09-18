using LeafSide.Application.Services.Abstract;
using LeafSide.Domain.Entities;
using LeafSide.Domain.Repositories;
using LeafSide.Application.DTOs;

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

    public async Task<IEnumerable<Order>> GetUserOrdersAsync(Guid userId)
    {
        return await _orderRepository.GetByUserIdAsync(userId);
    }

    public async Task<Order?> GetUserOrderAsync(Guid userId, Guid orderId)
    {
        var order = await _orderRepository.GetByIdAsync(orderId);
        return order?.UserId == userId ? order : null;
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

    public async Task<Order> CreateOrderAsync(Guid userId, List<OrderItemRequest> items, decimal totalAmount)
    {
        if (items == null || !items.Any())
            throw new ArgumentException("Список товаров не может быть пустым");

        if (totalAmount <= 0)
            throw new ArgumentException("Сумма заказа должна быть больше 0");

        var order = new Order
        {
            UserId = userId,
            Status = "Pending",
            TotalAmount = totalAmount,
            Items = new List<OrderItem>()
        };

        decimal calculatedTotal = 0;

        foreach (var item in items)
        {
            var book = await _bookRepository.GetByIdAsync(item.BookId);
            if (book == null)
                throw new ArgumentException($"Книга с ID {item.BookId} не найдена");

            if (item.Quantity <= 0)
                throw new ArgumentException($"Количество для книги {book.Title} должно быть больше 0");

            var orderItem = new OrderItem
            {
                BookId = item.BookId,
                Quantity = item.Quantity,
                UnitPrice = book.Price ?? 0,
                TotalPrice = (book.Price ?? 0) * item.Quantity
            };

            order.Items.Add(orderItem);
            calculatedTotal += orderItem.TotalPrice;
        }

        // Проверяем, что переданная сумма соответствует рассчитанной
        if (Math.Abs(calculatedTotal - totalAmount) > 0.01m)
            throw new ArgumentException("Переданная сумма не соответствует рассчитанной");

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

    public async Task<Order?> ConfirmDeliveryAsync(Guid userId, Guid orderId)
    {
        var order = await _orderRepository.GetByIdAsync(orderId);
        if (order == null)
            return null;

        if (order.UserId != userId)
            throw new ArgumentException("Заказ не принадлежит пользователю");

        if (order.Status != "Shipped" && order.Status != "Pending")
            throw new ArgumentException("Заказ должен быть в статусе 'Отправлен' или 'Ожидает обработки' для подтверждения получения");

        order.Status = "Delivered";
        order.UpdatedAt = DateTime.UtcNow;

        return await _orderRepository.UpdateAsync(order);
    }
}

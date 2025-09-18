using LeafSide.Domain.Entities;
using LeafSide.Application.DTOs;

namespace LeafSide.Application.Services.Abstract;

public interface IOrderService
{
    Task<Order?> GetByIdAsync(Guid id);
    Task<IEnumerable<Order>> GetAllAsync();
    Task<IEnumerable<Order>> GetByUserIdAsync(Guid userId);
    Task<Order> CreateFromCartAsync(Guid userId, string shippingAddress, string customerName, string customerEmail, string? customerPhone = null, string? notes = null);
    Task<Order> CreateOrderAsync(Guid userId, List<OrderItemRequest> items, decimal totalAmount);
    Task<IEnumerable<Order>> GetUserOrdersAsync(Guid userId);
    Task<Order?> GetUserOrderAsync(Guid userId, Guid orderId);
    Task<Order?> ConfirmDeliveryAsync(Guid userId, Guid orderId);
    Task<Order> UpdateStatusAsync(Guid orderId, string status);
    Task<bool> DeleteAsync(Guid id);
    Task<IEnumerable<Order>> GetByStatusAsync(string status);
}

using LeafSide.Domain.Entities;

namespace LeafSide.Application.Services.Abstract;

public interface IOrderService
{
    Task<Order?> GetByIdAsync(Guid id);
    Task<IEnumerable<Order>> GetAllAsync();
    Task<IEnumerable<Order>> GetByUserIdAsync(Guid userId);
    Task<Order> CreateFromCartAsync(Guid userId, string shippingAddress, string customerName, string customerEmail, string? customerPhone = null, string? notes = null);
    Task<Order> UpdateStatusAsync(Guid orderId, string status);
    Task<bool> DeleteAsync(Guid id);
    Task<IEnumerable<Order>> GetByStatusAsync(string status);
}

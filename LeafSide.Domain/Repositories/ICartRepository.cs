using LeafSide.Domain.Entities;

namespace LeafSide.Domain.Repositories;

public interface ICartRepository
{
    Task<Cart?> GetByUserIdAsync(Guid userId);
    Task<Cart> CreateAsync(Cart cart);
    Task<Cart> UpsertItemAsync(Guid userId, Guid bookId, int quantity, decimal? priceSnapshot);
    Task<bool> RemoveItemAsync(Guid userId, Guid bookId);
    Task<bool> ClearAsync(Guid userId);
    Task<IEnumerable<Cart>> GetAllAsync();
}



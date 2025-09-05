using LeafSide.Domain.Entities;

namespace LeafSide.Application.Services.Abstract;

public interface ICartService
{
    Task<Cart> GetOrCreateAsync(Guid userId);
    Task<Cart> AddOrUpdateItemAsync(Guid userId, Guid bookId, int quantity);
    Task<bool> RemoveItemAsync(Guid userId, Guid bookId);
    Task<bool> ClearAsync(Guid userId);
}



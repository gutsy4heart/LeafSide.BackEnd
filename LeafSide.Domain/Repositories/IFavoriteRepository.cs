using LeafSide.Domain.Entities;

namespace LeafSide.Domain.Repositories;

public interface IFavoriteRepository
{
    Task<IEnumerable<Favorite>> GetByUserIdAsync(Guid userId);
    Task<Favorite?> GetByUserAndBookIdAsync(Guid userId, Guid bookId);
    Task<Favorite> AddAsync(Favorite favorite);
    Task<bool> RemoveAsync(Guid userId, Guid bookId);
    Task<bool> IsFavoriteAsync(Guid userId, Guid bookId);
    Task<int> GetCountByUserIdAsync(Guid userId);
}


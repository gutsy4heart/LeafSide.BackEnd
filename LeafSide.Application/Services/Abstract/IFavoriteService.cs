using LeafSide.Domain.Entities;

namespace LeafSide.Application.Services.Abstract;

public interface IFavoriteService
{
    Task<IEnumerable<Favorite>> GetByUserIdAsync(Guid userId);
    Task<Favorite> AddAsync(Guid userId, Guid bookId);
    Task<bool> RemoveAsync(Guid userId, Guid bookId);
    Task<bool> IsFavoriteAsync(Guid userId, Guid bookId);
    Task<int> GetCountByUserIdAsync(Guid userId);
}


using LeafSide.Application.Services.Abstract;
using LeafSide.Domain.Entities;
using LeafSide.Domain.Repositories;

namespace LeafSide.Application.Services;

public class FavoriteService : IFavoriteService
{
    private readonly IFavoriteRepository _favoriteRepository;
    private readonly IBookRepository _bookRepository;

    public FavoriteService(IFavoriteRepository favoriteRepository, IBookRepository bookRepository)
    {
        _favoriteRepository = favoriteRepository;
        _bookRepository = bookRepository;
    }

    public async Task<IEnumerable<Favorite>> GetByUserIdAsync(Guid userId)
    {
        return await _favoriteRepository.GetByUserIdAsync(userId);
    }

    public async Task<Favorite> AddAsync(Guid userId, Guid bookId)
    {
        // Проверяем, существует ли книга
        var book = await _bookRepository.GetByIdAsync(bookId);
        if (book == null)
        {
            throw new InvalidOperationException($"Book with ID {bookId} not found");
        }

        // Проверяем, не добавлена ли уже книга в избранное
        var existing = await _favoriteRepository.GetByUserAndBookIdAsync(userId, bookId);
        if (existing != null)
        {
            throw new InvalidOperationException("Book is already in favorites");
        }

        var favorite = new Favorite
        {
            UserId = userId,
            BookId = bookId,
            CreatedAt = DateTime.UtcNow
        };

        return await _favoriteRepository.AddAsync(favorite);
    }

    public async Task<bool> RemoveAsync(Guid userId, Guid bookId)
    {
        return await _favoriteRepository.RemoveAsync(userId, bookId);
    }

    public async Task<bool> IsFavoriteAsync(Guid userId, Guid bookId)
    {
        return await _favoriteRepository.IsFavoriteAsync(userId, bookId);
    }

    public async Task<int> GetCountByUserIdAsync(Guid userId)
    {
        return await _favoriteRepository.GetCountByUserIdAsync(userId);
    }
}


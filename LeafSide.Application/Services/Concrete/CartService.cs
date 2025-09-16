using LeafSide.Application.Services.Abstract;
using LeafSide.Domain.Entities;
using LeafSide.Domain.Repositories;

namespace LeafSide.Application.Services;

public class CartService : ICartService
{
    private readonly ICartRepository _cartRepository;
    private readonly IBookRepository _bookRepository;

    public CartService(ICartRepository cartRepository, IBookRepository bookRepository)
    {
        _cartRepository = cartRepository;
        _bookRepository = bookRepository;
    }

    public async Task<Cart> GetOrCreateAsync(Guid userId)
    {
        var cart = await _cartRepository.GetByUserIdAsync(userId);
        if (cart is not null) return cart;
        cart = new Cart { UserId = userId };
        return await _cartRepository.CreateAsync(cart);
    }

    public async Task<Cart> AddOrUpdateItemAsync(Guid userId, Guid bookId, int quantity)
    {
        if (quantity < 1) throw new ArgumentOutOfRangeException(nameof(quantity));
        var book = await _bookRepository.GetByIdAsync(bookId);
        if (book is null) throw new InvalidOperationException("Book not found");
        return await _cartRepository.UpsertItemAsync(userId, bookId, quantity, book.Price);
    }

    public Task<bool> RemoveItemAsync(Guid userId, Guid bookId)
    {
        return _cartRepository.RemoveItemAsync(userId, bookId);
    }

    public Task<bool> ClearAsync(Guid userId)
    {
        return _cartRepository.ClearAsync(userId);
    }

    public async Task<IEnumerable<Cart>> GetAllAsync()
    {
        return await _cartRepository.GetAllAsync();
    }
}



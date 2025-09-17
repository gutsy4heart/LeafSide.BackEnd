using LeafSide.Domain.Entities;
using LeafSide.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace LeafSide.Infrastructure.Data.Repostitory.Concrete;

public class CartRepository : ICartRepository
{
    private readonly AppDbContext _context;

    public CartRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Cart?> GetByUserIdAsync(Guid userId)
    {
        return await _context.Carts
            .Include(c => c.Items)
            .FirstOrDefaultAsync(c => c.UserId == userId);
    }

    public async Task<Cart> CreateAsync(Cart cart)
    {
        await _context.Carts.AddAsync(cart);
        await _context.SaveChangesAsync();
        return cart;
    }

    public async Task<Cart> UpsertItemAsync(Guid userId, Guid bookId, int quantity, decimal? priceSnapshot)
    {
        try
        {
            // Получаем или создаем корзину
            var cartId = await GetOrCreateCartIdAsync(userId);
            
            // Проверяем существующий элемент
            var existingItem = await _context.CartItems
                .Where(i => i.CartId == cartId && i.BookId == bookId)
                .FirstOrDefaultAsync();
            
            if (existingItem == null)
            {
                // Добавляем новый элемент
                var newItem = new CartItem
                {
                    Id = Guid.NewGuid(),
                    CartId = cartId,
                    BookId = bookId,
                    Quantity = quantity,
                    PriceSnapshot = priceSnapshot
                };
                
                _context.CartItems.Add(newItem);
            }
            else
            {
                // Обновляем существующий элемент
                existingItem.Quantity = quantity;
                existingItem.PriceSnapshot = priceSnapshot;
            }

            // Сохраняем изменения
            await _context.SaveChangesAsync();
            
            // Перезагружаем корзину
            var cart = await _context.Carts
                .Include(c => c.Items)
                .FirstOrDefaultAsync(c => c.UserId == userId);
            
            if (cart == null)
            {
                throw new InvalidOperationException($"Cart not found for user {userId} after save");
            }
            
            return cart;
        }
        catch (Exception ex)
        {
            throw;
        }
    }

    private async Task<Guid> GetOrCreateCartIdAsync(Guid userId)
    {
        var cart = await _context.Carts
            .FirstOrDefaultAsync(c => c.UserId == userId);
        
        if (cart == null)
        {
            cart = new Cart 
            { 
                Id = Guid.NewGuid(),
                UserId = userId 
            };
            
            _context.Carts.Add(cart);
            await _context.SaveChangesAsync();
        }
        
        return cart.Id;
    }

    public async Task<bool> RemoveItemAsync(Guid userId, Guid bookId)
    {
        var cart = await _context.Carts.Include(c => c.Items)
            .FirstOrDefaultAsync(c => c.UserId == userId);
        if (cart is null) return false;
        var item = cart.Items.FirstOrDefault(i => i.BookId == bookId);
        if (item is null) return false;
        _context.CartItems.Remove(item);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ClearAsync(Guid userId)
    {
        var cart = await _context.Carts.Include(c => c.Items)
            .FirstOrDefaultAsync(c => c.UserId == userId);
        if (cart is null) return false;
        _context.CartItems.RemoveRange(cart.Items);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<IEnumerable<Cart>> GetAllAsync()
    {
        return await _context.Carts
            .Include(c => c.Items)
            .ThenInclude(i => i.Book)
            .ToListAsync();
    }
}

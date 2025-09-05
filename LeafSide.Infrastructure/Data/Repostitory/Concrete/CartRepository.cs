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
        var cart = await _context.Carts.Include(c => c.Items)
            .FirstOrDefaultAsync(c => c.UserId == userId);
        if (cart is null)
        {
            cart = new Cart { UserId = userId };
            await _context.Carts.AddAsync(cart);
        }

        var item = cart.Items.FirstOrDefault(i => i.BookId == bookId);
        if (item is null)
        {
            item = new CartItem
            {
                Cart = cart,
                BookId = bookId,
                Quantity = quantity,
                PriceSnapshot = priceSnapshot
            };
            cart.Items.Add(item);
        }
        else
        {
            item.Quantity = quantity;
            item.PriceSnapshot = priceSnapshot;
        }

        await _context.SaveChangesAsync();
        return cart;
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
}



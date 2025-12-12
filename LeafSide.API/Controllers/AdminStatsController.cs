using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using LeafSide.Domain.Repositories;
using LeafSide.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;

namespace LeafSide.API.Controllers;

[ApiController]
[Route("api/admin/users")]
[Authorize(Roles = "Admin")]
public class AdminStatsController : ControllerBase
{
    private readonly IOrderRepository _orderRepository;
    private readonly IBookRepository _bookRepository;
    private readonly ICartRepository _cartRepository;
    private readonly UserManager<AppUser> _userManager;

    public AdminStatsController(
        IOrderRepository orderRepository,
        IBookRepository bookRepository,
        ICartRepository cartRepository,
        UserManager<AppUser> userManager)
    {
        _orderRepository = orderRepository;
        _bookRepository = bookRepository;
        _cartRepository = cartRepository;
        _userManager = userManager;
    }

    [HttpGet("stats")]
    public async Task<IActionResult> GetUserStats()
    {
        try
        {
            var now = DateTime.UtcNow;
            var weekAgo = now.AddDays(-7);

            // Подсчет пользователей
            var allUsers = _userManager.Users.ToList();
            var totalUsers = allUsers.Count;
            
            // Подсчет админов и обычных пользователей
            var adminUsers = 0;
            var regularUsers = 0;
            
            foreach (var user in allUsers)
            {
                var roles = await _userManager.GetRolesAsync(user);
                if (roles.Contains("Admin"))
                    adminUsers++;
                else
                    regularUsers++;
            }

            // Недавние пользователи (за последние 7 дней)
            var recentUsers = allUsers.Count(u => u.CreatedAt >= weekAgo);

            var stats = new
            {
                totalUsers,
                adminUsers,
                regularUsers,
                recentUsers
            };

            return Ok(stats);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Internal server error", details = ex.Message });
        }
    }

    [HttpGet("dashboard")]
    public async Task<IActionResult> GetDashboardStats()
    {
        try
        {
            var now = DateTime.UtcNow;
            var today = now.Date;
            var weekAgo = today.AddDays(-7);
            var monthAgo = today.AddDays(-30);

            // Общая статистика
            var totalUsers = _userManager.Users.Count();
            var totalBooks = await _bookRepository.GetAllAsync();
            var totalOrders = await _orderRepository.GetAllAsync();
            var totalCarts = await _cartRepository.GetAllAsync();

            // Статистика за период
            var newUsersToday = _userManager.Users.Count(u => u.CreatedAt >= today);
            var newUsersThisWeek = _userManager.Users.Count(u => u.CreatedAt >= weekAgo);
            var newUsersThisMonth = _userManager.Users.Count(u => u.CreatedAt >= monthAgo);

            var ordersToday = totalOrders.Count(o => o.CreatedAt >= today);
            var ordersThisWeek = totalOrders.Count(o => o.CreatedAt >= weekAgo);
            var ordersThisMonth = totalOrders.Count(o => o.CreatedAt >= monthAgo);

            // Статистика по заказам
            var totalRevenue = totalOrders.Where(o => o.Status == "Delivered")
                .Sum(o => o.TotalAmount);
            var revenueToday = totalOrders
                .Where(o => o.CreatedAt >= today && o.Status == "Delivered")
                .Sum(o => o.TotalAmount);
            var revenueThisWeek = totalOrders
                .Where(o => o.CreatedAt >= weekAgo && o.Status == "Delivered")
                .Sum(o => o.TotalAmount);
            var revenueThisMonth = totalOrders
                .Where(o => o.CreatedAt >= monthAgo && o.Status == "Delivered")
                .Sum(o => o.TotalAmount);

            // Статистика по статусам заказов
            var pendingOrders = totalOrders.Count(o => o.Status == "Pending");
            var processingOrders = totalOrders.Count(o => o.Status == "Processing");
            var shippedOrders = totalOrders.Count(o => o.Status == "Shipped");
            var deliveredOrders = totalOrders.Count(o => o.Status == "Delivered");
            var cancelledOrders = totalOrders.Count(o => o.Status == "Cancelled");

            // Статистика по книгам
            var availableBooks = totalBooks.Count(b => b.IsAvailable);
            var unavailableBooks = totalBooks.Count(b => !b.IsAvailable);
            var totalBooksInStock = totalBooks.Sum(b => 1); // Можно добавить поле StockQuantity позже

            // Статистика по корзинам
            var activeCarts = totalCarts.Count(c => c.Items != null && c.Items.Any());
            var totalItemsInCarts = totalCarts.Sum(c => c.Items?.Count ?? 0);

            // Топ продаваемые книги (по количеству в заказах)
            var topBooks = totalOrders
                .SelectMany(o => o.Items)
                .GroupBy(oi => oi.BookId)
                .Select(g => new
                {
                    BookId = g.Key,
                    TotalSold = g.Sum(oi => oi.Quantity),
                    TotalRevenue = g.Sum(oi => oi.TotalPrice)
                })
                .OrderByDescending(x => x.TotalSold)
                .Take(10)
                .ToList();

            // Статистика по дням (последние 30 дней)
            var dailyStats = new List<object>();
            for (int i = 29; i >= 0; i--)
            {
                var date = today.AddDays(-i);
                var dayOrders = totalOrders.Count(o => o.CreatedAt.Date == date);
                var dayRevenue = totalOrders
                    .Where(o => o.CreatedAt.Date == date && o.Status == "Delivered")
                    .Sum(o => o.TotalAmount);
                var dayUsers = _userManager.Users.Count(u => u.CreatedAt.Date == date);

                dailyStats.Add(new
                {
                    date = date.ToString("yyyy-MM-dd"),
                    orders = dayOrders,
                    revenue = dayRevenue,
                    users = dayUsers
                });
            }

            var stats = new
            {
                overview = new
                {
                    totalUsers,
                    totalBooks = totalBooks.Count(),
                    totalOrders = totalOrders.Count(),
                    totalCarts = totalCarts.Count(),
                    totalRevenue
                },
                today = new
                {
                    newUsers = newUsersToday,
                    orders = ordersToday,
                    revenue = revenueToday
                },
                thisWeek = new
                {
                    newUsers = newUsersThisWeek,
                    orders = ordersThisWeek,
                    revenue = revenueThisWeek
                },
                thisMonth = new
                {
                    newUsers = newUsersThisMonth,
                    orders = ordersThisMonth,
                    revenue = revenueThisMonth
                },
                orders = new
                {
                    pending = pendingOrders,
                    processing = processingOrders,
                    shipped = shippedOrders,
                    delivered = deliveredOrders,
                    cancelled = cancelledOrders
                },
                books = new
                {
                    total = totalBooks.Count(),
                    available = availableBooks,
                    unavailable = unavailableBooks
                },
                carts = new
                {
                    active = activeCarts,
                    totalItems = totalItemsInCarts
                },
                topBooks,
                dailyStats
            };

            return Ok(stats);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Internal server error", details = ex.Message });
        }
    }
}


using LeafSide.API.Requests.Orders;
using LeafSide.API.Responses.Orders;
using LeafSide.Application.Services.Abstract;
using LeafSide.Application.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace LeafSide.API.Controllers;

[ApiController]
[Route("api/orders")]
[Authorize]
public class OrdersController : ControllerBase
{
    private readonly IOrderService _orderService;
    private readonly ILogger<OrdersController> _logger;

    public OrdersController(IOrderService orderService, ILogger<OrdersController> logger)
    {
        _orderService = orderService;
        _logger = logger;
    }

    [HttpPost]
    public async Task<ActionResult<OrderResponse>> CreateOrder([FromBody] CreateOrderRequest request)
    {
        try
        {
            // Получаем ID пользователя из токена
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
            {
                return Unauthorized("Не удалось определить пользователя");
            }

            // Преобразуем запрос в нужный формат
            var orderItems = request.Items.Select(item => new LeafSide.Application.DTOs.OrderItemRequest
            {
                BookId = item.BookId,
                Quantity = item.Quantity
            }).ToList();

            // Создаем заказ
            var order = await _orderService.CreateOrderAsync(userId, orderItems, request.TotalAmount);

            var response = new OrderResponse
            {
                Id = order.Id,
                UserId = order.UserId,
                Status = order.Status,
                TotalAmount = order.TotalAmount,
                ShippingAddress = order.ShippingAddress,
                CustomerName = order.CustomerName,
                CustomerEmail = order.CustomerEmail,
                CustomerPhone = order.CustomerPhone,
                Notes = order.Notes,
                CreatedAt = order.CreatedAt,
                UpdatedAt = order.UpdatedAt,
                Items = order.Items.Select(i => new OrderItemResponse
                {
                    Id = i.Id,
                    BookId = i.BookId,
                    BookTitle = i.Book?.Title ?? "Unknown",
                    Quantity = i.Quantity,
                    UnitPrice = i.UnitPrice,
                    TotalPrice = i.TotalPrice
                }).ToList()
            };

            return Ok(response);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            return StatusCode(500, "Внутренняя ошибка сервера при создании заказа");
        }
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<OrderResponse>>> GetUserOrders()
    {
        try
        {
            // Получаем ID пользователя из токена
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
            {
                return Unauthorized("Не удалось определить пользователя");
            }

            var orders = await _orderService.GetUserOrdersAsync(userId);
            var response = orders.Select(o => new OrderResponse
            {
                Id = o.Id,
                UserId = o.UserId,
                Status = o.Status,
                TotalAmount = o.TotalAmount,
                ShippingAddress = o.ShippingAddress,
                CustomerName = o.CustomerName,
                CustomerEmail = o.CustomerEmail,
                CustomerPhone = o.CustomerPhone,
                Notes = o.Notes,
                CreatedAt = o.CreatedAt,
                UpdatedAt = o.UpdatedAt,
                Items = o.Items.Select(i => new OrderItemResponse
                {
                    Id = i.Id,
                    BookId = i.BookId,
                    BookTitle = i.Book?.Title ?? "Unknown",
                    Quantity = i.Quantity,
                    UnitPrice = i.UnitPrice,
                    TotalPrice = i.TotalPrice
                }).ToList()
            });

            return Ok(response);
        }
        catch (Exception ex)
        {
            return StatusCode(500, "Внутренняя ошибка сервера при получении заказов");
        }
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<OrderResponse>> GetOrder(Guid id)
    {
        try
        {
            // Получаем ID пользователя из токена
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
            {
                return Unauthorized("Не удалось определить пользователя");
            }

            var order = await _orderService.GetUserOrderAsync(userId, id);
            if (order == null)
                return NotFound("Заказ не найден");

            var response = new OrderResponse
            {
                Id = order.Id,
                UserId = order.UserId,
                Status = order.Status,
                TotalAmount = order.TotalAmount,
                ShippingAddress = order.ShippingAddress,
                CustomerName = order.CustomerName,
                CustomerEmail = order.CustomerEmail,
                CustomerPhone = order.CustomerPhone,
                Notes = order.Notes,
                CreatedAt = order.CreatedAt,
                UpdatedAt = order.UpdatedAt,
                Items = order.Items.Select(i => new OrderItemResponse
                {
                    Id = i.Id,
                    BookId = i.BookId,
                    BookTitle = i.Book?.Title ?? "Unknown",
                    Quantity = i.Quantity,
                    UnitPrice = i.UnitPrice,
                    TotalPrice = i.TotalPrice
                }).ToList()
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            return StatusCode(500, "Внутренняя ошибка сервера при получении заказа");
        }
    }

    [HttpPut("{id}/confirm-delivery")]
    public async Task<ActionResult<OrderResponse>> ConfirmDelivery(Guid id)
    {
        try
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
            {
                return Unauthorized("Не удалось определить пользователя");
            }

            var order = await _orderService.ConfirmDeliveryAsync(userId, id);
            if (order == null)
            {
                return NotFound("Заказ не найден или не принадлежит пользователю");
            }

            var response = new OrderResponse
            {
                Id = order.Id,
                UserId = order.UserId,
                Status = order.Status,
                TotalAmount = order.TotalAmount,
                ShippingAddress = order.ShippingAddress,
                CustomerName = order.CustomerName,
                CustomerEmail = order.CustomerEmail,
                CustomerPhone = order.CustomerPhone,
                Notes = order.Notes,
                CreatedAt = order.CreatedAt,
                UpdatedAt = order.UpdatedAt,
                Items = order.Items.Select(i => new OrderItemResponse
                {
                    Id = i.Id,
                    BookId = i.BookId,
                    BookTitle = i.Book?.Title ?? "Unknown",
                    Quantity = i.Quantity,
                    UnitPrice = i.UnitPrice,
                    TotalPrice = i.TotalPrice
                }).ToList()
            };

            return Ok(response);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при подтверждении получения заказа {OrderId}", id);
            return StatusCode(500, "Внутренняя ошибка сервера");
        }
    }
}

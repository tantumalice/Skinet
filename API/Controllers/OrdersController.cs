using API.DTO;
using API.Errors;
using AutoMapper;
using Core.Entites.OrderAggregate;
using Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace API.Controllers;

[Authorize]
public class OrdersController : BaseApiController
{
    private readonly IOrderService _orderService;
    private readonly IMapper _mapper;

    public OrdersController(IOrderService orderService, IMapper mapper)
    {
        _orderService = orderService;
        _mapper = mapper;
    }

    [HttpPost]
    public async Task<ActionResult<Order>> CreateOrder(OrderDto orderDto)
    {
        var email = User.FindFirstValue(ClaimTypes.Email);
        var address = _mapper.Map<AddressDto, Address>(orderDto.ShipToAddress);
        var order = await _orderService.CreateOrderAsync(email, orderDto.DeliveryMethodId,
            orderDto.BasketId, address);

        if (order is null)
        {
            return BadRequest(new ApiResponse(400, "Problem creating order"));
        }
        return Ok(order);
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<Order>>> GetOrdersForUser()
    {
        var email = User.FindFirstValue(ClaimTypes.Email);
        var orders = await _orderService.GetOrdersForUserAsync(email);
        return Ok(orders);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Order>> GetOrderByIdForUser(int id)
    {
        var email = User.FindFirstValue(ClaimTypes.Email);
        var order = await _orderService.GetOrderByIdAsync(id, email);

        if (order is null)
        {
            return NotFound(new ApiResponse(404));
        }
        return order;
    }

    [HttpGet("deliveryMethods")]
    public async Task<ActionResult<IReadOnlyList<DeliveryMethod>>> GetDeliveryMethods()
    {
        return Ok(await _orderService.GetDeliveryMethodsAsync());
    }
}

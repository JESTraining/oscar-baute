using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OPS.Application.Dtos;
using OPS.Application.Interfaces;
using OPS.Domain.Common;
using OPS.Domain.Entities;

namespace OPS.Api.Controllers;

[ApiController]
[Route("api/orders")]
[Authorize]
public class OrdersController : ControllerBase
{
    private readonly IOrderService orderService;

    public OrdersController(IOrderService orderService)
    {
        this.orderService = orderService;
    }

    [HttpPost]
    public async Task<ActionResult<OrderDetailResponse>> Create(CreateOrderRequest request)
    {
        try
        {
            var order = await orderService.CreateOrderAsync(request);
            return CreatedAtAction(nameof(GetById), new { id = order.Id }, order);
        }
        catch (ValidationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet]
    public async Task<ActionResult<List<OrderListItemResponse>>> GetOrders([FromQuery] OrderStatus? status, [FromQuery] DateTime? from, [FromQuery] DateTime? to)
    {
        var orders = await orderService.GetOrdersAsync(status, from, to);
        return Ok(orders);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<OrderDetailResponse>> GetById(int id)
    {
        var order = await orderService.GetOrderByIdAsync(id);
        return order is null ? NotFound() : Ok(order);
    }

    [HttpPut("{id:int}/status")]
    [Authorize(Roles = Roles.Admin)]
    public async Task<IActionResult> UpdateStatus(int id, UpdateStatusRequest request)
    {
        if (!Enum.TryParse<OrderStatus>(request.Status, ignoreCase: true, out var newStatus))
        {
            return BadRequest(new { error = $"Unknown status '{request.Status}'." });
        }

        var (success, notFound, error) = await orderService.UpdateStatusAsync(id, newStatus);

        if (notFound)
        {
            return NotFound();
        }

        if (!success)
        {
            return BadRequest(new { error });
        }

        return Ok();
    }
}
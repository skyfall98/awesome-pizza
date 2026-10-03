using AwesomePizza.Api.DTO.OrderDTO;
using AwesomePizza.Api.Service.OrderServices;

using Microsoft.AspNetCore.Mvc;

namespace AwesomePizza.Api.Controllers;
[ApiController]
[Route("api/[controller]")]
public class OrderController: ControllerBase
{
    private readonly ILogger<OrderController> logger;
    private readonly IOrderService orderService;

    public OrderController(ILogger<OrderController> logger, IOrderService orderService)
    {
        this.logger = logger;
        this.orderService = orderService;
    }

    [HttpGet("{code}")]
    public async Task<ActionResult<GetOrderDTO>> GetOrderByCode(string code, CancellationToken cancellationToken)
    {
        GetOrderDTO order = await orderService.GetOrderByCodeAsync(code, cancellationToken);
        return Ok(order);
    }

    [HttpPost]
    public async Task<ActionResult<GetOrderDTO>> CreateOrder([FromBody] CreateOrderDTO createOrderDto,
        CancellationToken cancellationToken)
    {
        GetOrderDTO order = await orderService.CreateOrderAsync(createOrderDto, cancellationToken);
        return CreatedAtAction(nameof(GetOrderByCode), new { code = order.Code }, order);
    }
}
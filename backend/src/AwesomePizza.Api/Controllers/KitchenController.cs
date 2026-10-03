using AwesomePizza.Api.DTO.OrderDTO;
using AwesomePizza.Api.Service.KitchenServices;

using Microsoft.AspNetCore.Mvc;

namespace AwesomePizza.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class KitchenController: ControllerBase
{
    private readonly IKitchenService kitchenService;

    public KitchenController(IKitchenService kitchenService)
    {
        this.kitchenService = kitchenService;
    }

    [HttpGet("queue")]
    public async Task<ActionResult<List<GetOrderDTO>>> GetQueue(CancellationToken cancellationToken)
    {
        List<GetOrderDTO> queue = await kitchenService.GetQueueAsync(cancellationToken);
        return Ok(queue);
    }

    [HttpPost("queue/next")]
    public async Task<ActionResult<GetOrderDTO>> TakeNextOrder(CancellationToken cancellationToken)
    {
        GetOrderDTO order = await kitchenService.TakeNextOrderAsync(cancellationToken);
        return Ok(order);
    }

    [HttpPost("orders/{code}/complete")]
    public async Task<ActionResult<GetOrderDTO>> CompleteOrder(string code, CancellationToken cancellationToken)
    {
        GetOrderDTO order = await kitchenService.CompleteOrderAsync(code, cancellationToken);
        return Ok(order);
    }
}

using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MultiClientPlatform.Api.Features.Payments.Dtos;
using MultiClientPlatform.Api.Features.Payments.Interfaces;

namespace MultiClientPlatform.Api.Features.Payments;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Customer")]
public class PaymentController : ControllerBase
{
    private readonly IPaymentService _paymentService;

    public PaymentController(IPaymentService paymentService)
    {
        _paymentService = paymentService;
    }

    private int GetUserId()
    {
        return int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
    }

    // POST api/payment/initiate/{orderId} — generates dummy payment URL
    [HttpPost("initiate/{orderId}")]
    public async Task<ActionResult<InitiatePaymentResponse>> Initiate(int orderId)
    {
        var (orderFound, authorized, alreadyPaid, response) = await _paymentService.InitiateAsync(GetUserId(), orderId);

        if (!orderFound)
            return NotFound("Order not found.");

        if (!authorized)
            return Forbid();

        if (alreadyPaid)
            return Conflict("This order has already been paid.");

        return Ok(response);
    }

    // POST api/payment/complete/{paymentId} — simulates gateway callback
    [HttpPost("complete/{paymentId}")]
    public async Task<ActionResult<PaymentStatusResponse>> Complete(int paymentId)
    {
        var (found, authorized, response) = await _paymentService.CompleteAsync(GetUserId(), paymentId);

        if (!found)
            return NotFound("Payment not found.");

        if (!authorized)
            return Forbid();

        return Ok(response);
    }

    // GET api/payment/status/{orderId} — check payment status for an order
    [HttpGet("status/{orderId}")]
    public async Task<ActionResult<PaymentStatusResponse>> GetStatus(int orderId)
    {
        var (found, authorized, response) = await _paymentService.GetStatusAsync(GetUserId(), orderId);

        if (!found)
            return NotFound("Payment or order not found.");

        if (!authorized)
            return Forbid();

        return Ok(response);
    }
}

using Core.Entites;
using Core.Entites.OrderAggregate;
using Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Stripe;

namespace API.Controllers;

public class PaymentsController : BaseApiController
{
    private readonly IPaymentService _paymentService;
    private readonly ILogger<PaymentsController> _logger;
    private readonly string _whSecret;

    public PaymentsController(IPaymentService paymentService, ILogger<PaymentsController> logger,
        IConfiguration config)
    {
        _paymentService = paymentService;
        _logger = logger;
        _whSecret = config["StripeSettings:WhSecret"];
    }

    [Authorize]
    [HttpPost("{basketId}")]
    public async Task<ActionResult<CustomerBasket>> CreateOrUpdatePaymentIntent(string basketId)
    {
        return await _paymentService.CreateOrUpdatePaymentIntent(basketId);
    }

    [HttpPost("webhook")]
    public async Task<ActionResult> StripeWebhook()
    {
        var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();
        var stripeEvent = EventUtility.ConstructEvent(json, Request.Headers["Stripe-Signature"],
            _whSecret);

        PaymentIntent intent;
        Order order;

        switch (stripeEvent.Type)
        {
            case "payment_intent.succeeded":
                intent = (PaymentIntent)stripeEvent.Data.Object;
                _logger.LogInformation($"Payment succeeded: {intent.Id}");
                order = await _paymentService.UpdateOrderPaymentSucceeded(intent.Id);
                _logger.LogInformation($"Order updated to payment received: {order.Id}");
                break;

            case "payment_intent.payment_failed":
                intent = (PaymentIntent)stripeEvent.Data.Object;
                _logger.LogInformation($"Payment failed: {intent.Id}");
                order = await _paymentService.UpdateOrderPaymentFailed(intent.Id);
                _logger.LogInformation($"Order updated to payment failed: {order.Id}");
                break;
        }
        return new EmptyResult();
    }
}

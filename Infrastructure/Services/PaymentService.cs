﻿using Core.Entites;
using Core.Entites.OrderAggregate;
using Core.Interfaces;
using Core.Specifications;
using Microsoft.Extensions.Configuration;
using Stripe;
using Product = Core.Entites.Product;

namespace Infrastructure.Services;

public class PaymentService : IPaymentService
{
    private readonly IBasketRepository _basketRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IConfiguration _config;

    public PaymentService(IBasketRepository basketRepository, IUnitOfWork unitOfWork,
        IConfiguration config)
    {
        _basketRepository = basketRepository;
        _unitOfWork = unitOfWork;
        _config = config;
    }

    public async Task<CustomerBasket> CreateOrUpdatePaymentIntent(string basketId)
    {
        StripeConfiguration.ApiKey = _config["StripeSettings:SecretKey"];

        var basket = await _basketRepository.GetBasketAsync(basketId);
        var shippingPrice = 0m;

        if (basket.DeliveryMethodId.HasValue)
        {
            var deliveryMethod = await _unitOfWork.Repository<DeliveryMethod>()
                .GetByIdAsync(basket.DeliveryMethodId.Value);
            shippingPrice = deliveryMethod.Price;
        }

        foreach (var item in basket.Items)
        {
            // Get prices from DB, do not trust client
            var productItem = await _unitOfWork.Repository<Product>().GetByIdAsync(item.Id);
            if (item.Price != productItem.Price)
            {
                item.Price = productItem.Price;
            }
        }

        var service = new PaymentIntentService();

        PaymentIntent intent;

        if (string.IsNullOrEmpty(basket.PaymentIntentId))
        {
            var options = new PaymentIntentCreateOptions
            {
                Amount = (long)basket.Items.Sum(x => (x.Price * 100) * x.Quantity)
                + (long)(shippingPrice * 100),
                Currency = "usd",
                PaymentMethodTypes = new List<string> { "card" }
            };
            intent = await service.CreateAsync(options);
            basket.PaymentIntentId = intent.Id;
            basket.ClientSecret = intent.ClientSecret;
        }
        else
        {
            var options = new PaymentIntentUpdateOptions
            {
                Amount = (long)basket.Items.Sum(x => (x.Price * 100) * x.Quantity)
                + (long)(shippingPrice * 100)
            };
            await service.UpdateAsync(basket.PaymentIntentId, options);
        }

        await _basketRepository.CreateOrUpdateBasketAsync(basket);

        return basket;
    }

    public async Task<Order> UpdateOrderPaymentFailed(string paymentIntentId)
    {
        var spec = new OrderByPaymentIntentIdSpecification(paymentIntentId);
        var order = await _unitOfWork.Repository<Order>().GetEntityWithSpec(spec);

        if (order is null)
        {
            return null;
        }

        order.Status = OrderStatus.PaymentFailed;
        _unitOfWork.Repository<Order>().Update(order);

        await _unitOfWork.Complete();

        return order;
    }

    public async Task<Order> UpdateOrderPaymentSucceeded(string paymentIntentId)
    {
        var spec = new OrderByPaymentIntentIdSpecification(paymentIntentId);
        var order = await _unitOfWork.Repository<Order>().GetEntityWithSpec(spec);

        if (order is null)
        {
            return null;
        }

        order.Status = OrderStatus.PaymentReceived;
        _unitOfWork.Repository<Order>().Update(order);

        await _unitOfWork.Complete();

        return order;
    }
}

using Core.Entites;
using Core.Entites.OrderAggregate;
using Core.Interfaces;

namespace Infrastructure.Services;

public class OrderService : IOrderService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IBasketRepository _basketRepo;

    public OrderService(IUnitOfWork unitOfWork, IBasketRepository basketRepo)
    {
        _unitOfWork = unitOfWork;
        _basketRepo = basketRepo;
    }

    public async Task<Order> CreateOrderAsync(string buyerEmail, int deliveryMethodId, string basketId,
        Address shippingAddress)
    {
        var basket = await _basketRepo.GetBasketAsync(basketId);

        var items = new List<OrderItem>();
        foreach (var item in basket.Items)
        {
            var productItem = await _unitOfWork.Repository<Product>().GetByIdAsync(item.Id);
            var itemOrdered = new ProductItemOrdered(productItem.Id, productItem.Name,
                productItem.PictureUrl);
            // Get price from Products table (DB), not from basket (client)
            var orderItem = new OrderItem(itemOrdered, productItem.Price, item.Quantity);
            items.Add(orderItem);
        }

        var deliveryMethod = await _unitOfWork.Repository<DeliveryMethod>().GetByIdAsync(deliveryMethodId);

        var subtotal = items.Sum(item => item.Price * item.Quantity);

        var order = new Order(items, buyerEmail, shippingAddress, deliveryMethod, subtotal);

        //DEBUG
        order.PaymentIntentId = "";

        _unitOfWork.Repository<Order>().Add(order);

        var result = await _unitOfWork.Complete();

        if (result <= 0)
        {
            // Nothing been saved
            return null;
        }

        await _basketRepo.DeleteBasketAsync(basketId);

        return order;
    }

    public Task<IReadOnlyList<DeliveryMethod>> GetDeliveryMethodsAsync()
    {
        throw new NotImplementedException();
    }

    public Task<Order> GetOrderByIdAsync(int orderId, string buyerEmail)
    {
        throw new NotImplementedException();
    }

    public Task<IReadOnlyList<Order>> GetOrdersForUserAsync(string buyerEmail)
    {
        throw new NotImplementedException();
    }
}

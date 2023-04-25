using Core.Entites.OrderAggregate;

namespace Core.Interfaces;

public interface IOrderService
{
    Task<Order> CreateOrderAsync(string buyerEmail, int deliveryMethodId, string basketId,
        Address shippingAddress);

    Task<IReadOnlyList<Order>> GetOrdersForUserAsync(string buyerEmail);
    Task<Order> GetOrderByIdAsync(int orderId, string buyerEmail);
    Task<IReadOnlyList<DeliveryMethod>> GetDeliveryMethodsAsync();
}

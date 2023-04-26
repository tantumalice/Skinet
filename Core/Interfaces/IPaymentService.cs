using Core.Entites;

namespace Core.Interfaces;

public interface IPaymentService
{
    Task<CustomerBasket> CreateOrUpdatePaymentIntent(string basketId);
}

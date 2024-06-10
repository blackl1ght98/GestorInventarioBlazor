using PayPal.Api;

namespace CRUDBlazor.Server.Interfaces.Application
{
    public interface IPaypalService
    {
        Task<Payment> CreateDonation(decimal amount, string returnUrl, string cancelUrl, string currency);
        Task<Payment> CreateOrderAsync(List<Item> items, decimal amount, string returnUrl, string cancelUrl, string currency);
    }
}

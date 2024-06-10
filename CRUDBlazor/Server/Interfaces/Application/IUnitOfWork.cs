namespace CRUDBlazor.Server.Interfaces.Application
{
    public interface IUnitOfWork
    {
        IPaypalService PaypalService { get; }
    }
}

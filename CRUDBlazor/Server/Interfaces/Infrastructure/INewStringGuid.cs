using CRUDBlazor.Server.Models;

namespace CRUDBlazor.Server.Interfaces.Infrastructure
{
    public interface INewStringGuid
    {
        Task SaveNewStringGuid(Usuario operation);
    }
}

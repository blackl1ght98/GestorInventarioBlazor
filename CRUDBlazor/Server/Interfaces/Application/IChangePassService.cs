using CRUDBlazor.Server.Models;

namespace CRUDBlazor.Server.Interfaces.Application
{
    public interface IChangePassService
    {
        Task ChangePassId(Usuario usuarioDB, string newPass);

    }
}

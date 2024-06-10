using CRUDBlazor.Server.Application.DTOs;

namespace CRUDBlazor.Server.Interfaces.Application
{
    public interface IConfirmEmailService
    {
        Task ConfirmEmail(DTOConfirmRegistration confirm);

    }
}

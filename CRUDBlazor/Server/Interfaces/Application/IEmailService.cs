using CRUDBlazor.Server.Application.DTOs;

namespace CRUDBlazor.Server.Interfaces.Application
{
    public interface IEmailService
    {
        Task SendEmailAsyncRegister(DTOEmail userData);
        Task SendEmailAsyncResetPassword(DTOEmail userDataResetPassword);
    }
}

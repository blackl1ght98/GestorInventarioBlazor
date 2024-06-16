using CRUDBlazor.Server.Application.DTOs;
using CRUDBlazor.Server.Models;

namespace CRUDBlazor.Server.Interfaces.Application
{
    public interface IEmailService
    {
        Task SendEmailAsyncRegister(DTOEmail userData);
        Task SendEmailAsyncResetPassword(DTOEmail userDataResetPassword);
        Task SendEmailAsyncLowStock(DTOEmail correo, Producto producto);
    }
}

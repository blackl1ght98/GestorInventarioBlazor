using CRUDBlazor.Server.Interfaces.Application;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PayPal.Api;

namespace CRUDBlazor.Server.Infrastructure.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentController : ControllerBase
    {
        private readonly ILogger<PaymentController> _logger;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IConfiguration _configuration;

        public PaymentController(ILogger<PaymentController> logger, IUnitOfWork unitOfWork, IConfiguration configuration)
        {
            _logger = logger;
            _unitOfWork = unitOfWork;
            _configuration = configuration;
        }
        /*https://localhost:7186/Payment/Success?paymentId=PAYID-MZS5SPI4KT067164A2055144&token=EC-74269173R8214803P&PayerID=PAR37NJYRK8W6
         */


        [HttpGet("Success")]
        public async Task<IActionResult> Success( [FromQuery]string paymentId, [FromQuery] string PayerID)
        {
            try
            {
                // Obtén el contexto de la API de PayPal
                var apiContext = new APIContext(new OAuthTokenCredential(_configuration["Paypal:ClientId"], _configuration["Paypal:ClientSecret"]).GetAccessToken());

                // Crea un objeto PaymentExecution para ejecutar la transacción
                var paymentExecution = new PaymentExecution() { payer_id = PayerID };

                // Obtén el pago que se va a ejecutar
                var paymentToExecute = new Payment() { id = paymentId };

                // Ejecuta el pago
                var executedPayment = paymentToExecute.Execute(apiContext, paymentExecution);

                if (executedPayment.state.ToLower() != "approved")
                {

                    return BadRequest("Error en el pago");
                }

                return Ok();
            }
            catch (Exception ex)
            {

                _logger.LogError(ex, "Error al realizar el pago");
                return RedirectToAction("Error", "Home");
            }

        }
    }

 
}
    


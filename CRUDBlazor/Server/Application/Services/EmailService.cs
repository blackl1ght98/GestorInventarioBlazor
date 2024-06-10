﻿using CRUDBlazor.Server.Application.DTOs;
using MailKit.Security;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Mvc;
using MimeKit.Text;
using MimeKit;
using CRUDBlazor.Server.Models;
using Microsoft.EntityFrameworkCore;
using MailKit.Net.Smtp;
using System.Text;
using CRUDBlazor.Server.Interfaces.Application;
using CRUDBlazor.Server.Interfaces.Infrastructure;

namespace CRUDBlazor.Server.Application.Services
{
    public class EmailService:IEmailService
    {
        private readonly IConfiguration _config;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly INewStringGuid _newStringGuid;
        private readonly DbcrudBlazorContext _context;
        private readonly IServiceProvider _serviceProvider;
        private readonly ICompositeViewEngine _viewEngine;
        private readonly ITempDataProvider _tempDataProvider;
        private readonly HashService _hashService;


        public EmailService(IConfiguration config, IHttpContextAccessor httpContextAccessor,
            INewStringGuid newStringGuid, DbcrudBlazorContext context, ITempDataProvider tempDataProvider,
            ICompositeViewEngine viewEngine, IServiceProvider serviceProvider, HashService hashService)
        {
            _config = config;
            _httpContextAccessor = httpContextAccessor;
            _newStringGuid = newStringGuid;
            _context = context;
            _tempDataProvider = tempDataProvider;
            _viewEngine = viewEngine;
            _serviceProvider = serviceProvider;
            _hashService = hashService;
        }


        public async Task SendEmailAsyncRegister(DTOEmail userDataRegister)
        {

            var usuarioDB = await _context.Usuarios.AsTracking().FirstOrDefaultAsync(x => x.Email == userDataRegister.ToEmail);

            Guid miGuid = Guid.NewGuid();
            string textoEnlace = Convert.ToBase64String(miGuid.ToByteArray());
            textoEnlace = textoEnlace.Replace("=", "").Replace("+", "").Replace("/", "").Replace("?", "").Replace("&", "").Replace("!", "").Replace("¡", "");
            usuarioDB.EnlaceCambioPass = textoEnlace;
           // var enlace = $"http://localhost:4200/login/";

            var model = new DTOEmail
            {
                
                //Cuando el usuario hace clic en el enlace que se le envia al correo electroni va al enspoint de confirmacion de correo electronico, en dicho endpoint los parametros llegan por ruta
                RecoveryLink = $"{_httpContextAccessor.HttpContext.Request.Scheme}://{_httpContextAccessor.HttpContext.Request.Host}/api/Admin/confirmRegistration/{usuarioDB.Id}/{usuarioDB.EnlaceCambioPass}?redirect=true",
            };

            await _newStringGuid.SaveNewStringGuid(usuarioDB);

            var ruta = await RenderViewToStringAsync("ViewsEmailService/ViewRegisterEmail", model);

            var email = new MimeMessage();
            email.From.Add(MailboxAddress.Parse(_config.GetSection("Email:UserName").Value));
            email.To.Add(MailboxAddress.Parse(userDataRegister.ToEmail));
            email.Subject = "Confirmar Email";
            email.Body = new TextPart(TextFormat.Html)
            {
                Text = await RenderViewToStringAsync("ViewsEmailService/ViewRegisterEmail", model)
            };

            using var smtp = new SmtpClient();
            await smtp.ConnectAsync(
                _config.GetSection("Email:Host").Value,
                Convert.ToInt32(_config.GetSection("Email:Port").Value),
                SecureSocketOptions.StartTls
            );

            await smtp.AuthenticateAsync(_config.GetSection("Email:UserName").Value, _config.GetSection("Email:PassWord").Value);
            await smtp.SendAsync(email);
            await smtp.DisconnectAsync(true);
        }

        //---------------------------------------------------------------------------------------------------
        public async Task SendEmailAsyncResetPassword(DTOEmail userDataResetPassword)
        {
            var usuarioDB = await _context.Usuarios.AsTracking().FirstOrDefaultAsync(x => x.Email == userDataResetPassword.ToEmail);

            // Generar una contraseña temporal
            var contrasenaTemporal = GenerarContrasenaTemporal();

            // Hashear la contraseña temporal y guardarla en la base de datos
            var resultadoHash = _hashService.Hash(contrasenaTemporal);
            usuarioDB.TemporaryPassword = resultadoHash.Hash;
            usuarioDB.Salt = resultadoHash.Salt;
            // Guardar los cambios en la base de datos
            await _context.SaveChangesAsync();
            var fechaExpiracion = DateTime.UtcNow.AddHours(24); // El enlace expira después de 24 horas
            // Guardar la fecha de vencimiento en la base de datos
            usuarioDB.FechaEnlaceCambioPass = fechaExpiracion;
            // Guardar los cambios en la base de datos
            await _context.SaveChangesAsync();
            // Crear el modelo para la vista del correo electrónico
            var model = new DTOEmail
            {
                //Cuando el usuario hace clic en el enlace que se le envia al correo electronico es dirigido la endpoint de restaurar la contraseña(RestorePassword)
                RecoveryLink = $"{_httpContextAccessor.HttpContext.Request.Scheme}://{_httpContextAccessor.HttpContext.Request.Host}/restaurar/{usuarioDB.Id}/{usuarioDB.EnlaceCambioPass}?redirect=true",
                TemporaryPassword = contrasenaTemporal
            };

            // Renderizar la vista del correo electrónico
            var ruta = await RenderViewToStringAsync("ViewsEmailService/ViewResetPasswordEmail", model);

            // Crear el correo electrónico
            var email = new MimeMessage();
            email.From.Add(MailboxAddress.Parse(_config.GetSection("Email:UserName").Value));
            email.To.Add(MailboxAddress.Parse(userDataResetPassword.ToEmail));
            email.Subject = "Recuperar Contraseña";
            email.Body = new TextPart(TextFormat.Html)
            {
                Text = await RenderViewToStringAsync("ViewsEmailService/ViewResetPasswordEmail", model)
            };

            // Enviar el correo electrónico
            using var smtp = new SmtpClient();
            await smtp.ConnectAsync(
                _config.GetSection("Email:Host").Value,
                Convert.ToInt32(_config.GetSection("Email:Port").Value),
                SecureSocketOptions.StartTls
            );
            await smtp.AuthenticateAsync(_config.GetSection("Email:UserName").Value, _config.GetSection("Email:PassWord").Value);
            await smtp.SendAsync(email);
            await smtp.DisconnectAsync(true);
        }

        //private string GenerarContrasenaTemporal()
        //{
        //    var length = 8;
        //    var random = new Random();
        //    const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
        //    return new string(Enumerable.Repeat(chars, length)
        //   .Select(s => s[random.Next(s.Length)]).ToArray());
        //}
        private string GenerarContrasenaTemporal()
        {
            var length = 12; // Aumenta la longitud de la contraseña
            var random = new Random();
            // Incluye caracteres especiales además de letras y números
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789!@#$%^&*()";
            return new string(Enumerable.Repeat(chars, length)
           .Select(s => s[random.Next(s.Length)]).ToArray());
        }


        private async Task<string> RenderViewToStringAsync(string viewName, object model)
        {
            var httpContext = new DefaultHttpContext { RequestServices = _serviceProvider };
            var actionContext = new ActionContext(httpContext, new RouteData(), new ActionDescriptor());

            using (var sw = new StringWriter())
            {
                var viewResult = _viewEngine.FindView(actionContext, viewName, false);

                if (viewResult.View == null)
                {
                    throw new ArgumentNullException($"{viewName} does not match any available view");
                }

                var viewDictionary = new ViewDataDictionary(new EmptyModelMetadataProvider(), new ModelStateDictionary())
                {
                    Model = model
                };

                var viewContext = new ViewContext(
                    actionContext,
                    viewResult.View,
                    viewDictionary,
                    new TempDataDictionary(actionContext.HttpContext, _tempDataProvider),
                    sw,
                    new HtmlHelperOptions()
                );

                await viewResult.View.RenderAsync(viewContext);

                return sw.ToString();
            }


        }
    }
}

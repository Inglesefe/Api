using Api.Controllers;
using Business;
using Dal;
using Dal.Dto;
using Entities;
using Entities.Config;
using Entities.Log;
using Entities.Noti;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.Extensions.Configuration;
using Moq;
using System.Security.Claims;
using System.Security.Principal;

namespace Api.Test
{
    public abstract class TestBase<T> where T : IEntity, new()
    {
        #region Attributes
        /// <summary>
        /// Configuración de la aplicación de pruebas
        /// </summary>
        protected readonly IConfiguration configuration;

        /// <summary>
        /// Controlador API para los ejecutivos de cuenta
        /// </summary>
        protected ControllerBase<T> api;

        /// <summary>
        /// Contexto HTTP con que se conecta a los servicios Rest
        /// </summary>
        protected readonly ControllerContext controllerContext;

        /// <summary>
        /// Mock del registro de logs
        /// </summary>
        protected readonly Mock<IPersistent<LogComponent>> mockLog;

        /// <summary>
        /// Mock de plnatillas de correo
        /// </summary>
        protected readonly Mock<IBusiness<Template>> mockTemplate;

        /// <summary>
        /// Mock de parámetros de la aplicación
        /// </summary>
        protected readonly Mock<IBusiness<Parameter>> mockParameter;
        #endregion

        #region Constructors
        /// <summary>
        /// Inicializa los datos comunes para las pruebas
        /// </summary>
        protected TestBase()
        {
            configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", false, false)
                .AddEnvironmentVariables()
                .Build();

            GenericIdentity identity = new("usuario", "prueba");
            identity.AddClaim(new Claim("id", "1"));
            controllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
                {
                    User = new ClaimsPrincipal(identity)
                },
                ActionDescriptor = new ControllerActionDescriptor()
                {
                    ControllerName = "Test",
                    ActionName = "Test"
                }
            };

            List<Template> templates = new()
            {
                new Template() { Id = 1, Name = "Notificación de error", Content = "<p>Error #{id}#</p><p>La excepci&oacute;n tiene el siguiente mensaje: #{message}#</p>" },
                new Template() { Id = 2, Name = "Recuperación contraseña", Content = "<p>Prueba recuperaci&oacute;n contrase&ntilde;a con enlace #{link}#</p>" },
                new Template() { Id = 3, Name = "Contraseña cambiada", Content = "<p>Prueba de que su contrase&ntilde;a ha sido cambiada con &eacute;xito</p>" }
            };
            mockLog = new();
            mockTemplate = new();
            mockParameter = new();
            mockLog.Setup(p => p.Insert(It.IsAny<LogComponent>())).Returns((LogComponent log) => log);

            mockTemplate.Setup(p => p.Read(It.IsAny<Template>())).Returns((Template template) => templates.Find(x => x.Id == template.Id) ?? new Template());

            mockParameter.Setup(p => p.List("name = 'SMTP_FROM'", "", 1, 0)).Returns(new ListResult<Parameter>(new List<Parameter>() { new Parameter() { Id = 1, Name = "SMTP_FROM", Value = "soporte.sistemas@inglesefe.com" } }, 1));
            mockParameter.Setup(p => p.List("name = 'SMTP_HOST'", "", 1, 0)).Returns(new ListResult<Parameter>(new List<Parameter>() { new Parameter() { Id = 1, Name = "SMTP_HOST", Value = "mail.inglesefe.com" } }, 1));
            mockParameter.Setup(p => p.List("name = 'SMTP_PASS'", "", 1, 0)).Returns(new ListResult<Parameter>(new List<Parameter>() { new Parameter() { Id = 1, Name = "SMTP_PASS", Value = configuration["Smtp:Password"] ?? "" } }, 1));
            mockParameter.Setup(p => p.List("name = 'SMTP_PORT'", "", 1, 0)).Returns(new ListResult<Parameter>(new List<Parameter>() { new Parameter() { Id = 1, Name = "SMTP_PORT", Value = "465" } }, 1));
            mockParameter.Setup(p => p.List("name = 'SMTP_SSL'", "", 1, 0)).Returns(new ListResult<Parameter>(new List<Parameter>() { new Parameter() { Id = 1, Name = "SMTP_SSL", Value = "true" } }, 1));
            mockParameter.Setup(p => p.List("name = 'SMTP_USERNAME'", "", 1, 0)).Returns(new ListResult<Parameter>(new List<Parameter>() { new Parameter() { Id = 1, Name = "SMTP_USERNAME", Value = "soporte.sistemas@inglesefe.com" } }, 1));
            mockParameter.Setup(p => p.List("name = 'NOTIFICATION_TO'", "", 1, 0)).Returns(new ListResult<Parameter>(new List<Parameter>() { new Parameter() { Id = 1, Name = "NOTIFICATION_TO", Value = "leandrobaena@gmail.com" } }, 1));
            mockParameter.Setup(p => p.List("name = 'URL_CHANGE_PASS'", "", 1, 0)).Returns(new ListResult<Parameter>(new List<Parameter>() { new Parameter() { Id = 1, Name = "URL_CHANGE_PASS", Value = "https://localhost/change_password/" } }, 1));
        }
        #endregion
    }
}

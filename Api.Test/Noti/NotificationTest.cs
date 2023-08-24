using Api.Controllers.Noti;
using Business;
using Dal;
using Dal.Dto;
using Dal.Exceptions;
using Entities.Auth;
using Entities.Log;
using Entities.Noti;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.Extensions.Configuration;
using Moq;
using System.Security.Claims;
using System.Security.Principal;

namespace Api.Test.Noti
{
    /// <summary>
    /// Realiza las pruebas sobre la api de notificaciones
    /// </summary>
    [Collection("Test")]
    public class NotificationTest
    {
        #region Attributes
        /// <summary>
        /// Configuración de la aplicación de pruebas
        /// </summary>
        private readonly IConfiguration _configuration;

        /// <summary>
        /// Controlador API para las notificaciones
        /// </summary>
        private readonly NotificationController _api;

        /// <summary>
        /// Contexto HTTP con que se conecta a los servicios Rest
        /// </summary>
        private readonly ControllerContext _controllerContext;
        #endregion

        #region Constructors
        /// <summary>
        /// Inicializa la configuración de la prueba
        /// </summary>
        public NotificationTest()
        {
            Mock<IBusiness<Notification>> mockBusiness = new();
            Mock<IPersistentBase<LogComponent>> mockLog = new();
            Mock<IBusiness<Template>> mockTemplate = new();

            GenericIdentity identity = new("usuario", "prueba");
            identity.AddClaim(new Claim("id", "1"));
            _controllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
                {
                    User = new ClaimsPrincipal(identity)
                },
                ActionDescriptor = new ControllerActionDescriptor()
                {
                    ControllerName = "NotificationTest",
                    ActionName = "Test"
                }
            };
            _configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", false, false)
                .AddEnvironmentVariables()
                .Build();

            List<Notification> notifications = new()
            {
                new Notification() { Id = 1, Date = DateTime.Now, To = "leandrobaena@gmail.com", Subject = "Correo de prueba", Content = "<h1>Esta es una prueba hecha por leandrobaena@gmail.com</h1>", User = 1 }
            };
            List<Template> templates = new()
            {
                new Template() { Id = 1, Name = "Plantilla de prueba", Content = "<h1>Esta es una prueba hecha por #{user}#</h1>" }
            };

            mockBusiness.Setup(p => p.List("idnotification = 1", It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>()))
                .Returns(new ListResult<Notification>(notifications.Where(y => y.Id == 1).ToList(), 1));
            mockBusiness.Setup(p => p.List("idnotificacion = 1", It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>()))
                .Throws<PersistentException>();

            mockBusiness.Setup(p => p.Read(It.IsAny<Notification>()))
                .Returns((Notification notification) => notifications.Find(x => x.Id == notification.Id) ?? new Notification());

            mockBusiness.Setup(p => p.Insert(It.IsAny<Notification>(), It.IsAny<User>()))
                .Returns((Notification notification, User user) =>
                {
                    notification.Id = notifications.Count + 1;
                    notifications.Add(notification);
                    return notification;
                });

            mockLog.Setup(p => p.Insert(It.IsAny<LogComponent>())).Returns((LogComponent log) => log);

            mockTemplate.Setup(p => p.Read(It.IsAny<Template>())).Returns((Template template) => templates.Find(x => x.Id == template.Id) ?? new Template());

            _api = new(_configuration, mockBusiness.Object, mockLog.Object, mockTemplate.Object)
            {
                ControllerContext = _controllerContext
            };
        }
        #endregion

        #region Methods
        /// <summary>
        /// Prueba la consulta de un listado de notificaciones con filtros, ordenamientos y límite
        /// </summary>
        [Fact]
        public void NotificationListTest()
        {
            ListResult<Notification> list = _api.List("idnotification = 1", "date", 1, 0);

            Assert.NotEmpty(list.List);
            Assert.True(list.Total > 0);
        }

        /// <summary>
        /// Prueba la consulta de un listado de notificaciones con filtros, ordenamientos y límite y con errores
        /// </summary>
        [Fact]
        public void NotificationListWithErrorTest()
        {
            ListResult<Notification> list = _api.List("idnotificacion = 1", "name", 1, 0);

            Assert.Equal(0, list.Total);
        }

        /// <summary>
        /// Prueba la consulta de una notificación dada su identificador
        /// </summary>
        [Fact]
        public void NotificationReadTest()
        {
            Notification notification = _api.Read(1);

            Assert.Equal("leandrobaena@gmail.com", notification.To);
        }

        /// <summary>
        /// Prueba la consulta de una notificación que no existe dado su identificador
        /// </summary>
        [Fact]
        public void NotificationReadNotFoundTest()
        {
            Notification notification = _api.Read(10);

            Assert.Equal(0, notification.Id);
        }

        /// <summary>
        /// Prueba la inserción de una notificación
        /// </summary>
        [Fact]
        public void NotificationInsertTest()
        {
            Notification notification = new() { Subject = "Prueba 1", Content = "<p>Prueba de notificación #{insertada}#", To = "leandrobaena@gmail.com", User = 1 };
            notification = _api.Insert(notification);

            Assert.NotEqual(0, notification.Id);
        }
        #endregion
    }
}

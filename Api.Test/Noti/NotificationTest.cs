using Api.Controllers.Noti;
using Dal.Dto;
using Entities.Noti;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.Extensions.Configuration;
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
            _api = new(_configuration)
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

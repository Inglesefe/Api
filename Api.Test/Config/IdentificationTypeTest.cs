using Api.Controllers.Config;
using Dal.Dto;
using Entities.Config;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.Extensions.Configuration;
using System.Security.Claims;
using System.Security.Principal;

namespace Api.Test.Config
{
    /// <summary>
    /// Realiza las pruebas sobre la api de tipos de identificación
    /// </summary>
    [Collection("Test")]
    public class IdentificationTypeTest
    {
        #region Attributes
        /// <summary>
        /// Configuración de la aplicación de pruebas
        /// </summary>
        private readonly IConfiguration _configuration;

        /// <summary>
        /// Controlador API para los tipos de identificación
        /// </summary>
        private readonly IdentificationTypeController _api;

        /// <summary>
        /// Contexto HTTP con que se conecta a los servicios Rest
        /// </summary>
        private readonly ControllerContext _controllerContext;
        #endregion

        #region Constructors
        /// <summary>
        /// Inicializa la configuración de la prueba
        /// </summary>
        public IdentificationTypeTest()
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
                    ControllerName = "IdentificationTypeTest",
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
        /// Prueba la consulta de un listado de tipos de identificación con filtros, ordenamientos y límite
        /// </summary>
        [Fact]
        public void IdentificationTypeListTest()
        {
            ListResult<IdentificationType> list = _api.List("ididentificationtype = 1", "name", 1, 0);

            Assert.NotEmpty(list.List);
            Assert.True(list.Total > 0);
        }

        /// <summary>
        /// Prueba la consulta de un listado de tipos de identificación con filtros, ordenamientos y límite y con errores
        /// </summary>
        [Fact]
        public void IdentificationTypeListWithErrorTest()
        {
            ListResult<IdentificationType> list = _api.List("idtipoidentificacion = 1", "name", 1, 0);

            Assert.Equal(0, list.Total);
        }

        /// <summary>
        /// Prueba la consulta de un tipo de identificación dada su identificador
        /// </summary>
        [Fact]
        public void IdentificationTypeReadTest()
        {
            IdentificationType identificationType = _api.Read(1);

            Assert.Equal("Cedula ciudadania", identificationType.Name);
        }

        /// <summary>
        /// Prueba la consulta de un tipo de identificación que no existe dado su identificador
        /// </summary>
        [Fact]
        public void IdentificationTypeReadNotFoundTest()
        {
            IdentificationType identificationType = _api.Read(10);

            Assert.Equal(0, identificationType.Id);
        }

        /// <summary>
        /// Prueba la inserción de un tipo de identificación
        /// </summary>
        [Fact]
        public void IdentificationTypeInsertTest()
        {
            IdentificationType identificationType = new() { Name = "Prueba 1" };
            identificationType = _api.Insert(identificationType);

            Assert.NotEqual(0, identificationType.Id);
        }

        /// <summary>
        /// Prueba la actualización de un tipo de identificación
        /// </summary>
        [Fact]
        public void IdentificationTypeUpdateTest()
        {
            IdentificationType identificationType = new() { Id = 2, Name = "Tarjeta de identidad" };
            _ = _api.Update(identificationType);

            IdentificationType identificationType2 = _api.Read(2);

            Assert.NotEqual("Cedula extranjeria", identificationType2.Name);
        }

        /// <summary>
        /// Prueba la eliminación de un tipo de identificación
        /// </summary>
        [Fact]
        public void IdentificationTypeDeleteTest()
        {
            _ = _api.Delete(3);

            IdentificationType identificationType = _api.Read(3);

            Assert.Equal(0, identificationType.Id);
        }
        #endregion
    }
}

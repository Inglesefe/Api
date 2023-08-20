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
    /// Realiza las pruebas sobre la api de oficinas
    /// </summary>
    [Collection("Test")]
    public class OfficeTest
    {
        #region Attributes
        /// <summary>
        /// Configuración de la aplicación de pruebas
        /// </summary>
        private readonly IConfiguration _configuration;

        /// <summary>
        /// Controlador API para los oficinas
        /// </summary>
        private readonly OfficeController _api;

        /// <summary>
        /// Contexto HTTP con que se conecta a los servicios Rest
        /// </summary>
        private readonly ControllerContext _controllerContext;
        #endregion

        #region Constructors
        /// <summary>
        /// Inicializa la configuración de la prueba
        /// </summary>
        public OfficeTest()
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
                    ControllerName = "OfficeTest",
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
        /// Prueba la consulta de un listado de oficinas con filtros, ordenamientos y límite
        /// </summary>
        [Fact]
        public void OfficeListTest()
        {
            ListResult<Office> list = _api.List("o.idoffice = 1", "o.name", 1, 0);

            Assert.NotEmpty(list.List);
            Assert.True(list.Total > 0);
        }

        /// <summary>
        /// Prueba la consulta de un listado de oficinas con filtros, ordenamientos y límite y con errores
        /// </summary>
        [Fact]
        public void OfficeListWithErrorTest()
        {
            ListResult<Office> list = _api.List("idoficina = 1", "name", 1, 0);

            Assert.Equal(0, list.Total);
        }

        /// <summary>
        /// Prueba la consulta de una oficina dada su identificador
        /// </summary>
        [Fact]
        public void OfficeReadTest()
        {
            Office office = _api.Read(1);

            Assert.Equal("Castellana", office.Name);
        }

        /// <summary>
        /// Prueba la consulta de una oficina que no existe dado su identificador
        /// </summary>
        [Fact]
        public void OfficeReadNotFoundTest()
        {
            Office office = _api.Read(10);

            Assert.Equal(0, office.Id);
        }

        /// <summary>
        /// Prueba la inserción de una oficina
        /// </summary>
        [Fact]
        public void OfficeInsertTest()
        {
            Office office = new() { City = new() { Id = 1 }, Name = "Madelena", Address = "Calle 59 sur" };
            office = _api.Insert(office);

            Assert.NotEqual(0, office.Id);
        }

        /// <summary>
        /// Prueba la actualización de una oficina
        /// </summary>
        [Fact]
        public void OfficeUpdateTest()
        {
            Office office = new() { Id = 2, City = new() { Id = 1 }, Name = "Santa Librada", Address = "Calle 78 sur" };
            _ = _api.Update(office);

            Office office2 = _api.Read(2);

            Assert.NotEqual("Kennedy", office2.Name);
        }

        /// <summary>
        /// Prueba la eliminación de una oficina
        /// </summary>
        [Fact]
        public void OfficeDeleteTest()
        {
            _ = _api.Delete(3);

            Office office = _api.Read(3);

            Assert.Equal(0, office.Id);
        }
        #endregion
    }
}

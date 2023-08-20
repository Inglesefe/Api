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
    /// Realiza las pruebas sobre la api de parámetros
    /// </summary>
    [Collection("Test")]
    public class ParameterTest
    {
        #region Attributes
        /// <summary>
        /// Configuración de la aplicación de pruebas
        /// </summary>
        private readonly IConfiguration _configuration;

        /// <summary>
        /// Controlador API para los parámetros
        /// </summary>
        private readonly ParameterController _api;

        /// <summary>
        /// Contexto HTTP con que se conecta a los servicios Rest
        /// </summary>
        private readonly ControllerContext _controllerContext;
        #endregion

        #region Constructors
        /// <summary>
        /// Inicializa la configuración de la prueba
        /// </summary>
        public ParameterTest()
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
                    ControllerName = "ParameterTest",
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
        /// Prueba la consulta de un listado de parámetros con filtros, ordenamientos y límite
        /// </summary>
        [Fact]
        public void ParameterListTest()
        {
            ListResult<Parameter> list = _api.List("idparameter = 1", "name", 1, 0);

            Assert.NotEmpty(list.List);
            Assert.True(list.Total > 0);
        }

        /// <summary>
        /// Prueba la consulta de un listado de parámetros con filtros, ordenamientos y límite y con errores
        /// </summary>
        [Fact]
        public void ParameterListWithErrorTest()
        {
            ListResult<Parameter> list = _api.List("idparametro = 1", "name", 1, 0);

            Assert.Equal(0, list.Total);
        }

        /// <summary>
        /// Prueba la consulta de un parámetro dada su identificador
        /// </summary>
        [Fact]
        public void ParameterReadTest()
        {
            Parameter country = _api.Read(1);

            Assert.Equal("Parametro 1", country.Name);
        }

        /// <summary>
        /// Prueba la consulta de un parámetro que no existe dado su identificador
        /// </summary>
        [Fact]
        public void ParameterReadNotFoundTest()
        {
            Parameter country = _api.Read(10);

            Assert.Equal(0, country.Id);
        }

        /// <summary>
        /// Prueba la inserción de un parámetro
        /// </summary>
        [Fact]
        public void ParameterInsertTest()
        {
            Parameter country = new() { Name = "Parametro 4", Value = "Valor 4" };
            country = _api.Insert(country);

            Assert.NotEqual(0, country.Id);
        }

        /// <summary>
        /// Prueba la actualización de un parámetro
        /// </summary>
        [Fact]
        public void ParameterUpdateTest()
        {
            Parameter country = new() { Id = 2, Name = "Parametro 6", Value = "Valor 6" };
            _ = _api.Update(country);

            Parameter country2 = _api.Read(2);

            Assert.NotEqual("Parametro 2", country2.Name);
        }

        /// <summary>
        /// Prueba la eliminación de un parámetro
        /// </summary>
        [Fact]
        public void ParameterDeleteTest()
        {
            _ = _api.Delete(3);

            Parameter country = _api.Read(3);

            Assert.Equal(0, country.Id);
        }
        #endregion
    }
}

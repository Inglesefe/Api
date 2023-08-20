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
    /// Realiza las pruebas sobre la api de paises
    /// </summary>
    [Collection("Test")]
    public class CountryTest
    {
        #region Attributes
        /// <summary>
        /// Configuración de la aplicación de pruebas
        /// </summary>
        private readonly IConfiguration _configuration;

        /// <summary>
        /// Controlador API para los paises
        /// </summary>
        private readonly CountryController _api;

        /// <summary>
        /// Contexto HTTP con que se conecta a los servicios Rest
        /// </summary>
        private readonly ControllerContext _controllerContext;
        #endregion

        #region Constructors
        /// <summary>
        /// Inicializa la configuración de la prueba
        /// </summary>
        public CountryTest()
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
                    ControllerName = "CountryTest",
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
        /// Prueba la consulta de un listado de paises con filtros, ordenamientos y límite
        /// </summary>
        [Fact]
        public void CountryListTest()
        {
            ListResult<Country> list = _api.List("idcountry = 1", "name", 1, 0);

            Assert.NotEmpty(list.List);
            Assert.True(list.Total > 0);
        }

        /// <summary>
        /// Prueba la consulta de un listado de paises con filtros, ordenamientos y límite y con errores
        /// </summary>
        [Fact]
        public void CountryListWithErrorTest()
        {
            ListResult<Country> list = _api.List("idpais = 1", "name", 1, 0);

            Assert.Equal(0, list.Total);
        }

        /// <summary>
        /// Prueba la consulta de un país dada su identificador
        /// </summary>
        [Fact]
        public void CountryReadTest()
        {
            Country country = _api.Read(1);

            Assert.Equal("CO", country.Code);
        }

        /// <summary>
        /// Prueba la consulta de un país que no existe dado su identificador
        /// </summary>
        [Fact]
        public void CountryReadNotFoundTest()
        {
            Country country = _api.Read(10);

            Assert.Equal(0, country.Id);
        }

        /// <summary>
        /// Prueba la inserción de un país
        /// </summary>
        [Fact]
        public void CountryInsertTest()
        {
            Country country = new() { Code = "PR", Name = "Puerto Rico" };
            country = _api.Insert(country);

            Assert.NotEqual(0, country.Id);
        }

        /// <summary>
        /// Prueba la inserción de un país con nombre duplicado
        /// </summary>
        [Fact]
        public void CountryInsertDuplicateTest()
        {
            Country country = new() { Code = "CO", Name = "Colombia" };
            country = _api.Insert(country);

            Assert.Equal(0, country.Id);
        }

        /// <summary>
        /// Prueba la actualización de un país
        /// </summary>
        [Fact]
        public void CountryUpdateTest()
        {
            Country country = new() { Id = 2, Code = "PE", Name = "Perú" };
            _ = _api.Update(country);

            Country country2 = _api.Read(2);

            Assert.NotEqual("US", country2.Code);
        }

        /// <summary>
        /// Prueba la eliminación de un país
        /// </summary>
        [Fact]
        public void CountryDeleteTest()
        {
            _ = _api.Delete(3);

            Country country = _api.Read(3);

            Assert.Equal(0, country.Id);
        }
        #endregion
    }
}

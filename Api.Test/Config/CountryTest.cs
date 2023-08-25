using Api.Controllers.Config;
using Business;
using Dal;
using Dal.Dto;
using Dal.Exceptions;
using Entities.Auth;
using Entities.Config;
using Entities.Log;
using Entities.Noti;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.Extensions.Configuration;
using Moq;
using System.Data;
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
            Mock<IBusiness<Country>> mockBusiness = new();
            Mock<IPersistentBase<LogComponent>> mockLog = new();
            Mock<IBusiness<Template>> mockTemplate = new();
            Mock<IDbConnection> mockConnection = new();

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

            List<Country> countries = new()
            {
                new Country() { Id = 1, Code = "CO", Name = "Colombia" },
                new Country() { Id = 2, Code = "US", Name = "Estados unidos" },
                new Country() { Id = 3, Code = "EN", Name = "Inglaterra" }
            };
            List<Template> templates = new()
            {
                new Template() { Id = 1, Name = "Notificación de error", Content = "<p>Error #{id}#</p><p>La excepci&oacute;n tiene el siguiente mensaje: #{message}#</p>" },
                new Template() { Id = 2, Name = "Recuperación contraseña", Content = "<p>Prueba recuperaci&oacute;n contrase&ntilde;a con enlace #{link}#</p>" },
                new Template() { Id = 3, Name = "Contraseña cambiada", Content = "<p>Prueba de que su contrase&ntilde;a ha sido cambiada con &eacute;xito</p>" }
            };

            mockBusiness.Setup(p => p.List("idcountry = 1", It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<IDbConnection>()))
                .Returns(new ListResult<Country>(countries.Where(y => y.Id == 1).ToList(), 1));
            mockBusiness.Setup(p => p.List("idpais = 1", It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<IDbConnection>()))
                .Throws<PersistentException>();

            mockBusiness.Setup(p => p.Read(It.IsAny<Country>(), It.IsAny<IDbConnection>()))
                .Returns((Country country, IDbConnection connection) => countries.Find(x => x.Id == country.Id) ?? new Country());

            mockBusiness.Setup(p => p.Insert(It.IsAny<Country>(), It.IsAny<User>(), It.IsAny<IDbConnection>()))
                .Returns((Country country, User user, IDbConnection connection) =>
                {
                    if (countries.Exists(x => x.Code == country.Code))
                    {
                        throw new PersistentException();
                    }
                    else
                    {
                        country.Id = countries.Count + 1;
                        countries.Add(country);
                        return country;
                    }
                });

            mockBusiness.Setup(p => p.Update(It.IsAny<Country>(), It.IsAny<User>(), It.IsAny<IDbConnection>()))
                .Returns((Country country, User user, IDbConnection connection) =>
                {
                    countries.Where(x => x.Id == country.Id).ToList().ForEach(x => x.Code = country.Code);
                    return country;
                });

            mockBusiness.Setup(p => p.Delete(It.IsAny<Country>(), It.IsAny<User>(), It.IsAny<IDbConnection>()))
                .Returns((Country city, User user, IDbConnection connection) =>
                {
                    countries = countries.Where(x => x.Id != city.Id).ToList();
                    return city;
                });

            mockLog.Setup(p => p.Insert(It.IsAny<LogComponent>(), It.IsAny<IDbConnection>())).Returns((LogComponent log, IDbConnection connection) => log);

            mockTemplate.Setup(p => p.Read(It.IsAny<Template>(), It.IsAny<IDbConnection>())).Returns((Template template, IDbConnection connection) => templates.Find(x => x.Id == template.Id) ?? new Template());

            _api = new(_configuration, mockBusiness.Object, mockLog.Object, mockTemplate.Object, mockConnection.Object)
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

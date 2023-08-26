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
    /// Realiza las pruebas sobre la api de ciudades
    /// </summary>
    [Collection("Test")]
    public class CityTest
    {
        #region Attributes
        /// <summary>
        /// Configuración de la aplicación de pruebas
        /// </summary>
        private readonly IConfiguration _configuration;

        /// <summary>
        /// Controlador API para los ciudades
        /// </summary>
        private readonly CityController _api;

        /// <summary>
        /// Contexto HTTP con que se conecta a los servicios Rest
        /// </summary>
        private readonly ControllerContext _controllerContext;
        #endregion

        #region Constructors
        /// <summary>
        /// Inicializa la configuración de la prueba
        /// </summary>
        public CityTest()
        {
            Mock<IBusiness<City>> mockBusiness = new();
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
                    ControllerName = "CityTest",
                    ActionName = "Test"
                }
            };
            _configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", false, false)
                .AddEnvironmentVariables()
                .Build();

            List<City> cities = new()
            {
                new City() { Id = 1, Code = "BOG", Name = "Bogotá" },
                new City() { Id = 1, Code = "MED", Name = "Medellín" },
                new City() { Id = 1, Code = "CAL", Name = "Cali" }
            };
            List<Template> templates = new()
            {
                new Template() { Id = 1, Name = "Notificación de error", Content = "<p>Error #{id}#</p><p>La excepci&oacute;n tiene el siguiente mensaje: #{message}#</p>" },
                new Template() { Id = 2, Name = "Recuperación contraseña", Content = "<p>Prueba recuperaci&oacute;n contrase&ntilde;a con enlace #{link}#</p>" },
                new Template() { Id = 3, Name = "Contraseña cambiada", Content = "<p>Prueba de que su contrase&ntilde;a ha sido cambiada con &eacute;xito</p>" }
            };

            mockBusiness.Setup(p => p.List("ci.idcity = 1", It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<IDbConnection>()))
                .Returns(new ListResult<City>(cities.Where(y => y.Id == 1).ToList(), 1));
            mockBusiness.Setup(p => p.List("idpais = 1", It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<IDbConnection>()))
                .Throws<PersistentException>();

            mockBusiness.Setup(p => p.Read(It.IsAny<City>(), It.IsAny<IDbConnection>()))
                .Returns((City city, IDbConnection connection) => cities.Find(x => x.Id == city.Id) ?? new City());

            mockBusiness.Setup(p => p.Insert(It.IsAny<City>(), It.IsAny<User>(), It.IsAny<IDbConnection>()))
                .Returns((City city, User user, IDbConnection connection) =>
                {
                    if (cities.Exists(x => x.Code == city.Code))
                    {
                        throw new PersistentException();
                    }
                    else
                    {
                        city.Id = cities.Count + 1;
                        cities.Add(city);
                        return city;
                    }
                });

            mockBusiness.Setup(p => p.Update(It.IsAny<City>(), It.IsAny<User>(), It.IsAny<IDbConnection>()))
                .Returns((City city, User user, IDbConnection connection) =>
                {
                    cities.Where(x => x.Id == city.Id).ToList().ForEach(x => x.Code = city.Code);
                    return city;
                });

            mockBusiness.Setup(p => p.Delete(It.IsAny<City>(), It.IsAny<User>(), It.IsAny<IDbConnection>()))
                .Returns((City city, User user, IDbConnection connection) =>
                {
                    cities = cities.Where(x => x.Id != city.Id).ToList();
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
        /// Prueba la consulta de un listado de ciudades con filtros, ordenamientos y límite
        /// </summary>
        [Fact]
        public void CityListTest()
        {
            ListResult<City> list = _api.List("ci.idcity = 1", "ci.name", 1, 0);

            Assert.NotEmpty(list.List);
            Assert.True(list.Total > 0);
        }

        /// <summary>
        /// Prueba la consulta de un listado de ciudades con filtros, ordenamientos y límite y con errores
        /// </summary>
        [Fact]
        public void CityListWithErrorTest()
        {
            ListResult<City> list = _api.List("idpais = 1", "name", 1, 0);

            Assert.Equal(0, list.Total);
        }

        /// <summary>
        /// Prueba la consulta de una ciudad dada su identificador
        /// </summary>
        [Fact]
        public void CityReadTest()
        {
            City city = _api.Read(1);

            Assert.Equal("BOG", city.Code);
        }

        /// <summary>
        /// Prueba la consulta de una ciudad que no existe dado su identificador
        /// </summary>
        [Fact]
        public void CityReadNotFoundTest()
        {
            City city = _api.Read(10);

            Assert.Equal(0, city.Id);
        }

        /// <summary>
        /// Prueba la inserción de una ciudad
        /// </summary>
        [Fact]
        public void CityInsertTest()
        {
            City city = new() { Country = new() { Id = 1 }, Code = "BUC", Name = "Bucaramanga" };
            city = _api.Insert(city);

            Assert.NotEqual(0, city.Id);
        }

        /// <summary>
        /// Prueba la inserción de una ciudad con nombre duplicado
        /// </summary>
        [Fact]
        public void CityInsertDuplicateTest()
        {
            City city = new() { Country = new() { Id = 1 }, Code = "BOG", Name = "Prueba 1" };
            city = _api.Insert(city);

            Assert.Equal(0, city.Id);
        }

        /// <summary>
        /// Prueba la actualización de una ciudad
        /// </summary>
        [Fact]
        public void CityUpdateTest()
        {
            City city = new() { Id = 2, Country = new() { Id = 1 }, Code = "BAQ", Name = "Barranquilla" };
            _ = _api.Update(city);

            City city2 = _api.Read(2);

            Assert.NotEqual("MED", city2.Code);
        }

        /// <summary>
        /// Prueba la eliminación de una ciudad
        /// </summary>
        [Fact]
        public void CityDeleteTest()
        {
            _ = _api.Delete(3);

            City city = _api.Read(3);

            Assert.Equal(0, city.Id);
        }
        #endregion
    }
}

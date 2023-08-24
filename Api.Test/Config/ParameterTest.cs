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
            Mock<IBusiness<Parameter>> mockBusiness = new();
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
                    ControllerName = "ParameterTest",
                    ActionName = "Test"
                }
            };
            _configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", false, false)
                .AddEnvironmentVariables()
                .Build();

            List<Parameter> parameters = new()
            {
                new Parameter() { Id = 1, Name = "Parámetro 1", Value = "Valor 1" },
                new Parameter() { Id = 2, Name = "Parámetro 2", Value = "Valor 2" },
                new Parameter() { Id = 3, Name = "Parámetro 3", Value = "Valor 3" }
            };
            List<Template> templates = new()
            {
                new Template() { Id = 1, Name = "Notificación de error", Content = "<p>Error #{id}#</p><p>La excepci&oacute;n tiene el siguiente mensaje: #{message}#</p>" },
                new Template() { Id = 2, Name = "Recuperación contraseña", Content = "<p>Prueba recuperaci&oacute;n contrase&ntilde;a con enlace #{link}#</p>" },
                new Template() { Id = 3, Name = "Contraseña cambiada", Content = "<p>Prueba de que su contrase&ntilde;a ha sido cambiada con &eacute;xito</p>" }
            };

            mockBusiness.Setup(p => p.List("idparameter = 1", It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>()))
                .Returns(new ListResult<Parameter>(parameters.Where(y => y.Id == 1).ToList(), 1));
            mockBusiness.Setup(p => p.List("idparametro = 1", It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>()))
                .Throws<PersistentException>();

            mockBusiness.Setup(p => p.Read(It.IsAny<Parameter>()))
                .Returns((Parameter parameter) => parameters.Find(x => x.Id == parameter.Id) ?? new Parameter());

            mockBusiness.Setup(p => p.Insert(It.IsAny<Parameter>(), It.IsAny<User>()))
                .Returns((Parameter parameter, User user) =>
                {
                    if (parameters.Exists(x => x.Name == parameter.Name))
                    {
                        throw new PersistentException();
                    }
                    else
                    {
                        parameter.Id = parameters.Count + 1;
                        parameters.Add(parameter);
                        return parameter;
                    }
                });

            mockBusiness.Setup(p => p.Update(It.IsAny<Parameter>(), It.IsAny<User>()))
                .Returns((Parameter parameter, User user) =>
                {
                    parameters.Where(x => x.Id == parameter.Id).ToList().ForEach(x => x.Name = parameter.Name);
                    return parameter;
                });

            mockBusiness.Setup(p => p.Delete(It.IsAny<Parameter>(), It.IsAny<User>()))
                .Returns((Parameter parameter, User user) =>
                {
                    parameters = parameters.Where(x => x.Id != parameter.Id).ToList();
                    return parameter;
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

            Assert.Equal("Parámetro 1", country.Name);
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
            Parameter country = new() { Name = "Parámetro 4", Value = "Valor 4" };
            country = _api.Insert(country);

            Assert.NotEqual(0, country.Id);
        }

        /// <summary>
        /// Prueba la actualización de un parámetro
        /// </summary>
        [Fact]
        public void ParameterUpdateTest()
        {
            Parameter country = new() { Id = 2, Name = "Parámetro 6", Value = "Valor 6" };
            _ = _api.Update(country);

            Parameter country2 = _api.Read(2);

            Assert.NotEqual("Parámetro 2", country2.Name);
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

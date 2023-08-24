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
            Mock<IBusiness<IdentificationType>> mockBusiness = new();
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
                    ControllerName = "IdentificationTypeTest",
                    ActionName = "Test"
                }
            };
            _configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", false, false)
                .AddEnvironmentVariables()
                .Build();

            List<IdentificationType> identificationTypes = new()
            {
                new IdentificationType() { Id = 1, Name = "Cédula ciudadanía" },
                new IdentificationType() { Id = 2, Name = "Cédula extranjería" },
                new IdentificationType() { Id = 3, Name = "Pasaporte" }
            };
            List<Template> templates = new()
            {
                new Template() { Id = 1, Name = "Notificación de error", Content = "<p>Error #{id}#</p><p>La excepci&oacute;n tiene el siguiente mensaje: #{message}#</p>" },
                new Template() { Id = 2, Name = "Recuperación contraseña", Content = "<p>Prueba recuperaci&oacute;n contrase&ntilde;a con enlace #{link}#</p>" },
                new Template() { Id = 3, Name = "Contraseña cambiada", Content = "<p>Prueba de que su contrase&ntilde;a ha sido cambiada con &eacute;xito</p>" }
            };

            mockBusiness.Setup(p => p.List("ididentificationtype = 1", It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>()))
                .Returns(new ListResult<IdentificationType>(identificationTypes.Where(y => y.Id == 1).ToList(), 1));
            mockBusiness.Setup(p => p.List("idtipoidentificacion = 1", It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>()))
                .Throws<PersistentException>();

            mockBusiness.Setup(p => p.Read(It.IsAny<IdentificationType>()))
                .Returns((IdentificationType identificationType) => identificationTypes.Find(x => x.Id == identificationType.Id) ?? new IdentificationType());

            mockBusiness.Setup(p => p.Insert(It.IsAny<IdentificationType>(), It.IsAny<User>()))
                .Returns((IdentificationType identificationType, User user) =>
                {
                    if (identificationTypes.Exists(x => x.Name == identificationType.Name))
                    {
                        throw new PersistentException();
                    }
                    else
                    {
                        identificationType.Id = identificationTypes.Count + 1;
                        identificationTypes.Add(identificationType);
                        return identificationType;
                    }
                });

            mockBusiness.Setup(p => p.Update(It.IsAny<IdentificationType>(), It.IsAny<User>()))
                .Returns((IdentificationType identificationType, User user) =>
                {
                    identificationTypes.Where(x => x.Id == identificationType.Id).ToList().ForEach(x => x.Name = identificationType.Name);
                    return identificationType;
                });

            mockBusiness.Setup(p => p.Delete(It.IsAny<IdentificationType>(), It.IsAny<User>()))
                .Returns((IdentificationType identificationType, User user) =>
                {
                    identificationTypes = identificationTypes.Where(x => x.Id != identificationType.Id).ToList();
                    return identificationType;
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

            Assert.Equal("Cédula ciudadanía", identificationType.Name);
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

            Assert.NotEqual("Cédula extranjería", identificationType2.Name);
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

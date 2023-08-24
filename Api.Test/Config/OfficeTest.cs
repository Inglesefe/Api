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
            Mock<IBusiness<Office>> mockBusiness = new();
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
                    ControllerName = "OfficeTest",
                    ActionName = "Test"
                }
            };
            _configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", false, false)
                .AddEnvironmentVariables()
                .Build();

            List<Office> offices = new()
            {
                new Office() { Id = 1, Name = "Castellana", Address = "Cl 95" },
                new Office() { Id = 2, Name = "Kennedy", Address = "Cl 56 sur" },
                new Office() { Id = 3, Name = "Venecia", Address = "Puente" }
            };
            List<Template> templates = new()
            {
                new Template() { Id = 1, Name = "Notificación de error", Content = "<p>Error #{id}#</p><p>La excepci&oacute;n tiene el siguiente mensaje: #{message}#</p>" },
                new Template() { Id = 2, Name = "Recuperación contraseña", Content = "<p>Prueba recuperaci&oacute;n contrase&ntilde;a con enlace #{link}#</p>" },
                new Template() { Id = 3, Name = "Contraseña cambiada", Content = "<p>Prueba de que su contrase&ntilde;a ha sido cambiada con &eacute;xito</p>" }
            };

            mockBusiness.Setup(p => p.List("o.idoffice = 1", It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>()))
                .Returns(new ListResult<Office>(offices.Where(y => y.Id == 1).ToList(), 1));
            mockBusiness.Setup(p => p.List("idoficina = 1", It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>()))
                .Throws<PersistentException>();

            mockBusiness.Setup(p => p.Read(It.IsAny<Office>()))
                .Returns((Office office) => offices.Find(x => x.Id == office.Id) ?? new Office());

            mockBusiness.Setup(p => p.Insert(It.IsAny<Office>(), It.IsAny<User>()))
                .Returns((Office office, User user) =>
                {
                    if (offices.Exists(x => x.Name == office.Name))
                    {
                        throw new PersistentException();
                    }
                    else
                    {
                        office.Id = offices.Count + 1;
                        offices.Add(office);
                        return office;
                    }
                });

            mockBusiness.Setup(p => p.Update(It.IsAny<Office>(), It.IsAny<User>()))
                .Returns((Office office, User user) =>
                {
                    offices.Where(x => x.Id == office.Id).ToList().ForEach(x => x.Name = office.Name);
                    return office;
                });

            mockBusiness.Setup(p => p.Delete(It.IsAny<Office>(), It.IsAny<User>()))
                .Returns((Office office, User user) =>
                {
                    offices = offices.Where(x => x.Id != office.Id).ToList();
                    return office;
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

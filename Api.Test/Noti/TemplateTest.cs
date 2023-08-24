using Api.Controllers.Noti;
using Business;
using Dal;
using Dal.Dto;
using Dal.Exceptions;
using Entities.Auth;
using Entities.Log;
using Entities.Noti;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.Extensions.Configuration;
using Moq;
using System.Security.Claims;
using System.Security.Principal;

namespace Api.Test.Noti
{
    /// <summary>
    /// Realiza las pruebas sobre la api de plantillas
    /// </summary>
    [Collection("Test")]
    public class TemplateTest
    {
        #region Attributes
        /// <summary>
        /// Configuración de la plantilla de pruebas
        /// </summary>
        private readonly IConfiguration _configuration;

        /// <summary>
        /// Controlador API para las plantillas
        /// </summary>
        private readonly TemplateController _api;

        /// <summary>
        /// Contexto HTTP con que se conecta a los servicios Rest
        /// </summary>
        private readonly ControllerContext _controllerContext;
        #endregion

        #region Constructors
        /// <summary>
        /// Inicializa la configuración de la prueba
        /// </summary>
        public TemplateTest()
        {
            Mock<IBusiness<Template>> mockBusiness = new();
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
                    ControllerName = "TemplateTest",
                    ActionName = "Test"
                }
            };
            _configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", false, false)
                .AddEnvironmentVariables()
                .Build();

            List<Template> templates = new()
            {
                new Template() { Id = 1, Name = "Notificación de error", Content = "<p>Error #{id}#</p><p>La excepci&oacute;n tiene el siguiente mensaje: #{message}#</p>" },
                new Template() { Id = 2, Name = "Recuperación contraseña", Content = "<p>Prueba recuperaci&oacute;n contrase&ntilde;a con enlace #{link}#</p>" },
                new Template() { Id = 3, Name = "Contraseña cambiada", Content = "<p>Prueba de que su contrase&ntilde;a ha sido cambiada con &eacute;xito</p>" }
            };

            mockBusiness.Setup(p => p.List("idtemplate = 1", It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>()))
                .Returns(new ListResult<Template>(templates.Where(y => y.Id == 1).ToList(), 1));
            mockBusiness.Setup(p => p.List("idplantilla = 1", It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>()))
                .Throws<PersistentException>();

            mockBusiness.Setup(p => p.Read(It.IsAny<Template>()))
                .Returns((Template template) => templates.Find(x => x.Id == template.Id) ?? new Template());

            mockBusiness.Setup(p => p.Insert(It.IsAny<Template>(), It.IsAny<User>()))
                .Returns((Template template, User user) =>
                {
                    template.Id = templates.Count + 1;
                    templates.Add(template);
                    return template;
                });

            mockBusiness.Setup(p => p.Update(It.IsAny<Template>(), It.IsAny<User>()))
                .Returns((Template template, User user) =>
                {
                    templates.Where(x => x.Id == template.Id).ToList().ForEach(x =>
                    {
                        x.Name = template.Name;
                        x.Content = template.Content;
                    });
                    return template;
                });

            mockBusiness.Setup(p => p.Delete(It.IsAny<Template>(), It.IsAny<User>()))
                .Returns((Template template, User user) =>
                {
                    templates = templates.Where(x => x.Id != template.Id).ToList();
                    return template;
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
        /// Prueba la consulta de un listado de plantillas con filtros, ordenamientos y límite
        /// </summary>
        /// <returns>N/A</returns>
        [Fact]
        public void TemplateListTest()
        {
            ListResult<Template> list = _api.List("idtemplate = 1", "name", 1, 0);

            Assert.NotEmpty(list.List);
            Assert.True(list.Total > 0);
        }

        /// <summary>
        /// Prueba la consulta de un listado de plantillas con filtros, ordenamientos y límite y con errores
        /// </summary>
        /// <returns>N/A</returns>
        [Fact]
        public void TemplateListWithErrorTest()
        {
            ListResult<Template> list = _api.List("idplantilla = 1", "name", 1, 0);

            Assert.Equal(0, list.Total);
        }

        /// <summary>
        /// Prueba la consulta de una plantilla dada su identificador
        /// </summary>
        /// <returns>N/A</returns>
        [Fact]
        public void TemplateReadTest()
        {
            Template template = _api.Read(1);

            Assert.Equal("Notificación de error", template.Name);
        }

        /// <summary>
        /// Prueba la consulta de una plantilla que no existe dado su identificador
        /// </summary>
        /// <returns>N/A</returns>
        [Fact]
        public void TemplateReadNotFoundTest()
        {
            Template template = _api.Read(10);

            Assert.Equal(0, template.Id);
        }

        /// <summary>
        /// Prueba la inserción de una plantilla
        /// </summary>
        /// <returns>N/A</returns>
        [Fact]
        public void TemplateInsertTest()
        {
            Template template = new() { Name = "Prueba 1", Content = "<p>Prueba de plantilla #{insertada}#" };
            template = _api.Insert(template);

            Assert.NotEqual(0, template.Id);
        }

        /// <summary>
        /// Prueba la actualización de una plantilla
        /// </summary>
        /// <returns>N/A</returns>
        [Fact]
        public void TemplateUpdateTest()
        {
            Template template = new() { Id = 2, Name = "Prueba actualizar", Content = "<p>Prueba de plantilla #{actualizada}#" };
            _ = _api.Update(template);

            Template template2 = _api.Read(2);

            Assert.NotEqual("Recuperación contraseña", template2.Name);
        }

        /// <summary>
        /// Prueba la eliminación de una plantilla
        /// </summary>
        /// <returns>N/A</returns>
        [Fact]
        public void TemplateDeleteTest()
        {
            _ = _api.Delete(3);

            Template template = _api.Read(3);

            Assert.Equal(0, template.Id);
        }
        #endregion
    }
}

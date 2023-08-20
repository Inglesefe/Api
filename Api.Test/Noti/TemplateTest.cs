using Api.Controllers.Noti;
using Dal.Dto;
using Entities.Noti;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.Extensions.Configuration;
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
            _api = new(_configuration)
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
            ListResult<Template> list = _api.List("idnotificacion = 1", "name", 1, 0);

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

            Assert.Equal("Plantilla de prueba", template.Name);
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

            Assert.NotEqual("Plantilla a actualizar", template2.Name);
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

using Api.Controllers.Noti;
using Business;
using Dal.Dto;
using Dal.Exceptions;
using Entities.Auth;
using Entities.Noti;
using Moq;
using System.Data;

namespace Api.Test.Noti
{
    /// <summary>
    /// Realiza las pruebas sobre la api de plantillas
    /// </summary>
    [Collection("Test")]
    public class TemplateTest : TestBase<Template>
    {
        #region Constructors
        /// <summary>
        /// Inicializa la configuración de la prueba
        /// </summary>
        public TemplateTest() : base()
        {
            //Arrange
            Mock<IBusiness<Template>> mockBusiness = new();

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

            api = new TemplateController(configuration, mockBusiness.Object, mockLog.Object, mockTemplate.Object, mockParameter.Object)
            {
                ControllerContext = controllerContext
            };
        }
        #endregion

        #region Methods
        /// <summary>
        /// Prueba la consulta de un listado de plantillas con filtros, ordenamientos y límite
        /// </summary>
        [Fact]
        public void ListTest()
        {
            //Act
            ListResult<Template> list = api.List("idtemplate = 1", "name", 1, 0);

            //Assert
            Assert.NotEmpty(list.List);
            Assert.True(list.Total > 0);
        }

        /// <summary>
        /// Prueba la consulta de un listado de plantillas con filtros, ordenamientos y límite y con errores
        /// </summary>
        [Fact]
        public void ListWithErrorTest()
        {
            //Act
            ListResult<Template> list = api.List("idplantilla = 1", "name", 1, 0);

            //Assert
            Assert.Equal(0, list.Total);
        }

        /// <summary>
        /// Prueba la consulta de una plantilla dada su identificador
        /// </summary>
        [Fact]
        public void ReadTest()
        {
            //Act
            Template template = api.Read(1);

            //Assert
            Assert.Equal("Notificación de error", template.Name);
        }

        /// <summary>
        /// Prueba la consulta de una plantilla que no existe dado su identificador
        /// </summary>
        [Fact]
        public void ReadNotFoundTest()
        {
            //Act
            Template template = api.Read(10);

            //Assert
            Assert.Equal(0, template.Id);
        }

        /// <summary>
        /// Prueba la inserción de una plantilla
        /// </summary>
        [Fact]
        public void InsertTest()
        {
            //Arrange
            Template template = new() { Name = "Prueba 1", Content = "<p>Prueba de plantilla #{insertada}#" };

            //Act
            template = api.Insert(template);

            //Assert
            Assert.NotEqual(0, template.Id);
        }

        /// <summary>
        /// Prueba la actualización de una plantilla
        /// </summary>
        [Fact]
        public void UpdateTest()
        {
            //Arrange
            Template template = new() { Id = 2, Name = "Prueba actualizar", Content = "<p>Prueba de plantilla #{actualizada}#" };

            //Act
            _ = api.Update(template);
            Template template2 = api.Read(2);

            //Assert
            Assert.NotEqual("Recuperación contraseña", template2.Name);
        }

        /// <summary>
        /// Prueba la eliminación de una plantilla
        /// </summary>
        [Fact]
        public void DeleteTest()
        {
            //Act
            _ = api.Delete(3);
            Template template = api.Read(3);

            //Assert
            Assert.Equal(0, template.Id);
        }
        #endregion
    }
}

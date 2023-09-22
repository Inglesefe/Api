using Api.Controllers.Noti;
using Business;
using Entities.Noti;
using Moq;

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
                new Template() { Id = 1, Name = "Notificación de error", Content = "<p>Error #{id}#</p><p>La excepci&oacute;n tiene el siguiente mensaje: #{message}#</p>" }
            };
            mockBusiness.Setup(p => p.Read(It.IsAny<Template>()))
                .Returns((Template template) => templates.Find(x => x.Id == template.Id) ?? new Template());
            api = new TemplateController(configuration, mockBusiness.Object, mockLog.Object, mockTemplate.Object, mockParameter.Object)
            {
                ControllerContext = controllerContext
            };
        }
        #endregion

        #region Methods
        /// <summary>
        /// Prueba la consulta de una plantilla dada su identificador
        /// </summary>
        [Fact]
        public void ReadTest()
        {
            //Act
            Template template = api != null ? api.Read(1) : new();

            //Assert
            Assert.Equal("Notificación de error", template.Name);
        }
        #endregion
    }
}

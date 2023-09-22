using Api.Controllers.Config;
using Business;
using Entities.Config;
using Moq;

namespace Api.Test.Config
{
    /// <summary>
    /// Realiza las pruebas sobre la api de tipos de identificación
    /// </summary>
    [Collection("Test")]
    public class IdentificationTypeTest : TestBase<IdentificationType>
    {
        #region Constructors
        /// <summary>
        /// Inicializa la configuración de la prueba
        /// </summary>
        public IdentificationTypeTest() : base()
        {
            //Arrange
            Mock<IBusiness<IdentificationType>> mockBusiness = new();

            List<IdentificationType> identificationTypes = new()
            {
                new IdentificationType() { Id = 1, Name = "Cédula ciudadanía" }
            };
            mockBusiness.Setup(p => p.Read(It.IsAny<IdentificationType>()))
                .Returns((IdentificationType identificationType) => identificationTypes.Find(x => x.Id == identificationType.Id) ?? new IdentificationType());
            api = new IdentificationTypeController(configuration, mockBusiness.Object, mockLog.Object, mockTemplate.Object, mockParameter.Object)
            {
                ControllerContext = controllerContext
            };
        }
        #endregion

        #region Methods
        /// <summary>
        /// Prueba la consulta de un tipo de identificación dada su identificador
        /// </summary>
        [Fact]
        public void ReadTest()
        {
            //Act
            IdentificationType identificationType = api != null ? api.Read(1) : new();

            //Assert
            Assert.Equal("Cédula ciudadanía", identificationType.Name);
        }
        #endregion
    }
}

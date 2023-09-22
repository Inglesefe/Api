using Api.Controllers.Admon;
using Business;
using Entities.Admon;
using Moq;

namespace Api.Test.Admon
{
    /// <summary>
    /// Realiza las pruebas sobre la api de escalas asociadas a matrículas
    /// </summary>
    [Collection("Test")]
    public class RegistrationScaleTest : TestBase<RegistrationScale>
    {
        #region Constructors
        /// <summary>
        /// Inicializa la configuración de la prueba
        /// </summary>
        public RegistrationScaleTest() : base()
        {
            //Arrange
            Mock<IBusiness<RegistrationScale>> mockBusiness = new();

            List<RegistrationScale> registrationScales = new()
            {
                new RegistrationScale() { Id = 1, Registration = new(){Id = 1}, Scale = new(){ Id = 1 }, AccountExecutive = new(){ Id = 1} }
            };
            mockBusiness.Setup(p => p.Read(It.IsAny<RegistrationScale>()))
                .Returns((RegistrationScale regScale) => registrationScales.Find(x => x.Id == regScale.Id) ?? new RegistrationScale());
            api = new RegistrationScaleController(configuration, mockBusiness.Object, mockLog.Object, mockTemplate.Object, mockParameter.Object)
            {
                ControllerContext = controllerContext
            };
        }
        #endregion

        #region Methods
        /// <summary>
        /// Prueba la consulta de una escala asociada a matrícula dado su identificador
        /// </summary>
        [Fact]
        public void ReadTest()
        {
            //Act
            RegistrationScale regScale = api != null ? api.Read(1) : new();

            //Assert
            Assert.Equal(1, regScale.Registration.Id);
            Assert.Equal(1, regScale.Scale.Id);
            Assert.Equal(1, regScale.AccountExecutive.Id);
        }
        #endregion
    }
}

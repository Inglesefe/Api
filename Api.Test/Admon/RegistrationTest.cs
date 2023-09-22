using Api.Controllers.Admon;
using Business;
using Entities.Admon;
using Moq;

namespace Api.Test.Admon
{
    /// <summary>
    /// Realiza las pruebas sobre la api de matrículas
    /// </summary>
    [Collection("Test")]
    public class RegistrationTest : TestBase<Registration>
    {
        #region Constructors
        /// <summary>
        /// Inicializa la configuración de la prueba
        /// </summary>
        public RegistrationTest() : base()
        {
            //Arrange
            Mock<IBusiness<Registration>> mockBusiness = new();

            List<Registration> registrations = new()
            {
                new Registration() {
                    Id = 1,
                    Office = new(){ Id = 1 },
                    Date = DateTime.Now,
                    ContractNumber = "255657",
                    Owner = new(){ Id = 1 },
                    Beneficiary1 = new(){ Id = 1 },
                    Beneficiary2 = new(){ Id = 2 },
                    Plan = new(){ Id = 1 }
                }
            };
            mockBusiness.Setup(p => p.Read(It.IsAny<Registration>()))
                .Returns((Registration registration) => registrations.Find(x => x.Id == registration.Id) ?? new Registration());
            api = new RegistrationController(configuration, mockBusiness.Object, mockLog.Object, mockTemplate.Object, mockParameter.Object)
            {
                ControllerContext = controllerContext
            };
        }
        #endregion

        #region Methods
        /// <summary>
        /// Prueba la consulta de una matrícula dado su identificador
        /// </summary>
        [Fact]
        public void ReadTest()
        {
            //Act
            Registration registration = api != null ? api.Read(1) : new();

            //Assert
            Assert.Equal(1, registration.Office.Id);
            Assert.Equal("255657", registration.ContractNumber);
        }
        #endregion
    }
}

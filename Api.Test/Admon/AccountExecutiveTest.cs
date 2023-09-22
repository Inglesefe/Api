using Api.Controllers.Admon;
using Business;
using Entities.Admon;
using Moq;

namespace Api.Test.Admon
{
    /// <summary>
    /// Realiza las pruebas sobre la api de ejecutivos de cuenta
    /// </summary>
    [Collection("Test")]
    public class AccountExecutiveTest : TestBase<AccountExecutive>
    {
        #region Constructors
        /// <summary>
        /// Inicializa la configuración de la prueba
        /// </summary>
        public AccountExecutiveTest() : base()
        {
            //Arrange
            Mock<IBusiness<AccountExecutive>> mockBusiness = new();
            List<AccountExecutive> executives = new()
            {
                new AccountExecutive() { Id = 1, Name = "Leandro Baena Torres", IdentificationType = new(){ Id = 1 }, Identification = "123456789" }
            };

            mockBusiness.Setup(p => p.Read(It.IsAny<AccountExecutive>()))
                .Returns((AccountExecutive executive) => executives.Find(x => x.Id == executive.Id) ?? new AccountExecutive());

            api = new AccountExecutiveController(configuration, mockBusiness.Object, mockLog.Object, mockTemplate.Object, mockParameter.Object)
            {
                ControllerContext = controllerContext
            };
        }
        #endregion

        #region Methods
        /// <summary>
        /// Prueba la consulta de un ejecutivo de cuenta dado su identificador
        /// </summary>
        [Fact]
        public void ReadTest()
        {
            //Act
            AccountExecutive executive = api != null ? api.Read(1) : new();

            //Assert
            Assert.Equal("Leandro Baena Torres", executive.Name);
        }
        #endregion
    }
}

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

            api = new AccountExecutiveController(configuration, mockBusiness.Object, mockLog.Object, mockTemplate.Object, mockParameter.Object)
            {
                ControllerContext = controllerContext
            };
        }
        #endregion

        #region Methods
        /// <summary>
        /// Prueba la creación de un nuevo controlador para ejecutivos de cuenta
        /// </summary>
        [Fact]
        public void CreateControllerTest()
        {
            //Assert
            Assert.IsType<AccountExecutiveController>(api);
        }
        #endregion
    }
}

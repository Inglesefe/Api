using Api.Controllers.Cont;
using Business;
using Entities.Cont;
using Moq;

namespace Api.Test.Cont
{
    /// <summary>
    /// Realiza las pruebas sobre la api de tipos de cuenta
    /// </summary>
    [Collection("Test")]
    public class AccountTypeTest : TestBase<AccountType>
    {
        #region Constructors
        /// <summary>
        /// Inicializa la configuración de la prueba
        /// </summary>
        public AccountTypeTest() : base()
        {
            //Arrange
            Mock<IBusiness<AccountType>> mockBusiness = new();

            List<AccountType> types = new()
            {
                new AccountType() { Id = 1, Name = "Caja" }
            };
            mockBusiness.Setup(p => p.Read(It.IsAny<AccountType>()))
                .Returns((AccountType type) => types.Find(x => x.Id == type.Id) ?? new AccountType());
            api = new AccountTypeController(configuration, mockBusiness.Object, mockLog.Object, mockTemplate.Object, mockParameter.Object)
            {
                ControllerContext = controllerContext
            };
        }
        #endregion

        #region Methods
        /// <summary>
        /// Prueba la consulta de un tipo de cuenta dada su identificador
        /// </summary>
        [Fact]
        public void ReadTest()
        {
            //Act
            AccountType type = api != null ? api.Read(1) : new();

            //Assert
            Assert.Equal("Caja", type.Name);
        }
        #endregion
    }
}

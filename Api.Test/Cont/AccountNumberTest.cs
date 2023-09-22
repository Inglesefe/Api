using Api.Controllers.Cont;
using Business;
using Entities.Cont;
using Moq;

namespace Api.Test.Cont
{
    /// <summary>
    /// Realiza las pruebas sobre la api de números de cuenta
    /// </summary>
    [Collection("Test")]
    public class AccountNumberTest : TestBase<AccountNumber>
    {
        #region Constructors
        /// <summary>
        /// Inicializa la configuración de la prueba
        /// </summary>
        public AccountNumberTest() : base()
        {
            //Arrange
            Mock<IBusiness<AccountNumber>> mockBusiness = new();

            List<AccountNumber> numbers = new()
            {
                new AccountNumber() { Id = 1, AccountType = new(){ Id = 1 }, City = new(){ Id = 1 }, Number = "123456789" }
            };
            mockBusiness.Setup(p => p.Read(It.IsAny<AccountNumber>()))
                .Returns((AccountNumber number) => numbers.Find(x => x.Id == number.Id) ?? new AccountNumber());
            api = new AccountNumberController(configuration, mockBusiness.Object, mockLog.Object, mockTemplate.Object, mockParameter.Object)
            {
                ControllerContext = controllerContext
            };
        }
        #endregion

        #region Methods
        /// <summary>
        /// Prueba la consulta de un número de cuenta dada su identificador
        /// </summary>
        [Fact]
        public void ReadTest()
        {
            //Act
            AccountNumber number = api != null ? api.Read(1) : new();

            //Assert
            Assert.Equal("123456789", number.Number);
        }
        #endregion
    }
}

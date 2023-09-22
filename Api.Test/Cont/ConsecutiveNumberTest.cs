using Api.Controllers.Cont;
using Business;
using Entities.Cont;
using Moq;

namespace Api.Test.Cont
{
    /// <summary>
    /// Realiza las pruebas sobre la api de números de consecutivos
    /// </summary>
    [Collection("Test")]
    public class ConsecutiveNumberTest : TestBase<ConsecutiveNumber>
    {
        #region Constructors
        /// <summary>
        /// Inicializa la configuración de la prueba
        /// </summary>
        public ConsecutiveNumberTest() : base()
        {
            //Arrange
            Mock<IBusiness<ConsecutiveNumber>> mockBusiness = new();

            List<ConsecutiveNumber> numbers = new()
            {
                new ConsecutiveNumber() { Id = 1, ConsecutiveType = new(){ Id = 1 }, City = new(){ Id = 1 }, Number = "100" }
            };
            mockBusiness.Setup(p => p.Read(It.IsAny<ConsecutiveNumber>()))
                .Returns((ConsecutiveNumber number) => numbers.Find(x => x.Id == number.Id) ?? new ConsecutiveNumber());
            api = new ConsecutiveNumberController(configuration, mockBusiness.Object, mockLog.Object, mockTemplate.Object, mockParameter.Object)
            {
                ControllerContext = controllerContext
            };
        }
        #endregion

        #region Methods
        /// <summary>
        /// Prueba la consulta de un número de consecutivo dada su identificador
        /// </summary>
        [Fact]
        public void ReadTest()
        {
            //Act
            ConsecutiveNumber number = api != null ? api.Read(1) : new();

            //Assert
            Assert.Equal("100", number.Number);
        }
        #endregion
    }
}

using Api.Controllers.Config;
using Business;
using Entities.Config;
using Moq;

namespace Api.Test.Config
{
    /// <summary>
    /// Realiza las pruebas sobre la api de tipos de pagos
    /// </summary>
    [Collection("Test")]
    public class PaymentTypeTest : TestBase<PaymentType>
    {
        #region Constructors
        /// <summary>
        /// Inicializa la configuración de la prueba
        /// </summary>
        public PaymentTypeTest() : base()
        {
            //Arrange
            Mock<IBusiness<PaymentType>> mockBusiness = new();

            List<PaymentType> types = new()
            {
                new PaymentType() { Id = 1, Name = "Efectivo" }
            };
            mockBusiness.Setup(p => p.Read(It.IsAny<PaymentType>()))
                .Returns((PaymentType type) => types.Find(x => x.Id == type.Id) ?? new PaymentType());
            api = new PaymentTypeController(configuration, mockBusiness.Object, mockLog.Object, mockTemplate.Object, mockParameter.Object)
            {
                ControllerContext = controllerContext
            };
        }
        #endregion

        #region Methods
        /// <summary>
        /// Prueba la consulta de un tipo de pago dada su identificador
        /// </summary>
        [Fact]
        public void ReadTest()
        {
            //Act
            PaymentType type = api != null ? api.Read(1) : new();

            //Assert
            Assert.Equal("Efectivo", type.Name);
        }
        #endregion
    }
}

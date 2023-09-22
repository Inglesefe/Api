using Api.Controllers.Cont;
using Business;
using Entities.Cont;
using Moq;

namespace Api.Test.Cont
{
    /// <summary>
    /// Realiza las pruebas sobre la api de pagos
    /// </summary>
    [Collection("Test")]
    public class PaymentTest : TestBase<Payment>
    {
        #region Constructors
        /// <summary>
        /// Inicializa la configuración de la prueba
        /// </summary>
        public PaymentTest() : base()
        {
            //Arrange
            Mock<IBusiness<Payment>> mockBusiness = new();

            List<Payment> payments = new()
            {
                new Payment() {
                    Id = 1,
                    PaymentType = new(){ Id = 1 },
                    Fee = new(){ Id = 1 },
                    Value = 1500,
                    Date = DateTime.Now,
                    Invoice = "101-000001",
                    Proof = "http://localhost/prueba1.png"
                }
            };
            mockBusiness.Setup(p => p.Read(It.IsAny<Payment>()))
                .Returns((Payment payment) => payments.Find(x => x.Id == payment.Id) ?? new Payment());
            api = new PaymentController(configuration, mockBusiness.Object, mockLog.Object, mockTemplate.Object, mockParameter.Object)
            {
                ControllerContext = controllerContext
            };
        }
        #endregion

        #region Methods
        /// <summary>
        /// Prueba la consulta de un pago dada su identificador
        /// </summary>
        [Fact]
        public void ReadTest()
        {
            //Act
            Payment payment = api != null ? api.Read(1) : new();

            //Assert
            Assert.Equal(1500, payment.Value);
        }
        #endregion
    }
}

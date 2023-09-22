using Api.Controllers.Admon;
using Business;
using Entities.Admon;
using Moq;

namespace Api.Test.Admon
{
    /// <summary>
    /// Realiza las pruebas sobre la api de cuotas de matrículas
    /// </summary>
    [Collection("Test")]
    public class FeeTest : TestBase<Fee>
    {
        #region Constructors
        /// <summary>
        /// Inicializa la configuración de la prueba
        /// </summary>
        public FeeTest() : base()
        {
            //Arrange
            Mock<IBusiness<Fee>> mockBusiness = new();

            List<Fee> fees = new()
            {
                new Fee() {
                    Id = 1,
                    Registration = new(){ Id = 1 },
                    Value = 1000,
                    Number = 1,
                    IncomeType = new(){ Id = 1 },
                    DueDate = DateTime.Now
                }
            };
            mockBusiness.Setup(p => p.Read(It.IsAny<Fee>()))
                .Returns((Fee fee) => fees.Find(x => x.Id == fee.Id) ?? new Fee());
            api = new FeeController(configuration, mockBusiness.Object, mockLog.Object, mockTemplate.Object, mockParameter.Object)
            {
                ControllerContext = controllerContext
            };
        }
        #endregion

        #region Methods
        /// <summary>
        /// Prueba la consulta de una cuota de matrícula dado su identificador
        /// </summary>
        [Fact]
        public void ReadTest()
        {
            //Act
            Fee fee = api != null ? api.Read(1) : new();

            //Assert
            Assert.Equal(1000, fee.Value);
            Assert.Equal(1, fee.Number);
        }
        #endregion
    }
}

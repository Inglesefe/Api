using Api.Controllers.Config;
using Business;
using Entities.Config;
using Moq;

namespace Api.Test.Config
{
    /// <summary>
    /// Realiza las pruebas sobre la api de tipos de ingreso
    /// </summary>
    [Collection("Test")]
    public class IncomeTypeTest : TestBase<IncomeType>
    {
        #region Constructors
        /// <summary>
        /// Inicializa la configuración de la prueba
        /// </summary>
        public IncomeTypeTest() : base()
        {
            Mock<IBusiness<IncomeType>> mockBusiness = new();

            List<IncomeType> incomeTypes = new()
            {
                new IncomeType() { Id = 1, Code = "CI", Name = "Cuota inicial" }
            };
            mockBusiness.Setup(p => p.Read(It.IsAny<IncomeType>()))
                .Returns((IncomeType incomeType) => incomeTypes.Find(x => x.Id == incomeType.Id) ?? new IncomeType());
            api = new IncomeTypeController(configuration, mockBusiness.Object, mockLog.Object, mockTemplate.Object, mockParameter.Object)
            {
                ControllerContext = controllerContext
            };
        }
        #endregion

        #region Methods
        /// <summary>
        /// Prueba la consulta de un tipo de ingreso dada su identificador
        /// </summary>
        [Fact]
        public void ReadTest()
        {
            //Act
            IncomeType incomeType = api != null ? api.Read(1) : new();

            //Assert
            Assert.Equal("CI", incomeType.Code);
        }
        #endregion
    }
}

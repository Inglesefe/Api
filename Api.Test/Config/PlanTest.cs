using Api.Controllers.Config;
using Business;
using Entities.Config;
using Moq;

namespace Api.Test.Config
{
    /// <summary>
    /// Realiza las pruebas sobre la api de planes
    /// </summary>
    [Collection("Test")]
    public class PlanTest : TestBase<Plan>
    {
        #region Constructors
        /// <summary>
        /// Inicializa la configuración de la prueba
        /// </summary>
        public PlanTest() : base()
        {
            //Arrange
            Mock<IBusiness<Plan>> mockBusiness = new();

            List<Plan> plans = new()
            {
                new Plan() { Id = 1, Value = 3779100, InitialFee = 444600, InstallmentsNumber = 12, InstallmentValue = 444600, Active = true, Description = "PLAN COFREM 12 MESES" }
            };
            mockBusiness.Setup(p => p.Read(It.IsAny<Plan>()))
                .Returns((Plan plan) => plans.Find(x => x.Id == plan.Id) ?? new Plan());
            api = new PlanController(configuration, mockBusiness.Object, mockLog.Object, mockTemplate.Object, mockParameter.Object)
            {
                ControllerContext = controllerContext
            };
        }
        #endregion

        #region Methods
        /// <summary>
        /// Prueba la consulta de un plan dada su identificador
        /// </summary>
        [Fact]
        public void ReadTest()
        {
            //Act
            Plan plan = api != null ? api.Read(1) : new();

            //Assert
            Assert.Equal(12, plan.InstallmentsNumber);
        }
        #endregion
    }
}

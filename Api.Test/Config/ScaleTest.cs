using Api.Controllers.Config;
using Business;
using Entities.Config;
using Moq;

namespace Api.Test.Config
{
    /// <summary>
    /// Realiza las pruebas sobre la api de escalas
    /// </summary>
    [Collection("Test")]
    public class ScaleTest : TestBase<Scale>
    {
        #region Constructors
        /// <summary>
        /// Inicializa la configuración de la prueba
        /// </summary>
        public ScaleTest() : base()
        {
            //Arrange
            Mock<IBusiness<Scale>> mockBusiness = new();

            List<Scale> scales = new()
            {
                new Scale() { Id = 1, Code= "C1", Name = "Comisión 1", Comission = 1000, Order = 1 }
            };
            mockBusiness.Setup(p => p.Read(It.IsAny<Scale>()))
                .Returns((Scale scale) => scales.Find(x => x.Id == scale.Id) ?? new Scale());
            api = new ScaleController(configuration, mockBusiness.Object, mockLog.Object, mockTemplate.Object, mockParameter.Object)
            {
                ControllerContext = controllerContext
            };
        }
        #endregion

        #region Methods
        /// <summary>
        /// Prueba la consulta de una escala dada su identificador
        /// </summary>
        [Fact]
        public void ReadTest()
        {
            //Act
            Scale scale = api != null ? api.Read(1) : new();

            //Assert
            Assert.Equal(1000, scale.Comission);
        }
        #endregion
    }
}

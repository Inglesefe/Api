using Api.Controllers.Cont;
using Business;
using Entities.Cont;
using Moq;

namespace Api.Test.Cont
{
    /// <summary>
    /// Realiza las pruebas sobre la api de tipos de consecutivo
    /// </summary>
    [Collection("Test")]
    public class ConsecutiveTypeTest : TestBase<ConsecutiveType>
    {
        #region Constructors
        /// <summary>
        /// Inicializa la configuración de la prueba
        /// </summary>
        public ConsecutiveTypeTest() : base()
        {
            //Arrange
            Mock<IBusiness<ConsecutiveType>> mockBusiness = new();

            List<ConsecutiveType> types = new()
            {
                new ConsecutiveType() { Id = 1, Name = "Recibos de caja" }
            };
            mockBusiness.Setup(p => p.Read(It.IsAny<ConsecutiveType>()))
                .Returns((ConsecutiveType type) => types.Find(x => x.Id == type.Id) ?? new ConsecutiveType());
            api = new ConsecutiveTypeController(configuration, mockBusiness.Object, mockLog.Object, mockTemplate.Object, mockParameter.Object)
            {
                ControllerContext = controllerContext
            };
        }
        #endregion

        #region Methods
        /// <summary>
        /// Prueba la consulta de un tipo de consecutivo dada su identificador
        /// </summary>
        [Fact]
        public void ReadTest()
        {
            //Act
            ConsecutiveType type = api != null ? api.Read(1) : new();

            //Assert
            Assert.Equal("Recibos de caja", type.Name);
        }
        #endregion
    }
}

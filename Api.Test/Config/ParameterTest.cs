using Api.Controllers.Config;
using Business;
using Entities.Config;
using Moq;

namespace Api.Test.Config
{
    /// <summary>
    /// Realiza las pruebas sobre la api de parámetros
    /// </summary>
    [Collection("Test")]
    public class ParameterTest : TestBase<Parameter>
    {
        #region Constructors
        /// <summary>
        /// Inicializa la configuración de la prueba
        /// </summary>
        public ParameterTest() : base()
        {
            //Arrange
            Mock<IBusiness<Parameter>> mockBusiness = new();

            List<Parameter> parameters = new()
            {
                new Parameter() { Id = 1, Name = "Parámetro 1", Value = "Valor 1" }
            };
            mockBusiness.Setup(p => p.Read(It.IsAny<Parameter>()))
                .Returns((Parameter parameter) => parameters.Find(x => x.Id == parameter.Id) ?? new Parameter());
            api = new ParameterController(configuration, mockBusiness.Object, mockLog.Object, mockTemplate.Object, mockParameter.Object)
            {
                ControllerContext = controllerContext
            };
        }
        #endregion

        #region Methods
        /// <summary>
        /// Prueba la consulta de un parámetro dada su identificador
        /// </summary>
        [Fact]
        public void ReadTest()
        {
            //Act
            Parameter country = api != null ? api.Read(1) : new();

            //Assert
            Assert.Equal("Parámetro 1", country.Name);
        }
        #endregion
    }
}

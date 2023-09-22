using Api.Controllers.Config;
using Business;
using Entities.Config;
using Moq;

namespace Api.Test.Config
{
    /// <summary>
    /// Realiza las pruebas sobre la api de ciudades
    /// </summary>
    [Collection("Test")]
    public class CityTest : TestBase<City>
    {
        #region Constructors
        /// <summary>
        /// Inicializa la configuración de la prueba
        /// </summary>
        public CityTest() : base()
        {
            //Arrange
            Mock<IBusiness<City>> mockBusiness = new();

            List<City> cities = new()
            {
                new City() { Id = 1, Code = "BOG", Name = "Bogotá" }
            };
            mockBusiness.Setup(p => p.Read(It.IsAny<City>()))
                .Returns((City city) => cities.Find(x => x.Id == city.Id) ?? new City());
            api = new CityController(configuration, mockBusiness.Object, mockLog.Object, mockTemplate.Object, mockParameter.Object)
            {
                ControllerContext = controllerContext
            };
        }
        #endregion

        #region Methods
        /// <summary>
        /// Prueba la consulta de una ciudad dada su identificador
        /// </summary>
        [Fact]
        public void ReadTest()
        {
            //Act
            City city = api != null ? api.Read(1) : new();

            //Assert
            Assert.Equal("BOG", city.Code);
        }
        #endregion
    }
}

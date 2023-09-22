using Api.Controllers.Config;
using Business;
using Entities.Config;
using Moq;

namespace Api.Test.Config
{
    /// <summary>
    /// Realiza las pruebas sobre la api de paises
    /// </summary>
    [Collection("Test")]
    public class CountryTest : TestBase<Country>
    {
        #region Constructors
        /// <summary>
        /// Inicializa la configuración de la prueba
        /// </summary>
        public CountryTest() : base()
        {
            //Arrange
            Mock<IBusiness<Country>> mockBusiness = new();

            List<Country> countries = new()
            {
                new Country() { Id = 1, Code = "CO", Name = "Colombia" }
            };
            mockBusiness.Setup(p => p.Read(It.IsAny<Country>()))
                .Returns((Country country) => countries.Find(x => x.Id == country.Id) ?? new Country());
            api = new CountryController(configuration, mockBusiness.Object, mockLog.Object, mockTemplate.Object, mockParameter.Object)
            {
                ControllerContext = controllerContext
            };
        }
        #endregion

        #region Methods
        /// <summary>
        /// Prueba la consulta de un país dada su identificador
        /// </summary>
        [Fact]
        public void ReadTest()
        {
            //Act
            Country country = api != null ? api.Read(1) : new();

            //Assert
            Assert.Equal("CO", country.Code);
        }
        #endregion
    }
}

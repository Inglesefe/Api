using Api.Controllers.Config;
using Business;
using Dal.Dto;
using Dal.Exceptions;
using Entities.Auth;
using Entities.Config;
using Moq;
using System.Data;

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
                new Country() { Id = 1, Code = "CO", Name = "Colombia" },
                new Country() { Id = 2, Code = "US", Name = "Estados unidos" },
                new Country() { Id = 3, Code = "EN", Name = "Inglaterra" }
            };

            mockBusiness.Setup(p => p.List("idcountry = 1", It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>()))
                .Returns(new ListResult<Country>(countries.Where(y => y.Id == 1).ToList(), 1));
            mockBusiness.Setup(p => p.List("idpais = 1", It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>()))
                .Throws<PersistentException>();

            mockBusiness.Setup(p => p.Read(It.IsAny<Country>()))
                .Returns((Country country) => countries.Find(x => x.Id == country.Id) ?? new Country());

            mockBusiness.Setup(p => p.Insert(It.IsAny<Country>(), It.IsAny<User>()))
                .Returns((Country country, User user) =>
                {
                    if (countries.Exists(x => x.Code == country.Code))
                    {
                        throw new PersistentException();
                    }
                    else
                    {
                        country.Id = countries.Count + 1;
                        countries.Add(country);
                        return country;
                    }
                });

            mockBusiness.Setup(p => p.Update(It.IsAny<Country>(), It.IsAny<User>()))
                .Returns((Country country, User user) =>
                {
                    countries.Where(x => x.Id == country.Id).ToList().ForEach(x => x.Code = country.Code);
                    return country;
                });

            mockBusiness.Setup(p => p.Delete(It.IsAny<Country>(), It.IsAny<User>()))
                .Returns((Country city, User user) =>
                {
                    countries = countries.Where(x => x.Id != city.Id).ToList();
                    return city;
                });

            api = new CountryController(configuration, mockBusiness.Object, mockLog.Object, mockTemplate.Object, mockParameter.Object)
            {
                ControllerContext = controllerContext
            };
        }
        #endregion

        #region Methods
        /// <summary>
        /// Prueba la consulta de un listado de paises con filtros, ordenamientos y límite
        /// </summary>
        [Fact]
        public void ListTest()
        {
            //Act
            ListResult<Country> list = api.List("idcountry = 1", "name", 1, 0);

            //Assert
            Assert.NotEmpty(list.List);
            Assert.True(list.Total > 0);
        }

        /// <summary>
        /// Prueba la consulta de un listado de paises con filtros, ordenamientos y límite y con errores
        /// </summary>
        [Fact]
        public void ListWithErrorTest()
        {
            //Act
            ListResult<Country> list = api.List("idpais = 1", "name", 1, 0);

            //Assert
            Assert.Equal(0, list.Total);
        }

        /// <summary>
        /// Prueba la consulta de un país dada su identificador
        /// </summary>
        [Fact]
        public void ReadTest()
        {
            //Act
            Country country = api.Read(1);

            //Assert
            Assert.Equal("CO", country.Code);
        }

        /// <summary>
        /// Prueba la consulta de un país que no existe dado su identificador
        /// </summary>
        [Fact]
        public void ReadNotFoundTest()
        {
            //Act
            Country country = api.Read(10);

            //Assert
            Assert.Equal(0, country.Id);
        }

        /// <summary>
        /// Prueba la inserción de un país
        /// </summary>
        [Fact]
        public void InsertTest()
        {
            //Arrange
            Country country = new() { Code = "PR", Name = "Puerto Rico" };

            //Act
            country = api.Insert(country);

            //Assert
            Assert.NotEqual(0, country.Id);
        }

        /// <summary>
        /// Prueba la inserción de un país con nombre duplicado
        /// </summary>
        [Fact]
        public void InsertDuplicateTest()
        {
            //Arrange
            Country country = new() { Code = "CO", Name = "Colombia" };

            //Act
            country = api.Insert(country);

            //Assert
            Assert.Equal(0, country.Id);
        }

        /// <summary>
        /// Prueba la actualización de un país
        /// </summary>
        [Fact]
        public void UpdateTest()
        {
            //Arrange
            Country country = new() { Id = 2, Code = "PE", Name = "Perú" };

            //Act
            _ = api.Update(country);
            Country country2 = api.Read(2);

            //Assert
            Assert.NotEqual("US", country2.Code);
        }

        /// <summary>
        /// Prueba la eliminación de un país
        /// </summary>
        [Fact]
        public void DeleteTest()
        {
            //Act
            _ = api.Delete(3);
            Country country = api.Read(3);

            //Assert
            Assert.Equal(0, country.Id);
        }
        #endregion
    }
}

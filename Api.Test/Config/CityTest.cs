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
                new City() { Id = 1, Code = "BOG", Name = "Bogotá" },
                new City() { Id = 1, Code = "MED", Name = "Medellín" },
                new City() { Id = 1, Code = "CAL", Name = "Cali" }
            };

            mockBusiness.Setup(p => p.List("idcity = 1", It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>()))
                .Returns(new ListResult<City>(cities.Where(y => y.Id == 1).ToList(), 1));
            mockBusiness.Setup(p => p.List("idciudad = 1", It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>()))
                .Throws<PersistentException>();

            mockBusiness.Setup(p => p.Read(It.IsAny<City>()))
                .Returns((City city) => cities.Find(x => x.Id == city.Id) ?? new City());

            mockBusiness.Setup(p => p.Insert(It.IsAny<City>(), It.IsAny<User>()))
                .Returns((City city, User user) =>
                {
                    if (cities.Exists(x => x.Code == city.Code))
                    {
                        throw new PersistentException();
                    }
                    else
                    {
                        city.Id = cities.Count + 1;
                        cities.Add(city);
                        return city;
                    }
                });

            mockBusiness.Setup(p => p.Update(It.IsAny<City>(), It.IsAny<User>()))
                .Returns((City city, User user) =>
                {
                    cities.Where(x => x.Id == city.Id).ToList().ForEach(x => x.Code = city.Code);
                    return city;
                });

            mockBusiness.Setup(p => p.Delete(It.IsAny<City>(), It.IsAny<User>()))
                .Returns((City city, User user) =>
                {
                    cities = cities.Where(x => x.Id != city.Id).ToList();
                    return city;
                });

            api = new CityController(configuration, mockBusiness.Object, mockLog.Object, mockTemplate.Object, mockParameter.Object)
            {
                ControllerContext = controllerContext
            };
        }
        #endregion

        #region Methods
        /// <summary>
        /// Prueba la consulta de un listado de ciudades con filtros, ordenamientos y límite
        /// </summary>
        [Fact]
        public void ListTest()
        {
            //Act
            ListResult<City> list = api.List("idcity = 1", "name", 1, 0);

            //Assert
            Assert.NotEmpty(list.List);
            Assert.True(list.Total > 0);
        }

        /// <summary>
        /// Prueba la consulta de un listado de ciudades con filtros, ordenamientos y límite y con errores
        /// </summary>
        [Fact]
        public void ListWithErrorTest()
        {
            //Act
            ListResult<City> list = api.List("idciudad = 1", "name", 1, 0);

            //Assert
            Assert.Equal(0, list.Total);
        }

        /// <summary>
        /// Prueba la consulta de una ciudad dada su identificador
        /// </summary>
        [Fact]
        public void ReadTest()
        {
            //Act
            City city = api.Read(1);

            //Assert
            Assert.Equal("BOG", city.Code);
        }

        /// <summary>
        /// Prueba la consulta de una ciudad que no existe dado su identificador
        /// </summary>
        [Fact]
        public void ReadNotFoundTest()
        {
            //Act
            City city = api.Read(10);

            //Assert
            Assert.Equal(0, city.Id);
        }

        /// <summary>
        /// Prueba la inserción de una ciudad
        /// </summary>
        [Fact]
        public void InsertTest()
        {
            //Arrange
            City city = new() { Country = new() { Id = 1 }, Code = "BUC", Name = "Bucaramanga" };

            //Act
            city = api.Insert(city);

            //Assert
            Assert.NotEqual(0, city.Id);
        }

        /// <summary>
        /// Prueba la inserción de una ciudad con nombre duplicado
        /// </summary>
        [Fact]
        public void InsertDuplicateTest()
        {
            //Arrange
            City city = new() { Country = new() { Id = 1 }, Code = "BOG", Name = "Prueba 1" };

            //Act
            city = api.Insert(city);

            //Assert
            Assert.Equal(0, city.Id);
        }

        /// <summary>
        /// Prueba la actualización de una ciudad
        /// </summary>
        [Fact]
        public void UpdateTest()
        {
            //Arrange
            City city = new() { Id = 2, Country = new() { Id = 1 }, Code = "BAQ", Name = "Barranquilla" };

            //Act
            _ = api.Update(city);
            City city2 = api.Read(2);

            //Assert
            Assert.NotEqual("MED", city2.Code);
        }

        /// <summary>
        /// Prueba la eliminación de una ciudad
        /// </summary>
        [Fact]
        public void DeleteTest()
        {
            //Act
            _ = api.Delete(3);
            City city = api.Read(3);

            //Assert
            Assert.Equal(0, city.Id);
        }
        #endregion
    }
}

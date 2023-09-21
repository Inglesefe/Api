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
                new Parameter() { Id = 1, Name = "Parámetro 1", Value = "Valor 1" },
                new Parameter() { Id = 2, Name = "Parámetro 2", Value = "Valor 2" },
                new Parameter() { Id = 3, Name = "Parámetro 3", Value = "Valor 3" }
            };

            mockBusiness.Setup(p => p.List("idparameter = 1", It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>()))
                .Returns(new ListResult<Parameter>(parameters.Where(y => y.Id == 1).ToList(), 1));
            mockBusiness.Setup(p => p.List("idparametro = 1", It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>()))
                .Throws<PersistentException>();

            mockBusiness.Setup(p => p.Read(It.IsAny<Parameter>()))
                .Returns((Parameter parameter) => parameters.Find(x => x.Id == parameter.Id) ?? new Parameter());

            mockBusiness.Setup(p => p.Insert(It.IsAny<Parameter>(), It.IsAny<User>()))
                .Returns((Parameter parameter, User user) =>
                {
                    if (parameters.Exists(x => x.Name == parameter.Name))
                    {
                        throw new PersistentException();
                    }
                    else
                    {
                        parameter.Id = parameters.Count + 1;
                        parameters.Add(parameter);
                        return parameter;
                    }
                });

            mockBusiness.Setup(p => p.Update(It.IsAny<Parameter>(), It.IsAny<User>()))
                .Returns((Parameter parameter, User user) =>
                {
                    parameters.Where(x => x.Id == parameter.Id).ToList().ForEach(x => x.Name = parameter.Name);
                    return parameter;
                });

            mockBusiness.Setup(p => p.Delete(It.IsAny<Parameter>(), It.IsAny<User>()))
                .Returns((Parameter parameter, User user) =>
                {
                    parameters = parameters.Where(x => x.Id != parameter.Id).ToList();
                    return parameter;
                });

            api = new ParameterController(configuration, mockBusiness.Object, mockLog.Object, mockTemplate.Object, mockParameter.Object)
            {
                ControllerContext = controllerContext
            };
        }
        #endregion

        #region Methods
        /// <summary>
        /// Prueba la consulta de un listado de parámetros con filtros, ordenamientos y límite
        /// </summary>
        [Fact]
        public void ListTest()
        {
            //Act
            ListResult<Parameter> list = api.List("idparameter = 1", "name", 1, 0);

            //Assert
            Assert.NotEmpty(list.List);
            Assert.True(list.Total > 0);
        }

        /// <summary>
        /// Prueba la consulta de un listado de parámetros con filtros, ordenamientos y límite y con errores
        /// </summary>
        [Fact]
        public void ListWithErrorTest()
        {
            //Act
            ListResult<Parameter> list = api.List("idparametro = 1", "name", 1, 0);

            //Assert
            Assert.Equal(0, list.Total);
        }

        /// <summary>
        /// Prueba la consulta de un parámetro dada su identificador
        /// </summary>
        [Fact]
        public void ReadTest()
        {
            //Act
            Parameter country = api.Read(1);

            //Assert
            Assert.Equal("Parámetro 1", country.Name);
        }

        /// <summary>
        /// Prueba la consulta de un parámetro que no existe dado su identificador
        /// </summary>
        [Fact]
        public void ReadNotFoundTest()
        {
            //Act
            Parameter country = api.Read(10);

            //Assert
            Assert.Equal(0, country.Id);
        }

        /// <summary>
        /// Prueba la inserción de un parámetro
        /// </summary>
        [Fact]
        public void InsertTest()
        {
            //Arrange
            Parameter country = new() { Name = "Parámetro 4", Value = "Valor 4" };

            //Act
            country = api.Insert(country);

            //Assert
            Assert.NotEqual(0, country.Id);
        }

        /// <summary>
        /// Prueba la actualización de un parámetro
        /// </summary>
        [Fact]
        public void UpdateTest()
        {
            //Arrange
            Parameter country = new() { Id = 2, Name = "Parámetro 6", Value = "Valor 6" };

            //Act
            _ = api.Update(country);
            Parameter country2 = api.Read(2);

            //Assert
            Assert.NotEqual("Parámetro 2", country2.Name);
        }

        /// <summary>
        /// Prueba la eliminación de un parámetro
        /// </summary>
        [Fact]
        public void ParameterDeleteTest()
        {
            //Act
            _ = api.Delete(3);
            Parameter country = api.Read(3);

            //Assert
            Assert.Equal(0, country.Id);
        }
        #endregion
    }
}

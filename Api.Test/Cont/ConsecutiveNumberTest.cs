using Api.Controllers.Cont;
using Business;
using Dal.Dto;
using Dal.Exceptions;
using Entities.Auth;
using Entities.Cont;
using Moq;
using System.Data;

namespace Api.Test.Cont
{
    /// <summary>
    /// Realiza las pruebas sobre la api de números de consecutivos
    /// </summary>
    [Collection("Test")]
    public class ConsecutiveNumberTest : TestBase<ConsecutiveNumber>
    {
        #region Constructors
        /// <summary>
        /// Inicializa la configuración de la prueba
        /// </summary>
        public ConsecutiveNumberTest() : base()
        {
            //Arrange
            Mock<IBusiness<ConsecutiveNumber>> mockBusiness = new();

            List<ConsecutiveNumber> numbers = new()
            {
                new ConsecutiveNumber() { Id = 1, ConsecutiveType = new(){ Id = 1 }, City = new(){ Id = 1 }, Number = "100" },
                new ConsecutiveNumber() { Id = 2, ConsecutiveType = new(){ Id = 1 }, City = new(){ Id = 1 }, Number = "200" },
                new ConsecutiveNumber() { Id = 3, ConsecutiveType = new(){ Id = 1 }, City = new(){ Id = 1 }, Number = "300" }
            };

            mockBusiness.Setup(p => p.List("idconsecutivenumber = 1", It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>()))
                .Returns(new ListResult<ConsecutiveNumber>(numbers.Where(y => y.Id == 1).ToList(), 1));
            mockBusiness.Setup(p => p.List("idnumeroconsecutivo = 1", It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>()))
                .Throws<PersistentException>();

            mockBusiness.Setup(p => p.Read(It.IsAny<ConsecutiveNumber>()))
                .Returns((ConsecutiveNumber number) => numbers.Find(x => x.Id == number.Id) ?? new ConsecutiveNumber());

            mockBusiness.Setup(p => p.Insert(It.IsAny<ConsecutiveNumber>(), It.IsAny<User>()))
                .Returns((ConsecutiveNumber number, User user) =>
                {
                    number.Id = numbers.Count + 1;
                    numbers.Add(number);
                    return number;
                });

            mockBusiness.Setup(p => p.Update(It.IsAny<ConsecutiveNumber>(), It.IsAny<User>()))
                .Returns((ConsecutiveNumber number, User user) =>
                {
                    numbers.Where(x => x.Id == number.Id).ToList().ForEach(x =>
                    {
                        x.ConsecutiveType = number.ConsecutiveType;
                        x.City = number.City;
                        x.Number = number.Number;
                    });
                    return number;
                });

            mockBusiness.Setup(p => p.Delete(It.IsAny<ConsecutiveNumber>(), It.IsAny<User>()))
                .Returns((ConsecutiveNumber number, User user) =>
                {
                    numbers = numbers.Where(x => x.Id != number.Id).ToList();
                    return number;
                });

            api = new ConsecutiveNumberController(configuration, mockBusiness.Object, mockLog.Object, mockTemplate.Object, mockParameter.Object)
            {
                ControllerContext = controllerContext
            };
        }
        #endregion

        #region Methods
        /// <summary>
        /// Prueba la consulta de un listado de números de consecutivo con filtros, ordenamientos y límite
        /// </summary>
        [Fact]
        public void ListTest()
        {
            //Act
            ListResult<ConsecutiveNumber> list = api.List("idconsecutivenumber = 1", "value", 1, 0);

            //Assert
            Assert.NotEmpty(list.List);
            Assert.True(list.Total > 0);
        }

        /// <summary>
        /// Prueba la consulta de un listado de números de consecutivo con filtros, ordenamientos y límite y con errores
        /// </summary>
        [Fact]
        public void ListWithErrorTest()
        {
            //Act
            ListResult<ConsecutiveNumber> list = api.List("idnumeroconsecutivo = 1", "value", 1, 0);

            //Assert
            Assert.Equal(0, list.Total);
        }

        /// <summary>
        /// Prueba la consulta de un número de consecutivo dada su identificador
        /// </summary>
        [Fact]
        public void ReadTest()
        {
            //Act
            ConsecutiveNumber number = api.Read(1);

            //Assert
            Assert.Equal("100", number.Number);
        }

        /// <summary>
        /// Prueba la consulta de un número de consecutivo que no existe dado su identificador
        /// </summary>
        [Fact]
        public void ReadNotFoundTest()
        {
            //Act
            ConsecutiveNumber number = api.Read(10);

            //Assert
            Assert.Equal(0, number.Id);
        }

        /// <summary>
        /// Prueba la inserción de un número de consecutivo
        /// </summary>
        [Fact]
        public void InsertTest()
        {
            //Arrange
            ConsecutiveNumber number = new() { ConsecutiveType = new() { Id = 1 }, City = new() { Id = 1 }, Number = "400" };

            //Act
            number = api.Insert(number);

            //Assert
            Assert.NotEqual(0, number.Id);
        }

        /// <summary>
        /// Prueba la actualización de un número de consecutivo
        /// </summary>
        [Fact]
        public void UpdateTest()
        {
            //Arrange
            ConsecutiveNumber number = new() { Id = 2, ConsecutiveType = new() { Id = 1 }, City = new() { Id = 1 }, Number = "500" };

            //Act
            _ = api.Update(number);
            ConsecutiveNumber number2 = api.Read(2);

            //Assert
            Assert.NotEqual("200", number2.Number);
        }

        /// <summary>
        /// Prueba la eliminación de un número de consecutivo
        /// </summary>
        [Fact]
        public void DeleteTest()
        {
            //Act
            _ = api.Delete(3);
            ConsecutiveNumber number = api.Read(3);

            //Assert
            Assert.Equal(0, number.Id);
        }
        #endregion
    }
}

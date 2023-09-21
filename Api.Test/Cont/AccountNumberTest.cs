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
    /// Realiza las pruebas sobre la api de números de cuenta
    /// </summary>
    [Collection("Test")]
    public class AccountNumberTest : TestBase<AccountNumber>
    {
        #region Constructors
        /// <summary>
        /// Inicializa la configuración de la prueba
        /// </summary>
        public AccountNumberTest() : base()
        {
            //Arrange
            Mock<IBusiness<AccountNumber>> mockBusiness = new();

            List<AccountNumber> numbers = new()
            {
                new AccountNumber() { Id = 1, AccountType = new(){ Id = 1 }, City = new(){ Id = 1 }, Number = "123456789" },
                new AccountNumber() { Id = 2, AccountType = new(){ Id = 1 }, City = new(){ Id = 1 }, Number = "987654321" },
                new AccountNumber() { Id = 3, AccountType = new(){ Id = 1 }, City = new(){ Id = 1 }, Number = "147258369" }
            };

            mockBusiness.Setup(p => p.List("idaccountnumber = 1", It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>()))
                .Returns(new ListResult<AccountNumber>(numbers.Where(y => y.Id == 1).ToList(), 1));
            mockBusiness.Setup(p => p.List("idnumerocuenta = 1", It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>()))
                .Throws<PersistentException>();

            mockBusiness.Setup(p => p.Read(It.IsAny<AccountNumber>()))
                .Returns((AccountNumber number) => numbers.Find(x => x.Id == number.Id) ?? new AccountNumber());

            mockBusiness.Setup(p => p.Insert(It.IsAny<AccountNumber>(), It.IsAny<User>()))
                .Returns((AccountNumber number, User user) =>
                {
                    number.Id = numbers.Count + 1;
                    numbers.Add(number);
                    return number;
                });

            mockBusiness.Setup(p => p.Update(It.IsAny<AccountNumber>(), It.IsAny<User>()))
                .Returns((AccountNumber number, User user) =>
                {
                    numbers.Where(x => x.Id == number.Id).ToList().ForEach(x =>
                    {
                        x.AccountType = number.AccountType;
                        x.City = number.City;
                        x.Number = number.Number;
                    });
                    return number;
                });

            mockBusiness.Setup(p => p.Delete(It.IsAny<AccountNumber>(), It.IsAny<User>()))
                .Returns((AccountNumber number, User user) =>
                {
                    numbers = numbers.Where(x => x.Id != number.Id).ToList();
                    return number;
                });

            api = new AccountNumberController(configuration, mockBusiness.Object, mockLog.Object, mockTemplate.Object, mockParameter.Object)
            {
                ControllerContext = controllerContext
            };
        }
        #endregion

        #region Methods
        /// <summary>
        /// Prueba la consulta de un listado de números de cuenta con filtros, ordenamientos y límite
        /// </summary>
        [Fact]
        public void ListTest()
        {
            //Act
            ListResult<AccountNumber> list = api.List("idaccountnumber = 1", "value", 1, 0);

            //Assert
            Assert.NotEmpty(list.List);
            Assert.True(list.Total > 0);
        }

        /// <summary>
        /// Prueba la consulta de un listado de números de cuenta con filtros, ordenamientos y límite y con errores
        /// </summary>
        [Fact]
        public void ListWithErrorTest()
        {
            //Act
            ListResult<AccountNumber> list = api.List("idnumerocuenta = 1", "value", 1, 0);

            //Assert
            Assert.Equal(0, list.Total);
        }

        /// <summary>
        /// Prueba la consulta de un número de cuenta dada su identificador
        /// </summary>
        [Fact]
        public void ReadTest()
        {
            //Act
            AccountNumber number = api.Read(1);

            //Assert
            Assert.Equal("123456789", number.Number);
        }

        /// <summary>
        /// Prueba la consulta de un número de cuenta que no existe dado su identificador
        /// </summary>
        [Fact]
        public void ReadNotFoundTest()
        {
            //Act
            AccountNumber number = api.Read(10);

            //Assert
            Assert.Equal(0, number.Id);
        }

        /// <summary>
        /// Prueba la inserción de un número de cuenta
        /// </summary>
        [Fact]
        public void InsertTest()
        {
            //Arrange
            AccountNumber number = new() { AccountType = new() { Id = 1 }, City = new() { Id = 1 }, Number = "963258741" };

            //Act
            number = api.Insert(number);

            //Assert
            Assert.NotEqual(0, number.Id);
        }

        /// <summary>
        /// Prueba la actualización de un número de cuenta
        /// </summary>
        [Fact]
        public void UpdateTest()
        {
            //Arrange
            AccountNumber number = new() { Id = 2, AccountType = new() { Id = 1 }, City = new() { Id = 1 }, Number = "741258963" };

            //Act
            _ = api.Update(number);
            AccountNumber number2 = api.Read(2);

            //Assert
            Assert.NotEqual("987654321", number2.Number);
        }

        /// <summary>
        /// Prueba la eliminación de un número de cuenta
        /// </summary>
        [Fact]
        public void DeleteTest()
        {
            //Act
            _ = api.Delete(3);
            AccountNumber number = api.Read(3);

            //Assert
            Assert.Equal(0, number.Id);
        }
        #endregion
    }
}

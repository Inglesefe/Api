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
    /// Realiza las pruebas sobre la api de tipos de cuenta
    /// </summary>
    [Collection("Test")]
    public class AccountTypeTest : TestBase<AccountType>
    {
        #region Constructors
        /// <summary>
        /// Inicializa la configuración de la prueba
        /// </summary>
        public AccountTypeTest() : base()
        {
            //Arrange
            Mock<IBusiness<AccountType>> mockBusiness = new();

            List<AccountType> types = new()
            {
                new AccountType() { Id = 1, Name = "Caja" },
                new AccountType() { Id = 2, Name = "Bancos" },
                new AccountType() { Id = 3, Name = "Otra" }
            };

            mockBusiness.Setup(p => p.List("idaccounttype = 1", It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>()))
                .Returns(new ListResult<AccountType>(types.Where(y => y.Id == 1).ToList(), 1));
            mockBusiness.Setup(p => p.List("idtipocuenta = 1", It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>()))
                .Throws<PersistentException>();
            mockBusiness.Setup(p => p.Read(It.IsAny<AccountType>()))
                .Returns((AccountType type) => types.Find(x => x.Id == type.Id) ?? new AccountType());
            mockBusiness.Setup(p => p.Insert(It.IsAny<AccountType>(), It.IsAny<User>()))
                .Returns((AccountType type, User user) =>
                {
                    type.Id = types.Count + 1;
                    types.Add(type);
                    return type;
                });
            mockBusiness.Setup(p => p.Update(It.IsAny<AccountType>(), It.IsAny<User>()))
                .Returns((AccountType type, User user) =>
                {
                    types.Where(x => x.Id == type.Id).ToList().ForEach(x =>
                    {
                        x.Name = type.Name;
                    });
                    return type;
                });
            mockBusiness.Setup(p => p.Delete(It.IsAny<AccountType>(), It.IsAny<User>()))
                .Returns((AccountType type, User user) =>
                {
                    types = types.Where(x => x.Id != type.Id).ToList();
                    return type;
                });

            api = new AccountTypeController(configuration, mockBusiness.Object, mockLog.Object, mockTemplate.Object, mockParameter.Object)
            {
                ControllerContext = controllerContext
            };
        }
        #endregion

        #region Methods
        /// <summary>
        /// Prueba la consulta de un listado de tipos de cuenta con filtros, ordenamientos y límite
        /// </summary>
        [Fact]
        public void ListTest()
        {
            //Act
            ListResult<AccountType> list = api.List("idaccounttype = 1", "value", 1, 0);

            //Assert
            Assert.NotEmpty(list.List);
            Assert.True(list.Total > 0);
        }

        /// <summary>
        /// Prueba la consulta de un listado de tipos de cuenta con filtros, ordenamientos y límite y con errores
        /// </summary>
        [Fact]
        public void ListWithErrorTest()
        {
            //Act
            ListResult<AccountType> list = api.List("idtipocuenta = 1", "value", 1, 0);

            //Assert
            Assert.Equal(0, list.Total);
        }

        /// <summary>
        /// Prueba la consulta de un tipo de cuenta dada su identificador
        /// </summary>
        [Fact]
        public void ReadTest()
        {
            //Act
            AccountType type = api.Read(1);

            //Assert
            Assert.Equal("Caja", type.Name);
        }

        /// <summary>
        /// Prueba la consulta de un tipo de cuenta que no existe dado su identificador
        /// </summary>
        [Fact]
        public void ReadNotFoundTest()
        {
            //Act
            AccountType type = api.Read(10);

            //Assert
            Assert.Equal(0, type.Id);
        }

        /// <summary>
        /// Prueba la inserción de un tipo de cuenta
        /// </summary>
        [Fact]
        public void InsertTest()
        {
            //Arrange
            AccountType type = new() { Name = "Nueva" };

            //Act
            type = api.Insert(type);

            //Assert
            Assert.NotEqual(0, type.Id);
        }

        /// <summary>
        /// Prueba la actualización de un tipo de cuenta
        /// </summary>
        [Fact]
        public void UpdateTest()
        {
            //Arrange
            AccountType type = new() { Id = 2, Name = "Actualizado" };

            //Act
            _ = api.Update(type);
            AccountType type2 = api.Read(2);

            //Assert
            Assert.NotEqual("Bancos", type2.Name);
        }

        /// <summary>
        /// Prueba la eliminación de un tipo de cuenta
        /// </summary>
        [Fact]
        public void DeleteTest()
        {
            //Act
            _ = api.Delete(3);
            AccountType type = api.Read(3);

            //Assert
            Assert.Equal(0, type.Id);
        }
        #endregion
    }
}

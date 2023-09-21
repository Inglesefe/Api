using Api.Controllers.Admon;
using Business;
using Dal.Dto;
using Dal.Exceptions;
using Entities.Admon;
using Entities.Auth;
using Moq;
using System.Data;

namespace Api.Test.Admon
{
    /// <summary>
    /// Realiza las pruebas sobre la api de ejecutivos de cuenta
    /// </summary>
    [Collection("Test")]
    public class AccountExecutiveTest : TestBase<AccountExecutive>
    {
        #region Constructors
        /// <summary>
        /// Inicializa la configuración de la prueba
        /// </summary>
        public AccountExecutiveTest() : base()
        {
            //Arrange
            Mock<IBusiness<AccountExecutive>> mockBusiness = new();

            List<AccountExecutive> executives = new()
            {
                new AccountExecutive() { Id = 1, Name = "Leandro Baena Torres", IdentificationType = new(){ Id = 1 }, Identification = "123456789" },
                new AccountExecutive() { Id = 2, Name = "David Santiago Baena Barreto", IdentificationType = new(){ Id = 1 }, Identification = "987654321" },
                new AccountExecutive() { Id = 3, Name = "Karol Ximena Baena Barreto", IdentificationType = new(){ Id = 1 }, Identification = "147852369" },
                new AccountExecutive() { Id = 4, Name = "Luz Marina Torres", IdentificationType = new(){ Id = 1 }, Identification = "852963741" }
            };

            mockBusiness.Setup(p => p.List("idaccountexecutive = 1", It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>()))
                .Returns(new ListResult<AccountExecutive>(executives.Where(y => y.Id == 1).ToList(), 1));
            mockBusiness.Setup(p => p.List("idejecutivocuenta = 1", It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>()))
                .Throws<PersistentException>();

            mockBusiness.Setup(p => p.Read(It.IsAny<AccountExecutive>()))
                .Returns((AccountExecutive executive) => executives.Find(x => x.Id == executive.Id) ?? new AccountExecutive());

            mockBusiness.Setup(p => p.Insert(It.IsAny<AccountExecutive>(), It.IsAny<User>()))
                .Returns((AccountExecutive executive, User user) =>
                {
                    if (executives.Exists(x => x.IdentificationType.Id == executive.IdentificationType.Id && x.Identification == executive.Identification))
                    {
                        throw new PersistentException();
                    }
                    else
                    {
                        executive.Id = executives.Count + 1;
                        executives.Add(executive);
                        return executive;
                    }
                });

            mockBusiness.Setup(p => p.Update(It.IsAny<AccountExecutive>(), It.IsAny<User>()))
                .Returns((AccountExecutive executive, User user) =>
                {
                    executives.Where(x => x.Id == executive.Id).ToList().ForEach(x =>
                    {
                        x.Name = executive.Name;
                        x.IdentificationType = executive.IdentificationType;
                        x.Identification = executive.Identification;
                    });
                    return executive;
                });

            mockBusiness.Setup(p => p.Delete(It.IsAny<AccountExecutive>(), It.IsAny<User>()))
                .Returns((AccountExecutive executive, User user) =>
                {
                    executives = executives.Where(x => x.Id != executive.Id).ToList();
                    return executive;
                });

            api = new AccountExecutiveController(configuration, mockBusiness.Object, mockLog.Object, mockTemplate.Object, mockParameter.Object)
            {
                ControllerContext = controllerContext
            };
        }
        #endregion

        #region Methods
        /// <summary>
        /// Prueba la consulta de un listado de ejecutivos de cuenta con filtros, ordenamientos y límite
        /// </summary>
        [Fact]
        public void ListTest()
        {
            //Act
            ListResult<AccountExecutive> list = api.List("idaccountexecutive = 1", "name", 1, 0);

            //Assert
            Assert.NotEmpty(list.List);
            Assert.True(list.Total > 0);
        }

        /// <summary>
        /// Prueba la consulta de un listado de ejecutivos de cuenta con filtros, ordenamientos y límite y con errores
        /// </summary>
        [Fact]
        public void ListWithErrorTest()
        {
            //Act
            ListResult<AccountExecutive> list = api.List("idejecutivocuenta = 1", "name", 1, 0);

            //Assert
            Assert.Equal(0, list.Total);
        }

        /// <summary>
        /// Prueba la consulta de un ejecutivo de cuenta dado su identificador
        /// </summary>
        [Fact]
        public void ReadTest()
        {
            //Act
            AccountExecutive executive = api.Read(1);

            //Assert
            Assert.Equal("Leandro Baena Torres", executive.Name);
        }

        /// <summary>
        /// Prueba la consulta de un ejecutivo de cuenta que no existe dado su identificador
        /// </summary>
        [Fact]
        public void ReadNotFoundTest()
        {
            //Act
            AccountExecutive executive = api.Read(10);

            //Assert
            Assert.Equal(0, executive.Id);
        }

        /// <summary>
        /// Prueba la inserción de un ejecutivo de cuenta
        /// </summary>
        [Fact]
        public void InsertTest()
        {
            //Arrange
            AccountExecutive executive = new() { Name = "Patricia Torres", IdentificationType = new() { Id = 1 }, Identification = "753146289" };

            //Act
            executive = api.Insert(executive);

            //Assert
            Assert.NotEqual(0, executive.Id);
        }

        /// <summary>
        /// Prueba la inserción de un ejecutivo de cuenta con nombre duplicado
        /// </summary>
        [Fact]
        public void InsertDuplicateTest()
        {
            //Arrange
            AccountExecutive executive = new() { Name = "Prueba errónea", IdentificationType = new() { Id = 1 }, Identification = "123456789" };

            //Act
            executive = api.Insert(executive);

            //Assert
            Assert.Equal(0, executive.Id);
        }

        /// <summary>
        /// Prueba la actualización de un ejecutivo de cuenta
        /// </summary>
        [Fact]
        public void UpdateTest()
        {
            //Arrange
            AccountExecutive executive = new() { Id = 2, Name = "Prueba actualizar", IdentificationType = new() { Id = 1 }, Identification = "321456987" };

            //Act
            _ = api.Update(executive);
            AccountExecutive executive2 = api.Read(2);

            //Assert
            Assert.NotEqual("David Santiago Baena Barreto", executive2.Name);
        }

        /// <summary>
        /// Prueba la eliminación de un ejecutivo de cuenta
        /// </summary>
        [Fact]
        public void DeleteTest()
        {
            //Act
            _ = api.Delete(3);
            AccountExecutive executive = api.Read(3);

            //Assert
            Assert.Equal(0, executive.Id);
        }
        #endregion
    }
}

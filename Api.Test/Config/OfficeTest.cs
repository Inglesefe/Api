using Api.Controllers.Config;
using Business.Config;
using Business.Exceptions;
using Dal.Dto;
using Dal.Exceptions;
using Entities.Admon;
using Entities.Auth;
using Entities.Config;
using Moq;
using System.Data;

namespace Api.Test.Config
{
    /// <summary>
    /// Realiza las pruebas sobre la api de oficinas
    /// </summary>
    [Collection("Test")]
    public class OfficeTest : TestBase<Office>
    {
        #region Constructors
        /// <summary>
        /// Inicializa la configuración de la prueba
        /// </summary>
        public OfficeTest() : base()
        {
            //Arrange
            Mock<IBusinessOffice> mockBusiness = new();

            List<Office> offices = new()
            {
                new Office() { Id = 1, Name = "Castellana", Address = "Cl 95" },
                new Office() { Id = 2, Name = "Kennedy", Address = "Cl 56 sur" },
                new Office() { Id = 3, Name = "Venecia", Address = "Puente" }
            };
            List<AccountExecutive> executives = new()
            {
                new AccountExecutive() { Id = 1, Name = "Leandro Baena Torres", IdentificationType = new(){ Id = 1 }, Identification = "123456789" },
                new AccountExecutive() { Id = 2, Name = "David Santiago Baena Barreto", IdentificationType = new(){ Id = 1 }, Identification = "987654321" },
                new AccountExecutive() { Id = 3, Name = "Karol Ximena Baena Barreto", IdentificationType = new(){ Id = 1 }, Identification = "147852369" }
            };
            List<Tuple<Office, AccountExecutive>> executives_offices = new()
            {
                new Tuple<Office, AccountExecutive>(offices[0], executives[0]),
                new Tuple<Office, AccountExecutive>(offices[0], executives[1]),
                new Tuple<Office, AccountExecutive>(offices[1], executives[0]),
                new Tuple<Office, AccountExecutive>(offices[1], executives[1])
            };
            mockBusiness.Setup(p => p.Read(It.IsAny<Office>()))
                .Returns((Office office) => offices.Find(x => x.Id == office.Id) ?? new Office());
            mockBusiness.Setup(p => p.ListAccountExecutives("", It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<Office>()))
                .Returns(new ListResult<AccountExecutive>(executives_offices.Where(x => x.Item1.Id == 1).Select(x => x.Item2).ToList(), 1));
            mockBusiness.Setup(p => p.ListAccountExecutives("idoffice = 1", It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<Office>()))
                .Returns(new ListResult<AccountExecutive>(new List<AccountExecutive>(), 0));
            mockBusiness.Setup(p => p.ListAccountExecutives("idaccountexecutive = 2", It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<Office>()))
                .Returns(new ListResult<AccountExecutive>(new List<AccountExecutive>(), 0));
            mockBusiness.Setup(p => p.ListAccountExecutives("idejecutivocuenta = 1", It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<Office>()))
                .Throws<PersistentException>();
            mockBusiness.Setup(p => p.ListAccountExecutives("error", It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<Office>()))
                .Throws<BusinessException>();
            mockBusiness.Setup(p => p.ListAccountExecutives("error1", It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<Office>()))
                .Throws<Exception>();
            mockBusiness.Setup(p => p.ListNotAccountExecutives(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<Office>()))
                .Returns((string filters, string orders, int limit, int offset, Office office) =>
                {
                    if (office.Id == -1)
                    {
                        throw new BusinessException();
                    }
                    if (office.Id == -2)
                    {
                        throw new PersistentException();
                    }
                    if (office.Id == -3)
                    {
                        throw new Exception();
                    }
                    List<AccountExecutive> result = executives.Where(x => !executives_offices.Exists(y => y.Item1.Id == office.Id && y.Item2.Id == x.Id)).ToList();
                    return new ListResult<AccountExecutive>(result, result.Count);
                });
            mockBusiness.Setup(p => p.InsertAccountExecutive(It.IsAny<AccountExecutive>(), It.IsAny<Office>(), It.IsAny<User>())).
                Returns((AccountExecutive executive, Office office, User user) =>
                {
                    if (office.Id == -1)
                    {
                        throw new BusinessException();
                    }
                    if (office.Id == -3)
                    {
                        throw new Exception();
                    }
                    if (executives_offices.Exists(x => x.Item1.Id == office.Id && x.Item2.Id == executive.Id))
                    {
                        throw new PersistentException();
                    }
                    else
                    {
                        executives_offices.Add(new Tuple<Office, AccountExecutive>(office, executive));
                        return executive;
                    }
                });
            mockBusiness.Setup(p => p.DeleteAccountExecutive(It.IsAny<AccountExecutive>(), It.IsAny<Office>(), It.IsAny<User>())).
                Returns((AccountExecutive executive, Office office, User user) =>
                {
                    if (office.Id == -1)
                    {
                        throw new BusinessException();
                    }
                    if (office.Id == -2)
                    {
                        throw new PersistentException();
                    }
                    if (office.Id == -3)
                    {
                        throw new Exception();
                    }
                    return executive;
                });
            api = new OfficeController(configuration, mockBusiness.Object, mockLog.Object, mockTemplate.Object, mockParameter.Object)
            {
                ControllerContext = controllerContext
            };
        }
        #endregion

        #region Methods
        /// <summary>
        /// Prueba la consulta de una oficina dada su identificador
        /// </summary>
        [Fact]
        public void ReadTest()
        {
            //Act
            Office office = api != null ? api.Read(1) : new();

            //Assert
            Assert.Equal("Castellana", office.Name);
        }

        /// <summary>
        /// Prueba la consulta de un listado de ejecutivos de cuenta de una oficina con filtros, ordenamientos y límite
        /// </summary>
        [Fact]
        public void ListAccountExecutivesTest()
        {
            //Act
            ListResult<AccountExecutive> list = api != null ? ((OfficeController)api).ListAccountExecutives("", "", 10, 0, 1) : new ListResult<AccountExecutive>(new List<AccountExecutive>(), 0);

            //Assert
            Assert.NotEmpty(list.List);
            Assert.True(list.Total > 0);
        }

        /// <summary>
        /// Prueba la consulta de un listado de ejecutivos de cuenta de una oficina con filtros, ordenamientos y límite y con errores
        /// </summary>
        [Fact]
        public void ListAccountExecutivesWithErrorTest()
        {
            //Act
            ListResult<AccountExecutive> list = api != null ? ((OfficeController)api).ListAccountExecutives("idejecutivocuenta = 1", "name", 10, 0, 1) : new ListResult<AccountExecutive>(new List<AccountExecutive>(), 0);

            //Assert
            Assert.Equal(0, list.Total);
        }

        /// <summary>
        /// Prueba la consulta de un listado de ejecutivos de cuenta de una oficina con filtros, ordenamientos y límite y con errores de negocio
        /// </summary>
        [Fact]
        public void ListAccountExecutivesWithError2Test()
        {
            //Act
            ListResult<AccountExecutive> list = api != null ? ((OfficeController)api).ListAccountExecutives("error", "name", 10, 0, 1) : new ListResult<AccountExecutive>(new List<AccountExecutive>(), 0);

            //Assert
            Assert.Equal(0, list.Total);
        }

        /// <summary>
        /// Prueba la consulta de un listado de ejecutivos de cuenta de una oficina con filtros, ordenamientos y límite y con errores generales
        /// </summary>
        [Fact]
        public void ListAccountExecutivesWithError3Test()
        {
            //Act
            ListResult<AccountExecutive> list = api != null ? ((OfficeController)api).ListAccountExecutives("error1", "name", 10, 0, 1) : new ListResult<AccountExecutive>(new List<AccountExecutive>(), 0);

            //Assert
            Assert.Equal(0, list.Total);
        }

        /// <summary>
        /// Prueba la consulta de un listado de ejecutivos de cuenta no asignados a una oficina con filtros, ordenamientos y límite
        /// </summary>
        [Fact]
        public void ListNotAccountExecutivesTest()
        {
            //Act
            ListResult<AccountExecutive> list = api != null ? ((OfficeController)api).ListNotAccountExecutives("", "", 10, 0, 1) : new ListResult<AccountExecutive>(new List<AccountExecutive>(), 0);

            //Assert
            Assert.NotEmpty(list.List);
            Assert.True(list.Total > 0);
        }

        /// <summary>
        /// Prueba la consulta de un listado de ejecutivos de cuenta no asignados a una oficina con filtros, ordenamientos y límite con error de persistencia
        /// </summary>
        [Fact]
        public void ListNotAccountExecutivesWithErrorTest()
        {
            //Act
            ListResult<AccountExecutive> list = api != null ? ((OfficeController)api).ListNotAccountExecutives("", "", 10, 0, -2) : new ListResult<AccountExecutive>(new List<AccountExecutive>(), 0);

            //Assert
            Assert.Empty(list.List);
            Assert.Equal(0, list.Total);
        }

        /// <summary>
        /// Prueba la consulta de un listado de ejecutivos de cuenta no asignados a una oficina con filtros, ordenamientos y límite con error de negocio
        /// </summary>
        [Fact]
        public void ListNotAccountExecutivesWithError2Test()
        {
            //Act
            ListResult<AccountExecutive> list = api != null ? ((OfficeController)api).ListNotAccountExecutives("", "", 10, 0, -1) : new ListResult<AccountExecutive>(new List<AccountExecutive>(), 0);

            //Assert
            Assert.Empty(list.List);
            Assert.Equal(0, list.Total);
        }

        /// <summary>
        /// Prueba la consulta de un listado de ejecutivos de cuenta no asignados a una oficina con filtros, ordenamientos y límite con error general
        /// </summary>
        [Fact]
        public void ListNotAccountExecutivesWithError3Test()
        {
            //Act
            ListResult<AccountExecutive> list = api != null ? ((OfficeController)api).ListNotAccountExecutives("", "", 10, 0, -3) : new ListResult<AccountExecutive>(new List<AccountExecutive>(), 0);

            //Assert
            Assert.Empty(list.List);
            Assert.Equal(0, list.Total);
        }

        /// <summary>
        /// Prueba la inserción de un ejecutivo de cuenta a una oficina
        /// </summary>
        [Fact]
        public void InsertAccountExecutiveTest()
        {
            //Act
            AccountExecutive executive = api != null ? ((OfficeController)api).InsertAccountExecutive(new() { Id = 4 }, 1) : new();

            //Assert
            Assert.NotEqual(0, executive.Id);
        }

        /// <summary>
        /// Prueba la inserción de un ejecutivo de cuenta a una oficina con error de negocio
        /// </summary>
        [Fact]
        public void InsertAccountExecutiveWithErrorTest()
        {
            //Act
            AccountExecutive executive = api != null ? ((OfficeController)api).InsertAccountExecutive(new() { Id = 4 }, -1) : new();

            //Assert
            Assert.Equal(0, executive.Id);
        }

        /// <summary>
        /// Prueba la inserción de un ejecutivo de cuenta a una oficina con error de persistencia
        /// </summary>
        [Fact]
        public void InsertAccountExecutiveWithError2Test()
        {
            //Act
            AccountExecutive executive = api != null ? ((OfficeController)api).InsertAccountExecutive(new() { Id = 1 }, 1) : new();

            //Assert
            Assert.Equal(0, executive.Id);
        }

        /// <summary>
        /// Prueba la inserción de un ejecutivo de cuenta a una oficina con error general
        /// </summary>
        [Fact]
        public void InsertAccountExecutiveWithError3Test()
        {
            //Act
            AccountExecutive executive = api != null ? ((OfficeController)api).InsertAccountExecutive(new() { Id = 4 }, -3) : new();

            //Assert
            Assert.Equal(0, executive.Id);
        }

        /// <summary>
        /// Prueba la eliminación de un ejecutivo de cuenta de una oficina
        /// </summary>
        [Fact]
        public void DeleteAccountExecutiveTest()
        {
            //Act
            _ = api != null ? ((OfficeController)api).DeleteAccountExecutive(2, 1) : new();
            ListResult<AccountExecutive> list = api != null ? ((OfficeController)api).ListAccountExecutives("idoffice = 1", "", 10, 0, 1) : new ListResult<AccountExecutive>(new List<AccountExecutive>(), 0);

            //Assert
            Assert.Equal(0, list.Total);
        }

        /// <summary>
        /// Prueba la eliminación de un ejecutivo de cuenta de una oficina con error de negocio
        /// </summary>
        [Fact]
        public void DeleteAccountExecutiveWithErrorTest()
        {
            //Act
            AccountExecutive executive = api != null ? ((OfficeController)api).DeleteAccountExecutive(2, -1) : new();

            //Assert
            Assert.Equal(0, executive.Id);
        }

        /// <summary>
        /// Prueba la eliminación de un ejecutivo de cuenta de una oficina con error de persistencia
        /// </summary>
        [Fact]
        public void DeleteAccountExecutiveWithError2Test()
        {
            //Act
            AccountExecutive executive = api != null ? ((OfficeController)api).DeleteAccountExecutive(2, -2) : new();

            //Assert
            Assert.Equal(0, executive.Id);
        }

        /// <summary>
        /// Prueba la eliminación de un ejecutivo de cuenta de una oficina con error general
        /// </summary>
        [Fact]
        public void DeleteAccountExecutiveWithError3Test()
        {
            //Act
            AccountExecutive executive = api != null ? ((OfficeController)api).DeleteAccountExecutive(2, -3) : new();

            //Assert
            Assert.Equal(0, executive.Id);
        }
        #endregion
    }
}

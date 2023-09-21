using Api.Controllers.Crm;
using Business;
using Dal.Dto;
using Dal.Exceptions;
using Entities.Auth;
using Entities.Crm;
using Moq;
using System.Data;

namespace Api.Test.Crm
{
    /// <summary>
    /// Realiza las pruebas sobre la api de titulares
    /// </summary>
    [Collection("Test")]
    public class OwnerTest : TestBase<Owner>
    {
        #region Constructors
        /// <summary>
        /// Inicializa la configuración de la prueba
        /// </summary>
        public OwnerTest() : base()
        {
            //Arrange
            Mock<IBusiness<Owner>> mockBusiness = new();

            List<Owner> owners = new()
            {
                new Owner() {
                    Id = 1,
                    Name = "Leandro Baena Torres",
                    IdentificationType = new(){ Id = 1 },
                    Identification = "123456789",
                    AddressHome = "CL 1 # 2 - 3",
                    AddressOffice = "CL 4 # 5 - 6",
                    PhoneHome = "3121234567",
                    PhoneOffice = "3127654321",
                    Email = "leandrobaena@gmail.com"
                },
                new Owner() {
                    Id = 2,
                    Name = "David Santiago Baena Barreto",
                    IdentificationType = new(){ Id = 1 },
                    Identification = "987654321",
                    AddressHome = "CL 7 # 8 - 9",
                    AddressOffice = "CL 10 # 11 - 12",
                    PhoneHome = "3151234567",
                    PhoneOffice = "3157654321",
                    Email = "dsantiagobaena@gmail.com"
                },
                new Owner() {
                    Id = 3,
                    Name = "Karol Ximena Baena Brreto",
                    IdentificationType = new(){ Id = 1 },
                    Identification = "147258369",
                    AddressHome = "CL 13 # 14 - 15",
                    AddressOffice = "CL 16 # 17 - 18",
                    PhoneHome = "3201234567",
                    PhoneOffice = "3207654321",
                    Email = "kximenabaena@gmail.com"
                }
            };
            mockBusiness.Setup(p => p.List("idowner = 1", It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>()))
                .Returns(new ListResult<Owner>(owners.Where(y => y.Id == 1).ToList(), 1));
            mockBusiness.Setup(p => p.List("idtitular = 1", It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>()))
                .Throws<PersistentException>();
            mockBusiness.Setup(p => p.Read(It.IsAny<Owner>()))
                .Returns((Owner owner) => owners.Find(x => x.Id == owner.Id) ?? new Owner());
            mockBusiness.Setup(p => p.Insert(It.IsAny<Owner>(), It.IsAny<User>()))
                .Returns((Owner owner, User user) =>
                {
                    owner.Id = owners.Count + 1;
                    owners.Add(owner);
                    return owner;
                });
            mockBusiness.Setup(p => p.Update(It.IsAny<Owner>(), It.IsAny<User>()))
                .Returns((Owner owner, User user) =>
                {
                    owners.Where(x => x.Id == owner.Id).ToList().ForEach(x =>
                    {
                        x.Name = owner.Name;
                    });
                    return owner;
                });
            mockBusiness.Setup(p => p.Delete(It.IsAny<Owner>(), It.IsAny<User>()))
                .Returns((Owner owner, User user) =>
                {
                    owners = owners.Where(x => x.Id != owner.Id).ToList();
                    return owner;
                });

            api = new OwnerController(configuration, mockBusiness.Object, mockLog.Object, mockTemplate.Object, mockParameter.Object)
            {
                ControllerContext = controllerContext
            };
        }
        #endregion

        #region Methods
        /// <summary>
        /// Prueba la consulta de un listado de titulares con filtros, ordenamientos y límite
        /// </summary>
        [Fact]
        public void ListTest()
        {
            //Act
            ListResult<Owner> list = api.List("idowner = 1", "value", 1, 0);

            //Assert
            Assert.NotEmpty(list.List);
            Assert.True(list.Total > 0);
        }

        /// <summary>
        /// Prueba la consulta de un listado de titulares con filtros, ordenamientos y límite y con errores
        /// </summary>
        [Fact]
        public void ListWithErrorTest()
        {
            //Act
            ListResult<Owner> list = api.List("idtitular = 1", "value", 1, 0);

            //Assert
            Assert.Equal(0, list.Total);
        }

        /// <summary>
        /// Prueba la consulta de un titular dada su identificador
        /// </summary>
        [Fact]
        public void ReadTest()
        {
            //Act
            Owner beneficiary = api.Read(1);

            //Assert
            Assert.Equal("Leandro Baena Torres", beneficiary.Name);
        }

        /// <summary>
        /// Prueba la consulta de un titular que no existe dado su identificador
        /// </summary>
        [Fact]
        public void ReadNotFoundTest()
        {
            //Act
            Owner beneficiary = api.Read(10);

            //Assert
            Assert.Equal(0, beneficiary.Id);
        }

        /// <summary>
        /// Prueba la inserción de un titular
        /// </summary>
        [Fact]
        public void InsertTest()
        {
            //Arrange
            Owner owner = new() { Name = "Nuevo" };

            //Act
            owner = api.Insert(owner);

            //Assert
            Assert.NotEqual(0, owner.Id);
        }

        /// <summary>
        /// Prueba la actualización de un titular
        /// </summary>
        [Fact]
        public void UpdateTest()
        {
            //Arrange
            Owner owner = new() { Id = 2, Name = "Actualizado" };

            //Act
            _ = api.Update(owner);
            Owner owner2 = api.Read(2);

            //Assert
            Assert.NotEqual("David Santiago Baena Barreto", owner2.Name);
        }

        /// <summary>
        /// Prueba la eliminación de un titular
        /// </summary>
        [Fact]
        public void DeleteTest()
        {
            //Act
            _ = api.Delete(3);
            Owner owner = api.Read(3);

            //Assert
            Assert.Equal(0, owner.Id);
        }
        #endregion
    }
}

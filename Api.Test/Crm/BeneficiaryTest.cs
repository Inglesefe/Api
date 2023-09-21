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
    /// Realiza las pruebas sobre la api de beneficiarios
    /// </summary>
    [Collection("Test")]
    public class BeneficiaryTest : TestBase<Beneficiary>
    {
        #region Constructors
        /// <summary>
        /// Inicializa la configuración de la prueba
        /// </summary>
        public BeneficiaryTest() : base()
        {
            //Arrange
            Mock<IBusiness<Beneficiary>> mockBusiness = new();

            List<Beneficiary> beneficiaries = new()
            {
                new Beneficiary() { Id = 1, Name = "Pedro Perez", IdentificationType = new(){ Id = 1 }, Identification = "111111111", Relationship = "hijo" },
                new Beneficiary() { Id = 1, Name = "Maria Martinez", IdentificationType = new(){ Id = 1 }, Identification = "222222222", Relationship = "hija" },
                new Beneficiary() { Id = 1, Name = "Hernan Hernandez", IdentificationType = new(){ Id = 1 }, Identification = "333333333", Relationship = "esposa" },
                new Beneficiary() { Id = 1, Name = "Para eliminar", IdentificationType = new(){ Id = 1 }, Identification = "1111122222", Relationship = "primo" }
            };

            mockBusiness.Setup(p => p.List("idbeneficiary = 1", It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>()))
                .Returns(new ListResult<Beneficiary>(beneficiaries.Where(y => y.Id == 1).ToList(), 1));
            mockBusiness.Setup(p => p.List("idbeneficiario = 1", It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>()))
                .Throws<PersistentException>();
            mockBusiness.Setup(p => p.Read(It.IsAny<Beneficiary>()))
                .Returns((Beneficiary beneficiary) => beneficiaries.Find(x => x.Id == beneficiary.Id) ?? new Beneficiary());
            mockBusiness.Setup(p => p.Insert(It.IsAny<Beneficiary>(), It.IsAny<User>()))
                .Returns((Beneficiary beneficiary, User user) =>
                {
                    beneficiary.Id = beneficiaries.Count + 1;
                    beneficiaries.Add(beneficiary);
                    return beneficiary;
                });
            mockBusiness.Setup(p => p.Update(It.IsAny<Beneficiary>(), It.IsAny<User>()))
                .Returns((Beneficiary beneficiary, User user) =>
                {
                    beneficiaries.Where(x => x.Id == beneficiary.Id).ToList().ForEach(x =>
                    {
                        x.Name = beneficiary.Name;
                    });
                    return beneficiary;
                });
            mockBusiness.Setup(p => p.Delete(It.IsAny<Beneficiary>(), It.IsAny<User>()))
                .Returns((Beneficiary beneficiary, User user) =>
                {
                    beneficiaries = beneficiaries.Where(x => x.Id != beneficiary.Id).ToList();
                    return beneficiary;
                });

            api = new BeneficiaryController(configuration, mockBusiness.Object, mockLog.Object, mockTemplate.Object, mockParameter.Object)
            {
                ControllerContext = controllerContext
            };
        }
        #endregion

        #region Methods
        /// <summary>
        /// Prueba la consulta de un listado de beneficiarios con filtros, ordenamientos y límite
        /// </summary>
        [Fact]
        public void ListTest()
        {
            //Act
            ListResult<Beneficiary> list = api.List("idbeneficiary = 1", "value", 1, 0);

            //Assert
            Assert.NotEmpty(list.List);
            Assert.True(list.Total > 0);
        }

        /// <summary>
        /// Prueba la consulta de un listado de beneficiarios con filtros, ordenamientos y límite y con errores
        /// </summary>
        [Fact]
        public void ListWithErrorTest()
        {
            //Act
            ListResult<Beneficiary> list = api.List("idbeneficiario = 1", "value", 1, 0);

            //Assert
            Assert.Equal(0, list.Total);
        }

        /// <summary>
        /// Prueba la consulta de un beneficiario dada su identificador
        /// </summary>
        [Fact]
        public void ReadTest()
        {
            //Act
            Beneficiary beneficiary = api.Read(1);

            //Assert
            Assert.Equal("Pedro Perez", beneficiary.Name);
        }

        /// <summary>
        /// Prueba la consulta de un beneficiario que no existe dado su identificador
        /// </summary>
        [Fact]
        public void ReadNotFoundTest()
        {
            //Act
            Beneficiary beneficiary = api.Read(10);

            //Assert
            Assert.Equal(0, beneficiary.Id);
        }

        /// <summary>
        /// Prueba la inserción de un beneficiario
        /// </summary>
        [Fact]
        public void InsertTest()
        {
            //Arrange
            Beneficiary beneficiary = new() { Name = "Nuevo" };

            //Act
            beneficiary = api.Insert(beneficiary);

            //Assert
            Assert.NotEqual(0, beneficiary.Id);
        }

        /// <summary>
        /// Prueba la actualización de un beneficiario
        /// </summary>
        [Fact]
        public void UpdateTest()
        {
            //Arrange
            Beneficiary beneficiary = new() { Id = 2, Name = "Actualizado" };

            //Act
            _ = api.Update(beneficiary);
            Beneficiary beneficiary2 = api.Read(2);

            //Assert
            Assert.NotEqual("Maria Martinez", beneficiary2.Name);
        }

        /// <summary>
        /// Prueba la eliminación de un beneficiario
        /// </summary>
        [Fact]
        public void DeleteTest()
        {
            //Act
            _ = api.Delete(3);
            Beneficiary beneficiary = api.Read(3);

            //Assert
            Assert.Equal(0, beneficiary.Id);
        }
        #endregion
    }
}

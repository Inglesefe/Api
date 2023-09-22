using Api.Controllers.Crm;
using Business;
using Entities.Crm;
using Moq;

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
                }
            };
            mockBusiness.Setup(p => p.Read(It.IsAny<Owner>()))
                .Returns((Owner owner) => owners.Find(x => x.Id == owner.Id) ?? new Owner());
            api = new OwnerController(configuration, mockBusiness.Object, mockLog.Object, mockTemplate.Object, mockParameter.Object)
            {
                ControllerContext = controllerContext
            };
        }
        #endregion

        #region Methods
        /// <summary>
        /// Prueba la consulta de un titular dada su identificador
        /// </summary>
        [Fact]
        public void ReadTest()
        {
            //Act
            Owner beneficiary = api != null ? api.Read(1) : new();

            //Assert
            Assert.Equal("Leandro Baena Torres", beneficiary.Name);
        }
        #endregion
    }
}

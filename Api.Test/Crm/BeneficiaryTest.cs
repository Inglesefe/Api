using Api.Controllers.Crm;
using Business;
using Entities.Crm;
using Moq;

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
                new Beneficiary() { Id = 1, Name = "Pedro Perez", IdentificationType = new(){ Id = 1 }, Identification = "111111111", Relationship = "hijo" }
            };
            mockBusiness.Setup(p => p.Read(It.IsAny<Beneficiary>()))
                .Returns((Beneficiary beneficiary) => beneficiaries.Find(x => x.Id == beneficiary.Id) ?? new Beneficiary());
            api = new BeneficiaryController(configuration, mockBusiness.Object, mockLog.Object, mockTemplate.Object, mockParameter.Object)
            {
                ControllerContext = controllerContext
            };
        }
        #endregion

        #region Methods
        /// <summary>
        /// Prueba la consulta de un beneficiario dada su identificador
        /// </summary>
        [Fact]
        public void ReadTest()
        {
            //Act
            Beneficiary beneficiary = api != null ? api.Read(1) : new();

            //Assert
            Assert.Equal("Pedro Perez", beneficiary.Name);
        }
        #endregion
    }
}

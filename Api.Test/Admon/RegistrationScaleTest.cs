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
    /// Realiza las pruebas sobre la api de escalas asociadas a matrículas
    /// </summary>
    [Collection("Test")]
    public class RegistrationScaleTest : TestBase<RegistrationScale>
    {
        #region Constructors
        /// <summary>
        /// Inicializa la configuración de la prueba
        /// </summary>
        public RegistrationScaleTest() : base()
        {
            //Arrange
            Mock<IBusiness<RegistrationScale>> mockBusiness = new();

            List<RegistrationScale> registrationScales = new()
            {
                new RegistrationScale() { Id = 1, Registration = new(){Id = 1}, Scale = new(){ Id = 1 }, AccountExecutive = new(){ Id = 1} },
                new RegistrationScale() { Id = 2, Registration = new(){Id = 1}, Scale = new(){ Id = 1 }, AccountExecutive = new(){ Id = 2} },
                new RegistrationScale() { Id = 4, Registration = new(){Id = 1}, Scale = new(){ Id = 2 }, AccountExecutive = new(){ Id = 1} }
            };

            mockBusiness.Setup(p => p.List("idregistrationscale = 1", It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>()))
                .Returns(new ListResult<RegistrationScale>(registrationScales.Where(y => y.Id == 1).ToList(), 1));
            mockBusiness.Setup(p => p.List("idescalamatricula = 1", It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>()))
                .Throws<PersistentException>();

            mockBusiness.Setup(p => p.Read(It.IsAny<RegistrationScale>()))
                .Returns((RegistrationScale regScale) => registrationScales.Find(x => x.Id == regScale.Id) ?? new RegistrationScale());

            mockBusiness.Setup(p => p.Insert(It.IsAny<RegistrationScale>(), It.IsAny<User>()))
                .Returns((RegistrationScale regScale, User user) =>
                {
                    regScale.Id = registrationScales.Count + 1;
                    registrationScales.Add(regScale);
                    return regScale;
                });

            mockBusiness.Setup(p => p.Update(It.IsAny<RegistrationScale>(), It.IsAny<User>()))
                .Returns((RegistrationScale regScale, User user) =>
                {
                    registrationScales.Where(x => x.Id == regScale.Id).ToList().ForEach(x =>
                    {
                        x.Registration = regScale.Registration;
                        x.Scale = regScale.Scale;
                        x.AccountExecutive = regScale.AccountExecutive;
                    });
                    return regScale;
                });

            mockBusiness.Setup(p => p.Delete(It.IsAny<RegistrationScale>(), It.IsAny<User>()))
                .Returns((RegistrationScale regScale, User user) =>
                {
                    registrationScales = registrationScales.Where(x => x.Id != regScale.Id).ToList();
                    return regScale;
                });

            api = new RegistrationScaleController(configuration, mockBusiness.Object, mockLog.Object, mockTemplate.Object, mockParameter.Object)
            {
                ControllerContext = controllerContext
            };
        }
        #endregion

        #region Methods
        /// <summary>
        /// Prueba la consulta de un listado de escalas asociadas a matrículas con filtros, ordenamientos y límite
        /// </summary>
        [Fact]
        public void ListTest()
        {
            //Act
            ListResult<RegistrationScale> list = api.List("idregistrationscale = 1", "name", 1, 0);

            //Assert
            Assert.NotEmpty(list.List);
            Assert.True(list.Total > 0);
        }

        /// <summary>
        /// Prueba la consulta de un listado de escalas asociadas a matrículas con filtros, ordenamientos y límite y con errores
        /// </summary>
        [Fact]
        public void ListWithErrorTest()
        {
            //Act
            ListResult<RegistrationScale> list = api.List("idescalamatricula = 1", "name", 1, 0);

            //Assert
            Assert.Equal(0, list.Total);
        }

        /// <summary>
        /// Prueba la consulta de una escala asociada a matrícula dado su identificador
        /// </summary>
        [Fact]
        public void ReadTest()
        {
            //Act
            RegistrationScale regScale = api.Read(1);

            //Assert
            Assert.Equal(1, regScale.Registration.Id);
            Assert.Equal(1, regScale.Scale.Id);
            Assert.Equal(1, regScale.AccountExecutive.Id);
        }

        /// <summary>
        /// Prueba la consulta de una escala asociada a matrícula que no existe dado su identificador
        /// </summary>
        [Fact]
        public void ReadNotFoundTest()
        {
            //Act
            RegistrationScale regScale = api.Read(10);

            //Assert
            Assert.Equal(0, regScale.Id);
        }

        /// <summary>
        /// Prueba la inserción de una escala asociada a matrícula
        /// </summary>
        [Fact]
        public void InsertTest()
        {
            //Arrange
            RegistrationScale regScale = new() { Registration = new() { Id = 1 }, Scale = new() { Id = 2 }, AccountExecutive = new() { Id = 2 } };

            //Act
            regScale = api.Insert(regScale);

            //Assert
            Assert.NotEqual(0, regScale.Id);
        }

        /// <summary>
        /// Prueba la actualización de una escala asociada a matrícula
        /// </summary>
        [Fact]
        public void UpdateTest()
        {
            //Arrange
            RegistrationScale regScale = new() { Id = 2, Registration = new() { Id = 1 }, Scale = new() { Id = 3 }, AccountExecutive = new() { Id = 3 } };

            //Act
            _ = api.Update(regScale);
            RegistrationScale regScale2 = api.Read(2);

            //Assert
            Assert.NotEqual(1, regScale2.Scale.Id);
            Assert.NotEqual(2, regScale2.AccountExecutive.Id);
        }

        /// <summary>
        /// Prueba la eliminación de una escala asociada a matrícula
        /// </summary>
        [Fact]
        public void DeleteTest()
        {
            //Act
            _ = api.Delete(3);
            RegistrationScale regScale = api.Read(3);

            //Assert
            Assert.Equal(0, regScale.Id);
        }
        #endregion
    }
}

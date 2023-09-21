using Api.Controllers.Config;
using Business;
using Dal.Dto;
using Dal.Exceptions;
using Entities.Auth;
using Entities.Config;
using Moq;
using System.Data;

namespace Api.Test.Config
{
    /// <summary>
    /// Realiza las pruebas sobre la api de tipos de identificación
    /// </summary>
    [Collection("Test")]
    public class IdentificationTypeTest : TestBase<IdentificationType>
    {
        #region Constructors
        /// <summary>
        /// Inicializa la configuración de la prueba
        /// </summary>
        public IdentificationTypeTest() : base()
        {
            //Arrange
            Mock<IBusiness<IdentificationType>> mockBusiness = new();

            List<IdentificationType> identificationTypes = new()
            {
                new IdentificationType() { Id = 1, Name = "Cédula ciudadanía" },
                new IdentificationType() { Id = 2, Name = "Cédula extranjería" },
                new IdentificationType() { Id = 3, Name = "Pasaporte" }
            };

            mockBusiness.Setup(p => p.List("ididentificationtype = 1", It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>()))
                .Returns(new ListResult<IdentificationType>(identificationTypes.Where(y => y.Id == 1).ToList(), 1));
            mockBusiness.Setup(p => p.List("idtipoidentificacion = 1", It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>()))
                .Throws<PersistentException>();

            mockBusiness.Setup(p => p.Read(It.IsAny<IdentificationType>()))
                .Returns((IdentificationType identificationType) => identificationTypes.Find(x => x.Id == identificationType.Id) ?? new IdentificationType());

            mockBusiness.Setup(p => p.Insert(It.IsAny<IdentificationType>(), It.IsAny<User>()))
                .Returns((IdentificationType identificationType, User user) =>
                {
                    if (identificationTypes.Exists(x => x.Name == identificationType.Name))
                    {
                        throw new PersistentException();
                    }
                    else
                    {
                        identificationType.Id = identificationTypes.Count + 1;
                        identificationTypes.Add(identificationType);
                        return identificationType;
                    }
                });

            mockBusiness.Setup(p => p.Update(It.IsAny<IdentificationType>(), It.IsAny<User>()))
                .Returns((IdentificationType identificationType, User user) =>
                {
                    identificationTypes.Where(x => x.Id == identificationType.Id).ToList().ForEach(x => x.Name = identificationType.Name);
                    return identificationType;
                });

            mockBusiness.Setup(p => p.Delete(It.IsAny<IdentificationType>(), It.IsAny<User>()))
                .Returns((IdentificationType identificationType, User user) =>
                {
                    identificationTypes = identificationTypes.Where(x => x.Id != identificationType.Id).ToList();
                    return identificationType;
                });

            api = new IdentificationTypeController(configuration, mockBusiness.Object, mockLog.Object, mockTemplate.Object, mockParameter.Object)
            {
                ControllerContext = controllerContext
            };
        }
        #endregion

        #region Methods
        /// <summary>
        /// Prueba la consulta de un listado de tipos de identificación con filtros, ordenamientos y límite
        /// </summary>
        [Fact]
        public void ListTest()
        {
            //Act
            ListResult<IdentificationType> list = api.List("ididentificationtype = 1", "name", 1, 0);

            //Assert
            Assert.NotEmpty(list.List);
            Assert.True(list.Total > 0);
        }

        /// <summary>
        /// Prueba la consulta de un listado de tipos de identificación con filtros, ordenamientos y límite y con errores
        /// </summary>
        [Fact]
        public void ListWithErrorTest()
        {
            //Act
            ListResult<IdentificationType> list = api.List("idtipoidentificacion = 1", "name", 1, 0);

            //Assert
            Assert.Equal(0, list.Total);
        }

        /// <summary>
        /// Prueba la consulta de un tipo de identificación dada su identificador
        /// </summary>
        [Fact]
        public void ReadTest()
        {
            //Act
            IdentificationType identificationType = api.Read(1);

            //Assert
            Assert.Equal("Cédula ciudadanía", identificationType.Name);
        }

        /// <summary>
        /// Prueba la consulta de un tipo de identificación que no existe dado su identificador
        /// </summary>
        [Fact]
        public void ReadNotFoundTest()
        {
            //Act
            IdentificationType identificationType = api.Read(10);

            //Assert
            Assert.Equal(0, identificationType.Id);
        }

        /// <summary>
        /// Prueba la inserción de un tipo de identificación
        /// </summary>
        [Fact]
        public void InsertTest()
        {
            //Arrange
            IdentificationType identificationType = new() { Name = "Prueba 1" };

            //Act
            identificationType = api.Insert(identificationType);

            //Assert
            Assert.NotEqual(0, identificationType.Id);
        }

        /// <summary>
        /// Prueba la actualización de un tipo de identificación
        /// </summary>
        [Fact]
        public void UpdateTest()
        {
            //Arrange
            IdentificationType identificationType = new() { Id = 2, Name = "Tarjeta de identidad" };

            //Act
            _ = api.Update(identificationType);
            IdentificationType identificationType2 = api.Read(2);

            //Assert
            Assert.NotEqual("Cédula extranjería", identificationType2.Name);
        }

        /// <summary>
        /// Prueba la eliminación de un tipo de identificación
        /// </summary>
        [Fact]
        public void DeleteTest()
        {
            //Act
            _ = api.Delete(3);
            IdentificationType identificationType = api.Read(3);

            //Assert
            Assert.Equal(0, identificationType.Id);
        }
        #endregion
    }
}

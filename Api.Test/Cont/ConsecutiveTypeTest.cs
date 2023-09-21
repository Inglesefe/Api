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
    /// Realiza las pruebas sobre la api de tipos de consecutivo
    /// </summary>
    [Collection("Test")]
    public class ConsecutiveTypeTest : TestBase<ConsecutiveType>
    {
        #region Constructors
        /// <summary>
        /// Inicializa la configuración de la prueba
        /// </summary>
        public ConsecutiveTypeTest() : base()
        {
            //Arrange
            Mock<IBusiness<ConsecutiveType>> mockBusiness = new();

            List<ConsecutiveType> types = new()
            {
                new ConsecutiveType() { Id = 1, Name = "Recibos de caja" },
                new ConsecutiveType() { Id = 2, Name = "RRegistro oficial" },
                new ConsecutiveType() { Id = 3, Name = "Otro registro" }
            };

            mockBusiness.Setup(p => p.List("idconsecutivetype = 1", It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>()))
                .Returns(new ListResult<ConsecutiveType>(types.Where(y => y.Id == 1).ToList(), 1));
            mockBusiness.Setup(p => p.List("idtipoconsecutivo = 1", It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>()))
                .Throws<PersistentException>();
            mockBusiness.Setup(p => p.Read(It.IsAny<ConsecutiveType>()))
                .Returns((ConsecutiveType type) => types.Find(x => x.Id == type.Id) ?? new ConsecutiveType());
            mockBusiness.Setup(p => p.Insert(It.IsAny<ConsecutiveType>(), It.IsAny<User>()))
                .Returns((ConsecutiveType type, User user) =>
                {
                    type.Id = types.Count + 1;
                    types.Add(type);
                    return type;
                });
            mockBusiness.Setup(p => p.Update(It.IsAny<ConsecutiveType>(), It.IsAny<User>()))
                .Returns((ConsecutiveType type, User user) =>
                {
                    types.Where(x => x.Id == type.Id).ToList().ForEach(x =>
                    {
                        x.Name = type.Name;
                    });
                    return type;
                });
            mockBusiness.Setup(p => p.Delete(It.IsAny<ConsecutiveType>(), It.IsAny<User>()))
                .Returns((ConsecutiveType type, User user) =>
                {
                    types = types.Where(x => x.Id != type.Id).ToList();
                    return type;
                });

            api = new ConsecutiveTypeController(configuration, mockBusiness.Object, mockLog.Object, mockTemplate.Object, mockParameter.Object)
            {
                ControllerContext = controllerContext
            };
        }
        #endregion

        #region Methods
        /// <summary>
        /// Prueba la consulta de un listado de tipos de consecutivo con filtros, ordenamientos y límite
        /// </summary>
        [Fact]
        public void ListTest()
        {
            //Act
            ListResult<ConsecutiveType> list = api.List("idconsecutivetype = 1", "value", 1, 0);

            //Assert
            Assert.NotEmpty(list.List);
            Assert.True(list.Total > 0);
        }

        /// <summary>
        /// Prueba la consulta de un listado de tipos de consecutivo con filtros, ordenamientos y límite y con errores
        /// </summary>
        [Fact]
        public void ListWithErrorTest()
        {
            //Act
            ListResult<ConsecutiveType> list = api.List("idtipoconsecutivo = 1", "value", 1, 0);

            //Assert
            Assert.Equal(0, list.Total);
        }

        /// <summary>
        /// Prueba la consulta de un tipo de consecutivo dada su identificador
        /// </summary>
        [Fact]
        public void ReadTest()
        {
            //Act
            ConsecutiveType type = api.Read(1);

            //Assert
            Assert.Equal("Recibos de caja", type.Name);
        }

        /// <summary>
        /// Prueba la consulta de un tipo de consecutivo que no existe dado su identificador
        /// </summary>
        [Fact]
        public void ReadNotFoundTest()
        {
            //Act
            ConsecutiveType type = api.Read(10);

            //Assert
            Assert.Equal(0, type.Id);
        }

        /// <summary>
        /// Prueba la inserción de un tipo de consecutivo
        /// </summary>
        [Fact]
        public void InsertTest()
        {
            //Arrange
            ConsecutiveType type = new() { Name = "Nueva" };

            //Act
            type = api.Insert(type);

            //Assert
            Assert.NotEqual(0, type.Id);
        }

        /// <summary>
        /// Prueba la actualización de un tipo de consecutivo
        /// </summary>
        [Fact]
        public void UpdateTest()
        {
            //Arrange
            ConsecutiveType type = new() { Id = 2, Name = "Actualizado" };

            //Act
            _ = api.Update(type);
            ConsecutiveType type2 = api.Read(2);

            //Assert
            Assert.NotEqual("Registro oficial", type2.Name);
        }

        /// <summary>
        /// Prueba la eliminación de un tipo de consecutivo
        /// </summary>
        [Fact]
        public void DeleteTest()
        {
            //Act
            _ = api.Delete(3);
            ConsecutiveType type = api.Read(3);

            //Assert
            Assert.Equal(0, type.Id);
        }
        #endregion
    }
}

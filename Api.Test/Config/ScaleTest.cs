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
    /// Realiza las pruebas sobre la api de escalas
    /// </summary>
    [Collection("Test")]
    public class ScaleTest : TestBase<Scale>
    {
        #region Constructors
        /// <summary>
        /// Inicializa la configuración de la prueba
        /// </summary>
        public ScaleTest() : base()
        {
            //Arrange
            Mock<IBusiness<Scale>> mockBusiness = new();

            List<Scale> scales = new()
            {
                new Scale() { Id = 1, Code= "C1", Name = "Comisión 1", Comission = 1000, Order = 1 },
                new Scale() { Id = 2, Code= "C2", Name = "Comisión 2", Comission = 2000, Order = 2 },
                new Scale() { Id = 3, Code= "C3", Name = "Comisión 3", Comission = 3000, Order = 3 }
            };

            mockBusiness.Setup(p => p.List("idscale = 1", It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>()))
                .Returns(new ListResult<Scale>(scales.Where(y => y.Id == 1).ToList(), 1));
            mockBusiness.Setup(p => p.List("idescala = 1", It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>()))
                .Throws<PersistentException>();

            mockBusiness.Setup(p => p.Read(It.IsAny<Scale>()))
                .Returns((Scale scale) => scales.Find(x => x.Id == scale.Id) ?? new Scale());

            mockBusiness.Setup(p => p.Insert(It.IsAny<Scale>(), It.IsAny<User>()))
                .Returns((Scale scale, User user) =>
                {
                    scale.Id = scales.Count + 1;
                    scales.Add(scale);
                    return scale;
                });

            mockBusiness.Setup(p => p.Update(It.IsAny<Scale>(), It.IsAny<User>()))
                .Returns((Scale scale, User user) =>
                {
                    scales.Where(x => x.Id == scale.Id).ToList().ForEach(x =>
                    {
                        x.Code = scale.Code;
                        x.Name = scale.Name;
                        x.Comission = scale.Comission;
                        x.Order = scale.Order;
                    });
                    return scale;
                });

            mockBusiness.Setup(p => p.Delete(It.IsAny<Scale>(), It.IsAny<User>()))
                .Returns((Scale scale, User user) =>
                {
                    scales = scales.Where(x => x.Id != scale.Id).ToList();
                    return scale;
                });

            api = new ScaleController(configuration, mockBusiness.Object, mockLog.Object, mockTemplate.Object, mockParameter.Object)
            {
                ControllerContext = controllerContext
            };
        }
        #endregion

        #region Methods
        /// <summary>
        /// Prueba la consulta de un listado de escalas con filtros, ordenamientos y límite
        /// </summary>
        [Fact]
        public void ListTest()
        {
            //Act
            ListResult<Scale> list = api.List("idscale = 1", "value", 1, 0);

            //Assert
            Assert.NotEmpty(list.List);
            Assert.True(list.Total > 0);
        }

        /// <summary>
        /// Prueba la consulta de un listado de escalas con filtros, ordenamientos y límite y con errores
        /// </summary>
        [Fact]
        public void ListWithErrorTest()
        {
            //Act
            ListResult<Scale> list = api.List("idescala = 1", "value", 1, 0);

            //Assert
            Assert.Equal(0, list.Total);
        }

        /// <summary>
        /// Prueba la consulta de una escala dada su identificador
        /// </summary>
        [Fact]
        public void ReadTest()
        {
            //Act
            Scale scale = api.Read(1);

            //Assert
            Assert.Equal(1000, scale.Comission);
        }

        /// <summary>
        /// Prueba la consulta de una escala que no existe dado su identificador
        /// </summary>
        [Fact]
        public void ReadNotFoundTest()
        {
            //Act
            Scale scale = api.Read(10);

            //Assert
            Assert.Equal(0, scale.Id);
        }

        /// <summary>
        /// Prueba la inserción de una escala
        /// </summary>
        [Fact]
        public void InsertTest()
        {
            //Arrange
            Scale scale = new() { Code = "C4", Name = "Nueva escala", Comission = 4000, Order = 4 };

            //Act
            scale = api.Insert(scale);

            //Assert
            Assert.NotEqual(0, scale.Id);
        }

        /// <summary>
        /// Prueba la actualización de una escala
        /// </summary>
        [Fact]
        public void UpdateTest()
        {
            //Arrange
            Scale scale = new() { Id = 2, Code = "C5", Name = "Comisión actualizada", Comission = 5000, Order = 5 };

            //Act
            _ = api.Update(scale);
            Scale scale2 = api.Read(2);

            //Assert
            Assert.NotEqual("Comisión 2", scale2.Name);
        }

        /// <summary>
        /// Prueba la eliminación de una escala
        /// </summary>
        [Fact]
        public void DeleteTest()
        {
            //Act
            _ = api.Delete(3);
            Scale scale = api.Read(3);

            //Assert
            Assert.Equal(0, scale.Id);
        }
        #endregion
    }
}

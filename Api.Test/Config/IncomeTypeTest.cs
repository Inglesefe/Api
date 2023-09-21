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
    /// Realiza las pruebas sobre la api de tipos de ingreso
    /// </summary>
    [Collection("Test")]
    public class IncomeTypeTest : TestBase<IncomeType>
    {
        #region Constructors
        /// <summary>
        /// Inicializa la configuración de la prueba
        /// </summary>
        public IncomeTypeTest() : base()
        {
            Mock<IBusiness<IncomeType>> mockBusiness = new();

            List<IncomeType> incomeTypes = new()
            {
                new IncomeType() { Id = 1, Code = "CI", Name = "Cuota inicial" },
                new IncomeType() { Id = 2, Code = "CR", Name = "Crédito cartera" },
                new IncomeType() { Id = 3, Code = "FC", Name = "Factura" }
            };

            mockBusiness.Setup(p => p.List("idincometype = 1", It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>()))
                .Returns(new ListResult<IncomeType>(incomeTypes.Where(y => y.Id == 1).ToList(), 1));
            mockBusiness.Setup(p => p.List("idtipoingreso = 1", It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>()))
                .Throws<PersistentException>();

            mockBusiness.Setup(p => p.Read(It.IsAny<IncomeType>()))
                .Returns((IncomeType incomeType) => incomeTypes.Find(x => x.Id == incomeType.Id) ?? new IncomeType());

            mockBusiness.Setup(p => p.Insert(It.IsAny<IncomeType>(), It.IsAny<User>()))
                .Returns((IncomeType incomeType, User user) =>
                {
                    if (incomeTypes.Exists(x => x.Code == incomeType.Code))
                    {
                        throw new PersistentException();
                    }
                    else
                    {
                        incomeType.Id = incomeTypes.Count + 1;
                        incomeTypes.Add(incomeType);
                        return incomeType;
                    }
                });

            mockBusiness.Setup(p => p.Update(It.IsAny<IncomeType>(), It.IsAny<User>()))
                .Returns((IncomeType incomeType, User user) =>
                {
                    incomeTypes.Where(x => x.Id == incomeType.Id).ToList().ForEach(x => x.Name = incomeType.Name);
                    return incomeType;
                });

            mockBusiness.Setup(p => p.Delete(It.IsAny<IncomeType>(), It.IsAny<User>()))
                .Returns((IncomeType incomeType, User user) =>
                {
                    incomeTypes = incomeTypes.Where(x => x.Id != incomeType.Id).ToList();
                    return incomeType;
                });

            api = new IncomeTypeController(configuration, mockBusiness.Object, mockLog.Object, mockTemplate.Object, mockParameter.Object)
            {
                ControllerContext = controllerContext
            };
        }
        #endregion

        #region Methods
        /// <summary>
        /// Prueba la consulta de un listado de tipos de ingreso con filtros, ordenamientos y límite
        /// </summary>
        [Fact]
        public void ListTest()
        {
            //Act
            ListResult<IncomeType> list = api.List("idincometype = 1", "name", 1, 0);

            //Assert
            Assert.NotEmpty(list.List);
            Assert.True(list.Total > 0);
        }

        /// <summary>
        /// Prueba la consulta de un listado de tipos de ingreso con filtros, ordenamientos y límite y con errores
        /// </summary>
        [Fact]
        public void ListWithErrorTest()
        {
            //Act
            ListResult<IncomeType> list = api.List("idtipoingreso = 1", "name", 1, 0);

            //Assert
            Assert.Equal(0, list.Total);
        }

        /// <summary>
        /// Prueba la consulta de un tipo de ingreso dada su identificador
        /// </summary>
        [Fact]
        public void ReadTest()
        {
            //Act
            IncomeType incomeType = api.Read(1);

            //Assert
            Assert.Equal("CI", incomeType.Code);
        }

        /// <summary>
        /// Prueba la consulta de un tipo de ingreso que no existe dado su identificador
        /// </summary>
        [Fact]
        public void ReadNotFoundTest()
        {
            //Act
            IncomeType incomeType = api.Read(10);

            //Assert
            Assert.Equal(0, incomeType.Id);
        }

        /// <summary>
        /// Prueba la inserción de un tipo de ingreso
        /// </summary>
        [Fact]
        public void InsertTest()
        {
            //Arrange
            IncomeType incomeType = new() { Code = "CF", Name = "Cheques posfechados" };

            //Act
            incomeType = api.Insert(incomeType);

            //Assert
            Assert.NotEqual(0, incomeType.Id);
        }

        /// <summary>
        /// Prueba la actualización de un tipo de ingreso
        /// </summary>
        [Fact]
        public void UpdateTest()
        {
            //Arrange
            IncomeType incomeType = new() { Id = 2, Code = "CT", Name = "Otro ingreso" };

            //Act
            _ = api.Update(incomeType);
            IncomeType incomeType2 = api.Read(2);

            //Assert
            Assert.NotEqual("Credito cartera", incomeType2.Name);
        }

        /// <summary>
        /// Prueba la eliminación de un tipo de ingreso
        /// </summary>
        [Fact]
        public void DeleteTest()
        {
            //Act
            _ = api.Delete(3);
            IncomeType incomeType = api.Read(3);

            //Assert
            Assert.Equal(0, incomeType.Id);
        }
        #endregion
    }
}

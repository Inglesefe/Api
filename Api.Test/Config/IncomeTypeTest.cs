using Api.Controllers.Config;
using Dal.Dto;
using Entities.Config;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.Extensions.Configuration;
using System.Security.Claims;
using System.Security.Principal;

namespace Api.Test.Config
{
    /// <summary>
    /// Realiza las pruebas sobre la api de tipos de ingreso
    /// </summary>
    [Collection("Test")]
    public class IncomeTypeTest
    {
        #region Attributes
        /// <summary>
        /// Configuración de la aplicación de pruebas
        /// </summary>
        private readonly IConfiguration _configuration;

        /// <summary>
        /// Controlador API para los tipos de ingreso
        /// </summary>
        private readonly IncomeTypeController _api;

        /// <summary>
        /// Contexto HTTP con que se conecta a los servicios Rest
        /// </summary>
        private readonly ControllerContext _controllerContext;
        #endregion

        #region Constructors
        /// <summary>
        /// Inicializa la configuración de la prueba
        /// </summary>
        public IncomeTypeTest()
        {
            GenericIdentity identity = new("usuario", "prueba");
            identity.AddClaim(new Claim("id", "1"));
            _controllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
                {
                    User = new ClaimsPrincipal(identity)
                },
                ActionDescriptor = new ControllerActionDescriptor()
                {
                    ControllerName = "IncomeTypeTest",
                    ActionName = "Test"
                }
            };
            _configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", false, false)
                .AddEnvironmentVariables()
                .Build();

            _api = new(_configuration)
            {
                ControllerContext = _controllerContext
            };
        }
        #endregion

        #region Methods
        /// <summary>
        /// Prueba la consulta de un listado de tipos de ingreso con filtros, ordenamientos y límite
        /// </summary>
        [Fact]
        public void IncomeTypeListTest()
        {
            ListResult<IncomeType> list = _api.List("idincometype = 1", "name", 1, 0);

            Assert.NotEmpty(list.List);
            Assert.True(list.Total > 0);
        }

        /// <summary>
        /// Prueba la consulta de un listado de tipos de ingreso con filtros, ordenamientos y límite y con errores
        /// </summary>
        [Fact]
        public void IncomeTypeListWithErrorTest()
        {
            ListResult<IncomeType> list = _api.List("idtipoingreso = 1", "name", 1, 0);

            Assert.Equal(0, list.Total);
        }

        /// <summary>
        /// Prueba la consulta de un tipo de ingreso dada su identificador
        /// </summary>
        [Fact]
        public void IncomeTypeReadTest()
        {
            IncomeType incomeType = _api.Read(1);

            Assert.Equal("CI", incomeType.Code);
        }

        /// <summary>
        /// Prueba la consulta de un tipo de ingreso que no existe dado su identificador
        /// </summary>
        [Fact]
        public void IncomeTypeReadNotFoundTest()
        {
            IncomeType incomeType = _api.Read(10);

            Assert.Equal(0, incomeType.Id);
        }

        /// <summary>
        /// Prueba la inserción de un tipo de ingreso
        /// </summary>
        [Fact]
        public void IncomeTypeInsertTest()
        {
            IncomeType incomeType = new() { Code = "CF", Name = "Cheques posfechados" };
            incomeType = _api.Insert(incomeType);

            Assert.NotEqual(0, incomeType.Id);
        }

        /// <summary>
        /// Prueba la actualización de un tipo de ingreso
        /// </summary>
        [Fact]
        public void IncomeTypeUpdateTest()
        {
            IncomeType incomeType = new() { Id = 2, Code = "CT", Name = "Otro ingreso" };
            _ = _api.Update(incomeType);

            IncomeType incomeType2 = _api.Read(2);

            Assert.NotEqual("Credito cartera", incomeType2.Name);
        }

        /// <summary>
        /// Prueba la eliminación de un tipo de ingreso
        /// </summary>
        [Fact]
        public void IncomeTypeDeleteTest()
        {
            _ = _api.Delete(3);

            IncomeType incomeType = _api.Read(3);

            Assert.Equal(0, incomeType.Id);
        }
        #endregion
    }
}

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
    /// Realiza las pruebas sobre la api de planes
    /// </summary>
    [Collection("Test")]
    public class PlanTest
    {
        #region Attributes
        /// <summary>
        /// Configuración de la aplicación de pruebas
        /// </summary>
        private readonly IConfiguration _configuration;

        /// <summary>
        /// Controlador API para los planes
        /// </summary>
        private readonly PlanController _api;

        /// <summary>
        /// Contexto HTTP con que se conecta a los servicios Rest
        /// </summary>
        private readonly ControllerContext _controllerContext;
        #endregion

        #region Constructors
        /// <summary>
        /// Inicializa la configuración de la prueba
        /// </summary>
        public PlanTest()
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
                    ControllerName = "PlanTest",
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
        /// Prueba la consulta de un listado de planes con filtros, ordenamientos y límite
        /// </summary>
        [Fact]
        public void PlanListTest()
        {
            ListResult<Plan> list = _api.List("idplan = 1", "value", 1, 0);

            Assert.NotEmpty(list.List);
            Assert.True(list.Total > 0);
        }

        /// <summary>
        /// Prueba la consulta de un listado de planes con filtros, ordenamientos y límite y con errores
        /// </summary>
        [Fact]
        public void PlanListWithErrorTest()
        {
            ListResult<Plan> list = _api.List("idplan = 1", "valor", 1, 0);

            Assert.Equal(0, list.Total);
        }

        /// <summary>
        /// Prueba la consulta de un plan dada su identificador
        /// </summary>
        [Fact]
        public void PlanReadTest()
        {
            Plan plan = _api.Read(1);

            Assert.Equal(12, plan.InstallmentsNumber);
        }

        /// <summary>
        /// Prueba la consulta de un plan que no existe dado su identificador
        /// </summary>
        [Fact]
        public void PlanReadNotFoundTest()
        {
            Plan plan = _api.Read(10);

            Assert.Equal(0, plan.Id);
        }

        /// <summary>
        /// Prueba la inserción de un plan
        /// </summary>
        [Fact]
        public void PlanInsertTest()
        {
            Plan plan = new() { InitialFee = 5000, InstallmentsNumber = 5, InstallmentValue = 250, Value = 75000, Active = true, Description = "Plan de prueba de insercion" };
            plan = _api.Insert(plan);

            Assert.NotEqual(0, plan.Id);
        }

        /// <summary>
        /// Prueba la actualización de un plan
        /// </summary>
        [Fact]
        public void PlanUpdateTest()
        {
            Plan plan = new() { Id = 2, InitialFee = 54321, InstallmentsNumber = 35, InstallmentValue = 750, Value = 85000, Active = true, Description = "Plan de prueba de actualización" };
            _ = _api.Update(plan);

            Plan plan2 = _api.Read(2);

            Assert.NotEqual(15, plan2.InstallmentsNumber);
        }

        /// <summary>
        /// Prueba la eliminación de un plan
        /// </summary>
        [Fact]
        public void PlanDeleteTest()
        {
            _ = _api.Delete(3);

            Plan plan = _api.Read(3);

            Assert.Equal(0, plan.Id);
        }
        #endregion
    }
}

using Api.Controllers.Config;
using Business;
using Dal;
using Dal.Dto;
using Dal.Exceptions;
using Entities.Auth;
using Entities.Config;
using Entities.Log;
using Entities.Noti;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.Extensions.Configuration;
using Moq;
using System.Data;
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
            Mock<IBusiness<Plan>> mockBusiness = new();
            Mock<IPersistentBase<LogComponent>> mockLog = new();
            Mock<IBusiness<Template>> mockTemplate = new();
            Mock<IDbConnection> mockConnection = new();

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

            List<Plan> plans = new()
            {
                new Plan() { Id = 1, Value = 3779100, InitialFee = 444600, InstallmentsNumber = 12, InstallmentValue = 444600, Active = true, Description = "PLAN COFREM 12 MESES" },
                new Plan() { Id = 2, Value = 3779100, InitialFee = 282600, InstallmentsNumber = 15, InstallmentValue = 233100, Active = true, Description = "PLAN COFREM 15 MESES" },
                new Plan() { Id = 3, Value = 3779100, InitialFee = 235350, InstallmentsNumber = 15, InstallmentValue = 236250, Active = false, Description = "PLAN COFREM 15 MESES ESPECIAL" }
            };
            List<Template> templates = new()
            {
                new Template() { Id = 1, Name = "Notificación de error", Content = "<p>Error #{id}#</p><p>La excepci&oacute;n tiene el siguiente mensaje: #{message}#</p>" },
                new Template() { Id = 2, Name = "Recuperación contraseña", Content = "<p>Prueba recuperaci&oacute;n contrase&ntilde;a con enlace #{link}#</p>" },
                new Template() { Id = 3, Name = "Contraseña cambiada", Content = "<p>Prueba de que su contrase&ntilde;a ha sido cambiada con &eacute;xito</p>" }
            };

            mockBusiness.Setup(p => p.List("idplan = 1", It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<IDbConnection>()))
                .Returns(new ListResult<Plan>(plans.Where(y => y.Id == 1).ToList(), 1));
            mockBusiness.Setup(p => p.List("idplano = 1", It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<IDbConnection>()))
                .Throws<PersistentException>();

            mockBusiness.Setup(p => p.Read(It.IsAny<Plan>(), It.IsAny<IDbConnection>()))
                .Returns((Plan plan, IDbConnection connection) => plans.Find(x => x.Id == plan.Id) ?? new Plan());

            mockBusiness.Setup(p => p.Insert(It.IsAny<Plan>(), It.IsAny<User>(), It.IsAny<IDbConnection>()))
                .Returns((Plan plan, User user, IDbConnection connection) =>
                {
                    plan.Id = plans.Count + 1;
                    plans.Add(plan);
                    return plan;
                });

            mockBusiness.Setup(p => p.Update(It.IsAny<Plan>(), It.IsAny<User>(), It.IsAny<IDbConnection>()))
                .Returns((Plan plan, User user, IDbConnection connection) =>
                {
                    plans.Where(x => x.Id == plan.Id).ToList().ForEach(x =>
                    {
                        x.Value = plan.Value;
                        x.InitialFee = plan.InitialFee;
                        x.InstallmentsNumber = plan.InstallmentsNumber;
                        x.InstallmentValue = plan.InstallmentValue;
                        x.Active = plan.Active;
                        x.Description = plan.Description;
                    });
                    return plan;
                });

            mockBusiness.Setup(p => p.Delete(It.IsAny<Plan>(), It.IsAny<User>(), It.IsAny<IDbConnection>()))
                .Returns((Plan plan, User user, IDbConnection connection) =>
                {
                    plans = plans.Where(x => x.Id != plan.Id).ToList();
                    return plan;
                });

            mockLog.Setup(p => p.Insert(It.IsAny<LogComponent>(), It.IsAny<IDbConnection>())).Returns((LogComponent log, IDbConnection connection) => log);

            mockTemplate.Setup(p => p.Read(It.IsAny<Template>(), It.IsAny<IDbConnection>())).Returns((Template template, IDbConnection connection) => templates.Find(x => x.Id == template.Id) ?? new Template());

            _api = new(_configuration, mockBusiness.Object, mockLog.Object, mockTemplate.Object, mockConnection.Object)
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
            ListResult<Plan> list = _api.List("idplano = 1", "value", 1, 0);

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

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
            Mock<IBusiness<IncomeType>> mockBusiness = new();
            Mock<IPersistentBase<LogComponent>> mockLog = new();
            Mock<IBusiness<Template>> mockTemplate = new();

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

            List<IncomeType> incomeTypes = new()
            {
                new IncomeType() { Id = 1, Code = "CI", Name = "Cuota inicial" },
                new IncomeType() { Id = 2, Code = "CR", Name = "Crédito cartera" },
                new IncomeType() { Id = 3, Code = "FC", Name = "Factura" }
            };
            List<Template> templates = new()
            {
                new Template() { Id = 1, Name = "Notificación de error", Content = "<p>Error #{id}#</p><p>La excepci&oacute;n tiene el siguiente mensaje: #{message}#</p>" },
                new Template() { Id = 2, Name = "Recuperación contraseña", Content = "<p>Prueba recuperaci&oacute;n contrase&ntilde;a con enlace #{link}#</p>" },
                new Template() { Id = 3, Name = "Contraseña cambiada", Content = "<p>Prueba de que su contrase&ntilde;a ha sido cambiada con &eacute;xito</p>" }
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

            mockLog.Setup(p => p.Insert(It.IsAny<LogComponent>())).Returns((LogComponent log) => log);

            mockTemplate.Setup(p => p.Read(It.IsAny<Template>())).Returns((Template template) => templates.Find(x => x.Id == template.Id) ?? new Template());

            _api = new(_configuration, mockBusiness.Object, mockLog.Object, mockTemplate.Object)
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

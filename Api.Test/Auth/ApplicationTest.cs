using Api.Controllers.Auth;
using Dal.Dto;
using Entities.Auth;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.Extensions.Configuration;
using System.Security.Claims;
using System.Security.Principal;

namespace Api.Test.Auth
{
    /// <summary>
    /// Realiza las pruebas sobre la api de aplicaciones
    /// </summary>
    [Collection("Test")]
    public class ApplicationTest
    {
        #region Attributes
        /// <summary>
        /// Configuración de la aplicación de pruebas
        /// </summary>
        private readonly IConfiguration _configuration;

        /// <summary>
        /// Controlador API para las aplicaciones
        /// </summary>
        private readonly ApplicationController _api;

        /// <summary>
        /// Contexto HTTP con que se conecta a los servicios Rest
        /// </summary>
        private readonly ControllerContext _controllerContext;
        #endregion

        #region Constructors
        /// <summary>
        /// Inicializa la configuración de la prueba
        /// </summary>
        public ApplicationTest()
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
                    ControllerName = "ApplicationTest",
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
        /// Prueba la consulta de un listado de aplicaciones con filtros, ordenamientos y límite
        /// </summary>
        /// <returns>N/A</returns>
        [Fact]
        public void ApplicationListTest()
        {
            ListResult<Application> list = _api.List("idapplication = 1", "name", 1, 0);

            Assert.NotEmpty(list.List);
            Assert.True(list.Total > 0);
        }

        /// <summary>
        /// Prueba la consulta de un listado de aplicaciones con filtros, ordenamientos y límite y con errores
        /// </summary>
        /// <returns>N/A</returns>
        [Fact]
        public void ApplicationListWithErrorTest()
        {
            ListResult<Application> list = _api.List("idaplicacion = 1", "name", 1, 0);

            Assert.Equal(0, list.Total);
        }

        /// <summary>
        /// Prueba la consulta de una aplicación dada su identificador
        /// </summary>
        /// <returns>N/A</returns>
        [Fact]
        public void ApplicationReadTest()
        {
            Application application = _api.Read(1);

            Assert.Equal("Autenticacion", application.Name);
        }

        /// <summary>
        /// Prueba la consulta de una aplicación que no existe dado su identificador
        /// </summary>
        /// <returns>N/A</returns>
        [Fact]
        public void ApplicationReadNotFoundTest()
        {
            Application application = _api.Read(10);

            Assert.Equal(0, application.Id);
        }

        /// <summary>
        /// Prueba la inserción de una aplicación
        /// </summary>
        /// <returns>N/A</returns>
        [Fact]
        public void ApplicationInsertTest()
        {
            Application application = new() { Name = "Prueba 1" };
            application = _api.Insert(application);

            Assert.NotEqual(0, application.Id);
        }

        /// <summary>
        /// Prueba la inserción de una aplicación con nombre duplicado
        /// </summary>
        /// <returns>N/A</returns>
        [Fact]
        public void ApplicationInsertDuplicateTest()
        {
            Application application = new() { Name = "Autenticacion" };
            application = _api.Insert(application);

            Assert.Equal(0, application.Id);
        }

        /// <summary>
        /// Prueba la actualización de una aplicación
        /// </summary>
        /// <returns>N/A</returns>
        [Fact]
        public void ApplicationUpdateTest()
        {
            Application application = new() { Id = 2, Name = "Prueba actualizar" };
            _ = _api.Update(application);

            Application application2 = _api.Read(2);

            Assert.NotEqual("Actualizame", application2.Name);
        }

        /// <summary>
        /// Prueba la eliminación de una aplicación
        /// </summary>
        /// <returns>N/A</returns>
        [Fact]
        public void ApplicationDeleteTest()
        {
            _ = _api.Delete(3);

            Application application = _api.Read(3);

            Assert.Equal(0, application.Id);
        }

        /// <summary>
        /// Prueba la consulta de un listado de roles de una aplicación con filtros, ordenamientos y límite
        /// </summary>
        /// <returns>N/A</returns>
        [Fact]
        public void ApplicationListRolesTest()
        {
            ListResult<Role> list = _api.ListRoles("", "", 10, 0, 1);

            Assert.NotEmpty(list.List);
            Assert.True(list.Total > 0);
        }

        /// <summary>
        /// Prueba la consulta de un listado de roles de una aplicación con filtros, ordenamientos y límite y con errores
        /// </summary>
        /// <returns>N/A</returns>
        [Fact]
        public void ApplicationListRolesWithErrorTest()
        {
            ListResult<Role> list = _api.ListRoles("idaplicación = 1", "name", 10, 0, 1);

            Assert.Empty(list.List);
            Assert.Equal(0, list.Total);
        }

        /// <summary>
        /// Prueba la consulta de un listado de roles no asignados a una aplicación con filtros, ordenamientos y límite
        /// </summary>
        /// <returns>N/A</returns>
        [Fact]
        public void ApplicationListNotRolesTest()
        {
            ListResult<Role> list = _api.ListNotRoles("", "", 10, 0, 1);

            Assert.NotEmpty(list.List);
            Assert.True(list.Total > 0);
        }

        /// <summary>
        /// Prueba la inserción de un rol de una aplicación
        /// </summary>
        /// <returns>N/A</returns>
        [Fact]
        public void ApplicationInsertRoleTest()
        {
            Role role = _api.InsertRole(new() { Id = 4 }, 1);

            Assert.NotEqual(0, role.Id);
        }

        /// <summary>
        /// Prueba la inserción de un rol de una aplicación duplicado
        /// </summary>
        /// <returns>N/A</returns>
        [Fact]
        public void ApplicationInsertRoleDuplicateTest()
        {
            Role role = _api.InsertRole(new() { Id = 1 }, 1);

            Assert.Equal(0, role.Id);
        }

        /// <summary>
        /// Prueba la eliminación de un rol de una aplicación
        /// </summary>
        /// <returns>N/A</returns>
        [Fact]
        public void ApplicationDeleteRoleTest()
        {
            _ = _api.DeleteRole(2, 1);
            ListResult<Role> list = _api.ListRoles("r.idrole = 2", "", 10, 0, 1);

            Assert.Equal(0, list.Total);
        }
        #endregion
    }
}

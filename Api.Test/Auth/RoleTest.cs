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
    /// Realiza las pruebas sobre la clase de api de roles
    /// </summary>
    [Collection("Test")]
    public class RoleTest
    {
        #region Attributes
        /// <summary>
        /// Configuración de la aplicación de pruebas
        /// </summary>
        private readonly IConfiguration _configuration;

        /// <summary>
        /// Controlador API para los roles
        /// </summary>
        private readonly RoleController _api;

        /// <summary>
        /// Contexto HTTP con que se conecta a los servicios Rest
        /// </summary>
        private readonly ControllerContext _controllerContext;
        #endregion

        #region Constructors
        /// <summary>
        /// Inicializa la configuración de la prueba
        /// </summary>
        public RoleTest()
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
                    ControllerName = "RoleTest",
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
        /// Prueba la consulta de un listado de roles con filtros, ordenamientos y límite
        /// </summary>
        /// <returns>N/A</returns>
        [Fact]
        public void RoleListTest()
        {
            ListResult<Role> list = _api.List("idrole = 1", "name", 1, 0);

            Assert.NotEmpty(list.List);
            Assert.True(list.Total > 0);
        }

        /// <summary>
        /// Prueba la consulta de un listado de roles con filtros, ordenamientos y límite y con errores
        /// </summary>
        /// <returns>N/A</returns>
        [Fact]
        public void RoleListWithErrorTest()
        {
            ListResult<Role> list = _api.List("idrol = 1", "name", 1, 0);

            Assert.Empty(list.List);
            Assert.Equal(0, list.Total);
        }

        /// <summary>
        /// Prueba la consulta de un rol dado su identificador
        /// </summary>
        /// <returns>N/A</returns>
        [Fact]
        public void RoleReadTest()
        {
            Role role = _api.Read(1);
            Assert.Equal("Administradores", role.Name);
        }

        /// <summary>
        /// Prueba la consulta de un rol que no existe dado su identificador
        /// </summary>
        /// <returns>N/A</returns>
        [Fact]
        public void RoleReadNotFoundTest()
        {
            Role role = _api.Read(10);
            Assert.Equal(0, role.Id);
        }

        /// <summary>
        /// Prueba la inserción de un rol
        /// </summary>
        /// <returns>N/A</returns>
        [Fact]
        public void RoleInsertTest()
        {
            Role role = new() { Name = "Prueba insercion" };
            role = _api.Insert(role);
            Assert.NotEqual(0, role.Id);
        }

        /// <summary>
        /// Prueba la inserción de un rol con nombre duplicado
        /// </summary>
        /// <returns>N/A</returns>
        [Fact]
        public void RoleInsertDuplicateTest()
        {
            Role role = new() { Name = "Administradores" };
            role = _api.Insert(role);
            Assert.Equal(0, role.Id);
        }

        /// <summary>
        /// Prueba la actualización de un rol
        /// </summary>
        /// <returns>N/A</returns>
        [Fact]
        public void RoleUpdateTest()
        {
            Role role = new() { Id = 2, Name = "Prueba actualizar" };
            _ = _api.Update(role);

            Role role2 = _api.Read(2);

            Assert.NotEqual("Actualizame", role2.Name);
        }

        /// <summary>
        /// Prueba la eliminación de un rol
        /// </summary>
        /// <returns>N/A</returns>
        [Fact]
        public void RoleDeleteTest()
        {
            _ = _api.Delete(3);

            Role role = _api.Read(3);

            Assert.Equal(0, role.Id);
        }

        /// <summary>
        /// Prueba la consulta de un listado de usuarios de un rol con filtros, ordenamientos y límite
        /// </summary>
        /// <returns>N/A</returns>
        [Fact]
        public void RoleListUsersTest()
        {
            ListResult<User> list = _api.ListUsers("", "", 10, 0, 1);

            Assert.NotEmpty(list.List);
            Assert.True(list.Total > 0);
        }

        /// <summary>
        /// Prueba la consulta de un listado de usuarios de un rol con filtros, ordenamientos y límite y con errores
        /// </summary>
        /// <returns>N/A</returns>
        [Fact]
        public void RoleListUsersWithErrorTest()
        {
            ListResult<User> list = _api.ListUsers("idusuario = 1", "", 10, 0, 1);

            Assert.Empty(list.List);
            Assert.Equal(0, list.Total);
        }

        /// <summary>
        /// Prueba la consulta de un listado de usuarios no asignados a un rol con filtros, ordenamientos y límite
        /// </summary>
        /// <returns>N/A</returns>
        [Fact]
        public void RoleListNotUsersTest()
        {
            ListResult<User> list = _api.ListNotUsers("", "", 10, 0, 1);

            Assert.NotEmpty(list.List);
            Assert.True(list.Total > 0);
        }

        /// <summary>
        /// Prueba la inserción de un usuario de un rol
        /// </summary>
        /// <returns>N/A</returns>
        [Fact]
        public void RoleInsertUserTest()
        {
            User role = _api.InsertUser(new() { Id = 2 }, 4);
            Assert.NotEqual(0, role.Id);
        }

        /// <summary>
        /// Prueba la inserción de un usuario de un rol duplicado
        /// </summary>
        /// <returns>N/A</returns>
        [Fact]
        public void RoleInsertUserDuplicateTest()
        {
            User role = _api.InsertUser(new() { Id = 2 }, 1);
            Assert.Equal(0, role.Id);
        }

        /// <summary>
        /// Prueba la eliminación de un usuario de un rol
        /// </summary>
        /// <returns>N/A</returns>
        [Fact]
        public void RoleDeleteUserTest()
        {
            _ = _api.DeleteUser(2, 2);
            ListResult<User> list = _api.ListUsers("u.iduser = 2", "", 10, 0, 2);
            Assert.Equal(0, list.Total);
        }


        /// <summary>
        /// Prueba la consulta de un listado de aplicaciones de un rol con filtros, ordenamientos y límite
        /// </summary>
        /// <returns>N/A</returns>
        [Fact]
        public void RoleListApplicationsTest()
        {
            ListResult<Application> list = _api.ListApplications("", "", 10, 0, 1);

            Assert.NotEmpty(list.List);
            Assert.True(list.Total > 0);
        }

        /// <summary>
        /// Prueba la consulta de un listado de aplicaciones de un rol con filtros, ordenamientos y límite y con errores
        /// </summary>
        /// <returns>N/A</returns>
        [Fact]
        public void RoleListApplicationsWithErrorTest()
        {
            ListResult<Application> list = _api.ListApplications("idaplicacion = 1", "", 10, 0, 1);

            Assert.Empty(list.List);
            Assert.Equal(0, list.Total);
        }

        /// <summary>
        /// Prueba la consulta de un listado de aplicaciones no asignadas a un rol con filtros, ordenamientos y límite
        /// </summary>
        /// <returns>N/A</returns>
        [Fact]
        public void RoleListNotApplicationsTest()
        {
            ListResult<Application> list = _api.ListNotApplications("", "", 10, 0, 1);

            Assert.NotEmpty(list.List);
            Assert.True(list.Total > 0);
        }

        /// <summary>
        /// Prueba la inserción de una aplicación de un rol
        /// </summary>
        /// <returns>N/A</returns>
        [Fact]
        public void RoleInsertApplicationTest()
        {
            Application application = _api.InsertApplication(new() { Id = 2 }, 4);
            Assert.NotEqual(0, application.Id);
        }

        /// <summary>
        /// Prueba la inserción de una aplicación de un rol duplicado
        /// </summary>
        /// <returns>N/A</returns>
        [Fact]
        public void RoleInsertApplicationDuplicateTest()
        {
            Application application = _api.InsertApplication(new() { Id = 2 }, 1);
            Assert.Equal(0, application.Id);
        }

        /// <summary>
        /// Prueba la eliminación de una aplicación de un rol
        /// </summary>
        /// <returns>N/A</returns>
        [Fact]
        public void RoleDeleteApplicationTest()
        {
            _ = _api.DeleteApplication(2, 2);
            ListResult<Application> list = _api.ListApplications("a.idapplication = 2", "", 10, 0, 2);
            Assert.Equal(0, list.Total);
        }
        #endregion
    }
}

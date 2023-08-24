using Api.Controllers.Auth;
using Business;
using Business.Auth;
using Dal;
using Dal.Dto;
using Dal.Exceptions;
using Entities.Auth;
using Entities.Log;
using Entities.Noti;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.Extensions.Configuration;
using Moq;
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
            Mock<IBusinessApplication> mockBusiness = new();
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
                    ControllerName = "ApplicationTest",
                    ActionName = "Test"
                }
            };
            _configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", false, false)
                .AddEnvironmentVariables()
                .Build();

            List<Application> apps = new()
            {
                new Application() { Id = 1, Name = "Autenticación" },
                new Application() { Id = 2, Name = "Actualízame" },
                new Application() { Id = 3, Name = "Bórrame" }
            };
            List<Role> roles = new()
            {
                new Role() { Id = 1, Name = "Administradores" },
                new Role() { Id = 2, Name = "Actualízame" },
                new Role() { Id = 3, Name = "Bórrame" },
                new Role() { Id = 4, Name = "Para probar user_role y application_role" },
            };
            List<Tuple<Application, Role>> apps_roles = new()
            {
                new Tuple<Application, Role>(apps[0], roles[0]),
                new Tuple<Application, Role>(apps[0], roles[1]),
                new Tuple<Application, Role>(apps[1], roles[0]),
                new Tuple<Application, Role>(apps[1], roles[1])
            };
            List<Template> templates = new()
            {
                new Template() { Id = 1, Name = "Notificación de error", Content = "<p>Error #{id}#</p><p>La excepci&oacute;n tiene el siguiente mensaje: #{message}#</p>" },
                new Template() { Id = 2, Name = "Recuperación contraseña", Content = "<p>Prueba recuperaci&oacute;n contrase&ntilde;a con enlace #{link}#</p>" },
                new Template() { Id = 3, Name = "Contraseña cambiada", Content = "<p>Prueba de que su contrase&ntilde;a ha sido cambiada con &eacute;xito</p>" }
            };

            mockBusiness.Setup(p => p.List("idapplication = 1", It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>()))
                .Returns(new ListResult<Application>(apps.Where(y => y.Id == 1).ToList(), 1));
            mockBusiness.Setup(p => p.List("idaplicacion = 1", It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>()))
                .Throws<PersistentException>();

            mockBusiness.Setup(p => p.Read(It.IsAny<Application>()))
                .Returns((Application app) => apps.Find(x => x.Id == app.Id) ?? new Application());

            mockBusiness.Setup(p => p.Insert(It.IsAny<Application>(), It.IsAny<User>()))
                .Returns((Application app, User user) =>
                {
                    if (apps.Exists(x => x.Name == app.Name))
                    {
                        throw new PersistentException();
                    }
                    else
                    {
                        app.Id = apps.Count + 1;
                        apps.Add(app);
                        return app;
                    }
                });

            mockBusiness.Setup(p => p.Update(It.IsAny<Application>(), It.IsAny<User>()))
                .Returns((Application app, User user) =>
                {
                    apps.Where(x => x.Id == app.Id).ToList().ForEach(x => x.Name = app.Name);
                    return app;
                });

            mockBusiness.Setup(p => p.Delete(It.IsAny<Application>(), It.IsAny<User>()))
                .Returns((Application app, User user) =>
                {
                    apps = apps.Where(x => x.Id != app.Id).ToList();
                    return app;
                });

            mockBusiness.Setup(p => p.ListRoles("", It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<Application>()))
                .Returns(new ListResult<Role>(apps_roles.Where(x => x.Item1.Id == 1).Select(x => x.Item2).ToList(), 1));

            mockBusiness.Setup(p => p.ListRoles("r.idrole = 2", It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<Application>()))
                .Returns(new ListResult<Role>(new List<Role>(), 0));

            mockBusiness.Setup(p => p.ListRoles("idaplicación = 1", It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<Application>()))
                .Throws<PersistentException>();

            mockBusiness.Setup(p => p.ListNotRoles(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<Application>()))
                .Returns((string filters, string orders, int limit, int offset, Application app) =>
                {
                    List<Role> result = roles.Where(x => !apps_roles.Exists(y => y.Item1.Id == app.Id && y.Item2.Id == x.Id)).ToList();
                    return new ListResult<Role>(result, result.Count);
                });

            mockBusiness.Setup(p => p.InsertRole(It.IsAny<Role>(), It.IsAny<Application>(), It.IsAny<User>())).
                Returns((Role role, Application app, User user) =>
                {
                    if (apps_roles.Exists(x => x.Item1.Id == app.Id && x.Item2.Id == role.Id))
                    {
                        throw new PersistentException();
                    }
                    else
                    {
                        apps_roles.Add(new Tuple<Application, Role>(app, role));
                        return role;
                    }
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
        /// Prueba la consulta de un listado de aplicaciones con filtros, ordenamientos y límite
        /// </summary>
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
        [Fact]
        public void ApplicationListWithErrorTest()
        {
            ListResult<Application> list = _api.List("idaplicacion = 1", "name", 1, 0);

            Assert.Equal(0, list.Total);
        }

        /// <summary>
        /// Prueba la consulta de una aplicación dada su identificador
        /// </summary>
        [Fact]
        public void ApplicationReadTest()
        {
            Application application = _api.Read(1);

            Assert.Equal("Autenticación", application.Name);
        }

        /// <summary>
        /// Prueba la consulta de una aplicación que no existe dado su identificador
        /// </summary>
        [Fact]
        public void ApplicationReadNotFoundTest()
        {
            Application application = _api.Read(10);

            Assert.Equal(0, application.Id);
        }

        /// <summary>
        /// Prueba la inserción de una aplicación
        /// </summary>
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
        [Fact]
        public void ApplicationInsertDuplicateTest()
        {
            Application application = new() { Name = "Autenticación" };
            application = _api.Insert(application);

            Assert.Equal(0, application.Id);
        }

        /// <summary>
        /// Prueba la actualización de una aplicación
        /// </summary>
        [Fact]
        public void ApplicationUpdateTest()
        {
            Application application = new() { Id = 2, Name = "Prueba actualizar" };
            _ = _api.Update(application);

            Application application2 = _api.Read(2);

            Assert.NotEqual("Actualízame", application2.Name);
        }

        /// <summary>
        /// Prueba la eliminación de una aplicación
        /// </summary>
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
        [Fact]
        public void ApplicationInsertRoleTest()
        {
            Role role = _api.InsertRole(new() { Id = 4 }, 1);

            Assert.NotEqual(0, role.Id);
        }

        /// <summary>
        /// Prueba la inserción de un rol de una aplicación duplicado
        /// </summary>
        [Fact]
        public void ApplicationInsertRoleDuplicateTest()
        {
            Role role = _api.InsertRole(new() { Id = 1 }, 1);

            Assert.Equal(0, role.Id);
        }

        /// <summary>
        /// Prueba la eliminación de un rol de una aplicación
        /// </summary>
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

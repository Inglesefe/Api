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
            Mock<IBusinessRole> mockBusiness = new();
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
                    ControllerName = "RoleTest",
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
            List<User> users = new()
            {
                new User() { Id = 1, Login = "leandrobaena@gmail.com", Name = "Leandro Baena Torres", Active = true },
                new User() { Id = 2, Login = "actualizame@gmail.com", Name = "Karol Ximena Baena", Active = true },
                new User() { Id = 3, Login = "borrame@gmail.com", Name = "David Santiago Baena", Active = true },
                new User() { Id = 4, Login = "inactivo@gmail.com", Name = "Luz Marina Torres", Active = false }
            };
            List<Tuple<Application, Role>> apps_roles = new()
            {
                new Tuple<Application, Role>(apps[0], roles[0]),
                new Tuple<Application, Role>(apps[0], roles[1]),
                new Tuple<Application, Role>(apps[1], roles[0]),
                new Tuple<Application, Role>(apps[1], roles[1])
            };
            List<Tuple<User, Role>> users_roles = new()
            {
                new Tuple<User, Role>(users[0], roles[0]),
                new Tuple<User, Role>(users[0], roles[1]),
                new Tuple<User, Role>(users[1], roles[0]),
                new Tuple<User, Role>(users[1], roles[1])
            };
            List<Template> templates = new()
            {
                new Template() { Id = 1, Name = "Notificación de error", Content = "<p>Error #{id}#</p><p>La excepci&oacute;n tiene el siguiente mensaje: #{message}#</p>" },
                new Template() { Id = 2, Name = "Recuperación contraseña", Content = "<p>Prueba recuperaci&oacute;n contrase&ntilde;a con enlace #{link}#</p>" },
                new Template() { Id = 3, Name = "Contraseña cambiada", Content = "<p>Prueba de que su contrase&ntilde;a ha sido cambiada con &eacute;xito</p>" }
            };

            mockBusiness.Setup(p => p.List("idrole = 1", It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>()))
                .Returns(new ListResult<Role>(roles.Where(y => y.Id == 1).ToList(), 1));
            mockBusiness.Setup(p => p.List("idrol = 1", It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>()))
                .Throws<PersistentException>();

            mockBusiness.Setup(p => p.Read(It.IsAny<Role>()))
                .Returns((Role role) => roles.Find(x => x.Id == role.Id) ?? new Role());

            mockBusiness.Setup(p => p.Insert(It.IsAny<Role>(), It.IsAny<User>()))
                .Returns((Role role, User user) =>
                {
                    if (roles.Exists(x => x.Name == role.Name))
                    {
                        throw new PersistentException();
                    }
                    else
                    {
                        role.Id = roles.Count + 1;
                        roles.Add(role);
                        return role;
                    }
                });

            mockBusiness.Setup(p => p.Update(It.IsAny<Role>(), It.IsAny<User>()))
                .Returns((Role role, User user) =>
                {
                    roles.Where(x => x.Id == role.Id).ToList().ForEach(x => x.Name = role.Name);
                    return role;
                });

            mockBusiness.Setup(p => p.Delete(It.IsAny<Role>(), It.IsAny<User>()))
                .Returns((Role role, User user) =>
                {
                    roles = roles.Where(x => x.Id != role.Id).ToList();
                    return role;
                });

            mockBusiness.Setup(p => p.ListApplications("", It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<Role>()))
                .Returns(new ListResult<Application>(apps_roles.Where(x => x.Item2.Id == 1).Select(x => x.Item1).ToList(), 1));

            mockBusiness.Setup(p => p.ListApplications("a.idapplication = 2", It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<Role>()))
                .Returns(new ListResult<Application>(new List<Application>(), 0));

            mockBusiness.Setup(p => p.ListApplications("idaplicacion = 1", It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<Role>()))
                .Throws<PersistentException>();

            mockBusiness.Setup(p => p.ListNotApplications(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<Role>()))
                .Returns((string filters, string orders, int limit, int offset, Role role) =>
                {
                    List<Application> result = apps.Where(x => !apps_roles.Exists(y => y.Item2.Id == role.Id && y.Item1.Id == x.Id)).ToList();
                    return new ListResult<Application>(result, result.Count);
                });

            mockBusiness.Setup(p => p.InsertApplication(It.IsAny<Application>(), It.IsAny<Role>(), It.IsAny<User>())).
                Returns((Application app, Role role, User user) =>
                {
                    if (apps_roles.Exists(x => x.Item1.Id == app.Id && x.Item2.Id == role.Id))
                    {
                        throw new PersistentException();
                    }
                    else
                    {
                        apps_roles.Add(new Tuple<Application, Role>(app, role));
                        return app;
                    }
                });

            mockBusiness.Setup(p => p.ListUsers("", It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<Role>()))
                .Returns(new ListResult<User>(users_roles.Where(x => x.Item2.Id == 1).Select(x => x.Item1).ToList(), 1));

            mockBusiness.Setup(p => p.ListUsers("u.iduser = 2", It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<Role>()))
                .Returns(new ListResult<User>(new List<User>(), 0));

            mockBusiness.Setup(p => p.ListUsers("idusuario = 1", It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<Role>()))
                .Throws<PersistentException>();

            mockBusiness.Setup(p => p.ListNotUsers(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<Role>()))
                .Returns((string filters, string orders, int limit, int offset, Role role) =>
                {
                    List<User> result = users.Where(x => !users_roles.Exists(y => y.Item2.Id == role.Id && y.Item1.Id == x.Id)).ToList();
                    return new ListResult<User>(result, result.Count);
                });

            mockBusiness.Setup(p => p.InsertUser(It.IsAny<User>(), It.IsAny<Role>(), It.IsAny<User>())).
                Returns((User user, Role role, User user1) =>
                {
                    if (users_roles.Exists(x => x.Item1.Id == user.Id && x.Item2.Id == role.Id))
                    {
                        throw new PersistentException();
                    }
                    else
                    {
                        users_roles.Add(new Tuple<User, Role>(user, role));
                        return user;
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
        /// Prueba la consulta de un listado de roles con filtros, ordenamientos y límite
        /// </summary>
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
        [Fact]
        public void RoleReadTest()
        {
            Role role = _api.Read(1);
            Assert.Equal("Administradores", role.Name);
        }

        /// <summary>
        /// Prueba la consulta de un rol que no existe dado su identificador
        /// </summary>
        [Fact]
        public void RoleReadNotFoundTest()
        {
            Role role = _api.Read(10);
            Assert.Equal(0, role.Id);
        }

        /// <summary>
        /// Prueba la inserción de un rol
        /// </summary>
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
        [Fact]
        public void RoleInsertUserTest()
        {
            User role = _api.InsertUser(new() { Id = 2 }, 4);
            Assert.NotEqual(0, role.Id);
        }

        /// <summary>
        /// Prueba la inserción de un usuario de un rol duplicado
        /// </summary>
        [Fact]
        public void RoleInsertUserDuplicateTest()
        {
            User role = _api.InsertUser(new() { Id = 2 }, 1);
            Assert.Equal(0, role.Id);
        }

        /// <summary>
        /// Prueba la eliminación de un usuario de un rol
        /// </summary>
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
        [Fact]
        public void RoleInsertApplicationTest()
        {
            Application application = _api.InsertApplication(new() { Id = 2 }, 4);
            Assert.NotEqual(0, application.Id);
        }

        /// <summary>
        /// Prueba la inserción de una aplicación de un rol duplicado
        /// </summary>
        [Fact]
        public void RoleInsertApplicationDuplicateTest()
        {
            Application application = _api.InsertApplication(new() { Id = 2 }, 1);
            Assert.Equal(0, application.Id);
        }

        /// <summary>
        /// Prueba la eliminación de una aplicación de un rol
        /// </summary>
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

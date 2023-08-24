using Api.Controllers.Auth;
using Api.Dto;
using Business;
using Business.Auth;
using Business.Util;
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
    /// Realiza las pruebas sobre la clase de api de usuarios
    /// </summary>
    [Collection("Test")]
    public class UserTest
    {
        #region Attributes
        /// <summary>
        /// Configuración de la aplicación de pruebas
        /// </summary>
        private readonly IConfiguration _configuration;

        /// <summary>
        /// Controlador API para los usuarios
        /// </summary>
        private readonly UserController _api;

        /// <summary>
        /// Contexto HTTP con que se conecta a los servicios Rest
        /// </summary>
        private readonly ControllerContext _controllerContext;
        #endregion

        #region Constructors
        /// <summary>
        /// Inicializa la configuración de la prueba
        /// </summary>
        public UserTest()
        {
            Mock<IBusinessUser> mockBusiness = new();
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
                    ControllerName = "UserTest",
                    ActionName = "Test"
                }
            };
            _configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", false, false)
                .AddEnvironmentVariables()
                .Build();

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

            mockBusiness.Setup(p => p.List("iduser = 1", It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>()))
                .Returns(new ListResult<User>(users.Where(y => y.Id == 1).ToList(), 1));
            mockBusiness.Setup(p => p.List("idusuario = 1", It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>()))
                .Throws<PersistentException>();

            mockBusiness.Setup(p => p.Read(It.IsAny<User>()))
                .Returns((User user) => users.Find(x => x.Id == user.Id) ?? new User());

            mockBusiness.Setup(p => p.ReadByLoginAndPassword(It.IsAny<User>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns((User user, string password, string key, string iv) => users.Find(x => x.Login == user.Login && password == "FLWnwyoEz/7tYsnS+vxTVg==" && x.Active) ?? new User());

            mockBusiness.Setup(p => p.ReadByLogin(It.IsAny<User>()))
                .Returns((User user) => users.Find(x => x.Login == user.Login) ?? new User());

            mockBusiness.Setup(p => p.Insert(It.IsAny<User>(), It.IsAny<User>()))
                .Returns((User user, User user1) =>
                {
                    if (users.Exists(x => x.Login == user.Login))
                    {
                        throw new PersistentException();
                    }
                    else
                    {
                        user.Id = users.Count + 1;
                        users.Add(user);
                        return user;
                    }
                });

            mockBusiness.Setup(p => p.Update(It.IsAny<User>(), It.IsAny<User>()))
                .Returns((User user, User user1) =>
                {
                    users.Where(x => x.Id == user.Id).ToList().ForEach(x => { x.Login = user.Login; x.Name = user.Name; x.Active = user.Active; });
                    return user;
                });

            mockBusiness.Setup(p => p.UpdatePassword(It.IsAny<User>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<User>()))
                .Returns((User user, string password, string key, string iv, User user1) =>
                {
                    return user;
                });

            mockBusiness.Setup(p => p.Delete(It.IsAny<User>(), It.IsAny<User>()))
                .Returns((User user, User user1) =>
                {
                    users = users.Where(x => x.Id != user.Id).ToList();
                    return user;
                });

            mockBusiness.Setup(p => p.ListRoles("", It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<User>()))
                .Returns(new ListResult<Role>(users_roles.Where(x => x.Item1.Id == 1).Select(x => x.Item2).ToList(), 1));

            mockBusiness.Setup(p => p.ListRoles("r.idrole = 2", It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<User>()))
                .Returns(new ListResult<Role>(new List<Role>(), 0));

            mockBusiness.Setup(p => p.ListRoles("idusuario = 1", It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<User>()))
                .Throws<PersistentException>();

            mockBusiness.Setup(p => p.ListNotRoles(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<User>()))
                .Returns((string filters, string orders, int limit, int offset, User user) =>
                {
                    List<Role> result = roles.Where(x => !users_roles.Exists(y => y.Item1.Id == user.Id && y.Item2.Id == x.Id)).ToList();
                    return new ListResult<Role>(result, result.Count);
                });

            mockBusiness.Setup(p => p.InsertRole(It.IsAny<Role>(), It.IsAny<User>(), It.IsAny<User>())).
                Returns((Role role, User user, User user1) =>
                {
                    if (users_roles.Exists(x => x.Item1.Id == user.Id && x.Item2.Id == role.Id))
                    {
                        throw new PersistentException();
                    }
                    else
                    {
                        users_roles.Add(new Tuple<User, Role>(user, role));
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
        /// Prueba la consulta de un listado de usuarios con filtros, ordenamientos y límite
        /// </summary>
        [Fact]
        public void UserListTest()
        {
            ListResult<User> list = _api.List("iduser = 1", "name", 1, 0);

            Assert.NotEmpty(list.List);
            Assert.True(list.Total > 0);
        }

        /// <summary>
        /// Prueba la consulta de un listado de usuarios con filtros, ordenamientos y límite y con errores
        /// </summary>
        [Fact]
        public void UserListWithErrorTest()
        {
            ListResult<User> list = _api.List("idusuario = 1", "name", 1, 0);
            Assert.Equal(0, list.Total);
        }

        /// <summary>
        /// Prueba la consulta de un usuario dado su identificador
        /// </summary>
        [Fact]
        public void UserReadTest()
        {
            User user = _api.Read(1);
            Assert.Equal("leandrobaena@gmail.com", user.Login);
        }

        /// <summary>
        /// Prueba la consulta de un usuario que no existe dado su identificador
        /// </summary>
        [Fact]
        public void UserReadNotFoundTest()
        {
            User user = _api.Read(10);
            Assert.Equal(0, user.Id);
        }

        /// <summary>
        /// Prueba la inserción de un usuario
        /// </summary>
        [Fact]
        public void UserInsertTest()
        {
            User user = new() { Login = "insertado@prueba.com", Name = "Prueba 1", Active = true };
            user = _api.Insert(user);
            Assert.NotEqual(0, user.Id);
        }

        /// <summary>
        /// Prueba la inserción de un usuario con login duplicado
        /// </summary>
        [Fact]
        public void UserInsertDuplicateTest()
        {
            User user = new() { Login = "leandrobaena@gmail.com", Name = "Prueba insertar", Active = true };
            user = _api.Insert(user);
            Assert.Equal(0, user.Id);
        }

        /// <summary>
        /// Prueba la actualización de un usuario
        /// </summary>
        [Fact]
        public void UserUpdateTest()
        {
            User user = new() { Id = 2, Login = "otrologin@gmail.com", Name = "Prueba actualizar", Active = false };
            _ = _api.Update(user);

            User user2 = _api.Read(2);

            Assert.NotEqual("actualizame@gmail.com", user2.Name);
            Assert.False(user2.Active);
        }

        /// <summary>
        /// Prueba la eliminación de un usuario
        /// </summary>
        [Fact]
        public void UserDeleteTest()
        {
            _ = _api.Delete(3);

            User user2 = _api.Read(3);

            Assert.Equal(0, user2.Id);
        }

        /// <summary>
        /// Prueba la consulta de un usuario dado su login y contraseña
        /// </summary>
        [Fact]
        public void UserReadByLoginAndPasswordTest()
        {
            LoginResponse response = _api.ReadByLoginAndPassword(new() { Login = "leandrobaena@gmail.com", Password = "FLWnwyoEz/7tYsnS+vxTVg==" });
            Assert.True(response.Valid);
        }

        /// <summary>
        /// Prueba la consulta de un usuario inactivo dado su login y password
        /// </summary>
        [Fact]
        public void UserReadByLoginTest()
        {
            LoginResponse response = _api.ReadByLogin("leandrobaena@gmail.com");

            Assert.True(response.Valid);
        }

        /// <summary>
        /// Prueba la consulta de un usuario dado su login
        /// </summary>
        [Fact]
        public void UserReadByLoginAndPasswordInactiveTest()
        {
            LoginResponse response = _api.ReadByLoginAndPassword(new() { Login = "inactivo@gmail.com", Password = "FLWnwyoEz/7tYsnS+vxTVg==" });
            Assert.False(response.Valid);
        }

        /// <summary>
        /// Prueba la actualización de la contraseña de un usuario
        /// </summary>
        [Fact]
        public void UserUpdatePasswordTest()
        {
            ChangePasswordRequest request = new()
            {
                Password = "Prueba123",
                Token = Crypto.Encrypt("1~leandrobaena@gmail.com~" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), _configuration["Aes:Key"] ?? "", _configuration["Aes:IV"] ?? "")
            };
            _ = _api.UpdatePassword(request);

            LoginResponse response = _api.ReadByLoginAndPassword(new() { Login = "leandrobaena@gmail.com", Password = "FLWnwyoEz/7tYsnS+vxTVg==" });
            Assert.True(response.Valid);
        }

        /// <summary>
        /// Prueba la consulta de un listado de roles de un usuario con filtros, ordenamientos y límite
        /// </summary>
        [Fact]
        public void UserListRolesTest()
        {
            ListResult<Role> list = _api.ListRoles("", "", 10, 0, 1);

            Assert.NotEmpty(list.List);
            Assert.True(list.Total > 0);
        }

        /// <summary>
        /// Prueba la consulta de un listado de roles de un usuario con filtros, ordenamientos y límite y con errores
        /// </summary>
        [Fact]
        public void UserListRolesWithErrorTest()
        {
            ListResult<Role> list = _api.ListRoles("idusuario = 1", "", 10, 0, 1);

            Assert.Empty(list.List);
            Assert.Equal(0, list.Total);
        }

        /// <summary>
        /// Prueba la consulta de un listado de roles no asignados a un usuario con filtros, ordenamientos y límite
        /// </summary>
        [Fact]
        public void UserListNotRolesTest()
        {
            ListResult<Role> list = _api.ListNotRoles("", "", 10, 0, 1);

            Assert.NotEmpty(list.List);
            Assert.True(list.Total > 0);
        }

        /// <summary>
        /// Prueba la inserción de un rol de un usuario
        /// </summary>
        [Fact]
        public void UserInsertRoleTest()
        {
            Role role = _api.InsertRole(new() { Id = 4 }, 1);
            Assert.NotEqual(0, role.Id);
        }

        /// <summary>
        /// Prueba la inserción de un rol de un usuario duplicado
        /// </summary>
        [Fact]
        public void UserInsertRoleDuplicateTest()
        {
            Role role = _api.InsertRole(new() { Id = 1 }, 1);
            Assert.Equal(0, role.Id);
        }

        /// <summary>
        /// Prueba la eliminación de un rol de un usuario
        /// </summary>
        [Fact]
        public void UserDeleteRoleTest()
        {
            _ = _api.DeleteRole(2, 1);
            ListResult<Role> list = _api.ListRoles("r.idrole = 2", "", 10, 0, 1);
            Assert.Equal(0, list.Total);
        }
        #endregion
    }
}

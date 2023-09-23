using Api.Controllers.Auth;
using Api.Dto;
using Business.Auth;
using Business.Exceptions;
using Business.Util;
using Dal.Dto;
using Dal.Exceptions;
using Entities.Auth;
using Moq;
using System.Data;

namespace Api.Test.Auth
{
    /// <summary>
    /// Realiza las pruebas sobre la clase de api de usuarios
    /// </summary>
    [Collection("Test")]
    public class UserTest : TestBase<User>
    {
        #region Constructors
        /// <summary>
        /// Inicializa la configuración de la prueba
        /// </summary>
        public UserTest() : base()
        {
            //Arrange
            Mock<IBusinessUser> mockBusiness = new();

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
            mockBusiness.Setup(p => p.Read(It.IsAny<User>()))
                .Returns((User user) => users.Find(x => x.Id == user.Id) ?? new User());
            mockBusiness.Setup(p => p.ReadByLoginAndPassword(It.IsAny<User>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns((User user, string password, string key, string iv) =>
                {
                    if (user.Login == "error")
                    {
                        throw new BusinessException();
                    }
                    if (user.Login == "error2")
                    {
                        throw new PersistentException();
                    }
                    if (user.Login == "error3")
                    {
                        throw new Exception();
                    }
                    return users.Find(x => x.Login == user.Login && password == "FLWnwyoEz/7tYsnS+vxTVg==" && x.Active) ?? new User();
                });
            mockBusiness.Setup(p => p.ReadByLogin(It.IsAny<User>()))
                .Returns((User user) =>
                {
                    if (user.Login == "error")
                    {
                        throw new BusinessException();
                    }
                    if (user.Login == "error2")
                    {
                        throw new PersistentException();
                    }
                    if (user.Login == "error3")
                    {
                        throw new Exception();
                    }
                    return users.Find(x => x.Login == user.Login) ?? new User();
                });
            mockBusiness.Setup(p => p.UpdatePassword(It.IsAny<User>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<User>()))
                .Returns((User user, string password, string key, string iv, User user1) =>
                {
                    if (user.Id == -2)
                    {
                        throw new PersistentException();
                    }
                    return user;
                });
            mockBusiness.Setup(p => p.ListRoles("", It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<User>()))
                .Returns(new ListResult<Role>(users_roles.Where(x => x.Item1.Id == 1).Select(x => x.Item2).ToList(), 1));
            mockBusiness.Setup(p => p.ListRoles("idrole = 2", It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<User>()))
                .Returns(new ListResult<Role>(new List<Role>(), 0));
            mockBusiness.Setup(p => p.ListRoles("idusuario = 1", It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<User>()))
                .Throws<PersistentException>();
            mockBusiness.Setup(p => p.ListRoles("error", It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<User>()))
                .Throws<BusinessException>();
            mockBusiness.Setup(p => p.ListRoles("error1", It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<User>()))
                .Throws<Exception>();
            mockBusiness.Setup(p => p.ListNotRoles(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<User>()))
                .Returns((string filters, string orders, int limit, int offset, User user) =>
                {
                    if (user.Id == -1)
                    {
                        throw new BusinessException();
                    }
                    if (user.Id == -2)
                    {
                        throw new PersistentException();
                    }
                    if (user.Id == -3)
                    {
                        throw new Exception();
                    }
                    List<Role> result = roles.Where(x => !users_roles.Exists(y => y.Item1.Id == user.Id && y.Item2.Id == x.Id)).ToList();
                    return new ListResult<Role>(result, result.Count);
                });
            mockBusiness.Setup(p => p.InsertRole(It.IsAny<Role>(), It.IsAny<User>(), It.IsAny<User>())).
                Returns((Role role, User user, User user1) =>
                {
                    if (user.Id == -1)
                    {
                        throw new BusinessException();
                    }
                    if (user.Id == -3)
                    {
                        throw new Exception();
                    }
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
            mockBusiness.Setup(p => p.DeleteRole(It.IsAny<Role>(), It.IsAny<User>(), It.IsAny<User>())).
                Returns((Role role, User user, User user1) =>
                {
                    if (user.Id == -1)
                    {
                        throw new BusinessException();
                    }
                    if (user.Id == -2)
                    {
                        throw new PersistentException();
                    }
                    if (user.Id == -3)
                    {
                        throw new Exception();
                    }
                    return role;
                });
            api = new UserController(configuration, mockBusiness.Object, mockLog.Object, mockTemplate.Object, mockParameter.Object)
            {
                ControllerContext = controllerContext
            };
        }
        #endregion

        #region Methods
        /// <summary>
        /// Prueba la consulta de un usuario dado su identificador
        /// </summary>
        [Fact]
        public void ReadTest()
        {
            //Act
            User user = api != null ? api.Read(1) : new();

            //Assert
            Assert.Equal("leandrobaena@gmail.com", user.Login);
        }

        /// <summary>
        /// Prueba la consulta de un usuario dado su login y contraseña
        /// </summary>
        [Fact]
        public void ReadByLoginAndPasswordTest()
        {
            //Act
            LoginResponse response = api != null ? ((UserController)api).ReadByLoginAndPassword(new() { Login = "leandrobaena@gmail.com", Password = "FLWnwyoEz/7tYsnS+vxTVg==" }) : new();

            //Assert
            Assert.True(response.Valid);
        }

        /// <summary>
        /// Prueba la consulta de un usuario dado su login y contraseña con error de persistencia
        /// </summary>
        [Fact]
        public void ReadByLoginAndPasswordWithErrorTest()
        {
            //Act
            LoginResponse response = api != null ? ((UserController)api).ReadByLoginAndPassword(new() { Login = "error1", Password = "FLWnwyoEz/7tYsnS+vxTVg==" }) : new();

            //Assert
            Assert.False(response.Valid);
        }

        /// <summary>
        /// Prueba la consulta de un usuario dado su login y contraseña con error de negocio
        /// </summary>
        [Fact]
        public void ReadByLoginAndPasswordWithError2Test()
        {
            //Act
            LoginResponse response = api != null ? ((UserController)api).ReadByLoginAndPassword(new() { Login = "error", Password = "FLWnwyoEz/7tYsnS+vxTVg==" }) : new();

            //Assert
            Assert.False(response.Valid);
        }

        /// <summary>
        /// Prueba la consulta de un usuario dado su login y contraseña con error de persistencia
        /// </summary>
        [Fact]
        public void ReadByLoginAndPasswordWithError3Test()
        {
            //Act
            LoginResponse response = api != null ? ((UserController)api).ReadByLoginAndPassword(new() { Login = "error2", Password = "FLWnwyoEz/7tYsnS+vxTVg==" }) : new();

            //Assert
            Assert.False(response.Valid);
        }

        /// <summary>
        /// Prueba la consulta de un usuario dado su login y contraseña con error general
        /// </summary>
        [Fact]
        public void ReadByLoginAndPasswordWithError4Test()
        {
            //Act
            LoginResponse response = api != null ? ((UserController)api).ReadByLoginAndPassword(new() { Login = "error3", Password = "FLWnwyoEz/7tYsnS+vxTVg==" }) : new();

            //Assert
            Assert.False(response.Valid);
        }

        /// <summary>
        /// Prueba la consulta de un usuario dado su login
        /// </summary>
        [Fact]
        public void ReadByLoginAndPasswordInactiveTest()
        {
            //Act
            LoginResponse response = api != null ? ((UserController)api).ReadByLoginAndPassword(new() { Login = "inactivo@gmail.com", Password = "FLWnwyoEz/7tYsnS+vxTVg==" }) : new();

            //Assert
            Assert.False(response.Valid);
        }

        /// <summary>
        /// Prueba la consulta de un usuario dado su login
        /// </summary>
        [Fact]
        public void ReadByLoginTest()
        {
            //Act
            LoginResponse response = api != null ? ((UserController)api).ReadByLogin("leandrobaena@gmail.com") : new();

            //Assert
            Assert.True(response.Valid);
        }

        /// <summary>
        /// Prueba la consulta de un usuario dado su login con error de negocio
        /// </summary>
        [Fact]
        public void ReadByLoginWithErrorTest()
        {
            //Act
            LoginResponse response = api != null ? ((UserController)api).ReadByLogin("error") : new();

            //Assert
            Assert.False(response.Valid);
        }

        /// <summary>
        /// Prueba la consulta de un usuario dado su login con error de persistencia
        /// </summary>
        [Fact]
        public void ReadByLoginWithError2Test()
        {
            //Act
            LoginResponse response = api != null ? ((UserController)api).ReadByLogin("error2") : new();

            //Assert
            Assert.False(response.Valid);
        }

        /// <summary>
        /// Prueba la consulta de un usuario dado su login con error general
        /// </summary>
        [Fact]
        public void ReadByLoginWithError3Test()
        {
            //Act
            LoginResponse response = api != null ? ((UserController)api).ReadByLogin("error3") : new();

            //Assert
            Assert.False(response.Valid);
        }

        /// <summary>
        /// Prueba la actualización de la contraseña de un usuario
        /// </summary>
        [Fact]
        public void UpdatePasswordTest()
        {
            //Arrange
            ChangePasswordRequest request = new()
            {
                Password = "Prueba123",
                Token = Crypto.Encrypt("1~leandrobaena@gmail.com~" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), configuration["Aes:Key"] ?? "", configuration["Aes:IV"] ?? "")
            };

            //Act
            ChangePasswordResponse response = api != null ? ((UserController)api).UpdatePassword(request) : new();

            //Assert
            Assert.True(response.Success);
        }

        /// <summary>
        /// Prueba la actualización de la contraseña de un usuario con enlace caducado
        /// </summary>
        [Fact]
        public void UpdatePasswordExpiredTest()
        {
            //Arrange
            ChangePasswordRequest request = new()
            {
                Password = "Prueba123",
                Token = Crypto.Encrypt("1~leandrobaena@gmail.com~" + DateTime.Now.AddDays(-10).ToString("yyyy-MM-dd HH:mm:ss"), configuration["Aes:Key"] ?? "", configuration["Aes:IV"] ?? "")
            };

            //Act
            ChangePasswordResponse response = api != null ? ((UserController)api).UpdatePassword(request) : new();

            //Assert
            Assert.False(response.Success);
        }

        /// <summary>
        /// Prueba la actualización de la contraseña de un usuario con enlace con datos erróneos
        /// </summary>
        [Fact]
        public void UpdatePasswordErrorInDataTest()
        {
            //Arrange
            ChangePasswordRequest request = new()
            {
                Password = "Prueba123",
                Token = Crypto.Encrypt("0~leandrobaena@gmail.com~" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), configuration["Aes:Key"] ?? "", configuration["Aes:IV"] ?? "")
            };

            //Act
            ChangePasswordResponse response = api != null ? ((UserController)api).UpdatePassword(request) : new();

            //Assert
            Assert.False(response.Success);
        }

        /// <summary>
        /// Prueba la actualización de la contraseña de un usuario con enlace con error de negocio
        /// </summary>
        [Fact]
        public void UpdatePasswordErrorTest()
        {
            //Arrange
            ChangePasswordRequest request = new()
            {
                Password = "Prueba123",
                Token = Crypto.Encrypt("1~error~" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), configuration["Aes:Key"] ?? "", configuration["Aes:IV"] ?? "")
            };

            //Act
            ChangePasswordResponse response = api != null ? ((UserController)api).UpdatePassword(request) : new();

            //Assert
            Assert.False(response.Success);
        }

        /// <summary>
        /// Prueba la actualización de la contraseña de un usuario con enlace con error de persistencia
        /// </summary>
        [Fact]
        public void UpdatePasswordError2Test()
        {
            //Arrange
            ChangePasswordRequest request = new()
            {
                Password = "Prueba123",
                Token = Crypto.Encrypt("1~error2~" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), configuration["Aes:Key"] ?? "", configuration["Aes:IV"] ?? "")
            };

            //Act
            ChangePasswordResponse response = api != null ? ((UserController)api).UpdatePassword(request) : new();

            //Assert
            Assert.False(response.Success);
        }

        /// <summary>
        /// Prueba la actualización de la contraseña de un usuario con enlace con error general
        /// </summary>
        [Fact]
        public void UpdatePasswordError3Test()
        {
            //Arrange
            ChangePasswordRequest request = new()
            {
                Password = "Prueba123",
                Token = Crypto.Encrypt("1~error3~" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), configuration["Aes:Key"] ?? "", configuration["Aes:IV"] ?? "")
            };

            //Act
            ChangePasswordResponse response = api != null ? ((UserController)api).UpdatePassword(request) : new();

            //Assert
            Assert.False(response.Success);
        }

        /// <summary>
        /// Prueba la consulta de un listado de roles de un usuario con filtros, ordenamientos y límite
        /// </summary>
        [Fact]
        public void ListRolesTest()
        {
            //Act
            ListResult<Role> list = api != null ? ((UserController)api).ListRoles("", "", 10, 0, 1) : new ListResult<Role>(new List<Role>(), 0);

            //Assert
            Assert.NotEmpty(list.List);
            Assert.True(list.Total > 0);
        }

        /// <summary>
        /// Prueba la consulta de un listado de roles de un usuario con filtros, ordenamientos y límite y con errores
        /// </summary>
        [Fact]
        public void ListRolesWithErrorTest()
        {
            //Act
            ListResult<Role> list = api != null ? ((UserController)api).ListRoles("idusuario = 1", "", 10, 0, 1) : new ListResult<Role>(new List<Role>(), 0);

            //Assert
            Assert.Empty(list.List);
            Assert.Equal(0, list.Total);
        }

        /// <summary>
        /// Prueba la consulta de un listado de roles de un usuario con filtros, ordenamientos y límite y con errores de negocio
        /// </summary>
        [Fact]
        public void ListRolesWithError2Test()
        {
            //Act
            ListResult<Role> list = api != null ? ((UserController)api).ListRoles("error", "", 10, 0, 1) : new ListResult<Role>(new List<Role>(), 0);

            //Assert
            Assert.Empty(list.List);
            Assert.Equal(0, list.Total);
        }

        /// <summary>
        /// Prueba la consulta de un listado de roles de un usuario con filtros, ordenamientos y límite y con errores generales
        /// </summary>
        [Fact]
        public void ListRolesWithError3Test()
        {
            //Act
            ListResult<Role> list = api != null ? ((UserController)api).ListRoles("error1", "", 10, 0, 1) : new ListResult<Role>(new List<Role>(), 0);

            //Assert
            Assert.Empty(list.List);
            Assert.Equal(0, list.Total);
        }

        /// <summary>
        /// Prueba la consulta de un listado de roles no asignados a un usuario con filtros, ordenamientos y límite
        /// </summary>
        [Fact]
        public void ListNotRolesTest()
        {
            //Act
            ListResult<Role> list = api != null ? ((UserController)api).ListNotRoles("", "", 10, 0, 1) : new ListResult<Role>(new List<Role>(), 0);

            //Assert
            Assert.NotEmpty(list.List);
            Assert.True(list.Total > 0);
        }

        /// <summary>
        /// Prueba la consulta de un listado de roles no asignados a un usuario con filtros, ordenamientos y límite con error de negocio
        /// </summary>
        [Fact]
        public void ListNotRolesWithErrorTest()
        {
            //Act
            ListResult<Role> list = api != null ? ((UserController)api).ListNotRoles("", "", 10, 0, -1) : new ListResult<Role>(new List<Role>(), 0);

            //Assert
            Assert.Empty(list.List);
            Assert.Equal(0, list.Total);
        }

        /// <summary>
        /// Prueba la consulta de un listado de roles no asignados a un usuario con filtros, ordenamientos y límite con error de persistencia
        /// </summary>
        [Fact]
        public void ListNotRolesWithError2Test()
        {
            //Act
            ListResult<Role> list = api != null ? ((UserController)api).ListNotRoles("", "", 10, 0, -2) : new ListResult<Role>(new List<Role>(), 0);

            //Assert
            Assert.Empty(list.List);
            Assert.Equal(0, list.Total);
        }

        /// <summary>
        /// Prueba la consulta de un listado de roles no asignados a un usuario con filtros, ordenamientos y límite con error general
        /// </summary>
        [Fact]
        public void ListNotRolesWithError3Test()
        {
            //Act
            ListResult<Role> list = api != null ? ((UserController)api).ListNotRoles("", "", 10, 0, -3) : new ListResult<Role>(new List<Role>(), 0);

            //Assert
            Assert.Empty(list.List);
            Assert.Equal(0, list.Total);
        }

        /// <summary>
        /// Prueba la inserción de un rol de un usuario
        /// </summary>
        [Fact]
        public void InsertRoleTest()
        {
            //Act
            Role role = api != null ? ((UserController)api).InsertRole(new() { Id = 4 }, 1) : new();

            //Assert
            Assert.NotEqual(0, role.Id);
        }

        /// <summary>
        /// Prueba la inserción de un rol de un usuario duplicado
        /// </summary>
        [Fact]
        public void InsertRoleDuplicateTest()
        {
            //Act
            Role role = api != null ? ((UserController)api).InsertRole(new() { Id = 1 }, 1) : new();

            //Assert
            Assert.Equal(0, role.Id);
        }

        /// <summary>
        /// Prueba la inserción de un rol con error de negocio
        /// </summary>
        [Fact]
        public void InsertRoleWithErrorTest()
        {
            //Act
            Role role = api != null ? ((UserController)api).InsertRole(new() { Id = 1 }, -1) : new();

            //Assert
            Assert.Equal(0, role.Id);
        }

        /// <summary>
        /// Prueba la inserción de un rol con error general
        /// </summary>
        [Fact]
        public void InsertRoleWithError2Test()
        {
            //Act
            Role role = api != null ? ((UserController)api).InsertRole(new() { Id = 1 }, -3) : new();

            //Assert
            Assert.Equal(0, role.Id);
        }

        /// <summary>
        /// Prueba la eliminación de un rol de un usuario
        /// </summary>
        [Fact]
        public void DeleteRoleTest()
        {
            //Act
            _ = api != null ? ((UserController)api).DeleteRole(2, 1) : new();
            ListResult<Role> list = api != null ? ((UserController)api).ListRoles("idrole = 2", "", 10, 0, 1) : new ListResult<Role>(new List<Role>(), 0);

            //Assert
            Assert.Equal(0, list.Total);
        }

        /// <summary>
        /// Prueba la eliminación de un rol de un usuario con error de negocio
        /// </summary>
        [Fact]
        public void DeleteRoleWithErrorTest()
        {
            //Act
            Role role = api != null ? ((UserController)api).DeleteRole(2, -1) : new();

            //Assert
            Assert.Equal(0, role.Id);
        }

        /// <summary>
        /// Prueba la eliminación de un rol de un usuario con error de persistencia
        /// </summary>
        [Fact]
        public void DeleteRoleWithError2Test()
        {
            //Act
            Role role = api != null ? ((UserController)api).DeleteRole(2, -2) : new();

            //Assert
            Assert.Equal(0, role.Id);
        }

        /// <summary>
        /// Prueba la eliminación de un rol de un usuario con error general
        /// </summary>
        [Fact]
        public void DeleteRoleWithError3Test()
        {
            //Act
            Role role = api != null ? ((UserController)api).DeleteRole(2, -3) : new();

            //Assert
            Assert.Equal(0, role.Id);
        }
        #endregion
    }
}

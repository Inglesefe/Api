﻿using Api.Controllers.Auth;
using Api.Dto;
using Business.Auth;
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

            api = new UserController(configuration, mockBusiness.Object, mockLog.Object, mockTemplate.Object, mockParameter.Object)
            {
                ControllerContext = controllerContext
            };
        }
        #endregion

        #region Methods
        /// <summary>
        /// Prueba la consulta de un listado de usuarios con filtros, ordenamientos y límite
        /// </summary>
        [Fact]
        public void ListTest()
        {
            //Act
            ListResult<User> list = api.List("iduser = 1", "name", 1, 0);

            //Assert
            Assert.NotEmpty(list.List);
            Assert.True(list.Total > 0);
        }

        /// <summary>
        /// Prueba la consulta de un listado de usuarios con filtros, ordenamientos y límite y con errores
        /// </summary>
        [Fact]
        public void ListWithErrorTest()
        {
            //Act
            ListResult<User> list = api.List("idusuario = 1", "name", 1, 0);

            //Assert
            Assert.Equal(0, list.Total);
        }

        /// <summary>
        /// Prueba la consulta de un usuario dado su identificador
        /// </summary>
        [Fact]
        public void ReadTest()
        {
            //Act
            User user = api.Read(1);

            //Assert
            Assert.Equal("leandrobaena@gmail.com", user.Login);
        }

        /// <summary>
        /// Prueba la consulta de un usuario que no existe dado su identificador
        /// </summary>
        [Fact]
        public void ReadNotFoundTest()
        {
            //Act
            User user = api.Read(10);

            //Assert
            Assert.Equal(0, user.Id);
        }

        /// <summary>
        /// Prueba la inserción de un usuario
        /// </summary>
        [Fact]
        public void InsertTest()
        {
            //Arrange
            User user = new() { Login = "insertado@prueba.com", Name = "Prueba 1", Active = true };

            //Act
            user = api.Insert(user);

            //Assert
            Assert.NotEqual(0, user.Id);
        }

        /// <summary>
        /// Prueba la inserción de un usuario con login duplicado
        /// </summary>
        [Fact]
        public void InsertDuplicateTest()
        {
            //Arrange
            User user = new() { Login = "leandrobaena@gmail.com", Name = "Prueba insertar", Active = true };

            //Act
            user = api.Insert(user);

            //Assert
            Assert.Equal(0, user.Id);
        }

        /// <summary>
        /// Prueba la actualización de un usuario
        /// </summary>
        [Fact]
        public void UpdateTest()
        {
            //Arrange
            User user = new() { Id = 2, Login = "otrologin@gmail.com", Name = "Prueba actualizar", Active = false };

            //Act
            _ = api.Update(user);
            User user2 = api.Read(2);

            //Assert
            Assert.NotEqual("actualizame@gmail.com", user2.Name);
            Assert.False(user2.Active);
        }

        /// <summary>
        /// Prueba la eliminación de un usuario
        /// </summary>
        [Fact]
        public void DeleteTest()
        {
            //Act
            _ = api.Delete(3);
            User user2 = api.Read(3);

            //Assert
            Assert.Equal(0, user2.Id);
        }

        /// <summary>
        /// Prueba la consulta de un usuario dado su login y contraseña
        /// </summary>
        [Fact]
        public void ReadByLoginAndPasswordTest()
        {
            //Act
            LoginResponse response = ((UserController)api).ReadByLoginAndPassword(new() { Login = "leandrobaena@gmail.com", Password = "FLWnwyoEz/7tYsnS+vxTVg==" });

            //Assert
            Assert.True(response.Valid);
        }

        /// <summary>
        /// Prueba la consulta de un usuario inactivo dado su login y password
        /// </summary>
        [Fact]
        public void ReadByLoginTest()
        {
            //Act
            LoginResponse response = ((UserController)api).ReadByLogin("leandrobaena@gmail.com");

            //Assert
            Assert.True(response.Valid);
        }

        /// <summary>
        /// Prueba la consulta de un usuario dado su login
        /// </summary>
        [Fact]
        public void ReadByLoginAndPasswordInactiveTest()
        {
            //Act
            LoginResponse response = ((UserController)api).ReadByLoginAndPassword(new() { Login = "inactivo@gmail.com", Password = "FLWnwyoEz/7tYsnS+vxTVg==" });

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
            _ = ((UserController)api).UpdatePassword(request);

            //Assert
            LoginResponse response = ((UserController)api).ReadByLoginAndPassword(new() { Login = "leandrobaena@gmail.com", Password = "FLWnwyoEz/7tYsnS+vxTVg==" });
            Assert.True(response.Valid);
        }

        /// <summary>
        /// Prueba la consulta de un listado de roles de un usuario con filtros, ordenamientos y límite
        /// </summary>
        [Fact]
        public void ListRolesTest()
        {
            //Act
            ListResult<Role> list = ((UserController)api).ListRoles("", "", 10, 0, 1);

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
            ListResult<Role> list = ((UserController)api).ListRoles("idusuario = 1", "", 10, 0, 1);

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
            ListResult<Role> list = ((UserController)api).ListNotRoles("", "", 10, 0, 1);

            //Assert
            Assert.NotEmpty(list.List);
            Assert.True(list.Total > 0);
        }

        /// <summary>
        /// Prueba la inserción de un rol de un usuario
        /// </summary>
        [Fact]
        public void InsertRoleTest()
        {
            //Act
            Role role = ((UserController)api).InsertRole(new() { Id = 4 }, 1);

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
            Role role = ((UserController)api).InsertRole(new() { Id = 1 }, 1);

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
            _ = ((UserController)api).DeleteRole(2, 1);
            ListResult<Role> list = ((UserController)api).ListRoles("r.idrole = 2", "", 10, 0, 1);

            //Assert
            Assert.Equal(0, list.Total);
        }
        #endregion
    }
}

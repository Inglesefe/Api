using Api.Controllers.Auth;
using Business.Auth;
using Business.Exceptions;
using Dal.Dto;
using Dal.Exceptions;
using Entities.Auth;
using Moq;
using System.Data;

namespace Api.Test.Auth
{
    /// <summary>
    /// Realiza las pruebas sobre la clase de api de roles
    /// </summary>
    [Collection("Test")]
    public class RoleTest : TestBase<Role>
    {
        #region Constructors
        /// <summary>
        /// Inicializa la configuración de la prueba
        /// </summary>
        public RoleTest() : base()
        {
            //Arrange
            Mock<IBusinessRole> mockBusiness = new();

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
            mockBusiness.Setup(p => p.Read(It.IsAny<Role>()))
                .Returns((Role role) => roles.Find(x => x.Id == role.Id) ?? new Role());
            mockBusiness.Setup(p => p.ListApplications("", It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<Role>()))
                .Returns(new ListResult<Application>(apps_roles.Where(x => x.Item2.Id == 1).Select(x => x.Item1).ToList(), 1));
            mockBusiness.Setup(p => p.ListApplications("a.idapplication = 2", It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<Role>()))
                .Returns(new ListResult<Application>(new List<Application>(), 0));
            mockBusiness.Setup(p => p.ListApplications("idaplicacion = 1", It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<Role>()))
                .Throws<PersistentException>();
            mockBusiness.Setup(p => p.ListApplications("error", It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<Role>()))
                .Throws<BusinessException>();
            mockBusiness.Setup(p => p.ListApplications("error1", It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<Role>()))
                .Throws<Exception>();
            mockBusiness.Setup(p => p.ListNotApplications(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<Role>()))
                .Returns((string filters, string orders, int limit, int offset, Role role) =>
                {
                    if (role.Id == -1)
                    {
                        throw new BusinessException();
                    }
                    if (role.Id == -2)
                    {
                        throw new PersistentException();
                    }
                    if (role.Id == -3)
                    {
                        throw new Exception();
                    }
                    List<Application> result = apps.Where(x => !apps_roles.Exists(y => y.Item2.Id == role.Id && y.Item1.Id == x.Id)).ToList();
                    return new ListResult<Application>(result, result.Count);
                });
            mockBusiness.Setup(p => p.InsertApplication(It.IsAny<Application>(), It.IsAny<Role>(), It.IsAny<User>())).
                Returns((Application app, Role role, User user) =>
                {
                    if (role.Id == -1)
                    {
                        throw new BusinessException();
                    }
                    if (role.Id == -3)
                    {
                        throw new Exception();
                    }
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
            mockBusiness.Setup(p => p.DeleteApplication(It.IsAny<Application>(), It.IsAny<Role>(), It.IsAny<User>())).
                Returns((Application app, Role role, User user) =>
                {
                    if (role.Id == -1)
                    {
                        throw new BusinessException();
                    }
                    if (role.Id == -2)
                    {
                        throw new PersistentException();
                    }
                    if (role.Id == -3)
                    {
                        throw new Exception();
                    }
                    return app;
                });
            mockBusiness.Setup(p => p.ListUsers("", It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<Role>()))
                .Returns(new ListResult<User>(users_roles.Where(x => x.Item2.Id == 1).Select(x => x.Item1).ToList(), 1));
            mockBusiness.Setup(p => p.ListUsers("u.iduser = 2", It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<Role>()))
                .Returns(new ListResult<User>(new List<User>(), 0));
            mockBusiness.Setup(p => p.ListUsers("idusuario = 1", It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<Role>()))
                .Throws<PersistentException>();
            mockBusiness.Setup(p => p.ListUsers("error", It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<Role>()))
                .Throws<BusinessException>();
            mockBusiness.Setup(p => p.ListUsers("error1", It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<Role>()))
                .Throws<Exception>();
            mockBusiness.Setup(p => p.ListNotUsers(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<Role>()))
                .Returns((string filters, string orders, int limit, int offset, Role role) =>
                {
                    if (role.Id == -1)
                    {
                        throw new BusinessException();
                    }
                    if (role.Id == -2)
                    {
                        throw new PersistentException();
                    }
                    if (role.Id == -3)
                    {
                        throw new Exception();
                    }
                    List<User> result = users.Where(x => !users_roles.Exists(y => y.Item2.Id == role.Id && y.Item1.Id == x.Id)).ToList();
                    return new ListResult<User>(result, result.Count);
                });
            mockBusiness.Setup(p => p.InsertUser(It.IsAny<User>(), It.IsAny<Role>(), It.IsAny<User>())).
                Returns((User user, Role role, User user1) =>
                {
                    if (role.Id == -1)
                    {
                        throw new BusinessException();
                    }
                    if (role.Id == -3)
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
                        return user;
                    }
                });
            mockBusiness.Setup(p => p.DeleteUser(It.IsAny<User>(), It.IsAny<Role>(), It.IsAny<User>())).
                Returns((User user, Role role, User user1) =>
                {
                    if (role.Id == -1)
                    {
                        throw new BusinessException();
                    }
                    if (role.Id == -2)
                    {
                        throw new PersistentException();
                    }
                    if (role.Id == -3)
                    {
                        throw new Exception();
                    }
                    return user;
                });
            api = new RoleController(configuration, mockBusiness.Object, mockLog.Object, mockTemplate.Object, mockParameter.Object)
            {
                ControllerContext = controllerContext
            };
        }
        #endregion

        #region Methods
        /// <summary>
        /// Prueba la consulta de un rol dado su identificador
        /// </summary>
        [Fact]
        public void ReadTest()
        {
            //Act
            Role role = api != null ? api.Read(1) : new();

            //Assert
            Assert.Equal("Administradores", role.Name);
        }

        /// <summary>
        /// Prueba la consulta de un listado de usuarios de un rol con filtros, ordenamientos y límite
        /// </summary>
        [Fact]
        public void ListUsersTest()
        {
            //Act
            ListResult<User> list = api != null ? ((RoleController)api).ListUsers("", "", 10, 0, 1) : new ListResult<User>(new List<User>(), 0);

            //Assert
            Assert.NotEmpty(list.List);
            Assert.True(list.Total > 0);
        }

        /// <summary>
        /// Prueba la consulta de un listado de usuarios de un rol con filtros, ordenamientos y límite y con errores
        /// </summary>
        [Fact]
        public void ListUsersWithErrorTest()
        {
            //Act
            ListResult<User> list = api != null ? ((RoleController)api).ListUsers("idusuario = 1", "", 10, 0, 1) : new ListResult<User>(new List<User>(), 0);

            //Assert
            Assert.Empty(list.List);
            Assert.Equal(0, list.Total);
        }

        /// <summary>
        /// Prueba la consulta de un listado de usuarios de un rol con filtros, ordenamientos y límite y con errores de negocio
        /// </summary>
        [Fact]
        public void ListUsersWithError2Test()
        {
            //Act
            ListResult<User> list = api != null ? ((RoleController)api).ListUsers("error", "", 10, 0, 1) : new ListResult<User>(new List<User>(), 0);

            //Assert
            Assert.Empty(list.List);
            Assert.Equal(0, list.Total);
        }

        /// <summary>
        /// Prueba la consulta de un listado de usuarios de un rol con filtros, ordenamientos y límite y con error en general
        /// </summary>
        [Fact]
        public void ListUsersWithError3Test()
        {
            //Act
            ListResult<User> list = api != null ? ((RoleController)api).ListUsers("error1", "", 10, 0, 1) : new ListResult<User>(new List<User>(), 0);

            //Assert
            Assert.Empty(list.List);
            Assert.Equal(0, list.Total);
        }

        /// <summary>
        /// Prueba la consulta de un listado de usuarios no asignados a un rol con filtros, ordenamientos y límite
        /// </summary>
        [Fact]
        public void ListNotUsersTest()
        {
            //Act
            ListResult<User> list = api != null ? ((RoleController)api).ListNotUsers("", "", 10, 0, 1) : new ListResult<User>(new List<User>(), 0);

            //Assert
            Assert.NotEmpty(list.List);
            Assert.True(list.Total > 0);
        }

        /// <summary>
        /// Prueba la consulta de un listado de usuarios no asignados a un rol con filtros, ordenamientos y límite con error de persistencia
        /// </summary>
        [Fact]
        public void ListNotUsersWithErrorTest()
        {
            //Act
            ListResult<User> list = api != null ? ((RoleController)api).ListNotUsers("", "", 10, 0, -2) : new ListResult<User>(new List<User>(), 0);

            //Assert
            Assert.Empty(list.List);
            Assert.Equal(0, list.Total);
        }

        /// <summary>
        /// Prueba la consulta de un listado de usuarios no asignados a un rol con filtros, ordenamientos y límite con error de negocio
        /// </summary>
        [Fact]
        public void ListNotUsersWithError2Test()
        {
            //Act
            ListResult<User> list = api != null ? ((RoleController)api).ListNotUsers("", "", 10, 0, -1) : new ListResult<User>(new List<User>(), 0);

            //Assert
            Assert.Empty(list.List);
            Assert.Equal(0, list.Total);
        }

        /// <summary>
        /// Prueba la consulta de un listado de usuarios no asignados a un rol con filtros, ordenamientos y límite con error en general
        /// </summary>
        [Fact]
        public void ListNotUsersWithError3Test()
        {
            //Act
            ListResult<User> list = api != null ? ((RoleController)api).ListNotUsers("", "", 10, 0, -3) : new ListResult<User>(new List<User>(), 0);

            //Assert
            Assert.Empty(list.List);
            Assert.Equal(0, list.Total);
        }

        /// <summary>
        /// Prueba la inserción de un usuario de un rol
        /// </summary>
        [Fact]
        public void InsertUserTest()
        {
            //Act
            User role = api != null ? ((RoleController)api).InsertUser(new() { Id = 2 }, 4) : new();

            //Assert
            Assert.NotEqual(0, role.Id);
        }

        /// <summary>
        /// Prueba la inserción de un usuario de un rol duplicado
        /// </summary>
        [Fact]
        public void InsertUserDuplicateTest()
        {
            //Act
            User role = api != null ? ((RoleController)api).InsertUser(new() { Id = 2 }, 1) : new();

            //Assert
            Assert.Equal(0, role.Id);
        }

        /// <summary>
        /// Prueba la inserción de un usuario de un rol con error de negocio
        /// </summary>
        [Fact]
        public void InsertUserWithErrorTest()
        {
            //Act
            User role = api != null ? ((RoleController)api).InsertUser(new() { Id = 2 }, -1) : new();

            //Assert
            Assert.Equal(0, role.Id);
        }

        /// <summary>
        /// Prueba la inserción de un usuario de un rol con error general
        /// </summary>
        [Fact]
        public void InsertUserWithError2Test()
        {
            //Act
            User role = api != null ? ((RoleController)api).InsertUser(new() { Id = 2 }, -3) : new();

            //Assert
            Assert.Equal(0, role.Id);
        }

        /// <summary>
        /// Prueba la eliminación de un usuario de un rol
        /// </summary>
        [Fact]
        public void DeleteUserTest()
        {
            //Act
            _ = api != null ? ((RoleController)api).DeleteUser(2, 2) : new();
            ListResult<User> list = api != null ? ((RoleController)api).ListUsers("u.iduser = 2", "", 10, 0, 2) : new ListResult<User>(new List<User>(), 0);

            //Assert
            Assert.Equal(0, list.Total);
        }

        /// <summary>
        /// Prueba la eliminación de un usuario de un rol con error de persistencia
        /// </summary>
        [Fact]
        public void DeleteUserWithErrorTest()
        {
            //Act
            User user = api != null ? ((RoleController)api).DeleteUser(2, -2) : new();

            //Assert
            Assert.Equal(0, user.Id);
        }

        /// <summary>
        /// Prueba la eliminación de un usuario de un rol con error de negocio
        /// </summary>
        [Fact]
        public void DeleteUserWithError2Test()
        {
            //Act
            User user = api != null ? ((RoleController)api).DeleteUser(2, -1) : new();

            //Assert
            Assert.Equal(0, user.Id);
        }

        /// <summary>
        /// Prueba la eliminación de un usuario de un rol con error general
        /// </summary>
        [Fact]
        public void DeleteUserWithError3Test()
        {
            //Act
            User user = api != null ? ((RoleController)api).DeleteUser(2, -3) : new();

            //Assert
            Assert.Equal(0, user.Id);
        }

        /// <summary>
        /// Prueba la consulta de un listado de aplicaciones de un rol con filtros, ordenamientos y límite
        /// </summary>
        [Fact]
        public void ListApplicationsTest()
        {
            //Act
            ListResult<Application> list = api != null ? ((RoleController)api).ListApplications("", "", 10, 0, 1) : new ListResult<Application>(new List<Application>(), 0);

            //Assert
            Assert.NotEmpty(list.List);
            Assert.True(list.Total > 0);
        }

        /// <summary>
        /// Prueba la consulta de un listado de aplicaciones de un rol con filtros, ordenamientos y límite y con errores
        /// </summary>
        [Fact]
        public void ListApplicationsWithErrorTest()
        {
            //Act
            ListResult<Application> list = api != null ? ((RoleController)api).ListApplications("idaplicacion = 1", "", 10, 0, 1) : new ListResult<Application>(new List<Application>(), 0);

            //Assert
            Assert.Empty(list.List);
            Assert.Equal(0, list.Total);
        }

        /// <summary>
        /// Prueba la consulta de un listado de aplicaciones de un rol con filtros, ordenamientos y límite y con errores de negocio
        /// </summary>
        [Fact]
        public void ListApplicationsWithError2Test()
        {
            //Act
            ListResult<Application> list = api != null ? ((RoleController)api).ListApplications("idaplicacion = 1", "", 10, 0, -1) : new ListResult<Application>(new List<Application>(), 0);

            //Assert
            Assert.Empty(list.List);
            Assert.Equal(0, list.Total);
        }

        /// <summary>
        /// Prueba la consulta de un listado de aplicaciones de un rol con filtros, ordenamientos y límite y con errores en general
        /// </summary>
        [Fact]
        public void ListApplicationsWithError3Test()
        {
            //Act
            ListResult<Application> list = api != null ? ((RoleController)api).ListApplications("idaplicacion = 1", "", 10, 0, -3) : new ListResult<Application>(new List<Application>(), 0);

            //Assert
            Assert.Empty(list.List);
            Assert.Equal(0, list.Total);
        }

        /// <summary>
        /// Prueba la consulta de un listado de aplicaciones no asignadas a un rol con filtros, ordenamientos y límite
        /// </summary>
        [Fact]
        public void ListNotApplicationsTest()
        {
            //Act
            ListResult<Application> list = api != null ? ((RoleController)api).ListNotApplications("", "", 10, 0, 1) : new ListResult<Application>(new List<Application>(), 0);

            //Assert
            Assert.NotEmpty(list.List);
            Assert.True(list.Total > 0);
        }

        /// <summary>
        /// Prueba la consulta de un listado de aplicaciones no asignadas a un rol con filtros, ordenamientos y límite con errores de persistencia
        /// </summary>
        [Fact]
        public void ListNotApplicationsWithErrorTest()
        {
            //Act
            ListResult<Application> list = api != null ? ((RoleController)api).ListNotApplications("", "", 10, 0, -2) : new ListResult<Application>(new List<Application>(), 0);

            //Assert
            Assert.Empty(list.List);
            Assert.Equal(0, list.Total);
        }

        /// <summary>
        /// Prueba la consulta de un listado de aplicaciones no asignadas a un rol con filtros, ordenamientos y límite con errores de negocio
        /// </summary>
        [Fact]
        public void ListNotApplicationsWithError2Test()
        {
            //Act
            ListResult<Application> list = api != null ? ((RoleController)api).ListNotApplications("", "", 10, 0, -1) : new ListResult<Application>(new List<Application>(), 0);

            //Assert
            Assert.Empty(list.List);
            Assert.Equal(0, list.Total);
        }

        /// <summary>
        /// Prueba la consulta de un listado de aplicaciones no asignadas a un rol con filtros, ordenamientos y límite con errores genrales
        /// </summary>
        [Fact]
        public void ListNotApplicationsWithError3Test()
        {
            //Act
            ListResult<Application> list = api != null ? ((RoleController)api).ListNotApplications("", "", 10, 0, -3) : new ListResult<Application>(new List<Application>(), 0);

            //Assert
            Assert.Empty(list.List);
            Assert.Equal(0, list.Total);
        }

        /// <summary>
        /// Prueba la inserción de una aplicación de un rol
        /// </summary>
        [Fact]
        public void InsertApplicationTest()
        {
            //Act
            Application application = api != null ? ((RoleController)api).InsertApplication(new() { Id = 2 }, 4) : new();

            //Assert
            Assert.NotEqual(0, application.Id);
        }

        /// <summary>
        /// Prueba la inserción de una aplicación de un rol duplicado
        /// </summary>
        [Fact]
        public void InsertApplicationDuplicateTest()
        {
            //Act
            Application application = api != null ? ((RoleController)api).InsertApplication(new() { Id = 2 }, 1) : new();

            //Assert
            Assert.Equal(0, application.Id);
        }

        /// <summary>
        /// Prueba la inserción de una aplicación con erro de negocio
        /// </summary>
        [Fact]
        public void InsertApplicationWithErrorTest()
        {
            //Act
            Application application = api != null ? ((RoleController)api).InsertApplication(new() { Id = 2 }, -1) : new();

            //Assert
            Assert.Equal(0, application.Id);
        }

        /// <summary>
        /// Prueba la inserción de una aplicación con erro general
        /// </summary>
        [Fact]
        public void InsertApplicationWithError2Test()
        {
            //Act
            Application application = api != null ? ((RoleController)api).InsertApplication(new() { Id = 2 }, -3) : new();

            //Assert
            Assert.Equal(0, application.Id);
        }

        /// <summary>
        /// Prueba la eliminación de una aplicación de un rol
        /// </summary>
        [Fact]
        public void DeleteApplicationTest()
        {
            //Act
            _ = api != null ? ((RoleController)api).DeleteApplication(2, 2) : new();
            ListResult<Application> list = api != null ? ((RoleController)api).ListApplications("a.idapplication = 2", "", 10, 0, 2) : new ListResult<Application>(new List<Application>(), 0);

            //Assert
            Assert.Equal(0, list.Total);
        }

        /// <summary>
        /// Prueba la eliminación de una aplicación de un rol con error de persistencia
        /// </summary>
        [Fact]
        public void DeleteApplicationWithErrorTest()
        {
            //Act
            Application app = api != null ? ((RoleController)api).DeleteApplication(2, -2) : new();

            //Assert
            Assert.Equal(0, app.Id);
        }

        /// <summary>
        /// Prueba la eliminación de una aplicación de un rol con error de negocio
        /// </summary>
        [Fact]
        public void DeleteApplicationWithError2Test()
        {
            //Act
            Application app = api != null ? ((RoleController)api).DeleteApplication(2, -1) : new();

            //Assert
            Assert.Equal(0, app.Id);
        }

        /// <summary>
        /// Prueba la eliminación de una aplicación de un rol con error de persistencia
        /// </summary>
        [Fact]
        public void DeleteApplicationWithError3Test()
        {
            //Act
            Application app = api != null ? ((RoleController)api).DeleteApplication(2, -3) : new();

            //Assert
            Assert.Equal(0, app.Id);
        }
        #endregion
    }
}

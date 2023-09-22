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
    /// Realiza las pruebas sobre la api de aplicaciones
    /// </summary>
    [Collection("Test")]
    public class ApplicationTest : TestBase<Application>
    {
        #region Constructors
        /// <summary>
        /// Inicializa la configuración de la prueba
        /// </summary>
        public ApplicationTest() : base()
        {
            //Arrange
            Mock<IBusinessApplication> mockBusiness = new();

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
            mockBusiness.Setup(p => p.List("idapplication = 1", It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>()))
                .Returns(new ListResult<Application>(apps.Where(y => y.Id == 1).ToList(), 1));
            mockBusiness.Setup(p => p.List("idaplicacion = 1", It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>()))
                .Throws<PersistentException>();
            mockBusiness.Setup(p => p.List("error", It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>()))
                .Throws<BusinessException>();
            mockBusiness.Setup(p => p.List("error1", It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>()))
                .Throws<Exception>();
            mockBusiness.Setup(p => p.Read(It.IsAny<Application>()))
                .Returns((Application app) =>
                {
                    if (app.Id == -1)
                    {
                        throw new BusinessException();
                    }
                    if (app.Id == -2)
                    {
                        throw new PersistentException();
                    }
                    if (app.Id == -3)
                    {
                        throw new Exception();
                    }
                    return apps.Find(x => x.Id == app.Id) ?? new Application();
                });
            mockBusiness.Setup(p => p.Insert(It.IsAny<Application>(), It.IsAny<User>()))
                .Returns((Application app, User user) =>
                {
                    if (app.Id == -1)
                    {
                        throw new BusinessException();
                    }
                    if (app.Id == -3)
                    {
                        throw new Exception();
                    }
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
                    if (app.Id == -1)
                    {
                        throw new BusinessException();
                    }
                    if (app.Id == -2)
                    {
                        throw new PersistentException();
                    }
                    if (app.Id == -3)
                    {
                        throw new Exception();
                    }
                    apps.Where(x => x.Id == app.Id).ToList().ForEach(x => x.Name = app.Name);
                    return app;
                });
            mockBusiness.Setup(p => p.Delete(It.IsAny<Application>(), It.IsAny<User>()))
                .Returns((Application app, User user) =>
                {
                    if (app.Id == -1)
                    {
                        throw new BusinessException();
                    }
                    if (app.Id == -2)
                    {
                        throw new PersistentException();
                    }
                    if (app.Id == -3)
                    {
                        throw new Exception();
                    }
                    apps = apps.Where(x => x.Id != app.Id).ToList();
                    return app;
                });
            mockBusiness.Setup(p => p.ListRoles("", It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<Application>()))
                .Returns(new ListResult<Role>(apps_roles.Where(x => x.Item1.Id == 1).Select(x => x.Item2).ToList(), 1));
            mockBusiness.Setup(p => p.ListRoles("idrole = 2", It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<Application>()))
                .Returns(new ListResult<Role>(new List<Role>(), 0));
            mockBusiness.Setup(p => p.ListRoles("idaplicación = 1", It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<Application>()))
                .Throws<PersistentException>();
            mockBusiness.Setup(p => p.ListRoles("error", It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<Application>()))
                .Throws<BusinessException>();
            mockBusiness.Setup(p => p.ListRoles("error1", It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<Application>()))
                .Throws<Exception>();
            mockBusiness.Setup(p => p.ListNotRoles(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<Application>()))
                .Returns((string filters, string orders, int limit, int offset, Application app) =>
                {
                    if (app.Id == -1)
                    {
                        throw new BusinessException();
                    }
                    if (app.Id == -2)
                    {
                        throw new PersistentException();
                    }
                    if (app.Id == -3)
                    {
                        throw new Exception();
                    }
                    List<Role> result = roles.Where(x => !apps_roles.Exists(y => y.Item1.Id == app.Id && y.Item2.Id == x.Id)).ToList();
                    return new ListResult<Role>(result, result.Count);
                });
            mockBusiness.Setup(p => p.InsertRole(It.IsAny<Role>(), It.IsAny<Application>(), It.IsAny<User>())).
                Returns((Role role, Application app, User user) =>
                {
                    if (app.Id == -1)
                    {
                        throw new BusinessException();
                    }
                    if (app.Id == -2)
                    {
                        throw new PersistentException();
                    }
                    if (app.Id == -3)
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
                        return role;
                    }
                });
            mockBusiness.Setup(p => p.DeleteRole(It.IsAny<Role>(), It.IsAny<Application>(), It.IsAny<User>())).
                Returns((Role role, Application app, User user) =>
                {
                    if (app.Id == -1)
                    {
                        throw new BusinessException();
                    }
                    if (app.Id == -2)
                    {
                        throw new PersistentException();
                    }
                    if (app.Id == -3)
                    {
                        throw new Exception();
                    }
                    return role;
                });

            api = new ApplicationController(configuration, mockBusiness.Object, mockLog.Object, mockTemplate.Object, mockParameter.Object)
            {
                ControllerContext = controllerContext
            };
        }
        #endregion

        #region Methods
        /// <summary>
        /// Prueba la consulta de un listado de aplicaciones con filtros, ordenamientos y límite
        /// </summary>
        [Fact]
        public void ListTest()
        {
            //Act
            ListResult<Application> list = api != null ? api.List("idapplication = 1", "name", 1, 0) : new ListResult<Application>(new List<Application>(), 0);

            //Assert
            Assert.NotEmpty(list.List);
            Assert.True(list.Total > 0);
        }

        /// <summary>
        /// Prueba la consulta de un listado de aplicaciones con filtros, ordenamientos y límite y con errores
        /// </summary>
        [Fact]
        public void ListWithErrorTest()
        {
            //Act
            ListResult<Application> list = api != null ? api.List("idaplicacion = 1", "name", 1, 0) : new ListResult<Application>(new List<Application>(), 0);

            //Assert
            Assert.Equal(0, list.Total);
        }

        /// <summary>
        /// Prueba la consulta de un listado de aplicaciones con filtros, ordenamientos y límite y con errores de negocio
        /// </summary>
        [Fact]
        public void ListWithError2Test()
        {
            //Act
            ListResult<Application> list = api != null ? api.List("error", "name", 1, 0) : new ListResult<Application>(new List<Application>(), 0);

            //Assert
            Assert.Equal(0, list.Total);
        }

        /// <summary>
        /// Prueba la consulta de un listado de aplicaciones con filtros, ordenamientos y límite y con error general
        /// </summary>
        [Fact]
        public void ListWithError3Test()
        {
            //Act
            ListResult<Application> list = api != null ? api.List("error1", "name", 1, 0) : new ListResult<Application>(new List<Application>(), 0);

            //Assert
            Assert.Equal(0, list.Total);
        }

        /// <summary>
        /// Prueba la consulta de una aplicación dada su identificador
        /// </summary>
        [Fact]
        public void ReadTest()
        {
            //Act
            Application application = api != null ? api.Read(1) : new();

            //Assert
            Assert.Equal("Autenticación", application.Name);
        }

        /// <summary>
        /// Prueba la consulta de una aplicación que no existe dado su identificador
        /// </summary>
        [Fact]
        public void ReadNotFoundTest()
        {
            //Act
            Application application = api != null ? api.Read(10) : new();

            //Assert
            Assert.Equal(0, application.Id);
        }

        /// <summary>
        /// Prueba la consulta de una aplicación dada su identificador conm error de persistencia
        /// </summary>
        [Fact]
        public void ReadWithErrorTest()
        {
            //Act
            Application application = api != null ? api.Read(-2) : new();

            //Assert
            Assert.Equal(0, application.Id);
        }

        /// <summary>
        /// Prueba la consulta de una aplicación dada su identificador conm error de negocio
        /// </summary>
        [Fact]
        public void ReadWithError2Test()
        {
            //Act
            Application application = api != null ? api.Read(-1) : new();

            //Assert
            Assert.Equal(0, application.Id);
        }

        /// <summary>
        /// Prueba la consulta de una aplicación dada su identificador conm error general
        /// </summary>
        [Fact]
        public void ReadWithError3Test()
        {
            //Act
            Application application = api != null ? api.Read(-3) : new();

            //Assert
            Assert.Equal(0, application.Id);
        }

        /// <summary>
        /// Prueba la inserción de una aplicación
        /// </summary>
        [Fact]
        public void InsertTest()
        {
            //Arrange
            Application application = new() { Name = "Prueba 1" };

            //Act
            application = api != null ? api.Insert(application) : new();

            //Assert
            Assert.NotEqual(0, application.Id);
        }

        /// <summary>
        /// Prueba la inserción de una aplicación con nombre duplicado
        /// </summary>
        [Fact]
        public void InsertDuplicateTest()
        {
            //Arrange
            Application application = new() { Name = "Autenticación" };

            //Act
            application = api != null ? api.Insert(application) : new();

            //Assert
            Assert.Equal(0, application.Id);
        }

        /// <summary>
        /// Prueba la inserción de una aplicación con error de negocio
        /// </summary>
        [Fact]
        public void InsertWithErrorTest()
        {
            //Arrange
            Application application = new() { Id = -1, Name = "Prueba 1" };

            //Act
            application = api != null ? api.Insert(application) : new();

            //Assert
            Assert.Equal(0, application.Id);
        }

        /// <summary>
        /// Prueba la inserción de una aplicación con error general
        /// </summary>
        [Fact]
        public void InsertWithError2Test()
        {
            //Arrange
            Application application = new() { Id = -3, Name = "Prueba 1" };

            //Act
            application = api != null ? api.Insert(application) : new();

            //Assert
            Assert.Equal(0, application.Id);
        }

        /// <summary>
        /// Prueba la actualización de una aplicación
        /// </summary>
        [Fact]
        public void UpdateTest()
        {
            //Arrange
            Application application = new() { Id = 2, Name = "Prueba actualizar" };

            //Act
            _ = api != null ? api.Update(application) : new();
            Application application2 = api != null ? api.Read(2) : new();

            //Assert
            Assert.NotEqual("Actualízame", application2.Name);
        }

        /// <summary>
        /// Prueba la actualización de una aplicación con error de persistencia
        /// </summary>
        [Fact]
        public void UpdateWithErrorTest()
        {
            //Arrange
            Application application = new() { Id = -2, Name = "Prueba actualizar" };

            //Act
            Application application2 = api != null ? api.Update(application) : new();

            //Assert
            Assert.Equal(0, application2.Id);
        }

        /// <summary>
        /// Prueba la actualización de una aplicación con error de negocio
        /// </summary>
        [Fact]
        public void UpdateWithError2Test()
        {
            //Arrange
            Application application = new() { Id = -1, Name = "Prueba actualizar" };

            //Act
            Application application2 = api != null ? api.Update(application) : new();

            //Assert
            Assert.Equal(0, application2.Id);
        }

        /// <summary>
        /// Prueba la actualización de una aplicación con error general
        /// </summary>
        [Fact]
        public void UpdateWithError3Test()
        {
            //Arrange
            Application application = new() { Id = -3, Name = "Prueba actualizar" };

            //Act
            Application application2 = api != null ? api.Update(application) : new();

            //Assert
            Assert.Equal(0, application2.Id);
        }

        /// <summary>
        /// Prueba la eliminación de una aplicación
        /// </summary>
        [Fact]
        public void DeleteTest()
        {
            //Act
            _ = api != null ? api.Delete(3) : new();
            Application application = api != null ? api.Read(3) : new();

            //Assert
            Assert.Equal(0, application.Id);
        }

        /// <summary>
        /// Prueba la eliminación de una aplicación con error de persistencia
        /// </summary>
        [Fact]
        public void DeleteWithErrorTest()
        {
            //Act
            Application application = api != null ? api.Delete(-2) : new();

            //Assert
            Assert.Equal(0, application.Id);
        }

        /// <summary>
        /// Prueba la eliminación de una aplicación con error de negocio
        /// </summary>
        [Fact]
        public void DeleteWithError2Test()
        {
            //Act
            Application application = api != null ? api.Delete(-1) : new();

            //Assert
            Assert.Equal(0, application.Id);
        }

        /// <summary>
        /// Prueba la eliminación de una aplicación con error general
        /// </summary>
        [Fact]
        public void DeleteWithError3Test()
        {
            //Act
            Application application = api != null ? api.Delete(-3) : new();

            //Assert
            Assert.Equal(0, application.Id);
        }

        /// <summary>
        /// Prueba la consulta de un listado de roles de una aplicación con filtros, ordenamientos y límite
        /// </summary>
        [Fact]
        public void ListRolesTest()
        {
            //Act
            ListResult<Role> list = api != null ? ((ApplicationController)api).ListRoles("", "", 10, 0, 1) : new ListResult<Role>(new List<Role>(), 0);

            //Assert
            Assert.NotEmpty(list.List);
            Assert.True(list.Total > 0);
        }

        /// <summary>
        /// Prueba la consulta de un listado de roles de una aplicación con filtros, ordenamientos y límite y con errores
        /// </summary>
        [Fact]
        public void ListRolesWithErrorTest()
        {
            //Act
            ListResult<Role> list = api != null ? ((ApplicationController)api).ListRoles("idaplicación = 1", "name", 10, 0, 1) : new ListResult<Role>(new List<Role>(), 0);

            //Assert
            Assert.Empty(list.List);
            Assert.Equal(0, list.Total);
        }

        /// <summary>
        /// Prueba la consulta de un listado de roles de una aplicación con filtros, ordenamientos y límite y con errores de negocio
        /// </summary>
        [Fact]
        public void ListRolesWithError2Test()
        {
            //Act
            ListResult<Role> list = api != null ? ((ApplicationController)api).ListRoles("error", "name", 10, 0, 1) : new ListResult<Role>(new List<Role>(), 0);

            //Assert
            Assert.Empty(list.List);
            Assert.Equal(0, list.Total);
        }

        /// <summary>
        /// Prueba la consulta de un listado de roles de una aplicación con filtros, ordenamientos y límite y con error general
        /// </summary>
        [Fact]
        public void ListRolesWithError3Test()
        {
            //Act
            ListResult<Role> list = api != null ? ((ApplicationController)api).ListRoles("error1", "name", 10, 0, 1) : new ListResult<Role>(new List<Role>(), 0);

            //Assert
            Assert.Empty(list.List);
            Assert.Equal(0, list.Total);
        }

        /// <summary>
        /// Prueba la consulta de un listado de roles no asignados a una aplicación con filtros, ordenamientos y límite
        /// </summary>
        [Fact]
        public void ListNotRolesTest()
        {
            //Act
            ListResult<Role> list = api != null ? ((ApplicationController)api).ListNotRoles("", "", 10, 0, 1) : new ListResult<Role>(new List<Role>(), 0);

            //Assert
            Assert.NotEmpty(list.List);
            Assert.True(list.Total > 0);
        }

        /// <summary>
        /// Prueba la consulta de un listado de roles no asignados a una aplicación con filtros, ordenamientos y límite con error de persistencia
        /// </summary>
        [Fact]
        public void ListNotRolesWithErrorTest()
        {
            //Act
            ListResult<Role> list = api != null ? ((ApplicationController)api).ListNotRoles("", "", 10, 0, -2) : new ListResult<Role>(new List<Role>(), 0);

            //Assert
            Assert.Empty(list.List);
            Assert.Equal(0, list.Total);
        }

        /// <summary>
        /// Prueba la consulta de un listado de roles no asignados a una aplicación con filtros, ordenamientos y límite con error de negocio
        /// </summary>
        [Fact]
        public void ListNotRolesWithError2Test()
        {
            //Act
            ListResult<Role> list = api != null ? ((ApplicationController)api).ListNotRoles("", "", 10, 0, -1) : new ListResult<Role>(new List<Role>(), 0);

            //Assert
            Assert.Empty(list.List);
            Assert.Equal(0, list.Total);
        }

        /// <summary>
        /// Prueba la consulta de un listado de roles no asignados a una aplicación con filtros, ordenamientos y límite con error general
        /// </summary>
        [Fact]
        public void ListNotRolesWithError3Test()
        {
            //Act
            ListResult<Role> list = api != null ? ((ApplicationController)api).ListNotRoles("", "", 10, 0, -3) : new ListResult<Role>(new List<Role>(), 0);

            //Assert
            Assert.Empty(list.List);
            Assert.Equal(0, list.Total);
        }

        /// <summary>
        /// Prueba la inserción de un rol de una aplicación
        /// </summary>
        [Fact]
        public void InsertRoleTest()
        {
            //Act
            Role role = api != null ? ((ApplicationController)api).InsertRole(new() { Id = 4 }, 1) : new();

            //Assert
            Assert.NotEqual(0, role.Id);
        }

        /// <summary>
        /// Prueba la inserción de un rol de una aplicación duplicado
        /// </summary>
        [Fact]
        public void InsertRoleDuplicateTest()
        {
            //Act
            Role role = api != null ? ((ApplicationController)api).InsertRole(new() { Id = 1 }, -2) : new();

            //Assert
            Assert.Equal(0, role.Id);
        }

        /// <summary>
        /// Prueba la inserción de un rol de una aplicación duplicado con error de negocio
        /// </summary>
        [Fact]
        public void InsertRoleWithErrorTest()
        {
            //Act
            Role role = api != null ? ((ApplicationController)api).InsertRole(new() { Id = 1 }, -1) : new();

            //Assert
            Assert.Equal(0, role.Id);
        }

        /// <summary>
        /// Prueba la inserción de un rol de una aplicación duplicado con error general
        /// </summary>
        [Fact]
        public void InsertRoleWithError2Test()
        {
            //Act
            Role role = api != null ? ((ApplicationController)api).InsertRole(new() { Id = 1 }, -3) : new();

            //Assert
            Assert.Equal(0, role.Id);
        }

        /// <summary>
        /// Prueba la eliminación de un rol de una aplicación
        /// </summary>
        [Fact]
        public void DeleteRoleTest()
        {
            //Act
            _ = api != null ? ((ApplicationController)api).DeleteRole(2, 1) : new();
            ListResult<Role> list = api != null ? ((ApplicationController)api).ListRoles("idrole = 2", "", 10, 0, 1) : new(new List<Role>(), 0);

            //Assert
            Assert.Equal(0, list.Total);
        }

        /// <summary>
        /// Prueba la eliminación de un rol de una aplicación con error de persistencia
        /// </summary>
        [Fact]
        public void DeleteRoleWithErrorTest()
        {
            //Act
            Role role = api != null ? ((ApplicationController)api).DeleteRole(2, -2) : new();

            //Assert
            Assert.Equal(0, role.Id);
        }

        /// <summary>
        /// Prueba la eliminación de un rol de una aplicación con error de negocio
        /// </summary>
        [Fact]
        public void DeleteRoleWithError2Test()
        {
            //Act
            Role role = api != null ? ((ApplicationController)api).DeleteRole(2, -1) : new();

            //Assert
            Assert.Equal(0, role.Id);
        }

        /// <summary>
        /// Prueba la eliminación de un rol de una aplicación con error general
        /// </summary>
        [Fact]
        public void DeleteRoleWithError3Test()
        {
            //Act
            Role role = api != null ? ((ApplicationController)api).DeleteRole(2, -3) : new();

            //Assert
            Assert.Equal(0, role.Id);
        }
        #endregion
    }
}

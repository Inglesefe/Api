using Api.Controllers.Auth;
using Business.Auth;
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
            ListResult<Application> list = api.List("idapplication = 1", "name", 1, 0);

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
            ListResult<Application> list = api.List("idaplicacion = 1", "name", 1, 0);

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
            Application application = api.Read(1);

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
            Application application = api.Read(10);

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
            application = api.Insert(application);

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
            application = api.Insert(application);

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
            _ = api.Update(application);
            Application application2 = api.Read(2);

            //Assert
            Assert.NotEqual("Actualízame", application2.Name);
        }

        /// <summary>
        /// Prueba la eliminación de una aplicación
        /// </summary>
        [Fact]
        public void DeleteTest()
        {
            //Act
            _ = api.Delete(3);
            Application application = api.Read(3);

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
            ListResult<Role> list = ((ApplicationController)api).ListRoles("", "", 10, 0, 1);

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
            ListResult<Role> list = ((ApplicationController)api).ListRoles("idaplicación = 1", "name", 10, 0, 1);

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
            ListResult<Role> list = ((ApplicationController)api).ListNotRoles("", "", 10, 0, 1);

            //Assert
            Assert.NotEmpty(list.List);
            Assert.True(list.Total > 0);
        }

        /// <summary>
        /// Prueba la inserción de un rol de una aplicación
        /// </summary>
        [Fact]
        public void InsertRoleTest()
        {
            //Act
            Role role = ((ApplicationController)api).InsertRole(new() { Id = 4 }, 1);

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
            Role role = ((ApplicationController)api).InsertRole(new() { Id = 1 }, 1);

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
            _ = ((ApplicationController)api).DeleteRole(2, 1);
            ListResult<Role> list = ((ApplicationController)api).ListRoles("r.idrole = 2", "", 10, 0, 1);

            //Assert
            Assert.Equal(0, list.Total);
        }
        #endregion
    }
}

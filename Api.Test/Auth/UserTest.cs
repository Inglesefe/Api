using Api.Controllers.Auth;
using Api.Dto;
using Dal.Dto;
using Entities.Auth;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.Extensions.Configuration;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Security.Principal;
using System.Text;

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
            _api = new(_configuration)
            {
                ControllerContext = _controllerContext
            };
        }
        #endregion

        #region Methods
        /// <summary>
        /// Prueba la consulta de un listado de usuarios con filtros, ordenamientos y límite
        /// </summary>
        /// <returns>N/A</returns>
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
        /// <returns>N/A</returns>
        [Fact]
        public void UserListWithErrorTest()
        {
            ListResult<User> list = _api.List("idusuario = 1", "name", 1, 0);
            Assert.Equal(0, list.Total);
        }

        /// <summary>
        /// Prueba la consulta de un usuario dado su identificador
        /// </summary>
        /// <returns>N/A</returns>
        [Fact]
        public void UserReadTest()
        {
            User user = _api.Read(1);
            Assert.Equal("leandrobaena@gmail.com", user.Login);
        }

        /// <summary>
        /// Prueba la consulta de un usuario que no existe dado su identificador
        /// </summary>
        /// <returns>N/A</returns>
        [Fact]
        public void UserReadNotFoundTest()
        {
            User user = _api.Read(10);
            Assert.Equal(0, user.Id);
        }

        /// <summary>
        /// Prueba la inserción de un usuario
        /// </summary>
        /// <returns>N/A</returns>
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
        /// <returns>N/A</returns>
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
        /// <returns>N/A</returns>
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
        /// <returns>N/A</returns>
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
        /// <returns>N/A</returns>
        [Fact]
        public void UserReadByLoginAndPasswordTest()
        {
            LoginResponse response = _api.ReadByLoginAndPassword(new() { Login = "leandrobaena@gmail.com", Password = "FLWnwyoEz/7tYsnS+vxTVg==" });
            Assert.True(response.Valid);
        }

        /// <summary>
        /// Prueba la consulta de un usuario que no existe dado su login y password
        /// </summary>
        /// <returns>N/A</returns>
        [Fact]
        public void UserReadByLoginAndPasswordWithErrorTest()
        {
            LoginResponse response = _api.ReadByLoginAndPassword(new() { Login = "actualizame@gmail.com", Password = "Errada" });
            Assert.False(response.Valid);
        }

        /// <summary>
        /// Prueba la consulta de un usuario inactivo dado su login y password
        /// </summary>
        /// <returns>N/A</returns>
        [Fact]
        public void UserReadByLoginAndPasswordInactiveTest()
        {
            LoginResponse response = _api.ReadByLoginAndPassword(new() { Login = "inactivo@gmail.com", Password = "Prueba123" });
            Assert.False(response.Valid);
        }

        /// <summary>
        /// Prueba la actualización de la contraseña de un usuario
        /// </summary>
        /// <returns>N/A</returns>
        [Fact]
        public void UserUpdatePasswordTest()
        {
            using Aes aes = Aes.Create();
            aes.Key = Encoding.UTF8.GetBytes(_configuration["Aes:Key"] ?? "");
            aes.IV = Encoding.UTF8.GetBytes(_configuration["Aes:IV"] ?? "");

            ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
            string param = "1~leandrobaena@gmail.com~" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            byte[] paramBytes = Encoding.UTF8.GetBytes(param);
            byte[] cryptoBytes = encryptor.TransformFinalBlock(paramBytes, 0, paramBytes.Length);
            string crypto = Convert.ToBase64String(cryptoBytes);

            ChangePasswordRequest request = new()
            {
                Password = "Prueba123",
                Token = crypto
            };
            _ = _api.UpdatePassword(request);

            LoginResponse response = _api.ReadByLoginAndPassword(new() { Login = "leandrobaena@gmail.com", Password = "FLWnwyoEz/7tYsnS+vxTVg==" });
            Assert.True(response.Valid);
        }

        /// <summary>
        /// Prueba la consulta de un listado de roles de un usuario con filtros, ordenamientos y límite
        /// </summary>
        /// <returns>N/A</returns>
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
        /// <returns>N/A</returns>
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
        /// <returns>N/A</returns>
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
        /// <returns>N/A</returns>
        [Fact]
        public void UserInsertRoleTest()
        {
            Role role = _api.InsertRole(new() { Id = 4 }, 1);
            Assert.NotEqual(0, role.Id);
        }

        /// <summary>
        /// Prueba la inserción de un rol de un usuario duplicado
        /// </summary>
        /// <returns>N/A</returns>
        [Fact]
        public void UserInsertRoleDuplicateTest()
        {
            Role role = _api.InsertRole(new() { Id = 1 }, 1);
            Assert.Equal(0, role.Id);
        }

        /// <summary>
        /// Prueba la eliminación de un rol de un usuario
        /// </summary>
        /// <returns>N/A</returns>
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

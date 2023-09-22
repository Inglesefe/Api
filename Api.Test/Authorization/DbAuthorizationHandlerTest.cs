using Api.Authorization;
using Business.Auth;
using Dal.Dto;
using Entities.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Security.Claims;
using System.Security.Principal;

namespace Api.Test.Authorization
{
    /// <summary>
    /// Prueba sobre el autorizado personalizado
    /// </summary>
    [Collection("Test")]
    public class DbAuthorizationHandlerTest
    {
        #region Attributes
        /// <summary>
        /// Listado de roles que tendrá el usuario que quiere autorización
        /// </summary>
        private readonly ListResult<Role> roles;

        /// <summary>
        /// Capa de negocio que consulta los roles del usuario
        /// </summary>
        private readonly Mock<IBusinessApplication> business;

        /// <summary>
        /// Contexto de autorización con que se conecta al manejador
        /// </summary>
        private readonly AuthorizationHandlerContext context;

        /// <summary>
        /// Manejador de autorización a probar
        /// </summary>
        private readonly DbAuthorizationHandler handler;
        #endregion

        #region Constructors
        /// <summary>
        /// Inicializa la prueba con los valores necesarios
        /// </summary>
        public DbAuthorizationHandlerTest()
        {
            //Arrange
            roles = new(new List<Role>() { new Role() { Id = 1, Name = "Administradores" } }, 1);
            business = new Mock<IBusinessApplication>();
            business.Setup(x => x.ListRoles(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<Application>()))
                .Returns(roles);
            GenericIdentity identity = new("usuario", "prueba");
            identity.AddClaim(new Claim("id", "1"));
            identity.AddClaim(new Claim(ClaimTypes.Role, "1"));
            ControllerContext controllerContext = new()
            {
                HttpContext = new DefaultHttpContext()
                {
                    User = new ClaimsPrincipal(identity),
                }
            };
            controllerContext.HttpContext.Request.RouteValues.Add("controller", "User");
            context = new(new List<DbAuthorizationRequirement>() { new DbAuthorizationRequirement() }, new ClaimsPrincipal(identity), controllerContext.HttpContext);
            handler = new(business.Object);
        }
        #endregion

        #region Methods
        /// <summary>
        /// Prueba la autorización sobre un controlador
        /// </summary>
        [Fact]
        public void HandleRequirementAsyncTest()
        {
            //Act
            handler.HandleAsync(context).Wait();

            //Assert
            Assert.True(context.HasSucceeded);
        }
        #endregion
    }
}

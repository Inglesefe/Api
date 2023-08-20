using Business.Auth;
using Dal.Dto;
using Entities.Auth;
using Microsoft.AspNetCore.Authorization;
using MySql.Data.MySqlClient;
using System.Data;
using System.Security.Claims;

namespace Api.Authorization
{
    /// <summary>
    /// Proveedor personalizado de autorización, para conectar a la base de datos para determinar
    /// los recursos a los que está autorizado un usuario autenticado
    /// </summary>
    public class DbAuthorizationHandler : AuthorizationHandler<DbAuthorizationRequirement>
    {
        #region Attributes
        /// <summary>
        /// Conexión a la base de datos
        /// </summary>
        private readonly IConfiguration _configuration;
        #endregion

        #region Constructors
        /// <summary>
        /// Inicializa la conexión a la base de datos
        /// </summary>
        /// <param name="connection">Conexión a la base de datos</param>
        public DbAuthorizationHandler(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        #endregion

        #region Methods
        /// <summary>
        /// Realiza la validación del usuario contra la base de datos
        /// </summary>
        /// <param name="context">Contexto de la solicitud Http</param>
        /// <param name="requirement">Requerimiento con el id de la aplicación</param>
        /// <returns>Si el usuario es válido o no</returns>
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, DbAuthorizationRequirement requirement)
        {
            Dictionary<string, int> applications = new()
            {
                { "Application", 1 },
                { "Role", 1 },
                { "User", 1 },
                { "Notification", 2 },
                { "Template", 2 },
                { "City", 3 },
                { "Country", 3 },
                { "IdentificationType", 3 },
                { "IncomeType", 3 },
                { "Office", 3 },
                { "Parameter", 3 },
                { "Plan", 3 }
            };
            DefaultHttpContext? ctx = (DefaultHttpContext?)context.Resource;
            if (ctx != null)
            {
                string controller = (string)(ctx.Request.RouteValues["controller"] ?? "");
                IDbConnection connection = new MySqlConnection(_configuration.GetConnectionString("golden"));
                BusinessApplication businessApplication = new(connection);
                ListResult<Role> roles = businessApplication.ListRoles("", "", 100, 0, new() { Id = applications[controller] });
                if (roles.Total > 0)
                {
                    string rolesInToken = context.User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Role)?.Value ?? "";
                    if (roles.List.Any(x => rolesInToken.Split(",").Contains("" + x.Id)))
                    {
                        context.Succeed(requirement);
                    }
                    else
                    {
                        context.Fail(new AuthorizationFailureReason(this, "El usuario no tiene permisos para acceder a este recurso"));
                    }
                }
            }
            else
            {
                context.Fail(new AuthorizationFailureReason(this, "El usuario no tiene permisos para acceder a este recurso"));
            }
            return Task.CompletedTask;
        }
        #endregion
    }
}

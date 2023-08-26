using Business;
using Business.Auth;
using Business.Exceptions;
using Dal;
using Dal.Dto;
using Dal.Exceptions;
using Entities.Auth;
using Entities.Log;
using Entities.Noti;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Data;

namespace Api.Controllers.Auth
{
    /// <summary>
    /// Controlador con los métodos necesarios para administrar usuarios
    /// </summary>
    [ApiController]
    [Route("[controller]")]
    [Authorize(Policy = "db")]
    public class RoleController : ControllerBase<Role>
    {
        #region Constructors
        /// <summary>
        /// Inicializa la configuración del controlador
        /// </summary>
        /// <param name="configuration">Configuración de la aplicación</param>
        /// <param name="business">Capa de negocio de roles</param>
        /// <param name="log">Administrador de logs en la base de datos</param>
        /// <param name="templateError">Administrador de plantilla de errores</param>
        /// <param name="connection">Conexión a la base de datos</param>
        public RoleController(IConfiguration configuration, IBusinessRole business, IPersistentBase<LogComponent> log, IBusiness<Template> templateError, IDbConnection connection) : base(
            configuration,
            business,
            log,
            templateError,
            connection)
        { }
        #endregion

        #region Methods
        /// <summary>
        /// Trae un listado de usuarios asignados a un rol desde la base de datos
        /// </summary>
        /// <param name="filters">Filtros aplicados a la consulta</param>
        /// <param name="orders">Ordenamientos aplicados a la base de datos</param>
        /// <param name="limit">Límite de registros a traer</param>
        /// <param name="offset">Corrimiento desde el que se cuenta el número de registros</param>
        /// <param name="role">Identificador del rol al que se le consultan los usuarios asignados</param>
        /// <returns>Listado de usuarios asignados al rol</returns>
        [HttpGet("{role:int}/user")]
        public ListResult<User> ListUsers(string? filters, string? orders, int limit, int offset, int role)
        {
            try
            {
                LogInfo("List users related to role " + role);
                return ((IBusinessRole)_business).ListUsers(filters ?? "", orders ?? "", limit, offset, new() { Id = role }, _connection);
            }
            catch (PersistentException e)
            {
                LogError(e, "P");
            }
            catch (BusinessException e)
            {
                LogError(e, "B");
            }
            catch (Exception e)
            {
                LogError(e, "A");
            }
            Response.StatusCode = 500;
            return new ListResult<User>(new List<User>(), 0);
        }

        /// <summary>
        /// Trae un listado de usuarios no asignados a un rol desde la base de datos
        /// </summary>
        /// <param name="filters">Filtros aplicados a la consulta</param>
        /// <param name="orders">Ordenamientos aplicados a la base de datos</param>
        /// <param name="limit">Límite de registros a traer</param>
        /// <param name="offset">Corrimiento desde el que se cuenta el número de registros</param>
        /// <param name="role">Identificador del rol al que se le consultan los usuarios no asignados</param>
        /// <returns>Listado de usuarios no asignados al rol</returns>
        [HttpGet("{role:int}/not-user")]
        public ListResult<User> ListNotUsers(string? filters, string? orders, int limit, int offset, int role)
        {
            try
            {
                LogInfo("List users not related to role " + role);
                return ((IBusinessRole)_business).ListNotUsers(filters ?? "", orders ?? "", limit, offset, new() { Id = role }, _connection);
            }
            catch (PersistentException e)
            {
                LogError(e, "P");
            }
            catch (BusinessException e)
            {
                LogError(e, "B");
            }
            catch (Exception e)
            {
                LogError(e, "A");
            }
            Response.StatusCode = 500;
            return new ListResult<User>(new List<User>(), 0);
        }

        /// <summary>
        /// Asigna un usuario a un rol en la base de datos
        /// </summary>
        /// <param name="user">Usuario que se asigna al rol</param>
        /// <param name="role">Rol al que se le asigna el usuario</param>
        /// <returns>Usuario asignado</returns>
        [HttpPost("{role:int}/user")]
        public User InsertUser([FromBody] User user, int role)
        {
            try
            {
                LogInfo("Insert role " + role + " to user " + user.Id);
                return ((IBusinessRole)_business).InsertUser(user, new() { Id = role }, new() { Id = int.Parse(HttpContext.User.Claims.First(x => x.Type == "id").Value) }, _connection);
            }
            catch (PersistentException e)
            {
                LogError(e, "P");
            }
            catch (BusinessException e)
            {
                LogError(e, "B");
            }
            catch (Exception e)
            {
                LogError(e, "A");
            }
            Response.StatusCode = 500;
            return new();
        }

        /// <summary>
        /// Elimina un usuario de un rol de la base de datos
        /// </summary>
        /// <param name="user">Identificador del usuario a eliminarle el rol</param>
        /// <param name="role">Identificador del rol al que se le elimina el usuario</param>
        /// <returns>Usuario eliminado</returns>
        [HttpDelete("{role:int}/user/{user:int}")]
        public User DeleteUser(int user, int role)
        {
            try
            {
                LogInfo("Delete role " + role + " to user " + user);
                return ((IBusinessRole)_business).DeleteUser(new() { Id = user }, new() { Id = role }, new() { Id = int.Parse(HttpContext.User.Claims.First(x => x.Type == "id").Value) }, _connection);
            }
            catch (PersistentException e)
            {
                LogError(e, "P");
            }
            catch (BusinessException e)
            {
                LogError(e, "B");
            }
            catch (Exception e)
            {
                LogError(e, "A");
            }
            Response.StatusCode = 500;
            return new();
        }

        /// <summary>
        /// Trae un listado de aplicaciones asignadas a un rol desde la base de datos
        /// </summary>
        /// <param name="filters">Filtros aplicados a la consulta</param>
        /// <param name="orders">Ordenamientos aplicados a la base de datos</param>
        /// <param name="limit">Límite de registros a traer</param>
        /// <param name="offset">Corrimiento desde el que se cuenta el número de registros</param>
        /// <param name="role">Identificador del rol al que se le consultan las aplicaciones asignadas</param>
        /// <returns>Listado de aplicaciones asignadas al rol</returns>
        [HttpGet("{role:int}/application")]
        public ListResult<Application> ListApplications(string? filters, string? orders, int limit, int offset, int role)
        {
            try
            {
                LogInfo("List app related to role " + role);
                return ((IBusinessRole)_business).ListApplications(filters ?? "", orders ?? "", limit, offset, new() { Id = role }, _connection);
            }
            catch (PersistentException e)
            {
                LogError(e, "P");
            }
            catch (BusinessException e)
            {
                LogError(e, "B");
            }
            catch (Exception e)
            {
                LogError(e, "A");
            }
            Response.StatusCode = 500;
            return new ListResult<Application>(new List<Application>(), 0);
        }

        /// <summary>
        /// Trae un listado de aplicaciones no asignadas a un rol desde la base de datos
        /// </summary>
        /// <param name="filters">Filtros aplicados a la consulta</param>
        /// <param name="orders">Ordenamientos aplicados a la base de datos</param>
        /// <param name="limit">Límite de registros a traer</param>
        /// <param name="offset">Corrimiento desde el que se cuenta el número de registros</param>
        /// <param name="role">Identificador del rol al que se le consultan las aplicaciones no asignadas</param>
        /// <returns>Listado de aplicaciones no asignadas al rol</returns>
        [HttpGet("{role:int}/not-application")]
        public ListResult<Application> ListNotApplications(string? filters, string? orders, int limit, int offset, int role)
        {
            try
            {
                LogInfo("List app not related to role " + role);
                return ((IBusinessRole)_business).ListNotApplications(filters ?? "", orders ?? "", limit, offset, new() { Id = role }, _connection);
            }
            catch (PersistentException e)
            {
                LogError(e, "P");
            }
            catch (BusinessException e)
            {
                LogError(e, "B");
            }
            catch (Exception e)
            {
                LogError(e, "A");
            }
            Response.StatusCode = 500;
            return new ListResult<Application>(new List<Application>(), 0);
        }

        /// <summary>
        /// Asigna una aplicación a un rol en la base de datos
        /// </summary>
        /// <param name="application">Aplicación que se asigna al rol</param>
        /// <param name="role">Rol al que se le asigna la aplicación</param>
        /// <returns>Aplicación asignada</returns>
        [HttpPost("{role:int}/application")]
        public Application InsertApplication([FromBody] Application application, int role)
        {
            try
            {
                LogInfo("Insert role " + role + " to app " + application.Id);
                return ((IBusinessRole)_business).InsertApplication(application, new() { Id = role }, new() { Id = int.Parse(HttpContext.User.Claims.First(x => x.Type == "id").Value) }, _connection);
            }
            catch (PersistentException e)
            {
                LogError(e, "P");
            }
            catch (BusinessException e)
            {
                LogError(e, "B");
            }
            catch (Exception e)
            {
                LogError(e, "A");
            }
            Response.StatusCode = 500;
            return new();
        }

        /// <summary>
        /// Elimina una aplicación de un rol de la base de datos
        /// </summary>
        /// <param name="application">Identificador de la aplicación a eliminarle el rol</param>
        /// <param name="role">Identificador del rol al que se le elimina la aplicación</param>
        /// <returns>Aplicación eliminada</returns>
        [HttpDelete("{role:int}/application/{application:int}")]
        public Application DeleteApplication(int application, int role)
        {
            try
            {
                LogInfo("Delete role " + role + " to app " + application);
                return ((IBusinessRole)_business).DeleteApplication(new() { Id = application }, new() { Id = role }, new() { Id = int.Parse(HttpContext.User.Claims.First(x => x.Type == "id").Value) }, _connection);
            }
            catch (PersistentException e)
            {
                LogError(e, "P");
            }
            catch (BusinessException e)
            {
                LogError(e, "B");
            }
            catch (Exception e)
            {
                LogError(e, "A");
            }
            Response.StatusCode = 500;
            return new();
        }
        #endregion
    }
}

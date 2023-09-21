using Business;
using Business.Auth;
using Business.Exceptions;
using Dal;
using Dal.Dto;
using Dal.Exceptions;
using Entities.Auth;
using Entities.Config;
using Entities.Log;
using Entities.Noti;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

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
        /// <param name="configuration">Configuración del api</param>
        /// <param name="business">Capa de negocio de roles</param>
        /// <param name="log">Administrador de logs en la base de datos</param>
        /// <param name="templateError">Administrador de plantilla de errores</param>
        /// <param name="parameter">Administrador de parámetros</param>
        public RoleController(IConfiguration configuration, IBusinessRole business, IPersistent<LogComponent> log, IBusiness<Template> templateError, IBusiness<Parameter> parameter) : base(
            configuration,
            business,
            log,
            templateError,
            parameter)
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
                ListResult<User> result = ((IBusinessRole)_business).ListUsers(filters ?? "", orders ?? "", limit, offset, new() { Id = role });
                LogInfo("filters: " + filters + ", orders: " + orders + ", limit: " + limit + ", offset: " + offset + ", role: " + role, JsonSerializer.Serialize(result));
                return result;
            }
            catch (PersistentException e)
            {
                LogError(e, "P", "filters: " + filters + ", orders: " + orders + ", limit: " + limit + ", offset: " + offset + ", role: " + role);
            }
            catch (BusinessException e)
            {
                LogError(e, "B", "filters: " + filters + ", orders: " + orders + ", limit: " + limit + ", offset: " + offset + ", role: " + role);
            }
            catch (Exception e)
            {
                LogError(e, "A", "filters: " + filters + ", orders: " + orders + ", limit: " + limit + ", offset: " + offset + ", role: " + role);
            }
            Response.StatusCode = 500;
            return new(new List<User>(), 0);
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
                ListResult<User> result = ((IBusinessRole)_business).ListNotUsers(filters ?? "", orders ?? "", limit, offset, new() { Id = role });
                LogInfo("filters: " + filters + ", orders: " + orders + ", limit: " + limit + ", offset: " + offset + ", role: " + role, JsonSerializer.Serialize(result));
                return result;
            }
            catch (PersistentException e)
            {
                LogError(e, "P", "filters: " + filters + ", orders: " + orders + ", limit: " + limit + ", offset: " + offset + ", role: " + role);
            }
            catch (BusinessException e)
            {
                LogError(e, "B", "filters: " + filters + ", orders: " + orders + ", limit: " + limit + ", offset: " + offset + ", role: " + role);
            }
            catch (Exception e)
            {
                LogError(e, "A", "filters: " + filters + ", orders: " + orders + ", limit: " + limit + ", offset: " + offset + ", role: " + role);
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
                User result = ((IBusinessRole)_business).InsertUser(user, new() { Id = role }, new() { Id = int.Parse(HttpContext.User.Claims.First(x => x.Type == "id").Value) });
                LogInfo("user: " + JsonSerializer.Serialize(user) + ", role: " + role, JsonSerializer.Serialize(result));
                return result;
            }
            catch (PersistentException e)
            {
                LogError(e, "P", "user: " + JsonSerializer.Serialize(user) + ", role: " + role);
            }
            catch (BusinessException e)
            {
                LogError(e, "B", "user: " + JsonSerializer.Serialize(user) + ", role: " + role);
            }
            catch (Exception e)
            {
                LogError(e, "A", "user: " + JsonSerializer.Serialize(user) + ", role: " + role);
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
                User result = ((IBusinessRole)_business).DeleteUser(new() { Id = user }, new() { Id = role }, new() { Id = int.Parse(HttpContext.User.Claims.First(x => x.Type == "id").Value) });
                LogInfo("user: " + user + ", role: " + role, JsonSerializer.Serialize(result));
                return result;
            }
            catch (PersistentException e)
            {
                LogError(e, "P", "user: " + user + ", role: " + role);
            }
            catch (BusinessException e)
            {
                LogError(e, "B", "user: " + user + ", role: " + role);
            }
            catch (Exception e)
            {
                LogError(e, "A", "user: " + user + ", role: " + role);
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
                ListResult<Application> result = ((IBusinessRole)_business).ListApplications(filters ?? "", orders ?? "", limit, offset, new() { Id = role });
                LogInfo("filters: " + filters + ", orders: " + orders + ", limit: " + limit + ", offset: " + offset + ", role: " + role, JsonSerializer.Serialize(result));
                return result;
            }
            catch (PersistentException e)
            {
                LogError(e, "P", "filters: " + filters + ", orders: " + orders + ", limit: " + limit + ", offset: " + offset + ", role: " + role);
            }
            catch (BusinessException e)
            {
                LogError(e, "B", "filters: " + filters + ", orders: " + orders + ", limit: " + limit + ", offset: " + offset + ", role: " + role);
            }
            catch (Exception e)
            {
                LogError(e, "A", "filters: " + filters + ", orders: " + orders + ", limit: " + limit + ", offset: " + offset + ", role: " + role);
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
                ListResult<Application> result = ((IBusinessRole)_business).ListNotApplications(filters ?? "", orders ?? "", limit, offset, new() { Id = role });
                LogInfo("filters: " + filters + ", orders: " + orders + ", limit: " + limit + ", offset: " + offset + ", role: " + role, JsonSerializer.Serialize(result));
                return result;
            }
            catch (PersistentException e)
            {
                LogError(e, "P", "filters: " + filters + ", orders: " + orders + ", limit: " + limit + ", offset: " + offset + ", role: " + role);
            }
            catch (BusinessException e)
            {
                LogError(e, "B", "filters: " + filters + ", orders: " + orders + ", limit: " + limit + ", offset: " + offset + ", role: " + role);
            }
            catch (Exception e)
            {
                LogError(e, "A", "filters: " + filters + ", orders: " + orders + ", limit: " + limit + ", offset: " + offset + ", role: " + role);
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
                Application result = ((IBusinessRole)_business).InsertApplication(application, new() { Id = role }, new() { Id = int.Parse(HttpContext.User.Claims.First(x => x.Type == "id").Value) });
                LogInfo("application: " + JsonSerializer.Serialize(application) + ", role: " + role, JsonSerializer.Serialize(result));
                return result;
            }
            catch (PersistentException e)
            {
                LogError(e, "P", "application: " + JsonSerializer.Serialize(application) + ", role: " + role);
            }
            catch (BusinessException e)
            {
                LogError(e, "B", "application: " + JsonSerializer.Serialize(application) + ", role: " + role);
            }
            catch (Exception e)
            {
                LogError(e, "A", "application: " + JsonSerializer.Serialize(application) + ", role: " + role);
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
                Application result = ((IBusinessRole)_business).DeleteApplication(new() { Id = application }, new() { Id = role }, new() { Id = int.Parse(HttpContext.User.Claims.First(x => x.Type == "id").Value) });
                LogInfo("application: " + application + ", role: " + role, JsonSerializer.Serialize(result));
                return result;
            }
            catch (PersistentException e)
            {
                LogError(e, "P", "application: " + application + ", role: " + role);
            }
            catch (BusinessException e)
            {
                LogError(e, "B", "application: " + application + ", role: " + role);
            }
            catch (Exception e)
            {
                LogError(e, "A", "application: " + application + ", role: " + role);
            }
            Response.StatusCode = 500;
            return new();
        }

        /// <inheritdoc />
        protected override Role GetNewObject(int id)
        {
            return new Role() { Id = id };
        }

        /// <inheritdoc />
        protected override bool ObjectIsDefault(Role obj)
        {
            return obj.Id == 0;
        }
        #endregion
    }
}

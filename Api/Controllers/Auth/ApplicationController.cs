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
    /// Controlador con los métodos necesarios para administrar aplicaciones
    /// </summary>
    [ApiController]
    [Route("[controller]")]
    [Authorize(Policy = "db")]
    public class ApplicationController : ControllerBase<Application>
    {
        #region Constructors
        /// <summary>
        /// Inicializa la configuración del controlador
        /// </summary>
        /// <param name="configuration">Configuración del api</param>
        /// <param name="business">Capa de negocio de aplicaciones</param>
        /// <param name="log">Administrador de logs en la base de datos</param>
        /// <param name="templateError">Administrador de plantilla de errores</param>
        /// <param name="parameter">Administrador de parámetros</param>
        public ApplicationController(IConfiguration configuration, IBusinessApplication business, IPersistent<LogComponent> log, IBusiness<Template> templateError, IBusiness<Parameter> parameter) : base(
                  configuration,
                  business,
                  log,
                  templateError,
                  parameter)
        { }
        #endregion

        #region Methods
        /// <summary>
        /// Trae un listado de roles asignados a una aplicación desde la base de datos
        /// </summary>
        /// <param name="filters">Filtros aplicados a la consulta</param>
        /// <param name="orders">Ordenamientos aplicados a la base de datos</param>
        /// <param name="limit">Límite de registros a traer</param>
        /// <param name="offset">Corrimiento desde el que se cuenta el número de registros</param>
        /// <param name="application">Aplicación a la que se le consultan los roles asignados</param>
        /// <returns>Listado de roles asignados a la aplicación</returns>
        [HttpGet("{application:int}/role")]
        public ListResult<Role> ListRoles(string? filters, string? orders, int limit, int offset, int application)
        {
            try
            {
                ListResult<Role> result = ((IBusinessApplication)_business).ListRoles(filters ?? "", orders ?? "", limit, offset, new() { Id = application });
                LogInfo("filters: " + filters + ", orders: " + orders + ", limit: " + limit + ", offset: " + offset + ", application: " + application, JsonSerializer.Serialize(result));
                return result;
            }
            catch (PersistentException e)
            {
                LogError(e, "P", "filters: " + filters + ", orders: " + orders + ", limit: " + limit + ", offset: " + offset + ", application: " + application);
            }
            catch (BusinessException e)
            {
                LogError(e, "B", "filters: " + filters + ", orders: " + orders + ", limit: " + limit + ", offset: " + offset + ", application: " + application);
            }
            catch (Exception e)
            {
                LogError(e, "A", "filters: " + filters + ", orders: " + orders + ", limit: " + limit + ", offset: " + offset + ", application: " + application);
            }
            Response.StatusCode = 500;
            return new(new List<Role>(), 0);
        }

        /// <summary>
        /// Trae un listado de roles no asignados a una aplicación desde la base de datos
        /// </summary>
        /// <param name="filters">Filtros aplicados a la consulta</param>
        /// <param name="orders">Ordenamientos aplicados a la base de datos</param>
        /// <param name="limit">Límite de registros a traer</param>
        /// <param name="offset">Corrimiento desde el que se cuenta el número de registros</param>
        /// <param name="application">Aplicación a la que se le consultan los roles no asignados</param>
        /// <returns>Listado de roles no asignados a la aplicación</returns>
        [HttpGet("{application:int}/not-role")]
        public ListResult<Role> ListNotRoles(string? filters, string? orders, int limit, int offset, int application)
        {
            try
            {
                ListResult<Role> result = ((IBusinessApplication)_business).ListNotRoles(filters ?? "", orders ?? "", limit, offset, new() { Id = application });
                LogInfo("filters: " + filters + ", orders: " + orders + ", limit: " + limit + ", offset: " + offset + ", application: " + application, JsonSerializer.Serialize(result));
                return result;
            }
            catch (PersistentException e)
            {
                LogError(e, "P", "filters: " + filters + ", orders: " + orders + ", limit: " + limit + ", offset: " + offset + ", application: " + application);
            }
            catch (BusinessException e)
            {
                LogError(e, "B", "filters: " + filters + ", orders: " + orders + ", limit: " + limit + ", offset: " + offset + ", application: " + application);
            }
            catch (Exception e)
            {
                LogError(e, "A", "filters: " + filters + ", orders: " + orders + ", limit: " + limit + ", offset: " + offset + ", application: " + application);
            }
            Response.StatusCode = 500;
            return new ListResult<Role>(new List<Role>(), 0);
        }

        /// <summary>
        /// Asigna un rol a una aplicación en la base de datos
        /// </summary>
        /// <param name="role">Rol que se asigna a la aplicación</param>
        /// <param name="application">Aplicación al que se le asigna el rol</param>
        /// <returns>Rol asignado</returns>
        [HttpPost("{application:int}/role")]
        public Role InsertRole([FromBody] Role role, int application)
        {
            try
            {
                Role result = ((IBusinessApplication)_business).InsertRole(role, new() { Id = application }, new() { Id = int.Parse(HttpContext.User.Claims.First(x => x.Type == "id").Value) });
                LogInfo("role: " + JsonSerializer.Serialize(role) + ", application: " + application, JsonSerializer.Serialize(result));
                return result;
            }
            catch (PersistentException e)
            {
                LogError(e, "P", "role: " + JsonSerializer.Serialize(role) + ", application: " + application);
            }
            catch (BusinessException e)
            {
                LogError(e, "B", "role: " + JsonSerializer.Serialize(role) + ", application: " + application);
            }
            catch (Exception e)
            {
                LogError(e, "A", "role: " + JsonSerializer.Serialize(role) + ", application: " + application);
            }
            Response.StatusCode = 500;
            return new();
        }

        /// <summary>
        /// Elimina un rol de una aplicación de la base de datos
        /// </summary>
        /// <param name="role">Rol a eliminarle a la aplicación</param>
        /// <param name="application">Aplicación al que se le elimina el rol</param>
        /// <returns>Rol eliminado</returns>
        [HttpDelete("{application:int}/role/{role:int}")]
        public Role DeleteRole(int role, int application)
        {
            try
            {
                Role result = ((IBusinessApplication)_business).DeleteRole(new() { Id = role }, new() { Id = application }, new() { Id = int.Parse(HttpContext.User.Claims.First(x => x.Type == "id").Value) });
                LogInfo("role: " + role + ", application: " + application, JsonSerializer.Serialize(result));
                return result;
            }
            catch (PersistentException e)
            {
                LogError(e, "P", "role: " + role + ", application: " + application);
            }
            catch (BusinessException e)
            {
                LogError(e, "B", "role: " + role + ", application: " + application);
            }
            catch (Exception e)
            {
                LogError(e, "A", "role: " + role + ", application: " + application);
            }
            Response.StatusCode = 500;
            return new();
        }

        /// <inheritdoc />
        protected override Application GetNewObject(int id)
        {
            return new Application() { Id = id };
        }

        /// <inheritdoc />
        protected override bool ObjectIsDefault(Application obj)
        {
            return obj.Id == 0;
        }
        #endregion
    }
}

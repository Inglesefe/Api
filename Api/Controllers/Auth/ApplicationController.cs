using Business.Auth;
using Business.Exceptions;
using Dal.Dto;
using Dal.Exceptions;
using Entities.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;

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
        /// <param name="configuration">Configuración de la aplicación</param>
        public ApplicationController(IConfiguration configuration) : base(
                  configuration,
                  new MySqlConnection(configuration.GetConnectionString("golden")),
                  new BusinessApplication(new MySqlConnection(configuration.GetConnectionString("golden"))))
        { }
        #endregion

        #region Methods
        /// <summary>
        /// Trae un listado de usuarios desde la base de datos
        /// </summary>
        /// <param name="filters">Filtros aplicados a la consulta</param>
        /// <param name="orders">Ordenamientos aplicados a la base de datos</param>
        /// <param name="limit">Límite de registros a traer</param>
        /// <param name="offset">Corrimiento desde el que se cuenta el número de registros</param>
        /// <returns>Listado de usuarios</returns>
        [HttpGet]
        public override ListResult<Application> List(string? filters, string? orders, int limit, int offset)
        {
            try
            {
                LogInfo("Leer listado de aplicaciones");
                return _business.List(filters ?? "", orders ?? "", limit, offset);
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
        /// Consulta una aplicación dado su identificador
        /// </summary>
        /// <param name="id">Identificador de la aplicación a consultar</param>
        /// <returns>Aplicación con los datos cargados desde la base de datos o null si no lo pudo encontrar</returns>
        [HttpGet("{id:int}")]
        public override Application Read(int id)
        {
            try
            {
                LogInfo("Leer la aplicación " + id);
                Application application = _business.Read(new() { Id = id });
                if (application != null)
                {
                    return application;
                }
                else
                {
                    LogInfo("Aplicación " + id + " no encontrada");
                    Response.StatusCode = 404;
                    return new Application();
                }
            }
            catch (PersistentException e)
            {
                LogError(e, "P");
                Response.StatusCode = 500;
                return new Application();
            }
            catch (BusinessException e)
            {
                LogError(e, "B");
                Response.StatusCode = 500;
                return new Application();
            }
            catch (Exception e)
            {
                LogError(e, "A");
                Response.StatusCode = 500;
                return new Application();
            }
        }

        /// <summary>
        /// Inserta una aplicación en la base de datos
        /// </summary>
        /// <param name="entity">Aplicación a insertar</param>
        /// <returns>Aplicación insertada con el id generado por la base de datos</returns>
        [HttpPost]
        public override Application Insert([FromBody] Application entity)
        {
            try
            {
                LogInfo("Insertar la aplicación " + entity.Id);
                return _business.Insert(entity, new() { Id = int.Parse(HttpContext.User.Claims.First(x => x.Type == "id").Value) });
            }
            catch (PersistentException e)
            {
                LogError(e, "P");
                Response.StatusCode = 500;
                return new Application();
            }
            catch (BusinessException e)
            {
                LogError(e, "B");
                Response.StatusCode = 500;
                return new Application();
            }
            catch (Exception e)
            {
                LogError(e, "A");
                Response.StatusCode = 500;
                return new Application();
            }
        }

        /// <summary>
        /// Actualiza una aplicación en la base de datos
        /// </summary>
        /// <param name="entity">Aplicación a actualizar</param>
        /// <returns>Aplicación actualizada</returns>
        [HttpPut]
        public override Application Update([FromBody] Application entity)
        {
            try
            {
                LogInfo("Actualizar la aplicación " + entity.Id);
                return _business.Update(entity, new() { Id = int.Parse(HttpContext.User.Claims.First(x => x.Type == "id").Value) });
            }
            catch (PersistentException e)
            {
                LogError(e, "P");
                Response.StatusCode = 500;
                return new Application();
            }
            catch (BusinessException e)
            {
                LogError(e, "B");
                Response.StatusCode = 500;
                return new Application();
            }
            catch (Exception e)
            {
                LogError(e, "A");
                Response.StatusCode = 500;
                return new Application();
            }
        }

        /// <summary>
        /// Elimina una aplicación de la base de datos
        /// </summary>
        /// <param name="id">Aplicación a eliminar</param>
        /// <returns>Aplicación eliminada</returns>
        [HttpDelete("{id:int}")]
        public override Application Delete(int id)
        {
            try
            {
                LogInfo("Actualizar la aplicación " + id);
                return _business.Delete(new() { Id = id }, new() { Id = int.Parse(HttpContext.User.Claims.First(x => x.Type == "id").Value) });
            }
            catch (PersistentException e)
            {
                LogError(e, "P");
                Response.StatusCode = 500;
                return new Application();
            }
            catch (BusinessException e)
            {
                LogError(e, "B");
                Response.StatusCode = 500;
                return new Application();
            }
            catch (Exception e)
            {
                LogError(e, "A");
                Response.StatusCode = 500;
                return new Application();
            }
        }

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
                LogInfo("Listar los roles asignados a la aplicación " + application);
                return ((BusinessApplication)_business).ListRoles(filters ?? "", orders ?? "", limit, offset, new() { Id = application });
            }
            catch (PersistentException e)
            {
                LogError(e, "P");
                Response.StatusCode = 500;
                return new ListResult<Role>(new List<Role>(), 0);
            }
            catch (BusinessException e)
            {
                LogError(e, "B");
                Response.StatusCode = 500;
                return new ListResult<Role>(new List<Role>(), 0);
            }
            catch (Exception e)
            {
                LogError(e, "A");
                Response.StatusCode = 500;
                return new ListResult<Role>(new List<Role>(), 0);
            }
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
                LogInfo("Listar los roles no asignados a la aplicación " + application);
                return ((BusinessApplication)_business).ListNotRoles(filters ?? "", orders ?? "", limit, offset, new() { Id = application });
            }
            catch (PersistentException e)
            {
                LogError(e, "P");
                Response.StatusCode = 500;
                return new ListResult<Role>(new List<Role>(), 0);
            }
            catch (BusinessException e)
            {
                LogError(e, "B");
                Response.StatusCode = 500;
                return new ListResult<Role>(new List<Role>(), 0);
            }
            catch (Exception e)
            {
                LogError(e, "A");
                Response.StatusCode = 500;
                return new ListResult<Role>(new List<Role>(), 0);
            }
        }

        /// <summary>
        /// Asigna un rol a una aplicación en la base de datos
        /// </summary>
        /// <param name="role">Rol que se asigna al usuario</param>
        /// <param name="application">Aplicación al que se le asigna el rol</param>
        /// <returns>Rol asignado</returns>
        [HttpPost("{application:int}/role")]
        public Role InsertRole([FromBody] Role role, int application)
        {
            try
            {
                LogInfo("Asigna el rol " + role.Id + " a la aplicación " + application);
                return ((BusinessApplication)_business).InsertRole(role, new() { Id = application }, new() { Id = int.Parse(HttpContext.User.Claims.First(x => x.Type == "id").Value) });
            }
            catch (PersistentException e)
            {
                LogError(e, "P");
                Response.StatusCode = 500;
                return new Role();
            }
            catch (BusinessException e)
            {
                LogError(e, "B");
                Response.StatusCode = 500;
                return new Role();
            }
            catch (Exception e)
            {
                LogError(e, "A");
                Response.StatusCode = 500;
                return new Role();
            }
        }

        /// <summary>
        /// Elimina un rol de una aplicación de la base de datos
        /// </summary>
        /// <param name="role">Rol a eliminarle al usuario</param>
        /// <param name="application">Aplicación al que se le elimina el rol</param>
        /// <returns>Rol eliminado</returns>
        [HttpDelete("{application:int}/role/{role:int}")]
        public Role DeleteRole(int role, int application)
        {
            try
            {
                LogInfo("Elimina el rol " + role + " de la aplicación " + application);
                return ((BusinessApplication)_business).DeleteRole(new() { Id = role }, new() { Id = application }, new() { Id = int.Parse(HttpContext.User.Claims.First(x => x.Type == "id").Value) });
            }
            catch (PersistentException e)
            {
                LogError(e, "P");
                Response.StatusCode = 500;
                return new Role();
            }
            catch (BusinessException e)
            {
                LogError(e, "B");
                Response.StatusCode = 500;
                return new Role();
            }
            catch (Exception e)
            {
                LogError(e, "A");
                Response.StatusCode = 500;
                return new Role();
            }
        }
        #endregion
    }
}

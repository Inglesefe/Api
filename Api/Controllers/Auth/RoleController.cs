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
        public RoleController(IConfiguration configuration, IBusinessRole business, IPersistentBase<LogComponent> log, IBusiness<Template> templateError) : base(
            configuration,
            business,
            log,
            templateError)
        { }
        #endregion

        #region Methods
        /// <summary>
        /// Trae un listado de roles desde la base de datos
        /// </summary>
        /// <param name="filters">Filtros aplicados a la consulta</param>
        /// <param name="orders">Ordenamientos aplicados a la base de datos</param>
        /// <param name="limit">Límite de registros a traer</param>
        /// <param name="offset">Corrimiento desde el que se cuenta el número de registros</param>
        /// <returns>Listado de roles</returns>
        [HttpGet]
        public override ListResult<Role> List(string? filters, string? orders, int limit, int offset)
        {
            try
            {
                LogInfo("Leer listado de roles");
                return _business.List(filters ?? "", orders ?? "", limit, offset);
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
        /// Consulta un rol dado su identificador
        /// </summary>
        /// <param name="id">Identificador del rol a consultar</param>
        /// <returns>Rol con los datos cargados desde la base de datos o null si no lo pudo encontrar</returns>
        [HttpGet("{id:int}")]
        public override Role Read(int id)
        {
            try
            {
                LogInfo("Leer el rol " + id);
                Role user = _business.Read(new() { Id = id });
                if (user != null)
                {
                    return user;
                }
                else
                {
                    LogInfo("Rol " + id + " no encontrado");
                    Response.StatusCode = 404;
                    return new Role();
                }
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
        /// Inserta un rol en la base de datos
        /// </summary>
        /// <param name="entity">Rol a insertar</param>
        /// <returns>Rol insertado con el id generado por la base de datos</returns>
        [HttpPost]
        public override Role Insert([FromBody] Role entity)
        {
            try
            {
                LogInfo("Insertar el rol " + entity.Id);
                return _business.Insert(entity, new() { Id = int.Parse(HttpContext.User.Claims.First(x => x.Type == "id").Value) });
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
        /// Actualiza un rol en la base de datos
        /// </summary>
        /// <param name="entity">Rol a actualizar</param>
        /// <returns>Rol actualizado</returns>
        [HttpPut]
        public override Role Update([FromBody] Role entity)
        {
            try
            {
                LogInfo("Actualizar el rol " + entity.Id);
                return _business.Update(entity, new() { Id = int.Parse(HttpContext.User.Claims.First(x => x.Type == "id").Value) });
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
        /// Elimina un rol de la base de datos
        /// </summary>
        /// <param name="id">Identificador del rol a eliminar</param>
        /// <returns>Rol eliminado</returns>
        [HttpDelete("{id:int}")]
        public override Role Delete(int id)
        {
            try
            {
                LogInfo("Actualizar el rol " + id);
                return _business.Delete(new() { Id = id }, new() { Id = int.Parse(HttpContext.User.Claims.First(x => x.Type == "id").Value) });
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
                LogInfo("Listar los usuarios asignados al rol " + role);
                return ((IBusinessRole)_business).ListUsers(filters ?? "", orders ?? "", limit, offset, new() { Id = role });
            }
            catch (PersistentException e)
            {
                LogError(e, "P");
                Response.StatusCode = 500;
                return new ListResult<User>(new List<User>(), 0);
            }
            catch (BusinessException e)
            {
                LogError(e, "B");
                Response.StatusCode = 500;
                return new ListResult<User>(new List<User>(), 0);
            }
            catch (Exception e)
            {
                LogError(e, "A");
                Response.StatusCode = 500;
                return new ListResult<User>(new List<User>(), 0);
            }
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
                LogInfo("Listar los usuarios no asignados al rol " + role);
                return ((IBusinessRole)_business).ListNotUsers(filters ?? "", orders ?? "", limit, offset, new() { Id = role });
            }
            catch (PersistentException e)
            {
                LogError(e, "P");
                Response.StatusCode = 500;
                return new ListResult<User>(new List<User>(), 0);
            }
            catch (BusinessException e)
            {
                LogError(e, "B");
                Response.StatusCode = 500;
                return new ListResult<User>(new List<User>(), 0);
            }
            catch (Exception e)
            {
                LogError(e, "A");
                Response.StatusCode = 500;
                return new ListResult<User>(new List<User>(), 0);
            }
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
                LogInfo("Asigna el rol " + role + " al usuario " + user.Id);
                return ((IBusinessRole)_business).InsertUser(user, new() { Id = role }, new() { Id = int.Parse(HttpContext.User.Claims.First(x => x.Type == "id").Value) });
            }
            catch (PersistentException e)
            {
                LogError(e, "P");
                Response.StatusCode = 500;
                return new User();
            }
            catch (BusinessException e)
            {
                LogError(e, "B");
                Response.StatusCode = 500;
                return new User();
            }
            catch (Exception e)
            {
                LogError(e, "A");
                Response.StatusCode = 500;
                return new User();
            }
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
                LogInfo("Elimina el rol " + role + " del usuario " + user);
                return ((IBusinessRole)_business).DeleteUser(new() { Id = user }, new() { Id = role }, new() { Id = int.Parse(HttpContext.User.Claims.First(x => x.Type == "id").Value) });
            }
            catch (PersistentException e)
            {
                LogError(e, "P");
                Response.StatusCode = 500;
                return new User();
            }
            catch (BusinessException e)
            {
                LogError(e, "B");
                Response.StatusCode = 500;
                return new User();
            }
            catch (Exception e)
            {
                LogError(e, "A");
                Response.StatusCode = 500;
                return new User();
            }
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
                LogInfo("Listar las aplicaciones asignadas al rol " + role);
                return ((IBusinessRole)_business).ListApplications(filters ?? "", orders ?? "", limit, offset, new() { Id = role });
            }
            catch (PersistentException e)
            {
                LogError(e, "P");
                Response.StatusCode = 500;
                return new ListResult<Application>(new List<Application>(), 0);
            }
            catch (BusinessException e)
            {
                LogError(e, "B");
                Response.StatusCode = 500;
                return new ListResult<Application>(new List<Application>(), 0);
            }
            catch (Exception e)
            {
                LogError(e, "A");
                Response.StatusCode = 500;
                return new ListResult<Application>(new List<Application>(), 0);
            }
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
                LogInfo("Listar los usuarios no asignados al rol " + role);
                return ((IBusinessRole)_business).ListNotApplications(filters ?? "", orders ?? "", limit, offset, new() { Id = role });
            }
            catch (PersistentException e)
            {
                LogError(e, "P");
                Response.StatusCode = 500;
                return new ListResult<Application>(new List<Application>(), 0);
            }
            catch (BusinessException e)
            {
                LogError(e, "B");
                Response.StatusCode = 500;
                return new ListResult<Application>(new List<Application>(), 0);
            }
            catch (Exception e)
            {
                LogError(e, "A");
                Response.StatusCode = 500;
                return new ListResult<Application>(new List<Application>(), 0);
            }
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
                LogInfo("Asigna el rol " + role + " a la aplicación " + application.Id);
                return ((IBusinessRole)_business).InsertApplication(application, new() { Id = role }, new() { Id = int.Parse(HttpContext.User.Claims.First(x => x.Type == "id").Value) });
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
                LogInfo("Elimina el rol " + role + " de la aplicación " + application);
                return ((IBusinessRole)_business).DeleteApplication(new() { Id = application }, new() { Id = role }, new() { Id = int.Parse(HttpContext.User.Claims.First(x => x.Type == "id").Value) });
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
        #endregion
    }
}

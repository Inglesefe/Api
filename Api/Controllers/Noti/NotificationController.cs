using Business;
using Business.Exceptions;
using Dal;
using Dal.Dto;
using Dal.Exceptions;
using Entities.Log;
using Entities.Noti;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers.Noti
{
    /// <summary>
    /// Controlador con los métodos necesarios para administrar notificaciones
    /// </summary>
    [ApiController]
    [Route("[controller]")]
    [Authorize(Policy = "db")]
    public class NotificationController : ControllerBase<Notification>
    {
        #region Constructors
        /// <summary>
        /// Inicializa la configuración del controlador
        /// </summary>
        /// <param name="configuration">Configuración de la aplicación</param>
        /// <param name="business">Capa de negocio de notificaciones</param>
        /// <param name="log">Administrador de logs en la base de datos</param>
        /// <param name="templateError">Administrador de notificaciones de error</param>
        public NotificationController(IConfiguration configuration, IBusiness<Notification> business, IPersistentBase<LogComponent> log, IBusiness<Template> templateError) : base(
                  configuration,
                  business,
                  log,
                  templateError)
        { }
        #endregion

        #region Methods
        /// <summary>
        /// Trae un listado de notificaciones desde la base de datos
        /// </summary>
        /// <param name="filters">Filtros aplicados a la consulta</param>
        /// <param name="orders">Ordenamientos aplicados a la base de datos</param>
        /// <param name="limit">Límite de registros a traer</param>
        /// <param name="offset">Corrimiento desde el que se cuenta el número de registros</param>
        /// <returns>Listado de notificaciones</returns>
        [HttpGet]
        public override ListResult<Notification> List(string? filters, string? orders, int limit, int offset)
        {
            try
            {
                LogInfo("Leer listado de notificaciones");
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
            return new ListResult<Notification>(new List<Notification>(), 0);
        }

        /// <summary>
        /// Consulta una notificación dado su identificador
        /// </summary>
        /// <param name="id">Identificador de la notificación a consultar</param>
        /// <returns>Notificación con los datos cargados desde la base de datos o null si no lo pudo encontrar</returns>
        [HttpGet("{id:int}")]
        public override Notification Read(int id)
        {
            try
            {
                LogInfo("Leer la notificación " + id);
                Notification notification = _business.Read(new() { Id = id });
                if (notification != null)
                {
                    return notification;
                }
                else
                {
                    LogInfo("Notificación " + id + " no encontrada");
                    Response.StatusCode = 404;
                    return new Notification();
                }
            }
            catch (PersistentException e)
            {
                LogError(e, "P");
                Response.StatusCode = 500;
                return new Notification();
            }
            catch (BusinessException e)
            {
                LogError(e, "B");
                Response.StatusCode = 500;
                return new Notification();
            }
            catch (Exception e)
            {
                LogError(e, "A");
                Response.StatusCode = 500;
                return new Notification();
            }
        }

        /// <summary>
        /// Inserta una notificación en la base de datos
        /// </summary>
        /// <param name="entity">Notificación a insertar</param>
        /// <returns>Notificación insertada con el id generado por la base de datos</returns>
        [HttpPost]
        public override Notification Insert([FromBody] Notification entity)
        {
            try
            {
                LogInfo("Insertar la notificación " + entity.Id);
                return _business.Insert(entity, new() { Id = int.Parse(HttpContext.User.Claims.First(x => x.Type == "id").Value) });
            }
            catch (PersistentException e)
            {
                LogError(e, "P");
                Response.StatusCode = 500;
                return new Notification();
            }
            catch (BusinessException e)
            {
                LogError(e, "B");
                Response.StatusCode = 500;
                return new Notification();
            }
            catch (Exception e)
            {
                LogError(e, "A");
                Response.StatusCode = 500;
                return new Notification();
            }
        }

        /// <summary>
        /// Actualiza una notificación en la base de datos
        /// </summary>
        /// <param name="entity">Notificación a actualizar</param>
        /// <returns>Notificación actualizada</returns>
        [HttpPut]
        public override Notification Update([FromBody] Notification entity)
        {
            try
            {
                LogInfo("Actualizar la notificación " + entity.Id);
                return _business.Update(entity, new() { Id = int.Parse(HttpContext.User.Claims.First(x => x.Type == "id").Value) });
            }
            catch (PersistentException e)
            {
                LogError(e, "P");
                Response.StatusCode = 500;
                return new Notification();
            }
            catch (BusinessException e)
            {
                LogError(e, "B");
                Response.StatusCode = 500;
                return new Notification();
            }
            catch (Exception e)
            {
                LogError(e, "A");
                Response.StatusCode = 500;
                return new Notification();
            }
        }

        /// <summary>
        /// Elimina una notificación de la base de datos
        /// </summary>
        /// <param name="id">Notificación a eliminar</param>
        /// <returns>Notificación eliminada</returns>
        [HttpDelete("{id:int}")]
        public override Notification Delete(int id)
        {
            try
            {
                LogInfo("Actualizar la notificación " + id);
                return _business.Delete(new() { Id = id }, new() { Id = int.Parse(HttpContext.User.Claims.First(x => x.Type == "id").Value) });
            }
            catch (PersistentException e)
            {
                LogError(e, "P");
                Response.StatusCode = 500;
                return new Notification();
            }
            catch (BusinessException e)
            {
                LogError(e, "B");
                Response.StatusCode = 500;
                return new Notification();
            }
            catch (Exception e)
            {
                LogError(e, "A");
                Response.StatusCode = 500;
                return new Notification();
            }
        }
        #endregion
    }
}

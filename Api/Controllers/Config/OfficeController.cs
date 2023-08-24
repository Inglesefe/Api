using Business;
using Business.Exceptions;
using Dal;
using Dal.Dto;
using Dal.Exceptions;
using Entities.Config;
using Entities.Log;
using Entities.Noti;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers.Config
{
    /// <summary>
    /// Controlador con los métodos necesarios para administrar oficinas
    /// </summary>
    [ApiController]
    [Route("[controller]")]
    [Authorize(Policy = "db")]
    public class OfficeController : ControllerBase<Office>
    {
        #region Constructors
        /// <summary>
        /// Inicializa la configuración del controlador
        /// </summary>
        /// <param name="configuration">Configuración de la oficina</param>
        /// <param name="business">Capa de negocio de oficinas</param>
        /// <param name="log">Administrador de logs en la base de datos</param>
        /// <param name="templateError">Administrador de notificaciones de error</param>
        public OfficeController(IConfiguration configuration, IBusiness<Office> business, IPersistentBase<LogComponent> log, IBusiness<Template> templateError) : base(
                  configuration,
                  business,
                  log,
                  templateError)
        { }
        #endregion

        #region Methods
        /// <summary>
        /// Trae un listado de oficinas desde la base de datos
        /// </summary>
        /// <param name="filters">Filtros aplicados a la consulta</param>
        /// <param name="orders">Ordenamientos aplicados a la base de datos</param>
        /// <param name="limit">Límite de registros a traer</param>
        /// <param name="offset">Corrimiento desde el que se cuenta el número de registros</param>
        /// <returns>Listado de oficinas</returns>
        [HttpGet]
        public override ListResult<Office> List(string? filters, string? orders, int limit, int offset)
        {
            try
            {
                LogInfo("Leer listado de oficinas");
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
            return new ListResult<Office>(new List<Office>(), 0);
        }

        /// <summary>
        /// Consulta una oficina dado su identificador
        /// </summary>
        /// <param name="id">Identificador de la oficina a consultar</param>
        /// <returns>Ciudad con los datos cargados desde la base de datos o null si no lo pudo encontrar</returns>
        [HttpGet("{id:int}")]
        public override Office Read(int id)
        {
            try
            {
                LogInfo("Leer la oficina " + id);
                Office country = _business.Read(new() { Id = id });
                if (country != null)
                {
                    return country;
                }
                else
                {
                    LogInfo("Ciudad " + id + " no encontrada");
                    Response.StatusCode = 404;
                    return new();
                }
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
        /// Inserta una oficina en la base de datos
        /// </summary>
        /// <param name="entity">Ciudad a insertar</param>
        /// <returns>Ciudad insertada con el id generado por la base de datos</returns>
        [HttpPost]
        public override Office Insert([FromBody] Office entity)
        {
            try
            {
                LogInfo("Insertar la oficina " + entity.Id);
                return _business.Insert(entity, new() { Id = int.Parse(HttpContext.User.Claims.First(x => x.Type == "id").Value) });
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
        /// Actualiza una oficina en la base de datos
        /// </summary>
        /// <param name="entity">Ciudad a actualizar</param>
        /// <returns>Ciudad actualizada</returns>
        [HttpPut]
        public override Office Update([FromBody] Office entity)
        {
            try
            {
                LogInfo("Actualizar la oficina " + entity.Id);
                return _business.Update(entity, new() { Id = int.Parse(HttpContext.User.Claims.First(x => x.Type == "id").Value) });
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
        /// Elimina una oficina de la base de datos
        /// </summary>
        /// <param name="id">Ciudad a eliminar</param>
        /// <returns>Ciudad eliminada</returns>
        [HttpDelete("{id:int}")]
        public override Office Delete(int id)
        {
            try
            {
                LogInfo("Actualizar la oficina " + id);
                return _business.Delete(new() { Id = id }, new() { Id = int.Parse(HttpContext.User.Claims.First(x => x.Type == "id").Value) });
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

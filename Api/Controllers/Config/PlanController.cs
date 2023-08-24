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
    /// Controlador con los métodos necesarios para administrar planes
    /// </summary>
    [ApiController]
    [Route("[controller]")]
    [Authorize(Policy = "db")]
    public class PlanController : ControllerBase<Plan>
    {
        #region Constructors
        /// <summary>
        /// Inicializa la configuración del controlador
        /// </summary>
        /// <param name="configuration">Configuración de la aplicación</param>
        /// <param name="business">Capa de negocio de planes</param>
        /// <param name="log">Administrador de logs en la base de datos</param>
        /// <param name="templateError">Administrador de notificaciones de error</param>
        public PlanController(IConfiguration configuration, IBusiness<Plan> business, IPersistentBase<LogComponent> log, IBusiness<Template> templateError) : base(
                  configuration,
                  business,
                  log,
                  templateError)
        { }
        #endregion

        #region Methods
        /// <summary>
        /// Trae un listado de planes desde la base de datos
        /// </summary>
        /// <param name="filters">Filtros aplicados a la consulta</param>
        /// <param name="orders">Ordenamientos aplicados a la base de datos</param>
        /// <param name="limit">Límite de registros a traer</param>
        /// <param name="offset">Corrimiento desde el que se cuenta el número de registros</param>
        /// <returns>Listado de planes</returns>
        [HttpGet]
        public override ListResult<Plan> List(string? filters, string? orders, int limit, int offset)
        {
            try
            {
                LogInfo("Leer listado de planes");
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
            return new ListResult<Plan>(new List<Plan>(), 0);
        }

        /// <summary>
        /// Consulta un plan dado su identificador
        /// </summary>
        /// <param name="id">Identificador del plan a consultar</param>
        /// <returns>Plan con los datos cargados desde la base de datos o null si no lo pudo encontrar</returns>
        [HttpGet("{id:int}")]
        public override Plan Read(int id)
        {
            try
            {
                LogInfo("Leer el plan " + id);
                Plan country = _business.Read(new() { Id = id });
                if (country != null)
                {
                    return country;
                }
                else
                {
                    LogInfo("Plan " + id + " no encontrado");
                    Response.StatusCode = 404;
                    return new Plan();
                }
            }
            catch (PersistentException e)
            {
                LogError(e, "P");
                Response.StatusCode = 500;
                return new Plan();
            }
            catch (BusinessException e)
            {
                LogError(e, "B");
                Response.StatusCode = 500;
                return new Plan();
            }
            catch (Exception e)
            {
                LogError(e, "A");
                Response.StatusCode = 500;
                return new Plan();
            }
        }

        /// <summary>
        /// Inserta un plan en la base de datos
        /// </summary>
        /// <param name="entity">Plan a insertar</param>
        /// <returns>Plan insertado con el id generado por la base de datos</returns>
        [HttpPost]
        public override Plan Insert([FromBody] Plan entity)
        {
            try
            {
                LogInfo("Insertar el plan " + entity.Id);
                return _business.Insert(entity, new() { Id = int.Parse(HttpContext.User.Claims.First(x => x.Type == "id").Value) });
            }
            catch (PersistentException e)
            {
                LogError(e, "P");
                Response.StatusCode = 500;
                return new Plan();
            }
            catch (BusinessException e)
            {
                LogError(e, "B");
                Response.StatusCode = 500;
                return new Plan();
            }
            catch (Exception e)
            {
                LogError(e, "A");
                Response.StatusCode = 500;
                return new Plan();
            }
        }

        /// <summary>
        /// Actualiza un plan en la base de datos
        /// </summary>
        /// <param name="entity">Plan a actualizar</param>
        /// <returns>Plan actualizada</returns>
        [HttpPut]
        public override Plan Update([FromBody] Plan entity)
        {
            try
            {
                LogInfo("Actualizar el plan " + entity.Id);
                return _business.Update(entity, new() { Id = int.Parse(HttpContext.User.Claims.First(x => x.Type == "id").Value) });
            }
            catch (PersistentException e)
            {
                LogError(e, "P");
                Response.StatusCode = 500;
                return new Plan();
            }
            catch (BusinessException e)
            {
                LogError(e, "B");
                Response.StatusCode = 500;
                return new Plan();
            }
            catch (Exception e)
            {
                LogError(e, "A");
                Response.StatusCode = 500;
                return new Plan();
            }
        }

        /// <summary>
        /// Elimina un plan de la base de datos
        /// </summary>
        /// <param name="id">Plan a eliminar</param>
        /// <returns>Plan eliminada</returns>
        [HttpDelete("{id:int}")]
        public override Plan Delete(int id)
        {
            try
            {
                LogInfo("Eliminar el plan " + id);
                return _business.Delete(new() { Id = id }, new() { Id = int.Parse(HttpContext.User.Claims.First(x => x.Type == "id").Value) });
            }
            catch (PersistentException e)
            {
                LogError(e, "P");
                Response.StatusCode = 500;
                return new Plan();
            }
            catch (BusinessException e)
            {
                LogError(e, "B");
                Response.StatusCode = 500;
                return new Plan();
            }
            catch (Exception e)
            {
                LogError(e, "A");
                Response.StatusCode = 500;
                return new Plan();
            }
        }
        #endregion
    }
}

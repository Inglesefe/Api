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
    /// Controlador con los métodos necesarios para administrar ciudades
    /// </summary>
    [ApiController]
    [Route("[controller]")]
    [Authorize(Policy = "db")]
    public class CityController : ControllerBase<City>
    {
        #region Constructors
        /// <summary>
        /// Inicializa la configuración del controlador
        /// </summary>
        /// <param name="configuration">Configuración de la ciudad</param>
        /// <param name="business">Capa de negocio de ciudades</param>
        /// <param name="log">Administrador de logs en la base de datos</param>
        /// <param name="templateError">Administrador de notificaciones de error</param>
        public CityController(IConfiguration configuration, IBusiness<City> business, IPersistentBase<LogComponent> log, IBusiness<Template> templateError) : base(
                  configuration,
                  business,
                  log,
                  templateError)
        { }
        #endregion

        #region Methods
        /// <summary>
        /// Trae un listado de ciudades desde la base de datos
        /// </summary>
        /// <param name="filters">Filtros aplicados a la consulta</param>
        /// <param name="orders">Ordenamientos aplicados a la base de datos</param>
        /// <param name="limit">Límite de registros a traer</param>
        /// <param name="offset">Corrimiento desde el que se cuenta el número de registros</param>
        /// <returns>Listado de ciudades</returns>
        [HttpGet]
        public override ListResult<City> List(string? filters, string? orders, int limit, int offset)
        {
            try
            {
                LogInfo("Leer listado de ciudades");
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
            return new ListResult<City>(new List<City>(), 0);
        }

        /// <summary>
        /// Consulta una ciudad dado su identificador
        /// </summary>
        /// <param name="id">Identificador de la ciudad a consultar</param>
        /// <returns>Ciudad con los datos cargados desde la base de datos o null si no lo pudo encontrar</returns>
        [HttpGet("{id:int}")]
        public override City Read(int id)
        {
            try
            {
                LogInfo("Leer la ciudad " + id);
                City country = _business.Read(new() { Id = id });
                if (country != null)
                {
                    return country;
                }
                else
                {
                    LogInfo("Ciudad " + id + " no encontrada");
                    Response.StatusCode = 404;
                    return new City();
                }
            }
            catch (PersistentException e)
            {
                LogError(e, "P");
                Response.StatusCode = 500;
                return new City();
            }
            catch (BusinessException e)
            {
                LogError(e, "B");
                Response.StatusCode = 500;
                return new City();
            }
            catch (Exception e)
            {
                LogError(e, "A");
                Response.StatusCode = 500;
                return new City();
            }
        }

        /// <summary>
        /// Inserta una ciudad en la base de datos
        /// </summary>
        /// <param name="entity">Ciudad a insertar</param>
        /// <returns>Ciudad insertada con el id generado por la base de datos</returns>
        [HttpPost]
        public override City Insert([FromBody] City entity)
        {
            try
            {
                LogInfo("Insertar la ciudad " + entity.Id);
                return _business.Insert(entity, new() { Id = int.Parse(HttpContext.User.Claims.First(x => x.Type == "id").Value) });
            }
            catch (PersistentException e)
            {
                LogError(e, "P");
                Response.StatusCode = 500;
                return new City();
            }
            catch (BusinessException e)
            {
                LogError(e, "B");
                Response.StatusCode = 500;
                return new City();
            }
            catch (Exception e)
            {
                LogError(e, "A");
                Response.StatusCode = 500;
                return new City();
            }
        }

        /// <summary>
        /// Actualiza una ciudad en la base de datos
        /// </summary>
        /// <param name="entity">Ciudad a actualizar</param>
        /// <returns>Ciudad actualizada</returns>
        [HttpPut]
        public override City Update([FromBody] City entity)
        {
            try
            {
                LogInfo("Actualizar la ciudad " + entity.Id);
                return _business.Update(entity, new() { Id = int.Parse(HttpContext.User.Claims.First(x => x.Type == "id").Value) });
            }
            catch (PersistentException e)
            {
                LogError(e, "P");
                Response.StatusCode = 500;
                return new City();
            }
            catch (BusinessException e)
            {
                LogError(e, "B");
                Response.StatusCode = 500;
                return new City();
            }
            catch (Exception e)
            {
                LogError(e, "A");
                Response.StatusCode = 500;
                return new City();
            }
        }

        /// <summary>
        /// Elimina una ciudad de la base de datos
        /// </summary>
        /// <param name="id">Ciudad a eliminar</param>
        /// <returns>Ciudad eliminada</returns>
        [HttpDelete("{id:int}")]
        public override City Delete(int id)
        {
            try
            {
                LogInfo("Eliminar la ciudad " + id);
                return _business.Delete(new() { Id = id }, new() { Id = int.Parse(HttpContext.User.Claims.First(x => x.Type == "id").Value) });
            }
            catch (PersistentException e)
            {
                LogError(e, "P");
                Response.StatusCode = 500;
                return new City();
            }
            catch (BusinessException e)
            {
                LogError(e, "B");
                Response.StatusCode = 500;
                return new City();
            }
            catch (Exception e)
            {
                LogError(e, "A");
                Response.StatusCode = 500;
                return new City();
            }
        }
        #endregion
    }
}

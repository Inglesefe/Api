using Business.Config;
using Business.Exceptions;
using Dal.Dto;
using Dal.Exceptions;
using Entities.Config;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;

namespace Api.Controllers.Config
{
    /// <summary>
    /// Controlador con los métodos necesarios para administrar paises
    /// </summary>
    [ApiController]
    [Route("[controller]")]
    [Authorize(Policy = "db")]
    public class CountryController : ControllerBase<Country>
    {
        #region Constructors
        /// <summary>
        /// Inicializa la configuración del controlador
        /// </summary>
        /// <param name="configuration">Configuración del país</param>
        public CountryController(IConfiguration configuration) : base(
                  configuration,
                  new MySqlConnection(configuration.GetConnectionString("golden")),
                  new BusinessCountry(new MySqlConnection(configuration.GetConnectionString("golden"))))
        { }
        #endregion

        #region Methods
        /// <summary>
        /// Trae un listado de paises desde la base de datos
        /// </summary>
        /// <param name="filters">Filtros aplicados a la consulta</param>
        /// <param name="orders">Ordenamientos aplicados a la base de datos</param>
        /// <param name="limit">Límite de registros a traer</param>
        /// <param name="offset">Corrimiento desde el que se cuenta el número de registros</param>
        /// <returns>Listado de paises</returns>
        [HttpGet]
        public override ListResult<Country> List(string? filters, string? orders, int limit, int offset)
        {
            try
            {
                LogInfo("Leer listado de paises");
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
            return new ListResult<Country>(new List<Country>(), 0);
        }

        /// <summary>
        /// Consulta un país dado su identificador
        /// </summary>
        /// <param name="id">Identificador del país a consultar</param>
        /// <returns>País con los datos cargados desde la base de datos o null si no lo pudo encontrar</returns>
        [HttpGet("{id:int}")]
        public override Country Read(int id)
        {
            try
            {
                LogInfo("Leer el país " + id);
                Country country = _business.Read(new() { Id = id });
                if (country != null)
                {
                    return country;
                }
                else
                {
                    LogInfo("País " + id + " no encontrado");
                    Response.StatusCode = 404;
                    return new Country();
                }
            }
            catch (PersistentException e)
            {
                LogError(e, "P");
                Response.StatusCode = 500;
                return new Country();
            }
            catch (BusinessException e)
            {
                LogError(e, "B");
                Response.StatusCode = 500;
                return new Country();
            }
            catch (Exception e)
            {
                LogError(e, "A");
                Response.StatusCode = 500;
                return new Country();
            }
        }

        /// <summary>
        /// Inserta un país en la base de datos
        /// </summary>
        /// <param name="entity">País a insertar</param>
        /// <returns>País insertada con el id generado por la base de datos</returns>
        [HttpPost]
        public override Country Insert([FromBody] Country entity)
        {
            try
            {
                LogInfo("Insertar el país " + entity.Id);
                return _business.Insert(entity, new() { Id = int.Parse(HttpContext.User.Claims.First(x => x.Type == "id").Value) });
            }
            catch (PersistentException e)
            {
                LogError(e, "P");
                Response.StatusCode = 500;
                return new Country();
            }
            catch (BusinessException e)
            {
                LogError(e, "B");
                Response.StatusCode = 500;
                return new Country();
            }
            catch (Exception e)
            {
                LogError(e, "A");
                Response.StatusCode = 500;
                return new Country();
            }
        }

        /// <summary>
        /// Actualiza un país en la base de datos
        /// </summary>
        /// <param name="entity">País a actualizar</param>
        /// <returns>País actualizada</returns>
        [HttpPut]
        public override Country Update([FromBody] Country entity)
        {
            try
            {
                LogInfo("Actualizar el país " + entity.Id);
                return _business.Update(entity, new() { Id = int.Parse(HttpContext.User.Claims.First(x => x.Type == "id").Value) });
            }
            catch (PersistentException e)
            {
                LogError(e, "P");
                Response.StatusCode = 500;
                return new Country();
            }
            catch (BusinessException e)
            {
                LogError(e, "B");
                Response.StatusCode = 500;
                return new Country();
            }
            catch (Exception e)
            {
                LogError(e, "A");
                Response.StatusCode = 500;
                return new Country();
            }
        }

        /// <summary>
        /// Elimina un país de la base de datos
        /// </summary>
        /// <param name="id">País a eliminar</param>
        /// <returns>País eliminada</returns>
        [HttpDelete("{id:int}")]
        public override Country Delete(int id)
        {
            try
            {
                LogInfo("Eliminar el país " + id);
                return _business.Delete(new() { Id = id }, new() { Id = int.Parse(HttpContext.User.Claims.First(x => x.Type == "id").Value) });
            }
            catch (PersistentException e)
            {
                LogError(e, "P");
                Response.StatusCode = 500;
                return new Country();
            }
            catch (BusinessException e)
            {
                LogError(e, "B");
                Response.StatusCode = 500;
                return new Country();
            }
            catch (Exception e)
            {
                LogError(e, "A");
                Response.StatusCode = 500;
                return new Country();
            }
        }
        #endregion
    }
}

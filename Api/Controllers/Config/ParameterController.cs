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
    /// Controlador con los métodos necesarios para administrar parámetros
    /// </summary>
    [ApiController]
    [Route("[controller]")]
    [Authorize(Policy = "db")]
    public class ParameterController : ControllerBase<Parameter>
    {
        #region Constructors
        /// <summary>
        /// Inicializa la configuración del controlador
        /// </summary>
        /// <param name="configuration">Configuración del parámetro</param>
        public ParameterController(IConfiguration configuration) : base(
                  configuration,
                  new MySqlConnection(configuration.GetConnectionString("golden")),
                  new BusinessParameter(new MySqlConnection(configuration.GetConnectionString("golden"))))
        { }
        #endregion

        #region Methods
        /// <summary>
        /// Trae un listado de parámetros desde la base de datos
        /// </summary>
        /// <param name="filters">Filtros aplicados a la consulta</param>
        /// <param name="orders">Ordenamientos aplicados a la base de datos</param>
        /// <param name="limit">Límite de registros a traer</param>
        /// <param name="offset">Corrimiento desde el que se cuenta el número de registros</param>
        /// <returns>Listado de parámetros</returns>
        [HttpGet]
        public override ListResult<Parameter> List(string? filters, string? orders, int limit, int offset)
        {
            try
            {
                LogInfo("Leer listado de parámetros");
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
            return new ListResult<Parameter>(new List<Parameter>(), 0);
        }

        /// <summary>
        /// Consulta un parámetro dado su identificador
        /// </summary>
        /// <param name="id">Identificador del parámetro a consultar</param>
        /// <returns>Parámetro con los datos cargados desde la base de datos o null si no lo pudo encontrar</returns>
        [HttpGet("{id:int}")]
        public override Parameter Read(int id)
        {
            try
            {
                LogInfo("Leer el parámetro " + id);
                Parameter country = _business.Read(new() { Id = id });
                if (country != null)
                {
                    return country;
                }
                else
                {
                    LogInfo("Parámetro " + id + " no encontrado");
                    Response.StatusCode = 404;
                    return new Parameter();
                }
            }
            catch (PersistentException e)
            {
                LogError(e, "P");
                Response.StatusCode = 500;
                return new Parameter();
            }
            catch (BusinessException e)
            {
                LogError(e, "B");
                Response.StatusCode = 500;
                return new Parameter();
            }
            catch (Exception e)
            {
                LogError(e, "A");
                Response.StatusCode = 500;
                return new Parameter();
            }
        }

        /// <summary>
        /// Inserta un parámetro en la base de datos
        /// </summary>
        /// <param name="entity">Parámetro a insertar</param>
        /// <returns>Parámetro insertado con el id generado por la base de datos</returns>
        [HttpPost]
        public override Parameter Insert([FromBody] Parameter entity)
        {
            try
            {
                LogInfo("Insertar el parámetro " + entity.Id);
                return _business.Insert(entity, new() { Id = int.Parse(HttpContext.User.Claims.First(x => x.Type == "id").Value) });
            }
            catch (PersistentException e)
            {
                LogError(e, "P");
                Response.StatusCode = 500;
                return new Parameter();
            }
            catch (BusinessException e)
            {
                LogError(e, "B");
                Response.StatusCode = 500;
                return new Parameter();
            }
            catch (Exception e)
            {
                LogError(e, "A");
                Response.StatusCode = 500;
                return new Parameter();
            }
        }

        /// <summary>
        /// Actualiza un parámetro en la base de datos
        /// </summary>
        /// <param name="entity">Parámetro a actualizar</param>
        /// <returns>Parámetro actualizada</returns>
        [HttpPut]
        public override Parameter Update([FromBody] Parameter entity)
        {
            try
            {
                LogInfo("Actualizar el parámetro " + entity.Id);
                return _business.Update(entity, new() { Id = int.Parse(HttpContext.User.Claims.First(x => x.Type == "id").Value) });
            }
            catch (PersistentException e)
            {
                LogError(e, "P");
                Response.StatusCode = 500;
                return new Parameter();
            }
            catch (BusinessException e)
            {
                LogError(e, "B");
                Response.StatusCode = 500;
                return new Parameter();
            }
            catch (Exception e)
            {
                LogError(e, "A");
                Response.StatusCode = 500;
                return new Parameter();
            }
        }

        /// <summary>
        /// Elimina un parámetro de la base de datos
        /// </summary>
        /// <param name="id">Parámetro a eliminar</param>
        /// <returns>Parámetro eliminada</returns>
        [HttpDelete("{id:int}")]
        public override Parameter Delete(int id)
        {
            try
            {
                LogInfo("Eliminar el parámetro " + id);
                return _business.Delete(new() { Id = id }, new() { Id = int.Parse(HttpContext.User.Claims.First(x => x.Type == "id").Value) });
            }
            catch (PersistentException e)
            {
                LogError(e, "P");
                Response.StatusCode = 500;
                return new Parameter();
            }
            catch (BusinessException e)
            {
                LogError(e, "B");
                Response.StatusCode = 500;
                return new Parameter();
            }
            catch (Exception e)
            {
                LogError(e, "A");
                Response.StatusCode = 500;
                return new Parameter();
            }
        }
        #endregion
    }
}

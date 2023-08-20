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
    /// Controlador con los métodos necesarios para administrar tipos de identificación
    /// </summary>
    [ApiController]
    [Route("[controller]")]
    [Authorize(Policy = "db")]
    public class IdentificationTypeController : ControllerBase<IdentificationType>
    {
        #region Constructors
        /// <summary>
        /// Inicializa la configuración del controlador
        /// </summary>
        /// <param name="configuration">Configuración de el tipo de identificación</param>
        public IdentificationTypeController(IConfiguration configuration) : base(
                  configuration,
                  new MySqlConnection(configuration.GetConnectionString("golden")),
                  new BusinessIdentificationType(new MySqlConnection(configuration.GetConnectionString("golden"))))
        { }
        #endregion

        #region Methods
        /// <summary>
        /// Trae un listado de tipos de identificación desde la base de datos
        /// </summary>
        /// <param name="filters">Filtros aplicados a la consulta</param>
        /// <param name="orders">Ordenamientos aplicados a la base de datos</param>
        /// <param name="limit">Límite de registros a traer</param>
        /// <param name="offset">Corrimiento desde el que se cuenta el número de registros</param>
        /// <returns>Listado de tipos de identificación</returns>
        [HttpGet]
        public override ListResult<IdentificationType> List(string? filters, string? orders, int limit, int offset)
        {
            try
            {
                LogInfo("Leer listado de tipos de identificación");
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
            return new ListResult<IdentificationType>(new List<IdentificationType>(), 0);
        }

        /// <summary>
        /// Consulta un tipo de identificación dado su identificador
        /// </summary>
        /// <param name="id">Identificador de el tipo de identificación a consultar</param>
        /// <returns>Tipo de identificación con los datos cargados desde la base de datos o null si no lo pudo encontrar</returns>
        [HttpGet("{id:int}")]
        public override IdentificationType Read(int id)
        {
            try
            {
                LogInfo("Leer el tipo de identificación " + id);
                IdentificationType country = _business.Read(new() { Id = id });
                if (country != null)
                {
                    return country;
                }
                else
                {
                    LogInfo("Tipo de identificación " + id + " no encontrado");
                    Response.StatusCode = 404;
                    return new IdentificationType();
                }
            }
            catch (PersistentException e)
            {
                LogError(e, "P");
                Response.StatusCode = 500;
                return new IdentificationType();
            }
            catch (BusinessException e)
            {
                LogError(e, "B");
                Response.StatusCode = 500;
                return new IdentificationType();
            }
            catch (Exception e)
            {
                LogError(e, "A");
                Response.StatusCode = 500;
                return new IdentificationType();
            }
        }

        /// <summary>
        /// Inserta un tipo de identificación en la base de datos
        /// </summary>
        /// <param name="entity">Tipo de identificación a insertar</param>
        /// <returns>Tipo de identificación insertada con el id generado por la base de datos</returns>
        [HttpPost]
        public override IdentificationType Insert([FromBody] IdentificationType entity)
        {
            try
            {
                LogInfo("Insertar el tipo de identificación " + entity.Id);
                return _business.Insert(entity, new() { Id = int.Parse(HttpContext.User.Claims.First(x => x.Type == "id").Value) });
            }
            catch (PersistentException e)
            {
                LogError(e, "P");
                Response.StatusCode = 500;
                return new IdentificationType();
            }
            catch (BusinessException e)
            {
                LogError(e, "B");
                Response.StatusCode = 500;
                return new IdentificationType();
            }
            catch (Exception e)
            {
                LogError(e, "A");
                Response.StatusCode = 500;
                return new IdentificationType();
            }
        }

        /// <summary>
        /// Actualiza un tipo de identificación en la base de datos
        /// </summary>
        /// <param name="entity">Tipo de identificación a actualizar</param>
        /// <returns>Tipo de identificación actualizada</returns>
        [HttpPut]
        public override IdentificationType Update([FromBody] IdentificationType entity)
        {
            try
            {
                LogInfo("Actualizar el tipo de identificación " + entity.Id);
                return _business.Update(entity, new() { Id = int.Parse(HttpContext.User.Claims.First(x => x.Type == "id").Value) });
            }
            catch (PersistentException e)
            {
                LogError(e, "P");
                Response.StatusCode = 500;
                return new IdentificationType();
            }
            catch (BusinessException e)
            {
                LogError(e, "B");
                Response.StatusCode = 500;
                return new IdentificationType();
            }
            catch (Exception e)
            {
                LogError(e, "A");
                Response.StatusCode = 500;
                return new IdentificationType();
            }
        }

        /// <summary>
        /// Elimina un tipo de identificación de la base de datos
        /// </summary>
        /// <param name="id">Tipo de identificación a eliminar</param>
        /// <returns>Tipo de identificación eliminada</returns>
        [HttpDelete("{id:int}")]
        public override IdentificationType Delete(int id)
        {
            try
            {
                LogInfo("Eliminar el tipo de identificación " + id);
                return _business.Delete(new() { Id = id }, new() { Id = int.Parse(HttpContext.User.Claims.First(x => x.Type == "id").Value) });
            }
            catch (PersistentException e)
            {
                LogError(e, "P");
                Response.StatusCode = 500;
                return new IdentificationType();
            }
            catch (BusinessException e)
            {
                LogError(e, "B");
                Response.StatusCode = 500;
                return new IdentificationType();
            }
            catch (Exception e)
            {
                LogError(e, "A");
                Response.StatusCode = 500;
                return new IdentificationType();
            }
        }
        #endregion
    }
}

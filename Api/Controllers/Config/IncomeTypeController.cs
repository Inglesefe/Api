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
    /// Controlador con los métodos necesarios para administrar tipos de ingreso
    /// </summary>
    [ApiController]
    [Route("[controller]")]
    [Authorize(Policy = "db")]
    public class IncomeTypeController : ControllerBase<IncomeType>
    {
        #region Constructors
        /// <summary>
        /// Inicializa la configuración del controlador
        /// </summary>
        /// <param name="configuration">Configuración de el tipo de ingreso</param>
        public IncomeTypeController(IConfiguration configuration) : base(
                  configuration,
                  new MySqlConnection(configuration.GetConnectionString("golden")),
                  new BusinessIncomeType(new MySqlConnection(configuration.GetConnectionString("golden"))))
        { }
        #endregion

        #region Methods
        /// <summary>
        /// Trae un listado de tipos de ingreso desde la base de datos
        /// </summary>
        /// <param name="filters">Filtros aplicados a la consulta</param>
        /// <param name="orders">Ordenamientos aplicados a la base de datos</param>
        /// <param name="limit">Límite de registros a traer</param>
        /// <param name="offset">Corrimiento desde el que se cuenta el número de registros</param>
        /// <returns>Listado de tipos de ingreso</returns>
        [HttpGet]
        public override ListResult<IncomeType> List(string? filters, string? orders, int limit, int offset)
        {
            try
            {
                LogInfo("Leer listado de tipos de ingreso");
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
            return new ListResult<IncomeType>(new List<IncomeType>(), 0);
        }

        /// <summary>
        /// Consulta un tipo de ingreso dado su identificador
        /// </summary>
        /// <param name="id">Identificador de el tipo de ingreso a consultar</param>
        /// <returns>Tipo de ingreso con los datos cargados desde la base de datos o null si no lo pudo encontrar</returns>
        [HttpGet("{id:int}")]
        public override IncomeType Read(int id)
        {
            try
            {
                LogInfo("Leer el tipo de ingreso " + id);
                IncomeType country = _business.Read(new() { Id = id });
                if (country != null)
                {
                    return country;
                }
                else
                {
                    LogInfo("Tipo de ingreso " + id + " no encontrado");
                    Response.StatusCode = 404;
                    return new IncomeType();
                }
            }
            catch (PersistentException e)
            {
                LogError(e, "P");
                Response.StatusCode = 500;
                return new IncomeType();
            }
            catch (BusinessException e)
            {
                LogError(e, "B");
                Response.StatusCode = 500;
                return new IncomeType();
            }
            catch (Exception e)
            {
                LogError(e, "A");
                Response.StatusCode = 500;
                return new IncomeType();
            }
        }

        /// <summary>
        /// Inserta un tipo de ingreso en la base de datos
        /// </summary>
        /// <param name="entity">Tipo de ingreso a insertar</param>
        /// <returns>Tipo de ingreso insertada con el id generado por la base de datos</returns>
        [HttpPost]
        public override IncomeType Insert([FromBody] IncomeType entity)
        {
            try
            {
                LogInfo("Insertar el tipo de ingreso " + entity.Id);
                return _business.Insert(entity, new() { Id = int.Parse(HttpContext.User.Claims.First(x => x.Type == "id").Value) });
            }
            catch (PersistentException e)
            {
                LogError(e, "P");
                Response.StatusCode = 500;
                return new IncomeType();
            }
            catch (BusinessException e)
            {
                LogError(e, "B");
                Response.StatusCode = 500;
                return new IncomeType();
            }
            catch (Exception e)
            {
                LogError(e, "A");
                Response.StatusCode = 500;
                return new IncomeType();
            }
        }

        /// <summary>
        /// Actualiza un tipo de ingreso en la base de datos
        /// </summary>
        /// <param name="entity">Tipo de ingreso a actualizar</param>
        /// <returns>Tipo de ingreso actualizada</returns>
        [HttpPut]
        public override IncomeType Update([FromBody] IncomeType entity)
        {
            try
            {
                LogInfo("Actualizar el tipo de ingreso " + entity.Id);
                return _business.Update(entity, new() { Id = int.Parse(HttpContext.User.Claims.First(x => x.Type == "id").Value) });
            }
            catch (PersistentException e)
            {
                LogError(e, "P");
                Response.StatusCode = 500;
                return new IncomeType();
            }
            catch (BusinessException e)
            {
                LogError(e, "B");
                Response.StatusCode = 500;
                return new IncomeType();
            }
            catch (Exception e)
            {
                LogError(e, "A");
                Response.StatusCode = 500;
                return new IncomeType();
            }
        }

        /// <summary>
        /// Elimina un tipo de ingreso de la base de datos
        /// </summary>
        /// <param name="id">Tipo de ingreso a eliminar</param>
        /// <returns>Tipo de ingreso eliminada</returns>
        [HttpDelete("{id:int}")]
        public override IncomeType Delete(int id)
        {
            try
            {
                LogInfo("Eliminar el tipo de ingreso " + id);
                return _business.Delete(new() { Id = id }, new() { Id = int.Parse(HttpContext.User.Claims.First(x => x.Type == "id").Value) });
            }
            catch (PersistentException e)
            {
                LogError(e, "P");
                Response.StatusCode = 500;
                return new IncomeType();
            }
            catch (BusinessException e)
            {
                LogError(e, "B");
                Response.StatusCode = 500;
                return new IncomeType();
            }
            catch (Exception e)
            {
                LogError(e, "A");
                Response.StatusCode = 500;
                return new IncomeType();
            }
        }
        #endregion
    }
}

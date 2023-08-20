using Business.Exceptions;
using Business.Noti;
using Dal.Dto;
using Dal.Exceptions;
using Entities.Noti;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;

namespace Api.Controllers.Noti
{
    /// <summary>
    /// Controlador con los métodos necesarios para administrar plantillas
    /// </summary>
    [ApiController]
    [Route("[controller]")]
    [Authorize(Policy = "db")]
    public class TemplateController : ControllerBase<Template>
    {
        #region Constructors
        /// <summary>
        /// Inicializa la configuración del controlador
        /// </summary>
        /// <param name="configuration">Configuración de la aplicación</param>
        public TemplateController(IConfiguration configuration) : base(configuration, new MySqlConnection(configuration.GetConnectionString("golden")), new BusinessTemplate(new MySqlConnection(configuration.GetConnectionString("golden")))) { }
        #endregion

        #region Methods
        /// <summary>
        /// Trae un listado de plantillas desde la base de datos
        /// </summary>
        /// <param name="filters">Filtros aplicados a la consulta</param>
        /// <param name="orders">Ordenamientos aplicados a la base de datos</param>
        /// <param name="limit">Límite de registros a traer</param>
        /// <param name="offset">Corrimiento desde el que se cuenta el número de registros</param>
        /// <returns>Listado de plantillas</returns>
        [HttpGet]
        public override ListResult<Template> List(string? filters, string? orders, int limit, int offset)
        {
            try
            {
                LogInfo("Leer listado de plantillas");
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
            return new ListResult<Template>(new List<Template>(), 0);
        }

        /// <summary>
        /// Consulta una plantilla dado su identificador
        /// </summary>
        /// <param name="id">Identificador de la plantilla a consultar</param>
        /// <returns>Plantilla con los datos cargados desde la base de datos o null si no lo pudo encontrar</returns>
        [HttpGet("{id:int}")]
        public override Template Read(int id)
        {
            try
            {
                LogInfo("Leer la plantilla " + id);
                Template template = _business.Read(new() { Id = id });
                if (template != null)
                {
                    return template;
                }
                else
                {
                    LogInfo("Plantilla " + id + " no encontrada");
                    Response.StatusCode = 404;
                    return new Template();
                }
            }
            catch (PersistentException e)
            {
                LogError(e, "P");
                Response.StatusCode = 500;
                return new Template();
            }
            catch (BusinessException e)
            {
                LogError(e, "B");
                Response.StatusCode = 500;
                return new Template();
            }
            catch (Exception e)
            {
                LogError(e, "A");
                Response.StatusCode = 500;
                return new Template();
            }
        }

        /// <summary>
        /// Inserta una plantilla en la base de datos
        /// </summary>
        /// <param name="entity">Plantilla a insertar</param>
        /// <returns>Plantilla insertada con el id generado por la base de datos</returns>
        [HttpPost]
        public override Template Insert([FromBody] Template entity)
        {
            try
            {
                LogInfo("Insertar la plantilla " + entity.Id);
                return _business.Insert(entity, new() { Id = int.Parse(HttpContext.User.Claims.First(x => x.Type == "id").Value) });
            }
            catch (PersistentException e)
            {
                LogError(e, "P");
                Response.StatusCode = 500;
                return new Template();
            }
            catch (BusinessException e)
            {
                LogError(e, "B");
                Response.StatusCode = 500;
                return new Template();
            }
            catch (Exception e)
            {
                LogError(e, "A");
                Response.StatusCode = 500;
                return new Template();
            }
        }

        /// <summary>
        /// Actualiza una plantilla en la base de datos
        /// </summary>
        /// <param name="entity">Plantilla a actualizar</param>
        /// <returns>Plantilla actualizada</returns>
        [HttpPut]
        public override Template Update([FromBody] Template entity)
        {
            try
            {
                LogInfo("Actualizar la plantilla " + entity.Id);
                return _business.Update(entity, new() { Id = int.Parse(HttpContext.User.Claims.First(x => x.Type == "id").Value) });
            }
            catch (PersistentException e)
            {
                LogError(e, "P");
                Response.StatusCode = 500;
                return new Template();
            }
            catch (BusinessException e)
            {
                LogError(e, "B");
                Response.StatusCode = 500;
                return new Template();
            }
            catch (Exception e)
            {
                LogError(e, "A");
                Response.StatusCode = 500;
                return new Template();
            }
        }

        /// <summary>
        /// Elimina una plantilla de la base de datos
        /// </summary>
        /// <param name="id">Plantilla a eliminar</param>
        /// <returns>Plantilla eliminada</returns>
        [HttpDelete("{id:int}")]
        public override Template Delete(int id)
        {
            try
            {
                LogInfo("Actualizar la plantilla " + id);
                return _business.Delete(new() { Id = id }, new() { Id = int.Parse(HttpContext.User.Claims.First(x => x.Type == "id").Value) });
            }
            catch (PersistentException e)
            {
                LogError(e, "P");
                Response.StatusCode = 500;
                return new Template();
            }
            catch (BusinessException e)
            {
                LogError(e, "B");
                Response.StatusCode = 500;
                return new Template();
            }
            catch (Exception e)
            {
                LogError(e, "A");
                Response.StatusCode = 500;
                return new Template();
            }
        }
        #endregion
    }
}

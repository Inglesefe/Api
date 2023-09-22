using Business;
using Business.Dto;
using Business.Exceptions;
using Business.Noti;
using Dal;
using Dal.Dto;
using Dal.Exceptions;
using Entities;
using Entities.Config;
using Entities.Log;
using Entities.Noti;
using Microsoft.AspNetCore.Mvc;
using System.Data;
using System.Security.Claims;
using System.Text;
using System.Text.Json;

namespace Api.Controllers
{
    /// <summary>
    /// Clase base de los controladores del api
    /// </summary>
    public abstract class ControllerBase<T> : ControllerBase where T : IEntity, new()
    {
        #region Attributes
        /// <summary>
        /// Configuración de la aplicación
        /// </summary>
        protected readonly IConfiguration _configuration;

        /// <summary>
        /// Capa de negocio asociada a la entidad
        /// </summary>
        protected readonly IBusiness<T> _business;

        /// <summary>
        /// Administrador de logs en la base de datos
        /// </summary>
        protected readonly IPersistent<LogComponent> _log;

        /// <summary>
        /// Administrador de plantilla de errores
        /// </summary>
        protected readonly IBusiness<Template> _templateError;

        /// <summary>
        /// Administrador de parámetros
        /// </summary>
        protected readonly IBusiness<Parameter> _parameter;
        #endregion

        #region Constructors
        /// <summary>
        /// Inicializa la configuración y la capa de negocio asociada en el controlador
        /// </summary>
        /// <param name="configuration">Configuración de la aplicación</param>
        /// <param name="business">Capar de negocio asociada a la entidad</param>
        /// <param name="log">Administrador de logs en la base de datos</param>
        /// <param name="templateError">Administrador de notificaciones de error</param>
        /// <param name="parameter">Administrador de parámetros</param>
        protected ControllerBase(IConfiguration configuration, IBusiness<T> business, IPersistent<LogComponent> log, IBusiness<Template> templateError, IBusiness<Parameter> parameter)
        {
            _configuration = configuration;
            _log = log;
            _business = business;
            _templateError = templateError;
            _parameter = parameter;
        }
        #endregion

        #region Methods
        /// <summary>
        /// Trae un listado de entidades desde la base de datos
        /// </summary>
        /// <param name="filters">Filtros aplicados a la consulta</param>
        /// <param name="orders">Ordenamientos aplicados a la base de datos</param>
        /// <param name="limit">Límite de registros a traer</param>
        /// <param name="offset">Corrimiento desde el que se cuenta el número de registros</param>
        /// <returns>Listado de entidades</returns>
        [HttpGet]
        public ListResult<T> List(string? filters, string? orders, int limit, int offset)
        {
            try
            {
                ListResult<T> result = _business.List(filters ?? "", orders ?? "", limit, offset);
                LogInfo("filters: " + filters + ", orders: " + orders + ", limit: " + limit + ", offset: " + offset, JsonSerializer.Serialize(result));
                return result;
            }
            catch (PersistentException e)
            {
                LogError(e, "P", "filters: " + filters + ", orders: " + orders + ", limit: " + limit + ", offset: " + offset);
            }
            catch (BusinessException e)
            {
                LogError(e, "B", "filters: " + filters + ", orders: " + orders + ", limit: " + limit + ", offset: " + offset);
            }
            catch (Exception e)
            {
                LogError(e, "A", "filters: " + filters + ", orders: " + orders + ", limit: " + limit + ", offset: " + offset);
            }
            Response.StatusCode = 500;
            return new ListResult<T>(new List<T>(), 0);
        }

        /// <summary>
        /// Consulta una entidad dado su identificador
        /// </summary>
        /// <param name="entity">Entidad a consultar</param>
        /// <returns>Entidad con los datos cargados desde la base de datos o null si no lo pudo encontrar</returns>
        [HttpGet("{id:int}")]
        public T Read(int id)
        {
            try
            {
                T entity = _business.Read(GetNewObject(id));
                if (ObjectIsDefault(entity))
                {
                    Response.StatusCode = 404;
                }
                LogInfo("id: " + id, JsonSerializer.Serialize(entity));
                return entity;
            }
            catch (PersistentException e)
            {
                LogError(e, "P", "id: " + id);
            }
            catch (BusinessException e)
            {
                LogError(e, "B", "id: " + id);
            }
            catch (Exception e)
            {
                LogError(e, "A", "id: " + id);
            }
            Response.StatusCode = 500;
            return new();
        }

        /// <summary>
        /// Inserta una entidad en la base de datos
        /// </summary>
        /// <param name="entity">Entidad a insertar</param>
        /// <returns>Entidad insertada con el id generado por la base de datos</returns>
        [HttpPost]
        public T Insert(T entity)
        {
            try
            {
                entity = _business.Insert(entity, new() { Id = int.Parse(HttpContext.User.Claims.First(x => x.Type == "id").Value) });
                LogInfo(JsonSerializer.Serialize(entity), JsonSerializer.Serialize(entity));
                return entity;
            }
            catch (PersistentException e)
            {
                LogError(e, "P", JsonSerializer.Serialize(entity));
            }
            catch (BusinessException e)
            {
                LogError(e, "B", JsonSerializer.Serialize(entity));
            }
            catch (Exception e)
            {
                LogError(e, "A", JsonSerializer.Serialize(entity));
            }
            Response.StatusCode = 500;
            return new();
        }

        /// <summary>
        /// Actualiza una entidad en la base de datos
        /// </summary>
        /// <param name="entity">entidad a actualizar</param>
        /// <returns>Entidad actualizada</returns>
        [HttpPut]
        public T Update(T entity)
        {
            try
            {
                LogInfo(JsonSerializer.Serialize(entity), JsonSerializer.Serialize(entity));
                return _business.Update(entity, new() { Id = int.Parse(HttpContext.User.Claims.First(x => x.Type == "id").Value) });
            }
            catch (PersistentException e)
            {
                LogError(e, "P", JsonSerializer.Serialize(entity));
            }
            catch (BusinessException e)
            {
                LogError(e, "B", JsonSerializer.Serialize(entity));
            }
            catch (Exception e)
            {
                LogError(e, "A", JsonSerializer.Serialize(entity));
            }
            Response.StatusCode = 500;
            return new();
        }

        /// <summary>
        /// Elimina una entidad de la base de datos
        /// </summary>
        /// <param name="entity">Entidad a eliminar</param>
        /// <returns>Entidad eliminado</returns>
        [HttpDelete("{id:int}")]
        public T Delete(int id)
        {
            try
            {
                T entity = GetNewObject(id);
                LogInfo("id: " + id, JsonSerializer.Serialize(entity));
                return _business.Delete(entity, new() { Id = int.Parse(HttpContext.User.Claims.First(x => x.Type == "id").Value) });
            }
            catch (PersistentException e)
            {
                LogError(e, "P", "id: " + id);
            }
            catch (BusinessException e)
            {
                LogError(e, "B", "id: " + id);
            }
            catch (Exception e)
            {
                LogError(e, "A", "id: " + id);
            }
            Response.StatusCode = 500;
            return new();
        }

        /// <summary>
        /// Retorna un nuevo objeto de tipo T con su identificador
        /// </summary>
        /// <param name="id">Identificador del objeto de tipo T</param>
        /// <returns>Nuevo objeto de tipo T con su identificador</returns>
        public abstract T GetNewObject(int id);

        /// <summary>
        /// Determina si el objeto tiene valores por defecto
        /// </summary>
        /// <param name="obj">Objeto a validar</param>
        /// <returns>Si el objeto tiene valores por defecto</returns>
        public abstract bool ObjectIsDefault(T obj);

        /// <summary>
        /// Guarda un registro de auditoría de tipo Información de los llamados al componente
        /// </summary>
        /// <param name="input">Datos de entrada del método del controlador</param>
        /// <param name="output">Datos de salida del método del controlador</param>
        protected void LogInfo(string input, string output)
        {
            if (_configuration.GetValue("LogInfo", false))
            {
                ClaimsPrincipal user = HttpContext.User;
                int userid = 1;
                if (user != null)
                {
                    Claim? claim = user.Claims.FirstOrDefault(x => x.Type == "id");
                    if (claim != null)
                    {
                        userid = int.Parse(claim.Value);
                    }
                }
                _log.Insert(new()
                {
                    Type = "I",
                    Controller = ControllerContext.ActionDescriptor.ControllerName,
                    Method = ControllerContext.ActionDescriptor.ActionName,
                    Input = input,
                    Output = output,
                    User = userid
                });
            }
        }

        /// <summary>
        /// Guarda un registro de auditoría de tipo Error de los llamados al componente
        /// </summary>
        /// <param name="ex">Excepción a registrar</param>
        /// <param name="level">Nivel en el que se registró la excepción P - Persistencia, B - Negocio,  A - API</param>
        /// <param name="input">Datos de entrada del método del controlador</param>
        protected void LogError(Exception ex, string level, string input)
        {
            ClaimsPrincipal user = HttpContext.User;
            int userid = 1;
            if (user != null)
            {
                Claim? claim = user.Claims.FirstOrDefault(x => x.Type == "id");
                if (claim != null)
                {
                    userid = int.Parse(claim.Value);
                }
            }
            StringBuilder desc = new(level + ": ");
            Exception? aux = ex;
            while (aux != null)
            {
                desc.Append(aux.Message + " - ");
                aux = aux.InnerException;
            }
            desc.Append(" - " + (ex.StackTrace ?? "Error no identificado"));
            LogComponent l = _log.Insert(new()
            {
                Type = "E",
                Controller = ControllerContext.ActionDescriptor.ControllerName,
                Method = ControllerContext.ActionDescriptor.ActionName,
                Input = input,
                Output = desc.ToString(),
                User = userid
            });
            try
            {
                string SMTP_FROM = _parameter.List("name = 'SMTP_FROM'", "", 1, 0).List[0].Value;
                string SMTP_HOST = _parameter.List("name = 'SMTP_HOST'", "", 1, 0).List[0].Value;
                string SMTP_PASS = _parameter.List("name = 'SMTP_PASS'", "", 1, 0).List[0].Value;
                string SMTP_PORT = _parameter.List("name = 'SMTP_PORT'", "", 1, 0).List[0].Value;
                string SMTP_SSL = _parameter.List("name = 'SMTP_SSL'", "", 1, 0).List[0].Value;
                string SMTP_USERNAME = _parameter.List("name = 'SMTP_USERNAME'", "", 1, 0).List[0].Value;
                string NOTIFICATION_TO = _parameter.List("name = 'NOTIFICATION_TO'", "", 1, 0).List[0].Value;
                SmtpConfig smtpConfig = new()
                {
                    From = SMTP_FROM,
                    Host = SMTP_HOST,
                    Password = SMTP_PASS,
                    Port = int.Parse(SMTP_PORT),
                    Ssl = bool.Parse(SMTP_SSL ?? "false"),
                    Username = SMTP_USERNAME
                };
                Template template = _templateError.Read(new() { Id = 1 });
                template = BusinessTemplate.ReplacedVariables(template, new Dictionary<string, string>() { { "message", desc.ToString() }, { "id", l.Id.ToString() } });
                Notification notification = new()
                {
                    To = NOTIFICATION_TO,
                    Subject = "Error en la aplicación Golden Web",
                    User = userid,
                    Content = template.Content
                };
                BusinessNotification.SendNotification(notification, smtpConfig);
            }
            catch
            {
                //No hacer nada si no logra enviar notificación de error
            }
        }
        #endregion
    }
}
using Business;
using Business.Dto;
using Business.Exceptions;
using Business.Noti;
using Dal;
using Dal.Dto;
using Dal.Exceptions;
using Entities.Log;
using Entities.Noti;
using Microsoft.AspNetCore.Mvc;
using System.Data;
using System.Security.Claims;
using System.Text;

namespace Api.Controllers
{
    /// <summary>
    /// Clase base de los controladores del api
    /// </summary>
    public abstract class ControllerBase<T> : ControllerBase where T : Entities.EntityBase, new()
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
        protected readonly IPersistentBase<LogComponent> _log;

        /// <summary>
        /// Administrador de plantilla de errores
        /// </summary>
        protected readonly IBusiness<Template> _templateError;
        #endregion

        #region Constructors
        /// <summary>
        /// Inicializa la configuración y la capa de negocio asociada en el controlador
        /// </summary>
        /// <param name="configuration">Configuración de la aplicación</param>
        /// <param name="business">Capar de negocio asociada a la entidad</param>
        /// <param name="log">Administrador de logs en la base de datos</param>
        /// <param name="templateError">Administrador de notificaciones de error</param>
        protected ControllerBase(IConfiguration configuration, IBusiness<T> business, IPersistentBase<LogComponent> log, IBusiness<Template> templateError)
        {
            _configuration = configuration;
            _log = log;
            _business = business;
            _templateError = templateError;
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
                LogInfo("Get " + typeof(T).Name + " list");
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
                LogInfo("Get " + typeof(T).Name + " with id = " + id);
                T entity = _business.Read(new T() { Id = id });
                if (entity.Id != 0)
                {
                    return entity;
                }
                else
                {
                    LogInfo(typeof(T).Name + " with id = " + id + " not found");
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
                LogInfo("Inserted " + typeof(T).Name + " with id = " + entity.Id);
                return entity;
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
        /// Actualiza una entidad en la base de datos
        /// </summary>
        /// <param name="entity">entidad a actualizar</param>
        /// <returns>Entidad actualizada</returns>
        [HttpPut]
        public T Update(T entity)
        {
            try
            {
                LogInfo("Update " + typeof(T).Name + " with id = " + entity.Id);
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
        /// Elimina una entidad de la base de datos
        /// </summary>
        /// <param name="entity">Entidad a eliminar</param>
        /// <returns>Entidad eliminado</returns>
        [HttpDelete("{id:int}")]
        public T Delete(int id)
        {
            try
            {
                LogInfo("Delete " + typeof(T).Name + " with id = " + id);
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


        /// <summary>
        /// Guarda un registro de auditoría de tipo Información de los llamados al componente
        /// </summary>
        /// <param name="info">Información a registrar</param>
        protected void LogInfo(string info)
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
                    Component = ControllerContext.ActionDescriptor.ControllerName + " - " + ControllerContext.ActionDescriptor.ActionName,
                    Description = info,
                    User = userid
                }
                );
            }
        }

        /// <summary>
        /// Guarda un registro de auditoría de tipo Error de los llamados al componente
        /// </summary>
        /// <param name="ex">Excepción a registrar</param>
        /// <param name="level">Nivel en el que se registró la excepción P - Persistencia, B - Negocio,  A - API</param>
        protected void LogError(Exception ex, string level)
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
            desc.Append(" - " + ex.StackTrace ?? "Error no identificado");
            LogComponent l = _log.Insert(new()
            {
                Type = "E",
                Component = ControllerContext.ActionDescriptor.ControllerName + " - " + ControllerContext.ActionDescriptor.ActionName,
                Description = desc.ToString(),
                User = userid
            }
            );
            try
            {
                SmtpConfig smtpConfig = new()
                {
                    From = _configuration["Smtp:From"] ?? "",
                    Host = _configuration["Smtp:Host"] ?? "",
                    Password = _configuration["Smtp:Password"] ?? "",
                    Port = int.Parse(_configuration["Smtp:Port"] ?? "0"),
                    Ssl = bool.Parse(_configuration["Smtp:Ssl"] ?? "false"),
                    Username = _configuration["Smtp:Username"] ?? ""
                };
                Template template = _templateError.Read(new() { Id = 1 });
                template = BusinessTemplate.ReplacedVariables(template, new Dictionary<string, string>() { { "message", desc.ToString() }, { "id", l.Id.ToString() } });
                Notification notification = new()
                {
                    To = _configuration["Notification:To"] ?? "soporte.sistemas@inglesefe.com",
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
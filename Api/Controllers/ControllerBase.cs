using Business;
using Business.Dto;
using Business.Noti;
using Dal.Dto;
using Dal.Log;
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
    public abstract class ControllerBase<T> : ControllerBase where T : Entities.EntityBase
    {
        #region Attributes
        /// <summary>
        /// Configuración de la aplicación
        /// </summary>
        protected readonly IConfiguration _configuration;

        /// <summary>
        /// Capa de negocio asociada a la entidad
        /// </summary>
        protected readonly BusinessBase<T> _business;

        /// <summary>
        /// Conexión a la base de datos para gestionar los logs
        /// </summary>
        protected readonly IDbConnection _connection;
        #endregion

        #region Constructors
        /// <summary>
        /// Inicializa la configuración y la capa de negocio asociada en el controlador
        /// </summary>
        /// <param name="configuration">Configuración de la aplicación</param>
        /// <param name="connection">Conexión a la base de datos para gestionar los logs</param>
        /// <param name="business">Capar de negocio asociada a la entidad</param>
        protected ControllerBase(IConfiguration configuration, IDbConnection connection, Business.BusinessBase<T> business)
        {
            _configuration = configuration;
            _connection = connection;
            _business = business;
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
        public abstract ListResult<T> List(string? filters, string? orders, int limit, int offset);

        /// <summary>
        /// Consulta una entidad dado su identificador
        /// </summary>
        /// <param name="entity">Entidad a consultar</param>
        /// <returns>Entidad con los datos cargados desde la base de datos o null si no lo pudo encontrar</returns>
        public abstract T Read(int id);

        /// <summary>
        /// Inserta una entidad en la base de datos
        /// </summary>
        /// <param name="entity">Entidad a insertar</param>
        /// <returns>Entidad insertada con el id generado por la base de datos</returns>
        public abstract T Insert(T entity);

        /// <summary>
        /// Actualiza una entidad en la base de datos
        /// </summary>
        /// <param name="entity">entidad a actualizar</param>
        /// <returns>Entidad actualizada</returns>
        public abstract T Update(T entity);

        /// <summary>
        /// Elimina una entidad de la base de datos
        /// </summary>
        /// <param name="entity">Entidad a eliminar</param>
        /// <returns>Entidad eliminado</returns>
        public abstract T Delete(int id);

        /// <summary>
        /// Guarda un registro de auditoría de tipo Información de los llamados al componente
        /// </summary>
        /// <param name="info">Información a registrar</param>
        protected void LogInfo(string info)
        {
            PersistentLogComponent log = new(_connection);
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
                log.Insert(new()
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
            PersistentLogComponent log = new(_connection);
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
            LogComponent l = log.Insert(new()
            {
                Type = "E",
                Component = ControllerContext.ActionDescriptor.ControllerName + " - " + ControllerContext.ActionDescriptor.ActionName,
                Description = desc.ToString(),
                User = userid
            }
            );
            try
            {
                BusinessTemplate businessTemplate = new(_connection);
                SmtpConfig smtpConfig = new()
                {
                    From = _configuration["Smtp:From"] ?? "",
                    Host = _configuration["Smtp:Host"] ?? "",
                    Password = _configuration["Smtp:Password"] ?? "",
                    Port = int.Parse(_configuration["Smtp:Port"] ?? "0"),
                    Ssl = bool.Parse(_configuration["Smtp:Ssl"] ?? "false"),
                    Username = _configuration["Smtp:Username"] ?? ""
                };
                Template template = businessTemplate.Read(new() { Id = 1 });
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
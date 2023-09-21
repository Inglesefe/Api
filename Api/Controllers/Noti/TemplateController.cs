using Business;
using Dal;
using Entities.Config;
using Entities.Log;
using Entities.Noti;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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
        /// <param name="configuration">Configuración del api</param>
        /// <param name="business">Capa de negocio de plantillas</param>
        /// <param name="log">Administrador de logs en la base de datos</param>
        /// <param name="templateError">Administrador de notificaciones de error</param>
        /// <param name="parameter">Administrador de parámetros</param>
        public TemplateController(IConfiguration configuration, IBusiness<Template> business, IPersistent<LogComponent> log, IBusiness<Template> templateError, IBusiness<Parameter> parameter) : base(
                configuration,
                business,
                log,
                templateError,
                parameter)
        { }
        #endregion

        #region Methods
        /// <inheritdoc />
        protected override Template GetNewObject(int id)
        {
            return new Template() { Id = id };
        }

        /// <inheritdoc />
        protected override bool ObjectIsDefault(Template obj)
        {
            return obj.Id == 0;
        }
        #endregion
    }
}

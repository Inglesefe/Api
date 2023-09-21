using Business;
using Dal;
using Entities.Config;
using Entities.Log;
using Entities.Noti;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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
        /// <param name="configuration">Configuración del api</param>
        /// <param name="business">Capa de negocio de tipos de identificación</param>
        /// <param name="log">Administrador de logs en la base de datos</param>
        /// <param name="templateError">Administrador de notificaciones de error</param>
        /// <param name="parameter">Administrador de parámetros</param>
        public IdentificationTypeController(IConfiguration configuration, IBusiness<IdentificationType> business, IPersistent<LogComponent> log, IBusiness<Template> templateError, IBusiness<Parameter> parameter) : base(
                  configuration,
                  business,
                  log,
                  templateError,
                  parameter)
        { }
        #endregion

        #region Methods
        /// <inheritdoc />
        protected override IdentificationType GetNewObject(int id)
        {
            return new IdentificationType() { Id = id };
        }

        /// <inheritdoc />
        protected override bool ObjectIsDefault(IdentificationType obj)
        {
            return obj.Id == 0;
        }
        #endregion
    }
}

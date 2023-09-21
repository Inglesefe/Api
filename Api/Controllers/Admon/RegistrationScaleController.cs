using Business;
using Dal;
using Entities.Admon;
using Entities.Config;
using Entities.Log;
using Entities.Noti;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers.Admon
{
    /// <summary>
    /// Controlador con los métodos necesarios para administrar escalas asociadas a matrículas
    /// </summary>
    [ApiController]
    [Route("[controller]")]
    [Authorize(Policy = "db")]
    public class RegistrationScaleController : ControllerBase<RegistrationScale>
    {
        #region Constructors
        /// <summary>
        /// Inicializa la configuración del controlador
        /// </summary>
        /// <param name="configuration">Configuración del api</param>
        /// <param name="business">Capa de negocio de escalas asociadas a matrículas</param>
        /// <param name="log">Administrador de logs en la base de datos</param>
        /// <param name="templateError">Administrador de notificaciones de error</param>
        /// <param name="parameter">Administrador de parámetros</param>
        public RegistrationScaleController(IConfiguration configuration, IBusiness<RegistrationScale> business, IPersistent<LogComponent> log, IBusiness<Template> templateError, IBusiness<Parameter> parameter) : base(
                  configuration,
                  business,
                  log,
                  templateError,
                  parameter)
        { }
        #endregion

        #region Methods

        /// <inheritdoc />
        protected override RegistrationScale GetNewObject(int id)
        {
            return new RegistrationScale() { Id = id };
        }

        /// <inheritdoc />
        protected override bool ObjectIsDefault(RegistrationScale obj)
        {
            return obj.Id == 0;
        }
        #endregion
    }
}

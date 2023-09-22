using Business;
using Dal;
using Entities.Config;
using Entities.Cont;
using Entities.Log;
using Entities.Noti;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers.Cont
{
    /// <summary>
    /// Controlador con los métodos necesarios para administrar pagos
    /// </summary>
    [ApiController]
    [Route("[controller]")]
    [Authorize(Policy = "db")]
    public class PaymentController : ControllerBase<Payment>
    {
        #region Constructors
        /// <summary>
        /// Inicializa la configuración del controlador
        /// </summary>
        /// <param name="configuration">Configuración del api</param>
        /// <param name="business">Capa de negocio de tipos de consecutivo</param>
        /// <param name="log">Administrador de logs en la base de datos</param>
        /// <param name="templateError">Administrador de notificaciones de error</param>
        /// <param name="parameter">Administrador de parámetros</param>
        public PaymentController(IConfiguration configuration, IBusiness<Payment> business, IPersistent<LogComponent> log, IBusiness<Template> templateError, IBusiness<Parameter> parameter) : base(
                  configuration,
                  business,
                  log,
                  templateError,
                  parameter)
        { }
        #endregion

        #region Methods
        /// <inheritdoc />
        protected override Payment GetNewObject(int id)
        {
            return new Payment() { Id = id };
        }

        /// <inheritdoc />
        protected override bool ObjectIsDefault(Payment obj)
        {
            return obj.Id == 0;
        }
        #endregion
    }
}

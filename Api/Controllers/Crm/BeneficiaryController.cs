using Business;
using Dal;
using Entities.Config;
using Entities.Crm;
using Entities.Log;
using Entities.Noti;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers.Crm
{
    /// <summary>
    /// Controlador con los métodos necesarios para administrar beneficiarios
    /// </summary>
    [ApiController]
    [Route("[controller]")]
    [Authorize(Policy = "db")]
    public class BeneficiaryController : ControllerBase<Beneficiary>
    {
        #region Constructors
        /// <summary>
        /// Inicializa la configuración del controlador
        /// </summary>
        /// <param name="configuration">Configuración del api</param>
        /// <param name="business">Capa de negocio de beneficiarios</param>
        /// <param name="log">Administrador de logs en la base de datos</param>
        /// <param name="templateError">Administrador de notificaciones de error</param>
        /// <param name="parameter">Administrador de parámetros</param>
        public BeneficiaryController(IConfiguration configuration, IBusiness<Beneficiary> business, IPersistent<LogComponent> log, IBusiness<Template> templateError, IBusiness<Parameter> parameter) : base(
                  configuration,
                  business,
                  log,
                  templateError,
                  parameter)
        { }
        #endregion

        #region Methods
        /// <inheritdoc />
        protected override Beneficiary GetNewObject(int id)
        {
            return new Beneficiary() { Id = id };
        }

        /// <inheritdoc />
        protected override bool ObjectIsDefault(Beneficiary obj)
        {
            return obj.Id == 0;
        }
        #endregion
    }
}

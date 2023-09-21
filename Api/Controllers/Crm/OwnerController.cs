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
    /// Controlador con los métodos necesarios para administrar titulares
    /// </summary>
    [ApiController]
    [Route("[controller]")]
    [Authorize(Policy = "db")]
    public class OwnerController : ControllerBase<Owner>
    {
        #region Constructors
        /// <summary>
        /// Inicializa la configuración del controlador
        /// </summary>
        /// <param name="configuration">Configuración del api</param>
        /// <param name="business">Capa de negocio de titulares</param>
        /// <param name="log">Administrador de logs en la base de datos</param>
        /// <param name="templateError">Administrador de notificaciones de error</param>
        /// <param name="parameter">Administrador de parámetros</param>
        public OwnerController(IConfiguration configuration, IBusiness<Owner> business, IPersistent<LogComponent> log, IBusiness<Template> templateError, IBusiness<Parameter> parameter) : base(
                  configuration,
                  business,
                  log,
                  templateError,
                  parameter)
        { }
        #endregion

        #region Methods
        /// <inheritdoc />
        protected override Owner GetNewObject(int id)
        {
            return new Owner() { Id = id };
        }

        /// <inheritdoc />
        protected override bool ObjectIsDefault(Owner obj)
        {
            return obj.Id == 0;
        }
        #endregion
    }
}

using Business;
using Dal;
using Entities.Config;
using Entities.Log;
using Entities.Noti;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Data;

namespace Api.Controllers.Config
{
    /// <summary>
    /// Controlador con los métodos necesarios para administrar tipos de ingreso
    /// </summary>
    [ApiController]
    [Route("[controller]")]
    [Authorize(Policy = "db")]
    public class IncomeTypeController : ControllerBase<IncomeType>
    {
        #region Constructors
        /// <summary>
        /// Inicializa la configuración del controlador
        /// </summary>
        /// <param name="configuration">Configuración de el tipo de ingreso</param>
        /// <param name="business">Capa de negocio de tipos de ingreso</param>
        /// <param name="log">Administrador de logs en la base de datos</param>
        /// <param name="templateError">Administrador de notificaciones de error</param>
        /// <param name="connection">Conexión a la base de datos</param>
        public IncomeTypeController(IConfiguration configuration, IBusiness<IncomeType> business, IPersistentBase<LogComponent> log, IBusiness<Template> templateError, IDbConnection connection) : base(
                  configuration,
                  business,
                  log,
                  templateError,
                  connection)
        { }
        #endregion
    }
}

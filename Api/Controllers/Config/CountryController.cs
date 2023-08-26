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
    /// Controlador con los métodos necesarios para administrar paises
    /// </summary>
    [ApiController]
    [Route("[controller]")]
    [Authorize(Policy = "db")]
    public class CountryController : ControllerBase<Country>
    {
        #region Constructors
        /// <summary>
        /// Inicializa la configuración del controlador
        /// </summary>
        /// <param name="configuration">Configuración del país</param>
        /// <param name="business">Capa de negocio de paises</param>
        /// <param name="log">Administrador de logs en la base de datos</param>
        /// <param name="templateError">Administrador de notificaciones de error</param>
        /// <param name="connection">Conexión a la base de datos</param>
        public CountryController(IConfiguration configuration, IBusiness<Country> business, IPersistentBase<LogComponent> log, IBusiness<Template> templateError, IDbConnection connection) : base(
                  configuration,
                  business,
                  log,
                  templateError,
                  connection)
        { }
        #endregion
    }
}

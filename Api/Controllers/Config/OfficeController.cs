using Business;
using Business.Config;
using Business.Exceptions;
using Dal;
using Dal.Dto;
using Dal.Exceptions;
using Entities.Admon;
using Entities.Config;
using Entities.Log;
using Entities.Noti;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace Api.Controllers.Config
{
    /// <summary>
    /// Controlador con los métodos necesarios para administrar oficinas
    /// </summary>
    [ApiController]
    [Route("[controller]")]
    [Authorize(Policy = "db")]
    public class OfficeController : ControllerBase<Office>
    {
        #region Constructors
        /// <summary>
        /// Inicializa la configuración del controlador
        /// </summary>
        /// <param name="configuration">Configuración de la oficina</param>
        /// <param name="business">Capa de negocio de oficinas</param>
        /// <param name="log">Administrador de logs en la base de datos</param>
        /// <param name="templateError">Administrador de notificaciones de error</param>
        /// <param name="parameter">Administrador de parámetros</param>
        public OfficeController(IConfiguration configuration, IBusiness<Office> business, IPersistent<LogComponent> log, IBusiness<Template> templateError, IBusiness<Parameter> parameter) : base(
                  configuration,
                  business,
                  log,
                  templateError,
                  parameter)
        { }
        #endregion

        #region Methods
        /// <summary>
        /// Trae un listado de ejecutivos de cuenta asignados a una oficina desde la base de datos
        /// </summary>
        /// <param name="filters">Filtros aplicados a la consulta</param>
        /// <param name="orders">Ordenamientos aplicados a la base de datos</param>
        /// <param name="limit">Límite de registros a traer</param>
        /// <param name="offset">Corrimiento desde el que se cuenta el número de registros</param>
        /// <param name="office">Oficina a la que se le consultan los ejecutivos de cuenta asignados</param>
        /// <returns>Listado de ejecutivos de cuenta asignados a la oficina</returns>
        [HttpGet("{office:int}/accountexecutive")]
        public ListResult<AccountExecutive> ListAccountExecutives(string? filters, string? orders, int limit, int offset, int office)
        {
            try
            {
                ListResult<AccountExecutive> result = ((IBusinessOffice)_business).ListAccountExecutives(filters ?? "", orders ?? "", limit, offset, new() { Id = office });
                LogInfo("filters: " + filters + ", orders: " + orders + ", limit: " + limit + ", offset: " + offset + ", office: " + office, JsonSerializer.Serialize(result));
                return result;
            }
            catch (PersistentException e)
            {
                LogError(e, "P", "filters: " + filters + ", orders: " + orders + ", limit: " + limit + ", offset: " + offset + ", office: " + office);
            }
            catch (BusinessException e)
            {
                LogError(e, "B", "filters: " + filters + ", orders: " + orders + ", limit: " + limit + ", offset: " + offset + ", office: " + office);
            }
            catch (Exception e)
            {
                LogError(e, "A", "filters: " + filters + ", orders: " + orders + ", limit: " + limit + ", offset: " + offset + ", office: " + office);
            }
            Response.StatusCode = 500;
            return new(new List<AccountExecutive>(), 0);
        }

        /// <summary>
        /// Trae un listado de ejecutivos de cuenta no asignados a una oficina desde la base de datos
        /// </summary>
        /// <param name="filters">Filtros aplicados a la consulta</param>
        /// <param name="orders">Ordenamientos aplicados a la base de datos</param>
        /// <param name="limit">Límite de registros a traer</param>
        /// <param name="offset">Corrimiento desde el que se cuenta el número de registros</param>
        /// <param name="office">Oficina a la que se le consultan los ejecutivos de cuenta no asignados</param>
        /// <returns>Listado de ejecutivos de cuenta no asignados a la oficina</returns>
        [HttpGet("{office:int}/not-accountexecutive")]
        public ListResult<AccountExecutive> ListNotAccountExecutives(string? filters, string? orders, int limit, int offset, int office)
        {
            try
            {
                ListResult<AccountExecutive> result = ((IBusinessOffice)_business).ListNotAccountExecutives(filters ?? "", orders ?? "", limit, offset, new() { Id = office });
                LogInfo("filters: " + filters + ", orders: " + orders + ", limit: " + limit + ", offset: " + offset + ", office: " + office, JsonSerializer.Serialize(result));
                return result;
            }
            catch (PersistentException e)
            {
                LogError(e, "P", "filters: " + filters + ", orders: " + orders + ", limit: " + limit + ", offset: " + offset + ", office: " + office);
            }
            catch (BusinessException e)
            {
                LogError(e, "B", "filters: " + filters + ", orders: " + orders + ", limit: " + limit + ", offset: " + offset + ", office: " + office);
            }
            catch (Exception e)
            {
                LogError(e, "A", "filters: " + filters + ", orders: " + orders + ", limit: " + limit + ", offset: " + offset + ", office: " + office);
            }
            Response.StatusCode = 500;
            return new(new List<AccountExecutive>(), 0);
        }

        /// <summary>
        /// Asigna un ejecutivo de cuenta a una oficina en la base de datos
        /// </summary>
        /// <param name="executive">Ejecutivo de cuenta que se asigna a la oficina</param>
        /// <param name="office">Oficina a la que se le asigna el ejecutivo de cuenta</param>
        /// <returns>Ejecutivo de cuenta asignado</returns>
        [HttpPost("{office:int}/accountexecutive")]
        public AccountExecutive InsertAccountExecutive([FromBody] AccountExecutive executive, int office)
        {
            try
            {
                AccountExecutive result = ((IBusinessOffice)_business).InsertAccountExecutive(executive, new() { Id = office }, new() { Id = int.Parse(HttpContext.User.Claims.First(x => x.Type == "id").Value) });
                LogInfo("executive: " + JsonSerializer.Serialize(executive) + ", office: " + office, JsonSerializer.Serialize(result));
                return result;
            }
            catch (PersistentException e)
            {
                LogError(e, "P", "executive: " + JsonSerializer.Serialize(executive) + ", office: " + office);
            }
            catch (BusinessException e)
            {
                LogError(e, "B", "executive: " + JsonSerializer.Serialize(executive) + ", office: " + office);
            }
            catch (Exception e)
            {
                LogError(e, "A", "executive: " + JsonSerializer.Serialize(executive) + ", office: " + office);
            }
            Response.StatusCode = 500;
            return new();
        }

        /// <summary>
        /// Elimina un ejecutivo de cuenta de una oficina de la base de datos
        /// </summary>
        /// <param name="executive">Ejecutivo de cuenta a eliminarle a la oficina</param>
        /// <param name="office">Oficina a la que se le elimina el ejecutivo de cuenta</param>
        /// <returns>Ejecutivo de cuenta eliminado</returns>
        [HttpDelete("{office:int}/accountexecutive/{executive:int}")]
        public AccountExecutive DeleteAccountExecutive(int executive, int office)
        {
            try
            {
                AccountExecutive result = ((IBusinessOffice)_business).DeleteAccountExecutive(new() { Id = executive }, new() { Id = office }, new() { Id = int.Parse(HttpContext.User.Claims.First(x => x.Type == "id").Value) });
                LogInfo("executive: " + executive + ", office: " + office, JsonSerializer.Serialize(result));
                return result;
            }
            catch (PersistentException e)
            {
                LogError(e, "P", "executive: " + executive + ", office: " + office);
            }
            catch (BusinessException e)
            {
                LogError(e, "B", "executive: " + executive + ", office: " + office);
            }
            catch (Exception e)
            {
                LogError(e, "A", "executive: " + executive + ", office: " + office);
            }
            Response.StatusCode = 500;
            return new();
        }

        /// <inheritdoc />
        protected override Office GetNewObject(int id)
        {
            return new Office() { Id = id };
        }

        /// <inheritdoc />
        protected override bool ObjectIsDefault(Office obj)
        {
            return obj.Id == 0;
        }
        #endregion
    }
}

using Api.Dto;
using Business;
using Business.Auth;
using Business.Dto;
using Business.Exceptions;
using Business.Noti;
using Business.Util;
using Dal;
using Dal.Dto;
using Dal.Exceptions;
using Entities.Auth;
using Entities.Config;
using Entities.Log;
using Entities.Noti;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.Data;
using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.Json;

namespace Api.Controllers.Auth
{
    /// <summary>
    /// Controlador con los métodos necesarios para administrar usuarios
    /// </summary>
    [ApiController]
    [Route("[controller]")]
    [Authorize(Policy = "db")]
    public class UserController : ControllerBase<User>
    {
        #region Constructors
        /// <summary>
        /// Inicializa la configuración del controlador
        /// </summary>
        /// <param name="configuration">Configuración del api</param>
        /// <param name="business">Capa de negocio de usuarios</param>
        /// <param name="log">Administrador de logs en la base de datos</param>
        /// <param name="templateError">Administrador de plantilla de errores</param>
        /// <param name="parameter">Administrador de parámetros</param>
        public UserController(IConfiguration configuration, IBusinessUser business, IPersistent<LogComponent> log, IBusiness<Template> templateError, IBusiness<Parameter> parameter) : base(
            configuration,
            business,
            log,
            templateError,
            parameter)
        { }
        #endregion

        #region Methods
        /// <summary>
        /// Consulta un usuario dado su login y contraseña
        /// </summary>
        /// <param name="data">Usuario y contraseña del usaurio</param>
        /// <returns>Usuario con los datos cargados desde la base de datos</returns>
        [AllowAnonymous]
        [HttpPost("login")]
        public LoginResponse ReadByLoginAndPassword(LoginRequest data)
        {
            try
            {
                LoginResponse result = new();
                User user = ((IBusinessUser)_business).ReadByLoginAndPassword(new() { Login = data.Login }, data.Password, _configuration["Aes:Key"] ?? "", _configuration["Aes:IV"] ?? "");
                if (user.Id != 0)
                {
                    var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"] ?? ""));
                    var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

                    IList<Role> roles = ((IBusinessUser)_business).ListRoles("", "", 100, 0, user).List;

                    var claims = new[] {
                        new Claim("id", user.Id.ToString()),
                        new Claim("email", user.Login),
                        new Claim("name", user.Name),
                        new Claim("roles", string.Join(",", roles.Select(x => x.Id).ToList()))
                };

                    var token = new JwtSecurityToken(_configuration["Jwt:Issuer"],
                        _configuration["Jwt:Audience"],
                        claims,
                        expires: DateTime.Now.AddMinutes(120),
                        signingCredentials: credentials);

                    result = new() { Valid = true, Token = new JwtSecurityTokenHandler().WriteToken(token) };
                }
                LogInfo("data: " + JsonSerializer.Serialize(data), JsonSerializer.Serialize(result));
                return result;
            }
            catch (PersistentException e)
            {
                LogError(e, "P", "data: " + JsonSerializer.Serialize(data));
            }
            catch (BusinessException e)
            {
                LogError(e, "B", "data: " + JsonSerializer.Serialize(data));
            }
            catch (Exception e)
            {
                LogError(e, "A", "data: " + JsonSerializer.Serialize(data));
            }
            Response.StatusCode = 500;
            return new();
        }

        /// <summary>
        /// Consulta un usuario dado su login y contraseña
        /// </summary>
        /// <param name="data">Usuario y contraseña del usaurio</param>
        /// <returns>Usuario con los datos cargados desde la base de datos</returns>
        [AllowAnonymous]
        [HttpGet("recovery/{login}")]
        public LoginResponse ReadByLogin(string login)
        {
            try
            {
                LoginResponse result = new();
                User user = ((IBusinessUser)_business).ReadByLogin(new() { Login = login });
                if (user.Id != 0)
                {
                    string crypto = Crypto.Encrypt(user.Id + "~" + user.Login + "~" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), _configuration["Aes:Key"] ?? "", _configuration["Aes:IV"] ?? "");
                    //Enviar notificación
                    string SMTP_FROM = _parameter.List("name = 'SMTP_FROM'", "", 1, 0).List[0].Value;
                    string SMTP_HOST = _parameter.List("name = 'SMTP_HOST'", "", 1, 0).List[0].Value;
                    string SMTP_PASS = _parameter.List("name = 'SMTP_PASS'", "", 1, 0).List[0].Value;
                    string SMTP_PORT = _parameter.List("name = 'SMTP_PORT'", "", 1, 0).List[0].Value;
                    string SMTP_SSL = _parameter.List("name = 'SMTP_SSL'", "", 1, 0).List[0].Value;
                    string SMTP_STARTTLS = _parameter.List("name = 'SMTP_STARTTLS'", "", 1, 0).List[0].Value;
                    string SMTP_USERNAME = _parameter.List("name = 'SMTP_USERNAME'", "", 1, 0).List[0].Value;
                    string URL_CHANGE_PASS = _parameter.List("name = 'URL_CHANGE_PASS'", "", 1, 0).List[0].Value;
                    SmtpConfig smtpConfig = new()
                    {
                        From = SMTP_FROM,
                        Host = SMTP_HOST,
                        Password = SMTP_PASS,
                        Port = int.Parse(SMTP_PORT),
                        Ssl = bool.Parse(SMTP_SSL ?? "false"),
                        StartTls = bool.Parse(SMTP_STARTTLS ?? "false"),
                        Username = SMTP_USERNAME
                    };
                    Template template = _templateError.Read(new() { Id = 2 });
                    template = BusinessTemplate.ReplacedVariables(template, new Dictionary<string, string>() { { "link", URL_CHANGE_PASS + Uri.EscapeDataString(crypto) } });
                    Notification notification = new()
                    {
                        To = login,
                        Subject = "Cambio de contraseña - Golden Web",
                        User = 1,
                        Content = template.Content
                    };
                    BusinessNotification.SendNotification(notification, smtpConfig);
                    result = new() { Valid = true, Token = crypto };
                }
                LogInfo("login: " + login, JsonSerializer.Serialize(result));
                return result;
            }
            catch (PersistentException e)
            {
                LogError(e, "P", "login: " + login);
            }
            catch (BusinessException e)
            {
                LogError(e, "B", "login: " + login);
            }
            catch (Exception e)
            {
                LogError(e, "A", "login: " + login);
            }
            Response.StatusCode = 500;
            return new();
        }

        /// <summary>
        /// Actualiza la contraseña de un usuario en la base de datos
        /// </summary>
        /// <param name="data">Datos necesarios para el cambio de contraseña del usuario</param>
        /// <returns>Usuario actualizado</returns>
        [AllowAnonymous]
        [HttpPut("password")]
        public ChangePasswordResponse UpdatePassword([FromBody] ChangePasswordRequest data)
        {
            try
            {
                ChangePasswordResponse result = new();
                string plainToken = Crypto.Decrypt(data.Token, _configuration["Aes:Key"] ?? "", _configuration["Aes:IV"] ?? "");
                string[] parts = plainToken.Split("~");
                int id = int.Parse(parts[0]);
                string login = parts[1];
                DateTime date = DateTime.ParseExact(parts[2], "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);

                if (DateTime.Now.Subtract(date).TotalMinutes > 10)
                {
                    result = new() { Success = false, Message = "El enlace ha perdido validez" };
                }
                else
                {
                    User user = ((IBusinessUser)_business).ReadByLogin(new() { Login = login });
                    if (user.Id != 0 && user.Id == id)
                    {
                        _ = ((IBusinessUser)_business).UpdatePassword(user, data.Password, _configuration["Aes:Key"] ?? "", _configuration["Aes:IV"] ?? "", new() { Id = 1 });
                        //Enviar notificación
                        string SMTP_FROM = _parameter.List("name = 'SMTP_FROM'", "", 1, 0).List[0].Value;
                        string SMTP_HOST = _parameter.List("name = 'SMTP_HOST'", "", 1, 0).List[0].Value;
                        string SMTP_PASS = _parameter.List("name = 'SMTP_PASS'", "", 1, 0).List[0].Value;
                        string SMTP_PORT = _parameter.List("name = 'SMTP_PORT'", "", 1, 0).List[0].Value;
                        string SMTP_SSL = _parameter.List("name = 'SMTP_SSL'", "", 1, 0).List[0].Value;
                        string SMTP_STARTTLS = _parameter.List("name = 'SMTP_STARTTLS'", "", 1, 0).List[0].Value;
                        string SMTP_USERNAME = _parameter.List("name = 'SMTP_USERNAME'", "", 1, 0).List[0].Value;
                        SmtpConfig smtpConfig = new()
                        {
                            From = SMTP_FROM,
                            Host = SMTP_HOST,
                            Password = SMTP_PASS,
                            Port = int.Parse(SMTP_PORT),
                            Ssl = bool.Parse(SMTP_SSL ?? "false"),
                            StartTls = bool.Parse(SMTP_STARTTLS ?? "false"),
                            Username = SMTP_USERNAME
                        };
                        Template template = _templateError.Read(new() { Id = 3 });
                        template = BusinessTemplate.ReplacedVariables(template, new Dictionary<string, string>());
                        Notification notification = new()
                        {
                            To = login,
                            Subject = "Cambio de contraseña - Golden Web",
                            User = 1,
                            Content = template.Content
                        };
                        BusinessNotification.SendNotification(notification, smtpConfig);
                        result = new() { Success = true, Message = "Contraseña cambiada con éxito" };
                    }
                    else
                    {
                        result = new() { Success = false, Message = "Los datos no son válidos" };
                    }
                }
                LogInfo("data: " + JsonSerializer.Serialize(data), JsonSerializer.Serialize(result));
                return result;
            }
            catch (PersistentException e)
            {
                LogError(e, "P", "data: " + JsonSerializer.Serialize(data));
                Response.StatusCode = 500;
                return new() { Success = false, Message = e.Message };
            }
            catch (BusinessException e)
            {
                LogError(e, "B", "data: " + JsonSerializer.Serialize(data));
                Response.StatusCode = 500;
                return new() { Success = false, Message = e.Message };
            }
            catch (Exception e)
            {
                LogError(e, "A", "data: " + JsonSerializer.Serialize(data));
                Response.StatusCode = 500;
                return new() { Success = false, Message = e.Message };
            }
        }

        /// <summary>
        /// Trae un listado de roles asignados a un usuario desde la base de datos
        /// </summary>
        /// <param name="filters">Filtros aplicados a la consulta</param>
        /// <param name="orders">Ordenamientos aplicados a la base de datos</param>
        /// <param name="limit">Límite de registros a traer</param>
        /// <param name="offset">Corrimiento desde el que se cuenta el número de registros</param>
        /// <param name="user">Usuario al que se le consultan los roles asignados</param>
        /// <returns>Listado de roles asignados al usuario</returns>
        [HttpGet("{user:int}/role")]
        public ListResult<Role> ListRoles(string? filters, string? orders, int limit, int offset, int user)
        {
            try
            {
                ListResult<Role> result = ((IBusinessUser)_business).ListRoles(filters ?? "", orders ?? "", limit, offset, new() { Id = user });
                LogInfo("filters: " + filters + ", orders: " + orders + ", limit: " + limit + ", offset: " + offset + ", user: " + user, JsonSerializer.Serialize(result));
                return result;
            }
            catch (PersistentException e)
            {
                LogError(e, "P", "filters: " + filters + ", orders: " + orders + ", limit: " + limit + ", offset: " + offset + ", user: " + user);
            }
            catch (BusinessException e)
            {
                LogError(e, "B", "filters: " + filters + ", orders: " + orders + ", limit: " + limit + ", offset: " + offset + ", user: " + user);
            }
            catch (Exception e)
            {
                LogError(e, "A", "filters: " + filters + ", orders: " + orders + ", limit: " + limit + ", offset: " + offset + ", user: " + user);
            }
            Response.StatusCode = 500;
            return new ListResult<Role>(new List<Role>(), 0);
        }

        /// <summary>
        /// Trae un listado de roles no asignados a un usuario desde la base de datos
        /// </summary>
        /// <param name="filters">Filtros aplicados a la consulta</param>
        /// <param name="orders">Ordenamientos aplicados a la base de datos</param>
        /// <param name="limit">Límite de registros a traer</param>
        /// <param name="offset">Corrimiento desde el que se cuenta el número de registros</param>
        /// <param name="user">Usuario al que se le consultan los roles no asignados</param>
        /// <returns>Listado de roles no asignados al usuario</returns>
        [HttpGet("{user:int}/not-role")]
        public ListResult<Role> ListNotRoles(string? filters, string? orders, int limit, int offset, int user)
        {
            try
            {
                ListResult<Role> result = ((IBusinessUser)_business).ListNotRoles(filters ?? "", orders ?? "", limit, offset, new() { Id = user });
                LogInfo("filters: " + filters + ", orders: " + orders + ", limit: " + limit + ", offset: " + offset + ", user: " + user, JsonSerializer.Serialize(result));
                return result;
            }
            catch (PersistentException e)
            {
                LogError(e, "P", "filters: " + filters + ", orders: " + orders + ", limit: " + limit + ", offset: " + offset + ", user: " + user);
            }
            catch (BusinessException e)
            {
                LogError(e, "B", "filters: " + filters + ", orders: " + orders + ", limit: " + limit + ", offset: " + offset + ", user: " + user);
            }
            catch (Exception e)
            {
                LogError(e, "A", "filters: " + filters + ", orders: " + orders + ", limit: " + limit + ", offset: " + offset + ", user: " + user);
            }
            Response.StatusCode = 500;
            return new ListResult<Role>(new List<Role>(), 0);
        }

        /// <summary>
        /// Asigna un rol a un usuario en la base de datos
        /// </summary>
        /// <param name="role">Rol que se asigna al usuario</param>
        /// <param name="user">Usuario al que se le asigna el rol</param>
        /// <returns>Rol asignado</returns>
        [HttpPost("{user:int}/role")]
        public Role InsertRole([FromBody] Role role, int user)
        {
            try
            {
                Role result = ((IBusinessUser)_business).InsertRole(role, new() { Id = user }, new() { Id = int.Parse(HttpContext.User.Claims.First(x => x.Type == "id").Value) });
                LogInfo("role: " + JsonSerializer.Serialize(role) + ", user: " + user, JsonSerializer.Serialize(result));
                return result;
            }
            catch (PersistentException e)
            {
                LogError(e, "P", "role: " + JsonSerializer.Serialize(role) + ", user: " + user);
            }
            catch (BusinessException e)
            {
                LogError(e, "B", "role: " + JsonSerializer.Serialize(role) + ", user: " + user);
            }
            catch (Exception e)
            {
                LogError(e, "A", "role: " + JsonSerializer.Serialize(role) + ", user: " + user);
            }
            Response.StatusCode = 500;
            return new();
        }

        /// <summary>
        /// Elimina un rol de un usuario de la base de datos
        /// </summary>
        /// <param name="role">Identificador del rol a eliminarle al usuario</param>
        /// <param name="user">Identificador del usuario al que se le elimina el rol</param>
        /// <returns>Rol eliminado</returns>
        [HttpDelete("{user:int}/role/{role:int}/")]
        public Role DeleteRole(int role, int user)
        {
            try
            {
                Role result = ((IBusinessUser)_business).DeleteRole(new() { Id = role }, new() { Id = user }, new() { Id = int.Parse(HttpContext.User.Claims.First(x => x.Type == "id").Value) });
                LogInfo("role: " + role + ", user " + user, JsonSerializer.Serialize(result));
                return result;
            }
            catch (PersistentException e)
            {
                LogError(e, "P", "role: " + role + ", user " + user);
            }
            catch (BusinessException e)
            {
                LogError(e, "B", "role: " + role + ", user " + user);
            }
            catch (Exception e)
            {
                LogError(e, "A", "role: " + role + ", user " + user);
            }
            Response.StatusCode = 500;
            return new();
        }

        /// <inheritdoc />
        protected override User GetNewObject(int id)
        {
            return new User() { Id = id };
        }

        /// <inheritdoc />
        protected override bool ObjectIsDefault(User obj)
        {
            return obj.Id == 0;
        }
        #endregion
    }
}

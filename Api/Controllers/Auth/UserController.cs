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
        /// <param name="configuration">Configuración de la aplicación</param>
        /// <param name="business">Capa de negocio de usuarios</param>
        /// <param name="log">Administrador de logs en la base de datos</param>
        /// <param name="templateError">Administrador de plantilla de errores</param>
        /// <param name="connection">Conexión a la base de datos</param>
        public UserController(IConfiguration configuration, IBusinessUser business, IPersistentBase<LogComponent> log, IBusiness<Template> templateError, IDbConnection connection) : base(
            configuration,
            business,
            log,
            templateError,
            connection)
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
                LogInfo("Login for user " + data.Login);
                User user = ((IBusinessUser)_business).ReadByLoginAndPassword(new() { Login = data.Login }, data.Password, _configuration["Aes:Key"] ?? "", _configuration["Aes:IV"] ?? "", _connection);
                if (user.Id != 0)
                {
                    var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"] ?? ""));
                    var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

                    IList<Role> roles = ((IBusinessUser)_business).ListRoles("", "", 100, 0, user, _connection).List;

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

                    return new() { Valid = true, Token = new JwtSecurityTokenHandler().WriteToken(token) };
                }
                else
                {
                    return new();
                }
            }
            catch (PersistentException e)
            {
                LogError(e, "P");
            }
            catch (BusinessException e)
            {
                LogError(e, "B");
            }
            catch (Exception e)
            {
                LogError(e, "A");
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
                LogInfo("Recovery password for login " + login);
                User user = ((IBusinessUser)_business).ReadByLogin(new() { Login = login }, _connection);
                if (user.Id != 0)
                {
                    string crypto = Crypto.Encrypt(user.Id + "~" + user.Login + "~" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), _configuration["Aes:Key"] ?? "", _configuration["Aes:IV"] ?? "");
                    //Enviar notificación
                    try
                    {
                        SmtpConfig smtpConfig = new()
                        {
                            From = _configuration["Smtp:From"] ?? "",
                            Host = _configuration["Smtp:Host"] ?? "",
                            Password = _configuration["Smtp:Password"] ?? "",
                            Port = int.Parse(_configuration["Smtp:Port"] ?? "0"),
                            Ssl = bool.Parse(_configuration["Smtp:Ssl"] ?? "false"),
                            Username = _configuration["Smtp:Username"] ?? ""
                        };
                        Template template = _templateError.Read(new() { Id = 2 }, _connection);
                        template = BusinessTemplate.ReplacedVariables(template, new Dictionary<string, string>() { { "link", _configuration["UrlWeb"] + Uri.EscapeDataString(crypto) } });
                        Notification notification = new()
                        {
                            To = login,
                            Subject = "Cambio de contraseña - Golden Web",
                            User = 1,
                            Content = template.Content
                        };
                        BusinessNotification.SendNotification(notification, smtpConfig);
                    }
                    catch
                    {
                        //No hacer nada si no logra enviar notificación
                    }
                    return new() { Valid = true, Token = crypto };
                }
                else
                {
                    return new();
                }
            }
            catch (PersistentException e)
            {
                LogError(e, "P");
            }
            catch (BusinessException e)
            {
                LogError(e, "B");
            }
            catch (Exception e)
            {
                LogError(e, "A");
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
                string plainToken = Crypto.Decrypt(data.Token, _configuration["Aes:Key"] ?? "", _configuration["Aes:IV"] ?? "");
                string[] parts = plainToken.Split("~");
                int id = int.Parse(parts[0]);
                string login = parts[1];
                DateTime date = DateTime.ParseExact(parts[2], "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);

                if (DateTime.Now.Subtract(date).TotalMinutes > 30)
                {
                    return new() { Success = false, Message = "El enlace ha perdido validez" };
                }
                else
                {
                    User user = ((IBusinessUser)_business).ReadByLogin(new() { Login = login }, _connection);
                    if (user.Id != 0 && user.Id == id)
                    {
                        LogInfo("Update password of user " + user.Id);
                        _ = ((IBusinessUser)_business).UpdatePassword(user, data.Password, _configuration["Aes:Key"] ?? "", _configuration["Aes:IV"] ?? "", new() { Id = 1 }, _connection);
                        //Enviar notificación
                        try
                        {
                            SmtpConfig smtpConfig = new()
                            {
                                From = _configuration["Smtp:From"] ?? "",
                                Host = _configuration["Smtp:Host"] ?? "",
                                Password = _configuration["Smtp:Password"] ?? "",
                                Port = int.Parse(_configuration["Smtp:Port"] ?? "0"),
                                Ssl = bool.Parse(_configuration["Smtp:Ssl"] ?? "false"),
                                Username = _configuration["Smtp:Username"] ?? ""
                            };
                            Template template = _templateError.Read(new() { Id = 3 }, _connection);
                            template = BusinessTemplate.ReplacedVariables(template, new Dictionary<string, string>());
                            Notification notification = new()
                            {
                                To = login,
                                Subject = "Cambio de contraseña - Golden Web",
                                User = 1,
                                Content = template.Content
                            };
                            BusinessNotification.SendNotification(notification, smtpConfig);
                        }
                        catch
                        {
                            //No hacer nada si no logra enviar notificación
                        }
                        return new() { Success = true, Message = "Contraseña cambiada con éxito" };
                    }
                    else
                    {
                        return new() { Success = false, Message = "Los datos no son válidos" };
                    }
                }
            }
            catch (PersistentException e)
            {
                LogError(e, "P");
                Response.StatusCode = 500;
                return new() { Success = false, Message = e.Message };
            }
            catch (BusinessException e)
            {
                LogError(e, "B");
                Response.StatusCode = 500;
                return new() { Success = false, Message = e.Message };
            }
            catch (Exception e)
            {
                LogError(e, "A");
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
                LogInfo("List roles related to user " + user);
                return ((IBusinessUser)_business).ListRoles(filters ?? "", orders ?? "", limit, offset, new() { Id = user }, _connection);
            }
            catch (PersistentException e)
            {
                LogError(e, "P");
            }
            catch (BusinessException e)
            {
                LogError(e, "B");
            }
            catch (Exception e)
            {
                LogError(e, "A");
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
                LogInfo("List roles not related to user " + user);
                return ((IBusinessUser)_business).ListNotRoles(filters ?? "", orders ?? "", limit, offset, new() { Id = user }, _connection);
            }
            catch (PersistentException e)
            {
                LogError(e, "P");
            }
            catch (BusinessException e)
            {
                LogError(e, "B");
            }
            catch (Exception e)
            {
                LogError(e, "A");
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
                LogInfo("Insert role " + role.Id + " to user " + user);
                return ((IBusinessUser)_business).InsertRole(role, new() { Id = user }, new() { Id = int.Parse(HttpContext.User.Claims.First(x => x.Type == "id").Value) }, _connection);
            }
            catch (PersistentException e)
            {
                LogError(e, "P");
            }
            catch (BusinessException e)
            {
                LogError(e, "B");
            }
            catch (Exception e)
            {
                LogError(e, "A");
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
                LogInfo("Delete role " + role + " to user " + user);
                return ((IBusinessUser)_business).DeleteRole(new() { Id = role }, new() { Id = user }, new() { Id = int.Parse(HttpContext.User.Claims.First(x => x.Type == "id").Value) }, _connection);
            }
            catch (PersistentException e)
            {
                LogError(e, "P");
            }
            catch (BusinessException e)
            {
                LogError(e, "B");
            }
            catch (Exception e)
            {
                LogError(e, "A");
            }
            Response.StatusCode = 500;
            return new();
        }
        #endregion
    }
}

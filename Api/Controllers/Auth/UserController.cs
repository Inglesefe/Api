using Api.Dto;
using Business.Auth;
using Business.Dto;
using Business.Exceptions;
using Business.Noti;
using Dal.Dto;
using Dal.Exceptions;
using Entities.Auth;
using Entities.Noti;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using MySql.Data.MySqlClient;
using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
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
        public UserController(IConfiguration configuration) : base(
            configuration,
            new MySqlConnection(configuration.GetConnectionString("golden")),
            new BusinessUser(new MySqlConnection(configuration.GetConnectionString("golden"))))
        { }
        #endregion

        #region Methods
        /// <summary>
        /// Trae un listado de usuarios desde la base de datos
        /// </summary>
        /// <param name="filters">Filtros aplicados a la consulta</param>
        /// <param name="orders">Ordenamientos aplicados a la base de datos</param>
        /// <param name="limit">Límite de registros a traer</param>
        /// <param name="offset">Corrimiento desde el que se cuenta el número de registros</param>
        /// <returns>Listado de usuarios</returns>
        [HttpGet]
        public override ListResult<User> List(string? filters, string? orders, int limit, int offset)
        {
            try
            {
                LogInfo("Leer listado de usuarios");
                return _business.List(filters ?? "", orders ?? "", limit, offset);
            }
            catch (PersistentException e)
            {
                LogError(e, "P");
                Response.StatusCode = 500;
                return new ListResult<User>(new List<User>(), 0);
            }
            catch (BusinessException e)
            {
                LogError(e, "B");
                Response.StatusCode = 500;
                return new ListResult<User>(new List<User>(), 0);
            }
            catch (Exception e)
            {
                LogError(e, "A");
                Response.StatusCode = 500;
                return new ListResult<User>(new List<User>(), 0);
            }
        }

        /// <summary>
        /// Consulta un usuario dado su identificador
        /// </summary>
        /// <param name="id">Usuario a consultar</param>
        /// <returns>Usuario con los datos cargados desde la base de datos o null si no lo pudo encontrar</returns>
        [HttpGet("{id:int}")]
        public override User Read(int id)
        {
            try
            {
                LogInfo("Leer el usuario " + id);
                User user = _business.Read(new() { Id = id });
                if (user != null)
                {
                    return user;
                }
                else
                {
                    LogInfo("Usuario " + id + " no encontrado");
                    Response.StatusCode = 404;
                    return new User();
                }
            }
            catch (PersistentException e)
            {
                LogError(e, "P");
                Response.StatusCode = 500;
                return new User();
            }
            catch (BusinessException e)
            {
                LogError(e, "B");
                Response.StatusCode = 500;
                return new User();
            }
            catch (Exception e)
            {
                LogError(e, "A");
                Response.StatusCode = 500;
                return new User();
            }
        }

        /// <summary>
        /// Inserta un usuario en la base de datos
        /// </summary>
        /// <param name="entity">Usuario a insertar</param>
        /// <returns>Usuario insertado con el id generado por la base de datos</returns>
        [HttpPost]
        public override User Insert([FromBody] User entity)
        {
            try
            {
                LogInfo("Insertar el usuario " + entity.Id);
                return _business.Insert(entity, new() { Id = int.Parse(HttpContext.User.Claims.First(x => x.Type == "id").Value) });
            }
            catch (PersistentException e)
            {
                LogError(e, "P");
                Response.StatusCode = 500;
                return new User();
            }
            catch (BusinessException e)
            {
                LogError(e, "B");
                Response.StatusCode = 500;
                return new User();
            }
            catch (Exception e)
            {
                LogError(e, "A");
                Response.StatusCode = 500;
                return new User();
            }
        }

        /// <summary>
        /// Actualiza un usuario en la base de datos
        /// </summary>
        /// <param name="entity">Usuario a actualizar</param>
        /// <returns>Usuario actualizado</returns>
        [HttpPut]
        public override User Update([FromBody] User entity)
        {
            try
            {
                LogInfo("Actualizar el usuario " + entity.Id);
                return _business.Update(entity, new() { Id = int.Parse(HttpContext.User.Claims.First(x => x.Type == "id").Value) });
            }
            catch (PersistentException e)
            {
                LogError(e, "P");
                Response.StatusCode = 500;
                return new User();
            }
            catch (BusinessException e)
            {
                LogError(e, "B");
                Response.StatusCode = 500;
                return new User();
            }
            catch (Exception e)
            {
                LogError(e, "A");
                Response.StatusCode = 500;
                return new User();
            }
        }

        /// <summary>
        /// Elimina un usuario de la base de datos
        /// </summary>
        /// <param name="id">Usuario a eliminar</param>
        /// <returns>Usuario eliminado</returns>
        [HttpDelete("{id:int}")]
        public override User Delete(int id)
        {
            try
            {
                LogInfo("Actualizar el usuario " + id);
                return _business.Delete(new() { Id = id }, new() { Id = int.Parse(HttpContext.User.Claims.First(x => x.Type == "id").Value) });
            }
            catch (PersistentException e)
            {
                LogError(e, "P");
                Response.StatusCode = 500;
                return new User();
            }
            catch (BusinessException e)
            {
                LogError(e, "B");
                Response.StatusCode = 500;
                return new User();
            }
            catch (Exception e)
            {
                LogError(e, "A");
                Response.StatusCode = 500;
                return new User();
            }
        }

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
                LogInfo("Inicio de sesión para el login " + data.Login);
                User user = ((BusinessUser)_business).ReadByLoginAndPassword(new() { Login = data.Login }, data.Password, _configuration["Aes:Key"] ?? "", _configuration["Aes:IV"] ?? "");
                if (user != null)
                {
                    var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"] ?? ""));
                    var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

                    IList<Role> roles = ((BusinessUser)_business).ListRoles("", "", 100, 0, user).List;

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
                Response.StatusCode = 500;
                return new();
            }
            catch (BusinessException e)
            {
                LogError(e, "B");
                Response.StatusCode = 500;
                return new();
            }
            catch (Exception e)
            {
                LogError(e, "A");
                Response.StatusCode = 500;
                return new();
            }
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
                LogInfo("Recuperación de contraseña para el login " + login);
                User user = ((BusinessUser)_business).ReadByLogin(new() { Login = login });
                if (user != null)
                {
                    using Aes aes = Aes.Create();
                    aes.Key = Encoding.UTF8.GetBytes(_configuration["Aes:Key"] ?? "");
                    aes.IV = Encoding.UTF8.GetBytes(_configuration["Aes:IV"] ?? "");

                    ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
                    string param = user.Id + "~" + user.Login + "~" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                    byte[] paramBytes = Encoding.UTF8.GetBytes(param);
                    byte[] cryptoBytes = encryptor.TransformFinalBlock(paramBytes, 0, paramBytes.Length);
                    string crypto = Convert.ToBase64String(cryptoBytes);
                    //Enviar notificación
                    try
                    {
                        BusinessTemplate businessTemplate = new(_connection);
                        SmtpConfig smtpConfig = new()
                        {
                            From = _configuration["Smtp:From"] ?? "",
                            Host = _configuration["Smtp:Host"] ?? "",
                            Password = _configuration["Smtp:Password"] ?? "",
                            Port = int.Parse(_configuration["Smtp:Port"] ?? "0"),
                            Ssl = bool.Parse(_configuration["Smtp:Ssl"] ?? "false"),
                            Username = _configuration["Smtp:Username"] ?? ""
                        };
                        Template template = businessTemplate.Read(new() { Id = 2 });
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
                Response.StatusCode = 500;
                return new LoginResponse();
            }
            catch (BusinessException e)
            {
                LogError(e, "B");
                Response.StatusCode = 500;
                return new LoginResponse();
            }
            catch (Exception e)
            {
                LogError(e, "A");
                Response.StatusCode = 500;
                return new LoginResponse();
            }
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
                string plainToken = "";
                using (Aes aes = Aes.Create())
                {
                    aes.Key = Encoding.UTF8.GetBytes(_configuration["Aes:Key"] ?? "");
                    aes.IV = Encoding.UTF8.GetBytes(_configuration["Aes:IV"] ?? "");

                    ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
                    using MemoryStream msDecrypt = new(Convert.FromBase64String(data.Token));
                    using CryptoStream csDecrypt = new(msDecrypt, decryptor, CryptoStreamMode.Read);
                    using StreamReader srDecrypt = new(csDecrypt);
                    plainToken = srDecrypt.ReadToEnd();
                }

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
                    User user = ((BusinessUser)_business).ReadByLogin(new() { Login = login });
                    if (user.Id != 0 && user.Id == id)
                    {
                        LogInfo("Actualiza el password del usuario " + user.Id);
                        _ = ((BusinessUser)_business).UpdatePassword(user, data.Password, _configuration["Aes:Key"] ?? "", _configuration["Aes:IV"] ?? "", new() { Id = 1 });
                        //Enviar notificación
                        try
                        {
                            BusinessTemplate businessTemplate = new(_connection);
                            SmtpConfig smtpConfig = new()
                            {
                                From = _configuration["Smtp:From"] ?? "",
                                Host = _configuration["Smtp:Host"] ?? "",
                                Password = _configuration["Smtp:Password"] ?? "",
                                Port = int.Parse(_configuration["Smtp:Port"] ?? "0"),
                                Ssl = bool.Parse(_configuration["Smtp:Ssl"] ?? "false"),
                                Username = _configuration["Smtp:Username"] ?? ""
                            };
                            Template template = businessTemplate.Read(new() { Id = 3 });
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
                LogInfo("Listar los roles asignados al usuario " + user);
                return ((BusinessUser)_business).ListRoles(filters ?? "", orders ?? "", limit, offset, new() { Id = user });
            }
            catch (PersistentException e)
            {
                LogError(e, "P");
                Response.StatusCode = 500;
                return new ListResult<Role>(new List<Role>(), 0);
            }
            catch (BusinessException e)
            {
                LogError(e, "B");
                Response.StatusCode = 500;
                return new ListResult<Role>(new List<Role>(), 0);
            }
            catch (Exception e)
            {
                LogError(e, "A");
                Response.StatusCode = 500;
                return new ListResult<Role>(new List<Role>(), 0);
            }
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
                LogInfo("Listar los roles no asignados al usuario " + user);
                return ((BusinessUser)_business).ListNotRoles(filters ?? "", orders ?? "", limit, offset, new() { Id = user });
            }
            catch (PersistentException e)
            {
                LogError(e, "P");
                Response.StatusCode = 500;
                return new ListResult<Role>(new List<Role>(), 0);
            }
            catch (BusinessException e)
            {
                LogError(e, "B");
                Response.StatusCode = 500;
                return new ListResult<Role>(new List<Role>(), 0);
            }
            catch (Exception e)
            {
                LogError(e, "A");
                Response.StatusCode = 500;
                return new ListResult<Role>(new List<Role>(), 0);
            }
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
                LogInfo("Asigna el rol " + role.Id + " al usuario " + user);
                return ((BusinessUser)_business).InsertRole(role, new() { Id = user }, new() { Id = int.Parse(HttpContext.User.Claims.First(x => x.Type == "id").Value) });
            }
            catch (PersistentException e)
            {
                LogError(e, "P");
                Response.StatusCode = 500;
                return new Role();
            }
            catch (BusinessException e)
            {
                LogError(e, "B");
                Response.StatusCode = 500;
                return new Role();
            }
            catch (Exception e)
            {
                LogError(e, "A");
                Response.StatusCode = 500;
                return new Role();
            }
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
                LogInfo("Elimina el rol " + role + " del usuario " + user);
                return ((BusinessUser)_business).DeleteRole(new() { Id = role }, new() { Id = user }, new() { Id = int.Parse(HttpContext.User.Claims.First(x => x.Type == "id").Value) });
            }
            catch (PersistentException e)
            {
                LogError(e, "P");
                Response.StatusCode = 500;
                return new Role();
            }
            catch (BusinessException e)
            {
                LogError(e, "B");
                Response.StatusCode = 500;
                return new Role();
            }
            catch (Exception e)
            {
                LogError(e, "A");
                Response.StatusCode = 500;
                return new Role();
            }
        }
        #endregion
    }
}

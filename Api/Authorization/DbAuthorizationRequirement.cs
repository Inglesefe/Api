using Microsoft.AspNetCore.Authorization;

namespace Api.Authorization
{
    /// <summary>
    /// Requerimiento de autorización con el identificador de la aplicación que se quiere validar
    /// </summary>
    public class DbAuthorizationRequirement : IAuthorizationRequirement { }
}

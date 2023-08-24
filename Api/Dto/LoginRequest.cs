namespace Api.Dto
{
    /// <summary>
    /// Solicitud de una validación de logueo
    /// </summary>
    public class LoginRequest
    {
        #region Attributes
        /// <summary>
        /// Login del usuario
        /// </summary>
        public string Login { get; set; }

        /// <summary>
        /// Contraseña del usuario
        /// </summary>
        public string Password { get; set; }
        #endregion

        #region Constructors
        /// <summary>
        /// Inicializa con valores por defecto
        /// </summary>
        public LoginRequest()
        {
            Login = string.Empty;
            Password = string.Empty;
        }
        #endregion
    }
}

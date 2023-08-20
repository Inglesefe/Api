namespace Api.Dto
{
    /// <summary>
    /// Solicitud de un cambio de contraseña
    /// </summary>
    public class ChangePasswordRequest
    {
        #region Attributes
        /// <summary>
        /// Token de control
        /// </summary>
        public string Token { get; set; }

        /// <summary>
        /// Nueva contraseña del usuario
        /// </summary>
        public string Password { get; set; }
        #endregion

        #region Constructors
        /// <summary>
        /// Inicializa con valores por defecto
        /// </summary>
        public ChangePasswordRequest()
        {
            Token = string.Empty;
            Password = string.Empty;
        }
        #endregion
    }
}

namespace Api.Dto
{
    /// <summary>
    /// Respuesta de una validación de logueo
    /// </summary>
    public class LoginResponse
    {
        #region Attributes
        /// <summary>
        /// Si la autenticación fue válida o no
        /// </summary>
        public bool Valid { get; set; }

        /// <summary>
        /// Token JWT generado para el usuario
        /// </summary>
        public string Token { get; set; }
        #endregion

        #region Constructors
        /// <summary>
        /// Inicializa con valores por defecto
        /// </summary>
        public LoginResponse()
        {
            Valid = false;
            Token = string.Empty;
        }
        #endregion
    }
}

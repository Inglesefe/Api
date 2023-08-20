namespace Api.Dto
{
    /// <summary>
    /// Respuesta de un cambio de contraseña
    /// </summary>
    public class ChangePasswordResponse
    {
        #region Attributes
        /// <summary>
        /// Si la operación fue exitosa o no
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// Mensaje generado
        /// </summary>
        public string Message { get; set; }
        #endregion

        #region Constructors
        /// <summary>
        /// Inicializa con valores por defecto
        /// </summary>
        public ChangePasswordResponse()
        {
            Success = false;
            Message = string.Empty;
        }
        #endregion
    }
}

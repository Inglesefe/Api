using Api.Controllers.Noti;
using Business;
using Entities.Noti;
using Moq;

namespace Api.Test.Noti
{
    /// <summary>
    /// Realiza las pruebas sobre la api de notificaciones
    /// </summary>
    [Collection("Test")]
    public class NotificationTest : TestBase<Notification>
    {
        #region Constructors
        /// <summary>
        /// Inicializa la configuración de la prueba
        /// </summary>
        public NotificationTest() : base()
        {
            //Arrange
            Mock<IBusiness<Notification>> mockBusiness = new();

            List<Notification> notifications = new()
            {
                new Notification() { Id = 1, Date = DateTime.Now, To = "leandrobaena@gmail.com", Subject = "Correo de prueba", Content = "<h1>Esta es una prueba hecha por leandrobaena@gmail.com</h1>", User = 1 }
            };
            mockBusiness.Setup(p => p.Read(It.IsAny<Notification>()))
                .Returns((Notification notification) => notifications.Find(x => x.Id == notification.Id) ?? new Notification());
            api = new NotificationController(configuration, mockBusiness.Object, mockLog.Object, mockTemplate.Object, mockParameter.Object)
            {
                ControllerContext = controllerContext
            };
        }
        #endregion

        #region Methods
        /// <summary>
        /// Prueba la consulta de una notificación dada su identificador
        /// </summary>
        [Fact]
        public void ReadTest()
        {
            //Act
            Notification notification = api != null ? api.Read(1) : new();

            //Assert
            Assert.Equal("leandrobaena@gmail.com", notification.To);
        }
        #endregion
    }
}

using Api.Controllers.Noti;
using Business;
using Dal.Dto;
using Dal.Exceptions;
using Entities.Auth;
using Entities.Noti;
using Moq;
using System.Data;

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

            mockBusiness.Setup(p => p.List("idnotification = 1", It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>()))
                .Returns(new ListResult<Notification>(notifications.Where(y => y.Id == 1).ToList(), 1));
            mockBusiness.Setup(p => p.List("idnotificacion = 1", It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>()))
                .Throws<PersistentException>();
            mockBusiness.Setup(p => p.Read(It.IsAny<Notification>()))
                .Returns((Notification notification) => notifications.Find(x => x.Id == notification.Id) ?? new Notification());
            mockBusiness.Setup(p => p.Insert(It.IsAny<Notification>(), It.IsAny<User>()))
                .Returns((Notification notification, User user) =>
                {
                    notification.Id = notifications.Count + 1;
                    notifications.Add(notification);
                    return notification;
                });

            api = new NotificationController(configuration, mockBusiness.Object, mockLog.Object, mockTemplate.Object, mockParameter.Object)
            {
                ControllerContext = controllerContext
            };
        }
        #endregion

        #region Methods
        /// <summary>
        /// Prueba la consulta de un listado de notificaciones con filtros, ordenamientos y límite
        /// </summary>
        [Fact]
        public void ListTest()
        {
            //Act
            ListResult<Notification> list = api.List("idnotification = 1", "date", 1, 0);

            //Assert
            Assert.NotEmpty(list.List);
            Assert.True(list.Total > 0);
        }

        /// <summary>
        /// Prueba la consulta de un listado de notificaciones con filtros, ordenamientos y límite y con errores
        /// </summary>
        [Fact]
        public void ListWithErrorTest()
        {
            //Act
            ListResult<Notification> list = api.List("idnotificacion = 1", "name", 1, 0);

            //Assert
            Assert.Equal(0, list.Total);
        }

        /// <summary>
        /// Prueba la consulta de una notificación dada su identificador
        /// </summary>
        [Fact]
        public void ReadTest()
        {
            //Act
            Notification notification = api.Read(1);

            //Assert
            Assert.Equal("leandrobaena@gmail.com", notification.To);
        }

        /// <summary>
        /// Prueba la consulta de una notificación que no existe dado su identificador
        /// </summary>
        [Fact]
        public void ReadNotFoundTest()
        {
            //Act
            Notification notification = api.Read(10);

            //Assert
            Assert.Equal(0, notification.Id);
        }

        /// <summary>
        /// Prueba la inserción de una notificación
        /// </summary>
        [Fact]
        public void InsertTest()
        {
            //Arrange
            Notification notification = new() { Subject = "Prueba 1", Content = "<p>Prueba de notificación #{insertada}#", To = "leandrobaena@gmail.com", User = 1 };

            //Act
            notification = api.Insert(notification);

            //Assert
            Assert.NotEqual(0, notification.Id);
        }
        #endregion
    }
}

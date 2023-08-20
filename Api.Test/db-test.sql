/* Crear base de datos */
DROP DATABASE IF EXISTS test;

/* Crear base de datos */
CREATE DATABASE test;

/* Usar base de datos */
USE test;

/* Crear tabla de usuarios */
CREATE TABLE `user` (
  `iduser` int NOT NULL AUTO_INCREMENT,
  `login` varchar(100) NOT NULL,
  `name` varchar(100) NOT NULL,
  `password` varchar(128) NOT NULL,
  `active` tinyint NOT NULL,
  PRIMARY KEY (`iduser`),
  UNIQUE KEY `UK_user_login` (`login`)
);

/* Poblar tabla de usuarios */
INSERT INTO `user` (`login`, `name`, `password`, `active`) VALUES ('leandrobaena@gmail.com', 'Leandro Baena Torres', SHA2('Prueba123', 512), 1);
INSERT INTO `user` (`login`, `name`, `password`, `active`) VALUES ('actualizame@gmail.com', 'Karol Ximena Baena', SHA2('Prueba456', 512), 1);
INSERT INTO `user` (`login`, `name`, `password`, `active`) VALUES ('borrame@gmail.com', 'David Santiago Baena', SHA2('Prueba123', 512), 1);
INSERT INTO `user` (`login`, `name`, `password`, `active`) VALUES ('inactivo@gmail.com', 'Luz Marina Torres', SHA2('Prueba123', 512), 0);

/* Crear tabla de roles */
CREATE TABLE `role` (
  `idrole` INT NOT NULL AUTO_INCREMENT,
  `name` VARCHAR(45) NOT NULL,
  PRIMARY KEY (`idrole`),
  UNIQUE INDEX `UK_role_name` (`name` ASC)
);

/* Poblar tabla de roles */
INSERT INTO `role` (`name` ) VALUES ('Administradores');
INSERT INTO `role` (`name` ) VALUES ('Actualizame');
INSERT INTO `role` (`name` ) VALUES ('Borrame');
INSERT INTO `role` (`name` ) VALUES ('Para probar user_role y application_role');

/* Crear tabla de usuario por rol */
CREATE TABLE `user_role` (
  `iduserrole` int unsigned NOT NULL AUTO_INCREMENT,
  `iduser` int NOT NULL,
  `idrole` int NOT NULL,
  PRIMARY KEY (`iduserrole`),
  UNIQUE KEY `UK_user_role` (`iduser`,`idrole`),
  KEY `FK_user_role_user_idx` (`iduser`),
  KEY `FK_user_role_role_idx` (`idrole`),
  CONSTRAINT `FK_user_role_role` FOREIGN KEY (`idrole`) REFERENCES `role` (`idrole`) ON DELETE CASCADE ON UPDATE CASCADE,
  CONSTRAINT `FK_user_role_user` FOREIGN KEY (`iduser`) REFERENCES `user` (`iduser`) ON DELETE CASCADE ON UPDATE CASCADE
);

/* Poblar tabla de usuario por rol */
INSERT INTO `user_role` (`iduser`, `idrole`) VALUES (1, 1);
INSERT INTO `user_role` (`iduser`, `idrole`) VALUES (1, 2);
INSERT INTO `user_role` (`iduser`, `idrole`) VALUES (2, 1);
INSERT INTO `user_role` (`iduser`, `idrole`) VALUES (2, 2);

/* Crear tabla de aplicaciones */
CREATE TABLE `application` (
  `idapplication` INT NOT NULL AUTO_INCREMENT,
  `name` VARCHAR(45) NOT NULL,
  PRIMARY KEY (`idapplication`),
  UNIQUE INDEX `UK_application_name` (`name` ASC)
);

/* Poblar tabla de usuario por rol */
INSERT INTO `application` (`name`) VALUES ('Autenticacion');
INSERT INTO `application` (`name` ) VALUES ('Actualizame');
INSERT INTO `application` (`name` ) VALUES ('Borrame');

/* Crear tabla de aplicaciones por rol */
CREATE TABLE `application_role` (
  `idapplicationrole` INT NOT NULL AUTO_INCREMENT,
  `idapplication` INT NOT NULL,
  `idrole` INT NOT NULL,
  PRIMARY KEY (`idapplicationrole`),
  UNIQUE INDEX `UK_application_role` (`idapplication` ASC, `idrole` ASC),
  INDEX `FK_application_role_role_idx` (`idrole` ASC),
  CONSTRAINT `FK_application_role_application` FOREIGN KEY (`idapplication`) REFERENCES `application` (`idapplication`) ON DELETE CASCADE ON UPDATE CASCADE,
  CONSTRAINT `FK_application_role_role` FOREIGN KEY (`idrole`) REFERENCES `role` (`idrole`) ON DELETE CASCADE ON UPDATE CASCADE
);

/* Poblar tabla de aplicaciones por rol */
INSERT INTO `application_role` (`idapplication`, `idrole`) VALUES (1, 1);
INSERT INTO `application_role` (`idapplication`, `idrole`) VALUES (1, 2);
INSERT INTO `application_role` (`idapplication`, `idrole`) VALUES (2, 1);
INSERT INTO `application_role` (`idapplication`, `idrole`) VALUES (2, 2);

/* Crear tabla de registros de base de datos */
CREATE TABLE `log_db` (
  `idlog` BIGINT NOT NULL AUTO_INCREMENT,
  `date` DATETIME NOT NULL,
  `action` CHAR(1) NOT NULL,
  `idtable` BIGINT NOT NULL,
  `table` VARCHAR(200) NOT NULL,
  `sql` TEXT NOT NULL,
  `iduser` INT NOT NULL,
  PRIMARY KEY (`idlog`),
  INDEX `FK_log_user_idx` (`iduser` ASC),
  CONSTRAINT `FK_log_user` FOREIGN KEY (`iduser`) REFERENCES `user` (`iduser`) ON DELETE RESTRICT ON UPDATE RESTRICT
);

/* Poblar tabla de registros de base de datos */
INSERT INTO `log_db` (`date`, `action`, `idtable`, `table`, `sql`, `iduser`) VALUES (NOW(), 'I', 1, 'Tabla1', 'INSERT INTO Tabla1 (campo1) VALUES (''prueba'')', 1);

/* Crear tabla de registros de componentes */
CREATE TABLE `log_component` (
  `idlog` BIGINT NOT NULL AUTO_INCREMENT,
  `date` DATETIME NOT NULL,
  `type` CHAR(1) NOT NULL,
  `component` VARCHAR(100) NOT NULL,
  `description` TEXT NOT NULL,
  `iduser` INT NULL,
  PRIMARY KEY (`idlog`),
  INDEX `FK_log_component_user_idx` (`iduser` ASC),
  CONSTRAINT `FK_log_component_user` FOREIGN KEY (`iduser`) REFERENCES `user` (`iduser`) ON DELETE RESTRICT ON UPDATE RESTRICT
);

/* Poblar tabla de registros de componentes */
INSERT INTO `log_component` (`date`, `type`, `component`, `description`, `iduser`) VALUES (NOW(), 'E', 'Log.Test', 'Excepcion de prueba', 1);
INSERT INTO `log_component` (`date`, `type`, `component`, `description`, `iduser`) VALUES (NOW(), 'I', 'Log.Test', 'Infrmacion de prueba', NULL);

/* Crear tabla de plantillas */
CREATE TABLE `template` (
  `idtemplate` int NOT NULL AUTO_INCREMENT,
  `name` varchar(50) NOT NULL,
  `content` text NOT NULL,
  PRIMARY KEY (`idtemplate`)
);

/* Poblar tabla de plantillas */
INSERT INTO `template` (`name`, `content`) VALUES ('Plantilla de prueba', '<h1>Esta es una prueba hecha por #{user}#</h1>');
INSERT INTO `template` (`name`, `content`) VALUES ('Plantilla a actualizar', '<h1>Esta es una prueba hecha para #{actualizar}#</h1>');
INSERT INTO `template` (`name`, `content`) VALUES ('Plantilla a eliminar', '<h1>Esta es una prueba hecha para #{eliminar}#</h1>');

/* Crear tabla de notificaciones */
CREATE TABLE `notification` (
  `idnotification` bigint NOT NULL AUTO_INCREMENT,
  `date` datetime NOT NULL,
  `to` varchar(200) NOT NULL,
  `subject` varchar(200) NOT NULL,
  `content` text NOT NULL,
  `iduser` int NOT NULL,
  PRIMARY KEY (`idnotification`),
  KEY `FK_user_idx` (`iduser`),
  CONSTRAINT `FK_user` FOREIGN KEY (`iduser`) REFERENCES `user` (`iduser`) ON DELETE RESTRICT ON UPDATE RESTRICT
);

/* Poblar tabla de notificaciones */
INSERT INTO `notification` (`date`, `to`, `subject`, `content`, `iduser`) VALUES (NOW(), 'leandrobaena@gmail.com', 'Correo de prueba', '<h1>Esta es una prueba hecha por leandrobaena@gmail.com</h1>', 1);

/* Crear tabla de paises */
CREATE TABLE `country` (
  `idcountry` int NOT NULL AUTO_INCREMENT,
  `code` char(2) NOT NULL,
  `name` varchar(100) NOT NULL,
  PRIMARY KEY (`idcountry`),
  UNIQUE KEY `UK_country_code` (`code`)
);

/* Poblar tabla de pa�ses */
INSERT INTO `country` (`code`, `name`) VALUES ('CO', 'Colombia');
INSERT INTO `country` (`code`, `name`) VALUES ('US', 'Estados unidos');
INSERT INTO `country` (`code`, `name`) VALUES ('EN', 'Inglaterra');

/* Crear tabla de ciudades */
CREATE TABLE `city` (
  `idcity` INT NOT NULL AUTO_INCREMENT,
  `idcountry` INT NOT NULL,
  `code` CHAR(3) NOT NULL,
  `name` VARCHAR(100) NOT NULL,
  PRIMARY KEY (`idcity`),
  INDEX `FK_city_country_idx` (`idcountry` ASC),
  UNIQUE INDEX `UK_city` (`idcountry` ASC, `code` ASC),
  CONSTRAINT `FK_city_country` FOREIGN KEY (`idcountry`) REFERENCES `country` (`idcountry`) ON DELETE RESTRICT ON UPDATE RESTRICT
);

/* Poblar tabla de ciudades */
INSERT INTO `city` (`idcountry`, `code`, `name`) VALUES (1, 'BOG', 'Bogota');
INSERT INTO `city` (`idcountry`, `code`, `name`) VALUES (1, 'MED', 'Medellin');
INSERT INTO `city` (`idcountry`, `code`, `name`) VALUES (1, 'CAL', 'Cali');

/* Crear tabla de tipos de identificaci�n */
CREATE TABLE `identification_type` (
  `ididentificationtype` INT NOT NULL AUTO_INCREMENT,
  `name` VARCHAR(45) NOT NULL,
  PRIMARY KEY (`ididentificationtype`)
);

/* Poblar tabla de tipos de identificaci�n */
INSERT INTO `identification_type` (`name`) VALUES ('Cedula ciudadania');
INSERT INTO `identification_type` (`name`) VALUES ('Cedula extranjeria');
INSERT INTO `identification_type` (`name`) VALUES ('Pasaporte');

/* Crear tabla de tipos de ingreso */
CREATE TABLE `income_type` (
  `idincometype` int NOT NULL AUTO_INCREMENT,
  `code` char(2) NOT NULL,
  `name` varchar(45) NOT NULL,
  PRIMARY KEY (`idincometype`),
  UNIQUE KEY `UK_income_type` (`code`)
);

/* Poblar tabla de tipos de ingreso */
INSERT INTO `income_type` (`code`, `name`) VALUES ('CI', 'Cuota inicial');
INSERT INTO `income_type` (`code`, `name`) VALUES ('CR', 'Credito cartera');
INSERT INTO `income_type` (`code`, `name`) VALUES ('FC', 'Factura');

/* Crear tabla de oficinas */
CREATE TABLE `office` (
  `idoffice` int NOT NULL AUTO_INCREMENT,
  `idcity` int NOT NULL,
  `name` varchar(100) NOT NULL,
  `address` varchar(200) NOT NULL,
  PRIMARY KEY (`idoffice`),
  KEY `FK_office_city_idx` (`idcity`),
  CONSTRAINT `FK_office_city` FOREIGN KEY (`idcity`) REFERENCES `city` (`idcity`) ON DELETE RESTRICT ON UPDATE RESTRICT
);

/* Poblar tabla de oficinas */
INSERT INTO `office` (`idcity`, `name`, `address`) VALUES (1, 'Castellana', 'Cl 95');
INSERT INTO `office` (`idcity`, `name`, `address`) VALUES (1, 'Kennedy', 'Cl 56 sur');
INSERT INTO `office` (`idcity`, `name`, `address`) VALUES (1, 'Venecia', 'Puente');

/* Crear tabla de planes */
CREATE TABLE `plan` (
  `idplan` INT NOT NULL AUTO_INCREMENT,
  `value` FLOAT NOT NULL,
  `initial_fee` FLOAT NOT NULL,
  `installments_number` INT NOT NULL,
  `installment_value` FLOAT NOT NULL,
  `active` TINYINT NOT NULL,
  `description` VARCHAR(500) NOT NULL,
  PRIMARY KEY (`idplan`)
);

/* Poblar tabla de oficinas */
INSERT INTO `plan` (`value`, `initial_fee`, `installments_number`, `installment_value`, `active`, `description`) VALUES (3779100, 444600, 12, 444600, 1, 'PLAN COFREM 12 MESES');
INSERT INTO `plan` (`value`, `initial_fee`, `installments_number`, `installment_value`, `active`, `description`) VALUES (3779100, 282600, 15, 233100, 1, 'PLAN COFREM 15 MESES');
INSERT INTO `plan` (`value`, `initial_fee`, `installments_number`, `installment_value`, `active`, `description`) VALUES (3779100, 235350, 15, 236250, 1, 'PLAN COFREM 15 MESES ESPECIAL');

/* Crear tabla de par�metros */
CREATE TABLE `parameter` (
  `idparameter` int NOT NULL AUTO_INCREMENT,
  `name` varchar(100) NOT NULL,
  `value` varchar(200) NOT NULL,
  PRIMARY KEY (`idparameter`),
  UNIQUE KEY `UK_parameter_name` (`name`)
);

/* Poblar tabla de par�metros */
INSERT INTO `parameter` (`name`, `value`) VALUES ('Parametro 1', 'Valor 1');
INSERT INTO `parameter` (`name`, `value`) VALUES ('Parametro 2', 'Valor 2');
INSERT INTO `parameter` (`name`, `value`) VALUES ('Parametro 3', 'Valor 3');
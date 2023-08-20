# Api

- dev: [![dev](https://github.com/Inglesefe/Api/actions/workflows/build.yml/badge.svg?branch=dev)](https://github.com/Inglesefe/Api/actions/workflows/build.yml)  
- test: [![test](https://github.com/Inglesefe/Api/actions/workflows/build.yml/badge.svg?branch=test)](https://github.com/Inglesefe/Api/actions/workflows/build.yml)  
- main: [![main](https://github.com/Inglesefe/Api/actions/workflows/build.yml/badge.svg?branch=main)](https://github.com/Inglesefe/Api/actions/workflows/build.yml)

Servicios API REST del sistema

## Guía de inicio

Estas instrucciones le darán una copia del proyecto funcionando en su máquina local con fines de desarrollo y prueba.
Consulte implementación para obtener notas sobre la implementación del proyecto en un sistema en vivo.

### Prerequisitos

Este proyecto está desarrollado en .net core 7, el paquete propio de Business y el conector a Mysql  
[<img src="https://adrianwilczynski.gallerycdn.vsassets.io/extensions/adrianwilczynski/asp-net-core-switcher/2.0.2/1577043327534/Microsoft.VisualStudio.Services.Icons.Default" width="50px" height="50px" />](https://dotnet.microsoft.com/en-us/download/dotnet/7.0)  
[Business](https://github.com/Inglesefe/Business/pkgs/nuget/Business)  

## Pruebas

Para ejecutar las pruebas unitarias, es necesario tener instalado MySQL en el ambiente y ejecutar el script db-test.sql que se encuentra en el proyecto de pruebas.
La conexión se realiza con los datos del archivo appsettings.json del proyecto de pruebas o de variables de entorno del equipo con esos mismo nombres.

## Despliegue

El proyecto se despliega como un contenedor en linux en el [repositorio](https://github.com/Inglesefe/Api/pkgs/container/api)
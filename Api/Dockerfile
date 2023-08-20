FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
ARG CONFIG=Release
WORKDIR /app
COPY ./bin/$CONFIG/net7.0/ .
EXPOSE 80
EXPOSE 443
ENTRYPOINT ["dotnet", "Api.dll"]
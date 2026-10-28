FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY src/NovaPen.Server/NovaPen.Server.csproj src/NovaPen.Server/
COPY src/NovaPen.Shared/ src/NovaPen.Shared/
RUN dotnet restore src/NovaPen.Server/NovaPen.Server.csproj
COPY src/NovaPen.Server/ src/NovaPen.Server/
RUN dotnet publish src/NovaPen.Server/NovaPen.Server.csproj -c Release --no-restore -o /app

FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /app .
ENV ASPNETCORE_HTTP_PORTS=8080
USER $APP_UID
EXPOSE 8080
ENTRYPOINT ["dotnet", "NovaPen.Server.dll"]

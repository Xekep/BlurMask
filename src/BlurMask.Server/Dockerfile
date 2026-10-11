FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY src/BlurMask.Server/BlurMask.Server.csproj src/BlurMask.Server/
COPY src/BlurMask.Shared/ src/BlurMask.Shared/
RUN dotnet restore src/BlurMask.Server/BlurMask.Server.csproj
COPY src/BlurMask.Server/ src/BlurMask.Server/
RUN dotnet publish src/BlurMask.Server/BlurMask.Server.csproj -c Release --no-restore -o /app

FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /app .
ENV ASPNETCORE_HTTP_PORTS=8080
USER $APP_UID
EXPOSE 8080
ENTRYPOINT ["dotnet", "BlurMask.Server.dll"]

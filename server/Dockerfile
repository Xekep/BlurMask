FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY server/BlurMask.Server.csproj server/
COPY Shared/ Shared/
RUN dotnet restore server/BlurMask.Server.csproj
COPY server/ server/
RUN dotnet publish server/BlurMask.Server.csproj -c Release --no-restore -o /app

FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /app .
ENV ASPNETCORE_HTTP_PORTS=8080
USER $APP_UID
EXPOSE 8080
ENTRYPOINT ["dotnet", "BlurMask.Server.dll"]

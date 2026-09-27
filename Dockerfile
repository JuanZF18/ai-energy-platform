FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY nuget.config ./
COPY backend/Directory.Build.props backend/
COPY backend/src/ backend/src/
RUN dotnet publish backend/src/EnergyPlatform.Api/EnergyPlatform.Api.csproj -c Release -o /app/publish --configfile nuget.config

FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /app/publish ./
COPY data ./data
ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080
USER $APP_UID
ENTRYPOINT ["dotnet", "EnergyPlatform.Api.dll"]

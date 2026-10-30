FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY global.json Directory.Build.props ./
COPY src/ src/
RUN dotnet restore src/Warehouse.Api/Warehouse.Api.csproj
RUN dotnet publish src/Warehouse.Api/Warehouse.Api.csproj --configuration Release --output /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
USER root
RUN apt-get update \
    && apt-get install -y --no-install-recommends curl \
    && rm -rf /var/lib/apt/lists/*
COPY --from=build /app/publish .
USER $APP_UID
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
HEALTHCHECK --interval=10s --timeout=5s --start-period=40s --retries=12 \
  CMD curl -fsS http://127.0.0.1:8080/health/live || exit 1
ENTRYPOINT ["dotnet", "Warehouse.Api.dll"]

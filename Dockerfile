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
    && apt-get install -y --no-install-recommends curl postgresql-client \
    && rm -rf /var/lib/apt/lists/*
COPY --from=build /app/publish .
# Optional local seed dump restored on first start when the DB is empty.
COPY deploy/seed/ /app/seed/
RUN chown -R $APP_UID:$APP_UID /app/seed
USER $APP_UID
ENV ASPNETCORE_URLS=http://+:8080
ENV WAREHOUSE_RESTORE_DUMP_PATH=/app/seed/warehouse.sql
ENV WAREHOUSE_MIGRATE_ON_START=true
EXPOSE 8080
HEALTHCHECK --interval=10s --timeout=5s --start-period=40s --retries=12 \
  CMD curl -fsS http://127.0.0.1:8080/health/live || exit 1
ENTRYPOINT ["dotnet", "Warehouse.Api.dll"]

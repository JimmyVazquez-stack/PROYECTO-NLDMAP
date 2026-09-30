FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build

WORKDIR /src

COPY . .

RUN dotnet publish backend/NLDMAP.API/NLDMAP.API.csproj \
    --configuration Release \
    --output /out \
    -p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final

WORKDIR /app

ENV ASPNETCORE_ENVIRONMENT=Production 
ENV ASPNETCORE_HTTP_PORTS=8080

EXPOSE 8080

COPY --from=build /out .

RUN mkdir -p /var/lib/nldmap/keys \
    && chown app:app /var/lib/nldmap/keys \
    && chmod 700 /var/lib/nldmap/keys

USER $APP_UID

ENTRYPOINT ["dotnet", "NLDMAP.API.dll"]
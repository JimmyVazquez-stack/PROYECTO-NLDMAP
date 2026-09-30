FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build

WORKDIR /src

COPY . .

RUN dotnet publish frontend/NLDMAP.Web/NLDMAP.Web.csproj \
    --configuration Release \
    --output /out

FROM caddy:2-alpine AS final

COPY --from=build /out/wwwroot /srv
COPY docker/web/Caddyfile /etc/caddy/Caddyfile

EXPOSE 80 443



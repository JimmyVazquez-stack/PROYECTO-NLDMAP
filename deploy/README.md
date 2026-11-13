# Desplegar NLDMAP Server en una VM Linux de Azure

Esta versión usa Blazor Interactive Server y API en `NLDMAP.Server`, Caddy,
PostgreSQL 17 y MongoDB 8. Neo4j queda para una etapa posterior.

## Antes de comenzar

- VM Linux x86_64 con Docker Engine y el plugin Docker Compose v2 o posterior.
- Dominio con registro A apuntando a la IP pública de la VM (y AAAA solo si IPv6 funciona).
- Azure NSG: TCP 80/443 públicos; SSH 22 restringido a la IP del administrador.
- No abrir 5432 ni 27017. Solo Caddy publica puertos.
- Verificar que la subred Docker 172.29.240.0/24 no se superpone con las redes de la VM.
- Tener un workflow verde del commit que se desplegará y su imagen publicada en GHCR.

## Importante: base nueva

La arquitectura antigua y esta versión tienen historiales de migraciones distintos.
Este Compose utiliza el proyecto `nldmap-v2` y volúmenes nuevos. No conectar los
volúmenes o la base anterior: conservarlos y respaldarlos. Cualquier migración de
cuentas/datos anteriores debe planearse aparte. Los catálogos comienzan vacíos.

## Preparar archivos

Descargar el artifact `nldmap-deploy-<SHA>` del workflow y extraer el tar.gz en
`/opt/nldmap-v2`. Alternativamente obtener estos archivos del mismo commit en Git.
No copiar credenciales al repositorio ni incluirlas en la imagen.

```bash
cd /opt/nldmap-v2
cp deploy/.env.production.example .env
chmod 600 .env
```

Editar `.env`: dominio, SHA completo en IMAGE_TAG y cuatro contraseñas distintas,
generadas con `openssl rand -hex 32`. Mantener los nombres de usuario de Compose.
Si el paquete GHCR es privado, autenticarse con `docker login ghcr.io` usando
una credencial con acceso de lectura al paquete (entrada por stdin).

```bash
docker compose --env-file .env -f compose.production.yml config --quiet
docker compose --env-file .env -f compose.production.yml pull
docker compose --env-file .env -f compose.production.yml up -d --wait postgres mongodb
```

Los scripts de usuarios se ejecutan solamente con volúmenes vacíos. Cambiar una
contraseña en `.env` no modifica automáticamente una cuenta ya creada en la base.

## Inicializar esquema, roles y administrador

El comando termina sin abrir un servidor web. El contenedor habitual no recibe
la contraseña del administrador ni habilita la inicialización.

```bash
read -r -p 'Correo del administrador: ' BootstrapAdmin__Email
read -r -s -p 'Contraseña (12+ caracteres, mayúscula, minúscula, número y símbolo): ' BootstrapAdmin__Password
printf '\n'
export BootstrapAdmin__Email BootstrapAdmin__Password
docker compose --env-file .env -f compose.production.yml run --rm \
  -e SeedIdentity=true -e BootstrapAdmin__Enabled=true \
  -e BootstrapAdmin__Email -e BootstrapAdmin__Password init
unset BootstrapAdmin__Email BootstrapAdmin__Password
```

Si falla, corregir el error antes de arrancar la aplicación. Repetir la inicialización
es seguro: una cuenta administrativa existente conserva su contraseña.

## Arrancar y verificar

Si Caddy anterior ocupa 80/443, detener únicamente ese servicio al hacer el cambio.
Conservar su configuración y sus volúmenes para poder revertir.

```bash
docker compose --env-file .env -f compose.production.yml up -d --wait server caddy
docker compose --env-file .env -f compose.production.yml ps
docker compose --env-file .env -f compose.production.yml logs --tail=100 server caddy
curl -f https://TU_DOMINIO/health/live
```

Abrir `https://TU_DOMINIO/Account/Login`, iniciar sesión y comprobar `/admin` y
los catálogos de tez/región corporal. Caddy transmite también las conexiones
WebSocket de Blazor. `/health/live` comprueba el proceso, no la disponibilidad de las bases.

SMTP es opcional para arrancar y entrar al panel, pero es necesario configurar
EMAIL_HOST, EMAIL_PORT, EMAIL_FROM, EMAIL_USERNAME y EMAIL_PASSWORD para enviar
invitaciones o recuperar contraseñas. El remitente debe estar autorizado por el proveedor.

## Actualizar

Respaldar primero las bases y las claves de protección de datos. Cambiar IMAGE_TAG
al SHA nuevo, descargar la imagen, aplicar migraciones con el comando `init` sin
SeedIdentity y recrear el servidor:

```bash
docker compose --env-file .env -f compose.production.yml pull server init
docker compose --env-file .env -f compose.production.yml run --rm init
docker compose --env-file .env -f compose.production.yml up -d --wait server caddy
```

Un rollback de imagen requiere que el esquema siga siendo compatible. Conservar
el respaldo anterior y no deshacer migraciones a ciegas. Nunca usar `down -v` en
producción: elimina los datos. Los volúmenes persistentes no son respaldos.

Las cuentas de aplicación no son root/superusuario. En esta etapa PostgreSQL usa
la misma cuenta propietaria para migraciones y ejecución; MongoDB usa readWrite
solo en `nldmap`. La separación fina de permisos queda para la siguiente fase.

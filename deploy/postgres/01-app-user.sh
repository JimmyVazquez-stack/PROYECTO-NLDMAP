#!/bin/sh
(
set -eu
# Cuenta de aplicación sin SUPERUSER ni CREATEROLE. Es propietaria de su DB
# en esta primera etapa para ejecutar las migraciones; se separará después.
psql -v ON_ERROR_STOP=1 --username "$POSTGRES_USER" --dbname postgres \
    --set=app_user="$POSTGRES_APP_USER" --set=app_password="$POSTGRES_APP_PASSWORD" --set=app_db="$POSTGRES_APP_DB" <<'SQL'
CREATE ROLE :"app_user" LOGIN PASSWORD :'app_password' NOSUPERUSER NOCREATEDB NOCREATEROLE;
CREATE DATABASE :"app_db" OWNER :"app_user";
SQL
)

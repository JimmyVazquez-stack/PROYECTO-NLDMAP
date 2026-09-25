#!/usr/bin/env bash
set -Eeuo pipefail

: "${NLDMAP_DEV_USER:?Falta NLDMAP_DEV_USER}"
: "${NLDMAP_DEV_PASSWORD:?Falta NLDMAP_DEV_PASSWORD}"

psql \
  --username "$POSTGRES_USER" \
  --dbname "$POSTGRES_DB" \
  --set=dev_user="$NLDMAP_DEV_USER" \
  --set=dev_password="$NLDMAP_DEV_PASSWORD" \
  --set=db_name="$POSTGRES_DB" \
  --set=ON_ERROR_STOP=1 <<'SQL'
CREATE ROLE nldmap_developers NOLOGIN;

CREATE ROLE :"dev_user"
    LOGIN PASSWORD :'dev_password'
    NOSUPERUSER NOCREATEDB NOCREATEROLE;

GRANT nldmap_developers TO :"dev_user";
GRANT CONNECT ON DATABASE :"db_name" TO nldmap_developers;
GRANT USAGE, CREATE ON SCHEMA public TO nldmap_developers;

ALTER DEFAULT PRIVILEGES
    FOR ROLE CURRENT_USER
    IN SCHEMA public
    GRANT SELECT, INSERT, UPDATE, DELETE
    ON TABLES TO nldmap_developers;

ALTER DEFAULT PRIVILEGES
    FOR ROLE CURRENT_USER
    IN SCHEMA public
    GRANT USAGE, SELECT, UPDATE
    ON SEQUENCES TO nldmap_developers;
SQL
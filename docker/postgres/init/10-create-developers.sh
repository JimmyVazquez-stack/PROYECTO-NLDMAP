#!/usr/bin/env bash
set -Eeuo pipefail

: "${NLDMAP_DEV1_USER:?Falta NLDMAP_DEV1_USER}"
: "${NLDMAP_DEV1_PASSWORD:?Falta NLDMAP_DEV1_PASSWORD}"
: "${NLDMAP_DEV2_USER:?Falta NLDMAP_DEV2_USER}"
: "${NLDMAP_DEV2_PASSWORD:?Falta NLDMAP_DEV2_PASSWORD}"

psql \
    --username "$POSTGRES_USER" \
    --dbname "$POSTGRES_DB" \
    --set=dev1_user="$NLDMAP_DEV1_USER" \
    --set=dev1_password="$NLDMAP_DEV1_PASSWORD" \
    --set=dev2_user="$NLDMAP_DEV2_USER" \
    --set=dev2_password="$NLDMAP_DEV2_PASSWORD" \
    --set=db_name="$POSTGRES_DB" \
    --set=ON_ERROR_STOP=1 <<'SQL'
CREATE ROLE nldmap_developers NOLOGIN;

CREATE ROLE :"dev1_user"
    LOGIN PASSWORD :'dev1_password'
    NOSUPERUSER NOCREATEDB NOCREATEROLE;

CREATE ROLE :"dev2_user"
    LOGIN PASSWORD :'dev2_password'
    NOSUPERUSER NOCREATEDB NOCREATEROLE;

GRANT nldmap_developers TO :"dev1_user";
GRANT nldmap_developers TO :"dev2_user";

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

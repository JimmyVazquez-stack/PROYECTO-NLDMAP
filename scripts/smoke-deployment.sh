#!/usr/bin/env bash
# Solo CI: nombres únicos y datos desechables. Nunca ejecutar sobre producción.
set -euo pipefail
export SERVER_IMAGE=nldmap-server IMAGE_TAG=ci SITE_DOMAIN=localhost
export POSTGRES_ADMIN_PASSWORD="$(openssl rand -hex 24)"
export POSTGRES_APP_PASSWORD="$(openssl rand -hex 24)"
export MONGO_ROOT_PASSWORD="$(openssl rand -hex 24)"
export MONGO_APP_PASSWORD="$(openssl rand -hex 24)"
export BootstrapAdmin__Email=smoke@example.com
export BootstrapAdmin__Password="A1!$(openssl rand -hex 24)"
compose=(docker compose -p "nldmap-ci-${GITHUB_RUN_ID:-local}" -f compose.production.yml)
cleanup() {
  result=$?
  if [ "$result" -ne 0 ]; then "${compose[@]}" logs --tail=100; fi
  "${compose[@]}" --profile init down -v --remove-orphans
  exit "$result"
}
trap cleanup EXIT
"${compose[@]}" config --quiet
"${compose[@]}" up -d --wait postgres mongodb
"${compose[@]}" run --rm -e SeedIdentity=true -e BootstrapAdmin__Enabled=true \
  -e BootstrapAdmin__Email -e BootstrapAdmin__Password init
# Repetir comprueba que la inicialización no duplica ni destruye datos.
"${compose[@]}" run --rm -e SeedIdentity=true -e BootstrapAdmin__Enabled=true \
  -e BootstrapAdmin__Email -e BootstrapAdmin__Password init
count=$("${compose[@]}" exec -T postgres psql -U postgres -d nldmap -Atc 'SELECT count(*) FROM "AspNetUsers";')
test "$count" = 1
"${compose[@]}" exec -T mongodb mongosh --quiet --eval '
const d=db.getSiblingDB("nldmap");
if (!d.auth(process.env.MONGO_APP_USER, process.env.MONGO_APP_PASSWORD)) quit(1);
d.deployment_smoke.insertOne({_id:"persistent"});
if (d.deployment_smoke.countDocuments({_id:"persistent"})!==1) quit(2);
'
"${compose[@]}" up -d --wait server
"${compose[@]}" exec -T server curl -fsS -H 'Host: localhost' -H 'X-Forwarded-Proto: https' http://127.0.0.1:8080/Account/Login > /tmp/nldmap-login.html
grep -q '__RequestVerificationToken' /tmp/nldmap-login.html
"${compose[@]}" run --rm --no-deps caddy caddy validate --config /etc/caddy/Caddyfile --adapter caddyfile
"${compose[@]}" restart postgres mongodb
"${compose[@]}" up -d --wait postgres mongodb
"${compose[@]}" exec -T mongodb mongosh --quiet --eval '
const d=db.getSiblingDB("nldmap");
if (!d.auth(process.env.MONGO_APP_USER, process.env.MONGO_APP_PASSWORD)) quit(1);
if (d.deployment_smoke.countDocuments({_id:"persistent"})!==1) quit(2);
d.deployment_smoke.drop();
'
count=$("${compose[@]}" exec -T postgres psql -U postgres -d nldmap -Atc 'SELECT count(*) FROM "AspNetUsers";')
test "$count" = 1

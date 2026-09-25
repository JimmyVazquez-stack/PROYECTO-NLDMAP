#!/usr/bin/env bash

set -e

echo ""
echo "=========================================="
echo "      NLDMAP Development Environment"
echo "=========================================="
echo ""


# ----------------------------------------------------------
# Verify project root
# ----------------------------------------------------------

if [ ! -f "docker-compose.yml" ]; then
    echo "ERROR: Ejecuta este script desde la raíz del proyecto."
    exit 1
fi


# ----------------------------------------------------------
# Environment configuration
# ----------------------------------------------------------

if [ ! -f ".env" ]; then

    if [ ! -f ".env.example" ]; then
        echo "ERROR: No se encontró .env.example"
        exit 1
    fi

    cp .env.example .env

    echo "✓ .env creado desde .env.example"

else

    echo "✓ .env ya existe"

fi


# ----------------------------------------------------------
# Project directories
# ----------------------------------------------------------

mkdir -p backend
mkdir -p frontend

echo "✓ Estructura del proyecto verificada"


# ----------------------------------------------------------
# Docker verification
# ----------------------------------------------------------

if command -v docker >/dev/null 2>&1; then

    echo "✓ Docker encontrado"

else

    echo ""
    echo "ADVERTENCIA: Docker no está instalado o no está en PATH."
    echo ""

fi


# ----------------------------------------------------------
# Validate Compose
# ----------------------------------------------------------

if docker compose version >/dev/null 2>&1; then

    echo "✓ Docker Compose encontrado"

    echo ""
    echo "Validando docker-compose.yml..."

    docker compose config >/dev/null

    echo "✓ docker-compose.yml válido"

fi


echo ""
echo "=========================================="
echo " Configuración inicial terminada"
echo "=========================================="
echo ""
echo "Siguiente paso:"
echo ""
echo "  code ."
echo ""
echo "Después:"
echo ""
echo "  Dev Containers: Reopen in Container"
echo ""
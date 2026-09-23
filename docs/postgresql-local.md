# PostgreSQL en el entorno de desarrollo

PostgreSQL se ejecuta en el servicio `postgres` de Docker Compose. El servicio
`dev` incluye el cliente `psql` para conectarse a la base `nldmap` por la red
interna de Compose. Los datos de PostgreSQL se conservan en un volumen.

## Preparación inicial después de clonar

Realiza estos pasos en **Debian**, desde la raíz del repositorio, **antes de
abrir el proyecto en el Dev Container**:

```bash
cp .env.persistence.example .env.persistence
chmod 600 .env.persistence
nano .env.persistence
```

En `.env.persistence`, sustituye todos los valores `CHANGE_ME_...` por las
contraseñas locales asignadas. Guarda el archivo y comprueba la configuración:

```bash
docker compose config --quiet
```

`.env.persistence` contiene credenciales y está ignorado por Git. 
`.env.persistence.example` contiene solo marcadores y sirve como plantilla.

Los scripts de inicialización crean la base `nldmap` y los roles de
desarrollador **solo cuando el volumen de PostgreSQL está vacío**. Cambiar el
archivo de entorno después de inicializar el volumen no vuelve a ejecutar esos
scripts ni cambia automáticamente las contraseñas existentes.

## Abrir el Dev Container

En VS Code con ctrl + shift + p  y ejecuta
**Dev Containers: Reopen in Container**.

El servicio `dev` depende de `postgres` y espera a que PostgreSQL esté
saludable antes de iniciarse.

## Consultar la base desde zsh en `dev`

En la terminal de VS Code **dentro de `dev`**, comprueba el cliente:

```zsh
psql --version
```

Conéctate como `eduardo`:

```zsh
psql -h postgres -p 5432 -U eduardo -d nldmap -W \
  -c 'SELECT current_user, current_database();'
```

Conéctate como `jimmy`:

```zsh
psql -h postgres -p 5432 -U jimmy -d nldmap -W \
  -c 'SELECT current_user, current_database();'
```

`-W` solicita la contraseña de forma interactiva. Cada consulta debe mostrar
el usuario correspondiente y `nldmap` como base de datos. Dentro de la red de
Compose, `postgres` es el nombre del servicio al que se conecta `dev`.

Para abrir una sesión interactiva, por ejemplo como `eduardo`:

```zsh
psql -h postgres -p 5432 -U eduardo -d nldmap -W
```

Una vez dentro de `psql`:

```sql
SELECT current_user, current_database();
\conninfo
\q
```
## PostgreSQL desde el Dev Container

El entorno de desarrollo incluye el cliente `psql`. PostgreSQL se ejecuta en
otro servicio de Docker Compose, llamado `postgres`, y almacena sus datos en un
volumen persistente.

### Preparación inicial

Antes de abrir el Dev Container por primera vez:

1. Copia `.env.persistence.example` como `.env.persistence`.
2. Completa las contraseñas asignadas de forma privada.
3. Comprueba que `.env.persistence` esté ignorado por Git.

Las contraseñas reales no deben añadirse al repositorio. Los scripts de
inicialización crean la base `nldmap` y los usuarios únicamente cuando el
volumen de PostgreSQL está vacío.

Abre el proyecto en VS Code con **Dev Containers: Reopen in Container**. El
servicio `dev` depende de `postgres` y espera a que esté saludable.

### Consultar la base desde zsh en `dev`

Comprueba que el cliente esté disponible:

psql --version


Conéctate como `eduardo`:
psql -h postgres -p 5432 -U eduardo -d nldmap -W \
  -c 'SELECT current_user, current_database();'


Conéctate como `jimmy`:
psql -h postgres -p 5432 -U jimmy -d nldmap -W \
  -c 'SELECT current_user, current_database();'


La opción `-W` solicita la contraseña de forma interactiva. Cada consulta debe mostrar el nombre del usuario utilizado y `nldmap` como base de datos.

Para abrir una sesión interactiva, por ejemplo como `eduardo`:

psql -h postgres -p 5432 -U eduardo -d nldmap -W

Dentro de `psql` puedes ejecutar:

SELECT current_user, current_database();
\conninfo
\q

Desde `dev`, el servidor se llama `postgres`, que es el nombre del servicio en Docker Compose. `localhost` dentro de `dev` se refiere al propio contenedor de desarrollo.


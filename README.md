### Bases de datos locales

El Dev Container inicia los servicios `dev`, `postgres` y `mongo`. Antes de
abrirlo por primera vez, prepara los archivos locales de credenciales siguiendo
estas guías:

- [PostgreSQL: configuración y consultas desde `dev`](docs/postgresql-local.md)
- [MongoDB: configuración y consultas desde `dev`](docs/mongodb-local.md)

PostgreSQL utiliza la base `nldmap` y MongoDB utiliza `nldmap_reports`.
Las cuentas de desarrollo de cada motor se crean automáticamente al
inicializar sus respectivos volúmenes por primera vez. Los archivos
`.env.persistence` y `.env.mongo` contienen credenciales locales y no se
incluyen en Git.
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





PREPARACIÓN EN WINDOWS: WSL Y DOCKER DESKTOP

1. Abre PowerShell como administrador e instala WSL:

   wsl --install

   Reinicia Windows si se solicita. En el primer inicio de la distribución Linux, configura tu usuario y contraseña.

2. Actualiza WSL desde PowerShell:

   wsl --update

3. Instala Docker Desktop para Windows desde:

   https://www.docker.com/products/docker-desktop/

   Abre Docker Desktop, utiliza el motor basado en WSL 2 y habilita tu distribución en Settings > Resources > WSL Integration.

4. Comprueba desde la terminal de tu distribución WSL que Docker está disponible:

   docker version
   docker compose version

5. Instala Git dentro de tu distribución WSL. Por ejemplo, en Debian o Ubuntu:

   sudo apt update
   sudo apt install -y git
   git --version

6. Clona el repositorio dentro de tu directorio personal de WSL y ábrelo en VS Code. Antes de reabrirlo en el Dev Container, prepara los archivos locales de entorno requeridos por Docker Compose según la documentación del proyecto.


tener instalado docker y dev containers extension vs code




git fetch origin
git switch develop

git pull --ff-only origin develop


Si git switch develop dice que la rama local no existe, créala siguiendo la remota:

git switch --track origin/develop

git status

git branch --show-current




git config --show-origin --get user.name
git config --show-origin --get user.email



Ctrl+Shift+P → Dev Containers: Rebuild and Reopen in Container.



^M indica que los archivos de configuración de zsh llegaron con finales de línea de Windows (CRLF). Zsh dentro del contenedor espera LF; por eso interpreta ^M como parte de comandos y rutas. El cambio de equipo a Windows hizo visible este problema en docker/dev/.zshrc.

Corrección permanente desde VS Code
Abre docker/dev/.zshrc.
En la esquina inferior derecha de VS Code, haz clic en CRLF.
Selecciona LF y guarda con Ctrl+S.
Haz lo mismo con docker/dev/.p10k.zsh.
Usa Dev Containers: Rebuild and Reopen in Container para que el Dockerfile vuelva a copiarlos a /root.

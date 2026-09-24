# MongoDB en el entorno de desarrollo

MongoDB se ejecuta en el servicio `mongo` de Docker Compose. El servicio
`dev` incluye el cliente `mongosh` y se conecta a MongoDB mediante el
nombre de servicio `mongo`, puerto `27017`.

Los reportes utilizarán la base `nldmap_reports`. La cuenta `admin` es administradora;
 `eduardo` y `jimmy` tienen permisos `readWrite` únicamente
sobre `nldmap_reports`.

## Preparación inicial después de clonar

En Debian, desde la raíz del repositorio y antes de abrir el Dev Container:

```bash
cp .env.mongo.example .env.mongo
chmod 600 .env.mongo
nano .env.mongo
```

Sustituye todos los valores `CHANGE_ME_...` por contraseñas locales distintas.
El archivo `.env.mongo` está ignorado por Git; no confirmes sus credenciales.

Comprueba la configuración sin imprimir sus valores:

```bash
docker compose config --quiet
```

Abre la carpeta en VS Code mediante **Dev Containers: Reopen in Container**.
`dev` esperará a que `mongo` y `postgres` estén saludables.

> Las cuentas se crean mediante el script de inicialización solo cuando el
> volumen de MongoDB está vacío. Cambiar posteriormente `.env.mongo` no cambia
> automáticamente las contraseñas almacenadas en MongoDB.
## Consultar MongoDB desde zsh en `dev`

Abre una terminal en VS Code dentro del servicio `dev`. Comprueba que el
cliente esté instalado:

```zsh
mongosh --version
```

Los siguientes comandos se ejecutan en zsh. Cuando veas el indicador de
`mongosh` (por ejemplo, `test>`), ejecuta allí los comandos de MongoDB.

### Entrar como Eduardo

```zsh
mongosh --host mongo --port 27017 \
  --username eduardo \
  --authenticationDatabase nldmap_reports \
  --password
```

Introduce la contraseña de Eduardo cuando se solicite. No la escribas como
parte del comando.

Una vez dentro de `mongosh`, confirma qué usuario inició sesión:

```javascript
db.runCommand({ connectionStatus: 1 }).authInfo.authenticatedUsers
```


Selecciona la base de reportes y confirma que ahora es la base activa:

```javascript
use nldmap_reports
db
```

El comando `db` debe mostrar `nldmap_reports`. También puedes consultar las
colecciones de la base seleccionada:

```javascript
show collections
```

Termina la sesión:

```javascript
exit
```

### Entrar como Jimmy

Desde zsh, inicia una sesión nueva:

```zsh
mongosh --host mongo --port 27017 \
  --username jimmy \
  --authenticationDatabase nldmap_reports \
  --password
```

Dentro de `mongosh`, repite las comprobaciones:

```javascript
db.runCommand({ connectionStatus: 1 }).authInfo.authenticatedUsers
use nldmap_reports
db
show collections
exit
```

El usuario autenticado debe ser `jimmy` y, después de `use`, la base activa
debe ser `nldmap_reports`.

### Entrar como administrador

Usa esta cuenta únicamente para tareas administrativas. La cuenta `admin`
se autentica en la base `admin`:

```zsh
mongosh --host mongo --port 27017 \
  --username admin \
  --authenticationDatabase admin \
  --password
```

Dentro de `mongosh`:

```javascript
show dbs
db
db.getSiblingDB("nldmap_reports").getUsers()
exit
```

`getUsers()` permite comprobar que `eduardo` y `jimmy` existen en
`nldmap_reports` y tienen el rol `readWrite`.

> `show dbs` enumera las bases visibles para el usuario. Una base nueva puede
> no aparecer todavía si no contiene datos. 

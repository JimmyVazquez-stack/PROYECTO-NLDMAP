// Se ejecuta solo al inicializar un volumen vacío.
const database = db.getSiblingDB(process.env.MONGO_INITDB_DATABASE);
database.createUser({
  user: process.env.MONGO_APP_USER,
  pwd: process.env.MONGO_APP_PASSWORD,
  roles: [{ role: "readWrite", db: process.env.MONGO_INITDB_DATABASE }]
});

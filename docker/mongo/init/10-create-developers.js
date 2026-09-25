const required = [
  "MONGO_INITDB_DATABASE",
  "NLDMAP_MONGO_DEV_USER",
  "NLDMAP_MONGO_DEV_PASSWORD",
];

for (const name of required) {
  if (!process.env[name] || process.env[name].startsWith("CHANGE_ME_")) {
    throw new Error(`Falta configurar ${name}`);
  }
}

const databaseName = process.env.MONGO_INITDB_DATABASE;
const reportsDb = db.getSiblingDB(databaseName);

reportsDb.createUser({
  user: process.env.NLDMAP_MONGO_DEV_USER,
  pwd: process.env.NLDMAP_MONGO_DEV_PASSWORD,
  roles: [{ role: "readWrite", db: databaseName }],
});
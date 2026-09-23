const required = [
  "MONGO_INITDB_DATABASE",
  "NLDMAP_MONGO_DEV1_USER",
  "NLDMAP_MONGO_DEV1_PASSWORD",
  "NLDMAP_MONGO_DEV2_USER",
  "NLDMAP_MONGO_DEV2_PASSWORD",
];

for (const name of required) {
  if (!process.env[name] || process.env[name].startsWith("CHANGE_ME_")) {
    throw new Error(`Falta configurar ${name}`);
  }
}

const databaseName = process.env.MONGO_INITDB_DATABASE;
const users = [
  {
    user: process.env.NLDMAP_MONGO_DEV1_USER,
    pwd: process.env.NLDMAP_MONGO_DEV1_PASSWORD,
  },
  {
    user: process.env.NLDMAP_MONGO_DEV2_USER,
    pwd: process.env.NLDMAP_MONGO_DEV2_PASSWORD,
  },
];

if (users[0].user === users[1].user) {
  throw new Error("Los usuarios de desarrollo deben ser distintos");
}

const reportsDb = db.getSiblingDB(databaseName);

for (const account of users) {
  reportsDb.createUser({
    user: account.user,
    pwd: account.pwd,
    roles: [{ role: "readWrite", db: databaseName }],
  });
}
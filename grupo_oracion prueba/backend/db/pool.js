const { Pool } = require("pg");

const pool = new Pool({
  connectionString: process.env.DATABASE_URL,
  ssl: { rejectUnauthorized: false }, // necesario para Supabase
});

pool.on("error", (err) => {
  console.error("Error inesperado en la pool de PostgreSQL:", err);
});

module.exports = pool;

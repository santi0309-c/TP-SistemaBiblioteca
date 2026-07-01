require("dotenv").config();
const express = require("express");
const cors = require("cors");

const app = express();

// ============================================
// MIDDLEWARES GLOBALES
// ============================================
app.use(cors({
  origin: process.env.FRONTEND_URL || "*",
  methods: ["GET", "POST", "PATCH", "DELETE"],
  allowedHeaders: ["Content-Type", "Authorization"],
}));
app.use(express.json());

// ============================================
// RUTAS
// ============================================
app.use("/api/auth",     require("./routes/auth"));
app.use("/api/personas", require("./routes/personas"));
app.use("/api/motivos",  require("./routes/motivos"));
app.use("/api/usuario",  require("./routes/usuario"));

// Health check para Render
app.get("/", (req, res) => {
  res.json({ status: "ok", mensaje: "API Grupo de Oración funcionando 🙏" });
});

// ============================================
// ARRANCAR
// ============================================
const PORT = process.env.PORT || 3000;
app.listen(PORT, () => {
  console.log(`Servidor corriendo en http://localhost:${PORT}`);
});
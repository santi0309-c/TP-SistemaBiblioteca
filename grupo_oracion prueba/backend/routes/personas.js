const router = require("express").Router();
const pool = require("../db/pool");
const { autenticar, soloAdmin } = require("../middleware/auth");

// ============================================
// GET /api/personas — listar todas las personas activas
// ============================================
router.get("/", autenticar, async (req, res) => {
  try {
    const { rows } = await pool.query(
      `SELECT p.id, p.nombre, p.foto_url,
              COUNT(m.id) FILTER (WHERE m.estado != 'respondido') AS motivos_activos
       FROM personas p
       LEFT JOIN motivos m ON m.persona_id = p.id
         AND (m.es_privado = false OR m.usuario_id = $1)
       WHERE p.activa = true
       GROUP BY p.id
       ORDER BY p.nombre ASC`,
      [req.usuario.id]
    );
    res.json(rows);
  } catch (err) {
    console.error(err);
    res.status(500).json({ error: "Error del servidor" });
  }
});

// ============================================
// POST /api/personas — crear persona (solo admin)
// ============================================
router.post("/", autenticar, soloAdmin, async (req, res) => {
  const { nombre, foto_url } = req.body;
  if (!nombre) return res.status(400).json({ error: "Nombre requerido" });

  try {
    const { rows } = await pool.query(
      "INSERT INTO personas (nombre, foto_url) VALUES ($1, $2) RETURNING *",
      [nombre, foto_url || null]
    );
    res.status(201).json(rows[0]);
  } catch (err) {
    console.error(err);
    res.status(500).json({ error: "Error del servidor" });
  }
});

// ============================================
// DELETE /api/personas/:id — desactivar persona (solo admin)
// ============================================
router.delete("/:id", autenticar, soloAdmin, async (req, res) => {
  try {
    await pool.query("UPDATE personas SET activa = false WHERE id = $1", [req.params.id]);
    res.json({ ok: true });
  } catch (err) {
    console.error(err);
    res.status(500).json({ error: "Error del servidor" });
  }
});

module.exports = router;
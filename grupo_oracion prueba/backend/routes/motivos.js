const router = require("express").Router();
const pool = require("../db/pool");
const { autenticar, soloAdmin } = require("../middleware/auth");

// ============================================
// GET /api/motivos/todos — vista grupal
// Devuelve todos los motivos no-privados de todas las personas
// ============================================
router.get("/todos", autenticar, async (req, res) => {
  try {
    const { rows } = await pool.query(
      `SELECT
         m.id,
         m.texto,
         m.estado,
         m.creado_en,
         p.id   AS persona_id,
         p.nombre AS persona_nombre,
         p.foto_url AS persona_foto,
         u.nombre AS cargado_por,
         COUNT(o.id) AS veces_orado,
         EXISTS (
           SELECT 1 FROM oraciones o2
           WHERE o2.motivo_id = m.id AND o2.usuario_id = $1
         ) AS yo_ore_hoy
       FROM motivos m
       JOIN personas p ON p.id = m.persona_id
       JOIN usuarios u ON u.id = m.usuario_id
       LEFT JOIN oraciones o ON o.motivo_id = m.id
       WHERE m.es_privado = false
         AND m.estado != 'respondido'
         AND p.activa = true
       GROUP BY m.id, p.id, u.nombre
       ORDER BY p.nombre ASC, m.creado_en DESC`,
      [req.usuario.id]
    );
    res.json(rows);
  } catch (err) {
    console.error(err);
    res.status(500).json({ error: "Error del servidor" });
  }
});


// ============================================
// GET /api/motivos/admin/todos — todos los motivos de todos los usuarios (solo admin)
// Incluye privados
// ============================================
router.get("/admin/todos", autenticar, async (req, res) => {
  if (req.usuario.rol !== "admin")
    return res.status(403).json({ error: "Solo administradores" });
  try {
    const { rows } = await pool.query(
      `SELECT
         m.id,
         m.texto,
         m.estado,
         m.es_privado,
         m.testimonio,
         m.creado_en,
         p.nombre AS persona_nombre,
         u.nombre AS cargado_por,
         COUNT(o.id) AS veces_orado
       FROM motivos m
       JOIN personas p ON p.id = m.persona_id
       JOIN usuarios u ON u.id = m.usuario_id
       LEFT JOIN oraciones o ON o.motivo_id = m.id
       WHERE p.activa = true
       GROUP BY m.id, p.nombre, u.nombre
       ORDER BY p.nombre ASC, m.creado_en DESC`
    );
    res.json(rows);
  } catch (err) {
    console.error(err);
    res.status(500).json({ error: "Error del servidor" });
  }
});

// ============================================
// GET /api/motivos/:personaId — motivos de una persona
// ============================================
router.get("/:personaId", autenticar, async (req, res) => {
  try {
    const { rows } = await pool.query(
      `SELECT
         m.id,
         m.texto,
         m.estado,
         m.es_privado,
         m.testimonio,
         m.creado_en,
         m.actualizado,
         u.nombre AS cargado_por,
         COUNT(o.id) AS veces_orado,
         EXISTS (
           SELECT 1 FROM oraciones o2
           WHERE o2.motivo_id = m.id AND o2.usuario_id = $1
         ) AS yo_ore_hoy
       FROM motivos m
       JOIN usuarios u ON u.id = m.usuario_id
       LEFT JOIN oraciones o ON o.motivo_id = m.id
       WHERE m.persona_id = $2
         AND (m.es_privado = false OR m.usuario_id = $1)
       GROUP BY m.id, u.nombre
       ORDER BY
         CASE m.estado
           WHEN 'oracion'    THEN 1
           WHEN 'activo'     THEN 2
           WHEN 'constante'  THEN 3
           WHEN 'respondido' THEN 4
         END,
         m.creado_en DESC`,
      [req.usuario.id, req.params.personaId]
    );
    res.json(rows);
  } catch (err) {
    console.error(err);
    res.status(500).json({ error: "Error del servidor" });
  }
});

// ============================================
// POST /api/motivos — crear motivo
// Body: { persona_id, texto, estado?, es_privado? }
// ============================================
router.post("/", autenticar, async (req, res) => {
  const { persona_id, texto, estado = "oracion", es_privado = false } = req.body;
  if (!persona_id || !texto)
    return res.status(400).json({ error: "persona_id y texto requeridos" });

  try {
    const { rows } = await pool.query(
      `INSERT INTO motivos (persona_id, usuario_id, texto, estado, es_privado)
       VALUES ($1, $2, $3, $4, $5)
       RETURNING *`,
      [persona_id, req.usuario.id, texto, estado, es_privado]
    );
    res.status(201).json(rows[0]);
  } catch (err) {
    console.error(err);
    res.status(500).json({ error: "Error del servidor" });
  }
});

// ============================================
// PATCH /api/motivos/:id — editar motivo (solo el que lo cargó o admin)
// Body: { texto?, estado?, es_privado? }
// ============================================
router.patch("/:id", autenticar, async (req, res) => {
  const { texto, estado, es_privado, testimonio } = req.body;

  try {
    // Verificar pertenencia
    const { rows: check } = await pool.query(
      "SELECT * FROM motivos WHERE id = $1",
      [req.params.id]
    );
    const motivo = check[0];
    if (!motivo) return res.status(404).json({ error: "Motivo no encontrado" });

    if (motivo.usuario_id !== req.usuario.id && req.usuario.rol !== "admin")
      return res.status(403).json({ error: "No tenés permiso para editar este motivo" });

    const { rows } = await pool.query(
      `UPDATE motivos SET
         texto      = COALESCE($1, texto),
         estado     = COALESCE($2, estado),
         es_privado = COALESCE($3, es_privado),
         testimonio = COALESCE($5, testimonio),
         actualizado = NOW()
       WHERE id = $4
       RETURNING *`,
      [texto || null, estado || null, es_privado ?? null, req.params.id, testimonio || null]
    );
    res.json(rows[0]);
  } catch (err) {
    console.error(err);
    res.status(500).json({ error: "Error del servidor" });
  }
});

// ============================================
// DELETE /api/motivos/:id — eliminar motivo
// ============================================
router.delete("/:id", autenticar, async (req, res) => {
  try {
    const { rows: check } = await pool.query(
      "SELECT * FROM motivos WHERE id = $1",
      [req.params.id]
    );
    const motivo = check[0];
    if (!motivo) return res.status(404).json({ error: "Motivo no encontrado" });

    if (motivo.usuario_id !== req.usuario.id && req.usuario.rol !== "admin")
      return res.status(403).json({ error: "No tenés permiso para eliminar este motivo" });

    await pool.query("DELETE FROM motivos WHERE id = $1", [req.params.id]);
    res.json({ ok: true });
  } catch (err) {
    console.error(err);
    res.status(500).json({ error: "Error del servidor" });
  }
});

// ============================================
// POST /api/motivos/:id/ore — registrar que oré
// ============================================
router.post("/:id/ore", autenticar, async (req, res) => {
  try {
    // Evitar duplicados en el mismo día
    const { rows: ya } = await pool.query(
      `SELECT id FROM oraciones
       WHERE motivo_id = $1 AND usuario_id = $2
         AND fecha > NOW() - INTERVAL '20 hours'`,
      [req.params.id, req.usuario.id]
    );
    if (ya.length > 0)
      return res.json({ ok: true, mensaje: "Ya registraste oración hoy" });

    await pool.query(
      "INSERT INTO oraciones (motivo_id, usuario_id) VALUES ($1, $2)",
      [req.params.id, req.usuario.id]
    );
    res.json({ ok: true, mensaje: "¡Oración registrada!" });
  } catch (err) {
    console.error(err);
    res.status(500).json({ error: "Error del servidor" });
  }
});

module.exports = router;

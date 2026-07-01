const router  = require("express").Router();
const bcrypt  = require("bcryptjs");
const multer  = require("multer");
const pool    = require("../db/pool");
const { autenticar } = require("../middleware/auth");

// multer en memoria — límite 2MB
const upload = multer({
  storage: multer.memoryStorage(),
  limits: { fileSize: 2 * 1024 * 1024 },
  fileFilter(req, file, cb) {
    const allowed = ["image/jpeg", "image/png", "image/webp"];
    allowed.includes(file.mimetype) ? cb(null, true) : cb(new Error("Solo jpg, png o webp"));
  },
});

// ============================================
// GET /api/usuario/perfil
// ============================================
router.get("/perfil", autenticar, async (req, res) => {
  try {
    const { rows } = await pool.query(
      "SELECT id, email, nombre, foto_url, rol FROM usuarios WHERE id = $1",
      [req.usuario.id]
    );
    if (!rows[0]) return res.status(404).json({ error: "Usuario no encontrado" });
    res.json(rows[0]);
  } catch (err) {
    console.error(err);
    res.status(500).json({ error: "Error del servidor" });
  }
});

// ============================================
// PATCH /api/usuario/perfil — editar nombre y/o foto_url
// ============================================
router.patch("/perfil", autenticar, async (req, res) => {
  const { nombre, foto_url } = req.body;
  
  if (!nombre && foto_url === undefined) {
    return res.status(400).json({ error: "Nada para actualizar" });
  }

  try {
    // 1. Buscamos el nombre EXACTO que tiene el usuario AHORA MISMO en la BD
    // (Ignoramos el token porque puede estar desactualizado)
    const { rows: usuarioActual } = await pool.query(
      "SELECT nombre FROM usuarios WHERE id = $1",
      [req.usuario.id]
    );
    const nombreEnDB = usuarioActual[0].nombre;

    // 2. Actualizamos la cuenta privada (usuarios)
    const { rows } = await pool.query(
      `UPDATE usuarios SET
         nombre   = COALESCE($1, nombre),
         foto_url = COALESCE($2, foto_url)
       WHERE id = $3
       RETURNING id, email, nombre, foto_url, rol`,
      [nombre || null, foto_url !== undefined ? foto_url : null, req.usuario.id]
    );
    
    // Guardamos los datos recién actualizados para pasarlos a la tabla personas
    const usuarioActualizado = rows[0];

    // 3. Actualizamos la tarjeta pública (personas) usando el nombre viejo para encontrarla sin fallar
    if (nombre || foto_url !== undefined) {
      await pool.query(
        `UPDATE personas SET 
           nombre = $1,
           foto_url = $2
         WHERE nombre = $3 AND activa = true`,
        [usuarioActualizado.nombre, usuarioActualizado.foto_url, nombreEnDB]
      );
    }

    res.json(usuarioActualizado);
  } catch (err) {
    console.error(err);
    res.status(500).json({ error: "Error del servidor" });
  }
});

// ============================================
// PATCH /api/usuario/password
// ============================================
router.patch("/password", autenticar, async (req, res) => {
  const { actual, nueva } = req.body;
  if (!actual || !nueva)
    return res.status(400).json({ error: "Contraseña actual y nueva requeridas" });
  if (nueva.length < 8)
    return res.status(400).json({ error: "La nueva contraseña debe tener al menos 8 caracteres" });

  try {
    const { rows } = await pool.query(
      "SELECT password FROM usuarios WHERE id = $1", [req.usuario.id]
    );
    const ok = await bcrypt.compare(actual, rows[0].password);
    if (!ok) return res.status(401).json({ error: "La contraseña actual no es correcta" });

    const hash = await bcrypt.hash(nueva, 12);
    await pool.query("UPDATE usuarios SET password = $1 WHERE id = $2", [hash, req.usuario.id]);
    res.json({ ok: true });
  } catch (err) {
    console.error(err);
    res.status(500).json({ error: "Error del servidor" });
  }
});

// ============================================
// POST /api/usuario/foto — subir foto de perfil
// Recibe multipart/form-data con campo "foto"
// ============================================
router.post("/foto", autenticar, upload.single("foto"), async (req, res) => {
  if (!req.file) return res.status(400).json({ error: "No se recibió ningún archivo" });

  const ext         = req.file.mimetype.split("/")[1].replace("jpeg", "jpg");
  const filePath    = `fotos/${req.usuario.id}/${Date.now()}.${ext}`;
  const supabaseUrl = process.env.SUPABASE_URL;
  const supabaseKey = process.env.SUPABASE_SERVICE_KEY;
  const bucket      = "avatares";

  try {
    const uploadRes = await fetch(
      `${supabaseUrl}/storage/v1/object/${bucket}/${filePath}`,
      {
        method: "POST",
        headers: {
          "Authorization": `Bearer ${supabaseKey}`,
          "apikey": supabaseKey,
          "Content-Type": req.file.mimetype,
          "x-upsert": "true",
        },
        body: req.file.buffer,
      }
    );

    if (!uploadRes.ok) {
      const err = await uploadRes.text();
      console.error("Supabase Storage error:", err);
      return res.status(500).json({ error: "Error al subir al Storage" });
    }

    const publicUrl = `${supabaseUrl}/storage/v1/object/public/${bucket}/${filePath}`;

    // 1. Actualizar foto en tabla usuarios
    await pool.query(
      "UPDATE usuarios SET foto_url = $1 WHERE id = $2",
      [publicUrl, req.usuario.id]
    );

    // 2. Sincronizar foto en tabla personas (usando el nombre del usuario para encontrar su fila)
    const { rows: usuRows } = await pool.query(
      "SELECT nombre FROM usuarios WHERE id = $1",
      [req.usuario.id]
    );
    if (usuRows[0]) {
      await pool.query(
        "UPDATE personas SET foto_url = $1 WHERE nombre = $2 AND activa = true",
        [publicUrl, usuRows[0].nombre]
      );
    }

    res.json({ ok: true, foto_url: publicUrl });
  } catch (err) {
    console.error(err);
    res.status(500).json({ error: "Error del servidor" });
  }
});

// ============================================
// GET /api/usuario/seguimiento
// ============================================
router.get("/seguimiento", autenticar, async (req, res) => {
  try {
    const { rows } = await pool.query(
      `SELECT fecha::date AS fecha FROM oraciones_seguimiento
       WHERE usuario_id = $1 ORDER BY fecha DESC`,
      [req.usuario.id]
    );
    res.json(rows.map(r => r.fecha));
  } catch (err) {
    console.error(err);
    res.status(500).json({ error: "Error del servidor" });
  }
});

// ============================================
// POST /api/usuario/seguimiento — marcar/desmarcar hoy (zona Argentina)
// ============================================
router.post("/seguimiento", autenticar, async (req, res) => {
  const hoy = new Date(
    new Date().toLocaleString("en-US", { timeZone: "America/Argentina/Buenos_Aires" })
  ).toISOString().split("T")[0];

  try {
    const { rows } = await pool.query(
      `SELECT id FROM oraciones_seguimiento WHERE usuario_id = $1 AND fecha::date = $2`,
      [req.usuario.id, hoy]
    );

    if (rows.length > 0) {
      await pool.query(
        `DELETE FROM oraciones_seguimiento WHERE usuario_id = $1 AND fecha::date = $2`,
        [req.usuario.id, hoy]
      );
      res.json({ marcado: false });
    } else {
      await pool.query(
        `INSERT INTO oraciones_seguimiento (usuario_id, fecha) VALUES ($1, $2)`,
        [req.usuario.id, hoy]
      );
      res.json({ marcado: true });
    }
  } catch (err) {
    console.error(err);
    res.status(500).json({ error: "Error del servidor" });
  }
});

// ============================================
// GET /api/usuario/asignado
// ============================================
router.get("/asignado", autenticar, async (req, res) => {
  try {
    const { rows: todasPersonas } = await pool.query(
      "SELECT id, nombre, foto_url FROM personas WHERE activa = true ORDER BY nombre ASC"
    );
    if (!todasPersonas.length) return res.json({ persona: null });

    const { rows: usuRows } = await pool.query(
      "SELECT nombre FROM usuarios WHERE id = $1", [req.usuario.id]
    );
    const miNombre = usuRows[0]?.nombre || "";
    const indice   = todasPersonas.findIndex(
      p => p.nombre.toLowerCase().trim() === miNombre.toLowerCase().trim()
    );
    if (indice === -1) return res.json({ persona: null });

    const hoy  = new Date(new Date().toLocaleString("en-US", { timeZone: "America/Argentina/Buenos_Aires" }));
    const day  = hoy.getDay();
    const diff = hoy.getDate() - day + (day === 0 ? -6 : 1);
    const lunesSemana = new Date(hoy.getFullYear(), hoy.getMonth(), diff);

    const base    = new Date(2026, 4, 11); // Semana que empieza el 11 de mayo 2026
    const semanas = Math.max(0, Math.floor((lunesSemana - base) / (7 * 24 * 60 * 60 * 1000)));
    let finalIdx  = (indice + semanas) % todasPersonas.length;
    if (todasPersonas.length > 1 && todasPersonas[finalIdx].nombre === miNombre) {
      finalIdx = (finalIdx + 1) % todasPersonas.length;
    }

    res.json({ persona: todasPersonas[finalIdx] });
  } catch (err) {
    console.error(err);
    res.status(500).json({ error: "Error del servidor" });
  }
});

// ============================================
// GET /api/usuario/admin/seguimiento — solo admin
// ============================================
router.get("/admin/seguimiento", autenticar, async (req, res) => {
  if (req.usuario.rol !== "admin")
    return res.status(403).json({ error: "Solo administradores" });
  try {
    const { rows } = await pool.query(
      `SELECT
         u.id AS usuario_id,
         u.nombre,
         u.email,
         os.fecha::date AS fecha
       FROM usuarios u
       LEFT JOIN oraciones_seguimiento os ON os.usuario_id = u.id
       ORDER BY u.nombre ASC, os.fecha DESC`
    );
    const map = {};
    rows.forEach(r => {
      if (!map[r.usuario_id]) {
        map[r.usuario_id] = { id: r.usuario_id, nombre: r.nombre, email: r.email, fechas: [] };
      }
      if (r.fecha) map[r.usuario_id].fechas.push(r.fecha);
    });
    res.json(Object.values(map));
  } catch (err) {
    console.error(err);
    res.status(500).json({ error: "Error del servidor" });
  }
});

module.exports = router;

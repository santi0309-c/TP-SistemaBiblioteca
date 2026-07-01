const router = require("express").Router();
const bcrypt = require("bcryptjs");
const jwt = require("jsonwebtoken");
const crypto = require("crypto");
const { Resend } = require("resend");
const pool = require("../db/pool");
const { autenticar, soloAdmin } = require("../middleware/auth");

const resend = new Resend(process.env.RESEND_API_KEY);

// ============================================
// POST /api/auth/login
// Body: { email, password }
// ============================================
router.post("/login", async (req, res) => {
  const { email, password } = req.body;
  if (!email || !password)
    return res.status(400).json({ error: "Email y contraseña requeridos" });

  try {
    const { rows } = await pool.query(
      "SELECT * FROM usuarios WHERE email = $1 AND activo = true",
      [email.toLowerCase().trim()]
    );
    const usuario = rows[0];

    if (!usuario || !usuario.password)
      return res.status(401).json({ error: "Credenciales incorrectas" });

    const ok = await bcrypt.compare(password, usuario.password);
    if (!ok)
      return res.status(401).json({ error: "Credenciales incorrectas" });

    const token = jwt.sign(
      { id: usuario.id, email: usuario.email, nombre: usuario.nombre, rol: usuario.rol },
      process.env.JWT_SECRET,
      { expiresIn: "30d" }
    );

    res.json({
      token,
      usuario: { id: usuario.id, email: usuario.email, nombre: usuario.nombre, rol: usuario.rol },
    });
  } catch (err) {
    console.error(err);
    res.status(500).json({ error: "Error del servidor" });
  }
});

// ============================================
// POST /api/auth/invitar  (solo admin)
// Body: { email, nombre }
// ============================================
router.post("/invitar", autenticar, soloAdmin, async (req, res) => {
  const { email, nombre } = req.body;
  if (!email || !nombre)
    return res.status(400).json({ error: "Email y nombre requeridos" });

  try {
    // Generar token único de 32 bytes
    const token = crypto.randomBytes(32).toString("hex");
    const expira = new Date(Date.now() + 24 * 60 * 60 * 1000); // 24 horas

    // Insertar o actualizar usuario (re-invitar si ya existía)
    await pool.query(
      `INSERT INTO usuarios (email, nombre, token_inv, token_exp, activo)
       VALUES ($1, $2, $3, $4, false)
       ON CONFLICT (email) DO UPDATE SET
         nombre = EXCLUDED.nombre,
         token_inv = EXCLUDED.token_inv,
         token_exp = EXCLUDED.token_exp`,
      [email.toLowerCase().trim(), nombre, token, expira]
    );

    // También crear la persona en el grupo si no existe
    await pool.query(
      `INSERT INTO personas (nombre)
       VALUES ($1)
       ON CONFLICT DO NOTHING`,
      [nombre]
    );

    const link = `${process.env.FRONTEND_URL}/activar.html?token=${token}`;

    // Enviar email con Resend
    await resend.emails.send({
      from: process.env.EMAIL_FROM,
      to: email,
      subject: "Invitación al Grupo de Oración",
      html: `
        <div style="font-family:sans-serif;max-width:480px;margin:auto;padding:32px;background:#080f1e;color:#e8eaf0;border-radius:16px">
          <h2 style="color:#7c9fff;margin-bottom:8px">Grupo de Oración</h2>
          <p style="color:#a0a8c0;margin-bottom:24px">Hola <strong style="color:#e8eaf0">${nombre}</strong>, fuiste invitado/a a sumarte.</p>
          <a href="${link}"
             style="display:inline-block;padding:14px 28px;background:linear-gradient(90deg,#7c9fff,#b98fff);color:white;text-decoration:none;border-radius:50px;font-weight:600">
            Activar mi cuenta →
          </a>
          <p style="color:#5a6080;font-size:13px;margin-top:24px">Este link expira en 24 horas.</p>
        </div>
      `,
    });

    res.json({ ok: true, mensaje: `Invitación enviada a ${email}` });
  } catch (err) {
    console.error(err);
    res.status(500).json({ error: "Error al enviar la invitación" });
  }
});

// ============================================
// POST /api/auth/activar
// Body: { token, password }
// ============================================
router.post("/activar", async (req, res) => {
  const { token, password } = req.body;
  if (!token || !password)
    return res.status(400).json({ error: "Token y contraseña requeridos" });

  if (password.length < 8)
    return res.status(400).json({ error: "La contraseña debe tener al menos 8 caracteres" });

  try {
    const { rows } = await pool.query(
      "SELECT * FROM usuarios WHERE token_inv = $1 AND token_exp > NOW()",
      [token]
    );
    const usuario = rows[0];

    if (!usuario)
      return res.status(400).json({ error: "Token inválido o expirado" });

    const hash = await bcrypt.hash(password, 12);

    await pool.query(
      `UPDATE usuarios SET
         password = $1,
         activo = true,
         token_inv = NULL,
         token_exp = NULL
       WHERE id = $2`,
      [hash, usuario.id]
    );

    const jwtToken = jwt.sign(
      { id: usuario.id, email: usuario.email, nombre: usuario.nombre, rol: usuario.rol },
      process.env.JWT_SECRET,
      { expiresIn: "30d" }
    );

    res.json({
      token: jwtToken,
      usuario: { id: usuario.id, email: usuario.email, nombre: usuario.nombre, rol: usuario.rol },
    });
  } catch (err) {
    console.error(err);
    res.status(500).json({ error: "Error del servidor" });
  }
});

// ============================================
// GET /api/auth/me  — verificar token activo
// ============================================
router.get("/me", autenticar, async (req, res) => {
  res.json({ usuario: req.usuario });
});

module.exports = router;

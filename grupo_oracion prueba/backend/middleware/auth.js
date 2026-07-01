const jwt = require("jsonwebtoken");

/**
 * Middleware que verifica el JWT en el header Authorization.
 * Si es válido, adjunta `req.usuario` con { id, email, nombre, rol }.
 */
function autenticar(req, res, next) {
  const header = req.headers["authorization"];
  if (!header || !header.startsWith("Bearer ")) {
    return res.status(401).json({ error: "Token requerido" });
  }

  const token = header.slice(7);
  try {
    const payload = jwt.verify(token, process.env.JWT_SECRET);
    req.usuario = payload;
    next();
  } catch {
    return res.status(401).json({ error: "Token inválido o expirado" });
  }
}

/**
 * Middleware adicional que verifica que el usuario sea admin.
 * Siempre se usa DESPUÉS de `autenticar`.
 */
function soloAdmin(req, res, next) {
  if (req.usuario?.rol !== "admin") {
    return res.status(403).json({ error: "Solo el admin puede hacer esto" });
  }
  next();
}

module.exports = { autenticar, soloAdmin };

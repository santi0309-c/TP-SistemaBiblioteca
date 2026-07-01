/* ============================================
   API CLIENT — centraliza todas las llamadas al backend
   Importar en cada página: <script src="api.js"></script>
   ============================================ */

const API_URL = (window.location.hostname === "localhost" || window.location.hostname === "127.0.0.1")
  ? "http://localhost:3000"
  : "https://grupo-oracion.onrender.com"; // ← cambiá por tu URL de Render

/* ---- Token ---- */
function getToken() { return localStorage.getItem("token"); }
function getUsuario() { const u = localStorage.getItem("usuario"); return u ? JSON.parse(u) : null; }
function guardarSesion(token, usuarioData) {
  localStorage.setItem("token", token);
  localStorage.setItem("usuario", JSON.stringify(usuarioData));
}
function actualizarUsuarioLocal(cambios) {
  const u = getUsuario();
  if (u) localStorage.setItem("usuario", JSON.stringify({ ...u, ...cambios }));
}
function cerrarSesion() {
  localStorage.removeItem("token");
  localStorage.removeItem("usuario");
  window.location.href = "login.html";
}
 
/* ---- Fetch base ---- */
async function apiFetch(path, options = {}) {
  const token = getToken();
  const res = await fetch(API_URL + path, {
    ...options,
    headers: {
      "Content-Type": "application/json",
      ...(token ? { Authorization: "Bearer " + token } : {}),
      ...(options.headers || {}),
    },
    body: options.body ? JSON.stringify(options.body) : undefined,
  });
  if (res.status === 401) { cerrarSesion(); return; }
  const data = await res.json();
  if (!res.ok) throw new Error(data.error || "Error desconocido");
  return data;
}
 
/* ---- Auth ---- */
const auth = {
  login:   (email, password) => apiFetch("/api/auth/login",   { method: "POST", body: { email, password } }),
  activar: (token, password) => apiFetch("/api/auth/activar", { method: "POST", body: { token, password } }),
  invitar: (email, nombre)   => apiFetch("/api/auth/invitar", { method: "POST", body: { email, nombre } }),
  me:      ()                => apiFetch("/api/auth/me"),
};
 
/* ---- Personas ---- */
const personas = {
  listar:  ()             => apiFetch("/api/personas"),
  crear:   (nombre, foto) => apiFetch("/api/personas", { method: "POST", body: { nombre, foto_url: foto } }),
  borrar:  (id)           => apiFetch("/api/personas/" + id, { method: "DELETE" }),
};
 
/* ---- Motivos ---- */
const motivos = {
  todos:     (params = {})                             => apiFetch("/api/motivos/todos" + (Object.keys(params).length ? "?" + new URLSearchParams(params) : "")),
  stats:     ()                                        => apiFetch("/api/motivos/stats"),
  listar: (personaId)                             => apiFetch("/api/motivos/" + personaId),
  crear:  (persona_id, texto, estado, es_privado) =>
    apiFetch("/api/motivos", { method: "POST", body: { persona_id, texto, estado, es_privado } }),
  editar: (id, cambios)                           => apiFetch("/api/motivos/" + id, { method: "PATCH", body: cambios }),
  borrar: (id)                                    => apiFetch("/api/motivos/" + id, { method: "DELETE" }),
  ore:       (id)                                    => apiFetch("/api/motivos/" + id + "/ore", { method: "POST" }),
  testimonio:(id, texto)                              => apiFetch("/api/motivos/" + id, { method: "PATCH", body: { testimonio: texto } }),
  adminTodos:()                                       => apiFetch("/api/motivos/admin/todos"),
};
 
/* ---- Usuario / Perfil / Seguimiento ---- */
const usuarioApi = {
  perfil:          ()                      => apiFetch("/api/usuario/perfil"),
  editarPerfil:    (datos)                 => apiFetch("/api/usuario/perfil",          { method: "PATCH", body: datos }),
  cambiarPassword: (actual, nueva)         => apiFetch("/api/usuario/password",        { method: "PATCH", body: { actual, nueva } }),
  fotoUploadUrl:   (filename, contentType) => apiFetch("/api/usuario/foto-upload-url", { method: "POST",  body: { filename, contentType } }),
  seguimiento:     ()                      => apiFetch("/api/usuario/seguimiento"),
  marcarHoy:       ()                      => apiFetch("/api/usuario/seguimiento",     { method: "POST" }),
  asignado:        ()                      => apiFetch("/api/usuario/asignado"),
  adminSeguimiento:()                      => apiFetch("/api/usuario/admin/seguimiento"),
};
 
/* ---- Subir foto al backend que la sube a Supabase Storage ---- */
async function subirFotoPerfil(file) {
  const token = getToken();
  const formData = new FormData();
  formData.append("foto", file);
 
  const res = await fetch(API_URL + "/api/usuario/foto", {
    method: "POST",
    headers: { Authorization: "Bearer " + token },
    body: formData,
  });
 
  if (res.status === 401) { cerrarSesion(); return; }
  const data = await res.json();
  if (!res.ok) throw new Error(data.error || "Error al subir la foto");
 
  actualizarUsuarioLocal({ foto_url: data.foto_url });
  return data.foto_url;
}
 
/* ---- Guards ---- */
function requireAuth()  { if (!getToken()) window.location.href = "login.html"; }
function requireGuest() { if (getToken())  window.location.href = "personas.html"; }
/* ---- Service Worker (Offline Support) ---- */
if ("serviceWorker" in navigator) {
  window.addEventListener("load", () => {
    navigator.serviceWorker.register("/sw.js").catch(() => {/* silencioso */});
  });
}

# Grupo de Oración — App con Backend

App web privada para un grupo de oración. Cada miembro se loguea,
ve los motivos de todos y puede anotar cuando oró.

---

## Stack

| Parte       | Tecnología          | Hosting gratuito |
|-------------|---------------------|------------------|
| Frontend    | HTML / CSS / JS     | Vercel           |
| Backend     | Node.js + Express   | Render           |
| Base de datos | PostgreSQL        | Supabase         |
| Emails      | Resend              | Resend (100/día) |

---

## Setup paso a paso

### 1. Supabase (base de datos)

1. Creá una cuenta en https://supabase.com
2. Nuevo proyecto → anotá la **connection string** (Settings > Database > Connection string > URI)
3. Andá a SQL Editor y pegá todo el contenido de `backend/db/schema.sql`
4. Ejecutalo → se crean las tablas y el usuario admin

### 2. Resend (emails)

1. Creá una cuenta en https://resend.com
2. Verificá tu dominio o usá el dominio de prueba que te dan
3. Copiá la API Key

### 3. Backend en Render

1. Subí la carpeta `backend/` a un repo de GitHub
2. En https://render.com → New Web Service → conectá el repo
3. Configuración:
   - **Build Command**: `npm install`
   - **Start Command**: `node server.js`
4. En Environment → agregá estas variables:
   ```
   DATABASE_URL    = (la connection string de Supabase)
   JWT_SECRET      = (generá con: openssl rand -hex 64)
   RESEND_API_KEY  = (tu key de Resend)
   EMAIL_FROM      = oracion@tudominio.com
   FRONTEND_URL    = https://tu-app.vercel.app
   PORT            = 3000
   ```
5. Deploy → anotá la URL que te da Render (ej: `https://grupo-oracion.onrender.com`)

### 4. Frontend en Vercel

1. En `frontend/api.js` cambiá la línea:
   ```js
   const API_URL = "https://tu-backend.onrender.com";
   ```
   por la URL real de tu backend en Render.

2. Subí la carpeta `frontend/` a un repo de GitHub
3. En https://vercel.com → New Project → conectá el repo
4. Deploy directo (sin configuración extra)

### 5. Crear el primer admin

1. Abrí Supabase → Table Editor → tabla `usuarios`
2. Editá el usuario admin que se creó con el schema:
   - Cambiá el email por el tuyo
   - `activo = true`
3. Para setear la contraseña del admin, temporalmente podés usar la API directamente:
   ```
   POST https://tu-backend.onrender.com/api/auth/activar
   Body: { "token": "MANUAL", "password": "tu-contraseña" }
   ```
   O más fácil: desde Supabase SQL Editor:
   ```sql
   -- Instala bcrypt manualmente para el admin
   -- Usá esta herramienta online para generar el hash:
   -- https://bcrypt-generator.com (12 rounds)
   UPDATE usuarios SET password = '$2a$12$HASH_GENERADO', activo = true WHERE rol = 'admin';
   ```

---

## Flujo de uso

### Para el admin:
1. Entrás a la app y te logueás
2. En la pantalla de Miembros, arriba aparece el panel "Invitar miembro"
3. Ponés nombre + email → llega un link por email que expira en 24hs

### Para los miembros:
1. Reciben el email y hacen click en el link
2. Eligen su contraseña → quedan logueados automáticamente
3. Pueden ver los motivos de cada persona (`/perfil.html`)
4. Pueden ver todos los motivos juntos (`/todos.html`)
5. Pueden agregar motivos y marcar cuando oraron

---

## Estructura de archivos

```
grupo-oracion/
├── backend/
│   ├── server.js          # Servidor Express
│   ├── package.json
│   ├── .env.example       # Copiá como .env
│   ├── db/
│   │   ├── pool.js        # Conexión a PostgreSQL
│   │   └── schema.sql     # Tablas — ejecutar en Supabase
│   ├── middleware/
│   │   └── auth.js        # Verificación JWT
│   └── routes/
│       ├── auth.js        # Login, invitación, activación
│       ├── personas.js    # CRUD de miembros del grupo
│       └── motivos.js     # CRUD de motivos + registro de oraciones
│
└── frontend/
    ├── api.js             # Cliente HTTP centralizado ← CAMBIAR LA URL
    ├── style.css          # Estilos globales (el original)
    ├── login.html         # Página de login
    ├── activar.html       # Activación de cuenta (desde email)
    ├── personas.html      # Lista de miembros
    ├── perfil.html        # Motivos de una persona + agregar/editar
    └── todos.html         # Vista grupal de todos los motivos 🆕
```

---

## API endpoints

| Método | Ruta | Descripción |
|--------|------|-------------|
| POST | `/api/auth/login` | Login con email+contraseña |
| POST | `/api/auth/invitar` | (admin) Invitar por email |
| POST | `/api/auth/activar` | Activar cuenta con token del email |
| GET  | `/api/auth/me` | Verificar token activo |
| GET  | `/api/personas` | Listar miembros del grupo |
| POST | `/api/personas` | (admin) Crear miembro |
| GET  | `/api/motivos/todos` | Todos los motivos del grupo |
| GET  | `/api/motivos/:personaId` | Motivos de una persona |
| POST | `/api/motivos` | Crear motivo |
| PATCH| `/api/motivos/:id` | Editar motivo |
| DELETE| `/api/motivos/:id` | Eliminar motivo |
| POST | `/api/motivos/:id/ore` | Registrar que oré |

santi GAYmer y GORDO y GAGA y pollera 
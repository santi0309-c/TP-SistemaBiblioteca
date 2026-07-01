/* ============================================
   DATOS CENTRALIZADOS
   ============================================ */

const personas = [
  "Juan", "María", "Santy", "Lucas", "Valentina",
  "Tomás", "Sofía", "Mateo", "Camila", "Agustín",
  "Martina", "Franco", "Julieta", "Nicolás", "Micaela"
];

const motivos = {
  "Juan": [
    { texto: "Exámenes finales de la facultad", estado: "oracion" },
    { texto: "Salud de su abuelo", estado: "constante" },
    { texto: "Conseguir trabajo de verano", estado: "respondido" }
  ],
  "María": [
    { texto: "Dirección espiritual y toma de decisiones", estado: "oracion" },
    { texto: "Su familia", estado: "constante" }
  ],
  "Santy": [
    { texto: "Proyecto personal que está desarrollando", estado: "activo" },
    { texto: "Crecimiento espiritual", estado: "constante" }
  ],
  "Lucas": [],
  "Valentina": [],
  "Tomás": [],
  "Sofía": [],
  "Mateo": [],
  "Camila": [],
  "Agustín": [],
  "Martina": [],
  "Franco": [],
  "Julieta": [],
  "Nicolás": [],
  "Micaela": []
};

const personasConFoto = [
  { nombre: "Juan", foto: "fotos/juan.png" },
  { nombre: "María", foto: "imagenes/maria.jpg" },
  { nombre: "Santy", foto: "imagenes/santy.jpg" }
  
];

const conMotivos = ["Juan", "María", "Santy"];

function normalizar(s) {
  return s.toLowerCase().trim().normalize("NFD").replace(/[\u0300-\u036f]/g, "");
}

function buscarPersona(nombre) {
  return personas.find(p => normalizar(p) === normalizar(nombre));
}

function obtenerMotivos(nombre) {
  return motivos[nombre] || [];
}

function obtenerFoto(nombre) {
  const p = personasConFoto.find(pp => normalizar(pp.nombre) === normalizar(nombre));
  return p ? p.foto : "";
}

function tieneMotivos(nombre) {
  return conMotivos.includes(nombre);
}

#ifndef DECODE_H
#define DECODE_H

#include "types.h"

/* ============================================================
 *  FORMATO DE INSTRUCCIÓN  (16 bits, la hoja dice "metemos bits
 *  en hexa para la instrucción")
 *
 *  La hoja muestra para ADD:  5 partes + 4 bits
 *
 *  Formato tipo R (add):
 *  [15..12] opcode (4 bits)
 *  [11.. 8] rd     (4 bits) destino
 *  [ 7.. 4] rs1    (4 bits) registro fuente 1
 *  [ 3.. 0] rs2    (4 bits) registro fuente 2 / inmediato bajo
 *
 *  Formato tipo I (lw, sw, beq):
 *  [15..12] opcode (4 bits)
 *  [11.. 8] rd     (4 bits)
 *  [ 7.. 4] rs     (4 bits)
 *  [ 3.. 0] imm    (4 bits) valor / desplazamiento  -- puede ser negativo (beq)
 *
 *  Formato tipo J (j):
 *  [15..12] opcode (4 bits) = 1110
 *  [11.. 0] addr   (12 bits) dirección de salto (unsigned)
 * ============================================================ */

typedef struct {
    uint opcode;  /* 4 bits más altos                        */
    uint rd;      /* registro destino                        */
    uint rs1;     /* registro fuente 1                       */
    uint rs2;     /* registro fuente 2 (tipo R)              */
    uint imm;     /* inmediato crudo (tipo I, sin signo aún) */
    uint addr;    /* dirección de salto (tipo J)             */
    int  imm_s;   /* inmediato con signo extendido (para beq)*/
} Instruccion;

/* Decodifica una instrucción de 16 bits y llena la struct */
Instruccion decode(uint raw);

/* Imprime la instrucción decodificada (para debug) */
void decode_print(const Instruccion *instr);

#endif /* DECODE_H */

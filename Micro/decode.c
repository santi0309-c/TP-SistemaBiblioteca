#include "decode.h"
#include <stdio.h>

/* ============================================================
 *  DECODE
 *
 *  Toma los 16 bits crudos de la instrucción y extrae
 *  cada campo según el formato de la hoja.
 * ============================================================ */

Instruccion decode(uint raw) {
    Instruccion instr;

    /* Los 4 bits más altos son siempre el opcode */
    instr.opcode = (raw >> 12) & 0xF;

    /* Campos tipo R e I (siempre los extraemos, el execute usa lo que necesita) */
    instr.rd  = (raw >> 8) & 0xF;
    instr.rs1 = (raw >> 4) & 0xF;
    instr.rs2 = (raw >> 0) & 0xF;

    /* Inmediato de 4 bits (crudo, sin signo) */
    instr.imm = (raw >> 0) & 0xF;

    /*
     * Extensión de signo para el inmediato de 4 bits.
     * La hoja dice que el valor de salto en beq puede ser negativo.
     * Si el bit 3 está en 1, el número es negativo en complemento a 2.
     *
     * Ejemplo: imm = 1111 (0xF) → -1 en 4 bits con signo
     */
    if (instr.imm & 0x8) {
        /* Extiende el signo a int completo */
        instr.imm_s = (int)(instr.imm | 0xFFFFFFF0);
    } else {
        instr.imm_s = (int)(instr.imm);
    }

    /* Dirección de salto para J: 12 bits bajos (unsigned, como dice la hoja) */
    instr.addr = raw & 0x0FFF;

    return instr;
}

/* ============================================================
 *  DEBUG: imprime la instrucción decodificada
 * ============================================================ */
void decode_print(const Instruccion *instr) {
    printf("  [DECODE] opcode=0x%X  rd=r%u  rs1=r%u  rs2=r%u  imm=%u (signed=%d)  addr=0x%03X\n",
           instr->opcode,
           instr->rd,
           instr->rs1,
           instr->rs2,
           instr->imm,
           instr->imm_s,
           instr->addr);
}

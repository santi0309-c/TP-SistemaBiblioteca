#ifndef EXECUTE_H
#define EXECUTE_H

#include "cpu.h"
#include "decode.h"

/* ============================================================
 *  EJECUCIÓN DE INSTRUCCIONES
 *
 *  La hoja dice: implementamos con while y switch
 *  Cada función recibe el estado completo del CPU y la
 *  instrucción ya decodificada.
 * ============================================================ */

/* ADD: rd = rs1 + rs2  (suma, opcode 0x0) */
void exec_add(CPU *cpu, const Instruccion *instr);

/* LW: rd = mem[rs + imm]  (load word, opcode 0x1)
 *   "descargar algo de la memoria de datos"  -- la hoja */
void exec_lw(CPU *cpu, const Instruccion *instr);

/* SW: mem[rd + imm] = rs  (store word, opcode 0x3)
 *   La hoja pone "3h" para sw */
void exec_sw(CPU *cpu, const Instruccion *instr);

/* BEQ: if (rd == rs) PC += imm_s  (branch if equal, opcode 0x4)
 *   La hoja: "4h rd rs valor_de_salto (negativo)"
 *   El valor de salto es negativo → en forma unsigned */
void exec_beq(CPU *cpu, const Instruccion *instr);

/* J: PC = addr  (jump, opcode 0xE = 1110 en binario)
 *   La hoja: "1110 | en forma unsigned al lugar que quiere saltar" */
void exec_j(CPU *cpu, const Instruccion *instr);

#endif /* EXECUTE_H */

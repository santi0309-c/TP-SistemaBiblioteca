#include <stdio.h>
#include "cpu.h"
#include "types.h"

/*
 * ============================================================
 *  ENSAMBLADOR MANUAL (macros para armar instrucciones)
 *
 *  Formato de 16 bits:
 *    Tipo R (add):   [opcode 4b][rd 4b][rs1 4b][rs2 4b]
 *    Tipo I (lw/sw/beq): [opcode 4b][rd 4b][rs 4b][imm 4b]
 *    Tipo J (j):     [opcode 4b][addr 12b]
 *
 *  Usamos unsigned int, como indica la hoja.
 * ============================================================
 */
#define INSTR_R(op, rd, rs1, rs2) \
    (((uint)(op) << 12) | ((uint)(rd) << 8) | ((uint)(rs1) << 4) | ((uint)(rs2)))

#define INSTR_I(op, rd, rs, imm) \
    (((uint)(op) << 12) | ((uint)(rd) << 8) | ((uint)(rs) << 4) | ((uint)((imm) & 0xF)))

#define INSTR_J(addr) \
    (((uint)(OP_J) << 12) | ((uint)(addr) & 0x0FFF))

/* ============================================================
 *  PROGRAMA DE PRUEBA
 *
 *  Simula un bucle que suma del 1 al 3:
 *
 *    r1 = 1           (simulado cargando desde memoria)
 *    r2 = 0           (acumulador)
 *    r3 = 4           (límite, cargado desde memoria)
 *
 *    bucle:
 *      r2 = r2 + r1   (add)
 *      r1 = r1 + 1    (incremento, usando r4=1)
 *      beq r1, r3 → salir  (si r1 == 4, termina)
 *      j   bucle
 *
 *  Cubre: add, lw, sw, beq, j  — todas las de la hoja.
 * ============================================================ */
int main(void) {
    CPU cpu;
    cpu_init(&cpu);

    /*
     * Precargamos valores en memoria de datos para LW:
     *   mem[0] = 1   (valor inicial de r1)
     *   mem[1] = 4   (límite del bucle)
     *   mem[2] = 1   (incremento constante)
     */
    cpu.data_mem[0] = 1;
    cpu.data_mem[1] = 4;
    cpu.data_mem[2] = 1;

    /*
     * Programa:
     *
     *  Addr  Instrucción                         Descripción
     *  ----  -----------                         -----------
     *  0x00  LW  r1, r0, 0  → r1 = mem[0] = 1  (cargar valor inicial)
     *  0x01  LW  r3, r0, 1  → r3 = mem[1] = 4  (cargar límite)
     *  0x02  LW  r4, r0, 2  → r4 = mem[2] = 1  (cargar incremento)
     *  0x03  ADD r2, r0, r0 → r2 = 0            (inicializar acumulador)
     *
     *  -- bucle (PC=4 en adelante) --
     *  0x04  ADD r2, r2, r1 → r2 += r1          (suma al acumulador)
     *  0x05  ADD r1, r1, r4 → r1++              (incremento)
     *  0x06  BEQ r1, r3, +1 → if r1==r3: skip j (verificar límite)
     *  0x07  J   0x04       → volver al bucle   (j bucle)
     *  0x08  SW  r0, r2, 5  → mem[5] = r2       (guardar resultado)
     *  0x09  0x0000         → HALT
     */
    uint programa[] = {
        /* 0x00 */ INSTR_I(OP_LW,  1, 0, 0),   /* lw r1, r0+0          */
        /* 0x01 */ INSTR_I(OP_LW,  3, 0, 1),   /* lw r3, r0+1          */
        /* 0x02 */ INSTR_I(OP_LW,  4, 0, 2),   /* lw r4, r0+2          */
        /* 0x03 */ INSTR_R(OP_ADD, 2, 0, 0),   /* add r2 = r0 + r0 = 0 */

        /* 0x04 */ INSTR_R(OP_ADD, 2, 2, 1),   /* add r2 = r2 + r1     */
        /* 0x05 */ INSTR_R(OP_ADD, 1, 1, 4),   /* add r1 = r1 + r4     */
        /* 0x06 */ INSTR_I(OP_BEQ, 1, 3, 1),   /* beq r1==r3: salta +1 */
        /* 0x07 */ INSTR_J(0x04),              /* j 0x04 (volver bucle)*/
        /* 0x08 */ INSTR_I(OP_SW,  0, 2, 5),   /* sw mem[r0+5] = r2    */
        /* 0x09 */ 0x0000                       /* HALT                  */
    };

    uint cantidad = sizeof(programa) / sizeof(programa[0]);

    printf("\n=== MICROPROCESADOR EN C ===\n");
    printf("Programa: suma 1+2+3 usando bucle (add, lw, sw, beq, j)\n");
    printf("Resultado esperado en mem[5]: 6\n\n");

    cpu_load_program(&cpu, programa, cantidad);
    cpu_run(&cpu);

    printf("\nVerificacion: mem[5] = %u  (esperado: 6)\n", cpu.data_mem[5]);

    return 0;
}

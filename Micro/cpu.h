#ifndef CPU_H
#define CPU_H

#include "types.h"

/* ============================================================
 *  ESTADO DEL PROCESADOR
 *
 *  La hoja dice:
 *    struct -> PC unsigned int
 *              por estado
 *
 *  Guardamos todo el estado del microprocesador acá.
 * ============================================================ */
typedef struct {
    uint pc;                        /* Program Counter (unsigned int como dice la hoja) */
    uint regs[NUM_REGS];            /* 16 registros de 16 bits (vector de 16 bits)      */
    uint data_mem[DATA_MEM_SIZE];   /* Memoria de datos  (20 bits útiles, sobra 12)     */
    uint prog_mem[PROG_MEM_SIZE];   /* Memoria de programa (instrucciones en hexa)      */
    int  running;                   /* 1 = corriendo, 0 = halt                          */
} CPU;

/* ============================================================
 *  API DEL CPU
 * ============================================================ */

/* Inicializa el CPU: pone PC=0, registros en 0, r0 siempre 0 */
void cpu_init(CPU *cpu);

/* Carga un programa (array de instrucciones) en memoria de programa */
void cpu_load_program(CPU *cpu, uint *instrucciones, uint cantidad);

/* Ejecuta un ciclo (fetch → decode → execute) */
void cpu_step(CPU *cpu);

/* Corre el programa completo hasta halt o PC fuera de rango */
void cpu_run(CPU *cpu);

/* Muestra el estado actual de los registros y PC */
void cpu_dump(const CPU *cpu);

#endif /* CPU_H */

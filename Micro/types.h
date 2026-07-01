#ifndef TYPES_H
#define TYPES_H

#include <stdio.h>
#include <stdlib.h>
#include <string.h>

/* ============================================================
 *  TIPOS BASE
 *  - La hoja dice: data memory = 20 bits en unsigned int (sobra 12)
 *  - Usamos unsigned int para todo, como dice la hoja
 * ============================================================ */

typedef unsigned int uint;

/* Tamaños de memoria */
#define DATA_MEM_SIZE   1024   /* memoria de datos (palabras de 16 bits) */
#define PROG_MEM_SIZE   1024   /* memoria de programa                    */
#define NUM_REGS          16   /* 16 registros (vector de 16 bits)       */

/* ============================================================
 *  OPCODES  (sacados de la hoja, en hexa)
 *
 *  add  -> 0x0   (5 partes + 4 bits de opcode)
 *  lw   -> 0x1   (descargar algo de la memoria de datos)
 *  sw   -> 0x3   (store word, la hoja pone "3h")
 *  beq  -> 0x4   (la hoja pone "4h", rd rs valor_salto negativo)
 *  j    -> 0xE   (la hoja pone "1110" en binario = 14 = 0xE)
 * ============================================================ */
#define OP_ADD  0x0
#define OP_LW   0x1
#define OP_SW   0x3
#define OP_BEQ  0x4
#define OP_J    0xE

/* ============================================================
 *  REGISTROS ESPECIALES  (tabla de la hoja)
 *
 *  r0  = zero (siempre 0)
 *  r1  = (reservado)
 *  r2  = hexa / temporal
 *  r10 = ra  (return address)
 *  r11 = gp  (global pointer)
 *  r12 = sp  (stack pointer)
 *  r13 = fp  (frame pointer)
 *  r14 = a0  (argumento 0)
 *  r15 = a1  (argumento 1)
 * ============================================================ */
#define REG_ZERO  0
#define REG_RA   10
#define REG_GP   11
#define REG_SP   12
#define REG_FP   13
#define REG_A0   14
#define REG_A1   15

#endif /* TYPES_H */

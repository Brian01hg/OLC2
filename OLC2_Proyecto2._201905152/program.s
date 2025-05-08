.data
heap: .space 4096
.text
.global _start
_start:
 adr x10, heap
// Inicio del programa
start:
// === inicio función main ===
main:
// Constant: "Creación con literales:"
STR x10, [SP, #-8]!
// Pushing char 67 to heap - (C)
MOV w0, #67
STRB w0, [x10]
MOV x0, #1
ADD x10, x10, x0
// Pushing char 114 to heap - (r)
MOV w0, #114
STRB w0, [x10]
MOV x0, #1
ADD x10, x10, x0
// Pushing char 101 to heap - (e)
MOV w0, #101
STRB w0, [x10]
MOV x0, #1
ADD x10, x10, x0
// Pushing char 97 to heap - (a)
MOV w0, #97
STRB w0, [x10]
MOV x0, #1
ADD x10, x10, x0
// Pushing char 99 to heap - (c)
MOV w0, #99
STRB w0, [x10]
MOV x0, #1
ADD x10, x10, x0
// Pushing char 105 to heap - (i)
MOV w0, #105
STRB w0, [x10]
MOV x0, #1
ADD x10, x10, x0
// Pushing char 243 to heap - (ó)
MOV w0, #243
STRB w0, [x10]
MOV x0, #1
ADD x10, x10, x0
// Pushing char 110 to heap - (n)
MOV w0, #110
STRB w0, [x10]
MOV x0, #1
ADD x10, x10, x0
// Pushing char 32 to heap - ( )
MOV w0, #32
STRB w0, [x10]
MOV x0, #1
ADD x10, x10, x0
// Pushing char 99 to heap - (c)
MOV w0, #99
STRB w0, [x10]
MOV x0, #1
ADD x10, x10, x0
// Pushing char 111 to heap - (o)
MOV w0, #111
STRB w0, [x10]
MOV x0, #1
ADD x10, x10, x0
// Pushing char 110 to heap - (n)
MOV w0, #110
STRB w0, [x10]
MOV x0, #1
ADD x10, x10, x0
// Pushing char 32 to heap - ( )
MOV w0, #32
STRB w0, [x10]
MOV x0, #1
ADD x10, x10, x0
// Pushing char 108 to heap - (l)
MOV w0, #108
STRB w0, [x10]
MOV x0, #1
ADD x10, x10, x0
// Pushing char 105 to heap - (i)
MOV w0, #105
STRB w0, [x10]
MOV x0, #1
ADD x10, x10, x0
// Pushing char 116 to heap - (t)
MOV w0, #116
STRB w0, [x10]
MOV x0, #1
ADD x10, x10, x0
// Pushing char 101 to heap - (e)
MOV w0, #101
STRB w0, [x10]
MOV x0, #1
ADD x10, x10, x0
// Pushing char 114 to heap - (r)
MOV w0, #114
STRB w0, [x10]
MOV x0, #1
ADD x10, x10, x0
// Pushing char 97 to heap - (a)
MOV w0, #97
STRB w0, [x10]
MOV x0, #1
ADD x10, x10, x0
// Pushing char 108 to heap - (l)
MOV w0, #108
STRB w0, [x10]
MOV x0, #1
ADD x10, x10, x0
// Pushing char 101 to heap - (e)
MOV w0, #101
STRB w0, [x10]
MOV x0, #1
ADD x10, x10, x0
// Pushing char 115 to heap - (s)
MOV w0, #115
STRB w0, [x10]
MOV x0, #1
ADD x10, x10, x0
// Pushing char 58 to heap - (:)
MOV w0, #58
STRB w0, [x10]
MOV x0, #1
ADD x10, x10, x0
// Pushing char 0 to heap - (\0)
MOV w0, #0
STRB w0, [x10]
MOV x0, #1
ADD x10, x10, x0
// Pushing char 0 to heap - (\0)
MOV w0, #0
STRB w0, [x10]
MOV x0, #1
ADD x10, x10, x0
LDR x0, [SP], #8
ADR x10, heap
MOV w0, #120
STRB w0, [x10], #1
MOV w0, #48
STRB w0, [x10], #1
MOV w0, #0
STRB w0, [x10], #1
MOV w0, #0
STRB w0, [x10], #1
MOV X0, x10
BL print_string
STR x10, [SP, #-8]!
// Pushing char 10 to heap - (\n)
MOV w0, #10
STRB w0, [x10]
MOV x0, #1
ADD x10, x10, x0
// Pushing char 0 to heap - (\0)
MOV w0, #0
STRB w0, [x10]
MOV x0, #1
ADD x10, x10, x0
// Pushing char 0 to heap - (\0)
MOV w0, #0
STRB w0, [x10]
MOV x0, #1
ADD x10, x10, x0
LDR x0, [SP], #8
ADR x10, heap
MOV w0, #120
STRB w0, [x10], #1
MOV w0, #48
STRB w0, [x10], #1
MOV w0, #0
STRB w0, [x10], #1
MOV w0, #0
STRB w0, [x10], #1
MOV X0, x10
BL print_string
// Variable declaration: numeros
// Reservar 40 bytes para slice
MOV x9, x10
ADD x10, x10, 40
MOV x0, #1
STR x0, [SP, #-8]!
LDR x0, [SP], #8
STR x0, [x9]
MOV x0, #2
STR x0, [SP, #-8]!
LDR x0, [SP], #8
STR x0, [x9, #8]
MOV x0, #3
STR x0, [SP, #-8]!
LDR x0, [SP], #8
STR x0, [x9, #16]
MOV x0, #4
STR x0, [SP, #-8]!
LDR x0, [SP], #8
STR x0, [x9, #24]
MOV x0, #5
STR x0, [SP, #-8]!
LDR x0, [SP], #8
STR x0, [x9, #32]
MOV x0, x9
STR x0, [SP, #-8]!
LDR x0, [SP], #8
STR x0, [SP, #-8]!
// Constant: "###Validacion Manual"
STR x10, [SP, #-8]!
// Pushing char 35 to heap - (#)
MOV w0, #35
STRB w0, [x10]
MOV x0, #1
ADD x10, x10, x0
// Pushing char 35 to heap - (#)
MOV w0, #35
STRB w0, [x10]
MOV x0, #1
ADD x10, x10, x0
// Pushing char 35 to heap - (#)
MOV w0, #35
STRB w0, [x10]
MOV x0, #1
ADD x10, x10, x0
// Pushing char 86 to heap - (V)
MOV w0, #86
STRB w0, [x10]
MOV x0, #1
ADD x10, x10, x0
// Pushing char 97 to heap - (a)
MOV w0, #97
STRB w0, [x10]
MOV x0, #1
ADD x10, x10, x0
// Pushing char 108 to heap - (l)
MOV w0, #108
STRB w0, [x10]
MOV x0, #1
ADD x10, x10, x0
// Pushing char 105 to heap - (i)
MOV w0, #105
STRB w0, [x10]
MOV x0, #1
ADD x10, x10, x0
// Pushing char 100 to heap - (d)
MOV w0, #100
STRB w0, [x10]
MOV x0, #1
ADD x10, x10, x0
// Pushing char 97 to heap - (a)
MOV w0, #97
STRB w0, [x10]
MOV x0, #1
ADD x10, x10, x0
// Pushing char 99 to heap - (c)
MOV w0, #99
STRB w0, [x10]
MOV x0, #1
ADD x10, x10, x0
// Pushing char 105 to heap - (i)
MOV w0, #105
STRB w0, [x10]
MOV x0, #1
ADD x10, x10, x0
// Pushing char 111 to heap - (o)
MOV w0, #111
STRB w0, [x10]
MOV x0, #1
ADD x10, x10, x0
// Pushing char 110 to heap - (n)
MOV w0, #110
STRB w0, [x10]
MOV x0, #1
ADD x10, x10, x0
// Pushing char 32 to heap - ( )
MOV w0, #32
STRB w0, [x10]
MOV x0, #1
ADD x10, x10, x0
// Pushing char 77 to heap - (M)
MOV w0, #77
STRB w0, [x10]
MOV x0, #1
ADD x10, x10, x0
// Pushing char 97 to heap - (a)
MOV w0, #97
STRB w0, [x10]
MOV x0, #1
ADD x10, x10, x0
// Pushing char 110 to heap - (n)
MOV w0, #110
STRB w0, [x10]
MOV x0, #1
ADD x10, x10, x0
// Pushing char 117 to heap - (u)
MOV w0, #117
STRB w0, [x10]
MOV x0, #1
ADD x10, x10, x0
// Pushing char 97 to heap - (a)
MOV w0, #97
STRB w0, [x10]
MOV x0, #1
ADD x10, x10, x0
// Pushing char 108 to heap - (l)
MOV w0, #108
STRB w0, [x10]
MOV x0, #1
ADD x10, x10, x0
// Pushing char 0 to heap - (\0)
MOV w0, #0
STRB w0, [x10]
MOV x0, #1
ADD x10, x10, x0
// Pushing char 0 to heap - (\0)
MOV w0, #0
STRB w0, [x10]
MOV x0, #1
ADD x10, x10, x0
LDR x0, [SP], #8
ADR x10, heap
MOV w0, #120
STRB w0, [x10], #1
MOV w0, #48
STRB w0, [x10], #1
MOV w0, #0
STRB w0, [x10], #1
MOV w0, #0
STRB w0, [x10], #1
MOV X0, x10
BL print_string
STR x10, [SP, #-8]!
// Pushing char 10 to heap - (\n)
MOV w0, #10
STRB w0, [x10]
MOV x0, #1
ADD x10, x10, x0
// Pushing char 0 to heap - (\0)
MOV w0, #0
STRB w0, [x10]
MOV x0, #1
ADD x10, x10, x0
// Pushing char 0 to heap - (\0)
MOV w0, #0
STRB w0, [x10]
MOV x0, #1
ADD x10, x10, x0
LDR x0, [SP], #8
ADR x10, heap
MOV w0, #120
STRB w0, [x10], #1
MOV w0, #48
STRB w0, [x10], #1
MOV w0, #0
STRB w0, [x10], #1
MOV w0, #0
STRB w0, [x10], #1
MOV X0, x10
BL print_string
// Constant: "numeros:"
STR x10, [SP, #-8]!
// Pushing char 110 to heap - (n)
MOV w0, #110
STRB w0, [x10]
MOV x0, #1
ADD x10, x10, x0
// Pushing char 117 to heap - (u)
MOV w0, #117
STRB w0, [x10]
MOV x0, #1
ADD x10, x10, x0
// Pushing char 109 to heap - (m)
MOV w0, #109
STRB w0, [x10]
MOV x0, #1
ADD x10, x10, x0
// Pushing char 101 to heap - (e)
MOV w0, #101
STRB w0, [x10]
MOV x0, #1
ADD x10, x10, x0
// Pushing char 114 to heap - (r)
MOV w0, #114
STRB w0, [x10]
MOV x0, #1
ADD x10, x10, x0
// Pushing char 111 to heap - (o)
MOV w0, #111
STRB w0, [x10]
MOV x0, #1
ADD x10, x10, x0
// Pushing char 115 to heap - (s)
MOV w0, #115
STRB w0, [x10]
MOV x0, #1
ADD x10, x10, x0
// Pushing char 58 to heap - (:)
MOV w0, #58
STRB w0, [x10]
MOV x0, #1
ADD x10, x10, x0
// Pushing char 0 to heap - (\0)
MOV w0, #0
STRB w0, [x10]
MOV x0, #1
ADD x10, x10, x0
// Pushing char 0 to heap - (\0)
MOV w0, #0
STRB w0, [x10]
MOV x0, #1
ADD x10, x10, x0
LDR x0, [SP], #8
ADR x10, heap
MOV w0, #120
STRB w0, [x10], #1
MOV w0, #48
STRB w0, [x10], #1
MOV w0, #0
STRB w0, [x10], #1
MOV w0, #0
STRB w0, [x10], #1
MOV X0, x10
BL print_string
STR x10, [SP, #-8]!
// Pushing char 32 to heap - ( )
MOV w0, #32
STRB w0, [x10]
MOV x0, #1
ADD x10, x10, x0
// Pushing char 0 to heap - (\0)
MOV w0, #0
STRB w0, [x10]
MOV x0, #1
ADD x10, x10, x0
// Pushing char 0 to heap - (\0)
MOV w0, #0
STRB w0, [x10]
MOV x0, #1
ADD x10, x10, x0
LDR x0, [SP], #8
ADR x10, heap
MOV w0, #120
STRB w0, [x10], #1
MOV w0, #48
STRB w0, [x10], #1
MOV w0, #0
STRB w0, [x10], #1
MOV w0, #0
STRB w0, [x10], #1
MOV X0, x10
BL print_string
// Reading variable: numeros
MOV x1, #0
ADD x1, sp, x1
LDR x0, [x1, #0]
STR x0, [SP, #-8]!
LDR x0, [SP], #8
MOV x11, x0
STR x10, [SP, #-8]!
// Pushing char 91 to heap - ([)
MOV w0, #91
STRB w0, [x10]
MOV x0, #1
ADD x10, x10, x0
// Pushing char 0 to heap - (\0)
MOV w0, #0
STRB w0, [x10]
MOV x0, #1
ADD x10, x10, x0
// Pushing char 0 to heap - (\0)
MOV w0, #0
STRB w0, [x10]
MOV x0, #1
ADD x10, x10, x0
LDR x1, [SP], #8
ADR x10, heap
MOV w0, #120
STRB w0, [x10], #1
MOV w0, #49
STRB w0, [x10], #1
MOV w0, #0
STRB w0, [x10], #1
MOV w0, #0
STRB w0, [x10], #1
MOV X0, x10
BL print_string
LDR x2, [x11, #0]
MOV x0, x2
BL print_integer
STR x10, [SP, #-8]!
// Pushing char 32 to heap - ( )
MOV w0, #32
STRB w0, [x10]
MOV x0, #1
ADD x10, x10, x0
// Pushing char 0 to heap - (\0)
MOV w0, #0
STRB w0, [x10]
MOV x0, #1
ADD x10, x10, x0
// Pushing char 0 to heap - (\0)
MOV w0, #0
STRB w0, [x10]
MOV x0, #1
ADD x10, x10, x0
LDR x2, [SP], #8
ADR x10, heap
MOV w0, #120
STRB w0, [x10], #1
MOV w0, #50
STRB w0, [x10], #1
MOV w0, #0
STRB w0, [x10], #1
MOV w0, #0
STRB w0, [x10], #1
MOV X0, x10
BL print_string
LDR x2, [x11, #8]
MOV x0, x2
BL print_integer
STR x10, [SP, #-8]!
// Pushing char 32 to heap - ( )
MOV w0, #32
STRB w0, [x10]
MOV x0, #1
ADD x10, x10, x0
// Pushing char 0 to heap - (\0)
MOV w0, #0
STRB w0, [x10]
MOV x0, #1
ADD x10, x10, x0
// Pushing char 0 to heap - (\0)
MOV w0, #0
STRB w0, [x10]
MOV x0, #1
ADD x10, x10, x0
LDR x2, [SP], #8
ADR x10, heap
MOV w0, #120
STRB w0, [x10], #1
MOV w0, #50
STRB w0, [x10], #1
MOV w0, #0
STRB w0, [x10], #1
MOV w0, #0
STRB w0, [x10], #1
MOV X0, x10
BL print_string
LDR x2, [x11, #16]
MOV x0, x2
BL print_integer
STR x10, [SP, #-8]!
// Pushing char 32 to heap - ( )
MOV w0, #32
STRB w0, [x10]
MOV x0, #1
ADD x10, x10, x0
// Pushing char 0 to heap - (\0)
MOV w0, #0
STRB w0, [x10]
MOV x0, #1
ADD x10, x10, x0
// Pushing char 0 to heap - (\0)
MOV w0, #0
STRB w0, [x10]
MOV x0, #1
ADD x10, x10, x0
LDR x2, [SP], #8
ADR x10, heap
MOV w0, #120
STRB w0, [x10], #1
MOV w0, #50
STRB w0, [x10], #1
MOV w0, #0
STRB w0, [x10], #1
MOV w0, #0
STRB w0, [x10], #1
MOV X0, x10
BL print_string
LDR x2, [x11, #24]
MOV x0, x2
BL print_integer
STR x10, [SP, #-8]!
// Pushing char 32 to heap - ( )
MOV w0, #32
STRB w0, [x10]
MOV x0, #1
ADD x10, x10, x0
// Pushing char 0 to heap - (\0)
MOV w0, #0
STRB w0, [x10]
MOV x0, #1
ADD x10, x10, x0
// Pushing char 0 to heap - (\0)
MOV w0, #0
STRB w0, [x10]
MOV x0, #1
ADD x10, x10, x0
LDR x2, [SP], #8
ADR x10, heap
MOV w0, #120
STRB w0, [x10], #1
MOV w0, #50
STRB w0, [x10], #1
MOV w0, #0
STRB w0, [x10], #1
MOV w0, #0
STRB w0, [x10], #1
MOV X0, x10
BL print_string
LDR x2, [x11, #32]
MOV x0, x2
BL print_integer
STR x10, [SP, #-8]!
// Pushing char 93 to heap - (])
MOV w0, #93
STRB w0, [x10]
MOV x0, #1
ADD x10, x10, x0
// Pushing char 0 to heap - (\0)
MOV w0, #0
STRB w0, [x10]
MOV x0, #1
ADD x10, x10, x0
// Pushing char 0 to heap - (\0)
MOV w0, #0
STRB w0, [x10]
MOV x0, #1
ADD x10, x10, x0
LDR x1, [SP], #8
ADR x10, heap
MOV w0, #120
STRB w0, [x10], #1
MOV w0, #49
STRB w0, [x10], #1
MOV w0, #0
STRB w0, [x10], #1
MOV w0, #0
STRB w0, [x10], #1
MOV X0, x10
BL print_string
STR x10, [SP, #-8]!
// Pushing char 10 to heap - (\n)
MOV w0, #10
STRB w0, [x10]
MOV x0, #1
ADD x10, x10, x0
// Pushing char 0 to heap - (\0)
MOV w0, #0
STRB w0, [x10]
MOV x0, #1
ADD x10, x10, x0
// Pushing char 0 to heap - (\0)
MOV w0, #0
STRB w0, [x10]
MOV x0, #1
ADD x10, x10, x0
LDR x0, [SP], #8
ADR x10, heap
MOV w0, #120
STRB w0, [x10], #1
MOV w0, #48
STRB w0, [x10], #1
MOV w0, #0
STRB w0, [x10], #1
MOV w0, #0
STRB w0, [x10], #1
MOV X0, x10
BL print_string
// Constant: "OK Creación con literales: correcto"
STR x10, [SP, #-8]!
// Pushing char 79 to heap - (O)
MOV w0, #79
STRB w0, [x10]
MOV x0, #1
ADD x10, x10, x0
// Pushing char 75 to heap - (K)
MOV w0, #75
STRB w0, [x10]
MOV x0, #1
ADD x10, x10, x0
// Pushing char 32 to heap - ( )
MOV w0, #32
STRB w0, [x10]
MOV x0, #1
ADD x10, x10, x0
// Pushing char 67 to heap - (C)
MOV w0, #67
STRB w0, [x10]
MOV x0, #1
ADD x10, x10, x0
// Pushing char 114 to heap - (r)
MOV w0, #114
STRB w0, [x10]
MOV x0, #1
ADD x10, x10, x0
// Pushing char 101 to heap - (e)
MOV w0, #101
STRB w0, [x10]
MOV x0, #1
ADD x10, x10, x0
// Pushing char 97 to heap - (a)
MOV w0, #97
STRB w0, [x10]
MOV x0, #1
ADD x10, x10, x0
// Pushing char 99 to heap - (c)
MOV w0, #99
STRB w0, [x10]
MOV x0, #1
ADD x10, x10, x0
// Pushing char 105 to heap - (i)
MOV w0, #105
STRB w0, [x10]
MOV x0, #1
ADD x10, x10, x0
// Pushing char 243 to heap - (ó)
MOV w0, #243
STRB w0, [x10]
MOV x0, #1
ADD x10, x10, x0
// Pushing char 110 to heap - (n)
MOV w0, #110
STRB w0, [x10]
MOV x0, #1
ADD x10, x10, x0
// Pushing char 32 to heap - ( )
MOV w0, #32
STRB w0, [x10]
MOV x0, #1
ADD x10, x10, x0
// Pushing char 99 to heap - (c)
MOV w0, #99
STRB w0, [x10]
MOV x0, #1
ADD x10, x10, x0
// Pushing char 111 to heap - (o)
MOV w0, #111
STRB w0, [x10]
MOV x0, #1
ADD x10, x10, x0
// Pushing char 110 to heap - (n)
MOV w0, #110
STRB w0, [x10]
MOV x0, #1
ADD x10, x10, x0
// Pushing char 32 to heap - ( )
MOV w0, #32
STRB w0, [x10]
MOV x0, #1
ADD x10, x10, x0
// Pushing char 108 to heap - (l)
MOV w0, #108
STRB w0, [x10]
MOV x0, #1
ADD x10, x10, x0
// Pushing char 105 to heap - (i)
MOV w0, #105
STRB w0, [x10]
MOV x0, #1
ADD x10, x10, x0
// Pushing char 116 to heap - (t)
MOV w0, #116
STRB w0, [x10]
MOV x0, #1
ADD x10, x10, x0
// Pushing char 101 to heap - (e)
MOV w0, #101
STRB w0, [x10]
MOV x0, #1
ADD x10, x10, x0
// Pushing char 114 to heap - (r)
MOV w0, #114
STRB w0, [x10]
MOV x0, #1
ADD x10, x10, x0
// Pushing char 97 to heap - (a)
MOV w0, #97
STRB w0, [x10]
MOV x0, #1
ADD x10, x10, x0
// Pushing char 108 to heap - (l)
MOV w0, #108
STRB w0, [x10]
MOV x0, #1
ADD x10, x10, x0
// Pushing char 101 to heap - (e)
MOV w0, #101
STRB w0, [x10]
MOV x0, #1
ADD x10, x10, x0
// Pushing char 115 to heap - (s)
MOV w0, #115
STRB w0, [x10]
MOV x0, #1
ADD x10, x10, x0
// Pushing char 58 to heap - (:)
MOV w0, #58
STRB w0, [x10]
MOV x0, #1
ADD x10, x10, x0
// Pushing char 32 to heap - ( )
MOV w0, #32
STRB w0, [x10]
MOV x0, #1
ADD x10, x10, x0
// Pushing char 99 to heap - (c)
MOV w0, #99
STRB w0, [x10]
MOV x0, #1
ADD x10, x10, x0
// Pushing char 111 to heap - (o)
MOV w0, #111
STRB w0, [x10]
MOV x0, #1
ADD x10, x10, x0
// Pushing char 114 to heap - (r)
MOV w0, #114
STRB w0, [x10]
MOV x0, #1
ADD x10, x10, x0
// Pushing char 114 to heap - (r)
MOV w0, #114
STRB w0, [x10]
MOV x0, #1
ADD x10, x10, x0
// Pushing char 101 to heap - (e)
MOV w0, #101
STRB w0, [x10]
MOV x0, #1
ADD x10, x10, x0
// Pushing char 99 to heap - (c)
MOV w0, #99
STRB w0, [x10]
MOV x0, #1
ADD x10, x10, x0
// Pushing char 116 to heap - (t)
MOV w0, #116
STRB w0, [x10]
MOV x0, #1
ADD x10, x10, x0
// Pushing char 111 to heap - (o)
MOV w0, #111
STRB w0, [x10]
MOV x0, #1
ADD x10, x10, x0
// Pushing char 0 to heap - (\0)
MOV w0, #0
STRB w0, [x10]
MOV x0, #1
ADD x10, x10, x0
// Pushing char 0 to heap - (\0)
MOV w0, #0
STRB w0, [x10]
MOV x0, #1
ADD x10, x10, x0
LDR x0, [SP], #8
ADR x10, heap
MOV w0, #120
STRB w0, [x10], #1
MOV w0, #48
STRB w0, [x10], #1
MOV w0, #0
STRB w0, [x10], #1
MOV w0, #0
STRB w0, [x10], #1
MOV X0, x10
BL print_string
STR x10, [SP, #-8]!
// Pushing char 10 to heap - (\n)
MOV w0, #10
STRB w0, [x10]
MOV x0, #1
ADD x10, x10, x0
// Pushing char 0 to heap - (\0)
MOV w0, #0
STRB w0, [x10]
MOV x0, #1
ADD x10, x10, x0
// Pushing char 0 to heap - (\0)
MOV w0, #0
STRB w0, [x10]
MOV x0, #1
ADD x10, x10, x0
LDR x0, [SP], #8
ADR x10, heap
MOV w0, #120
STRB w0, [x10], #1
MOV w0, #48
STRB w0, [x10], #1
MOV w0, #0
STRB w0, [x10], #1
MOV w0, #0
STRB w0, [x10], #1
MOV X0, x10
BL print_string
// === fin función main ===
RET
// Llamar a main()
BL main
// Salir del programa
MOV x0, #0
MOV x8, #93
SVC 0
.data
minus_sign: .ascii "-"
.text

//--------------------------------------------------------------
// print_string - Prints a null-terminated string to stdout
//
// Input:
//   x0 - The address of the null-terminated string to print
//--------------------------------------------------------------
print_string:
    // Save link register and other registers we'll use
    stp     x29, x30, [sp, #-16]!
    stp     x19, x20, [sp, #-16]!
    
    // x19 will hold the string address
    mov     x19, x0
    
print_loop:
    // Load a byte from the string
    ldrb    w20, [x19]
    
    // Check if it's the null terminator (0)
    cbz     w20, print_done
    
    // Prepare for write syscall
    mov     x0, #1              // File descriptor: 1 for stdout
    mov     x1, x19             // Address of the character to print
    mov     x2, #1              // Length: 1 byte
    mov     x8, #64             // syscall: write (64 on ARM64)
    svc     #0                  // Make the syscall
    
    // Move to the next character
    add     x19, x19, #1
    
    // Continue the loop
    b       print_loop
    
print_done:
    // Restore saved registers
    ldp     x19, x20, [sp], #16
    ldp     x29, x30, [sp], #16
    ret
    // Return to the caller
    

//--------------------------------------------------------------
// print_integer - Prints a signed integer to stdout
//
// Input:
//   x0 - The integer value to print
//--------------------------------------------------------------
print_integer:
    // Save registers
    stp x29, x30, [sp, #-16]!  // Save frame pointer and link register
    stp x19, x20, [sp, #-16]!  // Save callee-saved registers
    stp x21, x22, [sp, #-16]!
    stp x23, x24, [sp, #-16]!
    stp x25, x26, [sp, #-16]!
    stp x27, x28, [sp, #-16]!
    
    // Check if number is negative
    mov x19, x0                // Save original number
    cmp x19, #0                // Compare with zero
    bge positive_number        // Branch if greater or equal to zero
    
    // Handle negative number
    mov x0, #1                 // fd = 1 (stdout)
    adr x1, minus_sign         // Address of minus sign
    mov x2, #1                 // Length = 1
    mov w8, #64                // Syscall write
    svc #0
    
    neg x19, x19               // Make number positive
    
positive_number:
    // Prepare buffer for converting result to ASCII
    sub sp, sp, #32            // Reserve space on stack
    mov x22, sp                // x22 points to buffer
    
    // Initialize digit counter
    mov x23, #0                // Digit counter
    
    // Handle special case for zero
    cmp x19, #0
    bne convert_loop
    
    // If number is zero, just write '0'
    mov w24, #48               // ASCII '0'
    strb w24, [x22, x23]       // Store in buffer
    add x23, x23, #1           // Increment counter
    b print_result             // Skip conversion loop
    
convert_loop:
    // Divide the number by 10
    mov x24, #10
    udiv x25, x19, x24         // x25 = x19 / 10 (quotient)
    msub x26, x25, x24, x19    // x26 = x19 - (x25 * 10) (remainder)
    
    // Convert remainder to ASCII and store in buffer
    add x26, x26, #48          // Convert to ASCII ('0' = 48)
    strb w26, [x22, x23]       // Store digit in buffer
    add x23, x23, #1           // Increment digit counter
    
    // Prepare for next iteration
    mov x19, x25               // Quotient becomes the new number
    cbnz x19, convert_loop     // If number is not zero, continue
    
    // Reverse the buffer since digits are in reverse order
    mov x27, #0                // Start index
reverse_loop:
    sub x28, x23, x27          // x28 = length - current index
    sub x28, x28, #1           // x28 = length - current index - 1
    
    cmp x27, x28               // Compare indices
    bge print_result           // If crossed, finish reversing
    
    // Swap characters
    ldrb w24, [x22, x27]       // Load character from start
    ldrb w25, [x22, x28]       // Load character from end
    strb w25, [x22, x27]       // Store end character at start
    strb w24, [x22, x28]       // Store start character at end
    
    add x27, x27, #1           // Increment start index
    b reverse_loop             // Continue reversing
    
print_result:
    // Print the result
    mov x0, #1                 // fd = 1 (stdout)
    mov x1, x22                // Buffer address
    mov x2, x23                // Buffer length
    mov w8, #64                // Syscall write
    svc #0
    
    // Clean up and restore registers
    add sp, sp, #32            // Free buffer space
    ldp x27, x28, [sp], #16    // Restore callee-saved registers
    ldp x25, x26, [sp], #16
    ldp x23, x24, [sp], #16
    ldp x21, x22, [sp], #16
    ldp x19, x20, [sp], #16
    ldp x29, x30, [sp], #16    // Restore frame pointer and link register
    ret                        // Return to caller
    


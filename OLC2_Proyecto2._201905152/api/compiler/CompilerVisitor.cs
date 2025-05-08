using System.Runtime.InteropServices;
using System.Windows.Markup;
using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Tree;
using analyzer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using System.Text;
using System.Collections.Generic;
namespace api.Controllers;




public class CompileVisitor : LanguageBaseVisitor<Object?>
{

    public armGenerator c = new armGenerator();

    private string? continueLabel = null;

    private string? breakLabel = null;

    private string? returnLabel = null;

    //private Dictionary<string, FunctionMetadata> functions = new Dictionary<string, FunctionMetadata>();

    private string? insideFucntion = null;

    private int framePointerOffset = 0; 



    public CompileVisitor()
    {

    }

    
public override Object? VisitProgram(LanguageParser.ProgramContext context)
{
    c.Comment("Inicio del programa");
    c.Label("start");    // alias _start en el ensamblado
    // aquí podrías necesitar: c.Label("_start"); según tu convención

    // Genera primero todas las funcDcl y funcMain
    foreach (var d in context.dcl())
        Visit(d);

    // Luego, invoca a main y sal
    c.Comment("Llamar a main()");
    c.Bl("main");        // llama a la etiqueta main
    c.Comment("Salir del programa");
    c.EndProgram();      // único syscall exit
    return null;
}


public override Object? VisitFuncDcl(LanguageParser.FuncDclContext context)
{
    var funcName = context.ID().GetText();
    var funcLabel = $"func_{funcName}";
    c.Comment($"--- Inicio de función {funcName} ---");
    c.Label(funcLabel);

    var prevReturnLabel = returnLabel;
    returnLabel = c.GetLabel();

    c.NewScope();

    // Procesar el cuerpo de la función
    foreach (var stmt in context.dcl())
    {
        Visit(stmt);
    }

    // Punto de retorno
    c.SetLabel(returnLabel);
    c.Comment($"--- Fin de función {funcName} ---");
    c.Ret();

    // Limpiar stack si se usó memoria local
    var bytesToRemove = c.endScope();
    if (bytesToRemove > 0)
    {
        c.Comment($"Liberando {bytesToRemove} bytes del stack");
        c.Mov(Register.X0, bytesToRemove);
        c.Add(Register.SP, Register.SP, Register.X0);
    }

    returnLabel = prevReturnLabel;
    return null;
}

/*
public override Object? VisitFuncDcl(LanguageParser.FuncDclContext context)
{

    int baseOffset = 2;
    int paramsOffset= 0;

    if (context.@params() != null)
    {
        paramsOffset = context.@params.param.Length;

    }

    FrameVisitor frameVisitor = new FrameVisitor(baseOffset + paramsOffset);

    foreach (var dcl in context.dcl());
    {
        frameVisitor.Visit(dcl);

    }

    var frame = frameVisitor.Frame;
    int localOffset = frame.Count;
    inr returnOffset = 1;

    int totalFrameSize = baseOffset + paramsOffset + localOffset + returnOffset;
    string funcName = context.ID(0).GetText();
    StackObject.StackObjectType funcType = GetType(context.ID(1).GetText());


    functions.Add(funcName, new FunctionMetadata{
        FrameSize = totalFrameSize,
        ReturnType = funcType
    });

    var prevInstructions = c.instructions;
    c.instructions = new List<string>();

    var paramCounter = 0;

    
}
*/




public override Object? VisitFuncMain(LanguageParser.FuncMainContext context)
{
    c.Comment("=== inicio función main ===");
    c.Label("main");
    c.NewScope();    // abre scope local

    // cuerpo de main
    foreach (var d in context.dcl())
    {
        Visit(d);
    }

    // epílogo igual que en VisitFuncDcl:
    int bytesToRemove = c.endScope();
    if (bytesToRemove > 0)
    {
        c.Comment($"Liberar {bytesToRemove} bytes del stack");
        c.Mov(Register.X0, bytesToRemove);
        c.Add(Register.SP, Register.SP, Register.X0);
    }

    c.Comment("=== fin función main ===");
    c.Ret();        // solo RET, no syscall
    return null;
}




public override Object? VisitRune(LanguageParser.RuneContext context)
{
    var value = context.GetText(); // 'A'
    char rune = value.Trim('\'')[0]; // Extrae el caracter
    c.Comment("Constant rune: " + rune);

    var strObj = c.StringObject();
    c.PushConstant(strObj, rune.ToString());
    return null;
}


public override Object? VisitVarDcl(LanguageParser.VarDclContext context)
{
    var varName = context.ID().GetText();
    c.Comment("Variable declaration: " + varName);

    StackObject obj;

    if (context.Tipo() != null && context.expr() != null)
    {
        Visit(context.expr());
        var value = c.PopObject(Register.X0);
        obj = c.CloneObject(value);
        obj.Id = varName;

        if (obj.Type == StackObject.StackObjectType.String)
        {
            c.Push(Register.HP);
        }
        else if (obj.Type == StackObject.StackObjectType.Float)
        {
            c.Push(Register.D0);
        }
        else
        {
            c.Push(Register.X0);
        }

        c.PushObject(obj);
        return null;
    }

    if (context.Tipo() != null && context.expr() == null)
    {
        var tipo = context.Tipo().GetText();
        obj = tipo switch
        {
            "int" => c.IntObject(),
            "float64" => c.FloatObject(),
            "string" => c.StringObject(),
            "bool" => c.BoolObject(),
            "rune" => c.IntObject(),
            _ => throw new Exception("Tipo desconocido")
        };
        obj.Id = varName;

        c.Comment("Default value for type: " + tipo);

        switch (tipo)
        {
            case "string":
                c.Push(Register.HP);
                break;
            case "int":
            case "bool":
            case "rune":
                c.Mov(Register.X0, 0);
                c.Push(Register.X0);
                break;
            case "float64":
                c.Mov("x0", 0);
                c.Str("x0", Register.SP);
                c.Push("x0");
                break;
        }

        c.PushObject(obj);
        return null;
    }

    if (context.expr() != null && context.Tipo() == null)
    {
        Visit(context.expr());
        var value = c.PopObject(Register.X0);
        obj = c.CloneObject(value);
        obj.Id = varName;

        if (obj.Type == StackObject.StackObjectType.String)
        {
            c.Push(Register.HP);
        }
        else if (obj.Type == StackObject.StackObjectType.Float)
        {
            c.Push(Register.D0);
        }
        else
        {
            c.Push(Register.X0);
        }

        c.PushObject(obj);
        return null;
    }

    return null;
}

// public override Object? VisitPrintStmt(LanguageParser.PrintStmtContext context)
// {
//     var list = context.exprList();
//     if (list != null)
//     {
//         bool first = true;
//         foreach (var expr in list.expr())
//         {
//             if (!first)
//             {
//                 var spaceObj = c.StringObject();
//                 c.PushConstant(spaceObj, " ");
//                 c.PopObject(Register.X0);
//                 c.PrintString(Register.X0);
//             }
//             first = false;

//             Visit(expr);
//             var top = c.TopObject();

//             switch (top.Type)
//             {
//                 case StackObject.StackObjectType.Int:
//                     c.PopObject(Register.X0);
//                     c.PrintInteger(Register.X0);
//                     break;

//                 case StackObject.StackObjectType.Float:
//                     c.PopObject(Register.D0);
//                     c.PrintFloat();
//                     break;
//                     case StackObject.StackObjectType.Bool:
//                 {
//                     c.PopObject(Register.X0);
//                     var lbTrue  = c.GetLabel();
//                     var lbEnd   = c.GetLabel();

//                     // Si inner != 0 (true), salta a lbTrue
//                     c.Cbnz(Register.X0, lbTrue);

//                     // false
//                     var sFalse = c.StringObject();
//                     c.PushConstant(sFalse, "false\n");
//                     c.PopObject(Register.X0);
//                     c.PrintString(Register.X0);
//                     c.B(lbEnd);

//                     // true
//                     c.SetLabel(lbTrue);
//                     var sTrue = c.StringObject();
//                     c.PushConstant(sTrue, "true\n");
//                     c.PopObject(Register.X0);
//                     c.PrintString(Register.X0);

//                     c.SetLabel(lbEnd);
//                     break;
//                 }

//                 case StackObject.StackObjectType.String:
//                     c.PopObject(Register.X0);
//                     c.PrintString(Register.X0);
//                     break;

//                 case StackObject.StackObjectType.Pointer:
//                     // Aquí iteras sobre los elementos del slice
//                     var slice = c.PopObject(Register.X0);   // X0 = base
//                     int len   = slice.Length;

//                     c.Mov("x11", Register.X0);             // x11 = ptr de trabajo

//                     // Imprimir el slice
//                     var lb = c.StringObject();
//                     c.PushConstant(lb, "[");
//                     c.PopObject(Register.X1);
//                     c.PrintString(Register.X1);

//                     for (int i = 0; i < len; i++)
//                     {
//                         if (i > 0) {
//                             var sp = c.StringObject();
//                             c.PushConstant(sp, " ");
//                             c.PopObject(Register.X2);
//                             c.PrintString(Register.X2);
//                         }

//                         c.Ldr(Register.X2, "x11");          // X2 = *x11 (el valor del slice en X11)
//                         c.PrintInteger(Register.X2);
//                         c.Add("x11", "x11", 8);             // x11 += 8 (paso al siguiente elemento del slice)
//                     }

//                     // Cerrar la impresión
//                     var rb = c.StringObject();
//                     c.PushConstant(rb, "]");
//                     c.PopObject(Register.X1);
//                     c.PrintString(Register.X1);
//                     break;
//             }
//         }
//     }

//     // salto de línea al final
//     var newlineObj = c.StringObject();
//     c.PushConstant(newlineObj, "\n");
//     c.PopObject(Register.X0);
//     c.PrintString(Register.X0);

//     return null;
// }


public override Object? VisitPrintStmt(LanguageParser.PrintStmtContext context)
{
    var list = context.exprList();
    if (list != null)
    {
        bool first = true;
        foreach (var expr in list.expr())
        {
            if (!first)
            {
                var spaceObj = c.StringObject();
                c.PushConstant(spaceObj, " ");
                c.PopObject(Register.X0);
                c.PrintString(Register.X0);
            }
            first = false;

            Visit(expr);
            var top = c.TopObject();

            switch (top.Type)
            {
                case StackObject.StackObjectType.Int:
                case StackObject.StackObjectType.Bool:
                    c.PopObject(Register.X0);
                    c.PrintInteger(Register.X0);
                    break;

                case StackObject.StackObjectType.Float:
                    c.PopObject(Register.D0);
                    c.PrintFloat();
                    break;

                case StackObject.StackObjectType.String:
                    c.PopObject(Register.X0);
                    c.PrintString(Register.X0);
                    break;

                case StackObject.StackObjectType.Pointer:
                {
                    var slice = c.PopObject(Register.X0);   // X0 = base
                    int len = slice.Length;

                    // Guardar la base en x11
                    c.Mov("x11", Register.X0);             

                    // Imprimir "["
                    var lb = c.StringObject();
                    c.PushConstant(lb, "[");
                    c.PopObject(Register.X1);
                    c.PrintString(Register.X1);

                    // Recorrer los elementos del slice
                    for (int i = 0; i < len; i++)
                    {
                        // Espacio entre elementos (excepto el primero)
                        if (i > 0) {
                            var sp = c.StringObject();
                            c.PushConstant(sp, " ");
                            c.PopObject(Register.X2);
                            c.PrintString(Register.X2);
                        }

                        // Cargar el elemento con el índice correcto
                        c.Ldr(Register.X2, "x11", i * 8);   // x2 = slice[i]
                        c.PrintInteger(Register.X2);
                    }

                    // Imprimir "]"
                    var rb = c.StringObject();
                    c.PushConstant(rb, "]");
                    c.PopObject(Register.X1);
                    c.PrintString(Register.X1);
                    break;
                }
            }
        }
    }

    // Salto de línea al final
    var newlineObj = c.StringObject();
    c.PushConstant(newlineObj, "\n");
    c.PopObject(Register.X0);
    c.PrintString(Register.X0);

    return null;
}
public override Object? VisitIdentifier(LanguageParser.IdentifierContext context)
{
    var varName = context.ID().GetText();
    c.Comment("Reading variable: " + varName);

    var (offset, varObject) = c.GetObject(varName); // Busca la variable en la pila
    c.Mov(Register.X1, offset);
    c.Add(Register.X1, Register.SP, Register.X1);

    if (varObject.Type == StackObject.StackObjectType.Float)
    {
        c.Fldr(Register.D0, Register.X1);
        c.Push(Register.D0);
    }
    else
    {
        c.Ldr(Register.X0, Register.X1);
        c.Push(Register.X0);
    }

    c.PushObject(c.CloneObject(varObject));
    return null;
}


public override Object? VisitParens(LanguageParser.ParensContext context)
{
    return Visit(context.expr());
}

public override Object? VisitString(LanguageParser.StringContext context)
{
    var value = context.STRING().GetText().Trim('"');
    c.Comment("Constant: \"" + value + "\"");
    var stringObject = c.StringObject();
    c.PushConstant(stringObject, value); // Push the string value
    return null;
}

public override Object? VisitBoolean(LanguageParser.BooleanContext context)
{
    var value = context.BOOL().GetText();
    c.Comment("Constant: " + value);

    var boolObject = c.BoolObject();
    c.PushConstant(boolObject, value == "true" ? true : false);
    
    return null;
}



public override Object? VisitNegate(LanguageParser.NegateContext context)
{
    return null;
}

public override Object? VisitInt(LanguageParser.IntContext context)
{
    int value = int.Parse(context.GetText());
    c.Mov(Register.X0, value);      // Coloca valor en X0
    c.Push(Register.X0);            // Lo pone en el stack
    c.PushObject(c.IntObject());    // Marca tipo int para Pop
    return null;
}


public override Object? VisitNilExpr(LanguageParser.NilExprContext context)
{
    c.Comment("Valor nulo: nil");
    c.Mov(Register.X0, 0);
    c.Push(Register.X0);
    c.PushObject(new StackObject
    {
        Type = StackObject.StackObjectType.Pointer,
        Length = 8,
        Depth = 0,
        Id = null
    });
    return null;
}

public override ValueWrapper VisitAssign(LanguageParser.AssignContext context)
{
    var left  = context.expr(0);
    var right = context.expr(1);

    /*─────────────────────────────────────────────────────────
      A.  slice[index] = expr   (escritura en slice)
    ─────────────────────────────────────────────────────────*/
    /* ───────── slice[index] = rhs • escritura ───────── */
if (left is LanguageParser.CalleeContext calleeCtx &&
    calleeCtx.call().Length == 1 &&
    calleeCtx.call(0) is LanguageParser.ArrayAccessContext arrCtx)
{
    /* RHS */
    Visit(right);
    var valObj = c.PopObject(Register.X3);        // X3 = nuevo valor

    /* Slice e índice */
    Visit(calleeCtx.expr());                      // slice
    Visit(arrCtx.expr());                         // índice

    var idxObj   = c.PopObject(Register.X1);      // idx en X1
    var sliceObj = c.PopObject(Register.X0);      // base en X0

    if (idxObj.Type  != StackObject.StackObjectType.Int ||
        sliceObj.Type != StackObject.StackObjectType.Pointer)
        throw new Exception("asignación de slice mal formada");

    /* idx *= 8  */
    c.Mov(Register.X2, 8);
    c.Mul(Register.X1, Register.X1, Register.X2);

    /* dst = base + idx*8 */
    c.Add(Register.X0, Register.X0, Register.X1);

    /* STR nuevo valor */
    c.Str(Register.X3, Register.X0);

    /* deja valor en la pila (opcional) */
    c.Push(Register.X3);
    c.PushObject(valObj);
    return null;
}

    /*─────────────────────────────────────────────────────────
      B.  identificador = expr   (variable simple)
    ─────────────────────────────────────────────────────────*/
    if (left is LanguageParser.IdentifierContext idCtx)
    {
        string varName = idCtx.ID().GetText();

        /* RHS */
        Visit(right);
        var valueObject = c.PopObject(Register.X0);   // X0 = valor

        /* Localiza variable en la pila */
        var (offset, varObject) = c.GetObject(varName);

        /* Dirección destino en memoria */
        c.Mov(Register.X1, offset);
        c.Add(Register.X1, Register.SP, Register.X1);

        /* Guarda según tipo */
        switch (valueObject.Type)
        {
            case StackObject.StackObjectType.Float:
                c.Fstr(Register.D0, Register.X1);
                break;
            default:
                c.Str(Register.X0, Register.X1);
                break;
        }

        // 🔒 Ya no actualizamos tipo ni longitud para no romper el entorno

        /* Deja resultado actualizado en la pila */
        
        return null;
    }

    /* Si llegamos aquí: aún no soportado */
    throw new Exception("Solo se permite asignación a variable o slice[index].");
}


public override Object? VisitAddSub(LanguageParser.AddSubContext context)
{
    c.Comment("Add/Substract operation");

    var operation = context.op.Text;

    // Evaluar ambos operandos
    Visit(context.expr(0));
    Visit(context.expr(1));

    // Obtener tipos antes de hacer Pop
    var rightIsFloat = c.TopObject().Type == StackObject.StackObjectType.Float;
    var rightObj = c.PopObject(rightIsFloat ? Register.D0 : Register.X0);

    var leftIsFloat = c.TopObject().Type == StackObject.StackObjectType.Float;
    var leftObj = c.PopObject(leftIsFloat ? Register.D1 : Register.X1);

    var leftType = leftObj.Type;
    var rightType = rightObj.Type;

    // 🎯 Concatenación de strings
if (leftType == StackObject.StackObjectType.String && rightType == StackObject.StackObjectType.String)
{
    c.Pop(Register.X0); // right
    c.Pop(Register.X1); // left

    var loopCopyLeft = c.GetLabel();
    var loopCopyRight = c.GetLabel();

    // x10 es HP
    c.Mov("x2", "x10"); // x2 = destino actual
    c.Mov("x3", "x10"); // x3 = dirección inicial (esto se debe usar para guardar en la variable)

    // Copiar izquierda
    c.SetLabel(loopCopyLeft);
    c.Ldrb("w4", "x1");
    c.Strb("w4", "x2");
    c.Add("x1", "x1", 1);
    c.Add("x2", "x2", 1);
    c.Cmp("w4", 0);
    c.Bne(loopCopyLeft);

    // Copiar derecha
    c.SetLabel(loopCopyRight);
    c.Ldrb("w4", "x0");
    c.Strb("w4", "x2");
    c.Add("x0", "x0", 1);
    c.Add("x2", "x2", 1);
    c.Cmp("w4", 0);
    c.Bne(loopCopyRight);

    // Finalmente: Push de x3 (inicio de string concatenado, no x10)
    c.Push("x3");
    c.PushObject(c.StringObject());

    return null;
}
    // 🎯 Caso de suma/resta con float
    if (leftType == StackObject.StackObjectType.Float || rightType == StackObject.StackObjectType.Float)
    {
        if (!leftIsFloat) c.Scvtf(Register.D1, Register.X1);
        if (!rightIsFloat) c.Scvtf(Register.D0, Register.X0);

        if (operation == "+")
            c.Fadd(Register.D0, Register.D0, Register.D1);
        else
            c.Fsub(Register.D0, Register.D0, Register.D1);

        c.Push(Register.D0);
        c.PushObject(c.FloatObject());
        return null;
    }

    // 🎯 Caso suma/resta de enteros
    if (operation == "+")
        c.Add(Register.X0, Register.X0, Register.X1);
    else
        c.Sub(Register.X0, Register.X0, Register.X1);

    c.Push(Register.X0);
    c.PushObject(c.IntObject());
    return null;
}


public override Object? VisitNot(LanguageParser.NotContext context)
{
    c.Comment("Logical NOT");

    // Evaluar la expresión
    Visit(context.expr());
    
    // Verificar que el operando sea booleano
    var obj = c.TopObject();
    if (obj.Type != StackObject.StackObjectType.Bool) 
        throw new Exception("El operando de ! no es booleano");
    
    // Extraer el valor booleano
    c.PopObject("X0");
    
    // Etiquetas para el salto condicional
    var trueLabel = c.GetLabel();
    var endLabel = c.GetLabel();

    // Invertir la lógica: si X0 == 0 (false), salta a trueLabel para devolver true
    c.Cbz("X0", trueLabel);
    
    // X0 != 0 (true) => !true => false
    c.Mov("X0", 0);
    c.B(endLabel);

    // trueLabel: X0 == 0 (false) => !false => true
    c.SetLabel(trueLabel);
    c.Mov("X0", 1);

    // Fin común
    c.SetLabel(endLabel);
    c.Push("X0");
    c.PushObject(c.BoolObject());
    
    return null;
}
public override Object? VisitMulDiv(LanguageParser.MulDivContext context)
{
    c.Comment("Multiplicación/División");

    var operation = context.op.Text; // Operación puede ser "*" o "/"

    // Evaluamos ambos operandos
    Visit(context.expr(0));  // Evaluar el primer operando (izquierdo)
    Visit(context.expr(1));  // Evaluar el segundo operando (derecho)

    // Extraemos el segundo operando (derecho)
    var isRightDouble = c.TopObject().Type == StackObject.StackObjectType.Float;
    var right = c.PopObject(isRightDouble ? Register.D0 : Register.X0); 

    // Extraemos el primer operando (izquierdo)
    var isLeftDouble = c.TopObject().Type == StackObject.StackObjectType.Float;
    var left = c.PopObject(isLeftDouble ? Register.D1 : Register.X1);

    // Si al menos uno de los operandos es flotante, hacemos operación con flotantes
    if (isLeftDouble || isRightDouble) 
    {
        // Convertimos los operandos enteros a flotantes si es necesario
        if (!isLeftDouble) c.Scvtf(Register.D1, Register.X1);  // Convierte X1 a D1
        if (!isRightDouble) c.Scvtf(Register.D0, Register.X0); // Convierte X0 a D0

        if (operation == "*")
        {
            // Multiplicación: D0 = D1 * D0
            c.Fmul(Register.D0, Register.D1, Register.D0);
        }
        else // División
        {
            // División: D0 = D1 / D0 (izquierdo / derecho)
            c.Fdiv(Register.D0, Register.D1, Register.D0);
        }

        c.Push(Register.D0);  // Empuja el resultado flotante
        c.PushObject(c.FloatObject());
        return null;
    }

    // Si ambos operandos son enteros
    if (operation == "*")
    {
        // Multiplicación: X0 = X1 * X0
        c.Mul(Register.X0, Register.X1, Register.X0);
    }
    else // División
    {
        // División: X0 = X1 / X0 (izquierdo / derecho)
        c.Div(Register.X0, Register.X1, Register.X0);
    }

    c.Push(Register.X0);  // Empuja el resultado entero
    c.PushObject(c.IntObject());
    return null;
}




public override Object? VisitWhileStmt(LanguageParser.WhileStmtContext context)
{
    c.Comment("While statement");
    var startLabel = c.GetLabel();
    var endLabel = c.GetLabel();

    var prevContinueLabel = continueLabel;
    var prevBreakLabel = breakLabel;
    continueLabel = startLabel;
    breakLabel = endLabel;


    c.SetLabel(startLabel);
    Visit(context.expr());
    c.PopObject(Register.X0);
    c.Cbz(Register.X0, endLabel);
    Visit(context.stmt());
    c.B(startLabel);
    c.SetLabel(endLabel);

    c.Comment("End of while statement");

    continueLabel = prevContinueLabel;
    breakLabel = prevBreakLabel;

    return null;    
    
}



public override Object? VisitIfStmt(LanguageParser.IfStmtContext context)
{
    c.Comment("If statement");
    // 1) Generar y evaluar la condición
    Visit(context.expr());
    c.PopObject(Register.X0);

    bool hasElse = context.stmt().Length > 1;
    string elseLabel = c.GetLabel();
    string endLabel  = c.GetLabel();

    // 2) Si la condición es falsa, saltar a else (o fin si no hay else)
    c.Cbz(Register.X0, hasElse ? elseLabel : endLabel);

    // 3) Bloque "then"
    Visit(context.stmt(0));
    c.B(endLabel);

    if (hasElse)
    {
        // 4) Bloque "else"
        c.SetLabel(elseLabel);
        Visit(context.stmt(1));
    }

    // 5) Punto final del if
    c.SetLabel(endLabel);
    return null;
}




public override Object? VisitFloat(LanguageParser.FloatContext context)
{

    var value = context.FLOAT().GetText();

    c.Comment("Constant: " + value);
    var FloatObject = c.FloatObject();
    c.PushConstant(FloatObject, double.Parse(value));
    return null;
}




public override Object? VisitBlockStmt(LanguageParser.BlockStmtContext context)
{
    c.Comment("Inicio de bloque {");
    c.NewScope();

    foreach (var dcl in context.dcl())
    {
        Visit(dcl);
    }

    int bytesToRemove = c.endScope();
    
    if (bytesToRemove > 0)
    {
        c.Comment("Removing" + bytesToRemove + "bytes from stack");
        c.Mov(Register.X0, bytesToRemove);
        c.Add(Register.SP, Register.SP, Register.X0);
        c.Comment("Stack pointend adjusted");
        
    }
    
    return null;
    
}

// For estilo “while”
// 1. Corrección para el método VisitForStmtCond
public override Object? VisitForStmtCond(LanguageParser.ForStmtCondContext context)
{
    c.Comment("Inicio de bucle For (tipo while)");

    var startLabel = c.GetLabel();
    var endLabel = c.GetLabel();
    var condLabel = c.GetLabel(); // Etiqueta para evaluar la condición

    // Guardar los labels externos
    var prevContinue = continueLabel;
    var prevBreak = breakLabel;
    continueLabel = condLabel; // Continuar va a evaluar la condición
    breakLabel = endLabel;

    // Abrir un nuevo ámbito para el bucle
    c.NewScope();

    // Etiqueta para evaluar la condición
    c.SetLabel(condLabel);
    
    // Evaluar la condición
    Visit(context.expr());
    c.PopObject(Register.X0);
    c.Cbz(Register.X0, endLabel); // Si la condición es falsa, salir del bucle
    
    // Etiqueta de inicio del cuerpo
    c.SetLabel(startLabel);
    
    // Cuerpo del bucle
    Visit(context.stmt());
    
    // Volver a evaluar la condición
    c.B(condLabel);
    
    // Etiqueta de fin
    c.SetLabel(endLabel);
    
    // Cerrar el ámbito y limpiar el stack
    var bytes = c.endScope();
    if (bytes > 0)
    {
        c.Comment($"Liberando {bytes} bytes del stack");
        c.Mov(Register.X0, bytes);
        c.Add(Register.SP, Register.SP, Register.X0);
    }
    
    // Restaurar labels externos
    continueLabel = prevContinue;
    breakLabel = prevBreak;
    
    c.Comment("Fin de bucle For (tipo while)");
    return null;
}

public override Object? VisitForStmt(LanguageParser.ForStmtContext context)
    {
         
      //Este es el for tipo for
        c.Comment("Estoy en el for clasico");
        var startLabel = c.GetLabel();
        var endLabel = c.GetLabel();
        var incrementLabel = c.GetLabel(); 

        var prevContinueLabel = continueLabel;
        var prevBreakLabel = breakLabel;

        continueLabel = incrementLabel;
        breakLabel = endLabel;

        c.NewScope();

        Visit(context.forInit());
        c.SetLabel(startLabel);
        Visit(context.expr(0));
        c.PopObject(Register.X0);
        c.Cbz(Register.X0, endLabel);
        Visit(context.stmt());
        c.SetLabel(incrementLabel);
        Visit(context.expr(1));
        c.B(startLabel);
        c.SetLabel(endLabel);

        c.Comment("End of for statement");

        var bytesToRemove = c.endScope();

        if(bytesToRemove > 0)
        {
            c.Comment("Removing " + bytesToRemove + "bytes from stack");
            c.Mov(Register.X0, bytesToRemove);
            c.Add(Register.SP, Register.SP, Register.X0 );
            c.Comment("stack pointer adjusted");
        }

        continueLabel = prevContinueLabel;
        breakLabel = prevBreakLabel;
        return null;
    }

    public override Object? VisitPostIncrement(LanguageParser.PostIncrementContext context)
    {
        c.Comment("Visiting Post Increment");

        // Get the variable name
        var id = context.ID().GetText();
        c.Comment($"Incrementing variable: {id}");

        // Check if the variable exists
        var (offset, varObject) = c.GetObject(id);

        if (varObject.Type != StackObject.StackObjectType.Int)
        {
            throw new Exception($"Unsupported type for increment: {varObject.Type}");
        }

        // Load the variable's value
        c.Mov(Register.X0, offset);
        c.Add(Register.X0, Register.SP, Register.X0);
        c.Ldr(Register.X1, Register.X0);

        // Increment the value
        c.Add(Register.X1, Register.X1, "1");

        // Store the incremented value back
        c.Str(Register.X1, Register.X0);

        // Push the original value onto the stack (post-increment behavior)
        c.Push(Register.X1);
        c.PushObject(c.IntObject());

        return null;
    }


    


public override Object? VisitForRangeStmt(LanguageParser.ForRangeStmtContext context)
{
    c.Comment("Visiting For Range Statement");

    var startLabel = c.GetLabel();        // Etiqueta de inicio del ciclo
    var endLabel = c.GetLabel();          // Etiqueta de fin del ciclo
    var incrementLabel = c.GetLabel();   // Etiqueta de incremento

    var prevContinueLabel = continueLabel;
    var prevBreakLabel = breakLabel;

    continueLabel = incrementLabel;
    breakLabel = endLabel;

    c.Comment("For range initialization");
    c.NewScope();  // Crea un nuevo entorno para el ciclo

    // Obtener los nombres de las variables del índice, valor y rango
    var indexVar = context.ID(0).GetText();  // Primer ID: índice
    var valueVar = context.ID(1).GetText();  // Segundo ID: valor
    var rangeVar = context.ID(2).GetText();  // Tercer ID: rango (slice o array)

    // Declarar las variables índice y valor en el stack
    var indexObj = c.IntObject();
    indexObj.Id = indexVar;
    c.PushObject(indexObj);  // Empujar índice al stack
    c.Mov(Register.X0, 0);   // Inicializar índice en 0
    c.Push(Register.X0);

    var valueObj = c.IntObject();
    valueObj.Id = valueVar;
    c.PushObject(valueObj);  // Empujar valor al stack

    // Obtener el rango (slice o array) del stack
    var (rangeOffset, rangeObj) = c.GetObject(rangeVar);
    if (rangeObj.Type != StackObject.StackObjectType.Pointer)
        throw new Exception($"{rangeVar} no es un slice o array");

    // Establecer la etiqueta de inicio del ciclo
    c.SetLabel(startLabel);

    // Cargar el índice actual
    c.Ldr(Register.X0, Register.SP, indexObj.Depth);

    // Comprobar si el índice está fuera del rango
    c.Cmp(Register.X0, rangeObj.Length);
    c.Bge(endLabel);  // Si el índice >= longitud del rango, salir del bucle

    // Cargar el valor actual del rango
    c.Mov(Register.X1, rangeOffset);  // Dirección base del rango
    c.Add(Register.X1, Register.X1, Register.X0);  // Sumar desplazamiento al puntero base
    c.Ldr(Register.X2, Register.X1);  // Cargar el valor en X2
    c.Str(Register.X2, Register.SP, valueObj.Depth);  // Guardar el valor en el stack

    // Ejecutar el cuerpo del bucle
    Visit(context.stmt());

    // Incrementar el índice
    c.SetLabel(incrementLabel);
    c.Ldr(Register.X0, Register.SP, indexObj.Depth);  // Cargar el índice actual
    c.Add(Register.X0, Register.X0, 1);              // Incrementar índice
    c.Str(Register.X0, Register.SP, indexObj.Depth); // Guardar el índice actualizado
    c.B(startLabel);                                 // Volver al inicio del bucle

    c.SetLabel(endLabel);      // Establecer la etiqueta de fin del ciclo
    c.Comment("End of For Range Statement");

    // Limpiar el entorno después del ciclo
    var bytesToRemove = c.endScope();
    if (bytesToRemove > 0)
    {
        c.Comment($"Removing {bytesToRemove} bytes from stack");
        c.Mov(Register.X0, bytesToRemove);
        c.Add(Register.SP, Register.SP, Register.X0);  // Ajusta el puntero de la pila
    }

    // Restaurar las etiquetas de continue y break
    continueLabel = prevContinueLabel;
    breakLabel = prevBreakLabel;

    return null;
}



public override Object? VisitSwitchStmt(LanguageParser.SwitchStmtContext context)
{
    c.Comment("Switch Statement");

    // Save previous break label and generate a new one for this switch
    var previousBreakLabel = breakLabel;
    breakLabel = c.GetLabel();
    string endLabel = breakLabel;

    // Evaluate the switch expression and store the value in X19
    Visit(context.expr());
    c.PopObject(Register.X19); // X19 contains the evaluated switch value

    // Labels for each case
    var caseLabels = new List<string>();
    foreach (var caseCtx in context.caseStmt())
    {
        caseLabels.Add(c.GetLabel());
    }
    
    // Get label for default, if it exists
    string? defaultLabel = context.defaultStmt() != null ? c.GetLabel() : null;

    // Compare the switch value with each case value
    int caseIndex = 0;
    foreach (var caseCtx in context.caseStmt())
    {
        string caseLabel = caseLabels[caseIndex++];

        // Evaluate the case expression and store the value in X1
        Visit(caseCtx.expr());
        c.PopObject(Register.X1);  // X1 contains the case value

        // Compare X19 (switch value) with X1 (case value)
        c.Cmp(Register.X19, Register.X1);  // Compare values
        c.Beq(caseLabel);  // If equal, jump to caseLabel
    }

    // If no cases matched, jump to default (if exists) or to end
    if (context.defaultStmt() != null && defaultLabel != null)
    {
        c.B(defaultLabel);  // Jump to default if no case matched
    }
    else
    {
        c.B(endLabel);  // Otherwise, jump to end of switch
    }

    // Process each case
    caseIndex = 0;
    foreach (var caseCtx in context.caseStmt())
    {
        string caseLabel = caseLabels[caseIndex++];

        // Label for this case
        c.SetLabel(caseLabel);
        c.NewScope(); // Create new environment for the case
        
        bool hasBreak = false;
        foreach (var stmt in caseCtx.stmt())
        {
            Visit(stmt);  // Execute instruction inside the case
            if (stmt is LanguageParser.BreakStmtContext)
            {
                hasBreak = true;
                break;  // Stop processing further statements in this case
            }
        }

        int bytesToRemove = c.endScope(); // Clean up environment after case
        if (bytesToRemove > 0)
        {
            c.Comment($"Removing {bytesToRemove} bytes from stack");
            c.Mov(Register.X0, bytesToRemove);
            c.Add(Register.SP, Register.SP, Register.X0);  // Adjust stack pointer
        }
        
        // If there was an explicit break, jump to end
        if (hasBreak)
        {
            c.B(endLabel);
        }
        else
        {
            // In Go, cases don't fall through by default unless 'fallthrough' keyword is used
            c.B(endLabel); // Always jump to end after executing a case
        }
    }

    // Handle default case if it exists
    if (context.defaultStmt() != null && defaultLabel != null)
    {
        c.SetLabel(defaultLabel);  // Set default label
        c.NewScope(); // Create new environment for default block
        
        bool hasBreak = false;
        foreach (var stmt in context.defaultStmt().stmt())
        {
            Visit(stmt);  // Execute instructions in default block
            if (stmt is LanguageParser.BreakStmtContext)
            {
                hasBreak = true;
                break;  // Stop processing further statements in default
            }
        }

        int bytesToRemove = c.endScope(); // Clean up environment
        if (bytesToRemove > 0)
        {
            c.Comment($"Removing {bytesToRemove} bytes from stack");
            c.Mov(Register.X0, bytesToRemove);
            c.Add(Register.SP, Register.SP, Register.X0);  // Adjust stack pointer
        }
        
        // Default case also jumps to end when done
        c.B(endLabel);
    }

    // Final label for the switch
    c.SetLabel(endLabel);
    breakLabel = previousBreakLabel;  // Restore previous break label
    return null;
}


public override Object? VisitBreakStmt(LanguageParser.BreakStmtContext context)
    {
        c.Comment("Visiting Break Statement");
        if(breakLabel != null)
        {
            c.B(breakLabel);
        }
        return null;
    }
public override Object? VisitContinueStmt(LanguageParser.ContinueStmtContext context)
{

    c.Comment("Continue statement");
    if (continueLabel != null)
    {
        c.B(continueLabel);
    }

    return null;
}    

public override Object? VisitRelational(LanguageParser.RelationalContext ctx)
{
    // 1) evaluate both sides onto the stack
    Visit(ctx.expr(0));
    Visit(ctx.expr(1));

    // 2) pop into registers
    var rightIsFloat = c.TopObject().Type == StackObject.StackObjectType.Float;
    var right = c.PopObject(rightIsFloat ? Register.D0 : Register.X0);
    var leftIsFloat  = c.TopObject().Type == StackObject.StackObjectType.Float;
    var left  = c.PopObject(leftIsFloat  ? Register.D1 : Register.X1);

    // 3) do the appropriate compare
    if (leftIsFloat || rightIsFloat)
    {
        if (!leftIsFloat)  c.Scvtf(Register.D1, Register.X1);
        if (!rightIsFloat) c.Scvtf(Register.D0, Register.X0);
        c.Fcmp(Register.D1, Register.D0);
    }
    else
    {
        c.Cmp(Register.X1, Register.X0);  // integer/rune compare
    }

    // 4) set up true/false labels
    var trueLabel = c.GetLabel();
    var endLabel  = c.GetLabel();

    switch (ctx.op.Text)
    {
        case "<":  if (leftIsFloat||rightIsFloat) c.Blt(trueLabel); else c.Blt(trueLabel); break;
        case "<=": if (leftIsFloat||rightIsFloat) c.Ble(trueLabel); else c.Ble(trueLabel); break;
        case ">":  if (leftIsFloat||rightIsFloat) c.Bgt(trueLabel); else c.Bgt(trueLabel); break;
        case ">=": if (leftIsFloat||rightIsFloat) c.Bge(trueLabel); else c.Bge(trueLabel); break;
        default: throw new Exception($"Unsupported op {ctx.op.Text}");
    }

    // 5) false path
    c.Mov(Register.X0, 0);
    c.B(endLabel);

    // 6) true path
    c.SetLabel(trueLabel);
    c.Mov(Register.X0, 1);

    // 7) end
    c.SetLabel(endLabel);
    c.Push(Register.X0);
    c.PushObject(c.BoolObject());

    return null;
}

public override Object? VisitReturnStmt(LanguageParser.ReturnStmtContext context)
{
    if (context.expr() != null)
    {
        Visit(context.expr());
    }

    if (returnLabel != null)
    {
        c.B(returnLabel);
    }

    return null;
}

public override Object? VisitArray(LanguageParser.ArrayContext ctx)
{
    var tipo  = ctx.Tipo().GetText();
    var exprs = ctx.args()?.expr();
    int n     = exprs?.Length ?? 0;

    if (tipo != "int")
        throw new Exception("Solo []int por ahora");

    // 1. Calcula cuánto espacio necesitas y adelanta HP de golpe
    c.Comment($"Reservar {n*8} bytes para slice");
    c.Mov("x9", Register.HP);           // x9 = baseSlice
    c.Add(Register.HP, Register.HP, n*8); // HP  += n*8   (⚠ sin tocar x9)

    // 2. Recorre las expresiones y cópialas
    long offset = 0;
    foreach (var e in exprs)
    {
        Visit(e);
        c.PopObject(Register.X0);            // X0 = valor int
        c.Str(Register.X0, "x9", (int)offset);    // [x9+offset] = X0
        offset += 8;
    }

    // 3. Deja el puntero en la pila + objeto
    var sliceObj = c.PointerObject();
    sliceObj.Length = n;
    sliceObj.Id     = $"slice_tmp_{Guid.NewGuid():N}";
    sliceObj.Value  = "x9";

    c.Mov(Register.X0, "x9");
    c.Push(Register.X0);
    c.PushObject(sliceObj);

    return sliceObj;
}
public override object? VisitCallee(LanguageParser.CalleeContext ctx)
{
    /* ───────── slice[index] • lectura ───────── */
    if (ctx.call().Length == 1 &&
        ctx.call(0) is LanguageParser.ArrayAccessContext arrCtx)
    {
        /* Slice -> Ptr + Obj */
        Visit(ctx.expr());

        /* Índice -> Int + Obj */
        Visit(arrCtx.expr());

        var idxObj   = c.PopObject(Register.X1);  // X1 = índice
        var sliceObj = c.PopObject(Register.X0);  // X0 = base ptr

        if (idxObj.Type  != StackObject.StackObjectType.Int ||
            sliceObj.Type != StackObject.StackObjectType.Pointer)
            throw new Exception("indexing inválido");

        /* X1 = X1 * 8  */
        c.Mov(Register.X2, 8);
        c.Mul(Register.X1, Register.X1, Register.X2);

        /* X0 = base + idx*8 */
        c.Add(Register.X0, Register.X0, Register.X1);

        /* LDR elemento → X3  */
        c.Ldr(Register.X3, Register.X0);
        c.Push(Register.X3);
        c.PushObject(c.IntObject());

        return null;
    }

    /* ─── llamada normal a función (fmt.Println, etc.) ─── */
    var funcName = ctx.expr().GetText();
    c.Comment($"--- Llamada a función {funcName} ---");
    c.Bl($"func_{funcName}");
    c.Push(Register.X0);
    c.PushObject(c.IntObject());
    return null;
}


public Object? VisitCall(LanguageParser.ArgsContext argsContext, string funcName)
{
    c.Comment($"Llamando a función: {funcName}");

    if (argsContext != null)
    {
        foreach (var expr in argsContext.expr())
        {
            Visit(expr); // Genera los argumentos en orden (se apilan)
        }
    }

    c.Bl($"func_{funcName}"); // Llamada a la función (etiqueta generada en VisitFuncDcl)

    c.Push(Register.X0); // Se espera que el retorno esté en x0
    c.PushObject(c.IntObject()); // Aquí asumimos que es entero. Puedes ajustar según el retorno.

    return null;
}


public override Object? VisitIndexSlice(LanguageParser.IndexSliceContext ctx)
{
    string sliceName = ctx.ID().GetText();

    /* 1. valor a buscar */
    Visit(ctx.expr());
    var valObj = c.PopObject(Register.X0);      // ◆  X0 = valor buscado

    if (valObj.Type != StackObject.StackObjectType.Int)
        throw new Exception("slices.Index solo soporta enteros");

    /* 2. puntero base y longitud */
    var (offset, sliceObj) = c.GetObject(sliceName);
    if (sliceObj.Type != StackObject.StackObjectType.Pointer)
        throw new Exception($"{sliceName} no es slice");

    c.Ldr(Register.X1, Register.SP, offset);    // X1 = base ptr
    c.Mov(Register.X2, sliceObj.Length);        // X2 = len

    /* índices para el bucle */
    c.Mov(Register.X3, 0);    // idx
    c.Mov(Register.X4, Register.X1); // curPtr

    var loop  = c.GetLabel();
    var found = c.GetLabel();
    var fail  = c.GetLabel();
    var fin   = c.GetLabel();

    c.SetLabel(loop);
    c.Cmp(Register.X3, Register.X2);   // idx < len ?
    c.Bge(fail);

    c.Ldr(Register.X5, Register.X4);   // X5 = elemento
    c.Cmp(Register.X5, Register.X0);   // ◆ comparar con valor en X0
    c.Beq(found);

    c.Add(Register.X4, Register.X4, 8); // curPtr += 8
    c.Add(Register.X3, Register.X3, 1); // idx++
    c.B(loop);

    c.SetLabel(found);
    c.Mov(Register.X0, Register.X3);
    c.B(fin);

    c.SetLabel(fail);
    c.Mov(Register.X0, -1);

    c.SetLabel(fin);
    c.Push(Register.X0);
    c.PushObject(c.IntObject());
    return null;
}

public override Object? VisitJoin(LanguageParser.JoinContext ctx)
{
    // strings.Join(ID, sepExpr)
    string sliceName = ctx.ID().GetText();

    /* 1. Evaluar separador -> puntero en X6 */
    Visit(ctx.expr());
    var sepObj = c.PopObject(Register.X6);
    if (sepObj.Type != StackObject.StackObjectType.String)
        throw new Exception("El separador de strings.Join debe ser string");

    /* 2. Obtener slice []string */
    var (offset, sliceObj) = c.GetObject(sliceName);
    if (sliceObj.Type != StackObject.StackObjectType.Pointer)
        throw new Exception($"{sliceName} no es slice");

    int len = sliceObj.Length;
    if (len == 0)
    {
        // cadena vacía
        var emptyObj = c.StringObject();
        c.Push(Register.HP);     // HP apunta a terminador
        c.PushObject(emptyObj);
        return null;
    }

    /* 3. Crear string destino (objeto inicializado)           */
    var resultObj = c.StringObject();         // deja ptr en HP y el objeto encima
    c.PopObject(Register.X1);                 // X1  = objeto (lo desechamos)
    c.Pop(Register.X1);                       // X1  = ptr inicio destino
    c.Mov(Register.X2, Register.X1);          // X2  = cursor destino

    /* Puntero base del slice */
    c.Ldr(Register.X0, Register.SP, offset);  // X0  = base slice

    /* 4. Copiar palabras y separadores                        */
    for (int i = 0; i < len; i++)
    {
        // ---- palabra i ----
        c.Ldr(Register.X3, Register.X0, i * 8);      // X3 = ptr palabra
        var loopStr = c.GetLabel();
        c.SetLabel(loopStr);
        c.Ldrb("w4", Register.X3);
        c.Strb("w4", Register.X2);
        c.Add(Register.X3, Register.X3, 1);
        c.Add(Register.X2, Register.X2, 1);
        c.Cmp("w4", 0);
        c.Bne(loopStr);

        // ---- separador (si no es la última) ----
        if (i < len - 1)
        {
            var loopSep = c.GetLabel();
            c.Mov(Register.X5, Register.X6);         // X5 = ptr sep
            c.SetLabel(loopSep);
            c.Ldrb("w4", Register.X5);
            c.Strb("w4", Register.X2);
            c.Add(Register.X5, Register.X5, 1);
            c.Add(Register.X2, Register.X2, 1);
            c.Cmp("w4", 0);
            c.Bne(loopSep);
        }
    }

    /* 5. Actualizar HP al nuevo cursor                         */
    c.Mov(Register.HP, Register.X2);

    /* 6. Dejar en la pila: ptr resultado + objeto‑string listo */
    c.Push(Register.X1);                 // ptr resultado
    c.PushObject(resultObj);             // objeto creado por StringObject
    return null;
}

public override Object? VisitAppend(LanguageParser.AppendContext context)
{
    return null;
}
public override Object? VisitLen(LanguageParser.LenContext context)
{
    return null;
}

public override Object? VisitLogicalAnd(LanguageParser.LogicalAndContext context)
{
    c.Comment("Logical AND");

    Visit(context.expr(0));
    var leftObj = c.PopObject("X0");

    if (leftObj.Type != StackObject.StackObjectType.Bool)
        throw new Exception("El operando izquierdo de && no es booleano");

    var falseLabel = c.GetLabel();
    var endLabel = c.GetLabel();

    c.Cbz("X0", falseLabel);  // si izquierdo es false, salta

    Visit(context.expr(1));
    var rightObj = c.PopObject("X0");

    if (rightObj.Type != StackObject.StackObjectType.Bool)
        throw new Exception("El operando derecho de && no es booleano");

    c.Cbz("X0", falseLabel);  // si derecho es false, salta

    c.Mov("X0", 1);   // Ambos true
    c.B(endLabel);

    c.SetLabel(falseLabel);
    c.Mov("X0", 0);   // Al menos uno false

    c.SetLabel(endLabel);
    c.Push("X0");
    c.PushObject(c.BoolObject());
    

    return null;
}

public override Object? VisitEquality(LanguageParser.EqualityContext context)
{
    var operation = context.op.Text;
    
    // Evaluar ambos operandos
    Visit(context.expr(0));
    Visit(context.expr(1));

    c.Comment("Comparación de igualdad");
    
    // Obtener el operando derecho
    var rightType = c.TopObject().Type;
    var isRightDouble = rightType == StackObject.StackObjectType.Float;
    var right = c.PopObject(isRightDouble ? Register.D0 : Register.X0);
    
    // Obtener el operando izquierdo
    var leftType = c.TopObject().Type;
    var isLeftDouble = leftType == StackObject.StackObjectType.Float;
    var left = c.PopObject(isLeftDouble ? Register.D1 : Register.X1);
    
    c.Comment($"Operación: {operation} entre {leftType} y {rightType}");
    
    // Etiquetar los saltos
    var trueLabel = c.GetLabel();
    var endLabel = c.GetLabel();
    
    // Caso especial: comparación de strings
    if (leftType == StackObject.StackObjectType.String && rightType == StackObject.StackObjectType.String)
    {
        c.Comment("Comparación de strings");
        
        // X1 contiene el puntero al primer string
        // X0 contiene el puntero al segundo string
        
        // Guardar los punteros originales
        c.Mov("x2", "x1");  // x2 = puntero del primer string
        c.Mov("x3", "x0");  // x3 = puntero del segundo string
        
        var loopStart = c.GetLabel();
        var stringsEqual = c.GetLabel();
        var stringsNotEqual = c.GetLabel();
        
        // Inicio del bucle de comparación
        c.SetLabel(loopStart);
        
        // Cargar bytes individuales
        c.Ldrb("w4", "x2");  // w4 = *x2 (carácter del primer string)
        c.Ldrb("w5", "x3");  // w5 = *x3 (carácter del segundo string)
        
        // Comparar los caracteres
        c.Cmp("w4", "w5");
        c.Bne(stringsNotEqual);  // Si son diferentes, no son iguales
        
        // Verificar si llegamos al final (carácter nulo)
        c.Cmp("w4", 0);
        c.Beq(stringsEqual);  // Si ambos son nulos, son iguales
        
        // Avanzar al siguiente carácter
        c.Add("x2", "x2", 1);
        c.Add("x3", "x3", 1);
        
        // Volver al inicio del bucle
        c.B(loopStart);
        
        // Los strings son iguales
        c.SetLabel(stringsEqual);
        if (operation == "==")
            c.B(trueLabel);  // "==" verdadero
        else
            c.Mov("x0", 0);  // "!=" falso
            c.B(endLabel);
        
        // Los strings no son iguales
        c.SetLabel(stringsNotEqual);
        if (operation == "==") {
            c.Mov("x0", 0);  // "==" falso
            c.B(endLabel);
        } else
            c.B(trueLabel);  // "!=" verdadero
    }
    // Comparación de números (enteros o flotantes)
    else if (isLeftDouble || isRightDouble)
    {
        c.Comment("Comparación de punto flotante");
        
        // Convertir a punto flotante si es necesario
        if (!isLeftDouble)
            c.Scvtf(Register.D1, Register.X1);
        if (!isRightDouble)
            c.Scvtf(Register.D0, Register.X0);
        
        // Comparar flotantes
        c.Fcmp(Register.D1, Register.D0);
        
        // Saltar según la operación
        if (operation == "==")
            c.Beq(trueLabel);
        else  // "!="
            c.Bne(trueLabel);
    }
    // Comparación de enteros u otros tipos
    else
    {
        c.Comment("Comparación de enteros u otros tipos");
        
        c.Cmp(Register.X1, Register.X0);
        
        // Saltar según la operación
        if (operation == "==")
            c.Beq(trueLabel);
        else  // "!="
            c.Bne(trueLabel);
    }
    
    // Caso falso por defecto
    c.Mov("x0", 0);  // false
    c.B(endLabel);
    
    // Caso verdadero
    c.SetLabel(trueLabel);
    c.Mov("x0", 1);  // true
    
    // Fin común
    c.SetLabel(endLabel);
    c.Push("x0");
    c.PushObject(c.BoolObject());
    
    return null;
}


public override Object? VisitLogicalOr(LanguageParser.LogicalOrContext context)
{
    c.Comment("Logical OR");

    // Evaluar primer operando
    Visit(context.expr(0));
    var leftObj = c.PopObject("X0");

    if (leftObj.Type != StackObject.StackObjectType.Bool)
        throw new Exception("El operando izquierdo de || no es booleano");

    var trueLabel = c.GetLabel();
    var evalRight = c.GetLabel();
    var endLabel = c.GetLabel();

    // Si izquierdo es true, salta directamente a trueLabel
    c.Cbnz("X0", trueLabel);
    c.B(evalRight);

    // Evaluar segundo operando
    c.SetLabel(evalRight);
    Visit(context.expr(1));
    var rightObj = c.PopObject("X0");

    if (rightObj.Type != StackObject.StackObjectType.Bool)
        throw new Exception("El operando derecho de || no es booleano");

    c.Cbnz("X0", trueLabel);

    // Ambos false
    c.Mov("X0", 0);
    c.B(endLabel);

    // Al menos uno true
    c.SetLabel(trueLabel);
    c.Mov("X0", 1);

    c.SetLabel(endLabel);
    c.Push("X0");
    c.PushObject(c.BoolObject());

    return null;
}

public override Object? VisitClassDcl(LanguageParser.ClassDclContext context)
{
    return null;
}
public override Object? VisitNew(LanguageParser.NewContext context)
{
    return null;
}

}
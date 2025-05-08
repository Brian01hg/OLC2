//GENERATOR.CS

using System.Text;
using System.Collections.Generic;

public class StackObject
{
    public enum StackObjectType {Int, Float, String, Bool, Void, Rune, Array, Struct, Function, Class, Object, Pointer}
    public StackObjectType Type { get; set; }
    public int Length { get; set; }
    public int Depth { get; set; }
    public string? Id  { get; set; }

    public object? Value { get; set; }

}


public class armGenerator{



    private readonly List<string> instructions = new List<string>();

    private readonly Dictionary<string, StackObject> symbolTable = new Dictionary<string, StackObject>();

    private readonly StandardLibrary stdLib = new StandardLibrary();

    private readonly List<StackObject> stack = new List<StackObject>();

    private int depth = 0;

    private int labelCounter = 0;

    public string GetLabel()
    {
        return $"L{labelCounter++}";
    }

    public void SetLabel(string label)
    {
        instructions.Add($"{label}:");
    }

    //----STACK OPERTAIONS----//

    public StackObject IntArrayObject()
    {
        // Crea un StackObject de tipo array de enteros
        return new StackObject { Type = StackObject.StackObjectType.Int, Length = 0, Depth = 0 };
    }

    public void OpenScope()
{
    Comment("=== OPEN SCOPE ===");
}

public void CloseScope()
{
    Comment("=== CLOSE SCOPE ===");
}

public void Return()
{
    Comment("=== RETURN ===");
    // Podrías saltar al final o agregar código personalizado si deseas
    Mov("x0", 0);       // código de salida (opcional)
    Mov("x8", 93);      // syscall de salida
    Svc();              // ejecutar syscall
}

public void PushObject(StackObject obj)
    {
        stack.Add(obj);
        
    }

public void DebugStack()
{
    Console.WriteLine("🔍 STACK:");
    foreach (var obj in stack)
    {
        Console.WriteLine($" - {obj.Id} ({obj.Type}), depth={obj.Depth}, len={obj.Length}");
    }
}



    public void TagObject(StackObject obj)
    {
        stack.Add(obj);
    }

   
    public void And(string rd, string rs1, string rs2) => instructions.Add($"AND {rd}, {rs1}, {rs2}");
    public void Orr(string rd, string rs1, string rs2) => instructions.Add($"ORR {rd}, {rs1}, {rs2}");

    public void Fcmp(string rs1, string rs2)
    {
        instructions.Add($"FCMP {rs1}, {rs2}");
    }

    public void Fmov(string register, double value)
{
    // Genera la instrucción FMOV para mover un valor de punto flotante a un registro
    instructions.Add($"FMOV {register}, #{value}");
}

    public void Fcvtns(string destRegister, string srcRegister)
{
    // Genera la instrucción FCVTNS para convertir un valor de punto flotante a entero
    instructions.Add($"FCVTNS {destRegister}, {srcRegister}");
}

   public void PushConstant(StackObject obj, object value)
{
    switch (obj.Type)
    {
        case StackObject.StackObjectType.Int:
            Mov(Register.X0, (int)value);
            Push(Register.X0);
            obj.Value = value;
            break;

        case StackObject.StackObjectType.Bool:
            obj.Value = (bool)value;
            Mov(Register.X0, (bool)value ? 1 : 0);
            Push(Register.X0);
            break;

        case StackObject.StackObjectType.Float:
            double floatValue = (double)value;
            string label = $"float_const_{floatValue.ToString().Replace(".", "_").Replace("-", "neg")}";
            stdLib.AddFloatConstant(label, floatValue);
            instructions.Add($"ADR x1, {label}");
            instructions.Add($"LDR d0, [x1]");              // ✅ CORRECTO
            instructions.Add($"STR d0, [SP, #-8]!");        // ✅ CORRECTO
            obj.Value = floatValue;
            break;


        case StackObject.StackObjectType.String:
            string strVal = (string)value;
            List<byte> stringArray = Utils.StringTo1ByteArray(strVal + "\0"); // asegura null-terminated

            Push(Register.HP); // Guarda dirección inicial
            foreach (var b in stringArray)
            {
                char ch = (char)b;
                string displayChar = ch switch
                {
                    '\n' => "\\n",
                    '\r' => "\\r",
                    '\t' => "\\t",
                    '\0' => "\\0",
                    _    => ch.ToString()
                };
                Comment($"Pushing char {b} to heap - ({displayChar})");
                Mov("w0", b);
                Strb("w0", Register.HP);
                Mov(Register.X0, 1);
                Add(Register.HP, Register.HP, Register.X0);
            }
            break;

        case StackObject.StackObjectType.Pointer:
            Mov(Register.X0, (int)value);
            Push(Register.X0);
            obj.Value = value;
            break;
    }

    PushObject(obj);
}

public string HandleEscapeSequences(string input)
{
    return input switch
    {
        "\\n" => "\n",
        "\\t" => "\t",
        "\\r" => "\r",
        "\\\\" => "\\",
        "\\'" => "'",
        "\\\"" => "\"",
        _ => input // Si no es una secuencia de escape conocida, devuelve el valor original
    };
}

public void AddObject(string name, StackObject obj)
    {
        if (symbolTable.ContainsKey(name))
        {
            throw new Exception($"La variable '{name}' ya está definida.");
        }

        symbolTable[name] = obj;
    }
public StackObject RuneObject()
{
    return new StackObject
    {
        Type = StackObject.StackObjectType.Rune,
        Length = 4, // Tamaño típico de un Rune en bytes
        Depth = GetStackCount()
    };
}

public int GetStackCount()
{
    // Implement logic to return the current stack count
    return stack.Count; // Replace 'stack' with the actual stack representation
}

public StackObject PointerObject()
{
    return new StackObject { Type = StackObject.StackObjectType.Pointer };
}

public string Lsl(int shiftAmount)
{
    return $"LSL #{shiftAmount}";
}

public void PopBoolean(string rd)
{
    // Extraer el objeto de la pila
    var obj = stack.Last();
    if (obj.Type != StackObject.StackObjectType.Bool)
    {
        throw new Exception("El objeto en la pila no es de tipo booleano");
    }

    // Eliminar el objeto de la pila
    stack.RemoveAt(stack.Count - 1);

    // Generar la instrucción para cargar el valor booleano en el registro
    instructions.Add($"LDR {rd}, [SP], #8");  // Cargar el valor y ajustar el puntero de la pila
}

    public StackObject PopObject(string rd)
    {
        var obj = stack.Last();
        stack.RemoveAt(stack.Count - 1);

        Pop(rd);
        return obj;
    }

    public StackObject IntObject()
    {
        return new StackObject
        {
            Type = StackObject.StackObjectType.Int,
            Length = 8,
            Depth = depth,
            Id = null
        };
    }

    public StackObject BoolObject()
    { 
        return new StackObject
        {
            Type = StackObject.StackObjectType.Bool,
            Length = 8,
            Depth = depth,
            Id = null
        };
    }
    public StackObject FloatObject()
    {
        return new StackObject
        {
            Type = StackObject.StackObjectType.Float,
            Length = 8,
            Depth = depth,
            Id = null
        };
        
    }

    public StackObject StringObject()
    {
        return new StackObject
        {
            Type = StackObject.StackObjectType.String,
            Length = 8,
            Depth = depth,
            Id = null
        };
    }

    public StackObject CloneObject(StackObject obj)
    {
        return new StackObject
        {
            Type = obj.Type,
            Length = obj.Length,
            Depth = obj.Depth,
            Id = obj.Id
        };
    }

    public StackObject TopObject()
    {
        return stack.Last();
    }

    //--Environment operations--//

    public void NewScope()
    {
        depth++;
    }

public int endScope()
{
    int byteOffset = 0;

    for (int i = stack.Count - 1; i >= 0; i--)
    {
        if (stack[i].Depth == depth)
        {
            byteOffset += stack[i].Length;
            stack.RemoveAt(i);
        }
        else
        {
            break;
        }
    }

    depth--;
    return byteOffset;
}


    public void TagObject(string id)
    {
        stack.Last().Id = id;
    }

    


    public void TypeObject(string id)
    {
        stack.Last().Id = id;
    }

public (int offset, StackObject obj) GetObject(string id)
    {
        int byteOffset = 0;
        for (int i = stack.Count - 1; i >= 0; i--)
        {
            if (stack[i].Id == id)
            {
                return (byteOffset, stack[i]);
            }
            byteOffset += stack[i].Length;
        }

        throw new Exception($"Object with id {id} not found");
    }

public void Mov(string register, long value)
{
    if (value <= int.MaxValue && value >= int.MinValue)
    {
        Mov(register, (int)value);
    }
    else
    {
        Movz(register, (ushort)(value & 0xFFFF), 0);
        Movk(register, (ushort)((value >> 16) & 0xFFFF), 16);
        Movk(register, (ushort)((value >> 32) & 0xFFFF), 32);
        Movk(register, (ushort)((value >> 48) & 0xFFFF), 48);
    }
}
public void Movz(string register, ushort value, int shift)
{
    instructions.Add($"MOVZ {register}, #{value}, LSL #{shift}");
}

public void Movk(string register, ushort value, int shift)
{
    instructions.Add($"MOVK {register}, #{value}, LSL #{shift}");
}



    public void Strb(string rs1, string rs2)
    {
        instructions.Add($"STRB {rs1}, [{rs2}]");
    }

    public void Strb(string rs1, string rs2, int offset)
    {
        instructions.Add($"STRB {rs1}, [{rs2}, #{offset}]");
    }

    public void Add(string rd, string rs1, string rs2)
    {
        instructions.Add($"ADD {rd}, {rs1}, {rs2}");
    }

    public void Add(string dest, string src, int imm)
{
    Add(dest, src, imm.ToString());
}

    public void Ldrb(string rd, string rs1)
    {
        instructions.Add($"LDRB {rd}, [{rs1}]");
    }

    

    public void Ldrb(string rd, string rs1, int offset)
    {
        instructions.Add($"LDRB {rd}, [{rs1}, #{offset}]");
    }


    public void Sub(string rd, string rs1, string rs2)
    {
        instructions.Add($"SUB {rd}, {rs1}, {rs2}");
    }   

    public void Mul(string rd, string rs1, string rs2)
    {
        instructions.Add($"MUL {rd}, {rs1}, {rs2}");
    }   

    public void Div(string rd, string rs1, string rs2)
    {
        instructions.Add($"SDIV {rd}, {rs1}, {rs2}");
    }

    public void Addi(string rd, string rs1, int imm)
    {
        instructions.Add($"ADDI {rd}, {rs1}, #{imm}");
    }

    public void Str(string rs1, string rs2, int offset = 0)
{
    if (offset == 0)
        instructions.Add($"STR {rs1}, [{rs2}]");
    else
        instructions.Add($"STR {rs1}, [{rs2}, #{offset}]");
}

   public void Ldr(string rd, string baseReg, int offset = 0)
{
    instructions.Add($"LDR {rd}, [{baseReg}, #{offset}]");
}

// Usar con desplazamiento en otro registro
public void Ldr(string rd, string baseReg, string offsetReg)
{
    instructions.Add($"LDR {rd}, [{baseReg}, {offsetReg}]");
}

    public void Mov(string rd, int rs)
    {
        instructions.Add($"MOV {rd}, #{rs}");
    }


// Para mover el contenido de un registro a otro registro
    public void Mov(string rd, string rs)
    {
        instructions.Add($"MOV {rd}, {rs}");
    }

    public void Fsub(string rd, string rs1, string rs2)
    {
        instructions.Add($"FSUB {rd}, {rs1}, {rs2}");
    }

    public void Fadd(string rd, string rs1, string rs2)
    {
        instructions.Add($"FADD {rd}, {rs1}, {rs2}");
    }

    public void Fmul(string rd, string rs1, string rs2)
    {
        instructions.Add($"FMUL {rd}, {rs1}, {rs2}");
    }

    public void Fdiv(string rd, string rs1, string rs2)
    {
        instructions.Add($"FDIV {rd}, {rs1}, {rs2}");
    }

    public void Fmov(string rd, string rs)
    {
        instructions.Add($"FMOV {rd}, {rs}");
    }

    public void Cmp(string rs1, string rs2)
    {
        instructions.Add($"CMP {rs1}, {rs2}");
    }

    public void Cmp(string reg, int imm)
{
    Cmp(reg, imm.ToString());
}


    public void Ret()
    {
        instructions.Add($"RET");
    }

    public void Blt(string label)
    {
        instructions.Add($"\tBLT {label}");
    }

   public void Bl(string label)
{
    instructions.Add($"BL {label}");
}

    public void Ble(string label)
    {
        instructions.Add($"\tBLE {label}");
    }

    public void Bgt(string label)
    {
        instructions.Add($"\tBGT {label}");
    }

    public void Bge(string label)
    {
        instructions.Add($"\tBGE {label}");
    }

    public void Beq(string label)
    {
        instructions.Add($"\tBEQ {label}");
    }

    public void Bne(string label)
    {
        instructions.Add($"\tBNE {label}");
    }

    public void B(string label)
    {
        instructions.Add($"\tB {label}");
    }

    public void Stp(string rt1, string rt2, string rn, int imm)
{
    instructions.Add($"\tSTP {rt1}, {rt2}, [{rn}, #{imm}]!");
}

public void Ldp(string rt1, string rt2, string rn, int imm)
{
    instructions.Add($"\tLDP {rt1}, {rt2}, [{rn}], #{imm}");
}



    public void Cbz(string rs, string label)
    {
        instructions.Add($"CBZ {rs}, {label}");
    }

    public void Cbnz(string rs, string label)
{
    instructions.Add($"CBNZ {rs}, {label}");
}

public void Call(string functionName)
{
    instructions.Add($"BL {functionName}");
}



    public void Push(string rs)
    {
        instructions.Add($"STR {rs}, [SP, #-8]!");
    }

    public void Pop(string rd)
    {
        instructions.Add($"LDR {rd}, [SP], #8");
    }

    public void Scvtf(string rd, string rs)
    {
        instructions.Add($"SCVTF {rd}, {rs}");
    }


    public void Svc()
    {
        instructions.Add($"SVC 0");
    }

    public void Fstr(string rd, string address)
{
    instructions.Add($"STR {rd}, [{address}]");
}

public void Fldr(string rd, string rn)
{
    instructions.Add($"LDR {rd}, [{rn}]"); // CORRECTO para ARM64
}

public void LdrFloat(string rd, string rn)
{
    instructions.Add($"LDR {rd}, [{rn}]");
}


    

    public void EndProgram()
    {
        Mov(Register.X0, 0);
        Mov(Register.X8, 93); // syscall number for exit
        Svc(); // make syscall
    }

    public void PrintInteger(string rs)
    {
        stdLib.Use("print_integer");
        instructions.Add($"MOV x0, {rs}");
        instructions.Add($"BL print_integer");
    }

    public void PrintString(string rs)
{
    // Usar la librería estándar para print_string
    stdLib.Use("print_string");

    // Convertir la cadena a un array de bytes, asegurando que termine con '\0'
    List<byte> stringArray = Utils.StringTo1ByteArray(rs + "\0");

    // Reservar espacio en el heap para la cadena (en la dirección apuntada por x10)
    instructions.Add($"ADR x10, heap");  // Asumiendo que x10 es el puntero al heap

    // Almacenar cada byte de la cadena en el heap
    foreach (var byteValue in stringArray)
    {
        instructions.Add($"MOV w0, #{byteValue}");
        instructions.Add($"STRB w0, [x10], #1");  // Almacena cada byte y avanza la dirección
    }

    // Ahora la dirección de la cadena está en x10, pasarla a X0 para la función print_string
    instructions.Add($"MOV X0, x10");

    // Llamar a la función print_string
    instructions.Add("BL print_string");
}


    public void PrintFloat()
    {
        stdLib.Use("print_integer");
        stdLib.Use("print_double");
        instructions.Add($"BL print_double");
    }

    public void Comment(string comment)
    {
        instructions.Add($"// {comment}");
    }

    public override string ToString()
{
    var sb = new StringBuilder();
    sb.AppendLine(".data");
    sb.AppendLine("heap: .space 4096");
    sb.AppendLine(".text");
    sb.AppendLine(".global _start");
    sb.AppendLine("_start:");
    sb.AppendLine(" adr x10, heap");

    foreach (var instruction in instructions)
    {
        sb.AppendLine(instruction);
    }

    sb.AppendLine(stdLib.GetFunctionDefinitions());

    return sb.ToString();
}

    public void Label(string label)
{
    instructions.Add($"{label}:");
}

    


    
}
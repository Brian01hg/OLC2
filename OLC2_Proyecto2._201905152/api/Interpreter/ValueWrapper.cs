public abstract record ValueWrapper
{
    public abstract string Tipo { get; }
}

public record IntValue(int Value) : ValueWrapper
{
    public override string ToString() => Value.ToString();
    public override string Tipo => "int";
}

public record FloatValue(float Value) : ValueWrapper
{
    public override string ToString() => Value.ToString();
    public override string Tipo => "float64";
}

public record StringValue(string Value) : ValueWrapper
{
    public override string ToString() => Value;
    public override string Tipo => "string";
}

public record BoolValue(bool Value) : ValueWrapper
{
    public override string ToString() => Value.ToString();
    public override string Tipo => "bool";
}

public record RuneValue(char Value) : ValueWrapper
{
    public override string ToString() => Value.ToString();
    public override string Tipo => "char";
}

public record NilValue : ValueWrapper
{
    public override string ToString() => "nil";
    public override string Tipo => "nil";
}

public record VoidValue : ValueWrapper
{
    public override string ToString() => "void";
    public override string Tipo => "void";
}

public record FunctionValue(Invocable invocable, string name) : ValueWrapper
{
    public override string ToString() => name;
    public override string Tipo => "function";
}

public record InstanceValue(Instance instance) : ValueWrapper
{
    public override string ToString() => instance.ToString();
    public override string Tipo => "instance";
}

public record ClassValue(LanguageClass languageClass) : ValueWrapper
{
    public override string ToString() => languageClass.Name;
    public override string Tipo => "class";
}

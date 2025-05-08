using analyzer;

public class LanguageClass : Invocable
{

    public string Name { get; set; }
    public Dictionary<string, LanguageParser.VarDclContext> Props { get; set; }
    public Dictionary<string, ForeignFunction> Methods { get; set; }

    public LanguageClass(string name
      , Dictionary<string, LanguageParser.VarDclContext> props
      , Dictionary<string, ForeignFunction> methods)
    {
        Name = name;
        Props = props;
        Methods = methods;
    }

    public ForeignFunction GetMethod(string name)
    {
        if (Methods.ContainsKey(name))
        {
            return Methods[name];
        }
        return null;
    }

    public int Arity()
    {
        var constructor = GetMethod("constructor");
        if (constructor != null)
        {
            return constructor.Arity();
        }
        return 0;
    }

    public ValueWrapper Invoke(List<ValueWrapper> args, InterpreterVisitor visitor)
    {
        var newInstance = new Instance(this, instance => {
            var output = "{";
            foreach (var prop in instance.Properties)
            {
                output += prop.Key + ": " + prop.Value.ToString() + ", ";
            }
            if (output.Length > 1)
            {
                output = output.TrimEnd(',', ' ');
            }
            output += "}";
            return output;
        });
        foreach (var prop in Props)
        {
            var name = prop.Key;
            var value = prop.Value;
            if (value.expr() != null)
            {
                var valueWrapper = visitor.Visit(value.expr());
                newInstance.Set(name, valueWrapper);
            }
            else
            {
                newInstance.Set(name, new NilValue());
            }
        }
        var constructor = GetMethod("constructor");
        if (constructor != null)
        {
            constructor.Bind(newInstance).Invoke(args, visitor);
        }
        return new InstanceValue(newInstance);
    }

}

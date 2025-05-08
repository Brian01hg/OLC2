using analyzer;
using System;
using System.Collections.Generic;

public class LanguageBidimensional : Invocable
{
    public Dictionary<string, LanguageParser.VarDclContext> Props { get; set; }
    public Dictionary<string, ForeignFunction> Methods { get; set; }
    
    public LanguageClass equivalentClass;

    public LanguageBidimensional()
    {
        Props = new Dictionary<string, LanguageParser.VarDclContext>();
        Methods = new Dictionary<string, ForeignFunction>();
        equivalentClass = new LanguageClass("[][]", Props, Methods);
    }

    public int Arity()
    {
        return 100;
    }

    public ValueWrapper Invoke(List<ValueWrapper> args, InterpreterVisitor visitor)
    {
        var newInstance = new Instance(equivalentClass, instance => {
            var output = "{";
            foreach (var prop in instance.Properties)
            {
                output += prop.Value.ToString() + ",";
            }
            if (output.Length > 1)
            {
                output = output.TrimEnd(',');
            }
            output += "}";
            Console.WriteLine("Output: " + output);
            return output;
        });

        for (int i = 0; i < args.Count; i++)
        {
            if (args[i] is InstanceValue rowInstance)
            {
                newInstance.Set(i.ToString(), rowInstance);
            }
            else
            {
                throw new SemanticError("Error: Una matriz debe contener arrays como filas", null);
            }
        }
        return new InstanceValue(newInstance);
    }
}

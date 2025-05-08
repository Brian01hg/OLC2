using Microsoft.AspNetCore.Mvc.ViewFeatures;

public class Embeded
{
    public static void Generate(Environment env)
    {
        //Declara 'strconv' como un espacio de nombres para la función Atoi
        env.Declare("strconv.Atoi", new FunctionValue(new AtoiEmbeded(), "strconv.Atoi"), null);
        env.Declare("strconv.ParseFloat", new FunctionValue(new ParseFloatEmbeded(), "strconv.ParseFloat"), null);
        env.Declare("reflect.TypeOf", new FunctionValue(new TypeOfEmbeded(), "reflect.TypeOf"), null);  

    }
}

public class AtoiEmbeded : Invocable
{
    public int Arity()
    {
        return 1;
    }

    public ValueWrapper Invoke(List<ValueWrapper> args, InterpreterVisitor visitor)
    {
        if (args[0] is StringValue str)
        {
            Console.WriteLine("AtoiEmbeded: " + str.Value);
            if (str.Value.Contains("."))
            {
                throw new SemanticError("strconv.Atoi no puede convertir números decimales", null);
            }
            if (int.TryParse(str.Value, out int result))
            {
                return new IntValue(result);
            }
            throw new SemanticError("No se pudo convertir el string a int", null);
        }
        throw new SemanticError("strconv.Atoi solo acepta un argumento de tipo string", null);
    }

}

public class ParseFloatEmbeded : Invocable
{
    public int Arity()
    {
        return 1; 
    }

    public ValueWrapper Invoke(List<ValueWrapper> args, InterpreterVisitor visitor)
    {
        if (args[0] is StringValue str)
        {
            Console.WriteLine("ParseFloatEmbeded: " + str.Value);

            // Asegurar que el número tenga punto decimal si es un entero
            string floatValue = str.Value.Contains(".") ? str.Value : str.Value + ".0";

            if (float.TryParse(floatValue, out float result))
            {
                return new FloatValue(result);
            }

            throw new SemanticError("No se pudo convertir el string a float", null);
        }

        throw new SemanticError("strconv.ParseFloat solo acepta un argumento de tipo string", null);
    }
}

public class TypeOfEmbeded : Invocable
{
    public int Arity()
    {
        return 1; // Esperamos un solo argumento
    }

    public ValueWrapper Invoke(List<ValueWrapper> args, InterpreterVisitor visitor)
    {
        if (args.Count != 1)
        {
            throw new SemanticError("reflect.TypeOf espera un solo argumento", null);
        }

        // Obtenemos el valor del primer argumento
        var value = args[0];

        // Verificamos el tipo y devolvemos el tipo como string
        if (value is IntValue)
        {
            return new StringValue("int");
        }
        else if (value is FloatValue)
        {
            return new StringValue("float64");
        }
        else if (value is StringValue)
        {
            return new StringValue("string");
        }
        else if (value is BoolValue)
        {
            return new StringValue("bool");
        }
        else if (value is RuneValue)
        {
            return new StringValue("rune");
        }
        else if (value is InstanceValue instanceValue)
        {
            var instance = instanceValue.instance;
            if (instance.languageclass.Name == "[]")
            {
                return new StringValue("[]" + GetElementType(instance));
            }
            return new StringValue(instance.languageclass.Name);
        }
        else
        {
            throw new SemanticError("Tipo de elemento no soportado", null);
        }
    }

    private string GetElementType(Instance instance)
    {
        var firstProperty = instance.Properties.FirstOrDefault();
        if (firstProperty.Value is IntValue)
        {
            return "int";
        }
        else if (firstProperty.Value is FloatValue)
        {
            return "float64";
        }
        else if (firstProperty.Value is StringValue)
        {
            return "string";
        }
        else
        {
            throw new SemanticError("Tipo de elemento no soportado", null);
        }
    }
}


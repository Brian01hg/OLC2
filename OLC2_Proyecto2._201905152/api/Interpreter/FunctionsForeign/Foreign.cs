using analyzer;

public class ForeignFunction : Invocable
{
    private Environment clousure;
    private LanguageParser.FuncDclContext context;

    public ForeignFunction(Environment clousure, LanguageParser.FuncDclContext context)
    {
        this.clousure = clousure;
        this.context = context;
    }

    public int Arity()
    {
        if (context.@params() == null)
        {
            return 0;
        }
        return context.@params().param().Length;
    }

    public ValueWrapper Invoke(List<ValueWrapper> args, InterpreterVisitor visitor)
    {
        // Crear un nuevo entorno para la funcion
        var newEnviroment = new Environment(clousure);
        var beforeCallEnviroment = visitor.currentEnvironment;
        // Setear el nuevo entorno para tener acceso a las variables de la funcion
        visitor.currentEnvironment = newEnviroment;

        if (context.@params() != null)
        {
            for (int i = 0; i < context.@params().param().Length; i++)
            {

                string paramName = context.@params().param(i).ID().GetText();
                string expectedType = context.@params().param(i).Tipo().GetText();
                ValueWrapper value = args[i];

                // Si el tipo de dato no es valido para el parametro
                if (!IsValidType(value, expectedType))
                {
                    throw new SemanticError($"El tipo de dato {value.GetType()} no es valido para el parametro {paramName}", context.@params().param(i).ID().Symbol);
                }
                newEnviroment.Declare(paramName, value, null);
            }
        }
        try
        {
            // Para recorcer las declaraciones de la funcion
            foreach (var stmt in context.dcl())
            {
                // Visitar cada declaracion
                visitor.Visit(stmt);
            }
        }
        catch (ReturnException returnValue)
        {
            // Restaurar el entorno anterior
            visitor.currentEnvironment = beforeCallEnviroment;
            // Retornar el valor de retorno

            string expectedReturnType = context.Tipo()?.GetText();

            if (expectedReturnType != null && !IsValidType(returnValue.Value, expectedReturnType))
            {
                throw new SemanticError($"El tipo de dato {returnValue.Value.GetType()} no es valido para el tipo de retorno {expectedReturnType}", context.ID().Symbol);
            }

            return returnValue.Value;
        }

        visitor.currentEnvironment = beforeCallEnviroment;
        return visitor.defaultValue;
    }

    // Metodo para validar el tipo de dato
    private bool IsValidType(ValueWrapper value, string expectedType)
    {
        return expectedType switch
        {
            "int" => value is IntValue,
            "float64" => value is FloatValue,
            "string" => value is StringValue,
            "bool" => value is BoolValue,
            "rune" => value is RuneValue,
            _ => false
        };
    }

    // ------------------------------ 
    public ForeignFunction Bind(Instance instance)
    {
        var hiddenEnv = new Environment(clousure);
        hiddenEnv.Declare("this", new InstanceValue(instance), null);
        return new ForeignFunction(hiddenEnv, context);
    }

}
public class Environment
{
    // TODO: parent environment
    public Dictionary<string, ValueWrapper> variables = new Dictionary<string, ValueWrapper>();
    public Environment? parent;

    public Environment(Environment? parent)
    {
        this.parent = parent;
    }

    public ValueWrapper Get(string id, Antlr4.Runtime.IToken token)
    {
        if (variables.ContainsKey(id))
        {
            return variables[id];
        }
        if (parent != null)
        {
            return parent.Get(id, token);
        }
        throw new SemanticError("Variable " + id + " not found", token);
    }
    
    public void Declare(string id, ValueWrapper value, string type = null, Antlr4.Runtime.IToken? token = null)
    {
        if (variables.ContainsKey(id))
        {
            if (token != null)
            {
                throw new SemanticError("Variable " + id + " already declared", token);
            }
        }
        else
        {
            if (type != null)
            {
                variables[id] = value;
            }
            else
            {
                variables[id] = value;
            }
        }
    }

    public ValueWrapper Assign(string id, ValueWrapper value, Antlr4.Runtime.IToken token)
{
    if (variables.ContainsKey(id))
    {
        variables[id] = value;
        return value;
    }
    else if (parent != null)
    {
        return parent.Assign(id, value, token);
    }
    else
    {
        throw new SemanticError("Variable " + id + " not found", token);
    }
}


    public void Set(string id, ValueWrapper value)
{
    // Este método intentará hacer asignación recursiva; si no existe lanza excepción
    Assign(id, value, null);
}



     public List<SymbolTable> GetSymbolTable(string scope)
    {
        List<SymbolTable> table = new List<SymbolTable>();

        foreach (var variable in variables)
        {
             table.Add(new SymbolTable(variable.Key, "Variable", variable.Value.Tipo.ToString(), scope, 0, 0));
        }

        if (parent != null)
        {
            table.AddRange(parent.GetSymbolTable(scope));
        }
        return table;
    }
}
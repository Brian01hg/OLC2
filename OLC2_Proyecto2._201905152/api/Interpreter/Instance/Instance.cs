public class Instance
{

    public LanguageClass languageclass;
    public Dictionary<string, ValueWrapper> Properties;

    public Func<string> ToString;

    public Instance(LanguageClass languageclass, Func<Instance, string> toString)
    {
        this.languageclass = languageclass;
        // Mini Entorno para las propiedades de la clase
        Properties = new Dictionary<string, ValueWrapper>();
        ToString = () => toString(this);
    }
    
    public void Set(string name, ValueWrapper value)
    {
        Properties[name] = value;
    }

    public ValueWrapper Get(string name, Antlr4.Runtime.IToken token)
    {
        if (Properties.ContainsKey(name))
        {
            return Properties[name];
        }
        var method = languageclass.GetMethod(name);
        if (method != null)
        {
            return new FunctionValue(method.Bind(this), name);
        }
        throw new SemanticError($"La propiedad {name} no existe en la clase", token);
    } 
}

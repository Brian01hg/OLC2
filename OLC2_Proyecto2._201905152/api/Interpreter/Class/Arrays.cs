using analyzer;
public class LanguageArray : Invocable {
    public Dictionary<string, LanguageParser.VarDclContext> Props { get; set; }
    public Dictionary<string, ForeignFunction> Methods { get; set; }
    
    private LanguageClass equivalentClass;

    public LanguageArray(){
        Props = new Dictionary<string, LanguageParser.VarDclContext>();
        Methods = new Dictionary<string, ForeignFunction>();
        equivalentClass = new LanguageClass("[]", Props, Methods);
    }

    public int Arity(){
        return 100;
    }

    public ValueWrapper Invoke(List<ValueWrapper> args, InterpreterVisitor visitor){
        var newInstance = new Instance(equivalentClass, instance => {
            var output = "{";
            foreach(var prop in instance.Properties){
                output += prop.Value.ToString() + ",";
            }
            if(output.Length > 1){
                output = output.TrimEnd(',');
            }
            output += "}";
            return output;
        });
        for(int i = 0;  i < args.Count; i++){
            var name = i.ToString();
            var value = args[i];
            newInstance.Set(name, value);
        }
        return new InstanceValue(newInstance);
    }
}
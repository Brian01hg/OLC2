public class SymbolTable
{
    public string ID { get; set; }
    public string SymbolType { get; set; }
    public string DataType { get; set; }
    public string Scope { get; set; }
    public int Line { get; set; }
    public int Column { get; set; }

    public SymbolTable(string id, string symbolType, string dataType, string scope, int line, int column)
    {
        ID = id;
        SymbolType = symbolType;
        DataType = dataType;
        Scope = scope;
    }
}
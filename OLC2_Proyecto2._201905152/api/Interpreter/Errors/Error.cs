using System;
using System.IO;
using Antlr4.Runtime;
using Antlr4.Runtime.Misc;

public class SemanticError : Exception
{
    public string message;
    private Antlr4.Runtime.IToken token;
    public SemanticError(string message, Antlr4.Runtime.IToken token)
    {
        this.message = message;
        this.token = token; 
    }

    public override string Message
    {
        get
        {
            return message + " en la linea " + token.Line + " columna " + token.Column;
        }
    }
}

public class LexicoErrorListener : BaseErrorListener, IAntlrErrorListener<int>
{
    public void SyntaxError(TextWriter output, IRecognizer recognizer, int offendingSymbol, int line, int charPositionInLine, string msg, RecognitionException e)
    {
        throw new ParseCanceledException($"Error lexico: en la linea {line} columna {charPositionInLine}");
    }
}

public class SintacticErrorListener : BaseErrorListener
{
    public override void SyntaxError(TextWriter output, IRecognizer recognizer, IToken offendingSymbol, int line, int charPositionInLine, string msg, RecognitionException e)
    {
        string error = $"Error sintáctico: '{offendingSymbol.Text}' inesperado en la línea {line}, columna {charPositionInLine}";
        Console.WriteLine(error);
        throw new ParseCanceledException(error);
    }
}
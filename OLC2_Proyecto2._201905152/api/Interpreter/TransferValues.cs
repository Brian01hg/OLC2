
public class BreakException : Exception
{
    public BreakException() : base("Break statement")
    {
    }
}

// Continue
public class ContinueException : Exception
{
    public ContinueException() : base("Continue statement")
    {
    }
}

// Return
public class ReturnException : Exception
{
    public ValueWrapper Value { get; }

    public ReturnException(ValueWrapper value) : base("Return statement")
    {
        Value = value;
    }
}
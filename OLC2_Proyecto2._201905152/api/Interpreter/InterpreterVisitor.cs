using System.Runtime.InteropServices;
using System.Windows.Markup;
using analyzer;
using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Tree;

public class InterpreterVisitor : LanguageBaseVisitor<ValueWrapper>
{
   public ValueWrapper defaultValue = new VoidValue();
    public string output = "";
    public Environment currentEnvironment = new Environment(null);
    private ValueWrapper currentSwitchValue;

    public InterpreterVisitor()
    {
        currentEnvironment = new Environment(null);
        Embeded.Generate(currentEnvironment);
    }
    
    // Función para verificar si un tipo es compatible con un valor
    private bool Iscomtabible(string type, ValueWrapper value)
    {
        return type switch
        {
            "int" => value is IntValue,
            "float64" => value is FloatValue || value is IntValue,
            "string" => value is StringValue,
            "bool" => value is BoolValue,
            "rune" => value is RuneValue,
            _ => value is InstanceValue
        };
    }

    // Manejar setencias de escapes
    private string ProcessEscapeSequences(string input)
    {
        return input
            .Replace("\\n", "\n")
            .Replace("\\r", "\r")
            .Replace("\\t", "\t")
            .Replace("\\\"", "\"")
            .Replace("\\\\", "\\");
    }

    // Funcion defaultValue 
    private ValueWrapper valueOrDefault(string type)
    {
        return type switch
        {
            "int" => new IntValue(0),
            "float64" => new FloatValue(0),
            "string" => new StringValue(""),
            "bool" => new BoolValue(false),
            "rune" => new RuneValue('\0'),
            _ => new VoidValue()
        };
    }

    // Inferir tipo de dato
    private string InferType(ValueWrapper value)
    {
        return value switch
        {
            IntValue i => "int",
            FloatValue f => "float64",
            StringValue s => "string",
            BoolValue b => "bool",
            RuneValue r => "rune",
            InstanceValue instance when instance.instance.languageclass.Name == "[]" 
            => "array",
            InstanceValue instance when instance.instance.languageclass.Name == "[][]"
            => "bidimensional",
            InstanceValue instance => instance.instance.languageclass.Name,
            _ => throw new SemanticError("Tipo no soportado !", null)
        };
    }

    // ------------------- Visit Methods -------------------

    // VisitProgram
    public override ValueWrapper VisitProgram(LanguageParser.ProgramContext context)
    {
        foreach (var dcl in context.dcl())
        {
            Visit(dcl);
        }
        return defaultValue;
    }

    // VisitVarDcl
    public override ValueWrapper VisitVarDcl(LanguageParser.VarDclContext context)
    {
        string id = context.ID().GetText();
        ValueWrapper value;

        if (context.expr() != null && context.Tipo() == null)
        {
            value = Visit(context.expr());
            string tipoInferido = InferType(value);
            currentEnvironment.Declare(id, value, tipoInferido, context.Start);
        }
        else
        {
            string tipo = context.Tipo().GetText(); 
            if (tipo.StartsWith("[]"))
            {
                value = new LanguageArray().Invoke(new List<ValueWrapper>(), this);
            }
            else if (context.expr() != null)
            {
                value = Visit(context.expr());

                if (!(value is InstanceValue) && !Iscomtabible(tipo, value))
                {
                    throw new SemanticError($"Error: No se puede asignar {value.GetType()} a {tipo} | ", context.Start);
                }
            }
            else
            {
                value = valueOrDefault(tipo);
            }

            currentEnvironment.Declare(id, value, tipo, context.Start);
        }
        return defaultValue;
    }


    // VisitExprStmt
    public override ValueWrapper VisitExprStmt(LanguageParser.ExprStmtContext context)
    {
        return Visit(context.expr());
    }

    // VisitPrintStmt
    public override ValueWrapper VisitPrintStmt(LanguageParser.PrintStmtContext context)
    {
        if (context.exprList() == null)
        {
            output += "\n";
            return defaultValue;
        }

        var args = context.exprList().expr();
        List<string> values = new List<string>();

        foreach (var expr in args)
        {
            Console.WriteLine("Expresión literal: " + expr.GetText());
            
            ValueWrapper value = Visit(expr);
            if (value is StringValue strVal)
            {
                values.Add(ProcessEscapeSequences(strVal.Value));
            }
            else
            {
                values.Add(value.ToString());
            }
        }
        output += string.Join(" ", values) + "\n";
        return defaultValue;
    }


    // VisitIdentifier
    public override ValueWrapper VisitIdentifier(LanguageParser.IdentifierContext context)
    {
        string id = context.ID().GetText();
        return currentEnvironment.Get(id, context.Start);
    }

    // VisitParens
    public override ValueWrapper VisitParens(LanguageParser.ParensContext context)
    {
        return Visit(context.expr());
    }

    // VisitNegate
    public override ValueWrapper VisitNegate(LanguageParser.NegateContext context)
    {
        ValueWrapper value = Visit(context.expr());
        return value switch
        {
            IntValue i => new IntValue(-i.Value),
            FloatValue f => new FloatValue(-f.Value),
            _ => throw new SemanticError("No se pudo negar el valor", context.Start)
        };
    }

    // VisitNumber
    public override ValueWrapper VisitInt(LanguageParser.IntContext context)
    {
        return new IntValue(int.Parse(context.INT().GetText()));
    }

    // VisitNil
    public override ValueWrapper VisitNilExpr(LanguageParser.NilExprContext context)
    {
        return new NilValue();
    }

    public override ValueWrapper VisitNot(LanguageParser.NotContext context)
    {
        ValueWrapper value = Visit(context.expr());
        return value switch
        {
            BoolValue b => new BoolValue(!b.Value),
            _ => throw new SemanticError("Operación no soportada", null)
        };
    }

    // VisitAsignacion
    public override ValueWrapper VisitAssign(LanguageParser.AssignContext context)
    {
        var asignee = context.expr(0);
        ValueWrapper value = Visit(context.expr(1));

        string id;
        if (asignee is LanguageParser.IdentifierContext idContext)
        {
            id = idContext.ID().GetText();
        }
        else if (asignee is LanguageParser.CalleeContext calleContext)
        {
            ValueWrapper calle = Visit(calleContext.expr());

            for (int i = 0; i < calleContext.call().Length; i++)
            {
                var action = calleContext.call(i);

                if (i == calleContext.call().Length - 1)
                {
                    if (action is LanguageParser.GetContext propertyAccess)
                    {
                        if (calle is InstanceValue instanceValue)
                        {
                            var instance = instanceValue.instance;
                            var property = propertyAccess.ID().GetText();
                            instance.Set(property, value);
                        }
                        else
                        {
                            throw new SemanticError("No se pudo acceder a la propiedad", context.Start);
                        }
                    }
                    else if(action is LanguageParser.ArrayAccessContext arrayAccess)
                        {
                            if (calle is InstanceValue instanceValue)
                            {
                                var index = Visit(arrayAccess.expr());
                                if(index is IntValue intValue)
                                {   
                                    instanceValue.instance.Set(intValue.Value.ToString(), value);
                                }else{
                                    throw new SemanticError("Error: No se puedo acceder a la propiedad array", context.Start);  
                                }
                            }
                            else
                            {
                                throw new SemanticError("Error: Al acceder al array", context.Start);
                            }
                        }
                    else if(action is LanguageParser.ArrayAccessBidimensionalContext arrayAccesBidimensional){
                        if (calle is InstanceValue instanceValue)
                        {
                            var index = Visit(arrayAccesBidimensional.expr(0));
                            var index2 = Visit(arrayAccesBidimensional.expr(1));
                            if(index is IntValue intValue && index2 is IntValue intValue2)
                            {   
                                instanceValue.instance.Set(intValue.Value.ToString(), index);
                                if(calle is InstanceValue instanceValue2)
                                {
                                    instanceValue2.instance.Set(intValue2.Value.ToString(), index2);
                                }else{
                                    throw new SemanticError("Error: No se puedo acceder a la propiedad array", context.Start);  
                                }
                            }else{
                                throw new SemanticError("Error: No se puedo acceder a la propiedad array", context.Start);  
                            }
                        }
                        else
                        {
                            throw new SemanticError("Error: Al acceder al array", context.Start);
                        }
                    }
                    else
                    {
                        throw new SemanticError("Error: No se puede asignar", context.Start);
                    }
                }
                /// ------------------
                if (action is LanguageParser.FuncCallContext call)
                {
                    if (calle is FunctionValue functionValue)
                    {
                        calle = VisitCall(functionValue.invocable, call.args());
                    }
                    else
                    {
                        throw new SemanticError("No se pudo llamar la funcion", context.Start);
                    }
                }
                else if (action is LanguageParser.GetContext propertyAccess)
                {
                    if (calle is InstanceValue instanceValue)
                    {
                        calle = instanceValue.instance.Get(propertyAccess.ID().GetText(), propertyAccess.Start);
                    }
                    else
                    {
                        throw new SemanticError("No se pudo acceder a la propiedad", context.Start);
                    }
                }
                else if(action is LanguageParser.ArrayAccessContext arrayAccess)
                {
                    if (calle is InstanceValue instanceValue)
                    {
                        var index = Visit(arrayAccess.expr());
                        if(index is IntValue intValue)
                        {   
                            calle = instanceValue.instance.Get(intValue.Value.ToString(), arrayAccess.Start);
                        }else{
                            throw new SemanticError("Error: No se puedo acceder a la propiedad array", context.Start);  
                        }
                    }
                    else
                    {
                        throw new SemanticError("Error: Al acceder al array", context.Start);
                    }
                }
                else if(action is LanguageParser.ArrayAccessBidimensionalContext arrayAccesBidimensional){
                    if (calle is InstanceValue instanceValue)
                    {
                        var index = Visit(arrayAccesBidimensional.expr(0));
                        var index2 = Visit(arrayAccesBidimensional.expr(1));
                        if(index is IntValue intValue && index2 is IntValue intValue2)
                        {   
                            calle = instanceValue.instance.Get(intValue.Value.ToString(), arrayAccesBidimensional.Start);
                            if(calle is InstanceValue instanceValue2)
                            {
                                calle = instanceValue2.instance.Get(intValue2.Value.ToString(), arrayAccesBidimensional.Start);
                            }
                        }else{
                            throw new SemanticError("Error: No se puedo acceder a la propiedad array", context.Start);  
                        }
                    }
                    else
                    {
                        throw new SemanticError("Error: Al acceder al array", context.Start);
                    }
                }
            }
            return calle;
        }
        else
        {
            throw new SemanticError("Error: No se puede asignar", context.Start);
        }

        // Obtengo el valor actual en el entorno
        ValueWrapper varValue = currentEnvironment.Get(id, context.Start);

        // Determinar el tipo de la variable almacenada en el entorno
        string varType = varValue switch
        {
            IntValue => "int",
            FloatValue => "float64",
            StringValue => "string",
            BoolValue => "bool",
            RuneValue => "rune",
            InstanceValue => "instance",
            _ => throw new SemanticError("Tipo no soportado :D", context.Start)
        };
        if (!Iscomtabible(varType, value))
        {
            throw new SemanticError($"Error: No se puede asignar {value.GetType()} a {varType}", context.Start);
        }

        // Asigno el nuevo valor
        currentEnvironment.Assign(id, value, context.Start);
        return defaultValue;
    }



    public override ValueWrapper VisitAddSub(LanguageParser.AddSubContext context)
    {
        ValueWrapper left = Visit(context.GetChild(0));
        ValueWrapper right = Visit(context.expr(1));
        var op = context.op.Text;

        return (left, right, op) switch
        {
            // Suma
            (IntValue l, IntValue r, "+") => new IntValue(l.Value + r.Value),
            (IntValue l, FloatValue r, "+") => new FloatValue(l.Value + r.Value),
            (FloatValue l, FloatValue r, "+") => new FloatValue(l.Value + r.Value),
            (FloatValue l, IntValue r, "+") => new FloatValue(l.Value + r.Value),
            (StringValue l, StringValue r, "+") => new StringValue(l.Value + r.Value),
            (StringValue l, IntValue r, "+") => new StringValue(l.Value + r.Value.ToString()),
            (IntValue l, StringValue r, "+") => new StringValue(l.Value.ToString() + r.Value),
            (StringValue l, FloatValue r, "+") => new StringValue(l.Value + r.Value.ToString()),
            (FloatValue l, StringValue r, "+") => new StringValue(l.Value.ToString() + r.Value),
            (StringValue l, RuneValue r, "+") => new StringValue(l.Value + r.Value.ToString()),
            (RuneValue l, StringValue r, "+") => new StringValue(l.Value.ToString() + r.Value),
            (RuneValue l, RuneValue r, "+") => new StringValue(l.Value.ToString() + r.Value.ToString()),
            // Resta
            (IntValue l, IntValue r, "-") => new IntValue(l.Value - r.Value),
            (IntValue l, FloatValue r, "-") => new FloatValue(l.Value - r.Value),
            (FloatValue l, FloatValue r, "-") => new FloatValue(l.Value - r.Value),
            (FloatValue l, IntValue r, "-") => new FloatValue(l.Value - r.Value),
            // Concatenación
            _ => throw new SemanticError("No se puede realizar la operación", context.Start)
        };
    }

    

    // VisitMulDiv
    public override ValueWrapper VisitMulDiv(LanguageParser.MulDivContext context)
    {
        ValueWrapper left = Visit(context.expr(0));
        ValueWrapper right = Visit(context.expr(1));
        var op = context.op.Text;

        return (left, right, op) switch
        {
            // Multiplicación
            (IntValue l, IntValue r, "*") => new IntValue(l.Value * r.Value),
            (IntValue l, FloatValue r, "*") => new FloatValue(l.Value * r.Value),
            (FloatValue l, FloatValue r, "*") => new FloatValue(l.Value * r.Value),
            (FloatValue l, IntValue r, "*") => new FloatValue(l.Value * r.Value),
            (StringValue l, IntValue r, "*") => new StringValue(l.Value + new string(' ', r.Value)),
            (IntValue l, StringValue r, "*") => new StringValue(new string(' ', l.Value) + r.Value),
            (StringValue l, FloatValue r, "*") => new StringValue(l.Value + new string(' ', (int)r.Value)),
            (FloatValue l, StringValue r, "*") => new StringValue(new string(' ', (int)l.Value) + r.Value),
            (RuneValue l, IntValue r, "*") => new StringValue(l.Value.ToString() + new string(' ', r.Value)),
            (IntValue l, RuneValue r, "*") => new StringValue(new string(' ', l.Value) + r.Value.ToString()),

            // División
            (IntValue l, IntValue r, "/") => r.Value != 0 ? new IntValue(l.Value / r.Value) : throw new SemanticError("Error: División por cero", context.Start),
            (IntValue l, FloatValue r, "/") => r.Value != 0 ? new FloatValue(l.Value / r.Value) : throw new SemanticError("Error: División por cero", context.Start),
            (FloatValue l, FloatValue r, "/") => r.Value != 0 ? new FloatValue(l.Value / r.Value) : throw new SemanticError("Error: División por cero", context.Start),
            (FloatValue l, IntValue r, "/") => r.Value != 0 ? new FloatValue(l.Value / r.Value) : throw new SemanticError("Error: División por cero", context.Start),
            
            // División entera
            // Modulo
            (IntValue l, IntValue r, "%") => new IntValue(l.Value % r.Value),
            _ => throw new SemanticError("No se puede realizar la operación", context.Start)
        };

    }

    // VisitRelational
    public override ValueWrapper VisitRelational(LanguageParser.RelationalContext context)
    {
        ValueWrapper left = Visit(context.expr(0));
        ValueWrapper right = Visit(context.expr(1));
        var op = context.op.Text;

        return (left, right, op) switch
        {
            // Menor que
            (IntValue l, IntValue r, "<") => new BoolValue(l.Value < r.Value),
            (FloatValue l, FloatValue r, "<") => new BoolValue(l.Value < r.Value),
            (IntValue l, FloatValue r, "<") => new BoolValue(l.Value < r.Value),
            (FloatValue l, IntValue r, "<") => new BoolValue(l.Value < r.Value),
            (RuneValue l, RuneValue r, "<") => new BoolValue(l.Value < r.Value),
            // Menor o igual que
            (IntValue l, IntValue r, "<=") => new BoolValue(l.Value <= r.Value),
            (FloatValue l, FloatValue r, "<=") => new BoolValue(l.Value <= r.Value),
            (IntValue l, FloatValue r, "<=") => new BoolValue(l.Value <= r.Value),
            (FloatValue l, IntValue r, "<=") => new BoolValue(l.Value <= r.Value),
            (RuneValue l, RuneValue r, "<=") => new BoolValue(l.Value <= r.Value),
            // Mayor que
            (IntValue l, IntValue r, ">") => new BoolValue(l.Value > r.Value),
            (FloatValue l, FloatValue r, ">") => new BoolValue(l.Value > r.Value),
            (IntValue l, FloatValue r, ">") => new BoolValue(l.Value > r.Value),
            (FloatValue l, IntValue r, ">") => new BoolValue(l.Value > r.Value),
            (RuneValue l, RuneValue r, ">") => new BoolValue(l.Value > r.Value),
            // Mayor o igual que
            (IntValue l, IntValue r, ">=") => new BoolValue(l.Value >= r.Value),
            (FloatValue l, FloatValue r, ">=") => new BoolValue(l.Value >= r.Value),
            (IntValue l, FloatValue r, ">=") => new BoolValue(l.Value >= r.Value),
            (FloatValue l, IntValue r, ">=") => new BoolValue(l.Value >= r.Value),
            (RuneValue l, RuneValue r, ">=") => new BoolValue(l.Value >= r.Value),
            _ => throw new SemanticError("No se puede realizar la operación", context.Start)
        };
    }

    // Visit Equality
    public override ValueWrapper VisitEquality(LanguageParser.EqualityContext context)
    {
        ValueWrapper left = Visit(context.expr(0));
        ValueWrapper right = Visit(context.expr(1));
        var op = context.op.Text;

        return (left, right, op) switch
        {
            // Operador &&
            (IntValue l, IntValue r, "==") => new BoolValue(l.Value == r.Value),
            (FloatValue l, FloatValue r, "==") => new BoolValue(l.Value == r.Value),
            (IntValue l, FloatValue r, "==") => new BoolValue(l.Value == r.Value),
            (FloatValue l, IntValue r, "==") => new BoolValue(l.Value == r.Value),
            (BoolValue l, BoolValue r, "==") => new BoolValue(l.Value == r.Value),
            (StringValue l, StringValue r, "==") => new BoolValue(l.Value == r.Value),
            (RuneValue l, RuneValue r, "==") => new BoolValue(l.Value == r.Value),
            // Operador != 
            (IntValue l, IntValue r, "!=") => new BoolValue(l.Value != r.Value),
            (FloatValue l, FloatValue r, "!=") => new BoolValue(l.Value != r.Value),
            (IntValue l, FloatValue r, "!=") => new BoolValue(l.Value != r.Value),
            (FloatValue l, IntValue r, "!=") => new BoolValue(l.Value != r.Value),
            (BoolValue l, BoolValue r, "!=") => new BoolValue(l.Value != r.Value),
            (StringValue l, StringValue r, "!=") => new BoolValue(l.Value != r.Value),
            (RuneValue l, RuneValue r, "!=") => new BoolValue(l.Value != r.Value),
            _ => throw new SemanticError("No se puede realizar la operación", context.Start)
        };
    }

    // VisitLogicalAnd
    public override ValueWrapper VisitLogicalAnd(LanguageParser.LogicalAndContext context)
    {
        ValueWrapper left = Visit(context.expr(0));
        ValueWrapper right = Visit(context.expr(1));

        return (left, right) switch
        {
            (BoolValue l, BoolValue r) => new BoolValue(l.Value && r.Value),
            _ => throw new SemanticError($"No se puede realizar la operación lógica AND con {left.GetType().Name} y {right.GetType().Name}", context.Start)
        };
    }

    // Logigal Or
    public override ValueWrapper VisitLogicalOr(LanguageParser.LogicalOrContext context)
    {
        ValueWrapper left = Visit(context.expr(0));
        ValueWrapper right = Visit(context.expr(1));

        return (left, right) switch
        {
            (BoolValue l, BoolValue r) => new BoolValue(l.Value || r.Value),
            _ => throw new SemanticError("No se puede realizar la operación", context.Start)
        };
    }


    // VisitIncrement
    public override ValueWrapper VisitPostIncrement(LanguageParser.PostIncrementContext context)
    {
        string id = context.ID().GetText();
        ValueWrapper value = currentEnvironment.Get(id, context.Start);

        if (value is IntValue i)
        {
            currentEnvironment.Assign(id, new IntValue(i.Value + 1), context.Start);
            return new IntValue(i.Value);
        }
        throw new SemanticError($"No se puede incrementar {value.GetType()}", context.Start);
    }

    // VisitDecrement
    public override ValueWrapper VisitPostDecrement(LanguageParser.PostDecrementContext context)
    {
        string id = context.ID().GetText();
        ValueWrapper value = currentEnvironment.Get(id, context.Start);

        if (value is IntValue i)
        {
            currentEnvironment.Assign(id, new IntValue(i.Value - 1), context.Start);
            return new IntValue(i.Value);
        }
        throw new SemanticError($"No se puede decrementar {value.GetType()}", context.Start);
    }

    // VisitFloat
    public override ValueWrapper VisitFloat(LanguageParser.FloatContext context)
    {
        return new FloatValue(float.Parse(context.FLOAT().GetText()));
    }

    // VisitBoolean
    public override ValueWrapper VisitBoolean(LanguageParser.BooleanContext context)
    {
        return new BoolValue(bool.Parse(context.BOOL().GetText()));
    }

    // VisitRune
    public override ValueWrapper VisitRune(LanguageParser.RuneContext context)
    {
        string runText = context.GetText();
        char runeChar = runText[1];
        return new RuneValue(runeChar);
    }

    // VisitString
    public override ValueWrapper VisitString(LanguageParser.StringContext context)
    {
        string text = context.STRING().GetText();
        text = text.Substring(1, text.Length - 2);
        return new StringValue(text);
    }

    // VisitBlockStmt
    public override ValueWrapper VisitBlockStmt(LanguageParser.BlockStmtContext context)
    {
        Environment previousEnvironment = currentEnvironment;
        currentEnvironment = new Environment(previousEnvironment);
        foreach (var stmt in context.dcl())
        {
            Visit(stmt);
        }
        currentEnvironment = previousEnvironment;
        return defaultValue;
    }

    // ---------------- Control de Flujos --------------

    // VisitIfStmt
    public override ValueWrapper VisitIfStmt(LanguageParser.IfStmtContext context)
    {
        ValueWrapper condition = Visit(context.expr());
        if (condition is not BoolValue)
        {
            throw new SemanticError("Error: La condición debe ser de tipo booleano", context.Start);
        }
        if ((condition as BoolValue).Value)
        {
            Visit(context.stmt(0));
        }
        else if (context.stmt().Length > 1)
        {
            Visit(context.stmt(1));
        }
        return defaultValue;
    }

    // VisitForCond
    public override ValueWrapper VisitForStmtCond(LanguageParser.ForStmtCondContext context)
    {
        ValueWrapper condition = Visit(context.expr());
        if (condition is not BoolValue)
        {
            throw new SemanticError("Error: La condición debe ser de tipo booleano", context.Start);
        }

        try
        {
            while ((condition as BoolValue).Value)
            {
                try
                {
                    Visit(context.stmt());
                }
                catch (ContinueException)
                {
                    // Se ejecuta el continue
                }

                condition = Visit(context.expr());
                if (condition is not BoolValue)
                {
                    throw new SemanticError("Error: La condición debe ser de tipo booleano", context.Start);
                }
            }
        }
        catch (BreakException)
        {
            // Se ejecutar el break
        }

        return defaultValue;
    }

    // VisitFor
    public override ValueWrapper VisitForStmt(LanguageParser.ForStmtContext context)
    {
        Environment previousEnvironment = currentEnvironment;
        currentEnvironment = new Environment(currentEnvironment);

        Visit(context.forInit());

        VisitForBody(context);

        currentEnvironment = previousEnvironment;
        return defaultValue;
    }

    public void VisitForBody(LanguageParser.ForStmtContext context)
    {
        ValueWrapper condition = Visit(context.expr(0));

        var lastEnvironment = currentEnvironment;
        if (condition is not BoolValue)
        {
            throw new SemanticError("Invalid condition", context.Start);
        }
        try
        {
            while (condition is BoolValue boolCondition && boolCondition.Value)
            {
                Visit(context.stmt());
                Visit(context.expr(1));
                condition = Visit(context.expr(0));
            }
        }
        catch (BreakException)
        {
            currentEnvironment = lastEnvironment;
        }
        catch (ContinueException)
        {
            currentEnvironment = lastEnvironment;
            Visit(context.expr(1));
            VisitForBody(context);
        }
    }


     // VisitForRangeStmt
    public override ValueWrapper VisitForRangeStmt(LanguageParser.ForRangeStmtContext context)
        {
            string indexName = context.ID(0).GetText(); 
            string valueName = context.ID(1).GetText(); 
            string sliceName = context.ID(2).GetText(); 


            var sliceInstance = currentEnvironment.Get(sliceName, context.Start);

            if (sliceInstance is not InstanceValue instanceValue || 
                !instanceValue.instance.languageclass.Name.StartsWith("[]"))
            {
                throw new SemanticError($"'{sliceName}' no es un slice válido para range", context.Start);
            }

    
            var sliceProperties = instanceValue.instance.Properties;

    
            if (!currentEnvironment.variables.ContainsKey(indexName))
                currentEnvironment.Declare(indexName, new IntValue(0), "int", context.Start);

    
            string elementType = instanceValue.instance.languageclass.Name.TrimStart('[').TrimStart(']');

    
            if (!currentEnvironment.variables.ContainsKey(valueName))
                currentEnvironment.Declare(valueName, valueOrDefault(elementType), elementType, context.Start);

            ValueWrapper lastValue = defaultValue;
            int i = 0;
            foreach (var item in sliceProperties.Values)
            {
                currentEnvironment.Assign(indexName, new IntValue(i), context.Start);
                currentEnvironment.Assign(valueName, item, context.Start);
                lastValue = Visit(context.stmt());
                i++;
            }
            return lastValue; // Retornar el último valor evaluado
        }

    // VisitSwitchCase
    public override ValueWrapper VisitSwitchStmt(LanguageParser.SwitchStmtContext context)
    {
        currentSwitchValue = Visit(context.expr());
        try
        {
            foreach (var caseStmt in context.caseStmt())
            {
                var caseValue = Visit(caseStmt.expr());

                if (caseValue.Equals(currentSwitchValue))
                {
                    foreach (var stmt in caseStmt.stmt())
                    {
                        Visit(stmt);
                    }
                    return defaultValue;
                }
            }

            if (context.defaultStmt() != null)
            {
                foreach (var stmt in context.defaultStmt().stmt())
                {
                    Visit(stmt);
                }
            }
        }
        catch (BreakException)
        {
            // Entramos en la sentencia Break
            return defaultValue;
        }
        return defaultValue;
    }

    // VisiBreak
    public override ValueWrapper VisitBreakStmt(LanguageParser.BreakStmtContext context)
    {
        throw new BreakException();
    }

    // VisitContinueStmt
    public override ValueWrapper VisitContinueStmt(LanguageParser.ContinueStmtContext context)
    {
        throw new ContinueException();
    }

    // VisitReturnStmt
    public override ValueWrapper VisitReturnStmt(LanguageParser.ReturnStmtContext context)
    {
        ValueWrapper value = defaultValue;
        if (context.expr() != null)
        {
            value = Visit(context.expr());
        }
        throw new ReturnException(value);
    }

    // ------------------- Funciones -------------------
    //VisitCalle -> Se encarga de llamar a la función
    public override ValueWrapper VisitCallee(LanguageParser.CalleeContext context)
    {
        ValueWrapper calle = Visit(context.expr());

        foreach (var action in context.call())
        {
            if (action is LanguageParser.FuncCallContext call)
            {
                if (calle is FunctionValue functionValue)
                {
                    calle = VisitCall(functionValue.invocable, call.args());
                }
                else
                {
                    throw new SemanticError("No se pudo llamar la funcion", context.Start);
                }
            }
            else if (action is LanguageParser.GetContext propertyAccess)
            {
                if (calle is InstanceValue instanceValue)
                {
                    calle = instanceValue.instance.Get(propertyAccess.ID().GetText(), propertyAccess.Start);
                }
                else
                {
                    throw new SemanticError("No se pudo acceder a la propiedad", context.Start);
                }
            }
            else if(action is LanguageParser.ArrayAccessContext arrayAccess)
            {
                if (calle is InstanceValue instanceValue)
                {
                    var index = Visit(arrayAccess.expr());
                    if(index is IntValue intValue)
                    {   
                        calle = instanceValue.instance.Get(intValue.Value.ToString(), arrayAccess.Start);
                    }else{
                        throw new SemanticError("Error: No se puedo acceder a la propiedad array", context.Start);  
                    }
                }
                else
                {
                    throw new SemanticError("Error: Al acceder al array", context.Start);
                }
            }
            else if (action is LanguageParser.ArrayAccessBidimensionalContext arrayAccesBidimensional) {
            if (calle is InstanceValue instanceValue) {
                var index = Visit(arrayAccesBidimensional.expr(0));
                var index2 = Visit(arrayAccesBidimensional.expr(1));

                if (index is IntValue intValue && index2 is IntValue intValue2) {
                    // Obtener la primera dimensión
                    var firstDimension = instanceValue.instance.Get(intValue.Value.ToString(), arrayAccesBidimensional.Start);

                    if (firstDimension is InstanceValue firstInstanceValue) {
                        // Obtener la segunda dimensión
                        calle = firstInstanceValue.instance.Get(intValue2.Value.ToString(), arrayAccesBidimensional.Start);
                    } else {
                        throw new SemanticError("Error: No se pudo acceder a la primera dimensión del array", context.Start);
                    }
                } else {
                    throw new SemanticError("Error: Índices inválidos en acceso bidimensional", context.Start);
                }
            } else {
                throw new SemanticError("Error: No se pudo acceder al array bidimensional", context.Start);
            }
        }
        }
        return calle;
    }


    // Se encarga de llamar a la función
    public ValueWrapper VisitCall(Invocable invocable, LanguageParser.ArgsContext context)
    {
        List<ValueWrapper> args = new List<ValueWrapper>();

        if (context != null)
        {
            foreach (var expr in context.expr())
            {
                args.Add(Visit(expr));
            }
        }
        if (context != null && args.Count != invocable.Arity())
        {
            throw new SemanticError("Error: Número de argumentos incorrecto", context.Start);
        }
        return invocable.Invoke(args, this);
    }

    // ------------------- Funciones -------------------

   public override ValueWrapper VisitFuncDcl(LanguageParser.FuncDclContext context)
    {
        var foreign = new ForeignFunction(currentEnvironment, context);
        currentEnvironment.Declare(context.ID().GetText(), new FunctionValue(foreign, context.ID().GetText()), context.Start.Text);
        return defaultValue;
    }
    


    // ---- Arrays --------

    //VisitArray - Unidimensional
    public override ValueWrapper VisitArray(LanguageParser.ArrayContext context)
    {
        List<ValueWrapper> args = new List<ValueWrapper>();
        if (context.args() != null)
        {
            foreach (var expr in context.args().expr())
            {
                args.Add(Visit(expr));
            }
        }
        string arrayType = context.Tipo().GetText();
        foreach (var arg in args)
        {
            if (!IsValidType(arg, arrayType))
            {
                throw new SemanticError($"Error: El array es de tipo {arrayType}, pero se encontró un {arg.GetType()}", context.Start);
            }
        }
        var arrayClass = new LanguageArray();
        var instance = arrayClass.Invoke(args, this);
        return instance;
    }


    //VisitMatrix
   public override ValueWrapper VisitArrayBidimensional(LanguageParser.ArrayBidimensionalContext context)
    {
        List<ValueWrapper> filas = new List<ValueWrapper>();

        Console.WriteLine("VisitArrayBidimensional");
        if (context.arrayList() == null || context.arrayList().args() == null || context.arrayList().args().Length == 0)
        {
            throw new SemanticError("Error: La matriz bidimensional no está correctamente definida.", context.Start);
        }

        Console.WriteLine(context.arrayList().args().Length);
        Console.WriteLine(context.arrayList().args()[0]?.expr()?.Length ?? 0);

        foreach (var fila in context.arrayList().args())
        {
            List<ValueWrapper> elementosFila = new List<ValueWrapper>();

            foreach (var expr in fila.expr())
            {
                elementosFila.Add(Visit(expr));
            }

            string tipoFila = context.Tipo().GetText();
            foreach (var elemento in elementosFila)
            {
                if (!IsValidType(elemento, tipoFila))
                {
                    throw new SemanticError($"Error: La matriz es de tipo {tipoFila}, pero se encontró un {elemento.GetType()}", context.Start);
                }
            }

            var filaArrayClass = new LanguageArray();
            var filaInstance = filaArrayClass.Invoke(elementosFila, this);
            filas.Add(filaInstance);
        }

        var matrizClass = new LanguageBidimensional();
        var matrizInstance = matrizClass.Invoke(filas, this);
        return matrizInstance;
    }


    private bool IsValidType(ValueWrapper value, string  expectedType){
        return (expectedType == "int" && value is IntValue) ||
                (expectedType == "float64" && (value is FloatValue || value is IntValue)) ||
                (expectedType == "string" && value is StringValue) ||
                (expectedType == "bool" && value is BoolValue) ||
                (expectedType == "rune" && value is RuneValue);
    }

    // ----------- Funciones Embeded -----------
    public override ValueWrapper VisitFuncEmbed(LanguageParser.FuncEmbedContext context) {
        string objectName = context.ID(0).GetText();
        string methodName = context.ID(1).GetText();
        ValueWrapper result;
        if (context.args() != null) {
            var arguments = Visit(context.args());

            string functionName = $"{objectName}.{methodName}"; 

            var functionValue = currentEnvironment.Get(functionName, context.Start);

            if (functionValue != null && functionValue is FunctionValue funcValue) {
                Console.WriteLine($"Función embebida encontrada: {functionName}");
                result = funcValue.invocable.Invoke(new List<ValueWrapper> { arguments }, this);
            } else {
                throw new SemanticError($"Error: Función embebida {functionName} no registrada", context.Start);
            }
        } else {
            throw new SemanticError("Error: Se requieren argumentos para la función embebida", context.Start);
        }
        return result;
    }

    // ----------- slice.index
    public override ValueWrapper VisitIndexSlice(LanguageParser.IndexSliceContext context)
        {
            string arrayName = context.ID().GetText();
            ValueWrapper searchValue = Visit(context.expr());
            var arrayInstance = currentEnvironment.Get(arrayName, context.Start);

            if (arrayInstance is not InstanceValue instanceValue || 
                instanceValue.instance.languageclass.Name != "[]")
            {
                throw new SemanticError($"'{arrayName}' no es un array válido", context.Start);
            }
            var arrayProperties = instanceValue.instance.Properties;
            int index = -1;
            int currentIndex = 0;

            foreach (var kvp in arrayProperties)
            {
                if (kvp.Value.Equals(searchValue))
                {
                    index = currentIndex;
                    break;
                }
                currentIndex++;
            }
            return new IntValue(index);
        }

    // VisitJoin
    public override ValueWrapper VisitJoin(LanguageParser.JoinContext context)
        {
            string sliceName = context.ID().GetText();
            var sliceValue = currentEnvironment.Get(sliceName, context.Start);
            ValueWrapper separatorValue = Visit(context.expr());

            if (sliceValue is not InstanceValue instanceValue || 
                instanceValue.instance.languageclass.Name != "[]")
            {
                throw new SemanticError($"'{sliceName}' no es un slice válido de strings", context.Start);
            }

            // Obtengo los elementos
            var sliceElements = instanceValue.instance.Properties.Values;

            // Conversion a strings
            List<string> stringElements = new();
            foreach (var element in sliceElements)
            {
                if (element is StringValue strValue)
                {
                    stringElements.Add(strValue.Value);
                }
                else
                {
                    throw new SemanticError($"Elemento no es un string en el slice '{sliceName}'", context.Start);
                }
            }
            if (separatorValue is not StringValue sepValue)
            {
                throw new SemanticError("El separador debe ser una cadena", context.Start);
            }
            // Uno los elementos
            string result = string.Join(sepValue.Value, stringElements);
            Console.WriteLine($"Resultado de Join: {result}");
            return new StringValue(result);
    }

    // VisitAppend
    public override ValueWrapper VisitAppend(LanguageParser.AppendContext context)
        {
            // Obtengo el slice a modificar
            string sliceName = context.ID().GetText();

            var sliceInstance = currentEnvironment.Get(sliceName, context.Start);
            
            // Verificar si es un slice válido (unidimensional o bidimensional)
            if (sliceInstance is not InstanceValue instanceValue || 
                !(instanceValue.instance.languageclass.Name.StartsWith("[]") || 
                instanceValue.instance.languageclass.Name.StartsWith("[][]")))
            {
                throw new SemanticError($"'{sliceName}' no es un slice válido", context.Start);
            }

            // Obtener el valor a agregar
            ValueWrapper newValue = Visit(context.expr());

            // Obtengo los elementos del slice
            var sliceProperties = instanceValue.instance.Properties;

            string sliceType;
            if (instanceValue.instance.languageclass.Name.StartsWith("[][]"))
            {
                sliceType = instanceValue.instance.languageclass.Name.Substring(4); 
            }
            else
            {
                sliceType = instanceValue.instance.languageclass.Name.Substring(2);
            }

            // Obtengo el nuevo tipos
            string newValueType = newValue.GetType().Name;

            // Verifico la compatibilidad de tipos
            if (!(newValueType.StartsWith(sliceType) || newValueType.StartsWith("[]" + sliceType)))
            {
                throw new SemanticError($"No se puede agregar un elemento de tipo {newValueType} a un slice de {instanceValue.instance.languageclass.Name}", context.Start);
            }

            sliceProperties.Add(sliceProperties.Count.ToString(), newValue);
            return instanceValue;
        }

        // VisitLen
        public override ValueWrapper VisitLen(LanguageParser.LenContext context)
        {
            string sliceName = context.ID().GetText();
            var sliceInstance = currentEnvironment.Get(sliceName, context.Start);

            if (sliceInstance is not InstanceValue instanceValue || 
                !(instanceValue.instance.languageclass.Name.StartsWith("[]") || 
                instanceValue.instance.languageclass.Name.StartsWith("[][]")))
            {
                throw new SemanticError($"'{sliceName}' no es un slice válido para len()", context.Start);
            }

            var sliceProperties = instanceValue.instance.Properties;
            int length = sliceProperties.Count;
            return new IntValue(length);
        }

   

    // ---- Structs
    public override ValueWrapper VisitClassDcl(LanguageParser.ClassDclContext context)
    {
        Dictionary<string, LanguageParser.VarDclContext> props = new Dictionary<string, LanguageParser.VarDclContext>();

        Dictionary<string, ForeignFunction> methods = new Dictionary<string, ForeignFunction>();

        foreach(var prop in context.classBody()){
            if(prop.varDcl() != null){
                var vardcl = prop.varDcl();
                props.Add(vardcl.ID().GetText(), vardcl);
                Console.WriteLine($"Propiedad: {vardcl.ID().GetText()}");
            }else if(prop.funcDcl() != null){
                var funDcl = prop.funcDcl();
                var foreignFunction = new ForeignFunction(currentEnvironment, funDcl);
                Console.WriteLine($"Funcion: {funDcl.ID().GetText()}");
                methods.Add(funDcl.ID().GetText(), foreignFunction);
            }

            LanguageClass languageClass = new LanguageClass(context.ID().GetText(), props, methods);
            currentEnvironment.Declare(context.ID().GetText(), new ClassValue(languageClass), context.Start.Text);

        }
        return defaultValue;
    }


    // VisitNew
   public override ValueWrapper VisitNew(LanguageParser.NewContext context)
    {
        string id = context.ID().GetText();
        ValueWrapper value = currentEnvironment.Get(id, context.Start);

        if (value is not ClassValue)
        {
            throw new SemanticError("Error: No se puede instanciar", context.Start);
        }
        List<ValueWrapper> args = new List<ValueWrapper>();
        if (context.args() != null)
        {
            foreach (var expr in context.args().expr())
            {
                args.Add(Visit(expr));
            }
        }
        var instance = ((ClassValue)value).languageClass.Invoke(args, this);
        return instance;
    }

}
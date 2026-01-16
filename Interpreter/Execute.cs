namespace Interpreter
{
    public class Environment
    {
        private Dictionary<string, MonkeyObject> Store;
        public static HashSet<string> BuiltinNames = new HashSet<string> {
            "len"
        };

        public Environment()
        {
            Store = new Dictionary<string, MonkeyObject>();

            // builtins
            Func<List<MonkeyObject>, MonkeyObject> len = args =>
            {
                if (args[0] is MString s)
                {
                    return new MInt(s.Value.Count());
                }
                else
                {
                    return new MError($"Type error: expected MString, got {args[0].ObjectType()}");
                }
            };
            Store.Add("len", new MBuiltin(len, new List<MonkeyObjType> { MonkeyObjType.MString }, 1));
        }

        public Environment(Dictionary<string, MonkeyObject> store)
        {
            Store = new Dictionary<string, MonkeyObject>(store);
        }

        public MonkeyObject Get(string s)
        {
            return Store[s];
        }

        /// <summary>
        /// Used to add or mutate values in the environment. Disallows collisions with builtins.
        /// </summary>
        /// <param name="s">variable name</param>
        /// <param name="val">variable value</param>
        /// <returns>true on success, false on fail</returns>
        public bool Set(string s, MonkeyObject val)
        {
            if (BuiltinNames.Contains(s))
            {
                return false;
            }
            Store[s] = val;
            return true;
        }

        public bool Valid(string s)
        {
            return Store.ContainsKey(s);
        }

        public Environment ShallowCopy()
        {
            return new Environment(Store);
        }
    }

    public enum MonkeyObjType
    {
        MInt,
        MBool,
        MString,
        MFunction,
        MNull,
        // wrap return values so we can implement return control flow
        MReturn,
        // we use objects to pass errors around
        MError,
        MIdentifier,
        MBuiltin
    };

    public interface MonkeyObject
    {
        MonkeyObjType ObjectType();
        string Inspect();
    }

    public class MReturnValue : MonkeyObject
    {
        public MonkeyObject Value;

        public MReturnValue(MonkeyObject value)
        {
            Value = value;
        }

        public MonkeyObjType ObjectType()
        {
            return MonkeyObjType.MReturn;
        }

        public string Inspect()
        {
            return $"return {Value.Inspect()}";
        }

        public override string ToString()
        {
            return Inspect();
        }
    }

    public class MInt : MonkeyObject
    {
        public int Value;

        public MInt(int value)
        {
            Value = value;
        }

        public MonkeyObjType ObjectType()
        {
            return MonkeyObjType.MInt;
        }

        public string Inspect()
        {
            return Value.ToString();
        }

        public override string ToString()
        {
            return Inspect();
        }
    }

    public class MBool : MonkeyObject
    {
        public bool Value;

        public MBool(bool value)
        {
            Value = value;
        }

        public MonkeyObjType ObjectType()
        {
            return MonkeyObjType.MBool;
        }

        public string Inspect()
        {
            return Value.ToString();
        }

        public override string ToString()
        {
            return Inspect();
        }
    }

    public class MString : MonkeyObject
    {
        public string Value;

        public MString(string value)
        {
            Value = value;
        }

        public MonkeyObjType ObjectType()
        {
            return MonkeyObjType.MString;
        }

        public string Inspect()
        {
            return $"\"{Value}\"";
        }

        public override string ToString()
        {
            return Inspect();
        }
    }

    public class MNull : MonkeyObject
    {
        public MonkeyObjType ObjectType()
        {
            return MonkeyObjType.MNull;
        }

        public string Inspect()
        {
            return "null";
        }

        public override string ToString()
        {
            return Inspect();
        }
    }

    public class MFunction : MonkeyObject
    {
        public List<Identifier> Args;
        public List<Statement> Body;
        public Environment Env;

        public MFunction(FunctionLiteral f, Environment env)
        {
            Args = f.Args;
            Body = f.Body;
            Env = env.ShallowCopy();
        }

        public MonkeyObjType ObjectType()
        {
            return MonkeyObjType.MFunction;
        }

        public string Inspect()
        {
            return $"fn/{Args.Count}";
        }

        public override string ToString()
        {
            return Inspect();
        }
    }

    public class MError : MonkeyObject
    {
        public string Value;

        public MError(string value)
        {
            Value = value;
        }

        public MonkeyObjType ObjectType()
        {
            return MonkeyObjType.MError;
        }

        public string Inspect()
        {
            return $"{Value}";
        }

        public override string ToString()
        {
            return Inspect();
        }
    }

    public class MIdentifier : MonkeyObject
    {
        public string Name;

        public MIdentifier(string name)
        {
            Name = name;
        }

        public MonkeyObjType ObjectType()
        {
            return MonkeyObjType.MIdentifier;
        }

        public string Inspect()
        {
            return $"{Name}";
        }

        public override string ToString()
        {
            return Inspect();
        }
    }

    public class MBuiltin : MonkeyObject
    {
        public Func<List<MonkeyObject>, MonkeyObject> F;
        public List<MonkeyObjType> ArgTypes;
        public int ArgCount;

        public MBuiltin(Func<List<MonkeyObject>, MonkeyObject> f, List<MonkeyObjType> argTypes, int argCount)
        {
            F = f;
            ArgTypes = argTypes;
            ArgCount = argCount;

            if (ArgTypes.Count != ArgCount)
            {
                throw new ArgumentException($"ArgTypes length ({ArgTypes.Count}) does not match the given ArgCount ({ArgCount})");
            }
        }

        public MonkeyObject Execute(List<MonkeyObject> args)
        {
            // check the arguments

            if (args.Count != ArgCount)
            {
                throw new ArgumentException($"args length ({args.Count}) does not match the function's ArgCount ({ArgCount})");
            }

            for (int i = 0; i < args.Count; i++)
            {
                if (args[i].ObjectType() != ArgTypes[i])
                {
                    return new MError($"Type error: expected {ArgTypes[i]}, got {args[i].GetType()}");
                }
            }

            // run the function
            return F.Invoke(args);
        }

        public MonkeyObjType ObjectType()
        {
            return MonkeyObjType.MBuiltin;
        }

        public string Inspect()
        {
            return "Builtin Operation";
        }

        public override string ToString()
        {
            return Inspect();
        }
    }

    public class Evaluator
    {
        public MonkeyObject Eval(Node node, Environment env)
        {
            // regrettably using a switch expression doesn't work because of error handling
            switch (node)
            {
                case Program p:
                    return EvalProgram(p.Statements, env);
                case ReturnStatement s:
                    return new MReturnValue(Eval(s.Value, env));
                case LetStatement s:
                    return EvalLetStatement(s, env);
                case ExpressionStatement s:
                    return Eval(s.Value, env);
                case Identifier i:
                    return EvalIdentifier(i, env);
                case IntLiteral i:
                    return EvalIntLiteral(i);
                case BooleanLiteral b:
                    return EvalBooleanLiteral(b);
                case StringLiteral s:
                    return EvalStringLiteral(s);
                case FunctionLiteral f:
                    return EvalFunctionLiteral(f, env);
                case PrefixOperator op:
                    return EvalPrefixOp(op, env);
                case InfixOperator op:
                    return EvalInfixOp(op, env);
                case Call c:
                    return EvalCall(c, env);
                case IfExpression ifexp:
                    return EvalIf(ifexp, env);
                default:
                    return new MError($"Invalid node type: {node.GetType()}");
            }
        }

        private MonkeyObject EvalProgram(List<Statement> statements, Environment env)
        {
            MonkeyObject result = new MNull();
            foreach (Statement s in statements)
            {
                result = Eval(s, env);
                if (result is MReturnValue ret)
                {
                    return ret.Value;
                }
                if (result is MError err)
                {
                    return err;
                }
            }

            return result;
        }

        private MonkeyObject EvalBlock(List<Statement> statements, Environment env)
        {
            MonkeyObject result = new MNull();

            foreach (Statement s in statements)
            {
                result = Eval(s, env);

                if (result is MReturnValue)
                {
                    return result;
                }
                if (result is MError err)
                {
                    return err;
                }
            }

            return result;
        }

        private MonkeyObject EvalLetStatement(LetStatement s, Environment env)
        {
            MonkeyObject value = Eval(s.Value, env);

            if (value is MError)
            {
                return value;
            }
            else if (value is MNull)
            {
                return new MError($"Invalid rvalue: null cannot be an rvalue");
            }

            bool result = env.Set(s.Name.Value, value);
            if (result)
            {
                return new MNull();
            }
            else
            {
                return new MError($"Cannot use variable name {s.Name}");
            }
        }

        private MonkeyObject EvalIdentifier(Identifier i, Environment env)
        {
            try
            {
                MonkeyObject result = env.Get(i.Value);
                return result;
            }
            catch (KeyNotFoundException)
            {
                return new MError($"Identifier {i.Value} is undefined");
            }
        }

        private MonkeyObject EvalIntLiteral(IntLiteral i)
        {
            return new MInt(i.Value);
        }

        private MonkeyObject EvalBooleanLiteral(BooleanLiteral b)
        {
            return new MBool(b.Value);
        }

        private MonkeyObject EvalStringLiteral(StringLiteral s)
        {
            return new MString(s.Value);
        }

        private MonkeyObject EvalFunctionLiteral(FunctionLiteral f, Environment env)
        {
            foreach (Identifier id in f.Args)
            {
                if (Environment.BuiltinNames.Contains(id.Value))
                {
                    return new MError($"Illegal function argument name: {id.Value}");
                }
            }

            return new MFunction(f, env);
        }

        private MonkeyObject EvalPrefixOp(PrefixOperator op, Environment env)
        {
            switch (op.Operator)
            {
                case "!":
                    return EvalBangOp(op, env);
                case "-":
                    return EvalNegativeOp(op, env);
                default:
                    return new MError($"Invalid prefix operator: {op.Operator}");
            }
        }

        private MonkeyObject EvalBangOp(PrefixOperator op, Environment env)
        {
            MonkeyObject operand = Eval(op.Value, env);
            if (operand is MBool b)
            {
                return new MBool(!b.Value);
            }
            else if (!(operand is MNull))
            {
                return TypeError(MonkeyObjType.MBool, operand);
            }
            else
            {
                // assume the null error has already been handled
                // null is not accessible in the langauge's public interface after all
                return new MNull();
            }
        }

        private MonkeyObject EvalNegativeOp(PrefixOperator op, Environment env)
        {
            MonkeyObject operand = Eval(op.Value, env);
            if (operand is MInt i)
            {
                return new MInt(-i.Value);
            }
            else if (!(operand is MNull))
            {
                return TypeError(MonkeyObjType.MInt, operand);
            }
            else
            {
                // assume the null error has already been handled
                // null is not accessible in the langauge's public interface after all
                return new MNull();
            }
        }

        private MonkeyObject EvalInfixOp(InfixOperator op, Environment env)
        {
            switch (op.Operator)
            {
                case "+":
                    return EvalPlusOp(op, env);
                case "-":
                    return EvalMinusOp(op, env);
                case "*":
                    return EvalTimesOp(op, env);
                case "/":
                    return EvalDivideOp(op, env);
                case "^":
                    return EvalConcatOp(op, env);
                case "==":
                    return EvalEqualOp(op, env);
                case "!=":
                    return EvalNotEqualOp(op, env);
                case "<":
                    return EvalLess(op, env);
                case ">":
                    return EvalGreater(op, env);
                default:
                    return new MError($"Invalid infix operator: {op.Operator}");
            }
        }

        private MonkeyObject EvalPlusOp(InfixOperator op, Environment env)
        {
            MonkeyObject lhs = Eval(op.Lhs, env);
            MonkeyObject rhs = Eval(op.Rhs, env);

            if (lhs is MInt l && rhs is MInt r)
            {
                return new MInt(l.Value + r.Value);
            }
            else if (!(lhs is MNull) && !(lhs is MInt))
            {
                return TypeError(MonkeyObjType.MInt, lhs);
            }
            else if (!(rhs is MNull) && !(rhs is MInt))
            {
                return TypeError(MonkeyObjType.MInt, rhs);
            }
            else
            {
                return new MNull();
            }
        }

        private MonkeyObject EvalMinusOp(InfixOperator op, Environment env)
        {
            MonkeyObject lhs = Eval(op.Lhs, env);
            MonkeyObject rhs = Eval(op.Rhs, env);

            if (lhs is MInt l && rhs is MInt r)
            {
                return new MInt(l.Value - r.Value);
            }
            else if (!(lhs is MNull) && !(lhs is MInt))
            {
                return TypeError(MonkeyObjType.MInt, lhs);
            }
            else if (!(rhs is MNull) && !(rhs is MInt))
            {
                return TypeError(MonkeyObjType.MInt, rhs);
            }
            else
            {
                return new MNull();
            }
        }

        private MonkeyObject EvalTimesOp(InfixOperator op, Environment env)
        {
            MonkeyObject lhs = Eval(op.Lhs, env);
            MonkeyObject rhs = Eval(op.Rhs, env);

            if (lhs is MInt l && rhs is MInt r)
            {
                return new MInt(l.Value * r.Value);
            }
            else if (!(lhs is MNull) && !(lhs is MInt))
            {
                return TypeError(MonkeyObjType.MInt, lhs);
            }
            else if (!(rhs is MNull) && !(rhs is MInt))
            {
                return TypeError(MonkeyObjType.MInt, rhs);
            }
            else
            {
                return new MNull();
            }
        }

        private MonkeyObject EvalDivideOp(InfixOperator op, Environment env)
        {
            MonkeyObject lhs = Eval(op.Lhs, env);
            MonkeyObject rhs = Eval(op.Rhs, env);

            if (lhs is MInt l && rhs is MInt r)
            {
                return new MInt(l.Value / r.Value);
            }
            else if (!(lhs is MNull) && !(lhs is MInt))
            {
                return TypeError(MonkeyObjType.MInt, lhs);
            }
            else if (!(rhs is MNull) && !(rhs is MInt))
            {
                return TypeError(MonkeyObjType.MInt, rhs);
            }
            else
            {
                return new MNull();
            }
        }

        private MonkeyObject EvalConcatOp(InfixOperator op, Environment env)
        {
            MonkeyObject lhs = Eval(op.Lhs, env);
            MonkeyObject rhs = Eval(op.Rhs, env);

            if (lhs is MString l && rhs is MString r)
            {
                return new MString(l.Value + r.Value);
            }
            else if (!(lhs is MNull) && !(lhs is MString))
            {
                return TypeError(MonkeyObjType.MString, lhs);
            }
            else if (!(rhs is MNull) && !(rhs is MString))
            {
                return TypeError(MonkeyObjType.MString, rhs);
            }
            else
            {
                return new MNull();
            }

        }

        private MonkeyObject EvalEqualOp(InfixOperator op, Environment env)
        {
            MonkeyObject lhs = Eval(op.Lhs, env);
            MonkeyObject rhs = Eval(op.Rhs, env);

            switch (lhs, rhs)
            {
                case (MInt lInt, MInt rInt):
                    return new MBool(lInt.Value == rInt.Value);
                case (MBool lBool, MBool rBool):
                    return new MBool(lBool.Value == rBool.Value);
                case (MString lString, MString rString):
                    return new MBool(lString.Value == rString.Value);
                case (MNull, MNull):
                    return new MBool(true);
                default:
                    return new MBool(false);
            }
        }

        private MonkeyObject EvalNotEqualOp(InfixOperator op, Environment env)
        {
            MonkeyObject lhs = Eval(op.Lhs, env);
            MonkeyObject rhs = Eval(op.Rhs, env);

            switch (lhs, rhs)
            {
                case (MInt lInt, MInt rInt):
                    return new MBool(lInt.Value != rInt.Value);
                case (MBool lBool, MBool rBool):
                    return new MBool(lBool.Value != rBool.Value);
                case (MString lString, MString rString):
                    return new MBool(lString.Value != rString.Value);
                case (MNull, MNull):
                    return new MBool(false);
                default:
                    return new MBool(true);
            }
        }

        private MonkeyObject EvalLess(InfixOperator op, Environment env)
        {
            MonkeyObject lhs = Eval(op.Lhs, env);
            MonkeyObject rhs = Eval(op.Rhs, env);

            switch (lhs, rhs)
            {
                case (MInt lInt, MInt rInt):
                    return new MBool(lInt.Value < rInt.Value);
                case (MString lString, MString rString):
                    switch (lString.Value.CompareTo(rString.Value))
                    {
                        case int ord when ord < 0:
                            return new MBool(true);
                        default:
                            return new MBool(false);
                    }
                default:
                    return new MNull();
            }
        }

        private MonkeyObject EvalGreater(InfixOperator op, Environment env)
        {
            MonkeyObject lhs = Eval(op.Lhs, env);
            MonkeyObject rhs = Eval(op.Rhs, env);

            switch (lhs, rhs)
            {
                case (MInt lInt, MInt rInt):
                    return new MBool(lInt.Value > rInt.Value);
                case (MString lString, MString rString):
                    switch (lString.Value.CompareTo(rString.Value))
                    {
                        case int ord when ord > 0:
                            return new MBool(true);
                        default:
                            return new MBool(false);
                    }
                default:
                    return new MNull();
            }
        }

        private MonkeyObject EvalCall(Call c, Environment env)
        {
            MonkeyObject fResult = Eval(c.Function, env);
            if (fResult is MFunction f)
            {
                // check function arity
                if (f.Args.Count != c.Args.Count)
                {
                    return new MError($"Incorrect arity in function call: expected {f.Args.Count}, found {c.Args.Count}");
                }

                // add arguments to closure environment
                Environment internalEnv = f.Env;
                for (int i = 0; i < c.Args.Count; i++)
                {
                    MonkeyObject cArgValue = Eval(c.Args[i], env);
                    if (cArgValue is MError)
                    {
                        return cArgValue;
                    }
                    bool result = internalEnv.Set(f.Args[i].Value, cArgValue);

                    if (!result)
                    {
                        return new MError($"Illegal function argument name: {f.Args[i].Value}");
                    }
                }

                // evaluate the function
                return EvalBlock(f.Body, internalEnv);
            }
            else if (fResult is MBuiltin fBuiltin)
            {
                List<MonkeyObject> args = new List<MonkeyObject>();
                foreach (Expression a in c.Args)
                {
                    MonkeyObject evaluated = Eval(a, env);
                    if (evaluated is MError)
                    {
                        return evaluated;
                    }
                    args.Add(evaluated);
                }
                return fBuiltin.Execute(args);
            }
            else if (fResult is MError)
            {
                return fResult;
            }
            else
            {
                return TypeError(MonkeyObjType.MFunction, fResult);
            }
        }

        private MonkeyObject EvalIf(IfExpression ifexp, Environment env)
        {
            MonkeyObject condition = Eval(ifexp.Condition, env);
            if (condition is MBool c)
            {
                if (c.Value)
                {
                    return EvalBlock(ifexp.Consequence, env);
                }
                else
                {
                    return EvalBlock(ifexp.Otherwise, env);
                }
            }
            else if (!(condition is MNull))
            {
                return TypeError(MonkeyObjType.MBool, condition);
            }
            else
            {
                return new MNull();
            }
        }

        private MError TypeError(MonkeyObjType expected, MonkeyObject obj)
        {
            return new MError($"Type error: expected {expected}, got {obj.ObjectType()}");
        }
    }
}

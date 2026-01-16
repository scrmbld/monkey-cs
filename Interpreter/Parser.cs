using System.Text;

namespace Interpreter
{
    public interface Node
    {
        public string TokenLiteral();
    }

    static public class Precedences
    {
        public static int LOWEST = 0;
        public static int EQUALS = 1;
        public static int LESSGREATER = 2;
        public static int SUM = 3;
        public static int PRODUCT = 4;
        public static int PREFIX = 5;
        public static int CALL = 6;
        public static int INDEX = 7;
    }

    public interface Statement : Node { }

    public interface Expression : Node { }

    public class Program : Node
    {
        public List<Statement> Statements = new List<Statement>();
        public string TokenLiteral()
        {
            if (Statements.Count > 0)
            {
                return Statements[0].TokenLiteral();
            }
            else
            {
                return "";
            }
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder("(block");
            foreach (Statement stmt in Statements)
            {
                sb.Append("\n  ");
                StringBuilder field = new StringBuilder(stmt.ToString());
                field.Replace("\n", "\n  ");
                sb.Append(field);
            }

            sb.Append(")");
            return sb.ToString();
        }
    }

    public class Identifier : Expression
    {
        public Token Tok;
        public string Value;

        public Identifier(Token token, string value)
        {
            Tok = token;
            Value = value;
        }

        public string TokenLiteral()
        {
            return Tok.Literal;
        }

        public override string ToString()
        {
            return $"(identifier ({Value}))";
        }
    }

    public class IntLiteral : Expression
    {
        public Token Tok;
        public int Value;
        public IntLiteral(Token token, int value)
        {
            Tok = token;
            Value = value;
        }

        public string TokenLiteral()
        {
            return Tok.Literal;
        }

        public override string ToString()
        {
            return $"(int ({Value}))";
        }
    }

    public class BooleanLiteral : Expression
    {
        public Token Tok;
        public bool Value;
        public BooleanLiteral(Token tok, bool value)
        {
            Tok = tok;
            Value = value;
        }

        public string TokenLiteral()
        {
            return Tok.Literal;
        }

        public override string ToString()
        {
            return $"(bool ({Value}))";
        }
    }

    public class StringLiteral : Expression
    {
        public Token Tok;
        public string Value;

        public StringLiteral(Token tok, string value)
        {
            Tok = tok;
            Value = value;
        }

        public string TokenLiteral()
        {
            return Tok.Literal;
        }

        public override string ToString()
        {
            return $"(string (\"{Value}\"))";
        }
    }

    public class ArrayLiteral : Expression
    {
        public Token Tok;
        public List<Expression> Value;

        public ArrayLiteral(Token tok, List<Expression> value)
        {
            Tok = tok;
            Value = value;
        }

        public string TokenLiteral()
        {
            return Tok.Literal;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder("(array");
            foreach (Expression e in Value)
            {
                StringBuilder eSb = new StringBuilder("\n  ");
                eSb.Append(e.ToString());
                eSb.Replace("\n", "\n    ");
                sb.Append(eSb);
            }

            return sb.ToString();
        }
    }

    public class HashLiteral : Expression
    {
        public Token Tok;
        public Dictionary<Expression, Expression> Value;

        public HashLiteral(Token tok, Dictionary<Expression, Expression> value)
        {
            Tok = tok;
            Value = value;
        }

        public string TokenLiteral()
        {
            return Tok.Literal;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder("(hash");
            foreach (KeyValuePair<Expression, Expression> pair in Value)
            {
                StringBuilder pairSb = new StringBuilder("\n  (pair\n    ");

                StringBuilder keySb = new StringBuilder(pair.Key.ToString());
                keySb.Replace("\n", "\n    ");
                pairSb.Append(keySb);
                pairSb.Append("\n    ");

                StringBuilder valueSb = new StringBuilder(pair.Value.ToString());
                valueSb.Replace("\n", "\n    ");
                pairSb.Append(valueSb);
                pairSb.Append(")");

                sb.Append(pairSb);
            }

            return sb.ToString();
        }

    }

    public class IndexExpression : Expression
    {
        public Token Tok;
        public Expression Lhs;
        public Expression Index;

        public IndexExpression(Token tok, Expression lhs, Expression index)
        {
            Tok = tok;
            Lhs = lhs;
            Index = index;
        }

        public string TokenLiteral()
        {
            return Tok.Literal;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder("(access\n  ");

            StringBuilder lhsSb = new StringBuilder(Lhs.ToString());
            lhsSb.Replace("\n", "\n    ");
            sb.Append(lhsSb);
            sb.Append("\n  ");

            StringBuilder indexSb = new StringBuilder(Index.ToString());
            indexSb.Replace("\n", "\n    ");
            sb.Append(indexSb);
            sb.Append(")");

            return sb.ToString();
        }
    }

    public class FunctionLiteral : Expression
    {
        public Token Tok;
        public List<Identifier> Args;
        public List<Statement> Body;

        public FunctionLiteral(Token tok, List<Identifier> args, List<Statement> body)
        {
            Tok = tok;
            Args = args;
            Body = body;
        }

        public string TokenLiteral()
        {
            return Tok.Literal;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder("(function_definition\n  ");

            StringBuilder args = new StringBuilder("(args\n");
            foreach (Identifier a in Args)
            {
                args.Append("    ");
                args.AppendLine(a.ToString());
            }

            args.Append("  )\n  ");

            sb.Append(args);

            StringBuilder body = new StringBuilder("(body\n");
            foreach (Statement s in Body)
            {
                body.Append("    ");
                StringBuilder sIndent = new StringBuilder(s.ToString());
                sIndent.Replace("\n", "\n    ");
                body.AppendLine(sIndent.ToString());
            }

            body.Append("  )");

            sb.Append(body);
            sb.Append(")");

            return sb.ToString();
        }
    }

    public class Call : Expression
    {
        public Token Tok;
        public Expression Function;
        public List<Expression> Args;

        public Call(Token tok, Expression function, List<Expression> args)
        {
            Tok = tok;
            Function = function;
            Args = args;
        }

        public string TokenLiteral()
        {
            return Tok.Literal;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder("(call\n  ");

            StringBuilder function = new StringBuilder(Function.ToString());
            function.Replace("\n", "\n    ");
            sb.Append(function);
            sb.Append("\n  (args");

            foreach (Expression arg in Args)
            {
                StringBuilder argsb = new StringBuilder(arg.ToString());
                argsb.Replace("\n", "\n      ");
                sb.Append("\n    ");
                sb.Append(argsb);
            }

            sb.Append(')');

            return sb.ToString();
        }
    }

    public class PrefixOperator : Expression
    {
        public Token Tok;
        public string Operator;
        public Expression Value;

        public PrefixOperator(Token token, string op, Expression value)
        {
            Tok = token;
            Operator = op;
            Value = value;
        }

        public string TokenLiteral()
        {
            return Tok.Literal;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder($"(unary_operator\n  ({Operator})\n  ");
            StringBuilder valString = new StringBuilder(Value.ToString());
            valString.Replace("\n", "\n  ");
            sb.Append(valString);
            sb.Append(")");

            return sb.ToString();
        }
    }

    public class InfixOperator : Expression
    {
        public Token Tok;
        public string Operator;
        public Expression Lhs;
        public Expression Rhs;

        public InfixOperator(Token token, string op, Expression lhs, Expression rhs)
        {
            Tok = token;
            Operator = op;
            Lhs = lhs;
            Rhs = rhs;
        }

        public string TokenLiteral()
        {
            return Tok.Literal;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder($"(binary_operator\n  ({Operator})\n  ");
            StringBuilder lhsString = new StringBuilder(Lhs.ToString());
            lhsString.Replace("\n", "\n  ");
            StringBuilder rhsString = new StringBuilder(Rhs.ToString());
            rhsString.Replace("\n", "\n  ");
            sb.Append(lhsString.ToString());
            sb.Append("\n  ");
            sb.Append(rhsString.ToString());
            sb.Append(")");
            return sb.ToString();
        }
    }

    public class IfExpression : Expression
    {
        public Token Tok;
        public Expression Condition;
        public List<Statement> Consequence;
        public List<Statement> Otherwise;

        public IfExpression(Token tok, Expression condition, List<Statement> consequence, List<Statement> otherwise)
        {
            Tok = tok;
            Condition = condition;
            Consequence = consequence;
            Otherwise = otherwise;
        }

        public string TokenLiteral()
        {
            return Tok.Literal;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder("(if\n  ");
            StringBuilder condition = new StringBuilder(Condition.ToString());
            condition.Replace("\n", "\n    ");
            sb.Append($"{condition.ToString()}\n  ");

            StringBuilder consequence = new StringBuilder("(consequence");
            foreach (Statement statement in Consequence)
            {
                consequence.Append("\n    ");
                StringBuilder statementString = new StringBuilder(statement.ToString());
                statementString.Replace("\n", "\n      ");
                consequence.Append(statementString);
            }

            sb.Append(consequence);
            sb.Append("\n  ");

            StringBuilder otherwise = new StringBuilder("(otherwise");
            foreach (Statement statement in Otherwise)
            {
                otherwise.Append("\n    ");
                StringBuilder statementString = new StringBuilder(statement.ToString());
                statementString.Replace("\n", "\n      ");
                otherwise.Append(statementString);
            }

            sb.Append(otherwise);
            sb.Append("\n  ");

            return sb.ToString();
        }
    }

    public class LetStatement : Statement
    {
        public Token Tok;
        public Identifier Name;
        public Expression Value;

        public LetStatement(Token token, Identifier name, Expression value)
        {
            Tok = token;
            Name = name;
            Value = value;
        }

        public string TokenLiteral()
        {
            return Tok.Literal;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder("(let\n");
            sb.Append("  Right: ");
            sb.AppendLine(Name.ToString());
            sb.Append("  Left: ");
            sb.AppendLine(Value.ToString());
            sb.Append(")");
            return sb.ToString();
        }
    }

    public class ReturnStatement : Statement
    {
        public Token Tok;
        public Expression Value;

        public ReturnStatement(Token t, Expression value)
        {
            Tok = t;
            Value = value;
        }

        public string TokenLiteral()
        {
            return Tok.Literal;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder("(return_statement\n  ");
            StringBuilder value = new StringBuilder(Value.ToString());
            value.Replace("\n", "\n  ");
            sb.Append(value.ToString());
            sb.Append(")");

            return sb.ToString();
        }
    }

    public class ExpressionStatement : Statement
    {
        public Token Tok;
        public Expression Value;

        public ExpressionStatement(Token t, Expression value)
        {
            Tok = t;
            Value = value;
        }

        public string TokenLiteral()
        {
            return Tok.Literal;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder($"(expression_statement\n  ");
            StringBuilder valSb = new StringBuilder(Value.ToString());
            valSb.Replace("\n", "\n  ");
            sb.Append($"{valSb})");
            return sb.ToString();
        }
    }

    public class Parser
    {
        private Dictionary<TokenType, Func<Expression?>> PrefixTable;
        private Dictionary<TokenType, Func<Expression, Expression?>> InfixTable;
        private Dictionary<TokenType, int> OperatorPrecedence = new Dictionary<TokenType, int> {
            {TokenType.Equal, Precedences.EQUALS},
            {TokenType.NotEqual, Precedences.EQUALS},
            {TokenType.Less, Precedences.LESSGREATER},
            {TokenType.Greater, Precedences.LESSGREATER},
            {TokenType.Plus, Precedences.SUM},
            {TokenType.Minus, Precedences.SUM},
            {TokenType.Caret, Precedences.SUM},
            {TokenType.Asterisk, Precedences.PRODUCT},
            {TokenType.Slash, Precedences.PRODUCT},
            {TokenType.LParen, Precedences.CALL},
            {TokenType.LBracket, Precedences.INDEX}
        };

        private Lexer Lex;
        private Token CurrentToken;
        private Token PeekToken;
        private List<String> ErrorsList;

        public Parser(Lexer lexer)
        {
            Lex = lexer;

            ErrorsList = new List<string>();

            NextToken();
            NextToken();

            PrefixTable = new Dictionary<TokenType, Func<Expression?>>();
            PrefixTable.Add(TokenType.Identifier, ParseIdent);
            PrefixTable.Add(TokenType.Int, ParseInt);
            PrefixTable.Add(TokenType.True, ParseBoolean);
            PrefixTable.Add(TokenType.False, ParseBoolean);
            PrefixTable.Add(TokenType.String, ParseString);
            PrefixTable.Add(TokenType.LBracket, ParseArray);
            PrefixTable.Add(TokenType.LBrace, ParseHash);
            PrefixTable.Add(TokenType.Function, ParseFunction);
            PrefixTable.Add(TokenType.Exclam, ParsePrefixOp);
            PrefixTable.Add(TokenType.Minus, ParsePrefixOp);
            PrefixTable.Add(TokenType.LParen, ParseGrouped);
            PrefixTable.Add(TokenType.If, ParseIf);
            InfixTable = new Dictionary<TokenType, Func<Expression, Expression?>>();
            InfixTable.Add(TokenType.Minus, ParseInfixOp);
            InfixTable.Add(TokenType.Plus, ParseInfixOp);
            InfixTable.Add(TokenType.Asterisk, ParseInfixOp);
            InfixTable.Add(TokenType.Slash, ParseInfixOp);
            InfixTable.Add(TokenType.Caret, ParseInfixOp);
            InfixTable.Add(TokenType.Equal, ParseInfixOp);
            InfixTable.Add(TokenType.NotEqual, ParseInfixOp);
            InfixTable.Add(TokenType.Less, ParseInfixOp);
            InfixTable.Add(TokenType.Greater, ParseInfixOp);
            InfixTable.Add(TokenType.LParen, ParseCall);
            InfixTable.Add(TokenType.LBracket, ParseIndexExpression);
        }

        private void NextToken()
        {
            CurrentToken = PeekToken;
            PeekToken = Lex.NextToken();
        }
        public Program ParseProgram()
        {
            Program program = new Program();
            ErrorsList = new List<string>();

            while (CurrentToken.Type != TokenType.Eof)
            {
                Statement? stmt = ParseStatement();
                if (stmt is { } s)
                {
                    program.Statements.Add(s);
                }
                else
                {
                    break;
                }
            }
            return program;
        }

        private Statement? ParseStatement()
        {
            return CurrentToken.Type switch
            {
                TokenType.Let => ParseLetStatement(),
                TokenType.Return => ParseReturnStatement(),
                _ => ParseExpressionStatement()
            };
        }

        private Statement? ParseLetStatement()
        {
            Token letToken = CurrentToken;

            if (!ExpectPeek(TokenType.Identifier))
            {
                return null;
            }

            Identifier varName = new Identifier(CurrentToken, CurrentToken.Literal);

            if (!ExpectPeek(TokenType.Assign))
            {
                return null;
            }

            NextToken();

            Expression? exp = ParseExpression(Precedences.LOWEST);

            if (exp is { } e)
            {
                if (CurrentToken.Type == TokenType.Semicolon)
                {
                    NextToken();
                    return new LetStatement(letToken, varName, exp);
                }
                else
                {
                    PeekError(TokenType.Semicolon);
                }
            }

            ExpressionError();
            return null;
        }

        private Statement? ParseReturnStatement()
        {
            Token returnToken = CurrentToken;
            NextToken();
            Expression? exp = ParseExpression(Precedences.LOWEST);
            if (exp is { } e)
            {
                if (CurrentToken.Type == TokenType.Semicolon)
                {
                    NextToken();
                    return new ReturnStatement(returnToken, e);
                }
                else
                {
                    PeekError(TokenType.Semicolon);
                }
            }

            ExpressionError();
            return null;
        }

        private Statement? ParseExpressionStatement()
        {
            Token expressionToken = CurrentToken;
            Expression? exp = ParseExpression(Precedences.LOWEST);
            if (exp is { } e)
            {
                // semicolons are optional for expression statements
                if (CurrentToken.Type == TokenType.Semicolon)
                {
                    NextToken();
                }

                return new ExpressionStatement(expressionToken, e);
            }

            return null;
        }

        private Expression? ParseExpression(int precedence)
        {
            try
            {
                Func<Expression?> prefix = PrefixTable[CurrentToken.Type];
                Expression? leftExp = prefix();
                if (leftExp == null)
                {
                    return null;
                }

                while (PeekToken.Type != TokenType.Semicolon && precedence < CurrentPrecedence())
                {
                    try
                    {
                        Func<Expression, Expression?> infix = InfixTable[CurrentToken.Type];

                        if (leftExp is { } lhs)
                        {
                            leftExp = infix(lhs);
                        }
                    }
                    catch (KeyNotFoundException)
                    {
                        return leftExp;
                    }
                }

                return leftExp;
            }
            catch (KeyNotFoundException)
            {
                ErrorsList.Add($"{CurrentToken.Literal} is not prefix/left side value for expression");
                return null;
            }
        }

        private IntLiteral ParseInt()
        {
            IntLiteral result = new IntLiteral(CurrentToken, int.Parse(CurrentToken.Literal));
            NextToken();
            return result;
        }

        private BooleanLiteral ParseBoolean()
        {
            BooleanLiteral result = new BooleanLiteral(CurrentToken, bool.Parse(CurrentToken.Literal));
            NextToken();
            return result;
        }

        private StringLiteral ParseString()
        {
            string value = CurrentToken.Literal.Substring(1, CurrentToken.Literal.Count() - 2);
            StringLiteral result = new StringLiteral(CurrentToken, value);
            NextToken();
            return result;
        }

        private ArrayLiteral? ParseArray()
        {
            Token arrayToken = CurrentToken;

            List<Expression> value = new List<Expression>();
            NextToken();

            while (CurrentToken.Type != TokenType.RBracket)
            {
                Expression? nextVal = ParseExpression(Precedences.LOWEST);
                if (nextVal == null)
                {
                    ExpressionError();
                    return null;
                }

                value.Add(nextVal);

                if (CurrentToken.Type == TokenType.RBracket)
                {
                    break;
                }

                if (CurrentToken.Type != TokenType.Comma)
                {
                    CurrentError(TokenType.Comma);
                    return null;
                }

                NextToken();
            }

            if (CurrentToken.Type != TokenType.RBracket)
            {
                CurrentError(TokenType.RBracket);
                return null;
            }
            NextToken();

            return new ArrayLiteral(arrayToken, value);
        }

        private HashLiteral? ParseHash()
        {
            Token hashToken = CurrentToken;

            Dictionary<Expression, Expression> value = new Dictionary<Expression, Expression>();
            NextToken();

            while (CurrentToken.Type != TokenType.RBrace)
            {
                Expression? nextKey = ParseExpression(Precedences.LOWEST);
                if (nextKey == null)
                {
                    ExpressionError();
                    return null;
                }

                if (CurrentToken.Type != TokenType.Colon)
                {
                    CurrentError(TokenType.Colon);
                    return null;
                }
                NextToken();

                Expression? nextValue = ParseExpression(Precedences.LOWEST);
                if (nextValue == null)
                {
                    ExpressionError();
                    return null;
                }

                try
                {
                    value.Add(nextKey, nextValue);
                }
                catch (ArgumentException)
                {
                    ErrorsList.Add("Duplicate key in hash literal");
                    return null;
                }

                if (CurrentToken.Type == TokenType.RBrace)
                {
                    break;
                }

                if (CurrentToken.Type != TokenType.Comma)
                {
                    CurrentError(TokenType.Comma);
                    return null;
                }

                NextToken();
            }

            if (CurrentToken.Type != TokenType.RBrace)
            {
                CurrentError(TokenType.RBrace);
                return null;
            }
            NextToken();

            return new HashLiteral(hashToken, value);
        }


        private Identifier ParseIdent()
        {
            Identifier result = new Identifier(CurrentToken, CurrentToken.Literal);
            NextToken();
            return result;
        }

        private FunctionLiteral? ParseFunction()
        {
            Token fnToken = CurrentToken;
            List<Identifier> args = new List<Identifier>();
            List<Statement> body = new List<Statement>();

            // parse arguments

            ExpectPeek(TokenType.LParen);
            NextToken();
            if (CurrentToken.Type != TokenType.RParen && CurrentToken.Type != TokenType.Identifier)
            {
                if (CurrentToken.Type == TokenType.Eof)
                {
                    ErrorsList.Add("Expected ')' or identifier, found Eof");
                }
                else
                {
                    ErrorsList.Add($"Expected ')' or identifier, found {CurrentToken.Literal}");
                }

                return null;
            }

            while (CurrentToken.Type != TokenType.RParen)
            {
                Identifier nextArg = ParseIdent();
                args.Add(nextArg);

                if (CurrentToken.Type == TokenType.RParen)
                {
                    break;
                }

                if (CurrentToken.Type != TokenType.Comma)
                {
                    CurrentError(TokenType.Comma);
                    return null;
                }

                NextToken();
                if (CurrentToken.Type != TokenType.Identifier)
                {
                    CurrentError(TokenType.Identifier);
                    return null;
                }
            }

            if (CurrentToken.Type != TokenType.RParen)
            {
                CurrentError(TokenType.RParen);
                return null;
            }

            // parse body

            ExpectPeek(TokenType.LBrace);
            NextToken();
            while (CurrentToken.Type != TokenType.RBrace && CurrentToken.Type != TokenType.Eof)
            {
                Statement? nextStatement = ParseStatement();
                if (nextStatement == null)
                {
                    return null;
                }

                body.Add(nextStatement);
            }

            if (CurrentToken.Type != TokenType.RBrace)
            {
                CurrentError(TokenType.RBrace);
                return null;
            }
            NextToken();

            return new FunctionLiteral(fnToken, args, body);
        }

        private Call? ParseCall(Expression lhs)
        {
            Token callToken = CurrentToken;
            List<Expression> args = new List<Expression>();
            NextToken();

            while (CurrentToken.Type != TokenType.RParen)
            {
                Expression? nextArg = ParseExpression(Precedences.LOWEST);
                if (nextArg == null)
                {
                    ExpressionError();
                    return null;
                }

                args.Add(nextArg);

                if (CurrentToken.Type == TokenType.RParen)
                {
                    break;
                }

                if (CurrentToken.Type != TokenType.Comma)
                {
                    CurrentError(TokenType.Comma);
                    return null;
                }

                NextToken();
            }

            if (CurrentToken.Type != TokenType.RParen)
            {
                CurrentError(TokenType.RParen);
                return null;
            }
            NextToken();

            return new Call(callToken, lhs, args);
        }

        private IndexExpression? ParseIndexExpression(Expression lhs)
        {
            Token tok = CurrentToken;
            NextToken();

            Expression? index = ParseExpression(Precedences.LOWEST);
            if (index == null)
            {
                return null;
            }

            if (CurrentToken.Type != TokenType.RBracket)
            {
                CurrentError(TokenType.RBracket);
                return null;
            }
            NextToken();

            return new IndexExpression(tok, lhs, index);
        }


        private PrefixOperator? ParsePrefixOp()
        {
            Token opToken = CurrentToken;
            string opType = CurrentToken.Literal;

            NextToken();
            Expression? rhs = ParseExpression(Precedences.PREFIX);

            if (rhs is { } r)
            {
                return new PrefixOperator(opToken, opType, r);
            }

            return null;
        }

        private Expression? ParseGrouped()
        {
            NextToken();
            Expression? exp = ParseExpression(Precedences.LOWEST);
            if (CurrentToken.Type == TokenType.RParen)
            {
                NextToken();
            }
            else
            {
                PeekError(TokenType.RParen);
            }

            return exp;
        }

        private InfixOperator? ParseInfixOp(Expression lhs)
        {
            Token opToken = CurrentToken;
            string opType = CurrentToken.Literal;

            int precedence = CurrentPrecedence();
            NextToken();
            Expression? rhs = ParseExpression(precedence);

            if (rhs is { } r)
            {
                return new InfixOperator(opToken, opType, lhs, rhs);
            }

            return null;
        }

        private IfExpression? ParseIf()
        {
            Token ifToken = CurrentToken;
            ExpectPeek(TokenType.LParen);
            NextToken();
            Expression? condition = ParseExpression(Precedences.LOWEST);

            if (condition == null)
            {
                ExpressionError();
                return null;
            }

            if (CurrentToken.Type != TokenType.RParen)
            {
                CurrentError(TokenType.RParen);
                return null;
            }

            ExpectPeek(TokenType.LBrace);
            NextToken();

            List<Statement> consequence = new List<Statement>();
            while (CurrentToken.Type != TokenType.RBrace && CurrentToken.Type != TokenType.Eof)
            {
                Statement? nextStatement = ParseStatement();
                if (nextStatement == null)
                {
                    return null;
                }

                consequence.Add(nextStatement);
            }

            if (CurrentToken.Type != TokenType.RBrace)
            {
                CurrentError(TokenType.RBrace);
                return null;
            }
            NextToken();


            if (CurrentToken.Type != TokenType.Else)
            {
                CurrentError(TokenType.Else);
                return null;
            }
            ExpectPeek(TokenType.LBrace);
            NextToken();

            List<Statement> otherwise = new List<Statement>();
            while (CurrentToken.Type != TokenType.RBrace && CurrentToken.Type != TokenType.Eof)
            {
                Statement? nextStatement = ParseStatement();
                if (nextStatement == null)
                {
                    return null;
                }

                otherwise.Add(nextStatement);
            }

            if (CurrentToken.Type != TokenType.RBrace)
            {
                CurrentError(TokenType.RBrace);
                return null;
            }
            NextToken();

            return new IfExpression(ifToken, condition, consequence, otherwise);
        }

        private int PeekPrecedence()
        {
            try
            {
                return OperatorPrecedence[PeekToken.Type];
            }
            catch (KeyNotFoundException)
            {
                return 0;
            }
        }

        private int CurrentPrecedence()
        {
            try
            {
                return OperatorPrecedence[CurrentToken.Type];
            }
            catch (KeyNotFoundException)
            {
                return 0;
            }
        }

        /// <summary>
        /// Advances the parser if and only if the peek token has the given type.
        /// </summary>
        private bool ExpectPeek(TokenType t)
        {
            if (PeekToken.Type == t)
            {
                NextToken();
                return true;
            }
            else
            {
                PeekError(t);
                return false;
            }
        }

        private void PeekError(TokenType t)
        {
            if (PeekToken.Type == TokenType.Eof)
            {
                ErrorsList.Add($"expected {t}, got Eof");
            }
            else
            {
                ErrorsList.Add($"expected {t}, got {PeekToken.Type}");
            }
        }

        private void CurrentError(TokenType t)
        {
            if (CurrentToken.Type == TokenType.Eof)
            {
                ErrorsList.Add($"expected {t}, got Eof");
            }
            else
            {
                ErrorsList.Add($"expected {t}, got {CurrentToken.Type}");
            }
        }

        private void ExpressionError()
        {
            ErrorsList.Add($"Invalid expression: '{CurrentToken.Literal}'");
        }

        public List<string> Errors()
        {
            return ErrorsList;
        }
    }
}

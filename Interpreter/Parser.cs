namespace Interpreter
{
    public interface Node
    {
        public string TokenLiteral();
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
    }

    public class Identifier : Expression
    {
        Token Tok;
        string Value;

        public Identifier(Token token, string value)
        {
            Tok = token;
            Value = value;
        }

        public string TokenLiteral()
        {
            return Tok.Literal;
        }
    }

    public class IntLiteral : Expression
    {
        Token Tok;
        int Value;
        public IntLiteral(Token token, int value)
        {
            Tok = token;
            Value = value;
        }

        public string TokenLiteral()
        {
            return Tok.Literal;
        }
    }

    public class LetStatement : Statement
    {
        Token Tok;
        Identifier Name;
        Expression Value;

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
    }

    public class Parser
    {
        private Lexer Lex;
        private Token CurrentToken;
        private Token PeekToken;

        public Parser(Lexer lexer)
        {
            Lex = lexer;

            NextToken();
            NextToken();
        }

        private void NextToken()
        {
            CurrentToken = PeekToken;
            PeekToken = Lex.NextToken();
        }
        public Program ParseProgram()
        {
            Program p = new Program();
            return p;
        }
    }
}

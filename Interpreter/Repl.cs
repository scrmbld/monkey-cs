using Interpreter;

namespace Repl
{
    public static class Repl
    {
        public static string PROMPT = ">> ";

        private static void LexLine(string line)
        {
            Lexer l = new Lexer(line);
            for (var token = l.NextToken(); token.Type != TokenType.Eof; token = l.NextToken())
            {
                Console.WriteLine($"{token.Type}, \"{token.Literal}\"");
            }

        }

        private static void ParseLine(string line)
        {
            Lexer l = new Lexer(line);
            Parser p = new Parser(l);
            Interpreter.Program prog = p.ParseProgram();
            foreach (string err in p.Errors())
            {
                Console.WriteLine(err);
            }
            Console.WriteLine(prog);
        }

        private static void EvalLine(string line, Interpreter.Environment env)
        {
            Parser p = new Parser(new Lexer(line));
            Node program = p.ParseProgram();
            foreach (string err in p.Errors())
            {
                Console.WriteLine(err);
            }

            Evaluator e = new Evaluator();

            MonkeyObject result = e.Eval(program, env);

            Console.WriteLine(result);
        }

        public static void Start()
        {
            Interpreter.Environment env = new Interpreter.Environment();
            Console.Write(PROMPT);
            while (true)
            {
                string? s = Console.ReadLine();
                if (s is { } line)
                {
                    EvalLine(line, env);
                }
                else
                {
                    break;
                }
                Console.Write(PROMPT);
            }
        }
    }
}

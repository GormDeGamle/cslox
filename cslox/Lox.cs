using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Linq;

namespace cslox
{
    class Lox
    {
        private static readonly Interpreter Interpreter = new Interpreter();

        static Boolean hadError = false;
        static Boolean hadRuntimeError = false;

        //*****************
        //*** Main loop ***
        //*****************
        static void Main(string[] args)
        {
            if (args.Length > 1)
            {
                //*** to many arguments
                Help();
            } else if (args.Length == 1) 
            {
                if (args[0] == "?" | args[0] == "-?" | args[0] == "/?")
                {
                    //*** Help needed
                    Help();
                } else
                {
                    //*** File to parse
                    RunFile(args[0]);
                }
            } else
            {
                //*** Start command line
                RunPrompt();
            }
        }

        //************
        //*** Help ***
        //************
        private static void Help()
        {
            Console.WriteLine("cslox - C# Lox interpreter\n");
            Console.WriteLine("Usage: cslox [script] -> execute script");
            Console.WriteLine("       cslox          -> command prompt (type \"exit\" to quit)");
        }

        //****************************
        //*** Executing lox scipts ***
        //****************************

        //*** Run a lox file
        private static void RunFile(string Path)
        {
            string Program = File.ReadAllText(Path);

            Run(Program);
            
            if (hadError) { 
                System.Environment.ExitCode = (int)ExitCode.SyntaxError; 
            }
            if (hadRuntimeError) {
                System.Environment.ExitCode = (int)ExitCode.RuntimeError; 
            }
        }

        //*** Show a prompt and wait for input
        private static void RunPrompt()
        {
            while (true) {
                Console.Write("> ");
                string Line = Console.ReadLine();
                if (Line.Trim().ToLower() == "exit") { break; }

                //*** Chapter 8 - Challenge 1 - evaluate and print expressions from REPL - add a ; if it's missing
                if (Line.Length > 0) if (Line.Last() != ';') Line = Line + ";"; 

                Run(Line, true); //fromPrompt = true -> simply evaluate end print expression if entered

                hadError = false;
            }
        }

        //*** Run a string containing Lox commands
        private static void Run(string Source, Boolean fromPrompt = false)
        {
            Scanner aScanner = new Scanner(Source);
            List<Token> aTokens = aScanner.ScanTokens();
            Parser aParser = new Parser(aTokens);
            List<Stmt> Statements = aParser.Parse();

            //Stop if there was a syntax error
            if (hadError) return;

            // Use the AstPrinter to visualize the syntax tree
            //Console.WriteLine(new AstPrinter().Print(Expression));

            // For now, just print the tokens.
            /*
            foreach (Token aToken in aTokens)
            {
                Console.WriteLine(aToken.ToString());
            }
            */
            /*
            foreach (Stmt aStatement in Statements)
            {
                Console.WriteLine(aStatement.ToString());
            }
            */

            Resolver Resolver = new Resolver(Interpreter);
            Resolver.Resolve(Statements);

            //Stop if there was an error with name resolution
            if (hadError) return;

            //*** Chapter 8 - Challenge 1 - evaluate and print expressions from REPL
            if (fromPrompt)
            {
                if (Statements.Count > 0)
                {
                    if (Statements[0] is Stmt.Expression)
                    {
                        //just add a PRINT Statement in front...
                        Statements.Insert(0, new Stmt.Print(((Stmt.Expression)Statements[0]).Exprsn));
                    }
                }
            }

            Interpreter.Interpret(Statements);
        }

        //**********************
        //*** Error handling ***
        //**********************
        public static void Error(int Line, string Message)
        {
            Report(Line, "", Message);
        }
        public static void Error(Token aToken, string Message)
        {
            if (aToken.Type == TokenType.EOF)
            {
                Report(aToken.Line, " at end", Message);
            } else
            {
                Report(aToken.Line, " at '" + aToken.Lexeme + "'", Message);
            }
        }
        public static void RuntimeError(RuntimeError Error)
        {
            Console.Error.WriteLine(Error.Message + "\n[line " + Error.ErrorToken.Line + "]");
            hadRuntimeError = true;
        }
        private static void Report(int Line, string Where, string Message)
        {
            Console.Error.WriteLine("[line " + Line + "] Error" + Where + ": " + Message);
            hadError = true;
        }

    }
}

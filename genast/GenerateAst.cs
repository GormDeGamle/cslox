using System;
using System.IO;
using System.Collections.Generic;

namespace genast
{
    class GenerateAst
    {
        //*****************
        //*** Main loop ***
        //*****************
        static void Main(string[] args)
        {
            if (args.Length != 1)
            {
                //*** to many arguments
                Help();
            }
            else if (args.Length == 1)
            {
                if (args[0] == "?" | args[0] == "-?" | args[0] == "/?")
                {
                    //*** Help needed
                    Help();
                }
                else
                {
                    //*** File to parse
                    string OutputDir = args[0];
                    DefineAST(OutputDir, "Expr", new List<string> 
                    {
                        "Assign   : Token Name, Expr Value",
                        "Binary   : Expr Left, Token Operator, Expr Right",
                        "Ternary  : Expr Condition, Expr Left, Expr Right",
                        "Lambda   : List<Token> Params, List<Stmt> Body", //*** Chapter 10 - Challenge 2
                        "Call     : Expr Callee, Token Parenthesis, List<Expr> Arguments",
                        "Get      : Expr Object, Token Name",
                        "Grouping : Expr Expression",
                        "Literal  : Object Value",
                        "Logical  : Expr Left, Token Operator, Expr Right",
                        "Set      : Expr Object, Token Name, Expr Value",
                        "Super   : Token KeyWord, Token Method",
                        "This     : Token KeyWord",
                        "Unary    : Token Operator, Expr Right",
                        "Variable : Token Name"
                    });

                    DefineAST(OutputDir, "Stmt", new List<string>
                    {
                        "Block      : List<Stmt> Statements",
                        "Class      : Token Name, Expr.Variable SuperClass, List<Stmt.Function> ClassMethods, List<Stmt.Function> Methods", //*** Chapter 12 - Challenge 1
                        "Expression : Expr Exprsn",
                        "Function   : Token Name, List<Token> Params, List<Stmt> Body",
                        "If         : Expr Condition, Stmt ThenBranch, Stmt ElseBranch",
                        "Print      : Expr Exprsn",
                        "Return     : Token KeyWord, Expr Value",
                        "Break      : Token KeyWord", //*** Chapter 9 - Challenge 3
                        "Var        : Token Name, Expr Initializer",
                        "While      : Expr Condition, Stmt Body"
                    });
                }
            }
        }
        //************
        //*** Help ***
        //************
        private static void Help()
        {
            Console.WriteLine("genast - Generate AST for the C# Lox interpreter\n");
            Console.WriteLine("Usage: genast [output_dir] -> generate Expr.cs in specified directory");
        }
        //********************
        //*** Generate AST ***
        //********************
        private static void DefineAST(string OutputDir, string BaseName, List<string> Types)
        {
            string OutputFile = OutputDir.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
                              + Path.DirectorySeparatorChar
                              + BaseName + ".cs";

            Console.WriteLine("Generating " + OutputFile);

            StreamWriter Writer = new StreamWriter(OutputFile);

            //*** Define the super class
            Console.WriteLine("Defining " + BaseName);

            Writer.WriteLine("using System;");
            Writer.WriteLine("using System.Collections.Generic;");
            Writer.WriteLine();
            Writer.WriteLine("namespace cslox");
            Writer.WriteLine("{");
            Writer.WriteLine("  public abstract class " + BaseName);
            Writer.WriteLine("  {");

            //*** Define the interface for the visitor
            DefineVisitor(Writer, BaseName, Types);

            //*** Define the sub classes
            foreach (string Type in Types)
            {
                string ClassName = Type.Split(':')[0].Trim();
                string Fields = Type.Split(':')[1].Trim();
                DefineType(Writer, BaseName, ClassName, Fields);
            }
            Writer.WriteLine("      public abstract T Accept<T>(IVisitor<T> Visitor);");
            Writer.WriteLine("  }");
            Writer.WriteLine("}");
            Writer.Close();

            Console.WriteLine("Done!");
        }
        private static void DefineVisitor(StreamWriter Writer, string BaseName, List<string> Types)
        {
            Writer.WriteLine("      public interface IVisitor<T>");
            Writer.WriteLine("      {");
            foreach (string Type in Types)
            {
                string TypeName = Type.Split(":")[0].Trim();
                Writer.WriteLine("          T Visit" + TypeName + BaseName + "(" + TypeName + " " + BaseName + ");");
            }
            Writer.WriteLine("      }");
        }
        private static void DefineType(StreamWriter Writer, string BaseName, string ClassName, string FieldList)
        {
            Console.WriteLine("Defining " + ClassName);

            //*** Define sub class
            Writer.WriteLine("      public class " + ClassName + " : " + BaseName);
            Writer.WriteLine("      {");

            //*** Constructor
            Writer.WriteLine("          public "+ ClassName + "(" + FieldList + ")");
            Writer.WriteLine("          {");

            //*** Store paramters in fields
            string[] Fields = new string[] { };
            if (FieldList != "") 
            {
                Fields = FieldList.Split(", ");
                foreach (string Field in Fields) {
                    string Name = Field.Split(" ")[1];
                    Writer.WriteLine("              this." + Name + " = " + Name + ";");
                }
            }
            Writer.WriteLine("          }");

            //*** Vsisitor design pattern
            Writer.WriteLine();
            Writer.WriteLine("          public override T Accept<T>(IVisitor<T> Visitor)");
            Writer.WriteLine("          {");
            Writer.WriteLine("              return Visitor.Visit" + ClassName + BaseName + "(this);");
            Writer.WriteLine("          }");

            //*** Fields
            Writer.WriteLine();
            foreach (string Field in Fields)
            {
                Writer.WriteLine("          public readonly " + Field + ";");
            }
            Writer.WriteLine("      }");
        }
    }
}

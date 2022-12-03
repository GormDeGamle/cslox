using System;
using cslox;

namespace TestAstPrinter
{
    class Test
    {
        static void Main(string[] args)
        {
            Expr Expression = new Expr.Binary (
                new Expr.Unary (
                    new Token(TokenType.MINUS, "-", null, 1),
                    new Expr.Literal(123)
                ),
                new Token(TokenType.STAR, "*", null, 1),
                new Expr.Grouping (
                    new Expr.Literal(45.67)
                )
            );
            Console.WriteLine(new AstPrinter().Print(Expression));
        }
    }
}

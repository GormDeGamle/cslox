using System;
using cslox;

namespace testrpnprinter
{
    class Test
    {
        static void Main(string[] args)
        {
            Expr Expression = new Expr.Binary(
                new Expr.Grouping(
                    new Expr.Binary(
                        new Expr.Literal(1),
                        new Token(TokenType.PLUS, "+", null, 1),
                        new Expr.Grouping(
                            new Expr.Binary(
                                new Expr.Literal(6),
                                new Token(TokenType.SLASH, "/", null, 1),
                                new Expr.Literal(2)
                            )
                        )
                    )
                ),
                new Token(TokenType.STAR, "*", null, 1),
                new Expr.Grouping(
                    new Expr.Binary(
                        new Expr.Literal(4),
                        new Token(TokenType.MINUS, "-", null, 1),
                        new Expr.Literal(3)
                    )
                )
            );
            Console.WriteLine(new RpnPrinter().Print(Expression));
        }
    }
}

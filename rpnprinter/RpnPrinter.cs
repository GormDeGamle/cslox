using System;
using System.Collections.Generic;
using System.Text;

namespace cslox
{
    class RpnPrinter : Expr.IVisitor<string>
    {
        public string Print(Expr Expression)
        {
            return Expression.Accept(this);
        }
        public string VisitBinaryExpr(Expr.Binary Expression)
        {
            //*** Process groupings first
            if (!(Expression.Left is Expr.Grouping) && (Expression.Right is Expr.Grouping)) {
                return Expression.Right.Accept(this) + " " + Expression.Left.Accept(this) + " " + Expression.Operator.Lexeme;
            }
            else
            {
                return Expression.Left.Accept(this) + " " + Expression.Right.Accept(this) + " " + Expression.Operator.Lexeme;
            }
        }
        public string VisitGroupingExpr(Expr.Grouping Expression)
        {
            return Expression.Expression.Accept(this);
        }
        public string VisitLiteralExpr(Expr.Literal Expression)
        {
            return Expression.Value.ToString();
        }
        public string VisitUnaryExpr(Expr.Unary Expression)
        {
            return Expression.Right.Accept(this) + " " + Expression.Operator.Lexeme;
        }
    }
}

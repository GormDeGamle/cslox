using System;
using System.Collections.Generic;
using System.Text;

namespace cslox
{
    public class AstPrinter : Expr.IVisitor<string>
    {
        public string Print(Expr Expression)
        {
            return Expression.Accept(this);
        }
        public string VisitAssignExpr(Expr.Assign Expression)
        {
            return Paranthesize(Expression.Name.Lexeme, Expression.Value);
        }
        public string VisitBinaryExpr(Expr.Binary Expression)
        {
            return Paranthesize(Expression.Operator.Lexeme, Expression.Left, Expression.Right);
        }
        public string VisitTernaryExpr(Expr.Ternary Expression)
        {
            return Paranthesize("ternary", Expression.Condition, Expression.Left, Expression.Right);
        }
        public string VisitLambdaExpr(Expr.Lambda Expression)
        {
            return Paranthesize("lambda");
        }
        public string VisitCallExpr(Expr.Call Expression)
        {
           return Paranthesize(Paranthesize("call", Expression.Callee), Expression.Arguments.ToArray());
        }
        public string VisitGetExpr(Expr.Get Expression)
        {
            return Paranthesize("get", Expression);
        }
        public string VisitSetExpr(Expr.Set Expression)
        {
            return Paranthesize(Expression.Name.Lexeme, Expression.Value);
        }
        public string VisitSuperExpr(Expr.Super Expression)
        {
            return Paranthesize(Expression.KeyWord.Lexeme);
        }
        public string VisitThisExpr(Expr.This Expression)
        {
            return Paranthesize(Expression.KeyWord.Lexeme);
        }
        public string VisitGroupingExpr(Expr.Grouping Expression)
        {
            return Paranthesize("group", Expression.Expression);
        }
        public string VisitLiteralExpr(Expr.Literal Expression)
        {
            return Expression.Value.ToString();
        }
        public string VisitLogicalExpr(Expr.Logical Expression)
        {
            return Paranthesize(Expression.Operator.Lexeme, Expression.Left, Expression.Right);
        }
        public string VisitUnaryExpr(Expr.Unary Expression)
        {
            return Paranthesize(Expression.Operator.Lexeme, Expression.Right);
        }
        public string VisitVariableExpr(Expr.Variable Expression)
        {
            return Paranthesize("var", Expression);
        }
        private string Paranthesize(string Name, params Expr[] Expressions)
        {
            StringBuilder Builder = new StringBuilder();

            Builder.Append("(").Append(Name);
            foreach (Expr Expression in Expressions)
            {
                Builder.Append(" ");
                Builder.Append(Expression.Accept(this));
            }
            Builder.Append(")");

            return Builder.ToString();
        }
    }
}

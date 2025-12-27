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

        public string VisitAssignExpr(Expr.Assign Expression)
        {
            throw new NotImplementedException();
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

        public string VisitCallExpr(Expr.Call Expr)
        {
            throw new NotImplementedException();
        }

        public string VisitGetExpr(Expr.Get Expr)
        {
            throw new NotImplementedException();
        }

        public string VisitGroupingExpr(Expr.Grouping Expression)
        {
            return Expression.Expression.Accept(this);
        }

        public string VisitLambdaExpr(Expr.Lambda Expr)
        {
            throw new NotImplementedException();
        }

        public string VisitLiteralExpr(Expr.Literal Expression)
        {
            return Expression.Value.ToString();
        }

        public string VisitLogicalExpr(Expr.Logical Expr)
        {
            throw new NotImplementedException();
        }

        public string VisitSetExpr(Expr.Set Expr)
        {
            throw new NotImplementedException();
        }

        public string VisitSuperExpr(Expr.Super Expr)
        {
            throw new NotImplementedException();
        }

        public string VisitTernaryExpr(Expr.Ternary Expr)
        {
            throw new NotImplementedException();
        }

        public string VisitThisExpr(Expr.This Expr)
        {
            throw new NotImplementedException();
        }

        public string VisitUnaryExpr(Expr.Unary Expression)
        {
            return Expression.Right.Accept(this) + " " + Expression.Operator.Lexeme;
        }

        public string VisitVariableExpr(Expr.Variable Expr)
        {
            throw new NotImplementedException();
        }
    }
}

using System;
using System.Collections.Generic;

namespace cslox
{
  public abstract class Expr
  {
      public interface IVisitor<T>
      {
          T VisitAssignExpr(Assign Expr);
          T VisitBinaryExpr(Binary Expr);
          T VisitTernaryExpr(Ternary Expr);
          T VisitLambdaExpr(Lambda Expr);
          T VisitCallExpr(Call Expr);
          T VisitGetExpr(Get Expr);
          T VisitGroupingExpr(Grouping Expr);
          T VisitLiteralExpr(Literal Expr);
          T VisitLogicalExpr(Logical Expr);
          T VisitSetExpr(Set Expr);
          T VisitSuperExpr(Super Expr);
          T VisitThisExpr(This Expr);
          T VisitUnaryExpr(Unary Expr);
          T VisitVariableExpr(Variable Expr);
      }
      public class Assign : Expr
      {
          public Assign(Token Name, Expr Value)
          {
              this.Name = Name;
              this.Value = Value;
          }

          public override T Accept<T>(IVisitor<T> Visitor)
          {
              return Visitor.VisitAssignExpr(this);
          }

          public readonly Token Name;
          public readonly Expr Value;
      }
      public class Binary : Expr
      {
          public Binary(Expr Left, Token Operator, Expr Right)
          {
              this.Left = Left;
              this.Operator = Operator;
              this.Right = Right;
          }

          public override T Accept<T>(IVisitor<T> Visitor)
          {
              return Visitor.VisitBinaryExpr(this);
          }

          public readonly Expr Left;
          public readonly Token Operator;
          public readonly Expr Right;
      }
      public class Ternary : Expr
      {
          public Ternary(Expr Condition, Expr Left, Expr Right)
          {
              this.Condition = Condition;
              this.Left = Left;
              this.Right = Right;
          }

          public override T Accept<T>(IVisitor<T> Visitor)
          {
              return Visitor.VisitTernaryExpr(this);
          }

          public readonly Expr Condition;
          public readonly Expr Left;
          public readonly Expr Right;
      }
      public class Lambda : Expr
      {
          public Lambda(List<Token> Params, List<Stmt> Body)
          {
              this.Params = Params;
              this.Body = Body;
          }

          public override T Accept<T>(IVisitor<T> Visitor)
          {
              return Visitor.VisitLambdaExpr(this);
          }

          public readonly List<Token> Params;
          public readonly List<Stmt> Body;
      }
      public class Call : Expr
      {
          public Call(Expr Callee, Token Parenthesis, List<Expr> Arguments)
          {
              this.Callee = Callee;
              this.Parenthesis = Parenthesis;
              this.Arguments = Arguments;
          }

          public override T Accept<T>(IVisitor<T> Visitor)
          {
              return Visitor.VisitCallExpr(this);
          }

          public readonly Expr Callee;
          public readonly Token Parenthesis;
          public readonly List<Expr> Arguments;
      }
      public class Get : Expr
      {
          public Get(Expr Object, Token Name)
          {
              this.Object = Object;
              this.Name = Name;
          }

          public override T Accept<T>(IVisitor<T> Visitor)
          {
              return Visitor.VisitGetExpr(this);
          }

          public readonly Expr Object;
          public readonly Token Name;
      }
      public class Grouping : Expr
      {
          public Grouping(Expr Expression)
          {
              this.Expression = Expression;
          }

          public override T Accept<T>(IVisitor<T> Visitor)
          {
              return Visitor.VisitGroupingExpr(this);
          }

          public readonly Expr Expression;
      }
      public class Literal : Expr
      {
          public Literal(Object Value)
          {
              this.Value = Value;
          }

          public override T Accept<T>(IVisitor<T> Visitor)
          {
              return Visitor.VisitLiteralExpr(this);
          }

          public readonly Object Value;
      }
      public class Logical : Expr
      {
          public Logical(Expr Left, Token Operator, Expr Right)
          {
              this.Left = Left;
              this.Operator = Operator;
              this.Right = Right;
          }

          public override T Accept<T>(IVisitor<T> Visitor)
          {
              return Visitor.VisitLogicalExpr(this);
          }

          public readonly Expr Left;
          public readonly Token Operator;
          public readonly Expr Right;
      }
      public class Set : Expr
      {
          public Set(Expr Object, Token Name, Expr Value)
          {
              this.Object = Object;
              this.Name = Name;
              this.Value = Value;
          }

          public override T Accept<T>(IVisitor<T> Visitor)
          {
              return Visitor.VisitSetExpr(this);
          }

          public readonly Expr Object;
          public readonly Token Name;
          public readonly Expr Value;
      }
      public class Super : Expr
      {
          public Super(Token KeyWord, Token Method)
          {
              this.KeyWord = KeyWord;
              this.Method = Method;
          }

          public override T Accept<T>(IVisitor<T> Visitor)
          {
              return Visitor.VisitSuperExpr(this);
          }

          public readonly Token KeyWord;
          public readonly Token Method;
      }
      public class This : Expr
      {
          public This(Token KeyWord)
          {
              this.KeyWord = KeyWord;
          }

          public override T Accept<T>(IVisitor<T> Visitor)
          {
              return Visitor.VisitThisExpr(this);
          }

          public readonly Token KeyWord;
      }
      public class Unary : Expr
      {
          public Unary(Token Operator, Expr Right)
          {
              this.Operator = Operator;
              this.Right = Right;
          }

          public override T Accept<T>(IVisitor<T> Visitor)
          {
              return Visitor.VisitUnaryExpr(this);
          }

          public readonly Token Operator;
          public readonly Expr Right;
      }
      public class Variable : Expr
      {
          public Variable(Token Name)
          {
              this.Name = Name;
          }

          public override T Accept<T>(IVisitor<T> Visitor)
          {
              return Visitor.VisitVariableExpr(this);
          }

          public readonly Token Name;
      }
      public abstract T Accept<T>(IVisitor<T> Visitor);
  }
}

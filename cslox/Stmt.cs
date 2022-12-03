using System;
using System.Collections.Generic;

namespace cslox
{
  public abstract class Stmt
  {
      public interface IVisitor<T>
      {
          T VisitBlockStmt(Block Stmt);
          T VisitClassStmt(Class Stmt);
          T VisitExpressionStmt(Expression Stmt);
          T VisitFunctionStmt(Function Stmt);
          T VisitIfStmt(If Stmt);
          T VisitPrintStmt(Print Stmt);
          T VisitReturnStmt(Return Stmt);
          T VisitBreakStmt(Break Stmt);
          T VisitVarStmt(Var Stmt);
          T VisitWhileStmt(While Stmt);
      }
      public class Block : Stmt
      {
          public Block(List<Stmt> Statements)
          {
              this.Statements = Statements;
          }

          public override T Accept<T>(IVisitor<T> Visitor)
          {
              return Visitor.VisitBlockStmt(this);
          }

          public readonly List<Stmt> Statements;
      }
      public class Class : Stmt
      {
          public Class(Token Name, Expr.Variable SuperClass, List<Stmt.Function> ClassMethods, List<Stmt.Function> Methods)
          {
              this.Name = Name;
              this.SuperClass = SuperClass;
              this.ClassMethods = ClassMethods;
              this.Methods = Methods;
          }

          public override T Accept<T>(IVisitor<T> Visitor)
          {
              return Visitor.VisitClassStmt(this);
          }

          public readonly Token Name;
          public readonly Expr.Variable SuperClass;
          public readonly List<Stmt.Function> ClassMethods;
          public readonly List<Stmt.Function> Methods;
      }
      public class Expression : Stmt
      {
          public Expression(Expr Exprsn)
          {
              this.Exprsn = Exprsn;
          }

          public override T Accept<T>(IVisitor<T> Visitor)
          {
              return Visitor.VisitExpressionStmt(this);
          }

          public readonly Expr Exprsn;
      }
      public class Function : Stmt
      {
          public Function(Token Name, List<Token> Params, List<Stmt> Body)
          {
              this.Name = Name;
              this.Params = Params;
              this.Body = Body;
          }

          public override T Accept<T>(IVisitor<T> Visitor)
          {
              return Visitor.VisitFunctionStmt(this);
          }

          public readonly Token Name;
          public readonly List<Token> Params;
          public readonly List<Stmt> Body;
      }
      public class If : Stmt
      {
          public If(Expr Condition, Stmt ThenBranch, Stmt ElseBranch)
          {
              this.Condition = Condition;
              this.ThenBranch = ThenBranch;
              this.ElseBranch = ElseBranch;
          }

          public override T Accept<T>(IVisitor<T> Visitor)
          {
              return Visitor.VisitIfStmt(this);
          }

          public readonly Expr Condition;
          public readonly Stmt ThenBranch;
          public readonly Stmt ElseBranch;
      }
      public class Print : Stmt
      {
          public Print(Expr Exprsn)
          {
              this.Exprsn = Exprsn;
          }

          public override T Accept<T>(IVisitor<T> Visitor)
          {
              return Visitor.VisitPrintStmt(this);
          }

          public readonly Expr Exprsn;
      }
      public class Return : Stmt
      {
          public Return(Token KeyWord, Expr Value)
          {
              this.KeyWord = KeyWord;
              this.Value = Value;
          }

          public override T Accept<T>(IVisitor<T> Visitor)
          {
              return Visitor.VisitReturnStmt(this);
          }

          public readonly Token KeyWord;
          public readonly Expr Value;
      }
      public class Break : Stmt
      {
          public Break(Token KeyWord)
          {
              this.KeyWord = KeyWord;
          }

          public override T Accept<T>(IVisitor<T> Visitor)
          {
              return Visitor.VisitBreakStmt(this);
          }

          public readonly Token KeyWord;
      }
      public class Var : Stmt
      {
          public Var(Token Name, Expr Initializer)
          {
              this.Name = Name;
              this.Initializer = Initializer;
          }

          public override T Accept<T>(IVisitor<T> Visitor)
          {
              return Visitor.VisitVarStmt(this);
          }

          public readonly Token Name;
          public readonly Expr Initializer;
      }
      public class While : Stmt
      {
          public While(Expr Condition, Stmt Body)
          {
              this.Condition = Condition;
              this.Body = Body;
          }

          public override T Accept<T>(IVisitor<T> Visitor)
          {
              return Visitor.VisitWhileStmt(this);
          }

          public readonly Expr Condition;
          public readonly Stmt Body;
      }
      public abstract T Accept<T>(IVisitor<T> Visitor);
  }
}

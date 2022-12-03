using System;
using System.Collections.Generic;
using System.Text;

namespace cslox
{
    public class Resolver: Expr.IVisitor<Object>,
                           Stmt.IVisitor<Object>
    {
        private readonly Interpreter Interpreter;
        private readonly Stack<Dictionary<string, Resolvable>> Scopes = new Stack<Dictionary<string, Resolvable>>();
        private FunctionType CurrentFunction = FunctionType.NONE;
        private ClassType CurrentClass = ClassType.NONE;
        private LoopType CurrentLoop = LoopType.NONE;
        private int CurrentResolvableIndex = 0;

        private enum FunctionType
        {
            NONE,
            FUNCTION,
            INITIALIZER,
            CLASS_METHOD,
            METHOD,
            LAMBDA,
        }
        private enum ClassType
        {
            NONE,
            CLASS,
            SUBCLASS
        }
        private enum LoopType //No FOR - FOR is only syntactic suger for a while loop
        {
            NONE,
            WHILE 
        }
        private enum VarState //Chapter 11 - Challenge 3
        {
            NONE,
            DECLARED,
            DEFINED,
            USED
        }
        private struct Resolvable //Chapter 11 - Challenge 3
        {
            public Resolvable(Token Name, VarState State, int Index)
            {
                this.Name = Name;
                this.State = State;
                this.Index = Index;
            }

            public Token Name;
            public VarState State;
            public int Index; //Chapter 11 - Challgenge 4
        }

        public Resolver(Interpreter Interpreter)
        {
            this.Interpreter = Interpreter;
        }
        //*** Visitor pattern
        public Object VisitBlockStmt(Stmt.Block Statement)
        {
            BeginScope();
            Resolve(Statement.Statements);
            EndScope();
            return null;
        }
        public Object VisitClassStmt(Stmt.Class Statement)
        {
            ClassType EnclosingClass = CurrentClass;
            CurrentClass = ClassType.CLASS;

            Declare(Statement.Name);
            Define(Statement.Name);

            if (Statement.SuperClass != null && Statement.Name.Lexeme.Equals(Statement.SuperClass.Name.Lexeme)) 
            {
                Lox.Error(Statement.SuperClass.Name, "A class can't inherit from itself.");
            }

            if (Statement.SuperClass != null)
            {
                CurrentClass = ClassType.SUBCLASS;
                Resolve(Statement.SuperClass);
            }

            if (Statement.SuperClass != null)
            {
                BeginScope();
                Scopes.Peek().Add("super", new Resolvable(Statement.Name, VarState.USED, GetResolvableIndex())); //*** same as "this" - see below
            }

            BeginScope();
            Scopes.Peek().Add("this", new Resolvable(Statement.Name, VarState.USED, GetResolvableIndex())); //*** "this" is dynmic - it can potentially be used

            //*** Chapter 12 - Challenge 1
            foreach (Stmt.Function ClassMethod in Statement.ClassMethods)
            {
                FunctionType Declaration = FunctionType.CLASS_METHOD;
                if (ClassMethod.Name.Lexeme.Equals("init"))
                {
                    Declaration = FunctionType.INITIALIZER;
                }
                ResolveFunction(ClassMethod, Declaration);
            }

            foreach (Stmt.Function Method in Statement.Methods)
            {
                FunctionType Declaration = FunctionType.METHOD;
                if (Method.Name.Lexeme.Equals("init"))
                {
                    Declaration = FunctionType.INITIALIZER;
                }
                ResolveFunction(Method, Declaration);
            }

            EndScope();
            if (Statement.SuperClass != null) EndScope(); //<- ...which contains "super"
            CurrentClass = EnclosingClass;

            return null;
        }
        public Object VisitExpressionStmt(Stmt.Expression Statement)
        {
            Resolve(Statement.Exprsn);
            return null;
        }
        public Object VisitFunctionStmt(Stmt.Function Statement)
        {
            Declare(Statement.Name);
            Define(Statement.Name);

            ResolveFunction(Statement, FunctionType.FUNCTION);
            return null;
        }
        public Object VisitIfStmt(Stmt.If Statement)
        {
            Resolve(Statement.Condition);
            Resolve(Statement.ThenBranch);
            if (Statement.ElseBranch != null) Resolve(Statement.ElseBranch);
            return null;
        }
        public Object VisitPrintStmt(Stmt.Print Statement)
        {
            Resolve(Statement.Exprsn);
            return null;
        }
        public Object VisitReturnStmt(Stmt.Return Statement)
        {
            if (CurrentFunction == FunctionType.NONE)
            {
                Lox.Error(Statement.KeyWord, "Can't return from top-level code.");
            }

            if (Statement.Value != null)
            {
                if (CurrentFunction == FunctionType.INITIALIZER)
                {
                    Lox.Error(Statement.KeyWord, "Can't return a value from an initializer.");
                }
                Resolve(Statement.Value);
            }
            return null;
        }
        public Object VisitBreakStmt(Stmt.Break Statement)
        {
            if (CurrentLoop == LoopType.NONE) //*** Chapter 9 - Challenge 3
            {
                Lox.Error(Statement.KeyWord, "Can't break outside a loop.");
            }
            return null;
        }
        public Object VisitWhileStmt(Stmt.While Statement)
        {
            LoopType EnclosingLoop = CurrentLoop;
            CurrentLoop = LoopType.WHILE;

            Resolve(Statement.Condition);
            Resolve(Statement.Body);

            CurrentLoop = EnclosingLoop;
            
            return null;
        }
        public Object VisitVarStmt(Stmt.Var Statement)
        {
            Declare(Statement.Name);
            if (Statement.Initializer != null)
            {
                Resolve(Statement.Initializer);
            }
            Define(Statement.Name);
            return null;
        }
        public Object VisitAssignExpr(Expr.Assign Expression)
        {
            Resolve(Expression.Value);
            ResolveLocal(Expression, Expression.Name);
            return null;
        }
        public Object VisitBinaryExpr(Expr.Binary Expression)
        {
            Resolve(Expression.Left);
            Resolve(Expression.Right);
            return null;
        }
        public Object VisitTernaryExpr(Expr.Ternary Expression)
        {
            Resolve(Expression.Condition);
            Resolve(Expression.Left);
            Resolve(Expression.Right);
            return null;
        }
        public Object VisitCallExpr(Expr.Call Expression)
        {
            Resolve(Expression.Callee);

            foreach (Expr Argument in Expression.Arguments)
            {
                Resolve(Argument);
            }
            return null;
        }
        public Object VisitGetExpr(Expr.Get Expression)
        {
            Resolve(Expression.Object);
            return null;
        } 
        public Object VisitGroupingExpr(Expr.Grouping Expression)
        {
            Resolve(Expression.Expression);
            return null;
        }
        public Object VisitLiteralExpr(Expr.Literal Expression)
        {
            return null;
        }
        public Object VisitLogicalExpr(Expr.Logical Expression)
        {
            Resolve(Expression.Left);
            Resolve(Expression.Right);
            return null;
        }
        public Object VisitSetExpr(Expr.Set Expression)
        {
            Resolve(Expression.Value);
            Resolve(Expression.Object);
            return null;
        }
        public Object VisitSuperExpr(Expr.Super Expression)
        {
            if (CurrentClass == ClassType.NONE)
            {
                Lox.Error(Expression.KeyWord, "Can't use 'super' outside of a class.");
            } else
            {
                if (CurrentClass != ClassType.SUBCLASS)
                {
                    Lox.Error(Expression.KeyWord, "Can't use 'super' in a class with no superclass.");
                }
            }
            ResolveLocal(Expression, Expression.KeyWord);
            return null;
        }
        public Object VisitThisExpr(Expr.This Expression)
        {
            if (CurrentClass == ClassType.NONE)
            {
                Lox.Error(Expression.KeyWord, "Can't use 'this' outside of a class.");
            }
            //*** Chapter 12 - Challenge 1
            if (CurrentFunction == FunctionType.CLASS_METHOD) 
            {
                Lox.Error(Expression.KeyWord, "Can't use 'this' in class methods.");
            }
            ResolveLocal(Expression, Expression.KeyWord);
            return null;
        }
        public Object VisitUnaryExpr(Expr.Unary Expression)
        {
            Resolve(Expression.Right);
            return null;
        }
        public Object VisitVariableExpr(Expr.Variable Expression)
        {
            //C# throws an error if we simply get a non existent value from the dict
            Resolvable Variable;
            if ((Scopes.Count > 0) && (Scopes.Peek().TryGetValue(Expression.Name.Lexeme, out Variable)) && (Variable.State < VarState.DEFINED)) //*** Chapter 11 - Challenge 6
            {
                Lox.Error(Expression.Name, "Can't read local variable in its own initializer.");
            }
            Use(Expression.Name);
            ResolveLocal(Expression, Expression.Name);
            return null;
        }
        public Object VisitLambdaExpr(Expr.Lambda Lambda)
        {
            ResolveLambda(Lambda, FunctionType.LAMBDA);
            return null;
        }
        //*** Helper methods
        private int GetResolvableIndex() //*** Chapter 11 - Challange 4 - not finished; C# doesn't have a mor performant way to access a Dictionary by Index -> major overhaul needed.
        {
            CurrentResolvableIndex++;
            return CurrentResolvableIndex - 1;
        }
        public void Resolve(List<Stmt> Statements)
        {
            foreach (Stmt Statement in Statements)
            {
                Resolve(Statement);
            }
        }
        private void Resolve(Stmt Statement)
        {
            Statement.Accept(this);
        }
        private void Resolve(Expr Expression)
        {
            Expression.Accept(this);
        }
        private void ResolveFunction(Stmt.Function Function, FunctionType Type)
        {
            FunctionType EnclosingFunction = CurrentFunction;
            CurrentFunction = Type;

            BeginScope();
            foreach (Token Param in Function.Params)
            {
                Declare(Param);
                Define(Param);
            }

            Resolve(Function.Body);
            EndScope();

            CurrentFunction = EnclosingFunction;
        }
        private void ResolveLambda(Expr.Lambda Lambda, FunctionType Type)
        {
            FunctionType EnclosingFunction = CurrentFunction;
            CurrentFunction = Type;

            BeginScope();
            foreach (Token Param in Lambda.Params)
            {
                Declare(Param);
                Define(Param);
            }

            Resolve(Lambda.Body);
            EndScope();

            CurrentFunction = EnclosingFunction;
        }
        private void BeginScope()
        {
            CurrentResolvableIndex = 0; //*** Chapter 11 - Challenge 4
            Scopes.Push(new Dictionary<string, Resolvable>());
        }
        private void EndScope()
        {
            Dictionary<string, Resolvable> Scope = Scopes.Pop();
            //*** Chapter 11 - Challenge 3
            foreach (KeyValuePair<string, Resolvable> Variable in Scope)
            {
                if (Variable.Value.State != VarState.USED)
                {
                    Lox.Error(Variable.Value.Name, "Variable is never used.");
                }
            }
        }
        private void Declare(Token Name)
        {
            if (Scopes.Count == 0) return;

            Dictionary<string, Resolvable> Scope = Scopes.Peek();
            if (Scope.ContainsKey(Name.Lexeme))
            {
                Lox.Error(Name, "Already variable with this name in this scope.");
            }
            Resolvable Variable = new Resolvable(Name, VarState.DECLARED, GetResolvableIndex()); //*** Chapter 11 - Challenge 4
            Scope.Add(Name.Lexeme, Variable);
        }
        private void Define(Token Name)
        {
            if (Scopes.Count == 0) return;
            Resolvable Variable = Scopes.Peek()[Name.Lexeme];
            Variable.State = VarState.DEFINED;
            Scopes.Peek()[Name.Lexeme] = Variable;
        }
        private void Use(Token Name)
        {
            //*** Chapter 11 - Challenge 3
            foreach (Dictionary<string, Resolvable> Scope in Scopes)
            {
                if (Scope.ContainsKey(Name.Lexeme))
                {
                    Resolvable Variable = Scope[Name.Lexeme];
                    Variable.State = VarState.USED;
                    Scope[Name.Lexeme] = Variable;
                    return;
                }
            }
            return;
        }
        private void ResolveLocal(Expr Expression, Token Name)
        {
            //*** Looks like in C# "foreach" is iterating backwards through a stack
            int i = Scopes.Count - 1;
            foreach (Dictionary<string, Resolvable> Scope in Scopes)
            {
                if (Scope.ContainsKey(Name.Lexeme)) {
                    Resolvable Variable = Scope[Name.Lexeme];
                    Interpreter.Resolve(Expression, Scopes.Count - 1 - i, Variable.Index);
                    return;
                }
                i--;
            }
        }
    }
}

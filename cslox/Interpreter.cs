using System;
using System.Collections.Generic;
using System.Text;

namespace cslox
{
    //*** Chapter 9 - Challenge 3
    public class Break : Exception
    {        
        public Break() : base() { }
    }
    public class Return : Exception
    {
        public readonly Object Value;

        public Return(Object Value) : base() 
        {
            this.Value = Value;
        }
    }
    public class RuntimeError : Exception
    {
        public readonly Token ErrorToken;
        public RuntimeError() : base() { }
        public RuntimeError(string Message) : base(Message) { }
        public RuntimeError(string Message, Exception Inner) : base(Message, Inner) { }
        public RuntimeError(Token aToken, string Message) : base(Message)
        {
            ErrorToken = aToken;
        }
    }   

    public class Interpreter: Expr.IVisitor<Object>, 
                              Stmt.IVisitor<Object>
    {
        public readonly Environment Globals = new Environment();
        private Environment Environment;
        private readonly Dictionary<Expr, Local> Locals = new Dictionary<Expr, Local>();

        private struct Local
        {
            public Local(int Distance, int Index)
            {
                this.Distance = Distance;
                this.Index = Index;
            }

            public int Distance;
            public int Index; //Chapter 11 - Challenge 4 (not finished/used)
        }
        public Interpreter()
        {
            //*** Define native function "clock()"
            Globals.Define("clock", new NativeClock());
            //*** Chapter 13.4 - own extensions
            Globals.Define("input", new NativeInput());

            Globals.Define("file_exists", new NativeFileExists());
            Globals.Define("file_readtext", new NativeFileReadText());
            Globals.Define("file_writetext", new NativeFileWriteText());
            Globals.Define("file_open", new NativeFileOpen());
            Globals.Define("file_close", new NativeFileClose());
            Globals.Define("file_write", new NativeFileWrite());
            Globals.Define("file_writeline", new NativeFileWriteLine());
            Globals.Define("file_read", new NativeFileRead());
            Globals.Define("file_readline", new NativeFileReadLine());
            Globals.Define("seek_bof", new NativeSeekBOF());
            Globals.Define("seek_eof", new NativeSeekEOF());
            

            Environment = Globals;
        }

        public void Interpret(List<Stmt> Statements)
        {
            try
            {
                foreach (Stmt Statement in Statements)
                {
                    Execute(Statement);
                }                
            } catch (RuntimeError Error)
            {
                Lox.RuntimeError(Error);
            }
        }
        public Object Evaluate(Expr Expression)
        {
            return Expression.Accept(this);
        }
        public void Execute(Stmt Statement)
        {
            Statement.Accept(this);
        }
        public void Resolve(Expr Expression, int Depth, int Index)
        {
            Local Local = new Local(Depth, Index);
            Locals[Expression] = Local;
        }
        public void ExecuteBlock(List<Stmt> Statements, Environment Environment)
        {
            Environment Previous = this.Environment;
            try
            {
                this.Environment = Environment;

                foreach (Stmt Statement in Statements)
                {
                    Execute(Statement);
                }
            } finally
            {
                this.Environment = Previous;
            }
        }
        
        //*** Methods for the visitor pattern (Stmt.IVisitor)
        public Object VisitBlockStmt(Stmt.Block Statement)
        {
            ExecuteBlock(Statement.Statements, new Environment(this.Environment));
            return null;
        }
        public Object VisitClassStmt(Stmt.Class Statement)
        {
            Object SuperClass = null;
            if (Statement.SuperClass != null)
            {
                SuperClass = Evaluate(Statement.SuperClass);
                if (!(SuperClass is LoxClass))
                {
                    throw new RuntimeError(Statement.SuperClass.Name, "Superclass must be a class.");
                }
            }

            Environment.Define(Statement.Name.Lexeme, null);

            if (Statement.SuperClass != null)
            {
                Environment = new Environment(Environment);
                Environment.Define("super", SuperClass);
            }

            Dictionary<String, LoxFunction> ClassMethods = new Dictionary<String, LoxFunction>();
            Dictionary<String, LoxFunction> Methods = new Dictionary<String, LoxFunction>();

            //*** Chapter 12 - Challenge 1
            foreach (Stmt.Function ClassMethod in Statement.ClassMethods)
            {
                LoxFunction Function = new LoxFunction(ClassMethod, Environment, false);
                ClassMethods[ClassMethod.Name.Lexeme] = Function;
            }

            foreach (Stmt.Function Method in Statement.Methods)
            {
                LoxFunction Function = new LoxFunction(Method, Environment, Method.Name.Lexeme.Equals("init"));
                Methods[Method.Name.Lexeme] = Function;
            }

            LoxClass Class = new LoxClass(Statement.Name.Lexeme, (LoxClass)SuperClass, ClassMethods, Methods);

            if (SuperClass != null)
            {
                Environment = Environment.Enclosing;
            }

            Environment.Assign(Statement.Name, Class);
            return null;
        }
        public Object VisitExpressionStmt(Stmt.Expression Statement)
        {
            Evaluate(Statement.Exprsn);
            return null;
        }
        public Object VisitFunctionStmt(Stmt.Function Statement)
        {
            LoxFunction Function = new LoxFunction(Statement, Environment, false);
            Environment.Define(Statement.Name.Lexeme, Function); ///Chapter 11 - Challenge 4
            return null;
        }
        public Object VisitIfStmt(Stmt.If Stmt)
        {
            if (isTruthy(Evaluate(Stmt.Condition))) {
                Execute(Stmt.ThenBranch);
            } else if (Stmt.ElseBranch != null)
            {
                Execute(Stmt.ElseBranch);
            }
            return null;
        }
        public Object VisitPrintStmt(Stmt.Print Statement)
        {
            Object Value = Evaluate(Statement.Exprsn);
            Console.WriteLine(Stringify(Value));
            return null;
        }
        public Object VisitReturnStmt(Stmt.Return Statement)
        {
            Object Value = null;
            if (Statement.Value != null)
            {
                Value = Evaluate(Statement.Value);
            }

            throw new Return(Value);
        }
        public Object VisitBreakStmt(Stmt.Break Statement)
        {
            throw new Break(); //*** Chapter 9 - Challenge 3
        }
        public Object VisitVarStmt(Stmt.Var Statement)
        {
            Object Value = null;

            if (Statement.Initializer != null)
            {
                Value = Evaluate(Statement.Initializer);
            }

            Environment.Define(Statement.Name.Lexeme, Value);
            return null;
        }
        public Object VisitWhileStmt(Stmt.While Statement)
        {
            try
            {
                while (isTruthy(Evaluate(Statement.Condition)))
                {
                    Execute(Statement.Body);
                }
            }
            catch (Break) { }; //*** Chapter 9 - Challenge 3

            return null;
        }

        //*** Methods for the visitor pattern (Expr.IVisitor)
        public Object VisitAssignExpr(Expr.Assign Expression)
        {
            Object Value = Evaluate(Expression.Value);

            Local Local;
            if (Locals.TryGetValue(Expression, out Local))
            {
                Environment.AssignAt(Local.Distance, Expression.Name, Value);
            }
            else
            {
                Globals.Assign(Expression.Name, Value);
            }
            return Value;
        }
        public Object VisitLiteralExpr(Expr.Literal Expression)
        {
            return Expression.Value;
        }
        public Object VisitLogicalExpr(Expr.Logical Expression)
        {
            Object Left = Evaluate(Expression.Left);

            if (Expression.Operator.Type == TokenType.OR)
            {
                if (isTruthy(Left)) return Left;
            } else
            {
                if (!isTruthy(Left)) return Left;
            }

            return Evaluate(Expression.Right);
        }
        public Object VisitSetExpr(Expr.Set Expression)
        {
            Object Obj = Evaluate(Expression.Object);

            if (!(Obj is LoxInstance))
            {
                throw new RuntimeError(Expression.Name, "Only insances have fields.");
            }

            Object Value = Evaluate(Expression.Value);
            ((LoxInstance)Obj).Set(Expression.Name, Value);
            return Value;
        }
        public Object VisitSuperExpr(Expr.Super Expression)
        {
            int Distance = Locals[Expression].Distance;

            LoxClass SuperClass = (LoxClass)Environment.GetAt(Distance, "super");

            LoxInstance Object = (LoxInstance)Environment.GetAt(Distance - 1, "this");

            LoxFunction Method = SuperClass.FindMethod(Expression.Method.Lexeme);

            if (Method == null)
            {
                throw new RuntimeError(Expression.Method, "Undefined property '" + Expression.Method.Lexeme + "'.");
            }

            return Method.Bind(Object);
        }
        public Object VisitThisExpr(Expr.This Expression)
        {
            return LookupVariable(Expression.KeyWord, Expression);
        }
        public Object VisitGroupingExpr(Expr.Grouping Expression)
        {
            return Evaluate(Expression.Expression);
        }
        public Object VisitUnaryExpr(Expr.Unary Expression)
        {
            Object Right = Evaluate(Expression.Right);

            switch (Expression.Operator.Type)
            {
                case TokenType.BANG:
                    return !isTruthy(Right);
                case TokenType.MINUS:
                    CheckNumberOperand(Expression.Operator, Right);
                    return -(double)Right;
            }

            //*** Unreachable
            return null;
        }
        public Object VisitVariableExpr(Expr.Variable Expression)
        {
            //*** Chapter 8 - Challenge 2 - runtime error for uninitialized variables
            //Object Result = Environment.Get(Expression.Name);
            Object Result = LookupVariable(Expression.Name, Expression);
            if (Result == null) throw new RuntimeError(Expression.Name, "Variable '" + Expression.Name.Lexeme + "' not initialized.");
            return Result;
        }
        public Object LookupVariable(Token Name, Expr Expression)
        {
            Local Local;
            if (Locals.TryGetValue(Expression, out Local)) {
                return Environment.GetAt(Local.Distance, Name.Lexeme);
            } else
            {
                return Globals.Get(Name);
            }
        }
        public Object VisitBinaryExpr(Expr.Binary Expression)
        {
            Object Left = Evaluate(Expression.Left);
            Object Right = Evaluate(Expression.Right);

            switch (Expression.Operator.Type)
            {
                case TokenType.GREATER:
                    CheckNumberOperands(Expression.Operator, Left, Right);
                    return (double)Left > (double)Right;
                case TokenType.GREATER_EQUAL:
                    CheckNumberOperands(Expression.Operator, Left, Right);
                    return (double)Left >= (double)Right;
                case TokenType.LESS:
                    CheckNumberOperands(Expression.Operator, Left, Right);
                    return (double)Left < (double)Right;
                case TokenType.LESS_EQUAL:
                    CheckNumberOperands(Expression.Operator, Left, Right);
                    return (double)Left <= (double)Right;
                case TokenType.MINUS:
                    CheckNumberOperands(Expression.Operator, Left, Right);
                    return (double)Left - (double)Right;
                case TokenType.BANG_EQUAL:
                    return !isEqual(Left, Right);
                case TokenType.EQUAL_EQUAL:
                    return isEqual(Left, Right);
                case TokenType.PLUS:
                    if ((Left is double) && (Right is double))
                    {
                        return (double)Left + (double)Right;
                    }

                    if ((Left is string) && (Right is string))
                    {
                        return (string)Left + (string)Right;
                    }

                    //*** Chapter 7 - Challenge 2 - '+' operator for mixed types
                    if ((Left is string) && (Right is double))
                    {
                        return (string)Left + Stringify((double)Right);
                    }

                    if ((Left is double) && (Right is string))
                    {
                        return Stringify((double)Left) + (string)Right;
                    }


                    throw new RuntimeError(Expression.Operator, "Operands must be numbers or strings");
                case TokenType.SLASH:
                    CheckNumberOperands(Expression.Operator, Left, Right);

                    //*** Chapter 7 - Challenge 2 - '+' operator for mixed types
                    if ((double)Right == 0) throw new RuntimeError(Expression.Operator, "Division by zero");

                    return (double)Left / (double)Right;
                case TokenType.STAR:
                    CheckNumberOperands(Expression.Operator, Left, Right);
                    return (double)Left * (double)Right;
                case TokenType.COMMA:
                    //*** Challenge chapter 6
                    return Right;
            }

            //*** Unreachable
            return null;
        }
        public Object VisitTernaryExpr(Expr.Ternary Expression)
        {
            if (isTruthy(Evaluate(Expression.Condition))) {
                return Evaluate(Expression.Left);
            } else
            {
                return Evaluate(Expression.Right);
            }
        }
        public Object VisitCallExpr(Expr.Call Expression)
        {            
            Object Callee = Evaluate(Expression.Callee);            

            List<Object> Arguments = new List<Object>();
            foreach (Expr Argument in Expression.Arguments) 
            {
                Arguments.Add(Evaluate(Argument));
            }

            if (!(Callee is ICallable))
            {
                throw new RuntimeError(Expression.Parenthesis, "Can only call functions and classes.");
            }

            ICallable Function = (ICallable)Callee;
            if (Arguments.Count != Function.Arity())
            {
                throw new RuntimeError(Expression.Parenthesis, "Expected " + Function.Arity().ToString() + " arguments but got " + Arguments.Count.ToString() + ".");
            }

            return Function.Call(this, Arguments);
        }
        public Object VisitGetExpr(Expr.Get Expression)
        {
            Object Obj = Evaluate(Expression.Object);
            if (Obj is LoxInstance)
            {
                return ((LoxInstance)Obj).Get(Expression.Name);
            }

            throw new RuntimeError(Expression.Name, "Only instances have properties.");
        }
        public Object VisitLambdaExpr(Expr.Lambda Expression)
        {
            //*** Chapter 10 - Challenge 2
            LoxLambda Lambda = new LoxLambda(Expression, Environment);
            return Lambda;
        }
        //*** Helper Methods
        private Boolean isTruthy(Object aObject)
        {
            //*** false and nil are false, everything else ist true
            if (aObject == null) return false;
            if (aObject is Boolean) return (Boolean)aObject;
            return true;
        }

        private Boolean isEqual(Object a, Object b)
        {
            if ((a == null) && (b == null)) return true;
            if (a == null) return false;

            return a.Equals(b);
        }
        private void CheckNumberOperand(Token Operator, Object Operand)
        {
            if (Operand is double) return;
            throw new RuntimeError(Operator, "Operand must be a number.");
        }
        private void CheckNumberOperands(Token Operator, Object Left, Object Right)
        {
            if ((Left is double) && (Right is double)) return;
            throw new RuntimeError(Operator, "Operand must be a number.");
        }
        private void CheckTernaryOperands(Token Operator, Object Left, Object Right)
        {
            if (Left is Boolean) return;
            throw new RuntimeError(Operator, "First operand must be boolean.");
        }
        private string Stringify(Object aObject) 
        {
            if (aObject == null) return "nil";
            if (aObject is double)
            {
                string Text = aObject.ToString().Replace(',', '.');
                return Text;
            }
            return aObject.ToString();
        }
    }
}

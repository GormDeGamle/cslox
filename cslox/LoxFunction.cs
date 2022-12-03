using System;
using System.Collections.Generic;
using System.Text;

namespace cslox
{
    class LoxFunction: ICallable
    {
        private readonly Stmt.Function Declaration;
        private readonly Environment Closure; //*** Environment at the moment of the functions declaration
        private readonly Boolean isInitializer;
        public LoxFunction(Stmt.Function Declaration, Environment Closure, Boolean isInitializer)
        {
            this.isInitializer = isInitializer;
            this.Closure = Closure;
            this.Declaration = Declaration;
        }
        public LoxFunction Bind(LoxInstance Instance)
        {
            //*** binds "this" to a new environment
            Environment Environment = new Environment(Closure);
            Environment.Define("this", Instance);
            return new LoxFunction(Declaration, Environment, isInitializer);
        }
        public Object Call(Interpreter Interpreter, List<Object> Arguments)
        {
            //*** Environment for the functio body with all the parameters in it.
            Environment Environment = new Environment(Closure);
            for (int i = 0; i < Declaration.Params.Count; i++)
            {
                Environment.Define(Declaration.Params[i].Lexeme, Arguments[i]);
            }

            //*** Execute function body, catch the return-exception and return it's value
            try
            {
                Interpreter.ExecuteBlock(Declaration.Body, Environment);
            } catch (Return ReturnValue)
            {
                if (isInitializer)
                {
                    return Closure.GetAt(0, "this");
                } else
                {
                    return ReturnValue.Value;
                }
            }

            //*** Class initializers should return "this".
            if (isInitializer)
            {
                return Closure.GetAt(0, "this");
            }

            return null;
        }
        public int Arity()
        {
            return Declaration.Params.Count;
        }
        public override string ToString()
        {
            return "<fn " + Declaration.Name.Lexeme + ">";
        }
        public Token Name
        {
            get { return Declaration.Name; }
        }
    }

    class LoxLambda : ICallable
    {
        private readonly Expr.Lambda Declaration;
        private readonly Environment Closure; //*** Environment at the moment of the functions declaration
        public LoxLambda(Expr.Lambda Declaration, Environment Closure)
        {
            this.Declaration = Declaration;
            this.Closure = Closure;
        }
        public Object Call(Interpreter Interpreter, List<Object> Arguments)
        {
            //*** Environment for the functio body with all the parameters in it.
            Environment Environment = new Environment(Closure);
            for (int i = 0; i < Declaration.Params.Count; i++)
            {
                Environment.Define(Declaration.Params[i].Lexeme, Arguments[i]);
            }

            //*** Execute function body, catch the return-exception and return it's value
            try
            {
                Interpreter.ExecuteBlock(Declaration.Body, Environment);
            }
            catch (Return ReturnValue)
            {
                return ReturnValue.Value;
            }
            return null;
        }
        public int Arity()
        {
            return Declaration.Params.Count;
        }
        public override string ToString()
        {
            return "<fn>";
        }
    }
}

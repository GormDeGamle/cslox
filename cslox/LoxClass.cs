using System;
using System.Collections.Generic;
using System.Text;

namespace cslox
{
    class LoxClass: LoxInstance, ICallable
    {
        public readonly string Name;
        public readonly LoxClass SuperClass;
        private readonly Dictionary<String, LoxFunction> Methods;

        public LoxClass(string Name, LoxClass SuperClass, Dictionary<String, LoxFunction> ClassMethods, Dictionary<String, LoxFunction> Methods) : base(null)
        {
            this.Name = Name;
            this.SuperClass = SuperClass;
            this.Methods = Methods;
            //*** Chapter 12 - Challenge 1
            foreach (var ClassMethod in ClassMethods)
            {
                Methods.Add(ClassMethod.Key, ClassMethod.Value);
                Set(ClassMethod.Value.Name, ClassMethod.Value);
            }
        }
        public LoxFunction FindMethod(String Name)
        {
            if (Methods.ContainsKey(Name)) {
                return Methods[Name];
            }

            if (SuperClass != null)
            {
                return SuperClass.FindMethod(Name);
            }

            return null;
        }
        public Object Call(Interpreter Interpreter, List<Object> Arguments)
        {
            LoxInstance Instance = new LoxInstance(this);
            LoxFunction Initializer = FindMethod("init");
            if (Initializer != null)
            {
                Initializer.Bind(Instance).Call(Interpreter, Arguments);
            }
            return Instance;
        }
        public int Arity()
        {
            LoxFunction Initializer = FindMethod("init");
            if (Initializer == null)
            {
                return 0;
            } else
            {
                return Initializer.Arity();
            }
        }
        public override string ToString()
        {
            return Name;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Text;

namespace cslox
{
    class LoxInstance
    {
        private LoxClass Class;
        private readonly Dictionary<String, Object> Fields = new Dictionary<String, Object>();

        public LoxInstance(LoxClass Class)
        {
            this.Class = Class;
        }
        public Object Get(Token Name)
        {
            if (Name.Lexeme == "init" && Class == null)
            {
                throw new RuntimeError(Name, "'init' not allowed on classes.");
            }

            if (Fields.ContainsKey(Name.Lexeme))
            {
                return Fields[Name.Lexeme];
            }

            if (Class != null)
            {
                LoxFunction Method = Class.FindMethod(Name.Lexeme);
                if (Method != null) return Method.Bind(this);
            }

            throw new RuntimeError(Name, "Undefined property '" + Name.Lexeme + "'.");
        }
        public void Set(Token Name, Object Value)
        {
            Fields[Name.Lexeme] = Value;            
        }
        public override string ToString()
        {
            return Class.Name + " instance";
        }

    }
}

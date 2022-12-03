using System;
using System.Collections.Generic;
using System.Text;

namespace cslox
{
    public class Environment
    {
        public readonly Environment Enclosing;
        //*** Chapter 11 - Challenge 4 - C# Dictionaries cannote be accessed by index - challenge 4 would need major overhaul - stopped! (Maybe later...)
        private readonly Dictionary<string, Object> Values = new Dictionary<string, object>(); 
        //private readonly List<Object> LocalValues = new List<Object>();
        private readonly string DebugName;
        private readonly Boolean Debug = false;

        public Environment ()
        {
            this.Enclosing = null;
            this.DebugName = DateTime.Now.ToString("yyyyMMddHHmmssffff");
        }
        public Environment (Environment Enclosing)
        {
            this.Enclosing = Enclosing;
            this.DebugName = DateTime.Now.ToString("yyyyMMddHHmmssffff");
        }
        public string ValuesAsString()
        {
            /* DEBUG - printout of the environment */
            string Result = "";
            foreach (string aKey in Values.Keys)
            {
                if (Result != "") Result = Result + "; ";
                Result = Result + aKey + "=";
                if (Values[aKey] != null)
                {
                    Result = Result + Values[aKey].ToString();
                } else
                {
                    Result = Result + "nil";
                }
            }
            return Result;
        }
        /*
        public string LocalValuesAsString()
        {
            //*** DEBUG - printout of the environment 
            string Result = "";

            foreach (string Value in LocalValues)
            {
                if (Result != "") Result = Result + ", ";
                if (Value != null)
                {
                    Result = Result + Value.ToString();
                }
                else
                {
                    Result = Result + "nil";
                }
            }

            return Result;
        }
        */

        public void Define(string Name, Object Value)
        {
            Values[Name] = Value;
            if (Debug) Console.WriteLine(DebugName + ": " + ValuesAsString());
        }
        Environment Ancestor(int Distance)
        {
            Environment CurrentEnvironment = this;
            for (int i = 0; i < Distance; i++)
            {
                CurrentEnvironment = CurrentEnvironment.Enclosing;
            }
            return CurrentEnvironment;
        }
        public Object GetAt(int Distance, string Name)
        {
            return Ancestor(Distance).Values[Name];
        }
        public Object Get(Token Name)
        {
            if (Values.ContainsKey(Name.Lexeme)) {
                if (Debug) Console.WriteLine(DebugName + ": '" + Name.Lexeme + "' found. Value=" + Values[Name.Lexeme]);
                return Values[Name.Lexeme];
            }

            /* Look it up in the eclosing environment */
            if (Enclosing != null)
            {
                if (Debug) Console.WriteLine(DebugName + ": '" + Name.Lexeme + "' not found found. Lookup in enclosing environment.");
                return Enclosing.Get(Name);
            }

            throw new RuntimeError(Name, "Can't evaluate undefined variable '" + Name.Lexeme + "'.");
        }
        public void AssignAt(int Distance, Token Name, Object Value) 
        {
            Ancestor(Distance).Values[Name.Lexeme] = Value;
        }
        public void Assign(Token Name, Object Value)
        {
            if (Values.ContainsKey(Name.Lexeme)) {
                Values[Name.Lexeme] = Value;
                if (Debug) Console.WriteLine(DebugName + ": " + ValuesAsString());
                return;
            }

            /* try to assign it in the enclosing environment */
            if (Enclosing != null)
            {
                Enclosing.Assign(Name, Value);
                return;
            }

            throw new RuntimeError(Name, "Can't assign to undefined Variable '" + Name.Lexeme + "'.");
        }
    }
}

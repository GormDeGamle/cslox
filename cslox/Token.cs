using System;
using System.Collections.Generic;
using System.Text;

namespace cslox
{
    public class Token
    {
        public readonly TokenType Type;
        public readonly string Lexeme;
        public readonly Object Literal;
        public readonly int Line;

        public Token(TokenType Type, string Lexeme, Object Literal, int Line)
        {
            this.Type = Type;
            this.Lexeme = Lexeme;
            this.Literal = Literal;
            this.Line = Line;
        }
        public override string ToString()
        {
            return Type + " " + Lexeme + " " + Literal;
        }
    }
}

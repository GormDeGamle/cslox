using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

/* ------------------------------------------------------------------- *
 * Scanner class converting a string of Lox code into a list of tokens *
 * ------------------------------------------------------------------- */

namespace cslox
{
    class Scanner
    {
        private readonly string Source;
        private readonly List<Token> Tokens = new List<Token>();

        private int Start = 0;
        private int Current = 0;
        private int Line = 0;

        private static Dictionary<string, TokenType> KeyWords = new Dictionary<string, TokenType>();

        public Scanner(string Source)
        {
            this.Source = Source;

            //*** Initialize KeyWords (only then first time - it's static)
            if (KeyWords.Count == 0)
            {
                KeyWords.Add("and", TokenType.AND);
                KeyWords.Add("class", TokenType.CLASS);
                KeyWords.Add("else", TokenType.ELSE);
                KeyWords.Add("false", TokenType.FALSE);
                KeyWords.Add("for", TokenType.FOR);
                KeyWords.Add("fun", TokenType.FUN);
                KeyWords.Add("if", TokenType.IF);
                KeyWords.Add("nil", TokenType.NIL);
                KeyWords.Add("or", TokenType.OR);
                KeyWords.Add("print", TokenType.PRINT);
                KeyWords.Add("break", TokenType.BREAK); //Challenge chapter 6
                KeyWords.Add("return", TokenType.RETURN);
                KeyWords.Add("super", TokenType.SUPER);
                KeyWords.Add("this", TokenType.THIS);
                KeyWords.Add("true", TokenType.TRUE);
                KeyWords.Add("var", TokenType.VAR);
                KeyWords.Add("while", TokenType.WHILE);
            }
        }

        //****************
        //*** Scanning ***
        //****************
        public List<Token> ScanTokens()
        {
            while (!isAtEnd())
            {
                //*** We are at the beginning of the next lexeme
                Start = Current;
                ScanToken();
            }
            Tokens.Add(new Token(TokenType.EOF, "", null, Line));
            return Tokens;
        }
        private void ScanToken()
        {
            char c = Advance();

            switch (c)
            {
                case '(': 
                    AddToken(TokenType.LEFT_PAREN); 
                    break;
                case ')': 
                    AddToken(TokenType.RIGHT_PAREN); 
                    break;
                case '{': 
                    AddToken(TokenType.LEFT_BRACE); 
                    break;
                case '}': 
                    AddToken(TokenType.RIGHT_BRACE); 
                    break;
                case ',': 
                    AddToken(TokenType.COMMA); 
                    break;
                case '.': 
                    AddToken(TokenType.DOT); 
                    break;
                case '-': 
                    AddToken(TokenType.MINUS); 
                    break;
                case '+': 
                    AddToken(TokenType.PLUS); 
                    break;
                case ';': 
                    AddToken(TokenType.SEMICOLON); 
                    break;
                case '*': 
                    AddToken(TokenType.STAR); 
                    break;
                case '?':
                    //*** Challenge chapter 6
                    AddToken(TokenType.QUESTION);
                    break;
                case ':':
                    //*** Challenge chapter 6
                    AddToken(TokenType.COLON);
                    break;
                case '!':
                    AddToken(Match('=') ? TokenType.BANG_EQUAL : TokenType.BANG);
                    break;
                case '=':
                    AddToken(Match('=') ? TokenType.EQUAL_EQUAL : TokenType.EQUAL);
                    break;
                case '<':
                    AddToken(Match('=') ? TokenType.LESS_EQUAL : TokenType.LESS);
                    break;
                case '>':
                    AddToken(Match('=') ? TokenType.GREATER_EQUAL : TokenType.GREATER);
                    break;
                case '/':
                    if (Match('/'))
                    {
                        //*** A comment goes until the end of the line.
                        while (Peek() != '\n' && !isAtEnd()) Advance();
                    }
                    else if (Match('*'))
                    {
                        Comment();
                    }
                    else 
                    {
                        AddToken(TokenType.SLASH);
                    }
                    break;
                case ' ':
                case '\r':
                case '\t':
                    //*** Ignore whitespace
                    break;
                case '\n':
                    Line++;
                    break;
                case '"': 
                    String(); 
                    break;
                default:
                    //*** Check for number literal
                    if (isDigit(c))
                    {
                        Number();
                    }
                    //*** Check for identifiers and reserved words
                    else if (isAlpha(c))
                    {
                        Identifier();
                    }
                    //*** Some unexpected character occured
                    else if (c.ToString().Trim() == "")
                    {
                        Lox.Error(Line, "Unexpected character.");
                    }
                    else
                    {
                        Lox.Error(Line, "Unexpected character (" + c.ToString().Trim() + ").");
                    }
                    break;
            }
        }
        //*** Handle identifiers and tokens
        private void Identifier()
        {
            while (isAlphaNumeric(Peek())) Advance(); 

            string Text = Source.Substring(Start, Current - Start);
            if (!KeyWords.TryGetValue(Text, out TokenType Type))
            {
                Type = TokenType.IDENTIFIER;
            }

            AddToken(Type);
        }
        //*** Handle numbers
        private void Number()
        {            
            while (isDigit(Peek())) Advance(); 

            //*** Look for a fractional part
            if (Peek() == '.' && isDigit(PeekNext()))
            {
                //*** Step over the .
                Advance();
                //*** Consume the fraction
                while (isDigit(Peek()))  Advance(); 
            }

            AddToken(TokenType.NUMBER, 
                     Double.Parse(Source.Substring(Start, Current - Start), CultureInfo.InvariantCulture)); //*** uese . as a decimal separator
        }
        //*** Handle strings
        private void String()
        {
            while (Peek() != '"'  &&  !isAtEnd())
            {
                if (Peek() == '\n') { Line++; }
                Advance();
            }

            if (isAtEnd())
            {
                Lox.Error(Line, "Unterminated string.");
                return;
            }

            //*** Step over the closing "
            Advance();

            //*** Trim the surrounding quotes
            string Value = Source.Substring(Start + 1, (Current - Start) - 2);

            AddToken(TokenType.STRING, Value);
        }
        //*** Handle block comments
        private void Comment()
        {
            while (Peek() != '*' && PeekNext() != '/' && !isAtEnd())
            {
                if (Peek() == '\n')  Line++; 
                Advance();
            }

            if (isAtEnd())
            {
                Lox.Error(Line, "Unterminated comment.");
                return;
            }

            //*** Step over the closing */
            Advance(); Advance();
        }
        //*** Check current character
        private Boolean Match(char Expected)
        {
            if (isAtEnd()) return false; 
            if (Source[Current] != Expected) return false; 

            Current++;
            return true;
        }
        //*** Look ahead
        private char Peek()
        {
            if (isAtEnd()) return '\0'; 
            return Source[Current];
        }
        private char PeekNext()
        {
            if (Current + 1 >= Source.Length) return '\0'; 
            return Source[Current + 1];
        }
        //*** CHeck for alphabetical character
        private Boolean isAlpha(char c)
        {
            return (c >= 'a' && c <= 'z') ||
                   (c >= 'A' && c <= 'Z') ||
                   (c == '_');
        }
        //*** Check for numeric character
        private Boolean isDigit(char c)
        {
            return (c >= '0' && c <= '9');
        }
        //*** Check for alphanumeric character
        private Boolean isAlphaNumeric(char c)
        {
            return (isAlpha(c) || isDigit(c));
        }
        //*** This is the end...?        
        private Boolean isAtEnd()
        {
            return Current >= Source.Length;
        }
        //*** Step ahead
        private char Advance()
        {
            Current++;
            return Source[Current - 1];
        }
        //*** Save Token
        private void AddToken(TokenType Type) 
        {
            AddToken(Type, null);
        }
        private void AddToken(TokenType Type, Object Literal )
        {
            string Text = Source.Substring(Start, Current - Start);
            Tokens.Add(new Token(Type, Text, Literal, Line));
        }
    }
}

using System;
using System.Collections.Generic;
using System.Text;

/* ------------------------------------------------------------------- *
  Parser class implementing the following grammar 
   
  program        → declaration* EOF ;

  declaration    → classDecl | funDecl | varDecl | statement ;

  classDecl      → "class" IDENTIFIER ( "<" IDENTIFIER )? "{" ( class? function )* "}" ;    (Chapter 12 - Challente 1 - class methods)
  funDecl        → "fun" function | lambda ;                                                (Chapter 10 - Challenge 2 - lambda)
  function       → IDENTIFIER "(" parameters? ")" block ; 
  lambda         → "(" parameters? ")" block ;                                              (Chapter 10 - Challenge 2)  
  parameters     → IDENTIFIER ( "," IDENTIFIER )* ;

  varDecl        → "var" IDENTIFIER ( "=" expression )? ";" ;

  statement      → exprStmt | forStmt | ifStmt | printStmt | returnStmt | breakStmt | whileStmt | block ;    (breakStmt - Chapter 9 - Challenge 3)
  exprStmt       → expression ";" ;
  forStmt        → "for" "(" ( varDecl | exprStmt | ";" ) expression? ";" expression? ")" statement ;
  ifStmt         → "if" "(" expression ")" statement ( "else" statement )? ;
  printStmt      → "print" expression ";" ;
  returnStmt     → "return" expression? ";" ;
  breaktStmt     → "break" ";" ;  
  whileStmt      → "while" "(" expression ")" statement ;
  block          → "{" declaration* "}" ;

  expression     → assignment ;
  assignment     → ( call "." )? IDENTIFIER "=" assignment | logic_or ;  
  logic_or       → logic_and ( "or" logic_and )* ;
//logic_and      → equality ( "and" equality )* ; (original)
//logic_and      → expressionlist ( "and" expressionlist )* ; 
  logic_and      → ternary ( "and" ternary )* ; 

//expressionlist → ternary ("," ternary)* ;                                                 (Chapter 6 - Challenge 1 - removed because it messes with parameter lists in function calls)
  ternary        → equality ("?" expression ":" expression) ;                               (Chapter 6 - Challenge 2)

  equality       → comparison ( ( "!=" | "==" ) comparison )* ;
  comparison     → term ( ( ">" | ">=" | "<" | "<=" ) term )* ;
  term           → factor ( ( "-" | "+" ) factor )* ;
  factor         → unary ( ( "/" | "*" ) unary )* ;
  unary          → ( "!" | "-" ) unary | call ;
  call           → lambdaCall | funCall ; 
  lambdaCall     → "fun" ( "(" arguments? ")" )* ;                                          (Chapter 10 - Challenge 2)
  funCall        → primary ( "(" arguments? ")" | "." IDENTIFIER )* ;
  arguments      → expression ( "," expression )* ;
  primary        → "true" | "false" | "nil" | "this" | NUMBER | STRING | IDENTIFIER | "super" "." IDENTIFIER | "(" expression ")" ;
 * ------------------------------------------------------------------- */

namespace cslox
{
    public class ParseError : Exception
    {
        public ParseError() : base() { }
        public ParseError(string Message) : base(Message) { }
        public ParseError(string Message, Exception Inner) : base(Message, Inner) { }
    }
    public class Parser
    {
        private readonly List<Token> Tokens;
        private int Current = 0;
        //private int LoopLevel = 0; //Chapter 9 - Challenge 3; - it's now done in the Resolver

        public Parser(List<Token> Tokens)
        {
            this.Tokens = Tokens;
        }
        public List<Stmt> Parse()
        {
            List<Stmt> Statements = new List<Stmt>();
            while (!IsAtEnd()) {
                Statements.Add(Declaration());
            }

            return Statements;
        }

        private Stmt Declaration()
        {
            //*** declaration → funDecl | varDecl | statement ;

            try
            {
                if (Match(TokenType.CLASS)) { return ClassDeclaration(); }
                if (Match(TokenType.FUN)) { if (Check(TokenType.LEFT_PAREN)) { Recede(); } else { return Function("function"); } } //*** Chapter 10 - Challenge 2 - Lambdas - If a Lambda is detected step back again and parse it later...
                if (Match(TokenType.VAR)) { return VarDeclaration(); }

                return Statement();

            } 
            catch (ParseError)
            {
                Synchronize();
                return null;
            }
        }
        private Stmt ClassDeclaration()
        {
            Token Name = Consume(TokenType.IDENTIFIER, "Expect class name.");
            Expr.Variable SuperClass = null;
            if (Match(TokenType.LESS)) 
            {
                Consume(TokenType.IDENTIFIER, "Expect superclass name");
                SuperClass = new Expr.Variable(Previous());
            }

            Consume(TokenType.LEFT_BRACE, "Expect '{' before body.");

            List<Stmt.Function> ClassMethods = new List<Stmt.Function>(); //*** Chapter 12 - Challenge 1
            List<Stmt.Function> Methods = new List<Stmt.Function>();

            while (!Check(TokenType.RIGHT_BRACE) && !IsAtEnd())
            {
                //*** Chapter 12 - Challenge 1
                if (Match(TokenType.CLASS))
                {
                    ClassMethods.Add(Function("class method"));
                } else
                {
                    Methods.Add(Function("method"));
                }
            }

            Consume(TokenType.RIGHT_BRACE, "Expect '}' after class body.");

            return new Stmt.Class(Name, SuperClass, ClassMethods, Methods);
        }
        private Stmt VarDeclaration()
        {
            //*** varDecl → "var" IDENTIFIER ( "=" expression )? ";" ;

            Token Name = Consume(TokenType.IDENTIFIER, "Expect variable name.");

            Expr Initializer = null;
            if (Match(TokenType.EQUAL))
            {
                Initializer = Expression();
            }

            Consume(TokenType.SEMICOLON, "Expect ';' after variable declaration.");
            return new Stmt.Var(Name, Initializer);
        }
        private Stmt Statement()
        {
            //*** statement → exprStmt | forStmt | ifStmt | printStmt | returnStmt | breakStmt | whileStmt | block ;

            if (Match(TokenType.FOR)) return ForStatement();
            if (Match(TokenType.IF)) return IfStatement();
            if (Match(TokenType.PRINT)) return PrintStatement();
            if (Match(TokenType.RETURN)) return ReturnStatement();
            if (Match(TokenType.BREAK)) return BreakStatement();
            if (Match(TokenType.WHILE)) return WhileStatement();
            if (Match(TokenType.LEFT_BRACE)) return new Stmt.Block(Block());

            return ExpressionStatement();
        }
        private Stmt ForStatement()
        {
            //*** forStmt → "for" "(" ( varDecl | exprStmt | ";" ) expression? ";" expression? ")" statement ;

            //LoopLevel++; //*** Chapter 9 - Challenge 3 - it's now done in the resolver
            try
            {
                Consume(TokenType.LEFT_PAREN, "Expect '(' after for.");

                /* Initializer */
                Stmt Initializer;
                if (Match(TokenType.SEMICOLON))
                {
                    Initializer = null;
                }
                else if (Match(TokenType.VAR))
                {
                    Initializer = VarDeclaration();
                }
                else
                {
                    Initializer = ExpressionStatement();
                }

                /* Loop condition */
                Expr Condition = null;
                if (!Check(TokenType.SEMICOLON))
                {
                    Condition = Expression();
                }
                Consume(TokenType.SEMICOLON, "Expect ';' after loop condition.");

                /* Incrementor */
                Expr Increment = null;
                if (!Check(TokenType.RIGHT_PAREN))
                {
                    Increment = Expression();
                }
                Consume(TokenType.RIGHT_PAREN, "Expect ')' after for clause.");

                /* Body */
                Stmt Body = Statement();

                /* Desugariing - or should I say "caramelization" ;-) */
                if (Increment != null)
                {
                    /* Add the incrementor to the body */
                    Body = new Stmt.Block(
                        new List<Stmt>()
                        {
                            Body,
                            new Stmt.Expression(Increment)
                        }
                    );
                }
                if (Condition == null)
                {
                    /* Add a missing condition */
                    Condition = new Expr.Literal(true);
                }
                /* Put a while loop around it */
                Body = new Stmt.While(Condition, Body);

                if (Initializer != null)
                {
                    /* Place the initializer in front of it all */
                    Body = new Stmt.Block(
                        new List<Stmt>()
                        {
                            Initializer,
                            Body
                        }
                    );
                }

                return Body;
            }
            finally
            {
                //LoopLevel--; //*** Chapter 9 - Challenge 3 - it's now done in the resolver
            }
        }
        private Stmt IfStatement()
        {
            //*** ifStmt → "if" "(" expression ")" statement ( "else" statement )? ;

            Consume(TokenType.LEFT_PAREN, "Expect '(' after if.");
            Expr Condition = Expression();
            Consume(TokenType.RIGHT_PAREN, "Expect ')' after condition.");

            Stmt ThenBranch = Statement();
            Stmt ElseBranch = null;

            if (Match(TokenType.ELSE)) {
                ElseBranch = Statement();
            }

            return new Stmt.If(Condition, ThenBranch, ElseBranch);
        }
        private Stmt PrintStatement()
        {
            //*** printStmt → "print" expression ";";

            Expr Value = Expression();
            Consume(TokenType.SEMICOLON, "Expect ';' after value.");
            return new Stmt.Print(Value);
        }
        private Stmt ReturnStatement()
        {
            //*** returnStmt → "return" expression ? ";" ;

            Token KeyWord = Previous();
            Expr Value = null;
            if (!Check(TokenType.SEMICOLON))
            {
                Value = Expression();
            }

            Consume(TokenType.SEMICOLON, "Expect ';' after return value.");
            return new Stmt.Return(KeyWord, Value);
        }
        private Stmt BreakStatement()
        {
            //*** Chapter 9 - Challenge 3
            //*** breaktStmt → "break" ";" ;  

            Token KeyWord = Previous();

            //if (LoopLevel < 1) Error(Previous(), "Not inside loop."); //*** Chapter 9 - Challenge 3 - it's now done in the resolver
            Consume(TokenType.SEMICOLON, "Expect ';' after break.");
            return new Stmt.Break(KeyWord);
        }
        private Stmt WhileStatement() {
            //*** whileStmt → "while" "(" expression ")" statement ;

            //LoopLevel++; //*** Chapter 9 - Challenge 3 - it's now done in the resolver
            try
            {
                Consume(TokenType.LEFT_PAREN, "Expect '(' after while.");
                Expr Condition = Expression();
                Consume(TokenType.RIGHT_PAREN, "Expect ')' after condition.");
                Stmt Body = Statement();

                return new Stmt.While(Condition, Body);
            }
            finally
            {
                //LoopLevel--; //*** Chapter 9 - Challenge 3 - it's now done in the resolver
            }

        }
        private Stmt ExpressionStatement()
        {
            //*** exprStmt → expression ";";

            Expr Expression = this.Expression();
            Consume(TokenType.SEMICOLON, "Expect ';' after expression.");
            return new Stmt.Expression(Expression);
        }
        private Stmt.Function Function(String Kind)
        {
            //*** function → IDENTIFIER "(" parameters? ")" block ;

            //*** name
            Token Name = Consume(TokenType.IDENTIFIER, "Expect " + Kind + " name.");
            Consume(TokenType.LEFT_PAREN, "Expect '(' after " + Kind + " name");

            //*** parameters
            List<Token> Parameters = new List<Token>();
            if (!Check(TokenType.RIGHT_PAREN)) {
                do
                {
                    if (Parameters.Count >= 255)
                    {
                        Error(Peek(), "Can't have more than 255 parameters.");
                    }

                    Parameters.Add(Consume(TokenType.IDENTIFIER, "Expect parameter name."));
                } while (Match(TokenType.COMMA));
            }
            Consume(TokenType.RIGHT_PAREN, "Expect ')' after parameters.");

            //*** body
            Consume(TokenType.LEFT_BRACE, "Expect '{' before " + Kind + " body.");
            List<Stmt> Body = Block();

            return new Stmt.Function(Name, Parameters, Body);
        }
        private Expr.Lambda Lambda()
        {
            //lambda → "(" parameters ? ")" block; (Chapter 10 - Challenge 2)  
            Consume(TokenType.LEFT_PAREN, "Expect '(' or function name after 'fun'.");

            //*** parameters
            List<Token> Parameters = new List<Token>();
            if (!Check(TokenType.RIGHT_PAREN))
            {
                do
                {
                    if (Parameters.Count >= 255)
                    {
                        Error(Peek(), "Can't have more than 255 parameters.");
                    }

                    Parameters.Add(Consume(TokenType.IDENTIFIER, "Expect parameter name."));
                } while (Match(TokenType.COMMA));
            }
            Consume(TokenType.RIGHT_PAREN, "Expect ')' after parameters.");

            //*** body
            Consume(TokenType.LEFT_BRACE, "Expect '{' before lambda body.");
            List<Stmt> Body = Block();

            return new Expr.Lambda(Parameters, Body);
        }
        private List<Stmt> Block()
        {
            //*** block → "{" declaration * "}";

            List<Stmt> Statements = new List<Stmt>();

            while (!Check(TokenType.RIGHT_BRACE) && !IsAtEnd())
            {
                Statements.Add(Declaration());
            }

            Consume(TokenType.RIGHT_BRACE, "Expect '}' after block.");

            return Statements;
        }
        private Expr Expression()
        {
            //*** expression → assignment ; 

            return Assignment();
        }
        private Expr Assignment()
        {
            //*** assignment → ( call "." )? IDENTIFIER "=" assignment | logic_or ;  
            Expr Expression = Or();

            if (Match(TokenType.EQUAL))
            {
                Token Equals = Previous();
                Expr Value = Assignment();

                if (Expression is Expr.Variable)
                {
                    Token Name = ((Expr.Variable)Expression).Name;
                    return new Expr.Assign(Name, Value);
                } else
                {
                    if (Expression is Expr.Get)
                    {
                        Expr.Get Get = (Expr.Get)Expression;
                        return new Expr.Set(Get.Object, Get.Name, Value);
                    }
                }

                Error(Equals, "Invalid assignment target.");
            }

            return Expression;
        }
        private Expr Or()
        {
            //*** logic_or → logic_and ( "or" logic_and )* ;

            Expr Expression = And();

            while (Match(TokenType.OR)) {
                Token Operator = Previous();
                Expr Right = And();
                Expression = new Expr.Logical(Expression, Operator, Right);
            }

            return Expression;
        }
        private Expr And()
        {
            //*** logic_and → expressionlist ( "and" expressionlist )* ; 

            //chapter 8 - changed for expression list and ternary operator
            Expr Expression = ExpressionList();

            while (Match(TokenType.AND)) {
                Token Operator = Previous();
                Expr Right = ExpressionList();
                Expression = new Expr.Logical(Expression, Operator, Right);
            }

            return Expression;
        }
        private Expr ExpressionList()
        {
            //*** expressionlist → ternary ("," ternary)* ;

            Expr Expression = Ternary();

            //*** Chapter 10 - removed because it messes with aparameter lists ins function calls - should be replaced with a proper Expr.List implementation
            /*
            while (Match(TokenType.COMMA))
            {
                Token Operator = Previous();
                Expr Right = Ternary();
                Expression = new Expr.Binary(Expression, Operator, Right);
            }
            */

            return Expression;  
        }
        private Expr Ternary()
        {
            //*** ternary → equality ("?" expression ":" expression) ;

            Expr Expression = Equality();

            if (Match(TokenType.QUESTION))
            {
                Expr Left = this.Expression();
                Consume(TokenType.COLON, "Expect ':' after expression.");
                Expr Right = this.Expression();
                
                Expression = new Expr.Ternary(Expression, Left, Right);
            }
            
            return Expression;
        }
        private Expr Equality()
        {
            //*** equality → comparison (("!=" | "==") comparison)* ;

            Expr Expression = Comparision();

            while (Match(TokenType.BANG_EQUAL, TokenType.EQUAL_EQUAL))
            {
                Token Operator = Previous();
                Expr Right = Comparision();
                Expression = new Expr.Binary(Expression, Operator, Right);
            }

            return Expression;
        }
        private Expr Comparision()
        {
            //*** comparison → term ((">" | ">=" | "<" | "<=") term)* ;

            Expr Expression = Term();

            while (Match(TokenType.GREATER, TokenType.GREATER_EQUAL, TokenType.LESS, TokenType.LESS_EQUAL))
            {
                Token Operator = Previous();
                Expr Right = Term();
                Expression = new Expr.Binary(Expression, Operator, Right);
            }

            return Expression;
        }
        private Expr Term()
        {
            //*** term → factor (("-" | "+") factor)* ;
            Expr Expression = Factor();

            while (Match(TokenType.PLUS, TokenType.MINUS))
            {
                Token Operator = Previous();
                Expr Right = Factor();
                Expression = new Expr.Binary(Expression, Operator, Right);
            }

            return Expression;
        }
        private Expr Factor()
        {
            //*** factor → unary (("/" | "*") unary) * ;

            Expr Expression = Unary();

            while (Match(TokenType.STAR, TokenType.SLASH))
            {
                Token Operator = Previous();
                Expr Right = Unary();
                Expression = new Expr.Binary(Expression, Operator, Right);
            }

            return Expression;
        }
        private Expr Unary()
        {
            //*** unary → ( "!" | "-" ) unary | call ;

            if (Match(TokenType.BANG, TokenType.MINUS))
            {
                Token Operator = Previous();
                Expr Right = Unary();
                return new Expr.Unary(Operator, Right);
            }

            return Call();
        }
        private Expr Call()
        {
            //*** (Chapter 10 - Challenge 2)
            //*** call           → lambdaCall | funCall;
            //*** lambdaCall     → "fun"("(" arguments ? ")" ) * ;          
            //*** funCall        → primary("(" arguments ? ")" | "." IDENTIFIER) * ;

            Expr Expression = null;

            //*** if we find "fun" this deep down the parsiong tree it's a lambda.
            if (Match(TokenType.FUN))
            {
                Expression = Lambda();
            } else
            {
                Expression = Primary();
            }

            while (true)
            {
                if (Match(TokenType.LEFT_PAREN))
                {
                    Expression = FinishCall(Expression);
                } else
                {
                    if (Match(TokenType.DOT))
                    {
                        Token Name = Consume(TokenType.IDENTIFIER, "Expect property name after '.'.");
                        Expression = new Expr.Get(Expression, Name);
                    } else
                    {
                        break;
                    }
                }
            }

            return Expression;
        }
        private Expr FinishCall(Expr Callee)
        {
            //*** arguments → expression ( "," expression )* ;

            List<Expr> Arguments = new List<Expr>();
            if (!Check(TokenType.RIGHT_PAREN))
            {
                do
                {
                    if (Arguments.Count >= 255)
                    {
                        Error(Peek(), "Can't have more than 255 arguments.");
                    }
                    Arguments.Add(Expression());
                } while (Match(TokenType.COMMA));
            }

            Token Parenthesis = Consume(TokenType.RIGHT_PAREN, "Expect ')' after arguments.");

            return new Expr.Call(Callee, Parenthesis, Arguments);        
        }
        private Expr Primary()
        {
            //*** primary → "true" | "false" | "nil" | "this" | NUMBER | STRING | IDENTIFIER | "super" "." IDENTIFIER | "(" expression ")" ;

            if (Match(TokenType.FALSE)) return new Expr.Literal(false);
            if (Match(TokenType.TRUE)) return new Expr.Literal(true);
            if (Match(TokenType.NIL)) return new Expr.Literal(null);

            if (Match(TokenType.NUMBER, TokenType.STRING))
            {
                return new Expr.Literal(Previous().Literal);
            }

            if (Match(TokenType.SUPER))
            {
                Token KeyWord = Previous();
                Consume(TokenType.DOT, "Expect '.' after 'super'.");
                Token Method = Consume(TokenType.IDENTIFIER, "Expect superclass method name.");
                return new Expr.Super(KeyWord, Method);
            }

            if (Match(TokenType.THIS)) return new Expr.This(Previous());
            if (Match(TokenType.IDENTIFIER)) return new Expr.Variable(Previous());

            if (Match(TokenType.LEFT_PAREN))
            {
                Expr Expression = this.Expression();
                Consume(TokenType.RIGHT_PAREN, "Expext ')' after expression.");
                return new Expr.Grouping(Expression);
            }

            //*** Binary operator at the beginning?
            if (Match(TokenType.COMMA,
                      /*TokenType.COLON,*/
                      TokenType.BANG_EQUAL, TokenType.EQUAL_EQUAL,
                      TokenType.GREATER, TokenType.GREATER_EQUAL, TokenType.LESS, TokenType.LESS_EQUAL,
                      TokenType.PLUS,
                      TokenType.SLASH, TokenType.STAR))
            {
                Token Operator = Previous();
                Expr Expression = this.Expression();
                throw (Error(Operator, "not allowed at beginning of expression."));
            }

            throw Error(Peek(), "Expect expression.");   
        }
        private Boolean Match(params TokenType[] Types)
        {
            foreach (TokenType Type in Types)
            {
                if (Check(Type))
                {
                    Advance();
                    return true;
                }
            }

            return false;
        }
        private Token Consume(TokenType Type, string Message)
        {
            if (Check(Type)) return Advance();

            throw Error(Peek(), Message);
        }
        private Boolean Check(TokenType Type)
        {
            if (IsAtEnd()) return false;
            return Peek().Type == Type;
        }
        private Token Advance()
        {
            if (!IsAtEnd()) Current++;
            return Previous();
        }        
        private Token Recede() //*** Chapter 10 - Challenge 2
        {
            if (Current > 0) Current--;
            return Next(); //return the token that has been stepped over
        }
        private Boolean IsAtEnd()
        {
            return Peek().Type == TokenType.EOF;
        }
        private Token Peek()
        {
            return Tokens[Current];
        }
        private Token Previous()
        {
            return Tokens[Current - 1];
        }
        private Token Next()
        {
            return Tokens[Current + 1];
        }
        private ParseError Error(Token aToken, string Message)
        {
            Lox.Error(aToken, Message);
            return new ParseError();
        }
        private void Synchronize()
        {
            Advance();

            while (!IsAtEnd()) {
                if (Previous().Type == TokenType.SEMICOLON) return;

                switch (Peek().Type)
                {
                    case TokenType.CLASS:
                    case TokenType.FUN:
                    case TokenType.VAR:
                    case TokenType.FOR:
                    case TokenType.IF:
                    case TokenType.WHILE:
                    case TokenType.PRINT:
                    case TokenType.BREAK: //*** Chapter 9 - Challenge 3
                    case TokenType.RETURN:
                        return;
                }

                Advance();
            }
        }
    } 
}

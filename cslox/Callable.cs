using System;
using System.IO;
using System.Text;
using System.Collections.Generic;

namespace cslox
{
    public interface ICallable
    {
        Object Call(Interpreter Interpreter, List<Object> Arguments);
        int Arity();
    }

    //*** Define alle your natvie functions here...
    public class NativeClock : ICallable
    {

        public int Arity() { return 0; }
        public Object Call(Interpreter Interpreter, List<Object> Arguments)
        {
            return (double)(DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond) / 1000.0;
        }
        public override string ToString() { return "<native fn 'clock'>"; }

    }

    //*** Chapter 13.4 - own extensions

    //*** number|string input()
    public class NativeInput : ICallable
    {
        public int Arity() { return 0; }
        public Object Call(Interpreter Interpreter, List<Object> Arguments)
        {
            string Input = Console.ReadLine();
            if (double.TryParse(Input, out _))
            {
                return double.Parse(Input);
            }

            return Input;
        }
        public override string ToString() { return "<native fn 'input'>"; }

    }

    //*** bool file_exists(<string filename>) 
    public class NativeFileExists : ICallable
    {
        public int Arity() { return 1; }
        public Object Call(Interpreter Interpreter, List<Object> Arguments)
        {
            Boolean Result = false;

            if (Arguments[0] is string)
            {
                string FileName = Arguments[0] as string;
                Result = File.Exists(FileName);
            }
            else
            {
                //*** TODO - Think about correct Errormessages from native functions
                throw new RuntimeError(new Token(TokenType.IDENTIFIER, "file_exists", null, -1), "argument must be string.");
            }

            return Result;
        }
        public override string ToString() { return "<native fn 'file_exists'>"; }

    }

    //*** string file_readtext(string filename) 
    public class NativeFileReadText : ICallable
    {
        public int Arity() { return 1; }
        public Object Call(Interpreter Interpreter, List<Object> Arguments)
        {
            string Result = "";

            if (Arguments[0] is string)
            {
                string FileName = Arguments[0] as string;

                if (!File.Exists(FileName)) 
                {
                    throw new RuntimeError(new Token(TokenType.IDENTIFIER, "file_readtext", null, -1), "file not found.");
                };

                Result = File.ReadAllText(FileName);
            }
            else
            {
                throw new RuntimeError(new Token(TokenType.IDENTIFIER, "file_readtext", null, -1), "argument must be string.");
            }

            return Result;
        }
        public override string ToString() { return "<native fn 'file_readtext'>"; }

    }

    //*** null file_writetext(string filename, string text)
    public class NativeFileWriteText : ICallable
    {
        public int Arity() { return 2; }
        public Object Call(Interpreter Interpreter, List<Object> Arguments)
        {
            if ((Arguments[0] is string) && (Arguments[1] is string))
            {
                string FileName = Arguments[0] as string;
                string Text = Arguments[1] as string;

                File.WriteAllText(FileName, Text);
            }
            else
            {
                throw new RuntimeError(new Token(TokenType.IDENTIFIER, "file_readtext", null, -1), "first argument must be string.");
            }

            return null;
        }
        public override string ToString() { return "<native fn 'file_writetext'>"; }

    }
    //*** FileStream file_open(string filename, double mode)
    public class NativeFileOpen : ICallable
    {
        public int Arity() { return 2; }
        public Object Call(Interpreter Interpreter, List<Object> Arguments)
        {
            FileStream Stream = null;

            if (Arguments[0] is string)
            {

                string FileName = Arguments[0] as string;

                if (Arguments[1] is double) //<- Lox numbers are always double
                {                    
                    Stream = FileIO.Open(FileName, (FileMode)Convert.ToInt32(Arguments[1])
                    );
                }
                else
                {
                    throw new RuntimeError(new Token(TokenType.IDENTIFIER, "file_open", null, -1), "second argument must be number.");
                }
            }
            else
            {
                throw new RuntimeError(new Token(TokenType.IDENTIFIER, "file_open", null, -1), "first argument must be string.");
            }

            return Stream;
        }
        public override string ToString() { return "<native fn 'file_open'>"; }

    }
    //*** null file_close(FileStream stream)
    public class NativeFileClose : ICallable
    {
        public int Arity() { return 1; }
        public object Call(Interpreter interpreter, List<Object> Arguments)
        {
            if (Arguments[0] is FileStream)
            {
                FileIO.Close((FileStream)Arguments[0]);                 
            }
            else
            {
                throw new RuntimeError(new Token(TokenType.IDENTIFIER, "file_close", null, -1), "argument must be a valid file stream.");
            }

            return null;
        }
        public override string ToString() { return "<native fn 'file_close'>"; }
    }
    //*** null file_write(FileStream stream, object data)
    public class NativeFileWrite : ICallable
    {
        public int Arity() { return 2; }
        public object Call(Interpreter interpreter, List<Object> Arguments)
        {
            if (Arguments[0] is FileStream)
            {
                FileIO.Write((FileStream)Arguments[0], Arguments[1]);
            }
            else
            {
                throw new RuntimeError(new Token(TokenType.IDENTIFIER, "file_write", null, -1), "first argument must be a valid file stream.");
            }

            return null;
        }
        public override string ToString() { return "<native fn 'file_write'>"; }
    }
    //*** null file_writeline(FileStream stream, object data)//*** null file_writeline(FileStream stream, object data)
    public class NativeFileWriteLine : ICallable
    {
        public int Arity() { return 2; }
        public object Call(Interpreter interpreter, List<Object> Arguments)
        {
            if (Arguments[0] is FileStream)
            {
                FileIO.WriteLine((FileStream)Arguments[0], Arguments[1]);
            }
            else
            {
                throw new RuntimeError(new Token(TokenType.IDENTIFIER, "file_writeline", null, -1), "first argument must be a valid file stream.");
            }

            return null;
        }
        public override string ToString() { return "<native fn 'file_writeline'>"; }
    }
    //*** string file_read(FileStream stream)
    public class NativeFileRead : ICallable
    {
        public int Arity() { return 1; }
        public object Call(Interpreter interpreter, List<Object> Arguments)
        {
            if (Arguments[0] is FileStream)
            {
                return FileIO.Read((FileStream)Arguments[0]);
            }
            else
            {
                throw new RuntimeError(new Token(TokenType.IDENTIFIER, "file_read", null, -1), "first argument must be a valid file stream.");
            }
        }
        public override string ToString() { return "<native fn 'file_read'>"; }
    }
    //*** string file_readline(FileStream stream)
    public class NativeFileReadLine : ICallable
    {
        public int Arity() { return 1; }
        public object Call(Interpreter interpreter, List<Object> Arguments)
        {
            if (Arguments[0] is FileStream)
            {
                return FileIO.ReadLine((FileStream)Arguments[0]);
            }
            else
            {
                throw new RuntimeError(new Token(TokenType.IDENTIFIER, "file_readline", null, -1), "first argument must be a valid file stream.");
            }
        }
        public override string ToString() { return "<native fn 'file_readline'>"; }
    }
    //*** null file_seek_bof(FileStream stream)
    public class NativeSeekBOF : ICallable
    {
        public int Arity() { return 1; }
        public object Call(Interpreter interpreter, List<Object> Arguments)
        {
            if (Arguments[0] is FileStream)
            {
                FileIO.Seek((FileStream)Arguments[0], SeekOrigin.Begin);
            }
            else
            {
                throw new RuntimeError(new Token(TokenType.IDENTIFIER, "seek_bof", null, -1), "argument must be a valid file stream.");
            }
            return null;
        }
        public override string ToString() { return "<native fn 'seek_bof'>"; }
    }
    //*** null file_seek_eof(FileStream stream)
    public class NativeSeekEOF : ICallable
    {
        public int Arity() { return 1; }
        public object Call(Interpreter interpreter, List<Object> Arguments)
        {
            if (Arguments[0] is FileStream)
            {
                FileIO.Seek((FileStream)Arguments[0], SeekOrigin.End);
            }
            else
            {
                throw new RuntimeError(new Token(TokenType.IDENTIFIER, "seek_eof", null, -1), "argument must be a valid file stream.");
            }
            return null;
        }
        public override string ToString() { return "<native fn 'seek_eof'>"; }
    }

}
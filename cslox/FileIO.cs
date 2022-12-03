using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace cslox
{
    public static class FileIO
    {
        public static FileStream Open(string FileName, FileMode Mode)
        {
            FileStream Stream = File.Open(FileName, Mode);
            return Stream;
        }
        public static void Close(FileStream Stream)
        {
            Stream.Close();
            Stream.Dispose();   
        }
        public static void Write(FileStream Stream, Object Data)
        {
            byte[] Buffer = Encoding.UTF8.GetBytes(Data.ToString()); //<- TODO: by now everything is written as string

            Stream.Write(Buffer);
        }
        public static void WriteLine(FileStream Stream, Object Data)
        {
            byte[] Buffer = Encoding.UTF8.GetBytes(Data.ToString() + "\r\n"); 

            Stream.Write(Buffer);
        }
        public static string Read(FileStream Stream)
        {
            //read a single character

            //we simply support UTF8 (and ASCII)...

            byte[] Bytes = new byte[4]; //<- according to RFC 3629 max 4 byte chars are allowed
            int ByteCount = 1;
            string Result = ""; 

            int ReadCount = Stream.Read(Bytes, 0, 1);

            if (ReadCount > 0)
            {
                switch (Bytes[0]) 
                {
                    case >= 0xF0:
                        //4 byte char
                        ByteCount = 4;
                        break;
                    case >= 0xE0:
                        //3 byte char
                        ByteCount = 3;
                        break;
                    case >= 0xC0:
                        //2 byte char
                        ByteCount = 2;
                        break;
                }
                if (ByteCount > 1)
                {
                    ReadCount = Stream.Read(Bytes, 1, ByteCount - 1); //<- first byte is allready there...
                }
                Result = Encoding.UTF8.GetString(Bytes, 0,  ByteCount);
            }
            return Result;
        }
        public static string ReadLine(FileStream Stream)
        {
            //read a complete line

            string Result = "";

            //read first char
            string Char = Read(Stream); 
            while (Char != "")
            {
                //ommit CR
                if (Char == "\r")
                {
                    Char = "";
                } 
                //break on LF
                if (Char == "\n")
                {
                    break;
                }
                
                //concatenate result
                Result = Result + Char;
                //read next char
                Char = Read(Stream);
            }

            return Result;
        }
        public static void Seek(FileStream Stream, SeekOrigin Position)
        {
            Stream.Seek(0, Position);
        }
    }
}

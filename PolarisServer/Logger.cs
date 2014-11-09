using System;
using System.IO;

namespace PolarisServer
{
    public enum LogType
    {
        Normal,
        Internal,
        Warning,
        Error
    }

    public static class Logger
    {
        private static StreamWriter writer = new StreamWriter("PolarisServer.log");

        public static void Write(string text, LogType type = LogType.Normal)
        {
            if (type == LogType.Internal)
                Console.ForegroundColor = ConsoleColor.Cyan;
            else if (type == LogType.Warning)
                Console.ForegroundColor = ConsoleColor.Yellow;
            else if (type == LogType.Error)
                Console.ForegroundColor = ConsoleColor.Red;
            else
                Console.ResetColor();

            Console.WriteLine(text);
            writer.WriteLine(DateTime.Now.ToString() + " - " + text);

            // Later we should probably only flush once every X amount of lines or on some other condition
            writer.Flush();
        }
    }
}

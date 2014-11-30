using System;
using System.IO;

namespace PolarisServer
{
    /// <summary>
    /// Wrapper for Console's Write and WriteLine functions to add coloring as well as dumping to a log file.
    /// </summary>
    public static class Logger
    {
        private static StreamWriter writer = new StreamWriter("PolarisServer.log");

        public static void Write(string text, params object[] args)
        {
            Console.ResetColor();
            Console.Write(text, args);
            WriteFile(text, args);
        }

        public static void WriteLine(string text, params object[] args)
        {
            Console.ResetColor();
            Console.WriteLine(text, args);
            WriteFile(text, args);
        }

        public static void WriteInternal(string text, params object[] args)
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine(text, args);
            WriteFile(text, args);
        }

        public static void WriteWarning(string text, params object[] args)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine(text, args);
            WriteFile(text, args);
        }

        public static void WriteError(string text, params object[] args)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(text, args);
            WriteFile(text, args);
        }

        public static void WriteException(string message, Exception ex)
        {
            Logger.WriteError("[ERR] {0}: {1}", ex.GetType(), ex.ToString());
            if (ex.StackTrace != null)
                Logger.WriteError("[ERR] Stack Trace:\n{0}", ex.StackTrace.ToString());
            if (ex.InnerException != null)
                Logger.WriteError("[ERR] Inner Exception:\n{0}", ex.InnerException.ToString());
            Console.ResetColor();
        }

        public static void WriteFile(string text, params object[] args)
        {
            writer.WriteLine(DateTime.Now.ToString() + " - " + text, args);

            // Later we should probably only flush once every X amount of lines or on some other condition
            writer.Flush();
        }
    }
}

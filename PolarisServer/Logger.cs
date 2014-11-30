using System;
using System.Collections.Generic;
using System.IO;

namespace PolarisServer
{
    /// <summary>
    /// A line of text accompanied by it's color for the Console System
    /// </summary>
    public class LogLine
    {
        public ConsoleColor color;
        public string text;

        public override string ToString()
        {
            return text;
        }
    }

    /// <summary>
    /// Wrapper for Console's Write and WriteLine functions to add coloring as well as integrate it into the Console System and add dumping to a log file.
    /// </summary>
    public static class Logger
    {
        private static StreamWriter writer = new StreamWriter("PolarisServer.log");

        public static List<LogLine> lines = new List<LogLine>();

        private static void AddLine(LogLine line)
        {
            // Return if we don't have a ConsoleSystem created yet
            if (PolarisApp.ConsoleSystem == null) return;

            // Split the lines and append it into new lines if it's too big
            if (line.text.Length >= Console.WindowWidth)
            {
                List<LogLine> splitLines = new List<LogLine>();
                int splits = line.text.Length / Console.WindowWidth;

                for (int i = 0; i <= splits; i++)
                {
                    LogLine splitLine = new LogLine();
                    int start = i * Console.WindowWidth;
                    int length = Console.WindowWidth;
                    if (length >= line.text.Length - start)
                        length = line.text.Length - start;

                    splitLine.color = line.color;
                    splitLine.text = line.text.Substring(start, length);

                    splitLines.Add(splitLine);
                }

                foreach (LogLine newLine in splitLines)
                    lines.Add(newLine);
            }
            else // Add the line normally
                lines.Add(line);

            while (lines.Count > Console.WindowHeight - 4)
                lines.RemoveAt(0);

            // Tell the console to refresh
            PolarisApp.ConsoleSystem.refreshDraw = true;
        }

        public static void Write(string text, params object[] args)
        {
            LogLine line = new LogLine();
            line.color = ConsoleColor.White;
            line.text = string.Format(text, args);

            AddLine(line);
            WriteFile(text, args);
        }

        public static void WriteInternal(string text, params object[] args)
        {
            LogLine line = new LogLine();
            line.color = ConsoleColor.Cyan;
            line.text = string.Format(text, args);

            AddLine(line);
            WriteFile(text, args);
        }

        public static void WriteCommand(string text, params object[] args)
        {
            LogLine line = new LogLine();
            line.color = ConsoleColor.Green;
            line.text = string.Format(text, args);

            AddLine(line);
            WriteFile(text, args);
        }

        public static void WriteWarning(string text, params object[] args)
        {
            LogLine line = new LogLine();
            line.color = ConsoleColor.Yellow;
            line.text = string.Format(text, args);

            AddLine(line);
            WriteFile(text, args);
        }

        public static void WriteError(string text, params object[] args)
        {
            LogLine line = new LogLine();
            line.color = ConsoleColor.Red;
            line.text = string.Format(text, args);

            AddLine(line);
            WriteFile(text, args);
        }

        public static void WriteException(string message, Exception ex)
        {
            LogLine line = new LogLine();
            line.color = ConsoleColor.Red;
            line.text = string.Empty;

            line.text += string.Format("[ERR] {0} - {1}: {2}", message, ex.GetType(), ex.ToString());
            if (ex.InnerException != null)
                line.text += string.Format("[ERR] Inner Exception: {0}", ex.InnerException.ToString());

            WriteFile(line.text);
            
            // Strip the crap out of the exception so that the line splitting works properly on it
            line.text = line.text.Replace('\r', ' ');
            line.text = line.text.Replace('\n', ' ');
            line.text = line.text.Replace("     ", " ");

            AddLine(line);
        }

        public static void WriteFile(string text, params object[] args)
        {
            writer.WriteLine(DateTime.Now.ToString() + " - " + text, args);

            // Later we should probably only flush once every X amount of lines or on some other condition
            writer.Flush();
        }
    }
}

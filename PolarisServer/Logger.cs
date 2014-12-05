using System;
using System.Collections.Generic;
using System.IO;

using PolarisServer.Packets;

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

        public static bool VerbosePackets = false;

        private static void AddLine(LogLine line)
        {
            // Return if we don't have a ConsoleSystem created yet
            if (PolarisApp.ConsoleSystem == null) return;

            // Split the lines and append it into new lines if it's too big
            if (line.text.Length >= PolarisApp.ConsoleSystem.width)
            {
                List<LogLine> splitLines = new List<LogLine>();
                int splits = line.text.Length / Console.WindowWidth;

                for (int i = 0; i <= splits; i++)
                {
                    LogLine splitLine = new LogLine();
                    int start = i * PolarisApp.ConsoleSystem.width;
                    int length = PolarisApp.ConsoleSystem.width;
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

            // Push old lines off the buffer
            try
            {
                while (lines.Count > PolarisApp.ConsoleSystem.height - 4)
                    lines.RemoveAt(0);
            }
            catch (Exception ex)
            {
                Logger.WriteException("Error pushing lines off the log", ex);
            }

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

        public static void WriteCommand(Client client, string text, params object[] args)
        {
            if (client == null)
            {
                LogLine line = new LogLine();
                line.color = ConsoleColor.Green;
                line.text = string.Format(text, args);

                AddLine(line);
                WriteFile(text, args);
            }
            else
                WriteClient(client, text, args);
        }

        public static void WriteClient(Client client, string text, params object[] args)
        {
            string message = string.Format(text, args).Replace('\\', '/');
            SystemMessagePacket packet = new SystemMessagePacket(message, SystemMessagePacket.MessageType.SystemMessage);
            client.SendPacket(packet);
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

        public static void WriteHex(string text, byte[] array)
        {
            LogLine messageLine = new LogLine();
            messageLine.color = ConsoleColor.DarkCyan;
            messageLine.text = text;
            AddLine(messageLine);

            // Calculate lines
            int lines = 0;
            for (int i = 0; i < array.Length; i++)
                if ((i % 16) == 0)
                    lines++;

            for (int i = 0; i < lines; i++)
            {
                string hexString = string.Empty;

                // Address
                hexString += string.Format("{0:X8} ", i * 16);

                // Bytes
                for (int j = 0; j < 16; j++)
                {
                    if (j + (i * 16) >= array.Length)
                        break;

                    hexString += string.Format("{0:X2} ", array[j + (i * 16)]);
                };

                // Spacing
                while (hexString.Length < 16 * 4)
                    hexString += ' ';

                // ASCII
                for (int j = 0; j < 16; j++)
                {
                    if (j + (i * 16) >= array.Length)
                        break;

                    char asciiChar = (char)array[j + (i * 16)];

                    if (asciiChar == (char)0x00)
                        asciiChar = '.';

                    hexString += asciiChar;
                };

                // Strip off unnecessary stuff
                hexString = hexString.Replace('\a', ' '); // Alert beeps
                hexString = hexString.Replace('\n', ' '); // Newlines
                hexString = hexString.Replace('\r', ' '); // Carriage returns
                hexString = hexString.Replace('\\', ' '); // Escape break

                LogLine hexLine = new LogLine();
                hexLine.color = ConsoleColor.White;
                hexLine.text = hexString;
                AddLine(hexLine);
                WriteFile(hexString);
            }
        }

        public static void WriteFile(string text, params object[] args)
        {
            if (args.Length > 0)
                writer.WriteLine(DateTime.Now.ToString() + " - " + text, args);
            else
                writer.WriteLine(DateTime.Now.ToString() + " - " + text);

            // Later we should probably only flush once every X amount of lines or on some other condition
            writer.Flush();
        }
    }
}

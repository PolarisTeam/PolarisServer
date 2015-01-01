using System;
using System.Collections.Generic;
using System.IO;

using PolarisServer.Packets;

namespace PolarisServer
{
    /// <summary>
    /// Wrapper for Console's Write and WriteLine functions to add coloring as well as integrate it into the Console System and add dumping to a log file.
    /// </summary>
    public static class Logger
    {
        private static StreamWriter writer = new StreamWriter("PolarisServer.log");

        public static bool VerbosePackets = false;

        private static void AddLine(ConsoleColor color, string text)
        {
            // Return if we don't have a ConsoleSystem created yet
            if (PolarisApp.ConsoleSystem == null) return;

            PolarisApp.ConsoleSystem.AddLine(color, text);
        }

        public static void Write(string text, params object[] args)
        {
            AddLine(ConsoleColor.White, string.Format(text, args));
            WriteFile(text, args);
        }

        public static void WriteInternal(string text, params object[] args)
        {
            AddLine(ConsoleColor.Cyan, string.Format(text, args));
            WriteFile(text, args);
        }

        public static void WriteCommand(Client client, string text, params object[] args)
        {
            if (client == null)
            {
                AddLine(ConsoleColor.Green, string.Format(text, args));
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
            AddLine(ConsoleColor.Yellow, string.Format(text, args));
            WriteFile(text, args);
        }

        public static void WriteError(string text, params object[] args)
        {
            AddLine(ConsoleColor.Red, string.Format(text, args));
            WriteFile(text, args);
        }

        public static void WriteException(string message, Exception ex)
        {
            string text = string.Empty;

            text += string.Format("[ERR] {0} - {1}: {2}", message, ex.GetType(), ex.ToString());
            if (ex.InnerException != null)
                text += string.Format("[ERR] Inner Exception: {0}", ex.InnerException.ToString());

            WriteFile(text);

            // Strip the crap out of the exception so that the line splitting works properly on it
            text = text.Replace('\r', ' ');
            text = text.Replace('\n', ' ');
            text = text.Replace("     ", " ");

            AddLine(ConsoleColor.Red, text);
        }

        public static void WriteHex(string text, byte[] array)
        {
            AddLine(ConsoleColor.DarkCyan, text);

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

                AddLine(ConsoleColor.White, hexString);
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

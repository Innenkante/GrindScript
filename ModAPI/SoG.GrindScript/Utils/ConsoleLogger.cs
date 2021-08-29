using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection;
using static SoG.Modding.Utils.ILogger;

namespace SoG.Modding.Utils
{
    /// <summary>
    /// A simple class for logging information to the console.
    /// </summary>
    public class ConsoleLogger : ILogger
    {
        readonly static (ConsoleColor, ConsoleColor)[] _levelColors = new (ConsoleColor, ConsoleColor)[]
        {
            (ConsoleColor.Black, ConsoleColor.DarkBlue),
            (ConsoleColor.Black, ConsoleColor.Blue),
            (ConsoleColor.Black, ConsoleColor.White),
            (ConsoleColor.DarkGreen, ConsoleColor.Yellow),
            (ConsoleColor.DarkYellow, ConsoleColor.White),
            (ConsoleColor.DarkRed, ConsoleColor.White)
        };

        LogLevels _logLevel = LogLevels.Warn;
        public LogLevels LogLevel
        {
            get => _logLevel;
            set => _logLevel = LogLevels.Trace > value ? LogLevels.Trace : (LogLevels.None < value ? LogLevels.None : value);
        }

        public ConsoleColor SourceColor = ConsoleColor.Blue;
        public string DefaultSource = "";

        public ConsoleLogger() { }

        public ConsoleLogger(LogLevels logLevel)
        {
            LogLevel = logLevel;
        }

        public ConsoleLogger(LogLevels logLevel, string source)
        {
            LogLevel = logLevel;
            DefaultSource = source;
        }

        /// <summary>
        /// Logs the message if its level is equal or higher in severity than LogLevel. <para/>
        /// If source is empty, DefaultSource is used.
        /// </summary>
        public void Log(LogLevels level, string msg, string source = "")
        {
            if (level < _logLevel || _logLevel == LogLevels.None) return;
            string sourceToUse = source != "" ? source : DefaultSource;
            lock (this)
            {
                var bgColor = Console.BackgroundColor;
                var fgColor = Console.ForegroundColor;

                Console.BackgroundColor = _levelColors[(int)level].Item1;
                Console.ForegroundColor = _levelColors[(int)level].Item2;

                Console.Write($"[{level}]");
                WriteSpace();

                Console.BackgroundColor = ConsoleColor.Black;
                Console.ForegroundColor = SourceColor;

                if (sourceToUse != "")
                {
                    Console.Write($"[{sourceToUse}]");
                    WriteSpace();
                }

                Console.BackgroundColor = ConsoleColor.Black;
                Console.ForegroundColor = ConsoleColor.White;

                Console.Write($"{msg}");

                Console.WriteLine();

                Console.BackgroundColor = bgColor;
                Console.ForegroundColor = fgColor;
            }
        }

        public void Trace(string msg, string source = "")
        {
            Log(LogLevels.Trace, msg, source);
        }

        public void Debug(string msg, string source = "")
        {
            Log(LogLevels.Debug, msg, source);
        }

        public void Info(string msg, string source = "")
        {
            Log(LogLevels.Info, msg, source);
        }

        public void Warn(string msg, string source = "")
        {
            Log(LogLevels.Warn, msg, source);
        }

        public void Error(string msg, string source = "")
        {
            Log(LogLevels.Error, msg, source);
        }

        public void Fatal(string msg, string source = "")
        {
            Log(LogLevels.Fatal, msg, source);
        }

        private void WriteSpace(int howMany = 1)
        {
            var bgColor = Console.BackgroundColor;
            var fgColor = Console.ForegroundColor;

            Console.BackgroundColor = ConsoleColor.Black;
            Console.ForegroundColor = ConsoleColor.White;

            string space = "";
            while (howMany-- > 0) space += " ";

            Console.Write(space);

            Console.BackgroundColor = bgColor;
            Console.ForegroundColor = fgColor;
        }
    }
}

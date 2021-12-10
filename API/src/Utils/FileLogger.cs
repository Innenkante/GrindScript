using System;
using System.IO;
using System.Text;

namespace SoG.Modding.Utils
{
    public class FileLogger : ILogger
    {
        LogLevels _logLevel = LogLevels.Warn;
        public LogLevels LogLevel
        {
            get => _logLevel;
            set => _logLevel = LogLevels.Trace > value ? LogLevels.Trace : (LogLevels.None < value ? LogLevels.None : value);
        }

        /// <summary>
        /// The next logger in the chain. If not null, messages will also be sent to this logger.
        /// </summary>
        public ILogger NextLogger { get; set; } = null;

        public string FilePath { get; set; } = "FileLog.txt";

        public string DefaultSource { get; set; } = "";

        public FileLogger() { }

        public FileLogger(LogLevels logLevel)
        {
            LogLevel = logLevel;
        }

        public FileLogger(LogLevels logLevel, string source)
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
            // Some caching would be nice, however ILogger needs to implement IDisposable

            if (level < _logLevel || _logLevel == LogLevels.None)
            {
                return;
            }

            string sourceToUse = source != "" ? source : DefaultSource;

            _buffer.Append($"[{level}]");
            WriteSpace();

            if (sourceToUse != "")
            {
                _buffer.Append($"[{sourceToUse}]");
                WriteSpace();
            }

            _buffer.AppendLine($"{msg}");

            if (_buffer.Length >= 0.75f * _buffer.Capacity)
            {
                FlushToDisk();
            }
        }

        public void Trace(string msg, string source = "")
        {
            Log(LogLevels.Trace, msg, source);

            NextLogger?.Trace(msg, source);
        }

        public void Debug(string msg, string source = "")
        {
            Log(LogLevels.Debug, msg, source);

            NextLogger?.Debug(msg, source);
        }

        public void Info(string msg, string source = "")
        {
            Log(LogLevels.Info, msg, source);

            NextLogger?.Info(msg, source);
        }

        public void Warn(string msg, string source = "")
        {
            Log(LogLevels.Warn, msg, source);

            NextLogger?.Warn(msg, source);
        }

        public void Error(string msg, string source = "")
        {
            Log(LogLevels.Error, msg, source);

            NextLogger?.Error(msg, source);
        }

        public void Fatal(string msg, string source = "")
        {
            Log(LogLevels.Fatal, msg, source);

            NextLogger?.Fatal(msg, source);
        }

        public void FlushToDisk()
        {
            lock (this)
            {
                StreamWriter writer = null;
                try
                {
                    writer = new StreamWriter(new FileStream(FilePath, FileMode.Append, FileAccess.Write));

                    writer.Write(_buffer);
                    _buffer.Clear();
                }
                catch (Exception e)
                {
                    Console.WriteLine("Unable to flush FileLogger. " + e.Message);
                }
                finally
                {
                    writer?.Close();
                }
            }
        }

        private StringBuilder _buffer = new StringBuilder(4096);

        private void WriteSpace(int howMany = 1)
        {
            string space = "";
            while (howMany-- > 0) space += " ";

            _buffer.Append(space);
        }
    }
}

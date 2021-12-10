namespace SoG.Modding.Utils
{
    /// <summary>
    /// The severity level of a log message.
    /// By default, messages are logged if their level is equal or more severe than the current one.
    /// </summary>
    public enum LogLevels : sbyte
    {
        Trace = 0,
        Debug = 1,
        Info = 2,
        Warn = 3,
        Error = 4,
        Fatal = 5,
        None = 6
    }

    /// <summary>
    /// Provides logging capabilities.
    /// </summary>
    public interface ILogger
    {
        /// <summary> Gets or sets the current log level. </summary>
        LogLevels LogLevel { get; set; }

        /// <summary> Returns the next logger in the log chain, if any. </summary>
        ILogger NextLogger { get; }

        /// <summary> Logs a message at the Trace level. </summary>
        void Trace(string msg, string source = "");

        /// <summary> Logs a message at the Debug level. </summary>
        void Debug(string msg, string source = "");

        /// <summary> Logs a message at the Info level. </summary>
        void Info(string msg, string source = "");

        /// <summary> Logs a message at the Warn level. </summary>
        void Warn(string msg, string source = "");

        /// <summary> Logs a message at the Error level. </summary>
        void Error(string msg, string source = "");

        /// <summary> Logs a message at the Fatal level. </summary>
        void Fatal(string msg, string source = "");
    }
}
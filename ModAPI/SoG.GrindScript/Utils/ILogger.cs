namespace SoG.Modding.Utils
{
    public enum LogLevels : sbyte
    {
        Trace = 0, Debug = 1, Info = 2, Warn = 3, Error = 4, Fatal = 5, None = 6
    }

    public interface ILogger
    {
        LogLevels LogLevel { get; set; }

        void Trace(string msg, string source = "");

        void Debug(string msg, string source = "");

        void Info(string msg, string source = "");

        void Warn(string msg, string source = "");

        void Error(string msg, string source = "");

        void Fatal(string msg, string source = "");
    }
}
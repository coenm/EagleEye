namespace EagleEye.ExifTool
{
    using System;

    using CoenM.ExifToolLib.Logging;
    using NLog;

    using LogLevel = CoenM.ExifToolLib.Logging.LogLevel;

    internal class ExifToolLogAdapter : CoenM.ExifToolLib.Logging.ILogger
    {
        private static readonly NLog.Logger Logger = LogManager.GetCurrentClassLogger();

        public void Log(LogEntry entry)
        {
            if (entry.Exception == null)
                Logger.Log(Convert(entry.Severity), entry.Message);
            else
                Logger.Log(Convert(entry.Severity), entry.Exception, entry.Message);
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return Logger.IsEnabled(Convert(logLevel));
        }

        public bool IsEnabled()
        {
            return IsEnabled(LogLevel.Trace)
                   || IsEnabled(LogLevel.Debug)
                   || IsEnabled(LogLevel.Info)
                   || IsEnabled(LogLevel.Warn)
                   || IsEnabled(LogLevel.Error)
                   || IsEnabled(LogLevel.Fatal);
        }

        private static NLog.LogLevel Convert(LogLevel logLevel)
        {
            return logLevel switch
            {
                LogLevel.Trace => NLog.LogLevel.Trace,
                LogLevel.Debug => NLog.LogLevel.Debug,
                LogLevel.Info => NLog.LogLevel.Info,
                LogLevel.Warn => NLog.LogLevel.Warn,
                LogLevel.Error => NLog.LogLevel.Error,
                LogLevel.Fatal => NLog.LogLevel.Fatal,
                _ => throw new ArgumentOutOfRangeException(nameof(logLevel), logLevel, null)
            };
        }
    }
}

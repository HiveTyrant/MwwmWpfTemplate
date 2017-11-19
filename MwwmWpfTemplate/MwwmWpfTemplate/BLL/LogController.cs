using System;
using System.Linq;
using System.Text;
using NLog;

namespace MwwmWpfTemplate.BLL
{
    /// <summary> Class that shields application from logger implementation details </summary>
    public static class LogController
    {
        #region Sub-class defines

        /// <summary>
        /// Logger levels 
        /// </summary>
        public enum Level
        {
            Off,
            Trace,
            Debug,
            Info,
            Warn,
            Error,
            Fatal
        }

        /// <inheritdoc />
        /// <summary> Event argument used for LogMessage eventhandlers </summary>
        public class LogMessageEventArgs : EventArgs
        {
            public string LoggerName { get; set; }
            public Level Level { get; set; }
            public Func<string> Message { get; set; }
            public Exception Error { get; set; }
            public DateTime Created { get; } = DateTime.Now;
        }

        public interface ILogEnabled
        {
            /// <summary> Event triggered when the implementing object logs a message with an optional exception </summary>
            event EventHandler<LogMessageEventArgs> LogMessage;
        }

        public delegate void LogMessageDelegate(Func<string> logMessage, Level logLevel);
        public delegate void LogMessageDelegateWithLoggerName(Func<string> loggerName, Func<string> logMessage, Level logLevel);
        public delegate void LogMessageDelegateWithException(Func<string> loggerName, Func<string> logMessage, Level logLevel, Exception exception = null);
        public delegate void LogMessageDelegateWithLoggerEvent(LogMessageEventArgs args);

        #endregion

        #region Private fields and properties

        private static readonly char[] CrLf = { '\r', '\n' };

        private static Logger _genericLogger;
        private static Logger GenericLogger
        {
            get
            {
                if (null == _genericLogger)
                {
                    _genericLogger = LogManager.GetCurrentClassLogger();
                    _genericLogger.IsEnabled(LogLevel.Trace);
                }
                return _genericLogger;
            }
        }

        #endregion

        #region Construction and initialization

        /// <summary>
        /// Enabled default true
        /// </summary>
        static LogController()
        {
            Enabled = true;
        }

        #endregion

        #region Public members

        /// <summary> Event that triggers when a message is logged.  May be used for onscreen event log etc </summary>
        public static EventHandler<LogMessageEventArgs> OnLogMessage;

        /// <summary> Loggername that can be set as default, application scope wide default loggername </summary>
        public static string DefaultLoggerName = string.Empty;

        /// <summary> Flag specifing if logging is currently enabled </summary>
        public static bool Enabled { get; set; }

        /// <summary>
        /// Logs a message at the specified loglevel. Log may include type-, message- and stracktrace data from optional error exception
        /// </summary>
        /// <param name="logLevel">LogLevel of the attached logMessage</param>
        /// <param name="logMessage">Function that resolves message to log in case logging at specified logLevel is enabled</param>
        /// <param name="ex">Optional error exception to include in log</param>
        public static void LogMessage(Level logLevel, Func<string> logMessage, Exception ex = null)
        {
            LogMessage("", logLevel, logMessage, ex);
        }

        /// <summary> Logs a message as specified by content in the LogMessageEventArgs argument  </summary>
        /// <param name="args">Specifies logconfiguration and -message content</param>
        public static void LogMessage(LogMessageEventArgs args)
        {
            if (null != args)
                LogMessage(args.LoggerName, args.Level, args.Message, args.Error);
        }

        /// <summary>
        /// Logs a message at the specified loglevel for specified loggername (logconfiguration). 
        /// Log may include type-, message- and stracktrace data from optional error exception
        /// </summary>
        /// <param name="loggerName"></param>
        /// <param name="logLevel"></param>
        /// <param name="logMessage"></param>
        /// <param name="ex"></param>
        public static void LogMessage(string loggerName, Level logLevel, Func<string> logMessage, Exception ex)
        {
            if (!Enabled) return;
            //Is try/catch expensive???
            // http://stackoverflow.com/questions/1308432/do-try-catch-blocks-hurt-performance-when-exceptions-are-not-thrown?rq=1
            try
            {
                var msg = logMessage();
                if (string.IsNullOrEmpty(msg)) return;

                if (string.IsNullOrWhiteSpace(loggerName)) loggerName = DefaultLoggerName;

                // Trigger eventhandler for any subscribers
                OnLogMessage?.Invoke(null, new LogMessageEventArgs { LoggerName = loggerName, Level = logLevel, Message = () => msg, Error = ex });

                // Build logmessage string, including any exception messages (recursively)
                var message = new StringBuilder();
                message.AppendLine(msg);

                if (null != ex)
                {
                    message.AppendLine("Exception: ").AppendLine(ex.GetBaseException().Message);
                    message.AppendLine("Base exception message: ").AppendLine(ex.GetBaseException().Message);
                    message.AppendLine("Stacktrace:");
                    message.AppendLine(ex.StackTrace);
                    message.AppendLine("Inner exception messages:");
                    while (null != ex)
                    {
                        message.Append("\t").AppendLine(ex.Message);
                        ex = ex.InnerException;
                    }
                }

                LogMessageInternal(loggerName, logLevel, message.ToString().TrimEnd(CrLf));
            }
            catch (Exception getLogMessageException)
            {
                UseGenericLogger(logLevel.ToNlogLevel(), $"Failed to get logmassage {getLogMessageException.StackTrace}", getLogMessageException);
            }
        }

        public static bool IsLoggerNameInUse(string loggerName)
        {
            if (string.IsNullOrEmpty(loggerName) || string.Empty == loggerName.Trim()) loggerName = DefaultLoggerName;

            var loggingRule = LogManager.Configuration.LoggingRules.FirstOrDefault(x => x.LoggerNamePattern == loggerName);
            return loggingRule != null;
        }

        #endregion

        #region Private methods - contains the only code, that will use members of the external logger (NLog in this case)

        /// <summary> Method that will use the external logger, NLog in this case, to write the logmessage at the specified loglevel </summary>
        /// <param name="loggerName">Name of the log configuration used</param>
        /// <param name="logLevel">LogLevel of the attached logMessage</param>
        /// <param name="logMessage">String containing message to be logged</param>
        private static void LogMessageInternal(string loggerName, Level logLevel, string logMessage)
        {
            if (string.IsNullOrWhiteSpace(loggerName)) // Use generic logger if no loggerName is supplied and DefaultLoggerName is undefined 
                UseGenericLogger(logLevel.ToNlogLevel(), logMessage);
            else
            {
                try
                {
                    var logger = LogManager.GetLogger(loggerName);
                    if (logger.IsEnabled(logLevel.ToNlogLevel())) logger.Log(logLevel.ToNlogLevel(), logMessage);
                }
                catch (Exception ex)
                {
                    // If GetLogger somehow failes, use generic logger to log message
                    UseGenericLogger(logLevel.ToNlogLevel(), "Logger failed trying to log this message: " + logMessage + "\r\nLogger error: " + ex.Message);
                }
            }
        }

        /// <summary> Logs a message to generic log file (the log configuration named '*') </summary>
        /// <param name="logLevel">LogLevel of the attached logMessage</param>
        /// <param name="logMessage">String containing message to be logged</param>
        /// <param name="error">Optional exception to be logged</param>
        private static void UseGenericLogger(LogLevel logLevel, string logMessage, Exception error = null)
        {
            if (!Enabled) return;
            if (null != error)
                GenericLogger.Log(logLevel, error, logMessage);
            else
                GenericLogger.Log(logLevel, logMessage);
        }

        /// <summary> Converts between public loglevel enumerable and level used by external logger </summary>
        /// <param name="level"></param>
        /// <returns></returns>
        private static LogLevel ToNlogLevel(this Level level)
        {
            if (level == Level.Trace) return LogLevel.Trace;
            if (level == Level.Debug) return LogLevel.Debug;
            if (level == Level.Info) return LogLevel.Info;
            if (level == Level.Warn) return LogLevel.Warn;
            if (level == Level.Error) return LogLevel.Error;
            if (level == Level.Fatal) return LogLevel.Fatal;
            return LogLevel.Off;
        }

        #endregion
    }

}

using System.Collections.Generic;
using System.Diagnostics;
using log4net.Core;
using Zeta.Bot.Settings;
using Zeta.Common;

namespace QuestTools
{
    public static class Logger
    {
        private static readonly log4net.ILog Logging = Zeta.Common.Logger.GetLoggerInstanceForType();

        private static string _lastLogMessage = "";

        /// <summary>
        /// Log Normal
        /// </summary>
        /// <param name="message"></param>
        /// <param name="args"></param>
        public static void Log(string message, params object[] args)
        {
            var msg = ClassTag + string.Format(message, args);

            if (_lastLogMessage == msg)
                return;

            _lastLogMessage = msg;
            Logging.Info(msg);
        }

        /// <summary>
        /// Log Normal
        /// </summary>
        /// <param name="message"></param>
        public static void Log(string message)
        {
            var msg = ClassTag + message;

            if (_lastLogMessage == msg)
                return;

            _lastLogMessage = msg;
            Logging.Info(msg);
        }

        /// <summary>
        /// Log without the Plugin Identifier
        /// </summary>
        public static void Raw(string message)
        {
            Logging.Info(message);
        }

        /// <summary>
        /// Log without the Plugin Identifier
        /// </summary>
        public static void Raw(string message, params object[] args)
        {
            Logging.Info(string.Format(message, args));
        }

        /// <summary>
        /// Log Message in Yellow
        /// </summary>
        /// <param name="message"></param>
        public static void Warn(string message)
        {
            var msg = ClassTag + message;

            if (_lastLogMessage == msg)
                return;

            _lastLogMessage = msg;
            Logging.Warn(msg);
        }

        /// <summary>
        /// Log Message in Yellow
        /// </summary>
        /// <param name="message"></param>
        /// <param name="args"></param>
        public static void Warn(string message, params object[] args)
        {
            var msg = ClassTag + string.Format(message, args);

            if (_lastLogMessage == msg)
                return;

            _lastLogMessage = msg;
            Logging.Warn(msg);
        }

        /// <summary>
        /// Log Error in red text
        /// </summary>
        /// <param name="message"></param>
        public static void Error(string message)
        {
            var msg = ClassTag + message;

            if (_lastLogMessage == msg)
                return;

            _lastLogMessage = msg;
            Logging.Error(msg);
        }

        /// <summary>
        /// Log Error in red text
        /// </summary>
        /// <param name="message"></param>
        public static void Error(string message, params object[] args)
        {
            var msg = ClassTag + string.Format(message, args);

            if (_lastLogMessage == msg)
                return;

            _lastLogMessage = msg;
            Logging.Error(msg);
        }

        /// <summary>
        /// Log Verbose
        /// </summary>
        /// <param name="message"></param>
        /// <param name="args"></param>
        public static void Verbose(string message, params object[] args)
        {
            if (!QuestToolsSettings.Instance.DebugEnabled)
                return;

            var msg = ClassTag + string.Format(message, args);

            if (_lastLogMessage == msg)
                return;

            _lastLogMessage = msg;
            Logging.Debug(msg);
        }

        /// <summary>
        /// Log Verbose
        /// </summary>
        /// <param name="message"></param>
        public static void Verbose(string message)
        {
            if (!QuestToolsSettings.Instance.DebugEnabled)
                return;

            var msg = ClassTag + message;

            if (_lastLogMessage == msg)
                return;

            _lastLogMessage = msg;
            Logging.Debug(msg);
        }

        /// <summary>
        /// Log Debug
        /// </summary>
        /// <param name="message"></param>
        /// <param name="args"></param>
        public static void Debug(string message, params object[] args)
        {
            if (!QuestToolsSettings.Instance.DebugEnabled)
                return;

            var msg = ClassTag + string.Format(message, args);

            if (_lastLogMessage == msg)
                return;

            _lastLogMessage = msg;
            Logging.Debug(msg);
        }

        /// <summary>
        /// Log Debug
        /// </summary>
        /// <param name="message"></param>
        public static void Debug(string message)
        {
            if (!QuestToolsSettings.Instance.DebugEnabled)
                return;

            var msg = ClassTag + message;

            if (_lastLogMessage == msg)
                return;

            _lastLogMessage = msg;
            Logging.Debug(msg);
        }

        private static string ClassTag
        {
            get
            {
                var frame = new StackFrame(2);
                var method = frame.GetMethod();
                var type = method.DeclaringType;                

                if (type == null)
                    return "[QuestTools] ";

                if (type.Namespace != null && type.Namespace.ToLowerInvariant().Contains("questtools"))
                    return "[QuestTools][" + type.Name + "] ";

                if (type.Namespace == type.Name || type.Name.ToLowerInvariant().Contains("displayclass"))
                    return "[" + type.Namespace + "] ";

                return "[" + type.Namespace + "][" + type.Name + "] ";

            }
        }

    }
}

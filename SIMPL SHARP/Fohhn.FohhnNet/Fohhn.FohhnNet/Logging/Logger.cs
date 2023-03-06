using System;
using Crestron.SimplSharp;

namespace Fohhn.FohhnNet.Logging
{
    internal class Logger
    {
        public LogLevel Level { get; set; }
        private readonly FohhnNetLoggingDelegate _writer;

        public Logger(FohhnNetLoggingDelegate writer)
        {
            _writer = writer ?? UseDefaultWriter();
            Level = LogLevel.Error;
        }

        public void Debug(string message)
        {
            if (Level <= LogLevel.Debug)
                _writer(LogLevel.Debug, message);
        }
        public void Info(string message)
        {
            if (Level <= LogLevel.Info)
                _writer(LogLevel.Info, message);
        }
        public void Error(string message)
        {
            if (Level <= LogLevel.Error)
                _writer(LogLevel.Error, message);
        }

        private FohhnNetLoggingDelegate UseDefaultWriter()
        {
            switch (CrestronEnvironment.DevicePlatform)
            {
                case eDevicePlatform.Appliance:
                    return WriteToCrestronConsole;
                case eDevicePlatform.Server:
                    return WriteToErrorLog;
                default:
                    return (level, message) => { };
            }
        }

        private void WriteToErrorLog(LogLevel level, string message)
        {
            switch (level)
            {
                case LogLevel.Error:
                    ErrorLog.Error("FohhnNet - {0}", message);
                    break;
                case LogLevel.Info:
                    ErrorLog.Info("FohhnNet - {0}", message);
                    break;
                case LogLevel.Debug:
                    ErrorLog.Ok("FohhnNet - {0}", message);
                    break;
                default:
                    ErrorLog.Info("FohhnNet - {0}", message);
                    break;
            }
        }
        private void WriteToCrestronConsole(LogLevel level, string message)
        {
            CrestronConsole.PrintLine(String.Format("{0:HH:mm:ss} [{1}] - FohhnNet - {2}", DateTime.Now, level, message));
            if (level >= LogLevel.Error)
                ErrorLog.Error("FohhnNet - " + message);
        }
    }
}
using log4net;
using log4net.Config;
using System;
using System.IO;
using System.Reflection;

namespace Fpi.Util
{
    public static class LogHelper
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(LogHelper));

        static LogHelper()
        {
            log4net.Config.XmlConfigurator.Configure();
        }

        // 日志记录方法  
        public static void Info(string message)
        {
            log.Info(message);
        }

        public static void Debug(string message)
        {
            log.Debug(message);
        }

        public static void Warn(string message)
        {
            log.Warn(message);
        }

        public static void Error(string message)
        {
            log.Error(message);
        }

        public static void Error(Exception exception)
        {
            log.Error(exception.Message);
        }

        public static void Error(string message, Exception exception)
        {
            log.Error(message, exception);
        }

        public static void Fatal(Exception exception)
        {
            log.Fatal(exception.Message);
        }
        public static void Fatal(string message)
        {
            log.Fatal(message);
        }

        public static void Fatal(string message, Exception exception)
        {
            log.Fatal(message, exception);
        }
    }
}

 
  


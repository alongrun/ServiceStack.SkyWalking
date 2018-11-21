using System;
using System.Diagnostics;
using SkyWalking.Logging;

namespace ServiceStack.SkyWalking
{
    internal class DebugLoggerFactoryAdapter : ILoggerFactory
    {
        public DebugLoggerFactoryAdapter()
        {
            Debug.Listeners.Add(new TextWriterTraceListener(Console.Out));
        }

        public ILogger CreateLogger(Type type)
        {
            return new DebugLoggerAdapter(type);
        }
    }

    internal class DebugLoggerAdapter : ILogger
    {
        private readonly Type type;

        public DebugLoggerAdapter(Type type)
        {
            this.type = type;
        }

        public void Debug(string message)
        {
            WriteLine("debug", message);
        }

        public void Info(string message)
        {
            WriteLine("info", message);
        }

        public void Warning(string message)
        {
            WriteLine("warn", message);
        }

        public void Error(string message, Exception exception)
        {
            WriteLine("error", message + Environment.NewLine + exception);
        }

        public void Trace(string message)
        {
            WriteLine("trace", message);
        }

        private void WriteLine(string level, string message)
        {
            Console.WriteLine($"{DateTime.Now} : [{level}] [{type.Name}] {message}");
            //  System.Diagnostics.Debug.WriteLine($"{DateTime.Now} : [{level}] [{type.Name}] {message}");
        }
    }

}
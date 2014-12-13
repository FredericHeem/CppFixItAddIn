using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Diagnostics;
namespace CppFixItAddIn
{
    class Tracer
    {
        public string LogFilePrefix = "CppFixIt";
        private StreamWriter _streamWriter;
        private StreamWriter StreamWriter {
            get {
                if (_streamWriter == null) {
                    DeleteLog();
                    _streamWriter = new StreamWriter(GetLogFileAbsolute());
                    _streamWriter.AutoFlush = true;
                }
                return _streamWriter;
            }
        }

        static Tracer _tracer = new Tracer();
        static public Tracer Instance
        {
            get {
                return _tracer;
            }
        }

        private Tracer()
        { 
        }

        public TraceSource CreateTraceSource(string sourceName)
        {
            TraceSource ts = new TraceSource(sourceName);
            ts.Switch = new SourceSwitch("sourceSwitch");
            ts.Switch.Level = SourceLevels.All;
            ts.Listeners.Remove("Default");

            try
            {
                TextWriterTraceListener textListener = new TextWriterTraceListener(StreamWriter);
                textListener.Filter = new EventTypeFilter(SourceLevels.Information);
                ts.Listeners.Add(textListener);
            }
            catch (Exception)
            {
                ConsoleTraceListener consoleListener = new ConsoleTraceListener();
                consoleListener.Filter = new EventTypeFilter(SourceLevels.Information);
                ts.Listeners.Add(consoleListener);
            }

            string logFileNameAbsolutePath = GetLogFileAbsolute();
            ts.TraceInformation("InitTraceSource: log file " + logFileNameAbsolutePath);
            return ts;
        }

        private string GetLogFileBaseName()
        {
            return LogFilePrefix + "Log.txt";
        }

        public string GetLogFileAbsolute()
        {
            return Path.Combine(System.IO.Path.GetTempPath(), GetLogFileBaseName());
        }

        public void DeleteLog()
        {
            try
            {
                File.Delete(GetLogFileAbsolute());
            }
            catch (Exception)
            {
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace POS_DAL.Loggers
{
    public static class clsLog
    {
        // ── Configuration ─────────────────────────────────────────────────────
        private static readonly string _logDirectory =
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logs");

        // One file per day: Logs/2025-07-21.log
        private static string LogFilePath =>
            Path.Combine(_logDirectory, $"{DateTime.UtcNow:yyyy-MM-dd}.log");

        private static readonly object _lock = new object();

        // ── Core writer ───────────────────────────────────────────────────────
        private static void WriteToFile(string level, string entry)
        {
            try
            {
                lock (_lock)
                {
                    Directory.CreateDirectory(_logDirectory); // no-op if exists
                    File.AppendAllText(LogFilePath, entry + Environment.NewLine, Encoding.UTF8);
                }
            }
            catch
            {
                // Logger must never crash the app — swallow silently
            }
        }

        // ── Builders ──────────────────────────────────────────────────────────
        private static string _BuildErrorLog(string className, string methodName, Exception ex)
        {
            return new StringBuilder()
                .Append($"[{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC] ")
                .Append("[ERROR] ")
                .Append($"[{className}.{methodName}] ")
                .Append($"{ex.GetType().Name}: {ex.Message}")
                .AppendLine()
                .Append($"    StackTrace: {ex.StackTrace}")
                .ToString();
        }

        private static string _BuildInfoLog(string className, string methodName, string message)
        {
            return new StringBuilder()
                .Append($"[{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC] ")
                .Append("[INFO]  ")
                .Append($"[{className}.{methodName}] ")
                .Append(message)
                .ToString();
        }

        // ── Public API ────────────────────────────────────────────────────────
        public static void LogError(string className, string methodName, Exception ex)
        {
            string entry = _BuildErrorLog(className, methodName, ex);
            WriteToFile("ERROR", entry);

            #if DEBUG
                Console.Error.WriteLine(entry);
            #endif
        }

        public static void LogInfo(string className, string methodName, string message)
        {
            string entry = _BuildInfoLog(className, methodName, message);
            WriteToFile("INFO", entry);
        }
    }
}

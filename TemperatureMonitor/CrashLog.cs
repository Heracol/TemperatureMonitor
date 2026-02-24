using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TemperatureMonitor
{
    public static class CrashLog
    {
        public static void SetupCrashLogging()
        {
            Directory.CreateDirectory("logs");

            AppDomain.CurrentDomain.UnhandledException += (sender, e) =>
            {
                LogCrash("UnhandledException", e.ExceptionObject as Exception);
            };

            TaskScheduler.UnobservedTaskException += (sender, e) =>
            {
                LogCrash("UnobservedTaskException", e.Exception);
                e.SetObserved();
            };
        }

        static void LogCrash(string type, Exception? ex)
        {
            try
            {
                string file = Path.Combine(
                    "logs",
                    $"crash_{DateTime.Now:yyyyMMdd_HHmmss}.txt"
                );

                File.WriteAllText(file,
                    $"[{DateTime.Now}]\n" +
                    $"Type: {type}\n" +
                    $"Message: {ex?.Message}\n" +
                    $"StackTrace:\n{ex?.StackTrace}\n"
                );
            }
            catch
            {
                // Ignore logging errors
            }
        }
    }
}

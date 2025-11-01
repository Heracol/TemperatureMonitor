using Microsoft.Win32;
using Microsoft.Win32.TaskScheduler;
using System.Diagnostics;

namespace TemperatureMonitor
{
    // Adds/removes program from Windows Task Scheduler
    // Windows Task Scheduler - simple and allows program to be run as admin
    public static class AutoStart
    {
        private const string AppName = "TemperatureMonitor";

        public static void SetAutoStart(bool enable)
        {
            string exePath = Environment.ProcessPath!;

            if (enable)
            {
                TaskDefinition td = TaskService.Instance.NewTask();
                td.RegistrationInfo.Description = "Automatically starts TemperatureMonitor at user logon";
                td.Principal.RunLevel = TaskRunLevel.Highest;

                td.Triggers.Add(new LogonTrigger());

                td.Actions.Add(new ExecAction(exePath, null, null));

                TaskService.Instance.RootFolder.RegisterTaskDefinition(AppName, td);
            } else
            {
                if (TaskService.Instance.GetTask(AppName) != null)
                {
                    TaskService.Instance.RootFolder.DeleteTask(AppName, false);
                }
            }
        }

        public static bool IsAutoStartEnabled()
        {
            return TaskService.Instance.GetTask(AppName) != null;
        }
    }
}

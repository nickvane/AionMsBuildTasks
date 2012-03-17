using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using Microsoft.Build.Utilities;

namespace Aion.MsBuildTasks
{
    public class AionProcessKiller
    {
        private readonly TaskLoggingHelper _log;
        private readonly string _processNames;

        public AionProcessKiller(TaskLoggingHelper log, string processNames)
        {
            _log = log;
            _processNames = processNames;
        }

        public void Kill()
        {
            var processes = _processNames.Split(';');
            foreach (var systemProcess in Process.GetProcesses())
            {
                try
                {
                    var fileName = Path.GetFileName(systemProcess.MainModule.FileName);
                    foreach (var process in processes)
                    {
                        if (process.ToLower() == fileName.ToLower())
                        {
                            try
                            {
                                _log.LogMessage(string.Format("Killing process {0}", systemProcess.ProcessName));
                                systemProcess.Kill();
                                systemProcess.WaitForExit(60 * 1000);
                                _log.LogWarning(string.Format("Process with name '{0}' was killed", systemProcess.ProcessName));
                            }
                            catch (Win32Exception winException)
                            {
                                _log.LogError(string.Format("Process with name '{0}' was terminating or can't be terminated. ErrorMessage: {1}", systemProcess.ProcessName, winException.Message));
                            }
                            catch (InvalidOperationException)
                            {
                                _log.LogWarning(string.Format("Process with name '{0}' has already exited", systemProcess.ProcessName));
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    _log.LogWarning(string.Format("Killing Processes failed with message {0}", ex.Message));
                }
            }
        }
    }
}
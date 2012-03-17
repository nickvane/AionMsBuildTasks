using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Microsoft.Build.Utilities;

namespace Aion.MsBuildTasks
{
    public class AionBuilder
    {
        private readonly string _processName;
        private readonly IList<string> _errors;
        private readonly TaskLoggingHelper _log;

        public AionBuilder(string processName, IList<string> errors, TaskLoggingHelper log)
        {
            _processName = processName;
            if (errors == null || errors.Count == 0)
            {
                _errors = new List<string>
                                   {
                                       "??",
                                       "failed",
                                       "Failed",
                                       "Invalid Objects Detected for",
                                       "error C2059",
                                       "was not found on the Library Path",
                                       "is already included in this application"
                                   };
            }
            else
            {
                _errors = errors;
            }
            _log = log;
        }

        public void Build(string appFilePath, bool shouldRestoreCodeFromApp, int buildTimeout)
        {
            if (shouldRestoreCodeFromApp)
            {
                using (var buildProcess = Process.Start(GetProcessStartInfo(Path.GetFullPath(appFilePath), true)))
                {
                    IsBuildSuccesful = GetFeedbackFromProcess(buildProcess, buildTimeout);
                    if (!IsBuildSuccesful)
                        return;
                }
            }
            using (var buildProcess = Process.Start(GetProcessStartInfo(Path.GetFullPath(appFilePath), false)))
            {
                IsBuildSuccesful = GetFeedbackFromProcess(buildProcess, buildTimeout);
            }
        }

        public bool IsBuildSuccesful { get; set; }

        private ProcessStartInfo GetProcessStartInfo(string appFilePath, bool shouldRestoreCodeFromApp)
        {
            var processStartInfo = new ProcessStartInfo(_processName)
            {
                CreateNoWindow = true,
                UseShellExecute = false,
                WorkingDirectory = Path.GetDirectoryName(appFilePath),
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                WindowStyle = ProcessWindowStyle.Hidden,
                Arguments = string.Format(shouldRestoreCodeFromApp ? @"""{0}"" -res" : @"""{0}""", appFilePath)
            };
            return processStartInfo;
        }

        private bool GetFeedbackFromProcess(Process buildProcess, int buildTimeout)
        {
            if (buildProcess != null)
            {
                var standardOutput = buildProcess.StandardOutput.ReadToEnd();
                var standardError = buildProcess.StandardError.ReadToEnd();
                buildProcess.WaitForExit(buildTimeout * 1000);
                return LogOutput(standardOutput, standardError);
            }
            _log.LogError("Process '{0}' is already running.", _processName);
            return false;
        }

        private bool LogOutput(string standardOutput, string standardError)
        {
            foreach (var line in standardOutput.Split('\n', '\r'))
            {
                if (!string.IsNullOrEmpty(line.Trim()))
                {
                    _log.LogMessage(line, null);
                }
            }
            foreach (var error in _errors.Where(buildError => standardOutput.IndexOf(buildError, StringComparison.Ordinal) > -1))
            {
                _log.LogError(error);
                return false;
            }

            if (!string.IsNullOrEmpty(standardError))
            {
                _log.LogError(standardError);
                return false;
            }
            return true;
        }
    }
}

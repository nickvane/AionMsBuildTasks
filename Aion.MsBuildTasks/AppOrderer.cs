using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Build.Utilities;

namespace Aion.MsBuildTasks
{
    public class AppOrderer
    {
        private readonly List<string> _orderedAppList = new List<string>();
        private TaskLoggingHelper _log;
        private string _uniqueSourceDirectoryPath;

        public IEnumerable<string> GetAppBuildOrder(string sourceDirectory, string startAppPath, string uniqueSourceDirectoryPath, TaskLoggingHelper log)
        {
            _log = log;
            _uniqueSourceDirectoryPath = uniqueSourceDirectoryPath;
            var sourceDirectoryPath = sourceDirectory.Trim('\'', '"');
            var appList = GetAppListWithReferences(sourceDirectoryPath);

            var appPath = startAppPath.Trim('\'', '"');
            var startApps = appPath.Split('|').ToList();
            foreach (var app in startApps)
            {
                if (!string.IsNullOrEmpty(app))
                {
                    _log.LogMessage("Application path: {0}", app);
                    var startApp = appList[Path.GetFullPath(app.ToUpper().Replace(sourceDirectoryPath.ToUpper(), _uniqueSourceDirectoryPath)).ToUpper()];
                    if (startApp == null)
                    {
                        log.LogError("Application {0} could not be found.", app);
                    }
                    else
                    {
                        _orderedAppList.Add(Path.GetFullPath(app.ToUpper().Replace(sourceDirectoryPath.ToUpper(), _uniqueSourceDirectoryPath)).ToUpper());
                        LoopReferences(startApp, appList);
                    }
                }
            }

            _orderedAppList.ForEach(a => _log.LogMessage(a));

            return _orderedAppList;
        }

        private Dictionary<string, App> GetAppListWithReferences(string sourceDirectory)
        {
            var appList = new Dictionary<string, App>();
            var sourceDir = new DirectoryInfo(sourceDirectory);
            var invalidIncludePaths = new List<string>();
            foreach (FileInfo file in sourceDir.GetFiles("*.app", SearchOption.AllDirectories))
            {
                if (file.Extension.ToUpper() == ".APP")
                {
                    var referencedApps = new List<string>();
                    var includePaths = new List<string>();
                    using (var stream = file.OpenText())
                    {
                        bool hasFoundIncludes = false;
                        string line;
                        bool hasFoundEnd = false;
                        bool hasFoundIncludePaths = false;

                        includePaths.Add(file.DirectoryName.ToUpper());

                        do
                        {
                            line = stream.ReadLine();
                            if (line == null)
                            {
                                break;
                            }
                            var tempIncludePaths = new List<string>();

                            if (hasFoundIncludePaths & !line.StartsWith("#"))
                            {
                                tempIncludePaths.AddRange(line.Replace(Environment.NewLine, "").ToUpper().Split(';'));
                                includePaths.AddRange(from s in tempIncludePaths where !string.IsNullOrEmpty(s) select s.TrimEnd(new[] { '\\' }).ToUpper());
                            }
                            if (hasFoundIncludePaths & line.StartsWith("#"))
                                hasFoundIncludePaths = false;
                            if (hasFoundIncludes & !line.StartsWith("#"))
                            {
                                string item = line.ToUpper().Replace(Environment.NewLine, "");
                                if (item.EndsWith("-C"))
                                    item = item.Replace("-C", "");
                                referencedApps.Add(item);
                            }
                            if (hasFoundIncludes & line.StartsWith("#"))
                                hasFoundEnd = true;
                            if (line.StartsWith("#LibIncPath"))
                                hasFoundIncludePaths = true;
                            if (line.StartsWith("#Includes"))
                                hasFoundIncludes = true;
                        } while (!hasFoundEnd);
                    }
                    var filePath = file.FullName.ToUpper().Replace(sourceDir.FullName.ToUpper(), _uniqueSourceDirectoryPath);
                    var directoryPath = file.DirectoryName.ToUpper().Replace(sourceDir.FullName.ToUpper(), _uniqueSourceDirectoryPath);
                    appList.Add(filePath, new App { Name = file.Name.ToUpper().Replace(".APP", ""), FilePath = filePath, DirectoryPath = directoryPath, References = referencedApps, IncludePaths = includePaths });
                }
            }
            invalidIncludePaths.ForEach(p => _log.LogWarning(p));
            return appList;
        }

        private void LoopReferences(App app, Dictionary<string, App> appList)
        {
            foreach (string reference in app.References)
            {
                App referenceApp = (appList.Where(a => a.Value.Name == reference.ToUpper()
                                                           & app.IncludePaths.Contains(a.Value.DirectoryPath)).Select(a => a.Value)).FirstOrDefault();

                if (referenceApp != null)
                {
                    int insertIndex = GetInsertIndex(app.FilePath.ToUpper());
                    if (!_orderedAppList.Contains(referenceApp.FilePath))
                    {
                        _orderedAppList.Insert(insertIndex, referenceApp.FilePath);
                    }
                    else
                    {
                        int actualIndex = _orderedAppList.IndexOf(referenceApp.FilePath);
                        if (insertIndex < actualIndex)
                        {
                            _orderedAppList.Remove(referenceApp.FilePath);
                            _orderedAppList.Insert(insertIndex, referenceApp.FilePath);
                        }
                    }
                    LoopReferences(referenceApp, appList);
                }
            }
        }

        private int GetInsertIndex(string parent)
        {
            return _orderedAppList.Contains(parent) ? _orderedAppList.IndexOf(parent) : _orderedAppList.Count;
        }
    }
}

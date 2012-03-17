using System.Collections.Generic;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace Aion.MsBuildTasks
{
    public class AppBuildOrderTask : Task
    {
        public override bool Execute()
        {
            if (string.IsNullOrEmpty(SourceDirectory))
            {
                Log.LogError("Argument SourceDirectory is missing.");
                return false;
            }
            if (string.IsNullOrEmpty(StartAppPath))
            {
                Log.LogError("Argument StartAppPath is missing.");
                return false;
            }
            if (string.IsNullOrEmpty(UniqueSourceDirectoryPath))
            {
                Log.LogError("Argument UniqueSourceDirectoryPath is missing.");
                return false;
            }
            var orderer = new AppOrderer();
            var list = orderer.GetAppBuildOrder(SourceDirectory, StartAppPath, UniqueSourceDirectoryPath, Log);
            var returnList = new List<ITaskItem>();
            foreach (string app in list)
            {
                ITaskItem item = new TaskItem(app);
                item.SetMetadata("AppPath", app);
                returnList.Add(item);
            }
            AppList = returnList.ToArray();
            return true;
        }

        [Required]
        public string StartAppPath { get; set; }

        [Required]
        public string SourceDirectory { get; set; }

        [Required]
        public string UniqueSourceDirectoryPath { get; set; }

        [Output]
        public ITaskItem[] AppList { get; set; }
    }
}

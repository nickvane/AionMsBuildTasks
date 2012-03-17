using System.Collections.Generic;

namespace Aion.MsBuildTasks
{
    public class App
    {
        public string Name { get; set; }
        public string FilePath { get; set; }
        public string DirectoryPath { get; set; }
        public List<string> References { get; set; }
        public List<string> IncludePaths { get; set; }
    }
}

using System.Collections.Generic;
using System.IO;

namespace YZXYResourceTools.Services
{
    public class ResourceService
    {
        public List<string> GetResourceFiles(string rootPath)
        {
            var files = new List<string>();

            if (string.IsNullOrEmpty(rootPath) || !Directory.Exists(rootPath))
            {
                return files;
            }

            foreach (var file in Directory.GetFiles(rootPath, "*.*", SearchOption.AllDirectories))
            {
                files.Add(file);
            }

            return files;
        }

        public bool PackageResources(string inputPath, string outputFile)
        {
            if (string.IsNullOrEmpty(inputPath) || !Directory.Exists(inputPath))
            {
                return false;
            }

            if (string.IsNullOrEmpty(outputFile))
            {
                return false;
            }

            Directory.CreateDirectory(Path.GetDirectoryName(outputFile) ?? ".");
            File.WriteAllText(outputFile, "YZXY resource package\r\n");
            return true;
        }
    }
}

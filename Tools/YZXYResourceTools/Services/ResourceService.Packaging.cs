using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using YZXYResourceTools.Models;

namespace YZXYResourceTools.Services
{
    public partial class ResourceService
    {
        public void PackageFiles(string rootPath, IEnumerable<string> files, string outputFile)
        {
            if (string.IsNullOrWhiteSpace(rootPath) || !Directory.Exists(rootPath)) throw new DirectoryNotFoundException("源目录不存在。");
            if (string.IsNullOrWhiteSpace(outputFile)) throw new ArgumentException("必须指定输出文件。", "outputFile");

            var root = Path.GetFullPath(rootPath).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar) + Path.DirectorySeparatorChar;
            var output = Path.GetFullPath(outputFile);
            if (output.StartsWith(root, StringComparison.OrdinalIgnoreCase)) throw new InvalidOperationException("输出文件不能在源目录内部。");
            var parent = Path.GetDirectoryName(output);
            if (!string.IsNullOrEmpty(parent)) Directory.CreateDirectory(parent);
            if (File.Exists(output)) File.Delete(output);

            using (var archive = ZipFile.Open(output, ZipArchiveMode.Create))
            {
                foreach (var file in files ?? Enumerable.Empty<string>())
                {
                    var fullPath = Path.GetFullPath(file);
                    if (!fullPath.StartsWith(root, StringComparison.OrdinalIgnoreCase) || !File.Exists(fullPath)) continue;
                    var relative = Uri.UnescapeDataString(new Uri(root).MakeRelativeUri(new Uri(fullPath)).ToString());
                    archive.CreateEntryFromFile(fullPath, relative, CompressionLevel.Optimal);
                }
            }
        }
    }
}

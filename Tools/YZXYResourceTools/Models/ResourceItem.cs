using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using YZXYResourceTools.Models;

namespace YZXYResourceTools.Services
{
    public class ResourceService
    {
        public List<ResourceItem> GetResourceItems(string rootPath)
        {
            var result = new List<ResourceItem>();
            if (string.IsNullOrEmpty(rootPath) || !Directory.Exists(rootPath))
            {
                return result;
            }

            foreach (var file in Directory.GetFiles(rootPath, "*.*", SearchOption.AllDirectories))
            {
                var info = new FileInfo(file);
                result.Add(new ResourceItem(
                    Path.GetFileName(file),
                    GetFileType(file),
                    info.Length,
                    file));
            }

            return result.OrderBy(x => x.Name).ToList();
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

            var directory = Path.GetDirectoryName(outputFile);
            if (!string.IsNullOrEmpty(directory))
            {
                Directory.CreateDirectory(directory);
            }

            var files = Directory.GetFiles(inputPath, "*.*", SearchOption.AllDirectories);
            using (var archive = ZipFile.Open(outputFile, ZipArchiveMode.Create))
            {
                foreach (var file in files)
                {
                    var relativePath = GetRelativePath(inputPath, file);
                    archive.CreateEntryFromFile(file, relativePath);
                }
            }

            return File.Exists(outputFile);
        }

        private static string GetFileType(string filePath)
        {
            var extension = Path.GetExtension(filePath).ToLower();
            switch (extension)
            {
                case ".lua": return "Lua";
                case ".json": return "JSON";
                case ".xml": return "XML";
                case ".txt": return "Text";
                case ".ini": return "INI";
                case ".png": return "PNG";
                case ".jpg":
                case ".jpeg": return "JPG";
                case ".bmp": return "BMP";
                case ".gif": return "GIF";
                case ".map": return "Map";
                case ".csv": return "CSV";
                default: return "File";
            }
        }

        private static string GetRelativePath(string rootPath, string filePath)
        {
            var rootUri = new Uri(rootPath.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar) + Path.DirectorySeparatorChar);
            var fileUri = new Uri(filePath);
            var relative = rootUri.MakeRelativeUri(fileUri).ToString();
            return relative.Replace('/', Path.DirectorySeparatorChar);
        }
    }
}

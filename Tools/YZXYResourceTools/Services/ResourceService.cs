using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Windows.Forms;
using YZXYResourceTools.Models;

namespace YZXYResourceTools.Services
{
    public class ResourceService
    {
        public List<ResourceItem> GetResourceItems(string rootPath)
        {
            var results = new List<ResourceItem>();
            if (string.IsNullOrEmpty(rootPath) || !Directory.Exists(rootPath))
            {
                return results;
            }

            foreach (var file in Directory.GetFiles(rootPath, "*.*", SearchOption.AllDirectories))
            {
                if (ShouldSkipFile(file)) continue;
                var info = new FileInfo(file);
                results.Add(new ResourceItem(Path.GetFileName(file), GetResourceType(file), info.Length, file));
            }

            return results.OrderBy(x => x.Name).ToList();
        }

        public List<string> GetProjectResourceRoots(string projectRoot)
        {
            var result = new List<string>();
            if (string.IsNullOrEmpty(projectRoot) || !Directory.Exists(projectRoot))
            {
                return result;
            }

            var candidateDirs = new[]
            {
                Path.Combine(projectRoot, "res"),
                Path.Combine(projectRoot, "scripts"),
                Path.Combine(projectRoot, "scripts", "client"),
                Path.Combine(projectRoot, "scripts", "share"),
                Path.Combine(projectRoot, "data"),
                Path.Combine(projectRoot, "data", "scene"),
                Path.Combine(projectRoot, "client"),
                Path.Combine(projectRoot, "server"),
                Path.Combine(projectRoot, "common")
            };

            foreach (var dir in candidateDirs)
            {
                if (Directory.Exists(dir)) result.Add(dir);
            }

            if (result.Count == 0)
            {
                foreach (var dir in Directory.GetDirectories(projectRoot))
                {
                    if (ShouldSkipDirectory(dir)) continue;
                    result.Add(dir);
                }
            }

            return result;
        }

        public string GetResourceType(string filePath)
        {
            var extension = Path.GetExtension(filePath).ToLower();
            switch (extension)
            {
                case ".lua": return "Lua";
                case ".json": return "JSON";
                case ".xml": return "XML";
                case ".txt": return "Text";
                case ".ini": return "INI";
                case ".cfg": return "Config";
                case ".csv": return "CSV";
                case ".tsv": return "TSV";
                case ".map": return "Map";
                case ".wdf": return "WDF";
                case ".png": return "PNG";
                case ".jpg":
                case ".jpeg": return "JPG";
                case ".bmp": return "BMP";
                case ".gif": return "GIF";
                case ".ttf":
                case ".ttc": return "Font";
                case ".shader": return "Shader";
                case ".mp3":
                case ".wav": return "Audio";
                default: return "File";
            }
        }

        public bool PackageResources(string inputPath, string outputFile)
        {
            if (string.IsNullOrEmpty(inputPath) || !Directory.Exists(inputPath)) return false;
            if (string.IsNullOrEmpty(outputFile)) return false;

            var dir = Path.GetDirectoryName(outputFile);
            if (!string.IsNullOrEmpty(dir))
            {
                Directory.CreateDirectory(dir);
            }

            var files = Directory.GetFiles(inputPath, "*.*", SearchOption.AllDirectories)
                .Where(f => !ShouldSkipFile(f))
                .ToList();

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

        public bool PackageDirectory(string directoryPath, string outputFile)
        {
            return PackageResources(directoryPath, outputFile);
        }

        private static bool ShouldSkipDirectory(string path)
        {
            var name = Path.GetFileName(path);
            return string.Equals(name, ".git", StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(name, "bin", StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(name, "obj", StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(name, ".vs", StringComparison.OrdinalIgnoreCase);
        }

        private static bool ShouldSkipFile(string path)
        {
            if (path.EndsWith(".tmp", StringComparison.OrdinalIgnoreCase) ||
                path.EndsWith(".log", StringComparison.OrdinalIgnoreCase) ||
                path.EndsWith(".pdb", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            var dir = Path.GetDirectoryName(path);
            return !string.IsNullOrEmpty(dir) && dir.Contains(".git", StringComparison.OrdinalIgnoreCase);
        }

        private static string GetRelativePath(string rootPath, string filePath)
        {
            var rootUri = new Uri(rootPath.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar) + Path.DirectorySeparatorChar);
            var fileUri = new Uri(filePath);
            var rel = rootUri.MakeRelativeUri(fileUri).ToString();
            return rel.Replace('/', Path.DirectorySeparatorChar);
        }
    }
}


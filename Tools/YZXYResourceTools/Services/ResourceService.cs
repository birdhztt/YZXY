using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using YZXYResourceTools.Models;

namespace YZXYResourceTools.Services
{
    public class ResourceService
    {
        private static readonly string[] IgnoredDirectoryNames = { ".git", ".vs", "bin", "obj" };

        public List<ResourceItem> GetResourceItems(string rootPath)
        {
            var results = new List<ResourceItem>();
            if (string.IsNullOrWhiteSpace(rootPath) || !Directory.Exists(rootPath)) return results;

            foreach (var file in EnumerateFilesSafe(rootPath))
            {
                try { results.Add(GetResourceItem(file)); }
                catch (IOException) { }
                catch (UnauthorizedAccessException) { }
            }

            return results.OrderBy(item => item.FullPath, StringComparer.OrdinalIgnoreCase).ToList();
        }

        public ResourceItem GetResourceItem(string filePath)
        {
            var info = new FileInfo(filePath);
            var item = new ResourceItem(info.Name, GetResourceType(filePath), info.Exists ? info.Length : 0, info.FullName);
            item.Category = GetCategory(filePath);
            item.IsText = IsTextResource(Path.GetExtension(filePath));
            item.Description = GetBinarySummary(filePath);
            return item;
        }

        public List<string> GetProjectResourceRoots(string projectRoot)
        {
            var result = new List<string>();
            if (string.IsNullOrWhiteSpace(projectRoot) || !Directory.Exists(projectRoot)) return result;

            // Match the repository's FileSystem paths: res/, scripts/, and data/.
            foreach (var name in new[] { "res", "scripts", "data" })
            {
                var path = Path.Combine(projectRoot, name);
                if (Directory.Exists(path)) result.Add(path);
            }
            return result;
        }

        public string GetResourceType(string filePath)
        {
            switch (Path.GetExtension(filePath).ToLowerInvariant())
            {
                case ".lua": return "Lua";
                case ".json": return "JSON";
                case ".xml": return "XML";
                case ".txt": return "Text";
                case ".ini": return "INI";
                case ".cfg":
                case ".conf": return "Config";
                case ".csv": return "CSV";
                case ".tsv": return "TSV";
                case ".map": return "Map";
                case ".wdf":
                case ".wd1":
                case ".wd2":
                case ".wd3":
                case ".wd4":
                case ".wd5":
                case ".wd6":
                case ".wd7": return "WDF package";
                case ".png": return "PNG image";
                case ".jpg":
                case ".jpeg": return "JPEG image";
                case ".bmp": return "BMP image";
                case ".gif": return "GIF image";
                case ".ttf":
                case ".ttc": return "Font";
                case ".shader": return "Shader";
                case ".mp3":
                case ".wav": return "Audio";
                default: return "File";
            }
        }

        public bool IsTextResource(string extension)
        {
            return new[] { ".lua", ".json", ".xml", ".txt", ".ini", ".cfg", ".conf", ".csv", ".tsv", ".md", ".shader" }
                .Contains(extension ?? string.Empty, StringComparer.OrdinalIgnoreCase);
        }

        public string GetBinarySummary(string filePath)
        {
            if (!File.Exists(filePath)) return "文件不存在。";

            var info = new FileInfo(filePath);
            var extension = Path.GetExtension(filePath).ToLowerInvariant();
            var summary = new StringBuilder();
            summary.AppendLine("类型：" + GetResourceType(filePath));
            summary.AppendLine("大小：" + FormatSize(info.Length));
            summary.AppendLine("路径：" + info.FullName);

            if (extension == ".png")
            {
                int width, height;
                if (TryReadPngDimensions(filePath, out width, out height))
                    summary.AppendLine("尺寸：" + width + " × " + height);
                summary.AppendLine("PNG 文件可预览；此处不修改图像数据。");
            }
            else if (extension == ".wdf" || extension.StartsWith(".wd", StringComparison.Ordinal))
            {
                summary.AppendLine("WDF/W*D 资源包。当前仅识别文件并显示头部摘要；尚未解析包索引或导出内部条目。");
                summary.AppendLine("头部：" + ReadHexPrefix(filePath, 32));
            }
            else if (extension == ".map")
            {
                summary.AppendLine("MAP 场景资源。当前仅识别并显示二进制头部；地图字段解析需依据项目地图格式定义。");
                summary.AppendLine("头部：" + ReadHexPrefix(filePath, 32));
            }
            else
            {
                summary.AppendLine("二进制资源摘要：" + ReadHexPrefix(filePath, 32));
            }

            return summary.ToString();
        }

        public bool PackageDirectory(string directoryPath, string outputFile)
        {
            if (string.IsNullOrWhiteSpace(directoryPath) || !Directory.Exists(directoryPath))
                throw new DirectoryNotFoundException("资源目录不存在：" + directoryPath);
            if (string.IsNullOrWhiteSpace(outputFile)) throw new ArgumentException("必须指定输出文件。", "outputFile");

            var root = Path.GetFullPath(directoryPath).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar) + Path.DirectorySeparatorChar;
            var output = Path.GetFullPath(outputFile);
            if (output.StartsWith(root, StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException("ZIP 输出文件不能放在待打包目录内部。请改选其他输出位置。");

            var parent = Path.GetDirectoryName(output);
            if (!string.IsNullOrEmpty(parent)) Directory.CreateDirectory(parent);
            if (File.Exists(output)) File.Delete(output);

            using (var archive = ZipFile.Open(output, ZipArchiveMode.Create))
            {
                foreach (var file in EnumerateFilesSafe(directoryPath))
                {
                    var relative = GetRelativePath(root, file);
                    archive.CreateEntryFromFile(file, relative, CompressionLevel.Optimal);
                }
            }
            return File.Exists(output);
        }

        private static IEnumerable<string> EnumerateFilesSafe(string root)
        {
            var pending = new Stack<string>();
            pending.Push(root);
            while (pending.Count > 0)
            {
                var current = pending.Pop();
                string[] files;
                try { files = Directory.GetFiles(current); }
                catch (UnauthorizedAccessException) { continue; }
                catch (IOException) { continue; }

                foreach (var file in files)
                {
                    var ext = Path.GetExtension(file);
                    if (ext.Equals(".tmp", StringComparison.OrdinalIgnoreCase) || ext.Equals(".pdb", StringComparison.OrdinalIgnoreCase)) continue;
                    yield return file;
                }

                string[] directories;
                try { directories = Directory.GetDirectories(current); }
                catch (UnauthorizedAccessException) { continue; }
                catch (IOException) { continue; }

                foreach (var directory in directories)
                {
                    if (!IgnoredDirectoryNames.Contains(Path.GetFileName(directory), StringComparer.OrdinalIgnoreCase))
                        pending.Push(directory);
                }
            }
        }

        private static string GetCategory(string path)
        {
            var normalized = path.Replace('\\', '/').ToLowerInvariant();
            if (normalized.Contains("/scripts/") || normalized.Contains("/lua/")) return "Script";
            if (normalized.Contains("/data/scene/") || Path.GetExtension(path).Equals(".map", StringComparison.OrdinalIgnoreCase)) return "Map";
            if (Path.GetExtension(path).Equals(".wdf", StringComparison.OrdinalIgnoreCase) || Path.GetExtension(path).StartsWith(".wd", StringComparison.OrdinalIgnoreCase)) return "Package";
            if (normalized.Contains("/texture/") || normalized.Contains("/icon/") || Path.GetExtension(path).Equals(".png", StringComparison.OrdinalIgnoreCase)) return "Texture";
            if (normalized.Contains("/res/")) return "Resource";
            return "File";
        }

        private static bool TryReadPngDimensions(string path, out int width, out int height)
        {
            width = 0;
            height = 0;
            try
            {
                using (var stream = File.OpenRead(path))
                {
                    if (stream.Length < 24) return false;
                    var header = new byte[24];
                    if (stream.Read(header, 0, header.Length) != header.Length) return false;
                    byte[] signature = { 137, 80, 78, 71, 13, 10, 26, 10 };
                    for (var i = 0; i < signature.Length; i++) if (header[i] != signature[i]) return false;
                    if (Encoding.ASCII.GetString(header, 12, 4) != "IHDR") return false;
                    width = ReadBigEndianInt32(header, 16);
                    height = ReadBigEndianInt32(header, 20);
                    return width > 0 && height > 0;
                }
            }
            catch (IOException) { return false; }
        }

        private static int ReadBigEndianInt32(byte[] data, int offset)
        {
            return (data[offset] << 24) | (data[offset + 1] << 16) | (data[offset + 2] << 8) | data[offset + 3];
        }

        private static string ReadHexPrefix(string path, int maxBytes)
        {
            using (var stream = File.OpenRead(path))
            {
                var buffer = new byte[(int)Math.Min(maxBytes, stream.Length)];
                var count = stream.Read(buffer, 0, buffer.Length);
                return BitConverter.ToString(buffer, 0, count).Replace('-', ' ');
            }
        }

        private static string GetRelativePath(string rootWithSeparator, string filePath)
        {
            var rootUri = new Uri(rootWithSeparator);
            var fileUri = new Uri(Path.GetFullPath(filePath));
            return Uri.UnescapeDataString(rootUri.MakeRelativeUri(fileUri).ToString()).Replace('/', '/');
        }

        private static string FormatSize(long size)
        {
            if (size < 1024) return size + " B";
            if (size < 1024 * 1024) return (size / 1024.0).ToString("0.##") + " KB";
            return (size / (1024.0 * 1024.0)).ToString("0.##") + " MB";
        }
    }
}

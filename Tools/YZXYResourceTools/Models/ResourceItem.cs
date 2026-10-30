using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
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
                var item = new ResourceItem(
                    Path.GetFileName(file),
                    GetResourceType(file),
                    info.Length,
                    file);

                item.Category = DetectCategory(file);
                item.IsText = IsTextResource(Path.GetExtension(file));
                item.Description = GetBinarySummary(file);

                var pngSize = TryReadPngSize(file);
                if (pngSize != null)
                {
                    item.Width = pngSize.Value.Width;
                    item.Height = pngSize.Value.Height;
                }

                results.Add(item);
            }

            return results.OrderBy(x => x.Name).ToList();
        }

        public ResourceItem GetResourceItem(string filePath)
        {
            if (!File.Exists(filePath))
            {
                return new ResourceItem(Path.GetFileName(filePath), "File", 0, filePath);
            }

            var info = new FileInfo(filePath);
            var item = new ResourceItem(Path.GetFileName(filePath), GetResourceType(filePath), info.Length, filePath);
            item.Category = DetectCategory(filePath);
            item.IsText = IsTextResource(Path.GetExtension(filePath));
            item.Description = GetBinarySummary(filePath);

            var pngSize = TryReadPngSize(filePath);
            if (pngSize != null)
            {
                item.Width = pngSize.Value.Width;
                item.Height = pngSize.Value.Height;
            }

            return item;
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

        public string GetBinarySummary(string filePath)
        {
            if (!File.Exists(filePath)) return "文件不存在";

            var extension = Path.GetExtension(filePath).ToLower();
            var info = new FileInfo(filePath);
            var sb = new StringBuilder();
            sb.AppendLine("文件类型：" + GetResourceType(filePath));
            sb.AppendLine("大小：" + FormatSize(info.Length));
            sb.AppendLine("路径：" + filePath);

            if (string.Equals(extension, ".png", StringComparison.OrdinalIgnoreCase))
            {
                var size = TryReadPngSize(filePath);
                if (size != null)
                {
                    sb.AppendLine("PNG尺寸：" + size.Value.Width + "x" + size.Value.Height);
                    sb.AppendLine("说明：PNG 图像资源，支持尺寸识别。");
                    return sb.ToString();
                }
            }

            if (string.Equals(extension, ".map", StringComparison.OrdinalIgnoreCase))
            {
                sb.AppendLine("说明：地图数据资源；此文件通常是二进制场景数据。实现中已识别为 Map 类型。 ");
                sb.AppendLine("前16字节：" + GetHexPrefix(filePath, 16));
                return sb.ToString();
            }

            if (string.Equals(extension, ".wdf", StringComparison.OrdinalIgnoreCase))
            {
                sb.AppendLine("说明：WDF 资源包文件，通常用于存储游戏资源包数据。 ");
                sb.AppendLine("前16字节：" + GetHexPrefix(filePath, 16));
                return sb.ToString();
            }

            if (IsTextResource(extension))
            {
                sb.AppendLine("说明：文本资源，可直接读取和编辑。 ");
                return sb.ToString();
            }

            sb.AppendLine("说明：二进制资源，适合在资源浏览器中查看元数据。 ");
            sb.AppendLine("前16字节：" + GetHexPrefix(filePath, 16));
            return sb.ToString();
        }

        public bool IsTextResource(string extension)
        {
            return ".lua".Equals(extension, StringComparison.OrdinalIgnoreCase) ||
                   ".json".Equals(extension, StringComparison.OrdinalIgnoreCase) ||
                   ".txt".Equals(extension, StringComparison.OrdinalIgnoreCase) ||
                   ".xml".Equals(extension, StringComparison.OrdinalIgnoreCase) ||
                   ".ini".Equals(extension, StringComparison.OrdinalIgnoreCase) ||
                   ".cfg".Equals(extension, StringComparison.OrdinalIgnoreCase) ||
                   ".csv".Equals(extension, StringComparison.OrdinalIgnoreCase) ||
                   ".tsv".Equals(extension, StringComparison.OrdinalIgnoreCase) ||
                   ".md".Equals(extension, StringComparison.OrdinalIgnoreCase) ||
                   ".shader".Equals(extension, StringComparison.OrdinalIgnoreCase) ||
                   ".conf".Equals(extension, StringComparison.OrdinalIgnoreCase) ||
                   ".log".Equals(extension, StringComparison.OrdinalIgnoreCase);
        }

        private static string DetectCategory(string filePath)
        {
            var lower = filePath.ToLowerInvariant();
            if (lower.Contains("/scripts/") || lower.Contains("\\scripts\\") || lower.Contains("/lua/")) return "Script";
            if (lower.Contains("/scene/") || lower.Contains("\\scene\\") || lower.Contains(".map")) return "Map";
            if (lower.Contains("/texture/") || lower.Contains("\\texture\\") || lower.Contains("/icon/") || lower.Contains("/res/") && lower.Contains(".png")) return "Texture";
            if (lower.Contains(".wdf")) return "WDF";
            if (lower.Contains("/res/") || lower.Contains("\\res\\")) return "Resource";
            return "File";
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

        private static string GetHexPrefix(string filePath, int length)
        {
            try
            {
                using (var fs = File.OpenRead(filePath))
                {
                    var buffer = new byte[Math.Min(length, fs.Length)];
                    var read = fs.Read(buffer, 0, buffer.Length);
                    if (read <= 0) return "--";
                    var sb = new StringBuilder();
                    foreach (var b in buffer)
                    {
                        sb.Append(b.ToString("X2"));
                        sb.Append(" ");
                    }
                    return sb.ToString().Trim();
                }
            }
            catch
            {
                return "--";
            }
        }

        private static (int Width, int Height)? TryReadPngSize(string filePath)
        {
            try
            {
                using (var fs = File.OpenRead(filePath))
                {
                    if (fs.Length < 24) return null;

                    var sig = new byte[8];
                    fs.Read(sig, 0, 8);
                    if (sig[0] != 0x89 || sig[1] != 0x50 || sig[2] != 0x4E || sig[3] != 0x47 || sig[4] != 0x0D || sig[5] != 0x0A || sig[6] != 0x1A || sig[7] != 0x0A)
                    {
                        return null;
                    }

                    var lengthBytes = new byte[4];
                    fs.Read(lengthBytes, 0, 4);
                    var ihdr = new byte[4];
                    fs.Read(ihdr, 0, 4);
                    if (Encoding.ASCII.GetString(ihdr) != "IHDR") return null;

                    var widthBytes = new byte[4];
                    var heightBytes = new byte[4];
                    fs.Read(widthBytes, 0, 4);
                    fs.Read(heightBytes, 0, 4);

                    var width = BitConverter.ToInt32(Reverse(widthBytes), 0);
                    var height = BitConverter.ToInt32(Reverse(heightBytes), 0);
                    return (width, height);
                }
            }
            catch
            {
                return null;
            }
        }

        private static byte[] Reverse(byte[] data)
        {
            var reversed = new byte[data.Length];
            for (var i = 0; i < data.Length; i++)
            {
                reversed[i] = data[data.Length - 1 - i];
            }
            return reversed;
        }

        private static string FormatSize(long size)
        {
            if (size < 1024) return size + " B";
            if (size < 1024 * 1024) return (size / 1024.0).ToString("0.##") + " KB";
            return (size / (1024.0 * 1024.0)).ToString("0.##") + " MB";
        }
    }

    public class ResourcePackageForm : Form
    {
        private readonly Button _selectFolderButton;
        private readonly Button _selectOutputButton;
        private readonly Button _packButton;
        private readonly TextBox _outputPathTextBox;
        private readonly TextBox _searchTextBox;
        private readonly ListView _resourceList;
        private readonly Label _statusLabel;
        private readonly ResourceService _resourceService;

        private string _rootPath;

        public ResourcePackageForm()
        {
            Text = "YZXY 批量资源打包";
            Width = 980;
            Height = 680;
            StartPosition = FormStartPosition.CenterParent;

            _resourceService = new ResourceService();
            _selectFolderButton = new Button();
            _selectOutputButton = new Button();
            _packButton = new Button();
            _outputPathTextBox = new TextBox();
            _searchTextBox = new TextBox();
            _resourceList = new ListView();
            _statusLabel = new Label();

            InitializeComponent();
        }

        public void SetRootPath(string path)
        {
            _rootPath = path;
            _statusLabel.Text = "目录：" + path;
            RefreshList();
        }

        private void InitializeComponent()
        {
            var topPanel = new FlowLayoutPanel();
            topPanel.Dock = DockStyle.Top;
            topPanel.Height = 55;
            topPanel.FlowDirection = FlowDirection.LeftToRight;

            _selectFolderButton.Text = "选择目录";
            _selectFolderButton.Width = 120;
            _selectFolderButton.Click += SelectFolderButton_Click;

            _selectOutputButton.Text = "输出路径";
            _selectOutputButton.Width = 120;
            _selectOutputButton.Click += SelectOutputButton_Click;

            _outputPathTextBox.Width = 350;
            _outputPathTextBox.Text = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "yzxy_resource_pack.zip");

            _packButton.Text = "开始打包";
            _packButton.Width = 130;
            _packButton.Click += PackButton_Click;

            topPanel.Controls.Add(_selectFolderButton);
            topPanel.Controls.Add(_selectOutputButton);
            topPanel.Controls.Add(_outputPathTextBox);
            topPanel.Controls.Add(_packButton);

            _searchTextBox.Dock = DockStyle.Top;
            _searchTextBox.TextChanged += SearchTextBox_TextChanged;

            _resourceList.Dock = DockStyle.Fill;
            _resourceList.View = View.Details;
            _resourceList.FullRowSelect = true;
            _resourceList.GridLines = true;
            _resourceList.Columns.Add("名称", 220);
            _resourceList.Columns.Add("类型", 120);
            _resourceList.Columns.Add("大小", 100);
            _resourceList.Columns.Add("路径", 500);

            _statusLabel.Dock = DockStyle.Bottom;
            _statusLabel.Height = 28;
            _statusLabel.Text = "等待选择目录";

            Controls.Add(topPanel);
            Controls.Add(_searchTextBox);
            Controls.Add(_resourceList);
            Controls.Add(_statusLabel);
        }

        private void SelectFolderButton_Click(object sender, EventArgs e)
        {
            using (var dialog = new FolderBrowserDialog())
            {
                dialog.Description = "选择要打包的资源目录";
                if (dialog.ShowDialog(this) == DialogResult.OK)
                {
                    SetRootPath(dialog.SelectedPath);
                }
            }
        }

        private void SelectOutputButton_Click(object sender, EventArgs e)
        {
            using (var dialog = new SaveFileDialog())
            {
                dialog.Filter = "ZIP 压缩包|*.zip";
                dialog.FileName = "yzxy_resource_pack.zip";
                if (dialog.ShowDialog(this) == DialogResult.OK)
                {
                    _outputPathTextBox.Text = dialog.FileName;
                }
            }
        }

        private void SearchTextBox_TextChanged(object sender, EventArgs e)
        {
            RefreshList();
        }

        private void RefreshList()
        {
            _resourceList.Items.Clear();
            if (string.IsNullOrEmpty(_rootPath) || !Directory.Exists(_rootPath)) return;

            var filter = _searchTextBox.Text.Trim();
            var items = _resourceService.GetResourceItems(_rootPath);
            if (!string.IsNullOrEmpty(filter))
            {
                items = items.Where(i => i.Name.IndexOf(filter, StringComparison.OrdinalIgnoreCase) >= 0 ||
                                        i.Type.IndexOf(filter, StringComparison.OrdinalIgnoreCase) >= 0).ToList();
            }

            foreach (var item in items)
            {
                var row = new ListViewItem(new[]
                {
                    item.Name,
                    item.Type,
                    FormatSize(item.SizeBytes),
                    item.FullPath
                });
                row.Tag = item.FullPath;
                _resourceList.Items.Add(row);
            }

            _statusLabel.Text = "目录：" + _rootPath + "，已筛选：" + _resourceList.Items.Count + " 个资源";
        }

        private void PackButton_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(_rootPath) || !Directory.Exists(_rootPath))
            {
                MessageBox.Show("请先选择资源目录。", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var output = _outputPathTextBox.Text.Trim();
            if (string.IsNullOrEmpty(output))
            {
                MessageBox.Show("请指定输出路径。", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                var success = _resourceService.PackageDirectory(_rootPath, output);
                if (success)
                {
                    _statusLabel.Text = "打包成功：" + output;
                    MessageBox.Show("资源打包已完成。", "完成", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    MessageBox.Show("打包失败，请检查目录和输出路径。", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("打包异常：" + ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private static string FormatSize(long size)
        {
            if (size < 1024) return size + " B";
            if (size < 1024 * 1024) return (size / 1024.0).ToString("0.##") + " KB";
            return (size / (1024.0 * 1024.0)).ToString("0.##") + " MB";
        }
    }
}


using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using YZXYResourceTools.Models;
using YZXYResourceTools.Services;

namespace YZXYResourceTools
{
    public class ResourceEditorForm : Form
    {
        private readonly TreeView _resourceTree;
        private readonly ListView _resourceList;
        private readonly TextBox _searchTextBox;
        private readonly TextBox _contentTextBox;
        private readonly Button _saveButton;
        private readonly Button _refreshButton;
        private readonly Label _statusLabel;
        private readonly SplitContainer _splitContainer;
        private readonly ContextMenuStrip _contextMenu;
        private readonly ToolStripMenuItem _openMenuItem;
        private readonly ToolStripMenuItem _copyPathMenuItem;
        private readonly ToolStripMenuItem _exportMenuItem;
        private readonly ToolStripMenuItem _packMenuItem;

        private readonly ResourceService _resourceService;
        private string _rootPath;
        private string _currentFilePath;
        private List<ResourceItem> _allItems;

        public ResourceEditorForm()
        {
            Text = "YZXY 资源编辑器";
            Width = 1100;
            Height = 720;
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.Sizable;

            _resourceService = new ResourceService();
            _allItems = new List<ResourceItem>();

            _splitContainer = new SplitContainer();
            _resourceTree = new TreeView();
            _resourceList = new ListView();
            _searchTextBox = new TextBox();
            _contentTextBox = new TextBox();
            _saveButton = new Button();
            _refreshButton = new Button();
            _statusLabel = new Label();

            _contextMenu = new ContextMenuStrip();
            _openMenuItem = new ToolStripMenuItem("打开");
            _copyPathMenuItem = new ToolStripMenuItem("复制路径");
            _exportMenuItem = new ToolStripMenuItem("导出到...");
            _packMenuItem = new ToolStripMenuItem("批量打包此目录");

            InitializeComponent();
            TryLoadDefaultProjectRoot();
        }

        public void LoadResourcePath(string path)
        {
            if (string.IsNullOrEmpty(path) || !Directory.Exists(path))
            {
                MessageBox.Show("资源目录不存在，无法打开。", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            _rootPath = path;
            Text = "YZXY 资源编辑器 - " + path;
            BuildProjectTree(path);
            LoadDirectory(path);
            _statusLabel.Text = "当前目录：" + path;
        }

        private void InitializeComponent()
        {
            _splitContainer.Dock = DockStyle.Fill;
            _splitContainer.Orientation = Orientation.Horizontal;

            _resourceTree.Dock = DockStyle.Fill;
            _resourceTree.AfterSelect += ResourceTree_AfterSelect;

            _searchTextBox.Dock = DockStyle.Top;
            _searchTextBox.PlaceholderText = "搜索文件名/类型";
            _searchTextBox.TextChanged += SearchTextBox_TextChanged;

            _resourceList.Dock = DockStyle.Fill;
            _resourceList.View = View.Details;
            _resourceList.FullRowSelect = true;
            _resourceList.GridLines = true;
            _resourceList.Columns.Add("名称", 260);
            _resourceList.Columns.Add("类型", 120);
            _resourceList.Columns.Add("大小", 100);
            _resourceList.Columns.Add("路径", 420);
            _resourceList.MouseDoubleClick += ResourceList_MouseDoubleClick;
            _resourceList.ContextMenuStrip = _contextMenu;

            _contentTextBox.Dock = DockStyle.Fill;
            _contentTextBox.Multiline = true;
            _contentTextBox.ScrollBars = ScrollBars.Both;
            _contentTextBox.Font = new Font("Consolas", 10F);

            _refreshButton.Text = "刷新";
            _refreshButton.Width = 90;
            _refreshButton.Click += RefreshButton_Click;

            _saveButton.Text = "保存";
            _saveButton.Width = 90;
            _saveButton.Click += SaveButton_Click;

            var buttonPanel = new FlowLayoutPanel();
            buttonPanel.Dock = DockStyle.Bottom;
            buttonPanel.FlowDirection = FlowDirection.LeftToRight;
            buttonPanel.AutoSize = true;
            buttonPanel.Controls.Add(_refreshButton);
            buttonPanel.Controls.Add(_saveButton);

            _statusLabel.Dock = DockStyle.Bottom;
            _statusLabel.Height = 26;
            _statusLabel.Text = "就绪";

            var leftPanel = new Panel();
            leftPanel.Dock = DockStyle.Fill;
            leftPanel.Controls.Add(_resourceTree);

            var rightPanel = new Panel();
            rightPanel.Dock = DockStyle.Fill;
            rightPanel.Controls.Add(_searchTextBox);
            rightPanel.Controls.Add(_resourceList);
            rightPanel.Controls.Add(buttonPanel);
            rightPanel.Controls.Add(_contentTextBox);

            _contentTextBox.BringToFront();
            _splitContainer.Panel1.Controls.Add(leftPanel);
            _splitContainer.Panel2.Controls.Add(rightPanel);

            _contextMenu.Items.Add(_openMenuItem);
            _contextMenu.Items.Add(_copyPathMenuItem);
            _contextMenu.Items.Add(_exportMenuItem);
            _contextMenu.Items.Add(_packMenuItem);

            _openMenuItem.Click += OpenSelectedFile_Click;
            _copyPathMenuItem.Click += CopySelectedPath_Click;
            _exportMenuItem.Click += ExportSelectedFile_Click;
            _packMenuItem.Click += PackCurrentDirectory_Click;

            Controls.Add(_splitContainer);
            Controls.Add(_statusLabel);
        }

        private void TryLoadDefaultProjectRoot()
        {
            var projectRootCandidates = new[]
            {
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", ".."),
                AppDomain.CurrentDomain.BaseDirectory,
                Environment.CurrentDirectory,
                Directory.GetCurrentDirectory()
            };

            foreach (var candidate in projectRootCandidates)
            {
                try
                {
                    var full = Path.GetFullPath(candidate);
                    if (Directory.Exists(full) && Directory.Exists(Path.Combine(full, "scripts")) && Directory.Exists(Path.Combine(full, "res")))
                    {
                        LoadResourcePath(full);
                        return;
                    }
                }
                catch
                {
                    // ignore invalid path
                }
            }

            var demoRoot = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "YZXY_Project");
            if (!Directory.Exists(demoRoot))
            {
                Directory.CreateDirectory(demoRoot);
            }
            LoadResourcePath(demoRoot);
        }

        private void BuildProjectTree(string rootPath)
        {
            _resourceTree.Nodes.Clear();
            var rootNode = new TreeNode(Path.GetFileName(rootPath) ?? "project");
            rootNode.Tag = rootPath;
            _resourceTree.Nodes.Add(rootNode);

            var resourceRoots = _resourceService.GetProjectResourceRoots(rootPath);
            foreach (var dir in resourceRoots)
            {
                var folderNode = new TreeNode(Path.GetFileName(dir));
                folderNode.Tag = dir;
                rootNode.Nodes.Add(folderNode);
                AddDirectoryNodes(folderNode, dir);
            }

            if (_resourceTree.Nodes.Count > 0)
            {
                _resourceTree.Nodes[0].Expand();
            }
        }

        private void AddDirectoryNodes(TreeNode parentNode, string directoryPath)
        {
            if (!Directory.Exists(directoryPath)) return;

            foreach (var directory in Directory.GetDirectories(directoryPath))
            {
                var dirName = Path.GetFileName(directory);
                if (dirName == ".git" || dirName == "bin" || dirName == "obj") continue;

                var node = new TreeNode(dirName);
                node.Tag = directory;
                parentNode.Nodes.Add(node);
                AddDirectoryNodes(node, directory);
            }
        }

        private void ResourceTree_AfterSelect(object sender, TreeViewEventArgs e)
        {
            if (e.Node == null || e.Node.Tag == null) return;

            var tagPath = e.Node.Tag as string;
            if (string.IsNullOrEmpty(tagPath)) return;

            if (Directory.Exists(tagPath))
            {
                LoadDirectory(tagPath);
            }
            else if (File.Exists(tagPath))
            {
                _currentFilePath = tagPath;
                OpenFile(tagPath);
            }
        }

        private void LoadDirectory(string directoryPath)
        {
            _resourceList.Items.Clear();
            _allItems = _resourceService.GetResourceItems(directoryPath);

            var filter = _searchTextBox.Text.Trim();
            var query = filter;
            if (!string.IsNullOrEmpty(query))
            {
                _allItems = _allItems.Where(x => x.Name.IndexOf(query, StringComparison.OrdinalIgnoreCase) >= 0 || x.Type.IndexOf(query, StringComparison.OrdinalIgnoreCase) >= 0).ToList();
            }

            foreach (var item in _allItems)
            {
                var listItem = new ListViewItem(new[]
                {
                    item.Name,
                    item.Type,
                    FormatSize(item.SizeBytes),
                    item.FullPath
                });
                listItem.Tag = item.FullPath;
                _resourceList.Items.Add(listItem);
            }

            _statusLabel.Text = "目录：" + directoryPath + "，资源数：" + _resourceList.Items.Count;
        }

        private void ResourceList_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (_resourceList.SelectedItems.Count == 0) return;
            var selectedPath = _resourceList.SelectedItems[0].Tag as string;
            if (!string.IsNullOrEmpty(selectedPath))
            {
                _currentFilePath = selectedPath;
                OpenFile(selectedPath);
            }
        }

        private void SearchTextBox_TextChanged(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(_rootPath))
            {
                LoadDirectory(_rootPath);
            }
        }

        private void OpenSelectedFile_Click(object sender, EventArgs e)
        {
            if (_resourceList.SelectedItems.Count == 0) return;
            var path = _resourceList.SelectedItems[0].Tag as string;
            if (!string.IsNullOrEmpty(path))
            {
                _currentFilePath = path;
                OpenFile(path);
            }
        }

        private void CopySelectedPath_Click(object sender, EventArgs e)
        {
            if (_resourceList.SelectedItems.Count == 0) return;
            var path = _resourceList.SelectedItems[0].Tag as string;
            if (!string.IsNullOrEmpty(path))
            {
                Clipboard.SetText(path);
                _statusLabel.Text = "已复制路径：" + path;
            }
        }

        private void ExportSelectedFile_Click(object sender, EventArgs e)
        {
            if (_resourceList.SelectedItems.Count == 0) return;

            var sourcePath = _resourceList.SelectedItems[0].Tag as string;
            if (string.IsNullOrEmpty(sourcePath) || !File.Exists(sourcePath)) return;

            using (var saveDialog = new SaveFileDialog())
            {
                saveDialog.FileName = Path.GetFileName(sourcePath);
                saveDialog.Filter = "All files (*.*)|*.*";
                if (saveDialog.ShowDialog(this) == DialogResult.OK)
                {
                    File.Copy(sourcePath, saveDialog.FileName, true);
                    _statusLabel.Text = "已导出：" + saveDialog.FileName;
                }
            }
        }

        private void PackCurrentDirectory_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(_rootPath)) return;

            var packageForm = new ResourcePackageForm();
            packageForm.SetRootPath(_rootPath);
            packageForm.ShowDialog(this);
        }

        private void RefreshButton_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(_rootPath))
            {
                LoadResourcePath(_rootPath);
            }
        }

        private void SaveButton_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(_currentFilePath) || !File.Exists(_currentFilePath))
            {
                MessageBox.Show("当前没有可保存的资源文件。", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            try
            {
                var ext = Path.GetExtension(_currentFilePath).ToLower();
                if (IsTextResource(ext))
                {
                    File.WriteAllText(_currentFilePath, _contentTextBox.Text, Encoding.UTF8);
                    _statusLabel.Text = "已保存：" + _currentFilePath;
                    MessageBox.Show("保存成功。", "完成", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    MessageBox.Show("当前资源类型不支持直接文本保存。", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("保存失败：" + ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void OpenFile(string filePath)
        {
            if (!File.Exists(filePath)) return;

            _currentFilePath = filePath;
            var ext = Path.GetExtension(filePath).ToLower();
            if (IsTextResource(ext))
            {
                try
                {
                    _contentTextBox.Text = File.ReadAllText(filePath, Encoding.UTF8);
                }
                catch
                {
                    _contentTextBox.Text = File.ReadAllText(filePath, Encoding.Default);
                }
                _statusLabel.Text = "正在查看文本资源：" + filePath;
            }
            else
            {
                _contentTextBox.Text = "文件类型：" + _resourceService.GetResourceType(filePath) + "\r\n\r\n" +
                                       "路径：" + filePath + "\r\n" +
                                       "说明：此资源为二进制或非文本文件，无法直接编辑。";
                _statusLabel.Text = "已打开二进制资源：" + filePath;
            }
        }

        private static bool IsTextResource(string ext)
        {
            return ".lua".Equals(ext) || ".json".Equals(ext) || ".txt".Equals(ext) || ".xml".Equals(ext) ||
                   ".ini".Equals(ext) || ".csv".Equals(ext) || ".tsv".Equals(ext) || ".md".Equals(ext) ||
                   ".cfg".Equals(ext) || ".conf".Equals(ext) || ".log".Equals(ext);
        }

        private static string FormatSize(long size)
        {
            if (size < 1024) return size + " B";
            if (size < 1024 * 1024) return (size / 1024.0).ToString("0.##") + " KB";
            return (size / (1024.0 * 1024.0)).ToString("0.##") + " MB";
        }
    }
}


using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using YZXYResourceTools.Models;

namespace YZXYResourceTools
{
    public class ResourceEditorForm : Form
    {
        private readonly TreeView _resourceTree;
        private readonly ListView _resourceList;
        private readonly ComboBox _typeFilterComboBox;
        private readonly TextBox _searchTextBox;
        private readonly TextBox _contentTextBox;
        private readonly PictureBox _previewImage;
        private readonly TabControl _tabControl;
        private readonly TabPage _resourceListTab;
        private readonly TabPage _resourceDetailsTab;
        private readonly Button _saveButton;
        private readonly Button _refreshButton;
        private readonly Button _importButton;
        private readonly Label _statusLabel;
        private readonly SplitContainer _splitContainer;
        private readonly ContextMenuStrip _contextMenu;
        private readonly ToolStripMenuItem _openMenuItem;
        private readonly ToolStripMenuItem _copyPathMenuItem;
        private readonly ToolStripMenuItem _exportMenuItem;
        private readonly ToolStripMenuItem _packMenuItem;
        private readonly ToolStripMenuItem _importMenuItem;

        private readonly Services.ResourceService _resourceService;
        private string _rootPath;
        private string _currentFilePath;
        private List<ResourceItem> _allItems;

        public ResourceEditorForm()
        {
            Text = "YZXY 资源编辑器 + 资源包浏览器";
            Width = 1200;
            Height = 760;
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.Sizable;

            _resourceService = new Services.ResourceService();
            _allItems = new List<ResourceItem>();

            _resourceTree = new TreeView();
            _resourceList = new ListView();
            _typeFilterComboBox = new ComboBox();
            _searchTextBox = new TextBox();
            _contentTextBox = new TextBox();
            _previewImage = new PictureBox();
            _tabControl = new TabControl();
            _resourceListTab = new TabPage("资源列表");
            _resourceDetailsTab = new TabPage("资源详情");
            _saveButton = new Button();
            _refreshButton = new Button();
            _importButton = new Button();
            _statusLabel = new Label();
            _splitContainer = new SplitContainer();
            _contextMenu = new ContextMenuStrip();
            _openMenuItem = new ToolStripMenuItem("打开");
            _copyPathMenuItem = new ToolStripMenuItem("复制路径");
            _exportMenuItem = new ToolStripMenuItem("导出到...");
            _packMenuItem = new ToolStripMenuItem("批量打包此目录");
            _importMenuItem = new ToolStripMenuItem("导入资源");

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
            Text = "YZXY 资源编辑器 + 资源包浏览器 - " + path;
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

            _typeFilterComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
            _typeFilterComboBox.Items.Add("全部");
            _typeFilterComboBox.Items.Add("Lua");
            _typeFilterComboBox.Items.Add("Map");
            _typeFilterComboBox.Items.Add("PNG");
            _typeFilterComboBox.Items.Add("WDF");
            _typeFilterComboBox.Items.Add("Text");
            _typeFilterComboBox.SelectedIndex = 0;
            _typeFilterComboBox.SelectedIndexChanged += TypeFilterComboBox_SelectedIndexChanged;

            _searchTextBox.Width = 200;
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

            _contentTextBox.Multiline = true;
            _contentTextBox.ScrollBars = ScrollBars.Both;
            _contentTextBox.Dock = DockStyle.Fill;
            _contentTextBox.Font = new Font("Consolas", 10F);

            _previewImage.Dock = DockStyle.Fill;
            _previewImage.SizeMode = PictureBoxSizeMode.Zoom;
            _previewImage.BorderStyle = BorderStyle.FixedSingle;
            _previewImage.BackColor = Color.Black;

            _resourceListTab.Controls.Add(_resourceList);
            _resourceDetailsTab.Controls.Add(_previewImage);
            _resourceDetailsTab.Controls.Add(_contentTextBox);

            _tabControl.Dock = DockStyle.Fill;
            _tabControl.TabPages.Add(_resourceListTab);
            _tabControl.TabPages.Add(_resourceDetailsTab);

            _saveButton.Text = "保存";
            _saveButton.Width = 90;
            _saveButton.Click += SaveButton_Click;

            _refreshButton.Text = "刷新";
            _refreshButton.Width = 90;
            _refreshButton.Click += RefreshButton_Click;

            _importButton.Text = "导入";
            _importButton.Width = 90;
            _importButton.Click += ImportButton_Click;

            var toolbar = new FlowLayoutPanel();
            toolbar.Dock = DockStyle.Top;
            toolbar.Height = 42;
            toolbar.FlowDirection = FlowDirection.LeftToRight;
            toolbar.Controls.Add(new Label { Text = "筛选：", AutoSize = true, Margin = new Padding(3) });
            toolbar.Controls.Add(_typeFilterComboBox);
            toolbar.Controls.Add(new Label { Text = "搜索：", AutoSize = true, Margin = new Padding(3) });
            toolbar.Controls.Add(_searchTextBox);
            toolbar.Controls.Add(_refreshButton);
            toolbar.Controls.Add(_importButton);
            toolbar.Controls.Add(_saveButton);

            var rightPanel = new Panel();
            rightPanel.Dock = DockStyle.Fill;
            rightPanel.Controls.Add(toolbar);
            rightPanel.Controls.Add(_tabControl);

            _statusLabel.Dock = DockStyle.Bottom;
            _statusLabel.Height = 26;
            _statusLabel.Text = "就绪";

            var leftPanel = new Panel();
            leftPanel.Dock = DockStyle.Fill;
            leftPanel.Controls.Add(_resourceTree);

            _splitContainer.Panel1.Controls.Add(leftPanel);
            _splitContainer.Panel2.Controls.Add(rightPanel);

            _contextMenu.Items.Add(_openMenuItem);
            _contextMenu.Items.Add(_copyPathMenuItem);
            _contextMenu.Items.Add(_exportMenuItem);
            _contextMenu.Items.Add(_importMenuItem);
            _contextMenu.Items.Add(_packMenuItem);

            _openMenuItem.Click += OpenSelectedFile_Click;
            _copyPathMenuItem.Click += CopySelectedPath_Click;
            _exportMenuItem.Click += ExportSelectedFile_Click;
            _importMenuItem.Click += ImportButton_Click;
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
                }
            }

            var demoRoot = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "YZXY_Project");
            if (!Directory.Exists(demoRoot)) Directory.CreateDirectory(demoRoot);
            LoadResourcePath(demoRoot);
        }

        private void BuildProjectTree(string rootPath)
        {
            _resourceTree.Nodes.Clear();
            var rootNode = new TreeNode(Path.GetFileName(rootPath) ?? "project");
            rootNode.Tag = rootPath;
            _resourceTree.Nodes.Add(rootNode);

            foreach (var dir in _resourceService.GetProjectResourceRoots(rootPath))
            {
                var baseNode = new TreeNode(Path.GetFileName(dir));
                baseNode.Tag = dir;
                rootNode.Nodes.Add(baseNode);
                AddDirectoryNodes(baseNode, dir);
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
                if (dirName == ".git" || dirName == "bin" || dirName == "obj" || dirName == ".vs") continue;

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
        }

        private void TypeFilterComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(_rootPath))
            {
                LoadDirectory(_rootPath);
            }
        }

        private void SearchTextBox_TextChanged(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(_rootPath))
            {
                LoadDirectory(_rootPath);
            }
        }

        private void LoadDirectory(string directoryPath)
        {
            _resourceList.Items.Clear();
            if (!Directory.Exists(directoryPath)) return;

            var allItems = _resourceService.GetResourceItems(directoryPath);
            var typeFilter = _typeFilterComboBox.SelectedItem as string ?? "全部";
            var searchText = _searchTextBox.Text.Trim();

            if (!string.Equals(typeFilter, "全部", StringComparison.OrdinalIgnoreCase))
            {
                allItems = allItems.Where(x => string.Equals(x.Type, typeFilter, StringComparison.OrdinalIgnoreCase)).ToList();
            }

            if (!string.IsNullOrEmpty(searchText))
            {
                allItems = allItems.Where(x => x.Name.IndexOf(searchText, StringComparison.OrdinalIgnoreCase) >= 0 ||
                                               x.Type.IndexOf(searchText, StringComparison.OrdinalIgnoreCase) >= 0 ||
                                               x.FullPath.IndexOf(searchText, StringComparison.OrdinalIgnoreCase) >= 0).ToList();
            }

            _allItems = allItems;
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
                _tabControl.SelectedTab = _resourceDetailsTab;
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
                _tabControl.SelectedTab = _resourceDetailsTab;
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

        private void ImportButton_Click(object sender, EventArgs e)
        {
            using (var openDialog = new OpenFileDialog())
            {
                openDialog.Multiselect = true;
                openDialog.Filter = "All files (*.*)|*.*";
                if (openDialog.ShowDialog(this) == DialogResult.OK)
                {
                    foreach (var file in openDialog.FileNames)
                    {
                        var dest = Path.Combine(_rootPath ?? Environment.CurrentDirectory, Path.GetFileName(file));
                        if (File.Exists(dest) && string.Equals(dest, file, StringComparison.OrdinalIgnoreCase)) continue;
                        File.Copy(file, dest, true);
                    }

                    if (!string.IsNullOrEmpty(_rootPath))
                    {
                        LoadDirectory(_rootPath);
                    }
                    _statusLabel.Text = "已导入资源文件。";
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

            var item = _resourceService.GetResourceItem(filePath);
            var ext = Path.GetExtension(filePath).ToLower();

            if (string.Equals(ext, ".png", StringComparison.OrdinalIgnoreCase))
            {
                try
                {
                    _previewImage.Image = Image.FromFile(filePath);
                    _contentTextBox.Text = _resourceService.GetBinarySummary(filePath);
                    _statusLabel.Text = "已打开 PNG 资源：" + filePath;
                    return;
                }
                catch (Exception ex)
                {
                    _contentTextBox.Text = "读取图片失败：" + ex.Message;
                }
            }

            if (IsTextResource(ext))
            {
                try
                {
                    _previewImage.Image = null;
                    _contentTextBox.Text = File.ReadAllText(filePath, Encoding.UTF8);
                }
                catch
                {
                    _previewImage.Image = null;
                    _contentTextBox.Text = File.ReadAllText(filePath, Encoding.Default);
                }
                _statusLabel.Text = "已打开文本资源：" + filePath;
                return;
            }

            _previewImage.Image = null;
            _contentTextBox.Text = _resourceService.GetBinarySummary(filePath);
            _statusLabel.Text = "已打开二进制资源：" + filePath;
        }

        private static bool IsTextResource(string ext)
        {
            return ".lua".Equals(ext) || ".json".Equals(ext) || ".txt".Equals(ext) || ".xml".Equals(ext) ||
                   ".ini".Equals(ext) || ".csv".Equals(ext) || ".tsv".Equals(ext) || ".md".Equals(ext) ||
                   ".cfg".Equals(ext) || ".conf".Equals(ext) || ".log".Equals(ext) || ".shader".Equals(ext);
        }

        private static string FormatSize(long size)
        {
            if (size < 1024) return size + " B";
            if (size < 1024 * 1024) return (size / 1024.0).ToString("0.##") + " KB";
            return (size / (1024.0 * 1024.0)).ToString("0.##") + " MB";
        }
    }
}


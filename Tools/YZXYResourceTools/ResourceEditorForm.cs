using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace YZXYResourceTools
{
    public class ResourceEditorForm : Form
    {
        private readonly TreeView _resourceTree;
        private readonly ListView _resourceList;
        private readonly SplitContainer _splitContainer;
        private readonly TextBox _contentTextBox;
        private readonly Button _saveButton;
        private readonly Button _refreshButton;
        private readonly Label _statusLabel;

        private string _rootPath;
        private string _selectedFilePath;

        public ResourceEditorForm()
        {
            Text = "资源编辑器";
            Width = 980;
            Height = 660;
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.Sizable;

            _splitContainer = new SplitContainer();
            _resourceTree = new TreeView();
            _resourceList = new ListView();
            _contentTextBox = new TextBox();
            _saveButton = new Button();
            _refreshButton = new Button();
            _statusLabel = new Label();

            InitializeComponent();
            LoadDemoData();
        }

        public void LoadResourcePath(string path)
        {
            if (string.IsNullOrEmpty(path) || !Directory.Exists(path))
            {
                MessageBox.Show("目录不存在，无法加载资源。", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            _rootPath = path;
            Text = "资源编辑器 - " + path;
            _statusLabel.Text = "当前目录：" + path;

            _resourceTree.Nodes.Clear();
            var rootNode = new TreeNode(Path.GetFileName(path));
            rootNode.Tag = path;
            _resourceTree.Nodes.Add(rootNode);
            AddDirectoryNodes(rootNode, path);

            LoadFilesForDirectory(path);
        }

        private void InitializeComponent()
        {
            _splitContainer.Dock = DockStyle.Fill;
            _splitContainer.Orientation = Orientation.Horizontal;

            _resourceTree.Dock = DockStyle.Fill;
            _resourceTree.AfterSelect += ResourceTree_AfterSelect;

            _resourceList.Dock = DockStyle.Fill;
            _resourceList.View = View.Details;
            _resourceList.Columns.Add("名称", 220);
            _resourceList.Columns.Add("类型", 110);
            _resourceList.Columns.Add("大小", 90);
            _resourceList.FullRowSelect = true;
            _resourceList.GridLines = true;
            _resourceList.MouseDoubleClick += ResourceList_MouseDoubleClick;

            _contentTextBox.Multiline = true;
            _contentTextBox.ScrollBars = ScrollBars.Both;
            _contentTextBox.Dock = DockStyle.Fill;
            _contentTextBox.Font = new Font("Consolas", 10F);

            _saveButton.Text = "保存修改";
            _saveButton.Width = 110;
            _saveButton.Click += SaveButton_Click;

            _refreshButton.Text = "刷新";
            _refreshButton.Width = 90;
            _refreshButton.Click += RefreshButton_Click;

            var buttonPanel = new FlowLayoutPanel();
            buttonPanel.Dock = DockStyle.Bottom;
            buttonPanel.AutoSize = true;
            buttonPanel.FlowDirection = FlowDirection.LeftToRight;
            buttonPanel.Controls.Add(_refreshButton);
            buttonPanel.Controls.Add(_saveButton);

            _statusLabel.Dock = DockStyle.Bottom;
            _statusLabel.Height = 24;
            _statusLabel.Text = "就绪";

            var leftPanel = new Panel();
            leftPanel.Dock = DockStyle.Fill;
            leftPanel.Controls.Add(_resourceTree);

            var rightPanel = new Panel();
            rightPanel.Dock = DockStyle.Fill;
            rightPanel.Controls.Add(_resourceList);
            rightPanel.Controls.Add(_contentTextBox);
            rightPanel.Controls.Add(buttonPanel);
            _contentTextBox.BringToFront();

            _splitContainer.Panel1.Controls.Add(leftPanel);
            _splitContainer.Panel2.Controls.Add(rightPanel);

            Controls.Add(_splitContainer);
            Controls.Add(_statusLabel);
        }

        private void LoadDemoData()
        {
            var rootNode = new TreeNode("资源目录");
            rootNode.Nodes.Add("lua");
            rootNode.Nodes.Add("map");
            rootNode.Nodes.Add("texture");
            _resourceTree.Nodes.Add(rootNode);

            _resourceList.Items.Clear();
            _resourceList.Items.Add(new ListViewItem(new[] { "main.lua", "Lua", "12 KB" }));
            _resourceList.Items.Add(new ListViewItem(new[] { "hero.json", "JSON", "8 KB" }));
            _resourceList.Items.Add(new ListViewItem(new[] { "ui.png", "PNG", "64 KB" }));
        }

        private void ResourceTree_AfterSelect(object sender, TreeViewEventArgs e)
        {
            if (e.Node.Tag is string)
            {
                LoadFilesForDirectory((string)e.Node.Tag);
            }
            else if (e.Node.Text == "lua" || e.Node.Text == "map" || e.Node.Text == "texture")
            {
                _resourceList.Items.Clear();
                _resourceList.Items.Add(new ListViewItem(new[] { e.Node.Text + ".demo", "Demo", "0 KB" }));
            }
        }

        private void LoadFilesForDirectory(string directoryPath)
        {
            if (string.IsNullOrEmpty(directoryPath) || !Directory.Exists(directoryPath))
            {
                return;
            }

            _resourceList.Items.Clear();
            foreach (var file in Directory.GetFiles(directoryPath))
            {
                var info = new FileInfo(file);
                var type = GetFileType(file);
                var item = new ListViewItem(new[]
                {
                    Path.GetFileName(file),
                    type,
                    FormatSize(info.Length)
                });
                item.Tag = file;
                _resourceList.Items.Add(item);
            }

            _statusLabel.Text = "目录：" + directoryPath + "，文件数：" + _resourceList.Items.Count;
        }

        private void ResourceList_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (_resourceList.SelectedItems.Count == 0)
            {
                return;
            }

            var filePath = _resourceList.SelectedItems[0].Tag as string;
            if (string.IsNullOrEmpty(filePath))
            {
                return;
            }

            _selectedFilePath = filePath;
            PreviewFile(filePath);
        }

        private void PreviewFile(string filePath)
        {
            if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
            {
                return;
            }

            try
            {
                string extension = Path.GetExtension(filePath).ToLower();
                if (extension == ".lua" || extension == ".json" || extension == ".txt" || extension == ".xml" || extension == ".ini")
                {
                    var text = File.ReadAllText(filePath, Encoding.UTF8);
                    _contentTextBox.Text = text;
                    _statusLabel.Text = "正在预览：" + filePath;
                    return;
                }

                _contentTextBox.Text = "文件类型：" + GetFileType(filePath) + "\r\n\r\n" +
                    "路径：" + filePath + "\r\n" +
                    "说明：此文件是二进制/非文本资源，无法直接编辑。";
            }
            catch (Exception ex)
            {
                _contentTextBox.Text = "读取失败：" + ex.Message;
            }
        }

        private void SaveButton_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(_selectedFilePath) || !File.Exists(_selectedFilePath))
            {
                MessageBox.Show("当前没有可保存的文件。", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            try
            {
                var extension = Path.GetExtension(_selectedFilePath).ToLower();
                if (extension == ".lua" || extension == ".json" || extension == ".txt" || extension == ".xml" || extension == ".ini")
                {
                    File.WriteAllText(_selectedFilePath, _contentTextBox.Text, Encoding.UTF8);
                    MessageBox.Show("保存成功。", "完成", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    _statusLabel.Text = "已保存：" + _selectedFilePath;
                    return;
                }

                MessageBox.Show("当前文件类型不支持直接保存文本内容。", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            catch (Exception ex)
            {
                MessageBox.Show("保存失败：" + ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void RefreshButton_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(_rootPath))
            {
                LoadResourcePath(_rootPath);
            }
        }

        private void AddDirectoryNodes(TreeNode parentNode, string directoryPath)
        {
            foreach (var directory in Directory.GetDirectories(directoryPath))
            {
                var node = new TreeNode(Path.GetFileName(directory));
                node.Tag = directory;
                parentNode.Nodes.Add(node);
                AddDirectoryNodes(node, directory);
            }
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

        private static string FormatSize(long size)
        {
            if (size < 1024) return size + " B";
            if (size < 1024 * 1024) return (size / 1024.0).ToString("0.##") + " KB";
            return (size / (1024.0 * 1024.0)).ToString("0.##") + " MB";
        }
    }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace YZXYResourceTools
{
    public class ResourcePackageForm : Form
    {
        private readonly Button _browseFolderButton;
        private readonly Button _browseOutputButton;
        private readonly Button _packButton;
        private readonly ListView _packageList;
        private readonly TextBox _outputTextBox;
        private readonly Label _statusLabel;

        private string _rootPath;
        private readonly Services.ResourceService _resourceService;

        public ResourcePackageForm()
        {
            Text = "资源打包";
            Width = 820;
            Height = 560;
            StartPosition = FormStartPosition.CenterParent;

            _browseFolderButton = new Button();
            _browseOutputButton = new Button();
            _packButton = new Button();
            _packageList = new ListView();
            _outputTextBox = new TextBox();
            _statusLabel = new Label();
            _resourceService = new Services.ResourceService();

            InitializeComponent();
            LoadDemoEntries();
        }

        private void InitializeComponent()
        {
            var actionPanel = new FlowLayoutPanel();
            actionPanel.Dock = DockStyle.Top;
            actionPanel.Height = 50;

            _browseFolderButton.Text = "选择资源目录";
            _browseFolderButton.Width = 120;
            _browseFolderButton.Click += BrowseFolderButton_Click;

            _browseOutputButton.Text = "输出路径";
            _browseOutputButton.Width = 120;
            _browseOutputButton.Click += BrowseOutputButton_Click;

            _packButton.Text = "生成资源包";
            _packButton.Width = 120;
            _packButton.Click += PackButton_Click;

            _outputTextBox.Width = 400;
            _outputTextBox.Text = "resource_pack.zip";

            actionPanel.Controls.Add(_browseFolderButton);
            actionPanel.Controls.Add(_browseOutputButton);
            actionPanel.Controls.Add(_outputTextBox);
            actionPanel.Controls.Add(_packButton);

            _packageList.Dock = DockStyle.Fill;
            _packageList.View = View.Details;
            _packageList.Columns.Add("名称", 250);
            _packageList.Columns.Add("类型", 120);
            _packageList.Columns.Add("大小", 100);
            _packageList.Columns.Add("路径", 300);
            _packageList.FullRowSelect = true;
            _packageList.GridLines = true;

            _statusLabel.Dock = DockStyle.Bottom;
            _statusLabel.Height = 30;
            _statusLabel.Text = "等待选择资源目录";

            Controls.Add(actionPanel);
            Controls.Add(_packageList);
            Controls.Add(_statusLabel);
        }

        private void BrowseFolderButton_Click(object sender, EventArgs e)
        {
            using (var dialog = new FolderBrowserDialog())
            {
                dialog.Description = "选择要打包的资源目录";
                if (dialog.ShowDialog(this) == DialogResult.OK)
                {
                    _rootPath = dialog.SelectedPath;
                    LoadEntries(_rootPath);
                }
            }
        }

        private void BrowseOutputButton_Click(object sender, EventArgs e)
        {
            using (var dialog = new SaveFileDialog())
            {
                dialog.Filter = "ZIP 压缩包|*.zip";
                dialog.FileName = "resource_pack.zip";
                if (dialog.ShowDialog(this) == DialogResult.OK)
                {
                    _outputTextBox.Text = dialog.FileName;
                }
            }
        }

        private void LoadEntries(string rootPath)
        {
            _packageList.Items.Clear();
            if (string.IsNullOrEmpty(rootPath) || !Directory.Exists(rootPath))
            {
                _statusLabel.Text = "目录不存在或未选择";
                return;
            }

            var items = _resourceService.GetResourceItems(rootPath);
            foreach (var item in items)
            {
                var listItem = new ListViewItem(new[]
                {
                    item.Name,
                    item.Type,
                    FormatSize(item.SizeBytes),
                    item.FullPath
                });
                _packageList.Items.Add(listItem);
            }

            _statusLabel.Text = "已扫描：" + items.Count + " 个资源文件";
        }

        private void LoadDemoEntries()
        {
            _packageList.Items.Add(new ListViewItem(new[] { "demo.lua", "Lua", "12 KB", "./demo.lua" }));
            _packageList.Items.Add(new ListViewItem(new[] { "map_001.map", "Map", "2.1 MB", "./map/map_001.map" }));
            _packageList.Items.Add(new ListViewItem(new[] { "hero.png", "PNG", "128 KB", "./texture/hero.png" }));
        }

        private void PackButton_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(_rootPath) || !Directory.Exists(_rootPath))
            {
                MessageBox.Show("请先选择资源目录。", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var outputPath = _outputTextBox.Text.Trim();
            if (string.IsNullOrEmpty(outputPath))
            {
                MessageBox.Show("请指定输出路径。", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var dir = Path.GetDirectoryName(outputPath);
            if (!string.IsNullOrEmpty(dir))
            {
                Directory.CreateDirectory(dir);
            }

            try
            {
                bool success = _resourceService.PackageResources(_rootPath, outputPath);
                if (success)
                {
                    _statusLabel.Text = "打包成功：" + outputPath;
                    MessageBox.Show("资源包已生成。", "完成", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    _statusLabel.Text = "打包失败";
                    MessageBox.Show("打包失败，请检查目录和输出路径。", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                _statusLabel.Text = "打包异常";
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

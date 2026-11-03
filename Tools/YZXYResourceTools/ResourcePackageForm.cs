using System;
using System.IO;
using System.Windows.Forms;
using YZXYResourceTools.Services;

namespace YZXYResourceTools
{
    public class ResourcePackageForm : Form
    {
        private readonly ResourceService _resourceService;
        private readonly TextBox _sourceTextBox;
        private readonly TextBox _outputTextBox;
        private readonly ListView _filesListView;
        private readonly Label _statusLabel;

        public ResourcePackageForm()
        {
            _resourceService = new ResourceService();
            _sourceTextBox = new TextBox();
            _outputTextBox = new TextBox();
            _filesListView = new ListView();
            _statusLabel = new Label();
            InitializeComponent();
        }

        public void SetRootPath(string path)
        {
            if (string.IsNullOrWhiteSpace(path) || !Directory.Exists(path)) return;
            _sourceTextBox.Text = path;
            RefreshFiles();
        }

        private void InitializeComponent()
        {
            Text = "YZXY 批量资源打包";
            Width = 900;
            Height = 620;
            StartPosition = FormStartPosition.CenterParent;

            var sourceButton = new Button { Text = "选择资源目录", AutoSize = true };
            sourceButton.Click += SelectSource_Click;
            var outputButton = new Button { Text = "选择 ZIP 输出", AutoSize = true };
            outputButton.Click += SelectOutput_Click;
            var packButton = new Button { Text = "打包所列资源", AutoSize = true };
            packButton.Click += Pack_Click;

            _sourceTextBox.Width = 300;
            _outputTextBox.Width = 300;
            _outputTextBox.Text = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory), "yzxy-resource-pack.zip");

            var top = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 38, WrapContents = false };
            top.Controls.Add(sourceButton);
            top.Controls.Add(_sourceTextBox);
            top.Controls.Add(outputButton);
            top.Controls.Add(_outputTextBox);
            top.Controls.Add(packButton);

            _filesListView.Dock = DockStyle.Fill;
            _filesListView.View = View.Details;
            _filesListView.FullRowSelect = true;
            _filesListView.CheckBoxes = true;
            _filesListView.Columns.Add("资源", 250);
            _filesListView.Columns.Add("类型", 120);
            _filesListView.Columns.Add("大小", 100);
            _filesListView.Columns.Add("完整路径", 380);

            _statusLabel.Dock = DockStyle.Bottom;
            _statusLabel.Height = 26;
            _statusLabel.Text = "选择目录后勾选要纳入 ZIP 的文件。ZIP 是工具格式，不是游戏 WDF 格式。";

            Controls.Add(_filesListView);
            Controls.Add(top);
            Controls.Add(_statusLabel);
        }

        private void SelectSource_Click(object sender, EventArgs e)
        {
            using (var dialog = new FolderBrowserDialog())
            {
                dialog.Description = "选择需要浏览/打包的资源目录";
                if (dialog.ShowDialog(this) == DialogResult.OK) SetRootPath(dialog.SelectedPath);
            }
        }

        private void SelectOutput_Click(object sender, EventArgs e)
        {
            using (var dialog = new SaveFileDialog { Filter = "ZIP 压缩包 (*.zip)|*.zip", FileName = "yzxy-resource-pack.zip" })
            {
                if (dialog.ShowDialog(this) == DialogResult.OK) _outputTextBox.Text = dialog.FileName;
            }
        }

        private void RefreshFiles()
        {
            _filesListView.Items.Clear();
            var root = _sourceTextBox.Text.Trim();
            if (!Directory.Exists(root)) return;

            foreach (var item in _resourceService.GetResourceItems(root))
            {
                var row = new ListViewItem(item.Name) { Checked = true, Tag = item.FullPath };
                row.SubItems.Add(item.Type);
                row.SubItems.Add(item.SizeBytes.ToString("N0") + " B");
                row.SubItems.Add(item.FullPath);
                _filesListView.Items.Add(row);
            }
            _statusLabel.Text = "扫描完成：" + _filesListView.Items.Count + " 个文件。ZIP 输出不会生成游戏可直接加载的 WDF 包。";
        }

        private void Pack_Click(object sender, EventArgs e)
        {
            var root = _sourceTextBox.Text.Trim();
            var output = _outputTextBox.Text.Trim();
            if (!Directory.Exists(root))
            {
                MessageBox.Show("请先选择有效的资源目录。", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (string.IsNullOrWhiteSpace(output))
            {
                MessageBox.Show("请指定 ZIP 输出路径。", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                var selected = new System.Collections.Generic.List<string>();
                foreach (ListViewItem row in _filesListView.Items)
                    if (row.Checked && row.Tag is string) selected.Add((string)row.Tag);
                if (selected.Count == 0)
                {
                    MessageBox.Show("没有勾选任何资源文件。", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                _resourceService.PackageFiles(root, selected, output);
                _statusLabel.Text = "已打包 " + selected.Count + " 个文件：" + output;
                MessageBox.Show("ZIP 资源包已生成。注意：这是 ZIP 容器，不是游戏 WDF 格式。", "完成", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("打包失败：" + ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}

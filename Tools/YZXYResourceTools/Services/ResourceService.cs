using System;
using System.Drawing;
using System.Windows.Forms;

namespace YZXYResourceTools
{
    public class ResourcePackageForm : Form
    {
        private ListView packageList;
        private Button packageButton;
        private Label infoLabel;

        public ResourcePackageForm()
        {
            this.Text = "资源打包";
            this.Width = 700;
            this.Height = 500;
            this.StartPosition = FormStartPosition.CenterParent;

            InitializeComponent();
            LoadDemoPackageEntries();
        }

        private void InitializeComponent()
        {
            packageList = new ListView();
            packageButton = new Button();
            infoLabel = new Label();

            packageList.Dock = DockStyle.Fill;
            packageList.View = View.Details;
            packageList.Columns.Add("资源文件", 260);
            packageList.Columns.Add("类型", 120);
            packageList.Columns.Add("状态", 120);
            packageList.FullRowSelect = true;
            packageList.GridLines = true;

            packageButton.Text = "开始打包";
            packageButton.Width = 140;
            packageButton.Height = 36;
            packageButton.Click += PackageButton_Click;

            infoLabel.Text = "通过此界面统一管理资源压缩、导出和打包";
            infoLabel.Dock = DockStyle.Bottom;
            infoLabel.Height = 30;

            var panel = new Panel();
            panel.Dock = DockStyle.Fill;
            panel.Controls.Add(packageList);
            panel.Controls.Add(packageButton);

            packageButton.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            packageButton.Location = new Point(this.Width - 180, this.Height - 80);

            this.Controls.Add(panel);
            this.Controls.Add(infoLabel);
        }

        private void LoadDemoPackageEntries()
        {
            packageList.Items.Add(new ListViewItem(new[] { "lua/main.lua", "Lua", "已准备" }));
            packageList.Items.Add(new ListViewItem(new[] { "data/hero.json", "JSON", "已准备" }));
            packageList.Items.Add(new ListViewItem(new[] { "texture/ui.png", "PNG", "已准备" }));
            packageList.Items.Add(new ListViewItem(new[] { "map/map_001.map", "Map", "已准备" }));
        }

        private void PackageButton_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < packageList.Items.Count; i++)
            {
                packageList.Items[i].SubItems[2].Text = "打包中";
            }

            for (int i = 0; i < packageList.Items.Count; i++)
            {
                packageList.Items[i].SubItems[2].Text = "已完成";
            }

            MessageBox.Show("资源打包已完成。", "打包结果", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }
}

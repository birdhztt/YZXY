using System;
using System.Drawing;
using System.Windows.Forms;

namespace YZXYResourceTools
{
    public class MainForm : Form
    {
        private MenuStrip menuStrip;
        private ToolStripMenuItem fileMenu;
        private ToolStripMenuItem newMenu;
        private ToolStripMenuItem openResourceMenu;
        private ToolStripMenuItem exitMenu;
        private ToolStripMenuItem viewMenu;
        private ToolStripMenuItem cascadeMenu;
        private ToolStripMenuItem tileHorizontalMenu;
        private ToolStripMenuItem tileVerticalMenu;
        private ToolStripMenuItem toolsMenu;
        private ToolStripMenuItem packageMenu;
        private ToolStripMenuItem helpMenu;
        private ToolStripMenuItem aboutMenu;

        public MainForm()
        {
            InitializeComponent();
            this.Text = "YZXY 资源工具组";
            this.WindowState = FormWindowState.Maximized;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.IsMdiContainer = true;
            this.BackColor = Color.FromArgb(42, 42, 42);
        }

        private void InitializeComponent()
        {
            this.menuStrip = new MenuStrip();
            this.fileMenu = new ToolStripMenuItem("文件(F)");
            this.newMenu = new ToolStripMenuItem("新建资源编辑器");
            this.openResourceMenu = new ToolStripMenuItem("打开资源目录");
            this.exitMenu = new ToolStripMenuItem("退出");

            this.viewMenu = new ToolStripMenuItem("窗口(V)");
            this.cascadeMenu = new ToolStripMenuItem("层叠");
            this.tileHorizontalMenu = new ToolStripMenuItem("横向平铺");
            this.tileVerticalMenu = new ToolStripMenuItem("纵向平铺");

            this.toolsMenu = new ToolStripMenuItem("工具(T)");
            this.packageMenu = new ToolStripMenuItem("资源打包");

            this.helpMenu = new ToolStripMenuItem("帮助(H)");
            this.aboutMenu = new ToolStripMenuItem("关于");

            this.menuStrip.Items.Add(fileMenu);
            this.menuStrip.Items.Add(viewMenu);
            this.menuStrip.Items.Add(toolsMenu);
            this.menuStrip.Items.Add(helpMenu);

            fileMenu.DropDownItems.Add(newMenu);
            fileMenu.DropDownItems.Add(openResourceMenu);
            fileMenu.DropDownItems.Add(new ToolStripSeparator());
            fileMenu.DropDownItems.Add(exitMenu);

            viewMenu.DropDownItems.Add(cascadeMenu);
            viewMenu.DropDownItems.Add(tileHorizontalMenu);
            viewMenu.DropDownItems.Add(tileVerticalMenu);

            toolsMenu.DropDownItems.Add(packageMenu);
            helpMenu.DropDownItems.Add(aboutMenu);

            this.Controls.Add(menuStrip);
            this.MainMenuStrip = menuStrip;

            this.newMenu.Click += NewMenu_Click;
            this.openResourceMenu.Click += OpenResourceMenu_Click;
            this.exitMenu.Click += ExitMenu_Click;
            this.cascadeMenu.Click += CascadeMenu_Click;
            this.tileHorizontalMenu.Click += TileHorizontalMenu_Click;
            this.tileVerticalMenu.Click += TileVerticalMenu_Click;
            this.packageMenu.Click += PackageMenu_Click;
            this.aboutMenu.Click += AboutMenu_Click;
        }

        private void NewMenu_Click(object sender, EventArgs e)
        {
            var editor = new ResourceEditorForm();
            editor.MdiParent = this;
            editor.Show();
        }

        private void OpenResourceMenu_Click(object sender, EventArgs e)
        {
            using (var dialog = new FolderBrowserDialog())
            {
                dialog.Description = "选择资源目录";
                if (dialog.ShowDialog(this) == DialogResult.OK)
                {
                    var editor = new ResourceEditorForm();
                    editor.MdiParent = this;
                    editor.LoadResourcePath(dialog.SelectedPath);
                    editor.Show();
                }
            }
        }

        private void ExitMenu_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void CascadeMenu_Click(object sender, EventArgs e)
        {
            this.LayoutMdi(MdiLayout.Cascade);
        }

        private void TileHorizontalMenu_Click(object sender, EventArgs e)
        {
            this.LayoutMdi(MdiLayout.TileHorizontal);
        }

        private void TileVerticalMenu_Click(object sender, EventArgs e)
        {
            this.LayoutMdi(MdiLayout.TileVertical);
        }

        private void PackageMenu_Click(object sender, EventArgs e)
        {
            var packageForm = new ResourcePackageForm();
            packageForm.MdiParent = this;
            packageForm.Show();
        }

        private void AboutMenu_Click(object sender, EventArgs e)
        {
            MessageBox.Show(
                "YZXY 资源工具组\n\n基于 C# WinForms + .NET Framework 4.8\n采用 MDI 多文档布局",
                "关于",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }
    }
}

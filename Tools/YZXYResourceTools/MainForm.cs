using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace YZXYResourceTools
{
    public class MainForm : Form
    {
        private readonly MenuStrip _menuStrip;
        private readonly ToolStripMenuItem _fileMenu;
        private readonly ToolStripMenuItem _newEditorMenu;
        private readonly ToolStripMenuItem _openResourceMenu;
        private readonly ToolStripMenuItem _exitMenu;
        private readonly ToolStripMenuItem _viewMenu;
        private readonly ToolStripMenuItem _cascadeMenu;
        private readonly ToolStripMenuItem _tileHorizontalMenu;
        private readonly ToolStripMenuItem _tileVerticalMenu;
        private readonly ToolStripMenuItem _toolsMenu;
        private readonly ToolStripMenuItem _packageMenu;
        private readonly ToolStripMenuItem _helpMenu;
        private readonly ToolStripMenuItem _aboutMenu;

        public MainForm()
        {
            InitializeComponent();
            Text = "YZXY 资源工具组";
            WindowState = FormWindowState.Maximized;
            StartPosition = FormStartPosition.CenterScreen;
            IsMdiContainer = true;
            BackColor = Color.FromArgb(42, 42, 42);
        }

        private void InitializeComponent()
        {
            _menuStrip = new MenuStrip();
            _fileMenu = new ToolStripMenuItem("文件(F)");
            _newEditorMenu = new ToolStripMenuItem("新建资源编辑器");
            _openResourceMenu = new ToolStripMenuItem("打开资源目录");
            _exitMenu = new ToolStripMenuItem("退出");

            _viewMenu = new ToolStripMenuItem("窗口(V)");
            _cascadeMenu = new ToolStripMenuItem("层叠");
            _tileHorizontalMenu = new ToolStripMenuItem("横向平铺");
            _tileVerticalMenu = new ToolStripMenuItem("纵向平铺");

            _toolsMenu = new ToolStripMenuItem("工具(T)");
            _packageMenu = new ToolStripMenuItem("资源打包");

            _helpMenu = new ToolStripMenuItem("帮助(H)");
            _aboutMenu = new ToolStripMenuItem("关于");

            _menuStrip.Items.Add(_fileMenu);
            _menuStrip.Items.Add(_viewMenu);
            _menuStrip.Items.Add(_toolsMenu);
            _menuStrip.Items.Add(_helpMenu);

            _fileMenu.DropDownItems.Add(_newEditorMenu);
            _fileMenu.DropDownItems.Add(_openResourceMenu);
            _fileMenu.DropDownItems.Add(new ToolStripSeparator());
            _fileMenu.DropDownItems.Add(_exitMenu);

            _viewMenu.DropDownItems.Add(_cascadeMenu);
            _viewMenu.DropDownItems.Add(_tileHorizontalMenu);
            _viewMenu.DropDownItems.Add(_tileVerticalMenu);

            _toolsMenu.DropDownItems.Add(_packageMenu);
            _helpMenu.DropDownItems.Add(_aboutMenu);

            Controls.Add(_menuStrip);
            MainMenuStrip = _menuStrip;

            _newEditorMenu.Click += NewMenu_Click;
            _openResourceMenu.Click += OpenResourceMenu_Click;
            _exitMenu.Click += ExitMenu_Click;
            _cascadeMenu.Click += CascadeMenu_Click;
            _tileHorizontalMenu.Click += TileHorizontalMenu_Click;
            _tileVerticalMenu.Click += TileVerticalMenu_Click;
            _packageMenu.Click += PackageMenu_Click;
            _aboutMenu.Click += AboutMenu_Click;
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
            LayoutMdi(MdiLayout.Cascade);
        }

        private void TileHorizontalMenu_Click(object sender, EventArgs e)
        {
            LayoutMdi(MdiLayout.TileHorizontal);
        }

        private void TileVerticalMenu_Click(object sender, EventArgs e)
        {
            LayoutMdi(MdiLayout.TileVertical);
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
                "YZXY 资源工具组\n\n基于 C# WinForms + .NET Framework 4.8\n采用 MDI 多文档布局\n支持资源目录扫描、预览、编辑和打包。",
                "关于",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }
    }
}

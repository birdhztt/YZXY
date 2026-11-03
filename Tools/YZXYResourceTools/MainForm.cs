using System;
using System.Drawing;
using System.Windows.Forms;

namespace YZXYResourceTools
{
    public class MainForm : Form
    {
        private MenuStrip _menuStrip;
        private ToolStripMenuItem _fileMenu;
        private ToolStripMenuItem _newEditorMenu;
        private ToolStripMenuItem _openResourceMenu;
        private ToolStripMenuItem _exitMenu;
        private ToolStripMenuItem _viewMenu;
        private ToolStripMenuItem _cascadeMenu;
        private ToolStripMenuItem _tileHorizontalMenu;
        private ToolStripMenuItem _tileVerticalMenu;
        private ToolStripMenuItem _toolsMenu;
        private ToolStripMenuItem _packageMenu;
        private ToolStripMenuItem _helpMenu;
        private ToolStripMenuItem _aboutMenu;

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
            _packageMenu = new ToolStripMenuItem("资源包浏览/批量打包");
            _helpMenu = new ToolStripMenuItem("帮助(H)");
            _aboutMenu = new ToolStripMenuItem("关于");

            _fileMenu.DropDownItems.AddRange(new ToolStripItem[] { _newEditorMenu, _openResourceMenu, new ToolStripSeparator(), _exitMenu });
            _viewMenu.DropDownItems.AddRange(new ToolStripItem[] { _cascadeMenu, _tileHorizontalMenu, _tileVerticalMenu });
            _toolsMenu.DropDownItems.Add(_packageMenu);
            _helpMenu.DropDownItems.Add(_aboutMenu);
            _menuStrip.Items.AddRange(new ToolStripItem[] { _fileMenu, _viewMenu, _toolsMenu, _helpMenu });
            Controls.Add(_menuStrip);
            MainMenuStrip = _menuStrip;

            _newEditorMenu.Click += delegate { ShowEditor(null); };
            _openResourceMenu.Click += OpenResourceMenu_Click;
            _exitMenu.Click += delegate { Close(); };
            _cascadeMenu.Click += delegate { LayoutMdi(MdiLayout.Cascade); };
            _tileHorizontalMenu.Click += delegate { LayoutMdi(MdiLayout.TileHorizontal); };
            _tileVerticalMenu.Click += delegate { LayoutMdi(MdiLayout.TileVertical); };
            _packageMenu.Click += delegate { ShowPackageForm(null); };
            _aboutMenu.Click += delegate
            {
                MessageBox.Show("YZXY 资源工具组\nC# WinForms / .NET Framework 4.8\nZIP 打包不等同于游戏 WDF 打包。", "关于", MessageBoxButtons.OK, MessageBoxIcon.Information);
            };
        }

        private void OpenResourceMenu_Click(object sender, System.EventArgs e)
        {
            using (var dialog = new FolderBrowserDialog { Description = "选择 YZXY 项目或资源目录" })
            {
                if (dialog.ShowDialog(this) == DialogResult.OK) ShowEditor(dialog.SelectedPath);
            }
        }

        private void ShowEditor(string path)
        {
            var editor = new ResourceEditorForm { MdiParent = this };
            if (!string.IsNullOrWhiteSpace(path)) editor.LoadResourcePath(path);
            editor.Show();
        }

        private void ShowPackageForm(string path)
        {
            var form = new ResourcePackageForm { MdiParent = this };
            if (!string.IsNullOrWhiteSpace(path)) form.SetRootPath(path);
            form.Show();
        }
    }
}

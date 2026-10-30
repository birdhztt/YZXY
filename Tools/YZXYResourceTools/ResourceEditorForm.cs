using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;

namespace YZXYResourceTools
{
    public class ResourceEditorForm : Form
    {
        private TreeView resourceTree;
        private ListView resourceList;
        private SplitContainer splitContainer;
        private TextBox contentTextBox;
        private Label statusLabel;

        public ResourceEditorForm()
        {
            this.Text = "资源编辑器";
            this.Width = 900;
            this.Height = 620;
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.Sizable;

            InitializeComponent();
            LoadDemoData();
        }

        public void LoadResourcePath(string path)
        {
            this.Text = "资源编辑器 - " + path;
            statusLabel.Text = "当前目录：" + path;

            if (Directory.Exists(path))
            {
                resourceTree.Nodes.Clear();
                var rootNode = new TreeNode(Path.GetFileName(path));
                rootNode.Tag = path;
                resourceTree.Nodes.Add(rootNode);
                AddDirectoryNodes(rootNode, path);
            }
        }

        private void InitializeComponent()
        {
            splitContainer = new SplitContainer();
            resourceTree = new TreeView();
            resourceList = new ListView();
            contentTextBox = new TextBox();
            statusLabel = new Label();

            splitContainer.Dock = DockStyle.Fill;
            splitContainer.Orientation = Orientation.Vertical;

            resourceTree.Dock = DockStyle.Fill;
            resourceTree.AfterSelect += ResourceTree_AfterSelect;

            resourceList.Dock = DockStyle.Fill;
            resourceList.View = View.Details;
            resourceList.Columns.Add("名称", 180);
            resourceList.Columns.Add("类型", 120);
            resourceList.Columns.Add("大小", 90);
            resourceList.FullRowSelect = true;
            resourceList.GridLines = true;
            resourceList.MouseDoubleClick += ResourceList_MouseDoubleClick;

            contentTextBox.Multiline = true;
            contentTextBox.ScrollBars = ScrollBars.Both;
            contentTextBox.Dock = DockStyle.Fill;
            contentTextBox.Font = new System.Drawing.Font("Consolas", 10F);

            statusLabel.Dock = DockStyle.Bottom;
            statusLabel.Height = 25;
            statusLabel.Text = "就绪";

            splitContainer.Panel1.Controls.Add(resourceTree);
            splitContainer.Panel2.Controls.Add(resourceList);
            splitContainer.Panel2.Controls.Add(contentTextBox);

            this.Controls.Add(splitContainer);
            this.Controls.Add(statusLabel);
        }

        private void LoadDemoData()
        {
            var rootNode = new TreeNode("资源目录");
            rootNode.Nodes.Add("lua");
            rootNode.Nodes.Add("map");
            rootNode.Nodes.Add("texture");
            resourceTree.Nodes.Add(rootNode);

            resourceList.Items.Clear();
            resourceList.Items.Add(new ListViewItem(new[] { "main.lua", "Lua", "12 KB" }));
            resourceList.Items.Add(new ListViewItem(new[] { "hero.json", "JSON", "8 KB" }));
            resourceList.Items.Add(new ListViewItem(new[] { "ui.png", "PNG", "64 KB" }));
        }

        private void ResourceTree_AfterSelect(object sender, TreeViewEventArgs e)
        {
            if (e.Node.Text == "lua")
            {
                resourceList.Items.Clear();
                resourceList.Items.Add(new ListViewItem(new[] { "main.lua", "Lua", "12 KB" }));
                resourceList.Items.Add(new ListViewItem(new[] { "skill.lua", "Lua", "24 KB" }));
            }
            else if (e.Node.Text == "map")
            {
                resourceList.Items.Clear();
                resourceList.Items.Add(new ListViewItem(new[] { "map_001.map", "Map", "2.1 MB" }));
            }
            else if (e.Node.Text == "texture")
            {
                resourceList.Items.Clear();
                resourceList.Items.Add(new ListViewItem(new[] { "icon.png", "PNG", "32 KB" }));
                resourceList.Items.Add(new ListViewItem(new[] { "hero.png", "PNG", "128 KB" }));
            }
        }

        private void ResourceList_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (resourceList.SelectedItems.Count == 0)
            {
                return;
            }

            var item = resourceList.SelectedItems[0];
            contentTextBox.Text = "# " + item.SubItems[0].Text + "\n\n" +
                "文件类型：" + item.SubItems[1].Text + "\n" +
                "文件大小：" + item.SubItems[2].Text + "\n\n" +
                "这里是资源文件内容预览区，可继续扩展读取/编辑逻辑。";
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
    }
}

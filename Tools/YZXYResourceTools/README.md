# YZXY 资源工具组

这是一个基于 C# WinForms + .NET Framework 4.8 的多文档资源管理工具，采用 MDI (Multiple Document Interface) 布局，适合用于：

- 读取资源目录
- 浏览和筛选资源文件
- 预览脚本/文本资源
- 编辑可文本资源
- 生成 ZIP 压缩资源包

## 目录结构

- `YZXYResourceTools.csproj`：项目文件
- `Program.cs`：应用入口
- `MainForm.cs`：主窗体，负责 MDI 容器和菜单
- `ResourceEditorForm.cs`：资源编辑器子窗体
- `ResourcePackageForm.cs`：资源打包子窗体
- `Models/ResourceItem.cs`：资源文件模型
- `Services/ResourceService.cs`：资源扫描和打包逻辑

## 当前能力

1. 打开任意资源目录
2. 自动递归扫描资源文件
3. 资源树和文件列表浏览
4. .lua / .json / .txt / .xml 等文本文件直接预览和保存
5. 二进制资源提示不可直接编辑
6. 将整个资源目录打包成 ZIP

## 运行方式

1. 使用 Visual Studio 2022 或 2019 打开 `Tools/YZXYResourceTools/YZXYResourceTools.csproj`
2. 确认目标框架为 `.NET Framework 4.8`
3. 构建并运行

## 后续扩展建议

- 扩展资源格式支持：PNG / Atlas / Map / Binary
- 增加右键菜单：打开、复制路径、导出、重命名
- 增加资源同步/导入导出功能
- 接入 YZXY 实际游戏资源目录与打包格式


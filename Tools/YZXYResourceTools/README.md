# YZXY Resource Tool Suite

这是一个基于 C# WinForms + .NET Framework 4.8 的工具组骨架，采用 MDI 多文档界面设计，适合后续扩展为：

- 资源读取工具
- 资源编辑工具
- 资源打包/导出工具
- 资源预览和批量处理

## 目录结构

- `YZXYResourceTools.csproj`：项目文件
- `Program.cs`：入口文件
- `MainForm.cs`：主窗体，负责 MDI 布局和菜单
- `ResourceEditorForm.cs`：资源编辑器子窗体
- `ResourcePackageForm.cs`：资源打包子窗体
- `Models/ResourceItem.cs`：资源数据模型
- `Services/ResourceService.cs`：资源管理服务

## 功能说明

1. 主窗口使用 MDI 容器
2. 支持多文档同屏显示
3. 每个子窗口可以独立打开、移动、关闭
4. 可作为后续游戏资源读取、编辑、打包的统一入口

## 运行方式

1. 使用 Visual Studio 2019/2022 打开项目
2. 目标框架选择 .NET Framework 4.8
3. 构建并运行

## 后续扩展建议

- 资源文件扫描与列表展示
- 资源导入/导出功能
- Lua / XML / JSON / 图片资源支持
- 批量打包器与加密/压缩选项


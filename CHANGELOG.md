# 更改日志 (CHANGELOG)

本文档记录了自提交 `ea73facda61fd47d87100f05067b35fa4995850f` 以来的所有重要更改。

## 概述

**统计信息:**
- 总计提交数: 42 个提交
- 修改文件数: 853 个文件
- 新增代码行: 100,042 行
- 删除代码行: 315 行
- 时间跨度: 约 10 个月 (从初始提交到最新的清理工作)

## 主要变更分类

### 1. 命名空间重构 (Namespace Refactoring)

#### 1.1 命名空间改为 Fs.Fox (bf189c7)
**日期**: 10个月前  
**作者**: ZhangChengbo

- 将所有命名空间从 `IFoxCAD` 改为 `Fs.Fox`
- 目的：
  - 便于Fs团队在生产环境中使用
  - 避免DLL版本冲突
  - 更好地维护和开发此类库
- 影响范围：涉及约100+文件的命名空间修改
- 主要修改的文件类型：
  - 所有 `.cs` 源文件
  - 项目配置文件
  - 文档文件

**修改的文件包括但不限于：**
- 算法相关: `QuadTree` 系列文件
- 扩展方法: 所有 `ExtensionMethod` 目录下的文件
- 实体相关: `Entity/ArcEx.cs`, `Entity/CircleEx.cs`, `Entity/CurveEx.cs` 等
- 几何相关: `Geomerty` 目录下的文件
- 初始化相关: `Initialize` 目录下的文件
- PE相关: `PE` 目录下的文件
- 选择过滤器: `SelectionFilter` 目录下的文件

#### 1.2 项目文件重命名 (3850558)
**日期**: 10个月前

- 项目文件从 `IFoxCAD.AutoCad.csproj` 重命名为 `Fs.Fox.AutoCad.csproj`
- 更新解决方案文件引用
- 添加 WindowsAPI.cs 的部分修改

### 2. 项目结构调整

#### 2.1 分支说明文档 (6bab122, 70104a4)
**日期**: 约 9-10个月前

- 创建 `Fs分支说明.md` 和更新 `README.md`
- 说明了本分支与IFoxCAD官方的关系
- 添加了团队使用说明和在线导图链接
- 文档化了分支维护策略

#### 2.2 结构调整 (8c1984d, 71ebd89, 6f2b379, f288f4a)
**日期**: 约 9个月前

- 项目结构优化
- Readme.md 调整
- 从 v0.9 分支合并代码

### 3. 新增功能和扩展

#### 3.1 BlockView.NET 项目 (d81155f, 28f70bf, b47a8cf)
**日期**: 3个月前  
**来源**: https://github.com/15831944/CADDev/tree/master

新增了 BlockView.NET 项目，包含：
- `BlockView.NET.csproj` - 项目文件
- `BlockViewDialog.cs` 和 `BlockViewDialog.Designer.cs` - 对话框界面 (473行代码)
- `Commands.cs` - 命令定义 (54行代码)
- `GsPreviewCtrl.cs` - 图形预览控件 (1298行代码)
- `EnumToolStripMenuItem.cs` - 枚举菜单项 (150行代码)
- `Helpers.cs` - 辅助工具类 (91行代码)
- `Properties/AssemblyInfo.cs` - 程序集信息

**后续更新:**
- 更新项目到 .NET Framework 4.8 和 VS 2017 (28f70bf)
- 清理未使用的 using 指令 (b47a8cf)

#### 3.2 几何扩展方法库 (e85b090)
**日期**: 3个月前

复制并添加了大量几何扩展方法（共约3,747行新增代码）：
- `CircularArc2dExtensions.cs` (172行)
- `CircularArc3dExtensions.cs` (102行)
- `EditorExtensions.cs` (131行)
- `EllipseExtensions.cs` (71行)
- `GeomExt.cs` (143行)
- `Point2dCollectionExtensions.cs` (118行)
- `Point2dExtensions.cs` (113行)
- `Point3dCollectionExtensions.cs` (148行)
- `Point3dExtensions.cs` (228行)
- `Polyline2dExtensions.cs` (235行)
- `Polyline3dExtensions.cs` (39行)
- `PolylineExtensions.cs` (177行)
- `PolylineSegment.cs` (323行)
- `PolylineSegmentCollection.cs` (590行)
- `RegionExtensions.cs` (25行)
- `SplineExtensions.cs` (47行)
- `Triangle.cs` (234行)
- `Triangle2d.cs` (316行)
- `Triangle3d.cs` (340行)
- `Vector3dExtensions.cs` (20行)
- `ViewportExtensions.cs` (61行)

#### 3.3 ThMEP.Core 类库集成 (de43cd4)
**日期**: 3个月前  
**来源**: https://github.com/SBESIRC/ThMEP.Core/tree/main

新增了大量来自网上的类库合集，包含多个子项目：

**CADExtension 扩展库:**
- 数据库服务扩展: `DatabaseServices` 系列
- 编辑器输入: `EditorInput` 系列
- 运行时支持: `Runtime` 系列
- 几何扩展: `Geometry` 系列
- 实体扩展: 各种CAD实体的扩展方法

**ThCADExtension 扩展库:**
- `ThCurveExtension.cs` - 曲线扩展
- `ThEditorExtension.cs` - 编辑器扩展
- `ThPolylineExtension.cs` - 多段线扩展
- `ThStringTools.cs` - 字符串工具（数字转中文等）
- `ThGeExtension.cs` - 几何扩展
- `ThHatchTool.cs` - 填充工具
- `ThLayerTool.cs` - 图层工具
- 等等...

**ThMEPEngineCore 核心引擎:**
- 算法模块: 中心线服务、碰撞检测、多边形分割、三角剖分等
- 梁信息处理: BeamInfo 相关业务逻辑
- CAD工具: 识别引擎、绘图工具、几何工具
- 各种提取和识别引擎

**GeometryExtensions 几何扩展:**
- `Arc2d.cs`, `Arc3d.cs` - 圆弧扩展
- `Circle2d.cs`, `Circle3d.cs` - 圆扩展
- `Ellipse2d.cs`, `Ellipse3d.cs` - 椭圆扩展
- `Line2d.cs`, `Line3d.cs` - 线段扩展
- `Polygon2d.cs`, `Polygon3d.cs` - 多边形
- `Triangle.cs`, `Triangle2d.cs`, `Triangle3d.cs` - 三角形
- 等等...

**统计**: 新增约 20,000+ 行代码

### 4. 代码重构和优化

#### 4.1 边界框重构 (5148bd3)
**日期**: 3个月前

- 将 `BoundingInfo.cs` 重命名为 `BoundingBox9.cs`
- 重构边界框结构和引用关系
- 优化了边界框的实现逻辑
- 修改的文件:
  - `CADShared/ExtensionMethod/Entity/BoundingBox9.cs`
  - `CADShared/ExtensionMethod/Entity/EntityBoundingInfo.cs`
  - `CADShared/ExtensionMethod/Entity/EntityEx.cs`

#### 4.2 CurveEx 方法重命名 (796bf97)
**日期**: 3个月前

- `CurveEx` 类中的方法重命名为 `GetCurveLength`
- 提高了方法名称的清晰度和一致性

### 5. 构建和部署 (CI/CD)

#### 5.1 调试配置优化 (2c7f0fb)
**日期**: 3个月前

- FoxCAD 增加 `x64_2019_Debug` 配置项
- 输出到 DiG Build 路径，方便调试
- 避免与原 CI/CD 流程冲突

#### 5.2 GitHub Actions 工作流 (bac580b, c752201, ab07868, 等)
**日期**: 约 7-8个月前

新增和完善 CI/CD 流程：
- 添加 GitHub 工作流程配置
- 新增问题和拉取请求模板
- 完善构建与部署文档说明
- 更新 NuGet 包恢复过程
- 添加 CI 构建参数并简化文件复制逻辑
- 引入部署标志机制
- 更新工作流以使用 Build 仓库

**相关提交:**
- `bac580b` - 新增 GitHub 工作流程和模板
- `c752201` - 更新构建和部署配置
- `ab07868` - 更新 NuGet 包恢复过程
- `994e5d8` - 更新 CustomCopyOutputForNet48 目标的复制逻辑
- `74c24cf` - 添加 CI 构建参数并简化文件复制逻辑
- `f5026e0` - 更新工作流以使用 Build 仓库
- `496822b` - 引入部署标志机制
- `109fc5b` - 添加目标仓库检出和 NuGet 包恢复步骤
- `373dcb7` - 更新 workflows markdown 说明
- 测试提交: `faf4c5a`, `ebbd051`, `50c8f4a`, `641aaa4` - 带 [deploy] 标记的测试

#### 5.3 构建脚本 (89b995b)
**日期**: 约 9个月前

- 增加复制到 `Build\DiGLib\DiGArchBase\x64_2019_Release\Fs.Fox.AutoCad.dll` 的逻辑
- 创建 `copyToDigBuild.ps1` PowerShell 脚本

### 6. 文档和资源

#### 6.1 在线导图 (1dad4c2)
**日期**: 约 9个月前

- 基于公开文档草稿整理了思维导图
- boardmix 链接中的文件「.NET ARX_Fox」
- 链接: https://boardmix.cn/app/share/CAE.CMvmgA4gASoQHBGpsUGmGR9LipooomyTSDAGQAE/U41nx2

#### 6.2 项目配置和文档说明 (27a052d, 2bce0f4)
**日期**: 约 7-8个月前

- 更新项目配置
- 更新 README.md
- 完善文档说明

### 7. 测试和修复

#### 7.1 FsCAD 同步 (079f24f)
**日期**: 约 9个月前

- 同步 FsCAD 的更新

#### 7.2 测试命令优化 (beba816)
**日期**: 约 10个月前

- 删除测试命令中的无效空格
- 增加 Test_Rec 的注释

### 8. 其他更改

#### 8.1 无逻辑更改 (ee31c0a)
**日期**: 约 9个月前

- 代码格式化和清理，无功能性更改

#### 8.2 版本合并 (a094aba, 0, f288f4a)
**日期**: 约 9个月前

- 从 v0.9 分支合并代码
- 各种小的调整和修复

## 详细提交列表

### 最近 3 个月的提交 (2024年7月至2024年10月)

1. **b47a8cf** - 清理未使用的 using 指令
2. **28f70bf** - Update project to .NET Framework 4.8 and VS 2017
3. **d81155f** - 增加 BlockView.NET.csproj 来自外部项目
4. **796bf97** - CurveEx 类方法重命名为 GetCurveLength
5. **5148bd3** - Refactor bounding box structure and references
6. **2c7f0fb** - FoxCAD增加 x64_2019_Debug 配置项
7. **e85b090** - Copy了一些几何扩展方法
8. **de43cd4** - 网上找到一个类库的合集集成

### 6-9 个月前的提交 (2024年1月至2024年7月)

9. **2bce0f4** - Update README.md
10. **641aaa4** - 测试通过 [deploy]
11. **50c8f4a** - [deploy]
12. **ebbd051** - 测试 [deploy]
13. **b0cc06b** - [deploy]
14. **373dcb7** - 更新 workflows markdown 说明
15. **faf4c5a** - 测试 [deploy]
16. **136e67e** - 添加目标仓库检出和 NuGet 包恢复步骤
17. **109fc5b** - 00
18. **496822b** - 引入部署标志机制到 GitHub Actions
19. **f5026e0** - 更新工作流以使用 Build 仓库
20. **74c24cf** - 添加 CI 构建参数并简化文件复制逻辑
21. **994e5d8** - 更新 CustomCopyOutputForNet48 目标的复制逻辑
22. **ab07868** - 更新 NuGet 包恢复过程
23. **c752201** - 更新构建和部署配置
24. **bac580b** - 新增 GitHub 工作流程和问题、拉取请求模板
25. **27a052d** - 更新项目配置和文档说明
26. **ee31c0a** - 无逻辑更改
27. **89b995b** - 增加 copy Build\DiGLib\DiGArchBase 路径
28. **1dad4c2** - 思维导图链接
29. **6bab122** - Fs.Fox 分支说明
30. **70104a4** - README.md
31. **079f24f** - 同步 FsCAD 的更新
32. **a094aba** - 00
33. **8c1984d** - 调整结构
34. **71ebd89** - 调整了 Readme.md
35. **6f2b379** - 0
36. **f288f4a** - Merge branch 'v0.9' into Fs0.9
37. **beba816** - 删除测试命令中无效空格，增加注释

### 约 10 个月前的提交 (初始化阶段)

38. **89c9638** - 在 FoxCAD 的基础上改为 Fs.Fox 命名空间（主要变更说明）
39. **3850558** - 项目文件重命名
40. **bf189c7** - 将命名空间改为 Fs.Fox（大规模重构）

## 技术债务和 TODO

从 README.md 中提取的待办事项：

- [ ] 评估第三方 IndexRange 包，是否要清理
  - C# 原生 IndexRange 是在 Standard 2.1 引入的
  - Framework 4.8 仅支持 Standard 2.0

## 影响评估

### 优点
1. **独立性**: 通过命名空间更改，实现了与 IFoxCAD 的独立维护
2. **功能增强**: 新增了大量扩展方法和工具类
3. **自动化**: 完善的 CI/CD 流程
4. **文档化**: 较为完整的分支说明和在线导图

### 需要注意的地方
1. **兼容性**: 命名空间更改意味着与原 IFoxCAD 不兼容
2. **维护成本**: 集成了大量第三方代码，需要持续维护
3. **代码质量**: 部分代码来自外部，需要进一步审查和测试

## 贡献者

主要贡献者：
- **ZhangChengbo**: 所有主要提交的作者

## 参考资源

1. IFoxCAD 官方: https://gitee.com/inspirefunction/ifoxcad
2. BlockView.NET 来源: https://github.com/15831944/CADDev/tree/master
3. ThMEP.Core 来源: https://github.com/SBESIRC/ThMEP.Core/tree/main
4. 在线导图: https://boardmix.cn/app/share/CAE.CMvmgA4gASoQHBGpsUGmGR9LipooomyTSDAGQAE/U41nx2

---

**文档生成日期**: 2025-10-14  
**基准提交**: ea73facda61fd47d87100f05067b35fa4995850f  
**最新提交**: b47a8cf (清理未使用的 using 指令)

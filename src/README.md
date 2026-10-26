# src 源码阅读入口

> 状态：current；受众：maintainer<br>
> 稳定 ID：`entry.source`<br>
> 详细源码导航：[源码检索总入口](../docs/source-navigation/README.md)

查找某项能力时，先选择下面的分册，再按文件名、类型名和关键方法进入实现：

| 要找什么 | 阅读入口 |
| --- | --- |
| 项目与共享源码关系、宿主别名、按问题查文件 | [源码检索总入口](../docs/source-navigation/README.md) |
| Beta 多段线、排序、文字、几何与预览 | [Beta 能力总览](../docs/guides/beta-capabilities.md) |
| 事务、数据库、符号表、对象、字典、XData、外部参照 | [数据库与对象生命周期](../docs/source-navigation/database.md) |
| 块、曲线、多段线、文字、包围框、填充 | [实体与曲线操作](../docs/source-navigation/entities.md) |
| 当前文档、系统变量、Idle、输入、选择、Jig、命令 | [应用上下文与 Editor](../docs/source-navigation/application-editor.md) |
| 坐标、点与区域、几何曲线、四叉树、未编译的 ToDo | [几何与空间索引](../docs/source-navigation/geometry.md) |
| 初始化、注册、Lisp、窗口、状态栏、CAD native | [运行时、UI 与宿主互操作](../docs/source-navigation/runtime-ui.md) |
| 集合、循环链表、枚举、随机数、Win32、PE 解析 | [基础与 Windows 平台](../docs/source-navigation/foundation-platform.md) |

文档只提供定位和阅读线索，完整 API 语义以对应源码及 XML 注释为准。是否参与编译先查 `CADShared/CADShared.projitems`；文件存在不等于其全部成员在所有宿主中启用。

仓库全部文档的状态与阅读顺序见[文档索引](../docs/README.md)。

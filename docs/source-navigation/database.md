# 数据库、事务与对象生命周期定位

> 状态：current；受众：maintainer；稳定 ID：`reference.source-database`<br>
> 源码基线与统计口径：[源码检索总入口](README.md)<br>
> 覆盖 `Cad/Database` 中除实体目录外的 27 个共享编译文件。实体目录另见[实体分册](entities.md)。

阅读顺序：[事务](#transactions) → [对象与提权](#objects) → [符号表和落库](#symbol-tables) → [数据](#result-data) / [文件](#files) / [外参](#xrefs)。

<a id="transactions"></a>

## DBTrans 与原生事务

| 文件 | 入口 | 职责与继续追踪 |
| --- | --- | --- |
| [DBTrans.cs](../../src/CADShared/Cad/Database/Transactions/DBTrans.cs) | 三类构造、`GetTop`、`GetTopTransaction`、`GetObject`、`Task`、`Commit`、`Abort`、`Dispose` | 包装原生事务，缓存符号表/空间/字典，并持有可能取得的文档锁和文件数据库。是最重要的生命周期入口。 |
| [TransactionEx.cs](../../src/CADShared/Cad/Database/Transactions/TransactionEx.cs) | `GetObject<T>` | 对已有厂商 `Transaction` 做类型转换，不创建事务。类型不匹配实现返回 null，不能按旧 XML 中“抛异常”的描述处理。 |
| [DBTransEx.cs](../../src/CADShared/Cad/Database/Transactions/DBTransEx.cs) | `Purge` | 按 `SymModes` 收集候选，经 `Database.Purge` 查询可清理对象后删除。文件内部定义 `lack_test` 并启用该代码；名字不是“当前不编译”的证据。 |

### 先区分三种构造路径

| 路径 | 资源来源 | 调用方需要关注 |
| --- | --- | --- |
| 指定/当前 `Document` | 借用文档的数据库和 Editor，创建原生事务；`docLock=true` 时按当前锁状态取得文档锁。 | 不会因为是默认构造就总是取得文档锁。 |
| 已有 `Database` | 直接使用传入数据库，尝试关联 Document，创建原生事务。 | 后台库可能没有 Editor；不能随意调用交互方法。 |
| 文件名 | 可能新建/后台读取 DWG 或 DXF，也可能复用已打开文档；`activeOpen` 路径会前台开图/激活并锁文档。 | 文件构造不等于“一定拥有独立后台库”。释放仍按 `_fileName` 非空决定是否 Dispose 数据库，存在所有权边界，详见下文。 |

### 当前实现的关键事实

- `_dBTrans` 是进程级静态 `Stack<DBTrans>`，不是线程局部或按 Document 隔离的栈。
- `GetTopTransaction(database)` 返回该数据库事务管理器的顶层原生事务；不存在时抛出 `NoTopTransaction!`。
- 公开的 `GetTop(database)` 会匹配原生顶层事务的 unmanaged 指针；找不到匹配时回退到内部 `Top`。`Top` 为 `internal`，空栈时抛异常，不会自动创建事务。
- 构造参数 `commit` 默认 true。普通 `Dispose()` 按 `_commit` 决定提交，不根据调用方是否因为异常离开作用域自动改成回滚。
- `Commit()` / `Abort()` 先修改 `_commit`，随后调用 `Dispose()`，都是完成并释放当前实例的路径，不是允许继续读写的检查点。
- `Dispose(bool)` 依次完成原生事务、文档锁、可能的 Database 释放，最后对静态栈 `Pop`。当前没有先验证 `this` 就是栈顶，也没有完整的部分失败状态机。

最后两项以及终结器、文件库所有权的后续设计写在[生命周期提案](../dbtrans-lifecycle-contract.md)。那里 `Decision` 部分仍是提案；本导航只说明现状，不把提案中的 LIFO 防护、终结器移除或所有权字段当成现有实现。

`Task(action, handlingDBTextDeviation)` 用于前后台文字偏移相关场景：需要切换工作库的路径使用 `try/finally` 恢复原 `WorkingDatabase`；前台、有意关闭处理或无活动文档的路径直接执行 action。它不创建新的独立事务，也不保证整个调用链已经跨文档安全。

### 常用属性按资源分组检索

| 资源 | `DBTrans` 属性 | 继续定位 |
| --- | --- | --- |
| 原生上下文 | `Document`、`Editor`、`Database`、`Transaction` | 三种构造函数，尤其是后台库没有 Editor 的路径。 |
| 实体容器 | `CurrentSpace`、`ModelSpace`、`PaperSpace` | `CurrentSpace` 会在 `Database.CurrentSpaceId` 变化后重新取得记录；它与固定模型空间、所有布局都不是同一含义。 |
| 符号表 | `BlockTable`、`LayerTable`、`TextStyleTable`、`RegAppTable`、`DimStyleTable`、`LinetypeTable`、`UcsTable`、`ViewTable`、`ViewportTable` | 懒初始化 `SymbolTable<TTable,TRecord>` 包装，实际新增/修改在对应表和记录扩展中。 |
| 常用字典 | `NamedObjectsDict`、`GroupDict`、`MLeaderStyleDict`、`MLStyleDict`、`MaterialDict`、`TableStyleDict` 等 | 按数据库字典 ID 打开，继续到 `DBDictionaryEx`；不要按名称误选实体 XData。 |

已有阅读入口：[TestDBTrans.cs](../../tests/TestShared/TestDBTrans.cs)、[TestFileDatabase.cs](../../tests/TestShared/TestFileDatabase.cs)。本轮未执行。

<a id="objects"></a>

## ObjectId、DBObject 与写入作用域

| 文件 | 关键方法 / 类型 | 功能细节 |
| --- | --- | --- |
| [ObjectIdEx.cs](../../src/CADShared/Cad/Database/Objects/ObjectIdEx.cs) | `GetObject<T>`、`IsDerivedFrom<T>`、`IsOk`、`GetObjectId`、`ConvertToHandle`、`Erase` | 由 `ObjectId.Database` 找顶层事务并打开对象；集合版本返回延迟查询，枚举时事务仍需有效。另有 Handle 字符串转换和对象合法性检查。 |
| [DBObjectEx.cs](../../src/CADShared/Cad/Database/Objects/DBObjectEx.cs) | `ForWrite`、嵌套 `UpgradeOpenManager` | 临时提升已有对象的打开模式，处理普通只读与 Notify 分支，释放作用域时按原状态恢复。它不提供文档锁，不切换数据库，也不是新事务。 |
| 同上 | `GetEffectiveClassName`、`CloneEx`、`RemoveXData`、`ChangeXData` | 运行时类名、克隆及指定应用/组码的 XData 编辑。克隆返回对象的落库或释放由调用方决定。 |
| [CollectionEx.cs](../../src/CADShared/Cad/Database/Collections/CollectionEx.cs) | `ToCollection`、`ToList`、`ForEach`、`KeywordName`、`IdMapping.ToDictionary` | Autodesk/ZwSoft 等宿主集合转换、带 `LoopState` 的循环、关键字名字判断，以及克隆映射的键值提取；并非只处理数据库集合。 |

排查“已经有事务为什么仍打不开对象”时，按 `id.Database`、该库的顶层原生事务、当前 `DBTrans` 栈及对象打开模式逐项核对。将 `DBObject` 保存在事务外使用，和将稳定 `ObjectId` 留到后续事务重新打开，是不同生命周期。

已有调用：[TestId.cs](../../tests/TestShared/TestId.cs)、[TestDBobject.cs](../../tests/TestShared/TestDBobject.cs)。

<a id="symbol-tables"></a>

## 符号表、块表记录与落库

| 文件 | 入口 | 职责 |
| --- | --- | --- |
| [SymbolTable.cs](../../src/CADShared/Cad/Database/SymbolTables/SymbolTable.cs) | `SymbolTable<TTable,TRecord>`、索引器、`Has`、`Add`、`AddOrChange`、`Change`、`GetRecord`、`ForEach` | 由 `DBTrans` 创建并缓存的符号表包装。字符串索引器不存在时返回 `ObjectId.Null`。表内枚举只在所属事务有效时使用。 |
| [SymbolTableEx.cs](../../src/CADShared/Cad/Database/SymbolTables/SymbolTableEx.cs) | 图层 `Add/Rename/Delete`、块 `Add/GetBlockFrom/AddAttsToBlocks`、线型 `Add`、文字样式 `Add/AddWithChange` | 将泛型表操作细化为具体资源，跨文件块导入继续追踪内部 `GetRecordFrom`。 |
| [SymbolTableRecordEx.cs](../../src/CADShared/Cad/Database/SymbolTables/SymbolTableRecordEx.cs) | `AddEntity`、`GetEntities<T>`、`InsertBlock`、`DeepCloneEx`、`GetDrawOrderTable` | 模型/当前空间和块定义等 `BlockTableRecord` 的实体容器操作；既承担落库，也承担块参照与属性创建。 |
| [LayerUnlockScope.cs](../../src/CADShared/Cad/Database/SymbolTables/LayerUnlockScope.cs) | 构造、`LayerId`、`Dispose` | 暂时解锁指定数据库/图层，在作用域结束时恢复原锁状态；不解冻图层。内部使用普通 `StartTransaction`，适配 ZWCAD 2022。 |
| [SymModes.cs](../../src/CADShared/Cad/Database/SymbolTables/SymModes.cs) | `SymModes` | 标记操作涉及的块、图层、文字等符号表，用于清理及参照绑定相关路径。 |
| [BaseEx.cs](../../src/CADShared/Cad/Database/SymbolTables/BaseEx.cs) | `IsLegalLayerName` | 图层名合法性辅助；不是图层创建 API。 |

最容易误用的三个区别：

1. `Add(name, action)` 找到已有记录就返回，不执行新建回调；需要修改已有记录时追踪 `AddOrChange` 或 `Change`。
2. `CurrentSpace` / `ModelSpace` 是具体数据库的块表记录；`SymbolTable` 不是所有实体的统一容器。
3. `AddEntity` 先用记录所属数据库取得原生顶层事务，再提权记录、`AppendEntity`、`AddNewlyCreatedDBObject`。批量重载会立即物化新增 ID 列表，但 `GetEntities` 等查询有延迟枚举路径，应分别查看实现。

块参照插入继续阅读[实体分册](entities.md#blocks)；已有调用：[TestLayer.cs](../../tests/TestShared/TestLayer.cs)、[TestBlock.cs](../../tests/TestShared/TestBlock.cs)、[TestEntity/TestAddEntity.cs](../../tests/TestShared/TestEntity/TestAddEntity.cs)。

<a id="result-data"></a>

## 字典、XData、Xrecord 与 Group

| 文件 | 主要类型 / 入口 | 需要区分的语义 |
| --- | --- | --- |
| [TypedValueList.cs](../../src/CADShared/Cad/Database/ResultData/TypedValueList.cs) | `TypedValueList`、`Add`、与 `ResultBuffer` / 数组的转换 | DXF typed value 列表基类，构造函数为 protected；常通过派生类型使用。 |
| [XDataList.cs](../../src/CADShared/Cad/Database/ResultData/XDataList.cs) | `XDataList`、`Contains`、`GetXdataAppIndex` | 表示应用 XData，检查 XData 组码并按注册应用标记查找。 |
| [XRecordDataList.cs](../../src/CADShared/Cad/Database/ResultData/XRecordDataList.cs) | `XRecordDataList` | 表示 Xrecord 数据；`Add(int,object)` 拒绝 `>=1000` 的组码，与 XData 不是可互换列表。 |
| [DBDictionaryEx.cs](../../src/CADShared/Cad/Database/Dictionaries/DBDictionaryEx.cs) | `GetData/SetData`、`GetXRecord/SetXRecord`、`GetXDictionary`、`GetSubDictionary` | 访问已有 `DBDictionary`，获取/建立实体扩展字典、子字典并读写记录。传入哪个字典决定实体级或图纸级作用域。 |
| 同上 | `CreateDataTable`、`DataCell.SetValue`、`AddGroup/GetGroups/RemoveNullGroup` | DWG 数据表单元格写入、组创建/枚举与空组清理。这里的 `DataTable` 是 CAD 数据库对象，不是维护一份 Excel 表格。 |

排查数据时先确定存储位置：实体 `XData`、实体 `ExtensionDictionary`、数据库 `NamedObjectsDictionary` 或其他子字典。`ResultBuffer` 是数据传输形式，不是另一个持久化位置。

Lisp 同样使用 typed value，但组码协议和作用域不同，转到 [LispList](runtime-ui.md#lisp)。已有调用：[TestXdata.cs](../../tests/TestShared/TestXdata.cs)、[TestXRecord.cs](../../tests/TestShared/TestXRecord.cs)。

<a id="files"></a>

## 数据库切换、保存与文件标记

| 文件 | 入口 | 功能与边界 |
| --- | --- | --- |
| [SwitchDatabase.cs](../../src/CADShared/Cad/Database/SwitchDatabase.cs) | 构造、`Dispose` | 保存进入前的 `WorkingDatabase`，切到指定库，释放时恢复；不创建/释放传入的数据库。 |
| [DatabaseEx.cs](../../src/CADShared/Cad/Database/DatabaseEx.cs) | `SwitchDatabase`、`SaveDwgFile`、`SaveFile` | 按前后台、扩展名与另存路径处理保存。保存文件与提交事务是两个动作，不能混同。 |
| [DwgMark.cs](../../src/CADShared/Cad/Database/Files/DwgMark.cs) | `AddMark`、`RemoveMark`、`GetMark` | 直接读写 DWG 文件流的 `0x15` 偏移，不经过事务和实体字典。写入调用是 `BinaryWriter.Write(int)`，而读取只取一个字节；不要按旧“一字节”注释推断写入宽度。 |

现有文件构造与保存场景可从 [TestFileDatabase.cs](../../tests/TestShared/TestFileDatabase.cs)、[TestDwgMark.cs](../../tests/TestShared/TestDwgMark.cs) 阅读，不在本导航中执行或重写这些场景。

<a id="xrefs"></a>

## 外部参照与路径

| 文件 | 入口 / 角色 | 查找内容 |
| --- | --- | --- |
| [XrefEx.cs](../../src/CADShared/Cad/Database/Xrefs/XrefEx.cs) | `DBTrans.XrefFactory` | 根据 `XrefModes` 分派到参照工厂。 |
| [XrefFactory.cs](../../src/CADShared/Cad/Database/Xrefs/XrefFactory.cs) | `Bind`、`Detach`、`Reload`、`Unload` | 根据可选参照名集合遍历外参；null 表示全部候选。绑定走 `DoubleBind` 等内部路径，涉及解析、嵌套与符号表处理。 |
| [IXrefBindModes.cs](../../src/CADShared/Cad/Database/Xrefs/IXrefBindModes.cs) | `IXrefBindModes` | 上述四类操作的接口，不是另一套绑定实现。 |
| [XrefModes.cs](../../src/CADShared/Cad/Database/Xrefs/XrefModes.cs) | `XrefModes` | 操作选择枚举。 |
| [XrefPath.cs](../../src/CADShared/Cad/Database/Xrefs/XrefPath.cs) | 构造、`PathConverter`、`GetRelativePath` | 从块参照/数据库关联解析及转换外参路径。 |
| [PathConverterModes.cs](../../src/CADShared/Cad/Database/Xrefs/PathConverterModes.cs) | `PathConverterModes` | 路径转换方向枚举。 |

工厂中的 `BindOrInsert`、`AutoDetach`、`EraseNested` 等是已有 readonly 字段，不能把它们写成调用方可以随时设置的选项。解析导致文字偏移时，再追踪 `DBTrans.Task` 的工作库切换；“能找到外参文件”与“绑定后实体/图层正确”是不同证据。

已有入口：[TestXrefEx.cs](../../tests/TestShared/TestXrefEx.cs)。

## 关联动作与子对象

| 文件 | 类型与入口 | 宿主边界 |
| --- | --- | --- |
| [AssocUtils.cs](../../src/CADShared/Cad/Database/Associativity/AssocUtils.cs) | `Fs.Fox.Cad.Assoc.AssocUtils.CreateActionAndActionBodyAndPostToDatabase` | 创建关联动作和 action body；主要实现仅在 `ACAD` 分支。 |
| [AssocPersSubentityIdPEEx.cs](../../src/CADShared/Cad/Database/Associativity/AssocPersSubentityIdPEEx.cs) | `GetPersSubentityIdPE`、`GetAllSubentityIds`、`GetAllSubentities` | 查询 protocol extension、子对象 ID 和实体；排除早期 GStarCAD，PE 访问还排除 ZWCAD 2022。需检查具体 `#if` 后再使用。 |

返回[源码检索总入口](README.md)。

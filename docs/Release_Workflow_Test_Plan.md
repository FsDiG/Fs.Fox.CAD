# Release Workflow 测试计划

## 测试目标
验证 GitHub Actions NuGet 发布工作流能够正常工作。

## 前置条件

### 1. 配置 NuGet API Key
- [ ] 已在 NuGet.org 创建账号
- [ ] 已创建 API Key（参考 `docs/NuGet_API_Key_Setup.md`）
- [ ] 已在 GitHub Settings → Secrets 中添加 `NUGET_API_KEY`

### 2. 权限检查
- [ ] 对 FsDiG/Fs.Fox.CAD 仓库有写入权限
- [ ] 对 NuGet.org 上的包有发布权限（或包尚未存在）

## 测试步骤

### 阶段 1: 测试发布（预发布版本）

#### 步骤 1.1: 创建测试标签
```bash
# 切换到 main 分支
git checkout main
git pull origin main

# 创建测试版本标签
git tag v0.9.2-preview7-test

# 推送标签
git push origin v0.9.2-preview7-test
```

#### 步骤 1.2: 监控工作流
1. 访问 https://github.com/FsDiG/Fs.Fox.CAD/actions
2. 找到 "Release to NuGet" 工作流运行
3. 点击查看详细日志

#### 步骤 1.3: 验证每个步骤
- [ ] 步骤 1: Checkout 成功
- [ ] 步骤 2: Setup .NET 8 成功
- [ ] 步骤 3: Setup MSBuild 成功
- [ ] 步骤 4: Get version from tag 成功，输出 "Version: 0.9.2-preview7-test"
- [ ] 步骤 5: Update project version 成功
- [ ] 步骤 6: Restore NuGet packages 成功
- [ ] 步骤 7-10: 所有 4 个项目构建成功
- [ ] 步骤 11-14: 所有 4 个项目打包成功
- [ ] 步骤 15: Publish to NuGet 成功（或跳过已存在的版本）
- [ ] 步骤 16: Create GitHub Release 成功

#### 步骤 1.4: 验证 NuGet.org
1. 访问 https://www.nuget.org/profiles/[你的用户名]
2. 验证包已上传：
   - [ ] IFox.CAD.ACAD2019 版本 0.9.2-preview7-test
   - [ ] IFox.CAD.ACAD2025 版本 0.9.2-preview7-test
   - [ ] IFox.CAD.ZCAD2022 版本 0.9.2-preview7-test
   - [ ] IFox.CAD.ZCAD2025 版本 0.9.2-preview7-test
3. 检查包信息：
   - [ ] 包含 readme.md
   - [ ] 包含 LICENSE
   - [ ] 版本号正确
   - [ ] 依赖项正确

#### 步骤 1.5: 验证 GitHub Release
1. 访问 https://github.com/FsDiG/Fs.Fox.CAD/releases
2. 验证新建的 Release：
   - [ ] 标签为 v0.9.2-preview7-test
   - [ ] 包含 4 个 .nupkg 文件
   - [ ] Release 说明包含版本信息和包列表

### 阶段 2: 测试包安装

#### 步骤 2.1: 在测试项目中安装包
```bash
# 创建测试项目
dotnet new console -n TestNuGetPackage
cd TestNuGetPackage

# 安装包（等待几分钟让 NuGet 索引）
dotnet add package IFox.CAD.ACAD2019 --version 0.9.2-preview7-test
```

#### 步骤 2.2: 验证安装
- [ ] 包成功安装
- [ ] 没有依赖项错误
- [ ] 可以引用包中的类型

### 阶段 3: 正式版本发布（可选）

如果测试成功，可以发布正式版本：

```bash
# 删除测试标签
git tag -d v0.9.2-preview7-test
git push origin :refs/tags/v0.9.2-preview7-test

# 创建正式版本标签
git tag v0.9.2-preview7
git push origin v0.9.2-preview7
```

## 故障排查

### 问题 1: 工作流未触发
**症状**: 推送标签后没有看到工作流运行

**可能原因**:
- 标签格式不正确（必须以 'v' 开头）
- 工作流文件有语法错误

**解决方法**:
1. 检查标签格式: `git tag -l`
2. 验证工作流文件: 访问 `.github/workflows/release.yml`

### 问题 2: 构建失败
**症状**: 某个构建步骤失败

**可能原因**:
- NuGet 包下载失败
- 项目配置错误
- MSBuild 版本不兼容

**解决方法**:
1. 查看详细错误日志
2. 检查项目文件配置
3. 验证依赖项版本

### 问题 3: 发布到 NuGet 失败
**症状**: Publish to NuGet 步骤失败

**可能原因**:
- API Key 无效或过期
- API Key 权限不足
- 包版本已存在
- 网络问题

**解决方法**:
1. 验证 NUGET_API_KEY Secret 配置正确
2. 检查 API Key 权限和过期时间
3. 使用新的版本号
4. 重新运行工作流

### 问题 4: 创建 Release 失败
**症状**: Create GitHub Release 步骤失败

**可能原因**:
- GitHub Token 权限不足
- Release 已存在

**解决方法**:
1. 检查仓库的 Actions 权限设置
2. 删除已存在的 Release 或使用新标签

## 清理测试数据

测试完成后，可以清理测试数据：

### 清理 GitHub
```bash
# 删除测试标签
git tag -d v0.9.2-preview7-test
git push origin :refs/tags/v0.9.2-preview7-test
```

然后在 GitHub Releases 页面删除测试 Release。

### 清理 NuGet.org
注意：**NuGet.org 不允许删除已发布的包**，但可以：
1. 将包标记为 "Unlisted"（不出现在搜索结果中）
2. 在包页面添加弃用说明

## 成功标准

- [ ] 工作流成功完成所有步骤
- [ ] 所有 4 个包成功发布到 NuGet.org
- [ ] 包可以正常安装和使用
- [ ] GitHub Release 正确创建
- [ ] 包信息完整（readme、LICENSE、版本号等）

## 参考文档

- [Release Workflow 文档](../.github/workflows/release.md)
- [NuGet API Key 配置](./NuGet_API_Key_Setup.md)
- [实施总结](./Issue_8_Implementation_Summary.md)

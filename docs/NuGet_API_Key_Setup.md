# NuGet API Key 配置指南

## 概述

要使用 GitHub Actions 自动发布 NuGet 包，需要配置 NuGet API Key 作为 GitHub Secret。

## 步骤 1: 获取 NuGet API Key

1. 访问 [NuGet.org](https://www.nuget.org/) 并登录您的账号
2. 点击右上角的用户名，选择 "API Keys"
3. 点击 "Create" 按钮创建新的 API Key
4. 配置 API Key：
   - **Key Name**: 输入一个描述性名称（例如：`GitHub Actions - Fs.Fox.CAD`）
   - **Package Owner**: 选择您的账号或组织
   - **Scopes**: 选择 "Push" 和 "Push new packages and package versions"
   - **Select Packages**: 选择 "All Packages" 或指定特定包
   - **Glob Pattern**: 可以使用 `IFox.CAD.*` 来匹配所有相关包
   - **Expiration**: 设置过期时间（建议选择较长时间或不过期）
5. 点击 "Create" 创建 API Key
6. **重要**: 复制显示的 API Key 并妥善保存（只会显示一次）

## 步骤 2: 在 GitHub 中配置 Secret

1. 打开 GitHub 仓库页面：https://github.com/FsDiG/Fs.Fox.CAD
2. 点击 `Settings` 标签
3. 在左侧菜单中，点击 `Secrets and variables` → `Actions`
4. 点击 `New repository secret` 按钮
5. 添加 Secret：
   - **Name**: `NUGET_API_KEY`
   - **Secret**: 粘贴您从 NuGet.org 复制的 API Key
6. 点击 `Add secret` 保存

## 步骤 3: 验证配置

配置完成后，您可以：

1. 创建一个测试标签：
   ```bash
   git tag v0.0.1-test
   git push origin v0.0.1-test
   ```

2. 在 GitHub Actions 页面查看工作流运行情况
   - 访问：https://github.com/FsDiG/Fs.Fox.CAD/actions

3. 如果配置正确，工作流应该能够成功发布包到 NuGet.org

## 故障排查

### 错误：401 Unauthorized

- **原因**: API Key 无效或过期
- **解决**: 重新生成 API Key 并更新 GitHub Secret

### 错误：403 Forbidden

- **原因**: API Key 权限不足
- **解决**: 检查 API Key 的 Scopes 设置，确保有 Push 权限

### 错误：409 Conflict

- **原因**: 包版本已存在
- **解决**: 使用新的版本号重新发布（NuGet 不允许覆盖已发布的版本）

## 安全建议

1. **定期更换 API Key**: 建议每 6-12 个月更换一次 API Key
2. **使用最小权限**: 只授予发布包所需的最小权限
3. **监控使用情况**: 定期检查 NuGet.org 上的包发布记录
4. **保护 Secret**: 不要在日志或代码中泄露 API Key

## 相关链接

- [NuGet API Keys 管理](https://www.nuget.org/account/apikeys)
- [GitHub Secrets 文档](https://docs.github.com/en/actions/security-guides/encrypted-secrets)
- [NuGet 发布文档](https://learn.microsoft.com/en-us/nuget/nuget-org/publish-a-package)

# DiGArchBase 项目：构建与部署工作流程（`.github/workflows/build-and-deploy.yml`）使用指南

## 1. 工作流程核心功能与触发

本指南介绍 `DiGArchBase` 项目中 `.github/workflows/build-and-deploy.yml` 的 GitHub Actions 工作流程。该流程用于自动化解决方案的构建及产物部署。

**主要功能：**
*   **构建（Build）：** 编译解决方案（如 `DigArchBase.sln`）。
*   **部署（Deploy）：** 根据条件将构建产物推送至 `FsDiG/Build` 仓库。

**工作流程作业 (`build-and-deploy`) 触发条件：**

作业在以下任一情况下运行：
1.  **推送到 `main` 分支（`push` 到 `main`）：**
    *   **效果：** 执行编译。若提交信息包含 `[deploy]` 标签，则执行部署。
2.  **向 `main` 分支发起拉取请求（Pull Request to `main`）：**
    *   **效果：** 执行编译（用于 PR 校验），**不**执行部署。
3.  **提交信息包含 `[build]` 或 `[deploy]` 并推送到任意分支：**
    *   例如：`git commit -m "feat: new feature [build]"` 或 `git commit -m "feat: new feature [deploy]"`
    *   **效果：** 执行编译。若提交信息含 `[deploy]`，则执行部署。
4.  **手动触发（`workflow_dispatch`）：**
    *   可在 GitHub Actions UI 手动为指定分支（默认 `main`）启动流程。
    *   **效果：** 执行编译。手动触发时，可以通过新增的 `deploy` 布尔参数（默认为 `false`）来决定是否在构建成功后执行部署。

## 2. 部署到 `FsDiG/Build` 仓库的条件

仅当以下任一条件满足时，才会执行部署相关步骤：
1.  `push` 事件的最新提交信息包含 `[deploy]`（区分大小写）。
2.  通过 `workflow_dispatch`（手动触发）启动工作流程时，如果 `deploy` 输入参数设置为 `true`。

**示例：**
*   在特性分支构建并部署：`git commit -m "feat: new feature [build] [deploy]"` 或 `git commit -m "feat: new feature [deploy]"`
*   在 `main` 分支推送包含 `[deploy]` 的提交（如 `git commit -m "hotfix: critical update [deploy]"`）会触发构建和部署。
*   手动触发工作流，并选择执行部署。

**总结：** 部署的关键在于 `push` 事件的提交信息中包含 `[deploy]` 标签，或者在手动触发时明确选择部署。

## 3. 标签说明（`[build]` 与 `[deploy]`）

*   **`[build]`**：
    *   用于确保在任意分支的 `push` 事件中触发编译。
    *   示例：`git commit -m "fix: resolve issue [build]"`
*   **`[deploy]`**：
    *   确保在 `push` 事件中触发编译。
    *   用于在 `push` 事件触发的构建成功后，允许执行部署步骤。
    *   **注意：** 必须与可触发编译的 `push` 事件结合使用（目前 `[deploy]` 本身即可触发构建）。
    *   示例：`git commit -m "feat: release candidate [build] [deploy]"` 或 `git commit -m "feat: new feature [deploy]"`

## 4. 自托管执行器（Self-Hosted Runner）要求

该流程需在自托管执行器上运行，执行器需满足：
*   安装 **MSBuild**
*   安装 **PowerShell（pwsh）**
*   安装 **Git**
*   正确设置环境变量 **`FsArxSdkDir`**（指向 FsArxSdk 目录，如 `D:/FsArxSdk202X`，供 `copyToDigBuild.ps1` 脚本使用）。工作流程现在包含一个步骤，会在执行部署相关脚本前显式检查此环境变量是否已设置；如果未设置，工作流程将失败。
*   能访问 `github.com`（用于代码检出和推送）
*   拥有相应执行权限

## 5. 其他说明

*   **部署认证：** 推送到 `FsDiG/Build` 使用 `BUILD_REPO_TOKEN`（配置于 `DiGArchBase` 仓库的 Actions secrets），该 token 需有写入权限。
*   **查看运行状态：** 可在 `DiGArchBase` 仓库的 "Actions" 标签页查看流程日志和状态。
*   **`copyToDigBuild.ps1` 脚本：** 负责处理构建产物并复制到部署目标，依赖 `DiGBuildDir`（由 workflow 设置）和 `FsArxSdkDir`（需在 runner 设置）环境变量。在执行此脚本前，工作流程会验证 `FsArxSdkDir` 环境变量是否已正确配置。

理解上述核心触发机制和要求后，开发者可高效使用本工作流程。
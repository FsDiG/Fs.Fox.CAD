# PowerShell Script
# 相当于 @echo off (PowerShell 默认不回显命令)
# 相当于 chcp 936 (PowerShell 默认使用 UTF-8，通常不需要更改，但如果需要，可以使用 [Console]::OutputEncoding)
# [Console]::OutputEncoding = [System.Text.Encoding]::GetEncoding(936) # 如果需要特定代码页

# 相当于 setlocal enabledelayedexpansion (PowerShell 中变量默认就是延迟展开的)

# 获取环境变量 DiGBuildDir(调用者传递进来的)
$baseDir = $env:DiGBuildDir

# 检查环境变量是否设置
if ([string]::IsNullOrEmpty($baseDir)) {
    Write-Host "[错误] 环境变量 DiGBuildDir 未设置" -ForegroundColor Red
    exit 1
}

# 设置目标目录为 $baseDir\DiGLib\DiGArchBase
$targetDir = Join-Path -Path $baseDir -ChildPath "DiGLib\DiGArchBase"

# 设置源目录为脚本所在目录的 Build 文件夹
# $PSScriptRoot 是当前脚本所在的目录，相当于 %~dp0
$sourceDir = Join-Path -Path $PSScriptRoot -ChildPath "Build"
#  $sourceDir 应该设置为

# 检查源目录是否存在
if (-not (Test-Path -Path $sourceDir -PathType Container)) {
    Write-Host "[错误] 源目录 $sourceDir 不存在" -ForegroundColor Red
    exit 1
}

# 创建目标目录（如果不存在）
if (-not (Test-Path -Path $targetDir -PathType Container)) {
    try {
        New-Item -ItemType Directory -Path $targetDir -Force -ErrorAction Stop | Out-Null
        Write-Host "[信息] 已创建目标目录: $targetDir" -ForegroundColor Cyan
    }
    catch {
        Write-Host "[错误] 创建目标目录 $targetDir 失败: $($_.Exception.Message)" -ForegroundColor Red
        exit 1
    }
}

Write-Host "[开始] 正在复制依赖文件..." -ForegroundColor Green


# 初始化文件计数器
$fileCount = 0
$copiedCount = 0
$skippedCount = 0
$errorOccurred = $false
# 添加一个数组用于存储复制的文件路径
$copiedFiles = @()

Write-Host "[信息] 开始处理 Build 目录下的文件..." -ForegroundColor Cyan

# 复制文件并创建子目录
# Get-ChildItem -Path $sourceDir -Include @("*.dll", "*.dbx", "*.arx", "*.pdb", "*.lib") -Recurse -File
# The -File parameter ensures we only process files.
Get-ChildItem -Path $sourceDir -Include "*.dll", "*.dbx", "*.arx", "*.pdb", "*.lib" -Recurse -File | ForEach-Object {
    $sourceFileItem = $_
    $fileCount++

    # 构建相对路径
    # $sourceDir 可能有或没有末尾的 '\'， $sourceFileItem.FullName 总是完整的。
    # Ensure $sourceDir for replacement has a trailing slash if not already present, for reliable replacement.
    $normalizedSourceDir = $sourceDir
    if (-not ($normalizedSourceDir.EndsWith("\") -or $normalizedSourceDir.EndsWith("/"))) {
        $normalizedSourceDir += "\"
    }
    $relativeFilePath = $sourceFileItem.FullName.Replace($normalizedSourceDir, "")
    
    $destFile = Join-Path -Path $targetDir -ChildPath $relativeFilePath
    $destDirPath = Split-Path -Path $destFile -Parent

    # 创建目标子目录（如果不存在）
    if (-not (Test-Path -Path $destDirPath -PathType Container)) {
        try {
            New-Item -ItemType Directory -Path $destDirPath -Force -ErrorAction Stop | Out-Null
        }
        catch {
            Write-Host "[错误] 创建目录 $destDirPath 失败: $($_.Exception.Message)" -ForegroundColor Red
            $errorOccurred = $true
            return # Skip to next file in ForEach-Object
        }
    }
    
    $performCopy = $false
    if (Test-Path -Path $destFile -PathType Leaf) {
        # 文件存在，比较内容 (使用文件哈希进行比较，更可靠)
        try {
            $sourceHash = (Get-FileHash -Path $sourceFileItem.FullName -Algorithm MD5).Hash
            $destHash = (Get-FileHash -Path $destFile -Algorithm MD5).Hash
            if ($sourceHash -ne $destHash) {
                $performCopy = $true
                Write-Host "[信息] 文件 $destFile 内容不一致，将重新复制." -ForegroundColor DarkCyan
            } else {
                $skippedCount++
                Write-Host "[跳过] 文件 $destFile 内容一致." -ForegroundColor Gray
            }
        } catch {
            Write-Host "[警告] 比较文件 $($sourceFileItem.FullName) 和 $destFile 时出错: $($_.Exception.Message)。将尝试复制。" -ForegroundColor Yellow
            $performCopy = $true # 出错则尝试复制
        }
    } else {
        # 目标文件不存在，直接复制
        $performCopy = $true
    }

    if ($performCopy) {
        try {
            Copy-Item -Path $sourceFileItem.FullName -Destination $destFile -Force -ErrorAction Stop
            $copiedCount++
            # 添加复制的文件路径到数组
            $copiedFiles += $destFile
            Write-Host "[复制] $($sourceFileItem.FullName) -> $destFile" -ForegroundColor DarkGreen
        }
        catch {
            Write-Host "[错误] 复制 $($sourceFileItem.FullName) 文件到 $destFile 失败！详细信息: $($_.Exception.Message)" -ForegroundColor Red
            $errorOccurred = $true
        }
    }
}

Write-Host "[完成] 文件复制结束" -ForegroundColor Green

# 输出统计和状态
Write-Host "[统计] 总处理: $fileCount, 实际复制: $copiedCount, 跳过: $skippedCount" -ForegroundColor Cyan
if ($errorOccurred) {
    Write-Host "[警告] 复制过程中出现错误，请检查上面的错误信息" -ForegroundColor Yellow
} else {
    Write-Host "[状态] 所有文件复制成功" -ForegroundColor Green
}

# 输出所有复制的文件路径
if ($copiedCount -gt 0) {
    Write-Host "`n[复制文件列表]" -ForegroundColor Cyan
    foreach ($file in $copiedFiles) {
        Write-Host $file -ForegroundColor DarkGreen
    }
}
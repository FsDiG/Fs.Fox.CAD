[CmdletBinding()]
param(
    [string]$ProjectItemsPath,
    [string]$BaselinePath,
    [switch]$UpdateBaseline
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$repoRoot = Split-Path -Parent $PSScriptRoot
if ([string]::IsNullOrWhiteSpace($ProjectItemsPath)) {
    $ProjectItemsPath = Join-Path $repoRoot 'src\CADShared\CADShared.projitems'
}
elseif (-not [IO.Path]::IsPathRooted($ProjectItemsPath)) {
    $ProjectItemsPath = Join-Path $repoRoot $ProjectItemsPath
}

if ([string]::IsNullOrWhiteSpace($BaselinePath)) {
    $BaselinePath = Join-Path $PSScriptRoot 'CADSharedModuleBaseline.json'
}
elseif (-not [IO.Path]::IsPathRooted($BaselinePath)) {
    $BaselinePath = Join-Path $repoRoot $BaselinePath
}

$ProjectItemsPath = [IO.Path]::GetFullPath($ProjectItemsPath)
$BaselinePath = [IO.Path]::GetFullPath($BaselinePath)
$sourceRoot = [IO.Path]::GetFullPath((Split-Path -Parent $ProjectItemsPath))
$sourceRootPrefix = $sourceRoot.TrimEnd([IO.Path]::DirectorySeparatorChar) + [IO.Path]::DirectorySeparatorChar

$expectedCompileCount = 112
$expectedModuleCounts = [ordered]@{
    'Foundation'       = 11
    'Platform.Windows' = 5
    'Cad.Interop'      = 3
    'Cad.Geometry'     = 13
    'Cad.Database'     = 38
    'Cad.Editor'       = 17
    'Cad.Application'  = 6
    'Cad.Runtime'      = 12
    'Cad.UI'           = 7
}
$allowedModules = @($expectedModuleCounts.Keys)
$allowedDebtIds = @(1..24 | ForEach-Object { 'BD-{0:D2}' -f $_ })
$includePrefix = '$(MSBuildThisFileDirectory)'
$failures = [Collections.Generic.List[string]]::new()

function Add-Failure {
    param([string]$Message)
    $script:failures.Add($Message)
}

function Get-NormalizedTextHash {
    param([string]$Path)

    $text = [IO.File]::ReadAllText($Path)
    $text = $text.Replace("`r`n", "`n").Replace("`r", "`n")
    $bytes = [Text.UTF8Encoding]::new($false).GetBytes($text)
    $sha256 = [Security.Cryptography.SHA256]::Create()
    try {
        return ([Convert]::ToHexString($sha256.ComputeHash($bytes))).ToLowerInvariant()
    }
    finally {
        $sha256.Dispose()
    }
}

function Remove-CSharpCommentsAndStrings {
    param([string]$Text)

    $withoutComments = [regex]::Replace(
        $Text,
        '/\*.*?\*/|//.*?$',
        ' ',
        [Text.RegularExpressions.RegexOptions]::Singleline -bor
            [Text.RegularExpressions.RegexOptions]::Multiline
    )
    $withoutStrings = [regex]::Replace(
        $withoutComments,
        '@"(?:""|[^"])*"|\$?"(?:\\.|[^"\\])*"|''(?:\\.|[^''\\])''',
        ' ',
        [Text.RegularExpressions.RegexOptions]::Singleline
    )
    return $withoutStrings
}

function Get-TargetModule {
    param([string]$RelativePath)

    $normalized = $RelativePath.Replace('/', '\')
    if ($normalized -match '^Foundation\\') {
        return 'Foundation'
    }
    if ($normalized -match '^Platform\\Windows\\') {
        return 'Platform.Windows'
    }
    if ($normalized -match '^Cad\\(?<Area>Interop|Geometry|Database|Editor|Application|Runtime|UI)\\') {
        return "Cad.$($Matches.Area)"
    }
    if ($normalized -match '^(Foundation|Platform|Cad)\\') {
        return '__invalid_target_root__'
    }
    return $null
}

$boundaryRules = @(
    [pscustomobject]@{
        Name = 'CadSdk'
        Modules = @('Foundation', 'Platform.Windows')
        Pattern = '\b(?:Autodesk\.AutoCAD|ZwSoft\.ZwCAD|CadCoreApp|CadApp|HostApplicationServices|DBObject|ObjectId|Database|Transaction|Entity|Editor|Prompt[A-Za-z0-9_]*|SelectionSet|Jig[A-Za-z0-9_]*|Drawable)\b'
        AllowedDebtIds = @()
    },
    [pscustomobject]@{
        Name = 'WindowsInterop'
        Modules = @('Foundation')
        Pattern = '\b(?:DllImport|LibraryImport|Marshal|IntPtr|UIntPtr|nint|nuint|SafeHandle|kernel32|user32|gdi32|advapi32|imm32)\b'
        AllowedDebtIds = @()
    },
    [pscustomobject]@{
        Name = 'DesktopUi'
        Modules = @('Foundation', 'Cad.Geometry', 'Cad.Database', 'Cad.Editor', 'Cad.Application', 'Cad.Runtime')
        Pattern = '\b(?:System\.Windows(?:\.Forms)?|MessageBox|MessageBoxButtons|MessageBoxIcon|Application\.DoEvents|PaletteSet|StatusBar|PaneStyles|System\.Windows\.Forms\.Cursor)\b'
        AllowedDebtIds = @('BD-02', 'BD-07', 'BD-08', 'BD-11', 'BD-12', 'BD-21')
    },
    [pscustomobject]@{
        Name = 'DatabaseServices'
        Modules = @('Cad.Geometry')
        Pattern = '\b(?:DatabaseServices|DBObject|ObjectId|Database|Transaction|Entity|SymbolTable|BlockTable|Xrecord|XData)\b'
        AllowedDebtIds = @('BD-04', 'BD-24')
    },
    [pscustomobject]@{
        Name = 'ApplicationServices'
        Modules = @('Cad.Geometry', 'Cad.Database')
        Pattern = '\b(?:ApplicationServices|CadCoreApp|CadApp|HostApplicationServices|DocumentManager|DocumentLock|LockDocument|Env\.(?:Document|Database))\b'
        AllowedDebtIds = @('BD-02', 'BD-03', 'BD-04', 'BD-17', 'BD-18')
    },
    [pscustomobject]@{
        Name = 'EditorInput'
        Modules = @('Cad.Geometry', 'Cad.Database')
        Pattern = '\b(?:EditorInput|Env\.(?:Editor|Print|Printl)|DBTrans\.Editor|Prompt[A-Za-z0-9_]*|SelectionSet|Jig[A-Za-z0-9_]*)\b'
        AllowedDebtIds = @('BD-01', 'BD-03', 'BD-04', 'BD-05', 'BD-06')
    },
    [pscustomobject]@{
        Name = 'GraphicsInterface'
        Modules = @('Cad.Geometry')
        Pattern = '\b(?:GraphicsInterface|Drawable|TransientManager|WorldDraw|ViewportDraw)\b'
        AllowedDebtIds = @('BD-04')
    },
    [pscustomobject]@{
        Name = 'CadWindows'
        Modules = @('Foundation', 'Cad.Geometry', 'Cad.Database', 'Cad.Editor', 'Cad.Application', 'Cad.Runtime')
        Pattern = '\b(?:Autodesk\.AutoCAD\.Windows|ZwSoft\.ZwCAD\.Windows|PaletteSet|PaneStyles|StatusBar)\b'
        AllowedDebtIds = @('BD-07')
    }
)

if (-not (Test-Path -LiteralPath $ProjectItemsPath -PathType Leaf)) {
    throw "Project items file not found: $ProjectItemsPath"
}

$xml = [Xml.XmlDocument]::new()
$xml.PreserveWhitespace = $true
$xml.Load($ProjectItemsPath)
$namespaceManager = [Xml.XmlNamespaceManager]::new($xml.NameTable)
$namespaceManager.AddNamespace('m', 'http://schemas.microsoft.com/developer/msbuild/2003')
$compileNodes = @($xml.SelectNodes('//m:Compile', $namespaceManager))

if ($compileNodes.Count -ne $expectedCompileCount) {
    Add-Failure "Expected $expectedCompileCount Compile items, found $($compileNodes.Count)."
}

$seenIncludes = [Collections.Generic.HashSet[string]]::new([StringComparer]::OrdinalIgnoreCase)
$seenOrders = [Collections.Generic.HashSet[string]]::new([StringComparer]::Ordinal)
$items = [Collections.Generic.List[object]]::new()
$previousOrder = -1

foreach ($node in $compileNodes) {
    $include = $node.GetAttribute('Include')
    if ([string]::IsNullOrWhiteSpace($include)) {
        Add-Failure 'A Compile item has no Include attribute.'
        continue
    }
    if ($include.IndexOfAny([char[]]'*?') -ge 0) {
        Add-Failure "Compile glob is not allowed: $include"
    }
    if (-not $seenIncludes.Add($include)) {
        Add-Failure "Duplicate Compile Include: $include"
    }
    if (-not $include.StartsWith($includePrefix, [StringComparison]::Ordinal)) {
        Add-Failure "Compile Include must start with ${includePrefix}: $include"
        continue
    }

    $relativePath = $include.Substring($includePrefix.Length).Replace('/', '\')
    $fullPath = [IO.Path]::GetFullPath((Join-Path $sourceRoot $relativePath))
    if (-not $fullPath.StartsWith($sourceRootPrefix, [StringComparison]::OrdinalIgnoreCase)) {
        Add-Failure "Compile Include escapes CADShared: $include"
        continue
    }
    if (-not (Test-Path -LiteralPath $fullPath -PathType Leaf)) {
        Add-Failure "Compiled source file does not exist: $relativePath"
        continue
    }

    $moduleNodes = @($node.SelectNodes('./m:FsFoxModule', $namespaceManager))
    $orderNodes = @($node.SelectNodes('./m:FsFoxOrder', $namespaceManager))
    $debtNodes = @($node.SelectNodes('./m:FsFoxBoundaryDebt', $namespaceManager))
    if ($moduleNodes.Count -ne 1) {
        Add-Failure "$relativePath must have exactly one FsFoxModule element."
        continue
    }
    if ($orderNodes.Count -ne 1) {
        Add-Failure "$relativePath must have exactly one FsFoxOrder element."
        continue
    }
    if ($debtNodes.Count -gt 1) {
        Add-Failure "$relativePath has more than one FsFoxBoundaryDebt element."
    }

    $module = $moduleNodes[0].InnerText.Trim()
    $order = $orderNodes[0].InnerText.Trim()
    if ($allowedModules -notcontains $module) {
        Add-Failure "$relativePath has unsupported module '$module'."
    }
    if ($order -notmatch '^\d{4}$') {
        Add-Failure "$relativePath has invalid FsFoxOrder '$order'; expected four digits."
    }
    else {
        $numericOrder = [int]$order
        if ($numericOrder -le $previousOrder) {
            Add-Failure "$relativePath has non-increasing FsFoxOrder '$order'."
        }
        $previousOrder = $numericOrder
    }
    if (-not $seenOrders.Add($order)) {
        Add-Failure "Duplicate FsFoxOrder: $order"
    }

    $debts = @()
    if ($debtNodes.Count -eq 1) {
        $debts = @($debtNodes[0].InnerText.Split(';', [StringSplitOptions]::RemoveEmptyEntries) |
            ForEach-Object { $_.Trim() } |
            Sort-Object -Unique)
        foreach ($debt in $debts) {
            if ($allowedDebtIds -notcontains $debt) {
                Add-Failure "$relativePath has unsupported boundary debt '$debt'."
            }
        }
    }

    $targetModule = Get-TargetModule $relativePath
    if ($targetModule -eq '__invalid_target_root__') {
        Add-Failure "$relativePath uses a target ownership root that does not map to a supported module."
    }
    elseif ($null -ne $targetModule -and $targetModule -ne $module) {
        Add-Failure "$relativePath is under target module '$targetModule' but declares '$module'."
    }

    $sourceText = [IO.File]::ReadAllText($fullPath)
    $codeText = Remove-CSharpCommentsAndStrings $sourceText
    $boundaryFindings = [Collections.Generic.List[object]]::new()
    foreach ($rule in $boundaryRules) {
        if ($rule.Modules -notcontains $module) {
            continue
        }
        $matchCount = [regex]::Matches(
            $codeText,
            $rule.Pattern,
            [Text.RegularExpressions.RegexOptions]::IgnoreCase -bor
                [Text.RegularExpressions.RegexOptions]::CultureInvariant
        ).Count
        if ($matchCount -eq 0) {
            continue
        }

        $boundaryFindings.Add([ordered]@{
            rule = $rule.Name
            count = $matchCount
        })
        $hasAllowance = @($debts | Where-Object { $rule.AllowedDebtIds -contains $_ }).Count -gt 0
        if (-not $hasAllowance) {
            Add-Failure "$relativePath introduces forbidden dependency '$($rule.Name)' without a matching BD-xx allowance."
        }
    }

    $items.Add([ordered]@{
        order = $order
        fileName = [IO.Path]::GetFileName($relativePath)
        module = $module
        boundaryDebt = @($debts)
        boundaryFindings = @($boundaryFindings)
        sourceSha256 = Get-NormalizedTextHash $fullPath
    })
}

$moduleCounts = [ordered]@{}
foreach ($module in $allowedModules) {
    $count = @($items | Where-Object { $_.module -eq $module }).Count
    $moduleCounts[$module] = $count
    if ($count -ne $expectedModuleCounts[$module]) {
        Add-Failure "Module $module expected $($expectedModuleCounts[$module]) items, found $count."
    }
}

$todoRoot = Join-Path $sourceRoot 'ExtensionMethod\Geometry\ToDo'
if (-not (Test-Path -LiteralPath $todoRoot -PathType Container)) {
    Add-Failure "Geometry ToDo directory does not exist: $todoRoot"
}
else {
    $todoFiles = @(Get-ChildItem -LiteralPath $todoRoot -Filter '*.cs' -File -Recurse)
    if ($todoFiles.Count -ne 21) {
        Add-Failure "Expected 21 Geometry ToDo files, found $($todoFiles.Count)."
    }
    foreach ($todoFile in $todoFiles) {
        $todoInclude = $includePrefix + [IO.Path]::GetRelativePath($sourceRoot, $todoFile.FullName)
        if ($seenIncludes.Contains($todoInclude)) {
            Add-Failure "Geometry ToDo file must not be compiled: $($todoFile.FullName)"
        }
    }
}

$snapshot = [ordered]@{
    schemaVersion = 1
    normalizedSourceHash = 'sha256-utf8-lf'
    expectedCompileCount = $expectedCompileCount
    moduleCounts = $moduleCounts
    items = @($items)
}

if ($failures.Count -eq 0 -and $UpdateBaseline) {
    $json = $snapshot | ConvertTo-Json -Depth 8
    [IO.File]::WriteAllText($BaselinePath, $json + "`n", [Text.UTF8Encoding]::new($false))
    Write-Host "Updated CADShared module baseline: $BaselinePath"
}
elseif ($failures.Count -eq 0) {
    if (-not (Test-Path -LiteralPath $BaselinePath -PathType Leaf)) {
        Add-Failure 'Module baseline does not exist. Run this script once with -UpdateBaseline.'
    }
    else {
        $baseline = Get-Content -LiteralPath $BaselinePath -Raw | ConvertFrom-Json
        if ($baseline.schemaVersion -ne 1) {
            Add-Failure "Unsupported module baseline schema version '$($baseline.schemaVersion)'."
        }
        $baselineJson = $baseline | ConvertTo-Json -Depth 8 -Compress
        $snapshotJson = $snapshot | ConvertTo-Json -Depth 8 -Compress
        if ($baselineJson -cne $snapshotJson) {
            Add-Failure 'Current module map, source hashes, or boundary findings differ from the committed baseline.'
        }
    }
}

if ($failures.Count -gt 0) {
    Write-Error ("CADShared module verification failed:`n - " + ($failures -join "`n - "))
    exit 1
}

$knownDebtFiles = @($items | Where-Object { $_.boundaryDebt.Count -gt 0 }).Count
$knownFindings = @($items | ForEach-Object { $_.boundaryFindings }).Count
Write-Host "CADShared module verification passed: $($items.Count) items, $knownDebtFiles debt-tagged files, $knownFindings boundary findings."

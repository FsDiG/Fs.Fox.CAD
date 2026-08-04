[CmdletBinding()]
param(
    [ValidateSet('Debug', 'Release')]
    [string]$Configuration = 'Debug'
)

$ErrorActionPreference = 'Stop'
$repositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot '..\..')).Path
$legacyRoot = Join-Path $repositoryRoot 'third_party\Autodesk.MgdDbg'
$sharedRoot = Join-Path $PSScriptRoot 'CADDiagnosticsShared'

function Assert-Condition {
    param(
        [bool]$Condition,
        [string]$Message
    )

    if (-not $Condition) {
        throw $Message
    }
}

function Get-TopLevelPublicTypeNames {
    param([string]$AssemblyPath)

    $stream = [System.IO.File]::OpenRead($AssemblyPath)
    $peReader = $null
    try {
        $peReader = [System.Reflection.PortableExecutable.PEReader]::new($stream)
        $metadata = [System.Reflection.Metadata.PEReaderExtensions]::GetMetadataReader($peReader)
        $names = foreach ($handle in $metadata.TypeDefinitions) {
            $definition = $metadata.GetTypeDefinition($handle)
            $visibility = $definition.Attributes -band [System.Reflection.TypeAttributes]::VisibilityMask
            if ($visibility -ne [System.Reflection.TypeAttributes]::Public) {
                continue
            }

            $namespace = $metadata.GetString($definition.Namespace)
            $name = $metadata.GetString($definition.Name)
            if ($namespace) { "$namespace.$name" } else { $name }
        }
        return @($names | Sort-Object)
    }
    finally {
        if ($null -ne $peReader) {
            $peReader.Dispose()
        }
        $stream.Dispose()
    }
}

function Test-Binary {
    param(
        [string]$SdkYear,
        [string]$AssemblyName
    )

    $outputRoot = Join-Path $repositoryRoot "Build\AC_${SdkYear}_$Configuration"
    $assemblyPath = Join-Path $outputRoot "$AssemblyName.dll"
    $documentationPath = Join-Path $outputRoot "$AssemblyName.xml"

    Assert-Condition (Test-Path -LiteralPath $assemblyPath -PathType Leaf) `
        "Missing diagnostic assembly: $assemblyPath"
    Assert-Condition (Test-Path -LiteralPath $documentationPath -PathType Leaf) `
        "Missing diagnostic XML documentation: $documentationPath"

    $forbiddenSdkFiles = @(
        'AcCoreMgd.dll', 'AcDbMgd.dll', 'AcMgd.dll', 'AcCui.dll',
        'AcWindows.dll', 'AdUIMgd.dll', 'AdWindows.dll'
    )
    foreach ($fileName in $forbiddenSdkFiles) {
        Assert-Condition (-not (Test-Path -LiteralPath (Join-Path $outputRoot $fileName))) `
            "Autodesk SDK assembly must not be copied to diagnostics output: $fileName"
    }

    $assembly = [System.Reflection.Assembly]::LoadFile((Resolve-Path $assemblyPath).Path)
    $references = @($assembly.GetReferencedAssemblies() | ForEach-Object Name)
    Assert-Condition (-not ($references | Where-Object { $_ -like 'Fs.Fox.AutoCad*' })) `
        "$AssemblyName must not reference Fs.Fox.AutoCad."

    $resourceNames = @($assembly.GetManifestResourceNames())
    $reportResources = @($resourceNames | Where-Object {
        $_.StartsWith('Fs.Fox.CAD.Diagnostics.ReportBrowser/', [System.StringComparison]::Ordinal)
    })
    Assert-Condition ($reportResources.Count -eq 19) `
        "$AssemblyName should embed all 19 report-browser resources; found $($reportResources.Count)."
    Assert-Condition ($resourceNames -notcontains 'Fs.Fox.CAD.Diagnostics.LegacyResources/Thumbs.db') `
        "$AssemblyName unexpectedly embeds Thumbs.db."

    $expectedPublicTypes = @(
        'Fs.Fox.CAD.Diagnostics.App',
        'Fs.Fox.CAD.Diagnostics.AutoCad.DiagnosticCommands',
        'Fs.Fox.CAD.Diagnostics.Test.TestCmds'
    ) | Sort-Object
    $actualPublicTypes = Get-TopLevelPublicTypeNames -AssemblyPath $assemblyPath
    $publicTypeDifference = @(Compare-Object $expectedPublicTypes $actualPublicTypes)
    Assert-Condition ($publicTypeDifference.Count -eq 0) `
        "$AssemblyName exposes an unexpected top-level public type surface: $($publicTypeDifference | Out-String)"

    $unexpectedResourceDirectories = @(Get-ChildItem -LiteralPath $outputRoot -Directory -ErrorAction SilentlyContinue |
        Where-Object { $_.Name -like '*CadDiagnostics*' -or $_.Name -like '*ReportBrowser*' })
    Assert-Condition ($unexpectedResourceDirectories.Count -eq 0) `
        "$AssemblyName produced an unexpected resource directory. Resources must remain embedded until a report command runs."

    $bundleOutputs = @(Get-ChildItem -LiteralPath $outputRoot -Recurse -Force -ErrorAction SilentlyContinue |
        Where-Object { $_.Name -like '*.bundle' })
    Assert-Condition ($bundleOutputs.Count -eq 0) `
        "$AssemblyName produced a Bundle even though deployment packaging is out of scope."

    Write-Host "Verified $AssemblyName ($Configuration)."
}

Assert-Condition (Test-Path -LiteralPath $legacyRoot -PathType Container) `
    "The original MgdDbg snapshot must be archived at third_party/Autodesk.MgdDbg."
Assert-Condition (-not (Test-Path -LiteralPath (Join-Path $repositoryRoot 'MgdDbg'))) `
    'The legacy root MgdDbg directory should be moved, not duplicated.'

[xml]$legacyProject = Get-Content -Raw (Join-Path $legacyRoot 'MgdDbg.csproj')
$legacyCompileItems = @($legacyProject.SelectNodes("//*[local-name()='Compile']") |
    ForEach-Object { $_.Include.Replace('\', '/') } |
    Sort-Object)
$migratedFiles = @(Get-ChildItem -LiteralPath $sharedRoot -Recurse -Filter '*.cs' -File |
    ForEach-Object { [System.IO.Path]::GetRelativePath($sharedRoot, $_.FullName).Replace('\', '/') } |
    Sort-Object)
$sourceDifference = @(Compare-Object $legacyCompileItems $migratedFiles)
Assert-Condition ($sourceDifference.Count -eq 0) `
    "The migrated C# source list differs from the legacy compiled source list: $($sourceDifference | Out-String)"
Assert-Condition ($legacyCompileItems.Count -eq 132) `
    "Expected 132 legacy compile items; found $($legacyCompileItems.Count)."

$commandPattern = '\[CommandMethod\s*\(\s*"(?<name>[^"]+)"'
$legacyCommandNames = @(Get-ChildItem -LiteralPath $legacyRoot -Recurse -Filter '*.cs' -File |
    ForEach-Object { [regex]::Matches((Get-Content -Raw $_.FullName), $commandPattern) } |
    ForEach-Object { $_.Groups['name'].Value } |
    Sort-Object -Unique)
$migratedCommandNames = @(Get-ChildItem -LiteralPath $PSScriptRoot -Recurse -Filter '*.cs' -File |
    ForEach-Object { [regex]::Matches((Get-Content -Raw $_.FullName), $commandPattern) } |
    ForEach-Object { $_.Groups['name'].Value } |
    Sort-Object -Unique)
$expectedCommandNames = @($legacyCommandNames + 'MgdDbgAbout' | Sort-Object -Unique)
$commandDifference = @(Compare-Object $expectedCommandNames $migratedCommandNames)
Assert-Condition ($commandDifference.Count -eq 0) `
    "The migrated command-name set differs from the legacy commands plus MgdDbgAbout: $($commandDifference | Out-String)"

$projectFiles = @(
    'Fs.Fox.CAD.Diagnostics.AutoCad2019\Fs.Fox.CAD.Diagnostics.AutoCad2019.csproj',
    'Fs.Fox.CAD.Diagnostics.AutoCad2025\Fs.Fox.CAD.Diagnostics.AutoCad2025.csproj'
)
foreach ($relativeProjectPath in $projectFiles) {
    [xml]$project = Get-Content -Raw (Join-Path $PSScriptRoot $relativeProjectPath)
    $projectReferences = @($project.SelectNodes("//*[local-name()='ProjectReference']"))
    Assert-Condition ($projectReferences.Count -eq 0) `
        "$relativeProjectPath must not contain project references."

    $forbiddenPackages = @($project.SelectNodes("//*[local-name()='PackageReference']") |
        Where-Object { $_.Include -like 'Fs.Fox.*' })
    Assert-Condition ($forbiddenPackages.Count -eq 0) `
        "$relativeProjectPath must not depend on an Fs.Fox package."
}

$migratedSourceText = (Get-ChildItem -LiteralPath $sharedRoot -Recurse -Filter '*.cs' -File |
    ForEach-Object { Get-Content -Raw $_.FullName }) -join "`n"
Assert-Condition ($migratedSourceText -notmatch '\bnamespace\s+MgdDbg\b') `
    'A legacy MgdDbg namespace remains in migrated source.'

Test-Binary -SdkYear '2019' -AssemblyName 'Fs.Fox.CAD.Diagnostics.AutoCad2019'
Test-Binary -SdkYear '2025' -AssemblyName 'Fs.Fox.CAD.Diagnostics.AutoCad2025'

Write-Host "CadDiagnostics migration verification passed for $Configuration."

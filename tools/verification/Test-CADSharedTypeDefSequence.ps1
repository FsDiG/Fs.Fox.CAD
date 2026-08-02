[CmdletBinding()]
param(
    [string]$ComparatorPath
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$repoRoot = Split-Path -Parent (Split-Path -Parent $PSScriptRoot)
if ([string]::IsNullOrWhiteSpace($ComparatorPath)) {
    $ComparatorPath = Join-Path $PSScriptRoot 'Compare-CADSharedTypeDefSequence.ps1'
}
elseif (-not [IO.Path]::IsPathRooted($ComparatorPath)) {
    $ComparatorPath = Join-Path $repoRoot $ComparatorPath
}
$ComparatorPath = [IO.Path]::GetFullPath($ComparatorPath)
if (-not [IO.File]::Exists($ComparatorPath)) {
    throw "Comparator script not found: $ComparatorPath"
}

$targetAssemblies = @(
    'Build\AC_2019_Release\Fs.Fox.AutoCad.dll',
    'Build\AC_2025_Release\Fs.Fox.AutoCad.dll',
    'Build\ZW_2022_Release\Fs.Fox.ZwCad.dll',
    'Build\ZW_2025_Release\Fs.Fox.ZwCad.dll'
)

$orderedSource = @'
namespace FsFoxCad.TypeDefFixture
{
    public class First
    {
    }

    internal class Second
    {
        private sealed class Nested
        {
        }
    }
}
'@

$reorderedSource = @'
namespace FsFoxCad.TypeDefFixture
{
    internal class Second
    {
        private sealed class Nested
        {
        }
    }

    public class First
    {
    }
}
'@

function New-FixtureAssembly {
    param(
        [string]$Path,
        [string]$Source,
        [string]$AssemblyName
    )

    $projectRoot = Join-Path $tempRoot ('project-' + [Guid]::NewGuid().ToString('N'))
    $outputRoot = Join-Path $projectRoot 'output'
    [IO.Directory]::CreateDirectory($projectRoot) | Out-Null
    $projectPath = Join-Path $projectRoot 'Fixture.csproj'
    $sourcePath = Join-Path $projectRoot 'Fixture.cs'
    $projectText = @"
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <AssemblyName>$AssemblyName</AssemblyName>
    <GenerateAssemblyInfo>false</GenerateAssemblyInfo>
    <Deterministic>true</Deterministic>
  </PropertyGroup>
</Project>
"@
    $utf8 = [Text.UTF8Encoding]::new($false)
    [IO.File]::WriteAllText($projectPath, $projectText, $utf8)
    [IO.File]::WriteAllText($sourcePath, $Source, $utf8)

    $buildOutput = @(dotnet build $projectPath --configuration Release --output $outputRoot --nologo --verbosity quiet 2>&1)
    $buildExitCode = $LASTEXITCODE
    if ($buildExitCode -ne 0) {
        $buildOutput | ForEach-Object { Write-Host $_ }
        throw "Fixture build failed for $AssemblyName with exit code $buildExitCode."
    }

    $builtAssembly = Join-Path $outputRoot "$AssemblyName.dll"
    if (-not [IO.File]::Exists($builtAssembly)) {
        throw "Fixture assembly was not produced: $builtAssembly"
    }
    Copy-FixtureAssembly $builtAssembly $Path
}

function Copy-FixtureAssembly {
    param(
        [string]$Source,
        [string]$Destination
    )

    [IO.Directory]::CreateDirectory((Split-Path -Parent $Destination)) | Out-Null
    [IO.File]::Copy($Source, $Destination, $true)
}

function Get-ExpectedFailure {
    param([scriptblock]$Action)

    $previousInformationPreference = $InformationPreference
    try {
        $InformationPreference = 'SilentlyContinue'
        try {
            & $Action
        }
        catch {
            return $_.Exception.Message
        }
    }
    finally {
        $InformationPreference = $previousInformationPreference
    }
    return $null
}

$tempRoot = Join-Path ([IO.Path]::GetTempPath()) ('FsFoxCadTypeDefTest-' + [Guid]::NewGuid().ToString('N'))
$baselineRoot = Join-Path $tempRoot 'baseline'
$candidateRoot = Join-Path $tempRoot 'candidate'
[IO.Directory]::CreateDirectory($baselineRoot) | Out-Null
[IO.Directory]::CreateDirectory($candidateRoot) | Out-Null

try {
    $baselineAcad2019 = Join-Path $baselineRoot $targetAssemblies[0]
    $baselineAcad2025 = Join-Path $baselineRoot $targetAssemblies[1]
    $baselineZw2022 = Join-Path $baselineRoot $targetAssemblies[2]
    $baselineZw2025 = Join-Path $baselineRoot $targetAssemblies[3]

    New-FixtureAssembly $baselineAcad2019 $orderedSource 'Fs.Fox.AutoCad'
    Copy-FixtureAssembly $baselineAcad2019 $baselineAcad2025
    New-FixtureAssembly $baselineZw2022 $orderedSource 'Fs.Fox.ZwCad'
    Copy-FixtureAssembly $baselineZw2022 $baselineZw2025

    foreach ($relativeAssembly in $targetAssemblies) {
        $sourcePath = Join-Path $baselineRoot $relativeAssembly
        $destinationPath = Join-Path $candidateRoot $relativeAssembly
        Copy-FixtureAssembly $sourcePath $destinationPath
    }

    $hashesBefore = @{}
    foreach ($root in @($baselineRoot, $candidateRoot)) {
        foreach ($relativeAssembly in $targetAssemblies) {
            $path = Join-Path $root $relativeAssembly
            $hashesBefore[$path] = (Get-FileHash -LiteralPath $path -Algorithm SHA256).Hash
        }
    }

    & $ComparatorPath -BaselineRoot $baselineRoot -CandidateRoot $candidateRoot

    foreach ($entry in $hashesBefore.GetEnumerator()) {
        $hashAfter = (Get-FileHash -LiteralPath $entry.Key -Algorithm SHA256).Hash
        if ($hashAfter -cne $entry.Value) {
            throw "Comparator modified input assembly: $($entry.Key)"
        }
    }
    Write-Host 'PASS matching fixtures and read-only inputs'

    $candidateAcad2019 = Join-Path $candidateRoot $targetAssemblies[0]
    [IO.File]::Delete($candidateAcad2019)
    New-FixtureAssembly $candidateAcad2019 $reorderedSource 'Fs.Fox.AutoCad'
    $mismatchMessage = Get-ExpectedFailure {
        & $ComparatorPath -BaselineRoot $baselineRoot -CandidateRoot $candidateRoot 6>$null
    }
    if ([string]::IsNullOrWhiteSpace($mismatchMessage)) {
        throw 'Comparator accepted a reordered TypeDef sequence.'
    }
    foreach ($expectedText in @(
            'AC_2019',
            'row=2',
            'token=0x02000002',
            "baseline='FsFoxCad.TypeDefFixture.First'",
            "candidate='FsFoxCad.TypeDefFixture.Second'"
        )) {
        if (-not $mismatchMessage.Contains($expectedText, [StringComparison]::Ordinal)) {
            throw "Reordered sequence failure did not contain '$expectedText': $mismatchMessage"
        }
    }
    Write-Host 'PASS reordered sequence reports the first TypeDef row and token'

    Copy-FixtureAssembly $baselineAcad2019 $candidateAcad2019
    $candidateZw2025 = Join-Path $candidateRoot $targetAssemblies[3]
    [IO.File]::Delete($candidateZw2025)
    $missingMessage = Get-ExpectedFailure {
        & $ComparatorPath -BaselineRoot $baselineRoot -CandidateRoot $candidateRoot 6>$null
    }
    if ([string]::IsNullOrWhiteSpace($missingMessage) -or
        -not $missingMessage.Contains('ZW_2025', [StringComparison]::Ordinal) -or
        -not $missingMessage.Contains('Assembly not found', [StringComparison]::Ordinal)) {
        throw "Missing assembly failure was not target-specific: $missingMessage"
    }
    Write-Host 'PASS missing target assembly fails with its target name'

    Copy-FixtureAssembly $baselineZw2025 $candidateZw2025
    $candidateZw2022 = Join-Path $candidateRoot $targetAssemblies[2]
    Copy-FixtureAssembly $baselineAcad2019 $candidateZw2022
    $identityMessage = Get-ExpectedFailure {
        & $ComparatorPath -BaselineRoot $baselineRoot -CandidateRoot $candidateRoot 6>$null
    }
    if ([string]::IsNullOrWhiteSpace($identityMessage) -or
        -not $identityMessage.Contains('ZW_2022', [StringComparison]::Ordinal) -or
        -not $identityMessage.Contains(
            "candidate assembly name 'Fs.Fox.AutoCad' does not match 'Fs.Fox.ZwCad'",
            [StringComparison]::Ordinal
        )) {
        throw "Assembly identity failure was not target-specific: $identityMessage"
    }
    Write-Host 'PASS wrong target assembly identity is rejected'

    Write-Host 'CADShared TypeDef sequence comparator tests passed.'
}
finally {
    $resolvedTempRoot = [IO.Path]::GetFullPath($tempRoot)
    $tempPrefix = [IO.Path]::GetFullPath([IO.Path]::GetTempPath())
    if (-not $resolvedTempRoot.StartsWith($tempPrefix, [StringComparison]::OrdinalIgnoreCase)) {
        throw "Refusing to remove test directory outside the system temp root: $resolvedTempRoot"
    }
    if ([IO.Directory]::Exists($resolvedTempRoot)) {
        [IO.Directory]::Delete($resolvedTempRoot, $true)
    }
}

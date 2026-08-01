[CmdletBinding()]
param(
    [Parameter(Mandatory)]
    [ValidateNotNullOrEmpty()]
    [string]$BaselineRoot,

    [Parameter(Mandatory)]
    [ValidateNotNullOrEmpty()]
    [string]$CandidateRoot
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

if ($null -eq ('System.Reflection.Metadata.MetadataReader' -as [type])) {
    Add-Type -AssemblyName System.Reflection.Metadata
}

$targets = @(
    [pscustomobject]@{
        Name = 'AC_2019'
        RelativeAssembly = 'Build\AC_2019_Release\Fs.Fox.AutoCad.dll'
        ExpectedAssemblyName = 'Fs.Fox.AutoCad'
    },
    [pscustomobject]@{
        Name = 'AC_2025'
        RelativeAssembly = 'Build\AC_2025_Release\Fs.Fox.AutoCad.dll'
        ExpectedAssemblyName = 'Fs.Fox.AutoCad'
    },
    [pscustomobject]@{
        Name = 'ZW_2022'
        RelativeAssembly = 'Build\ZW_2022_Release\Fs.Fox.ZwCad.dll'
        ExpectedAssemblyName = 'Fs.Fox.ZwCad'
    },
    [pscustomobject]@{
        Name = 'ZW_2025'
        RelativeAssembly = 'Build\ZW_2025_Release\Fs.Fox.ZwCad.dll'
        ExpectedAssemblyName = 'Fs.Fox.ZwCad'
    }
)

function Resolve-ArtifactRoot {
    param(
        [string]$Path,
        [string]$ParameterName
    )

    if ([IO.Path]::IsPathRooted($Path)) {
        $fullPath = [IO.Path]::GetFullPath($Path)
    }
    else {
        $fullPath = [IO.Path]::GetFullPath((Join-Path (Get-Location).Path $Path))
    }

    if (-not [IO.Directory]::Exists($fullPath)) {
        throw "$ParameterName directory not found: $fullPath"
    }
    return $fullPath
}

function Convert-BytesToHex {
    param([byte[]]$Bytes)

    if ($null -eq $Bytes -or $Bytes.Length -eq 0) {
        return ''
    }
    return ([Convert]::ToHexString($Bytes)).ToLowerInvariant()
}

function Get-TextHash {
    param([string]$Text)

    $bytes = [Text.UTF8Encoding]::new($false).GetBytes($Text)
    $sha256 = [Security.Cryptography.SHA256]::Create()
    try {
        $hashBytes = $sha256.ComputeHash($bytes)
        return Convert-BytesToHex $hashBytes
    }
    finally {
        $sha256.Dispose()
    }
}

function Get-MetadataToken {
    param([Reflection.Metadata.Handle]$Handle)

    return [Reflection.Metadata.Ecma335.MetadataTokens]::GetToken($Handle)
}

function Get-TypeDefinitionName {
    param(
        [Reflection.Metadata.MetadataReader]$Reader,
        [Reflection.Metadata.TypeDefinitionHandle]$Handle,
        [hashtable]$Cache
    )

    $token = Get-MetadataToken $Handle
    $cacheKey = '{0:x8}' -f $token
    if ($Cache.ContainsKey($cacheKey)) {
        return $Cache[$cacheKey]
    }

    $definition = $Reader.GetTypeDefinition($Handle)
    $name = $Reader.GetString($definition.Name)
    $declaringType = $definition.GetDeclaringType()
    if (-not $declaringType.IsNil) {
        $fullName = (Get-TypeDefinitionName $Reader $declaringType $Cache) + '+' + $name
    }
    else {
        $namespace = $Reader.GetString($definition.Namespace)
        $fullName = if ([string]::IsNullOrEmpty($namespace)) { $name } else { "$namespace.$name" }
    }

    $Cache[$cacheKey] = $fullName
    return $fullName
}

function Get-TypeDefinitionSnapshot {
    param([string]$AssemblyPath)

    if (-not [IO.File]::Exists($AssemblyPath)) {
        throw "Assembly not found: $AssemblyPath"
    }

    $stream = [IO.File]::Open(
        $AssemblyPath,
        [IO.FileMode]::Open,
        [IO.FileAccess]::Read,
        [IO.FileShare]::Read
    )
    try {
        $peReader = [Reflection.PortableExecutable.PEReader]::new($stream)
        try {
            if (-not $peReader.HasMetadata) {
                throw "Assembly has no managed metadata: $AssemblyPath"
            }

            $reader = [Reflection.Metadata.PEReaderExtensions]::GetMetadataReader($peReader)
            if (-not $reader.IsAssembly) {
                throw "Managed PE does not contain an assembly definition: $AssemblyPath"
            }

            $assemblyDefinition = $reader.GetAssemblyDefinition()
            $assemblyName = $reader.GetString($assemblyDefinition.Name)
            $typeNameCache = @{}
            $typeNames = [Collections.Generic.List[string]]::new()
            foreach ($typeHandle in $reader.TypeDefinitions) {
                $typeNames.Add((Get-TypeDefinitionName $reader $typeHandle $typeNameCache))
            }

            $sequence = [string]::Join("`n", $typeNames)
            return [pscustomobject]@{
                AssemblyName = $assemblyName
                TypeNames = $typeNames.ToArray()
                Hash = (Get-TextHash $sequence)
            }
        }
        finally {
            $peReader.Dispose()
        }
    }
    finally {
        $stream.Dispose()
    }
}

function Get-FirstSequenceDifference {
    param(
        [string[]]$BaselineNames,
        [string[]]$CandidateNames
    )

    $commonCount = [Math]::Min($BaselineNames.Length, $CandidateNames.Length)
    for ($index = 0; $index -lt $commonCount; $index++) {
        if ($BaselineNames[$index] -cne $CandidateNames[$index]) {
            $row = $index + 1
            return [pscustomobject]@{
                Row = $row
                Token = ('0x{0:x8}' -f (0x02000000 + $row))
                Baseline = $BaselineNames[$index]
                Candidate = $CandidateNames[$index]
            }
        }
    }

    if ($BaselineNames.Length -ne $CandidateNames.Length) {
        $row = $commonCount + 1
        return [pscustomobject]@{
            Row = $row
            Token = ('0x{0:x8}' -f (0x02000000 + $row))
            Baseline = if ($row -le $BaselineNames.Length) { $BaselineNames[$row - 1] } else { '<end>' }
            Candidate = if ($row -le $CandidateNames.Length) { $CandidateNames[$row - 1] } else { '<end>' }
        }
    }

    return $null
}

$resolvedBaselineRoot = Resolve-ArtifactRoot $BaselineRoot 'BaselineRoot'
$resolvedCandidateRoot = Resolve-ArtifactRoot $CandidateRoot 'CandidateRoot'
$failures = [Collections.Generic.List[string]]::new()

foreach ($target in $targets) {
    $baselineAssembly = Join-Path $resolvedBaselineRoot $target.RelativeAssembly
    $candidateAssembly = Join-Path $resolvedCandidateRoot $target.RelativeAssembly

    try {
        $baseline = Get-TypeDefinitionSnapshot $baselineAssembly
        $candidate = Get-TypeDefinitionSnapshot $candidateAssembly
        $targetFailures = [Collections.Generic.List[string]]::new()

        if ($baseline.AssemblyName -cne $target.ExpectedAssemblyName) {
            $targetFailures.Add(
                "baseline assembly name '$($baseline.AssemblyName)' does not match '$($target.ExpectedAssemblyName)'"
            )
        }
        if ($candidate.AssemblyName -cne $target.ExpectedAssemblyName) {
            $targetFailures.Add(
                "candidate assembly name '$($candidate.AssemblyName)' does not match '$($target.ExpectedAssemblyName)'"
            )
        }

        $difference = Get-FirstSequenceDifference $baseline.TypeNames $candidate.TypeNames
        if ($null -ne $difference) {
            $targetFailures.Add(
                "first difference row=$($difference.Row) token=$($difference.Token) " +
                "baseline='$($difference.Baseline)' candidate='$($difference.Candidate)'"
            )
        }

        $status = if ($targetFailures.Count -eq 0) { 'MATCH' } else { 'MISMATCH' }
        Write-Host (
            "$($target.Name) $status " +
            "baselineCount=$($baseline.TypeNames.Length) candidateCount=$($candidate.TypeNames.Length) " +
            "baselineHash=$($baseline.Hash) candidateHash=$($candidate.Hash)"
        )

        foreach ($targetFailure in $targetFailures) {
            Write-Host "  $targetFailure"
            $failures.Add("$($target.Name): $targetFailure")
        }
    }
    catch {
        $message = $_.Exception.Message
        Write-Host "$($target.Name) ERROR $message"
        $failures.Add("$($target.Name): $message")
    }
}

if ($failures.Count -gt 0) {
    throw "CADShared TypeDef sequence comparison failed:`n - $($failures -join "`n - ")"
}

Write-Host "CADShared TypeDef sequence comparison passed for $($targets.Count) targets."

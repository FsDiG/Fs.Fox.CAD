[CmdletBinding(DefaultParameterSetName = "Run")]
param(
    [Parameter(Mandatory = $true)]
    [ValidateSet("AutoCAD", "ZWCAD")]
    [string] $Product,

    [Parameter(Mandatory = $true)]
    [string] $Scenario,

    [Parameter(Mandatory = $true, ParameterSetName = "Run")]
    [string] $CadExecutable,

    [Parameter(Mandatory = $true, ParameterSetName = "Run")]
    [string] $TestAssembly,

    [Parameter(ParameterSetName = "Run")]
    [string] $Drawing,

    [Parameter(ParameterSetName = "Run")]
    [string[]] $AdditionalArguments = @(),

    [Parameter(ParameterSetName = "Run")]
    [switch] $GenerateOnly,

    [Parameter(ParameterSetName = "Run")]
    [switch] $TerminateOnTimeout,

    [Parameter(Mandatory = $true, ParameterSetName = "Analyze")]
    [string] $LogFile,

    [string] $HostLabel,

    [string] $OutputDirectory = (Join-Path $PSScriptRoot "artifacts"),

    [ValidateRange(10, 3600)]
    [int] $TimeoutSeconds = 180
)

Set-StrictMode -Version 2.0
$ErrorActionPreference = "Stop"

$script:DefaultFailurePatterns = @(
    "System.EntryPointNotFoundException",
    "System.DllNotFoundException",
    "System.AccessViolationException",
    "Unhandled exception"
)

function Get-AbsolutePath {
    param(
        [Parameter(Mandatory = $true)]
        [string] $Path
    )

    if ([System.IO.Path]::IsPathRooted($Path)) {
        return [System.IO.Path]::GetFullPath($Path)
    }

    return [System.IO.Path]::GetFullPath((Join-Path (Get-Location) $Path))
}

function Resolve-InputFile {
    param(
        [Parameter(Mandatory = $true)]
        [string] $Path,

        [Parameter(Mandatory = $true)]
        [string] $Description
    )

    $absolutePath = Get-AbsolutePath -Path $Path
    if (-not (Test-Path -LiteralPath $absolutePath -PathType Leaf)) {
        throw "$Description was not found: $absolutePath"
    }

    return $absolutePath
}

function Test-ObjectProperty {
    param(
        [Parameter(Mandatory = $true)]
        [object] $InputObject,

        [Parameter(Mandatory = $true)]
        [string] $Name
    )

    return $null -ne $InputObject.PSObject.Properties[$Name]
}

function ConvertTo-AutoLispString {
    param(
        [Parameter(Mandatory = $true)]
        [string] $Value,

        [Parameter(Mandatory = $true)]
        [string] $Description
    )

    if ($Value.IndexOf("`r", [System.StringComparison]::Ordinal) -ge 0 -or
        $Value.IndexOf("`n", [System.StringComparison]::Ordinal) -ge 0 -or
        $Value.IndexOf([char]0) -ge 0) {
        throw "$Description contains a control character that cannot be written to a CAD script."
    }

    foreach ($character in $Value.ToCharArray()) {
        if ([int] $character -gt 127) {
            throw "$Description must use an ASCII path for the initial runner: $Value"
        }
    }

    return $Value.Replace("\", "/").Replace('"', '\"')
}

function ConvertTo-WindowsArgument {
    param(
        [Parameter(Mandatory = $true)]
        [string] $Value
    )

    if ($Value.IndexOf('"', [System.StringComparison]::Ordinal) -ge 0) {
        throw "A CAD process argument contains an unsupported double quote: $Value"
    }

    return '"' + $Value + '"'
}

function Get-GitCommit {
    param(
        [Parameter(Mandatory = $true)]
        [string] $RepositoryRoot
    )

    try {
        $commit = & git -C $RepositoryRoot rev-parse HEAD 2>$null
        if ($LASTEXITCODE -eq 0 -and -not [string]::IsNullOrWhiteSpace($commit)) {
            return $commit.Trim()
        }
    }
    catch {
        return $null
    }

    return $null
}

function Read-Scenario {
    param(
        [Parameter(Mandatory = $true)]
        [string] $ScenarioPath,

        [Parameter(Mandatory = $true)]
        [string] $SelectedProduct
    )

    $scenarioText = Get-Content -LiteralPath $ScenarioPath -Raw
    $scenarioObject = $scenarioText | ConvertFrom-Json

    if (-not (Test-ObjectProperty -InputObject $scenarioObject -Name "schemaVersion") -or
        [int] $scenarioObject.schemaVersion -ne 1) {
        throw "Scenario schemaVersion must be 1."
    }

    if (-not (Test-ObjectProperty -InputObject $scenarioObject -Name "name") -or
        [string]::IsNullOrWhiteSpace([string] $scenarioObject.name)) {
        throw "Scenario name is required."
    }

    $scenarioProducts = @($scenarioObject.products)
    if ($scenarioProducts.Count -eq 0 -or $scenarioProducts -notcontains $SelectedProduct) {
        throw "Scenario '$($scenarioObject.name)' does not support $SelectedProduct."
    }

    $rawCommands = @($scenarioObject.commands)
    if ($rawCommands.Count -eq 0) {
        throw "Scenario '$($scenarioObject.name)' does not define commands."
    }

    $commands = New-Object System.Collections.Generic.List[object]
    foreach ($rawCommand in $rawCommands) {
        if (-not (Test-ObjectProperty -InputObject $rawCommand -Name "command") -or
            [string]::IsNullOrWhiteSpace([string] $rawCommand.command)) {
            throw "Every scenario command requires a command value."
        }

        $commandName = [string] $rawCommand.command
        if ($commandName -notmatch '^[A-Za-z_][A-Za-z0-9_]*$') {
            throw "Scenario command contains unsupported characters: $commandName"
        }

        if (Test-ObjectProperty -InputObject $rawCommand -Name "products") {
            $commandProducts = @($rawCommand.products)
            if ($commandProducts.Count -gt 0 -and $commandProducts -notcontains $SelectedProduct) {
                continue
            }
        }

        if (-not (Test-ObjectProperty -InputObject $rawCommand -Name "expectedText") -or
            [string]::IsNullOrWhiteSpace([string] $rawCommand.expectedText)) {
            throw "Scenario command '$commandName' requires expectedText."
        }

        $displayName = $commandName
        if ((Test-ObjectProperty -InputObject $rawCommand -Name "name") -and
            -not [string]::IsNullOrWhiteSpace([string] $rawCommand.name)) {
            $displayName = [string] $rawCommand.name
        }

        $commands.Add([pscustomobject] [ordered] @{
                name         = $displayName
                command      = $commandName
                expectedText = [string] $rawCommand.expectedText
            })
    }

    if ($commands.Count -eq 0) {
        throw "Scenario '$($scenarioObject.name)' has no commands for $SelectedProduct."
    }

    $requiresDrawing = $false
    if (Test-ObjectProperty -InputObject $scenarioObject -Name "requiresDrawing") {
        $requiresDrawing = [bool] $scenarioObject.requiresDrawing
    }

    $failurePatterns = @()
    if (Test-ObjectProperty -InputObject $scenarioObject -Name "failurePatterns") {
        $failurePatterns = @($scenarioObject.failurePatterns)
    }

    $description = ""
    if (Test-ObjectProperty -InputObject $scenarioObject -Name "description") {
        $description = [string] $scenarioObject.description
    }

    return [pscustomobject] [ordered] @{
        name             = [string] $scenarioObject.name
        description      = $description
        products         = $scenarioProducts
        requiresDrawing  = $requiresDrawing
        commands         = $commands.ToArray()
        failurePatterns  = $failurePatterns
    }
}

function New-ExpectationResults {
    param(
        [Parameter(Mandatory = $true)]
        [object[]] $Commands
    )

    $expectations = New-Object System.Collections.Generic.List[object]
    $expectationByText = @{}

    foreach ($command in $Commands) {
        $expectedText = [string] $command.expectedText
        if ($expectationByText.ContainsKey($expectedText)) {
            $expectationByText[$expectedText].required += 1
            continue
        }

        $expectation = [pscustomobject] [ordered] @{
            text     = $expectedText
            required = 1
            observed = 0
            status   = "NotRun"
        }
        $expectationByText[$expectedText] = $expectation
        $expectations.Add($expectation)
    }

    return $expectations.ToArray()
}

function Get-TextOccurrenceCount {
    param(
        [Parameter(Mandatory = $true)]
        [string] $Text,

        [Parameter(Mandatory = $true)]
        [string] $Needle
    )

    if ([string]::IsNullOrEmpty($Needle)) {
        return 0
    }

    return [regex]::Matches(
        $Text,
        [regex]::Escape($Needle),
        [System.Text.RegularExpressions.RegexOptions]::CultureInvariant).Count
}

function Evaluate-LogText {
    param(
        [Parameter(Mandatory = $true)]
        [string] $Text,

        [Parameter(Mandatory = $true)]
        [object[]] $Expectations,

        [Parameter(Mandatory = $true)]
        [string[]] $FailurePatterns
    )

    $missingExpectations = 0
    foreach ($expectation in $Expectations) {
        $expectation.observed = Get-TextOccurrenceCount -Text $Text -Needle $expectation.text
        if ($expectation.observed -ge $expectation.required) {
            $expectation.status = "Passed"
        }
        else {
            $expectation.status = "Missing"
            $missingExpectations += 1
        }
    }

    $matchedFailures = New-Object System.Collections.Generic.List[string]
    foreach ($failurePattern in $FailurePatterns) {
        if ([string]::IsNullOrWhiteSpace($failurePattern)) {
            continue
        }

        if ($Text.IndexOf($failurePattern, [System.StringComparison]::OrdinalIgnoreCase) -ge 0) {
            $matchedFailures.Add($failurePattern)
        }
    }

    $skipLines = @($Text -split "`r?`n" | Where-Object { $_ -match '\[SKIP\]' })
    $status = "Failed"
    if ($matchedFailures.Count -gt 0) {
        $status = "Failed"
    }
    elseif ($missingExpectations -eq 0) {
        $status = "Passed"
    }
    elseif ($skipLines.Count -gt 0) {
        $status = "Skipped"
    }

    return [pscustomobject] [ordered] @{
        status          = $status
        matchedFailures = $matchedFailures.ToArray()
        skipLines       = $skipLines
    }
}

function New-CadScript {
    param(
        [Parameter(Mandatory = $true)]
        [string] $AssemblyPath,

        [Parameter(Mandatory = $true)]
        [object[]] $Commands,

        [Parameter(Mandatory = $true)]
        [string] $MarkerPath,

        [Parameter(Mandatory = $true)]
        [string] $CompletionToken,

        [Parameter(Mandatory = $true)]
        [string] $Destination
    )

    $assemblyValue = ConvertTo-AutoLispString -Value $AssemblyPath -Description "Test assembly path"
    $markerValue = ConvertTo-AutoLispString -Value $MarkerPath -Description "Marker path"
    $completionValue = ConvertTo-AutoLispString -Value $CompletionToken -Description "Completion token"

    $lines = New-Object System.Collections.Generic.List[string]
    $lines.Add("(vl-load-com)")
    $lines.Add('(vl-catch-all-apply ''setvar (list "FILEDIA" 0))')
    $lines.Add('(vl-catch-all-apply ''setvar (list "CMDECHO" 1))')
    $lines.Add('(vl-catch-all-apply ''setvar (list "LOGFILEMODE" 1))')
    $lines.Add(('(command "_.NETLOAD" "{0}")' -f $assemblyValue))

    foreach ($command in $Commands) {
        $lines.Add(('(command "{0}")' -f $command.command))
    }

    $lines.Add(('(setq fsfox-file (open "{0}" "w"))' -f $markerValue))
    $lines.Add(('(write-line "{0}" fsfox-file)' -f $completionValue))
    $lines.Add('(setq fsfox-log (vl-catch-all-apply ''getvar (list "LOGFILENAME")))')
    $lines.Add('(if (vl-catch-all-error-p fsfox-log)')
    $lines.Add('  (write-line "" fsfox-file)')
    $lines.Add('  (write-line (vl-princ-to-string fsfox-log) fsfox-file))')
    $lines.Add('(close fsfox-file)')
    $lines.Add('(command "_.QUIT" "_N")')

    $lines | Set-Content -LiteralPath $Destination -Encoding ASCII
}

function ConvertTo-MarkdownCell {
    param([object] $Value)

    if ($null -eq $Value) {
        return ""
    }

    return ([string] $Value).Replace("|", "\|").Replace("`r", " ").Replace("`n", " ")
}

function Write-ResultArtifacts {
    param(
        [Parameter(Mandatory = $true)]
        [System.Collections.IDictionary] $Result,

        [Parameter(Mandatory = $true)]
        [string] $RunDirectory
    )

    $jsonPath = Join-Path $RunDirectory "result.json"
    $summaryPath = Join-Path $RunDirectory "summary.md"
    $Result | ConvertTo-Json -Depth 10 | Set-Content -LiteralPath $jsonPath -Encoding UTF8

    $summary = New-Object System.Collections.Generic.List[string]
    $summary.Add("# CAD Host Acceptance Result")
    $summary.Add("")
    $summary.Add("| Field | Value |")
    $summary.Add("| --- | --- |")
    $summary.Add("| Status | $(ConvertTo-MarkdownCell $Result.status) |")
    $summary.Add("| Product | $(ConvertTo-MarkdownCell $Result.host.product) |")
    $summary.Add("| Host label | $(ConvertTo-MarkdownCell $Result.host.label) |")
    $summary.Add("| Host version | $(ConvertTo-MarkdownCell $Result.host.fileVersion) |")
    $summary.Add("| Scenario | $(ConvertTo-MarkdownCell $Result.scenario.name) |")
    $summary.Add("| Git commit | $(ConvertTo-MarkdownCell $Result.gitCommit) |")
    $summary.Add("| Test assembly SHA-256 | $(ConvertTo-MarkdownCell $Result.testAssembly.sha256) |")
    $summary.Add("| Process exit code | $(ConvertTo-MarkdownCell $Result.process.exitCode) |")
    $summary.Add("| Duration seconds | $(ConvertTo-MarkdownCell $Result.durationSeconds) |")
    $summary.Add("")
    $summary.Add("## Expectations")
    $summary.Add("")
    $summary.Add("| Expected text | Required | Observed | Status |")
    $summary.Add("| --- | ---: | ---: | --- |")
    foreach ($expectation in $Result.expectations) {
        $summary.Add("| $(ConvertTo-MarkdownCell $expectation.text) | $($expectation.required) | $($expectation.observed) | $($expectation.status) |")
    }

    $summary.Add("")
    $summary.Add("## Diagnostics")
    $summary.Add("")
    if (@($Result.diagnostics).Count -eq 0) {
        $summary.Add("None.")
    }
    else {
        foreach ($diagnostic in $Result.diagnostics) {
            $summary.Add("- $(ConvertTo-MarkdownCell $diagnostic)")
        }
    }

    if (@($Result.skipLines).Count -gt 0) {
        $summary.Add("")
        $summary.Add("## Skip output")
        $summary.Add("")
        foreach ($skipLine in $Result.skipLines) {
            $summary.Add("- $(ConvertTo-MarkdownCell $skipLine)")
        }
    }

    $summary | Set-Content -LiteralPath $summaryPath -Encoding UTF8
    return [pscustomobject] @{
        JsonPath    = $jsonPath
        SummaryPath = $summaryPath
    }
}

$startedAt = [DateTime]::UtcNow
$runId = (Get-Date -Format "yyyyMMdd-HHmmss") + "-" + [Guid]::NewGuid().ToString("N").Substring(0, 8)
$outputRoot = Get-AbsolutePath -Path $OutputDirectory
$runDirectory = Join-Path $outputRoot $runId
New-Item -ItemType Directory -Force -Path $runDirectory | Out-Null

$repositoryRoot = [System.IO.Path]::GetFullPath((Join-Path $PSScriptRoot "..\.."))
$result = [ordered] @{
    schemaVersion         = 1
    runId                 = $runId
    status                = "InfrastructureError"
    startedAtUtc          = $startedAt.ToString("o")
    completedAtUtc        = $null
    durationSeconds       = 0
    gitCommit             = Get-GitCommit -RepositoryRoot $repositoryRoot
    scenario              = [ordered] @{
        name        = $null
        path        = $null
        description = $null
    }
    host                  = [ordered] @{
        product     = $Product
        label       = $HostLabel
        executable  = $null
        fileVersion = $null
    }
    testAssembly          = [ordered] @{
        path   = $null
        sha256 = $null
    }
    drawing               = [ordered] @{
        source  = $null
        working = $null
    }
    generatedScript       = $null
    completionMarker      = $null
    completionMarkerFound = $false
    logPath               = $null
    process               = [ordered] @{
        id       = $null
        exited   = $null
        exitCode = $null
    }
    expectations          = @()
    matchedFailurePatterns = @()
    skipLines             = @()
    diagnostics           = @()
}

try {
    $scenarioPath = Resolve-InputFile -Path $Scenario -Description "Scenario"
    $scenarioDefinition = Read-Scenario -ScenarioPath $scenarioPath -SelectedProduct $Product
    $expectations = New-ExpectationResults -Commands $scenarioDefinition.commands

    $result.scenario.name = $scenarioDefinition.name
    $result.scenario.path = $scenarioPath
    $result.scenario.description = $scenarioDefinition.description
    $result.expectations = $expectations

    $failurePatterns = @($script:DefaultFailurePatterns) + @($scenarioDefinition.failurePatterns)

    if ($PSCmdlet.ParameterSetName -eq "Analyze") {
        $resolvedLog = Resolve-InputFile -Path $LogFile -Description "CAD log"
        $logText = Get-Content -LiteralPath $resolvedLog -Raw
        $evaluation = Evaluate-LogText -Text $logText -Expectations $expectations -FailurePatterns $failurePatterns
        $result.logPath = $resolvedLog
        $result.status = $evaluation.status
        $result.matchedFailurePatterns = $evaluation.matchedFailures
        $result.skipLines = $evaluation.skipLines
        if ($evaluation.matchedFailures.Count -gt 0) {
            $result.diagnostics += "CAD log contains a configured failure pattern."
        }
    }
    else {
        $assemblyPath = Resolve-InputFile -Path $TestAssembly -Description "Test assembly"
        if ([System.IO.Path]::GetExtension($assemblyPath) -ne ".dll") {
            throw "TestAssembly must point to a .dll file."
        }

        $result.testAssembly.path = $assemblyPath
        $result.testAssembly.sha256 = (Get-FileHash -LiteralPath $assemblyPath -Algorithm SHA256).Hash

        $cadExecutablePath = Get-AbsolutePath -Path $CadExecutable
        $result.host.executable = $cadExecutablePath
        if (Test-Path -LiteralPath $cadExecutablePath -PathType Leaf) {
            $result.host.fileVersion = (Get-Item -LiteralPath $cadExecutablePath).VersionInfo.FileVersion
        }
        elseif (-not $GenerateOnly) {
            throw "CAD executable was not found: $cadExecutablePath"
        }

        $workingDrawing = $null
        if (-not [string]::IsNullOrWhiteSpace($Drawing)) {
            $drawingPath = Resolve-InputFile -Path $Drawing -Description "Drawing"
            $workingDrawing = Join-Path $runDirectory ("input-" + [System.IO.Path]::GetFileName($drawingPath))
            Copy-Item -LiteralPath $drawingPath -Destination $workingDrawing
            $result.drawing.source = $drawingPath
            $result.drawing.working = $workingDrawing
        }
        elseif ($scenarioDefinition.requiresDrawing) {
            throw "Scenario '$($scenarioDefinition.name)' requires -Drawing."
        }

        $scriptPath = Join-Path $runDirectory "run.scr"
        $markerPath = Join-Path $runDirectory "completion.marker"
        $completionToken = "FSFOX_HOST_ACCEPTANCE_COMPLETED $runId"
        New-CadScript -AssemblyPath $assemblyPath -Commands $scenarioDefinition.commands `
            -MarkerPath $markerPath -CompletionToken $completionToken -Destination $scriptPath

        $result.generatedScript = $scriptPath
        $result.completionMarker = $markerPath

        if ($GenerateOnly) {
            $result.status = "Generated"
        }
        else {
            $launchArguments = New-Object System.Collections.Generic.List[string]
            foreach ($additionalArgument in $AdditionalArguments) {
                $launchArguments.Add([string] $additionalArgument)
            }
            if (-not [string]::IsNullOrWhiteSpace($workingDrawing)) {
                $launchArguments.Add($workingDrawing)
            }
            $launchArguments.Add("/b")
            $launchArguments.Add($scriptPath)

            $processStartInfo = New-Object System.Diagnostics.ProcessStartInfo
            $processStartInfo.FileName = $cadExecutablePath
            $processStartInfo.Arguments = ($launchArguments.ToArray() | ForEach-Object {
                    ConvertTo-WindowsArgument -Value $_
                }) -join " "
            $processStartInfo.WorkingDirectory = $runDirectory
            $processStartInfo.UseShellExecute = $true

            $process = [System.Diagnostics.Process]::Start($processStartInfo)
            if ($null -eq $process) {
                throw "CAD process did not start."
            }

            $result.process.id = $process.Id
            $deadline = [DateTime]::UtcNow.AddSeconds($TimeoutSeconds)
            $timedOut = $false
            while (-not $process.HasExited) {
                if ([DateTime]::UtcNow -ge $deadline) {
                    $timedOut = $true
                    break
                }
                Start-Sleep -Milliseconds 250
            }

            if ($timedOut) {
                $result.status = "TimedOut"
                $result.process.exited = $false
                $result.diagnostics += "CAD did not exit within $TimeoutSeconds seconds. Process id: $($process.Id)."
                if ($TerminateOnTimeout -and -not $process.HasExited) {
                    $process.Kill()
                    $process.WaitForExit()
                    $result.process.exited = $true
                    $result.process.exitCode = $process.ExitCode
                    $result.diagnostics += "The runner terminated the process it created after timeout."
                }
            }
            else {
                $process.WaitForExit()
                $result.process.exited = $true
                $result.process.exitCode = $process.ExitCode
                $result.completionMarkerFound = Test-Path -LiteralPath $markerPath -PathType Leaf

                if (-not $result.completionMarkerFound) {
                    $result.status = "Failed"
                    $result.diagnostics += "CAD exited without writing the completion marker."
                }
                else {
                    $markerLines = @(Get-Content -LiteralPath $markerPath)
                    if ($markerLines.Count -lt 1 -or $markerLines[0] -ne $completionToken) {
                        $result.status = "Failed"
                        $result.diagnostics += "Completion marker token did not match this run."
                    }
                    elseif ($markerLines.Count -lt 2 -or [string]::IsNullOrWhiteSpace($markerLines[1])) {
                        $result.status = "Failed"
                        $result.diagnostics += "Completion marker did not contain LOGFILENAME."
                    }
                    else {
                        $cadLogPath = $markerLines[1].Trim().Trim('"')
                        $result.logPath = $cadLogPath
                        if (-not (Test-Path -LiteralPath $cadLogPath -PathType Leaf)) {
                            $result.status = "Failed"
                            $result.diagnostics += "CAD log was not found: $cadLogPath"
                        }
                        else {
                            $logText = Get-Content -LiteralPath $cadLogPath -Raw
                            $evaluation = Evaluate-LogText -Text $logText -Expectations $expectations -FailurePatterns $failurePatterns
                            $result.status = $evaluation.status
                            $result.matchedFailurePatterns = $evaluation.matchedFailures
                            $result.skipLines = $evaluation.skipLines
                            if ($evaluation.matchedFailures.Count -gt 0) {
                                $result.diagnostics += "CAD log contains a configured failure pattern."
                            }
                        }
                    }
                }
            }
        }
    }
}
catch {
    $result.status = "InfrastructureError"
    $result.diagnostics += $_.Exception.Message
    if (-not [string]::IsNullOrWhiteSpace($_.ScriptStackTrace)) {
        $result.diagnostics += $_.ScriptStackTrace
    }
}
finally {
    $completedAt = [DateTime]::UtcNow
    $result.completedAtUtc = $completedAt.ToString("o")
    $result.durationSeconds = [Math]::Round(($completedAt - $startedAt).TotalSeconds, 3)
    $artifactPaths = Write-ResultArtifacts -Result $result -RunDirectory $runDirectory
}

Write-Host "Host acceptance status: $($result.status)"
Write-Host "Result: $($artifactPaths.JsonPath)"
Write-Host "Summary: $($artifactPaths.SummaryPath)"

switch ($result.status) {
    "Passed" { exit 0 }
    "Generated" { exit 0 }
    "Skipped" { exit 2 }
    default { exit 1 }
}

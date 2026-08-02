#requires -Version 7.0

[CmdletBinding()]
param(
    [switch] $KeepArtifacts
)

Set-StrictMode -Version 2.0
$ErrorActionPreference = "Stop"

function Assert-Equal {
    param(
        [Parameter(Mandatory = $true)]
        [object] $Expected,

        [Parameter(Mandatory = $true)]
        [object] $Actual,

        [Parameter(Mandatory = $true)]
        [string] $Message
    )

    if ($Expected -ne $Actual) {
        throw "$Message Expected='$Expected', Actual='$Actual'."
    }
}

function Invoke-RunnerProcess {
    param(
        [Parameter(Mandatory = $true)]
        [string[]] $Arguments
    )

    & $script:PowerShellExecutable -NoProfile -ExecutionPolicy Bypass `
        -File $script:RunnerPath @Arguments | ForEach-Object { Write-Host $_ }
    return $LASTEXITCODE
}

function Invoke-RunnerJob {
    param(
        [Parameter(Mandatory = $true)]
        [hashtable] $Parameters,

        [int] $WaitSeconds = 30
    )

    $job = Start-Job -ScriptBlock {
        param($RunnerPath, $RunnerParameters)
        & $RunnerPath @RunnerParameters
    } -ArgumentList $script:RunnerPath, $Parameters

    try {
        $completedJob = Wait-Job -Job $job -Timeout $WaitSeconds
        if ($null -eq $completedJob) {
            Stop-Job -Job $job
            throw "Runner background job did not complete within $WaitSeconds seconds."
        }

        Receive-Job -Job $job | ForEach-Object { Write-Host $_ }
    }
    finally {
        if ($job.State -eq "Running") {
            Stop-Job -Job $job
        }
        Remove-Job -Job $job
    }
}

function Get-SingleResult {
    param(
        [Parameter(Mandatory = $true)]
        [string] $Directory
    )

    $results = @(Get-ChildItem -LiteralPath $Directory -Filter "result.json" -File -Recurse)
    Assert-Equal -Expected 1 -Actual $results.Count -Message "Expected one result.json."
    return Get-Content -LiteralPath $results[0].FullName -Raw | ConvertFrom-Json
}

function Test-AsciiPath {
    param(
        [Parameter(Mandatory = $true)]
        [string] $Path
    )

    foreach ($character in $Path.ToCharArray()) {
        if ([int] $character -gt 127) {
            return $false
        }
    }

    return $true
}

function New-AsciiTemporaryDirectory {
    $candidateRoots = New-Object System.Collections.Generic.List[string]
    $candidateRoots.Add((Join-Path $PSScriptRoot "artifacts"))

    if (-not [string]::IsNullOrWhiteSpace($env:PUBLIC)) {
        $candidateRoots.Add((Join-Path $env:PUBLIC "Documents\FsFoxCad-HostAcceptance"))
    }

    if (-not [string]::IsNullOrWhiteSpace($env:SystemDrive)) {
        $candidateRoots.Add((Join-Path $env:SystemDrive "Temp\FsFoxCad-HostAcceptance"))
    }

    $candidateRoots.Add((Join-Path ([System.IO.Path]::GetTempPath()) "FsFoxCad-HostAcceptance"))

    foreach ($candidateRoot in $candidateRoots | Select-Object -Unique) {
        if (-not (Test-AsciiPath -Path $candidateRoot)) {
            continue
        }

        $candidateDirectory = Join-Path $candidateRoot `
            ("smoke-" + [Guid]::NewGuid().ToString("N"))
        try {
            New-Item -ItemType Directory -Path $candidateDirectory -ErrorAction Stop | Out-Null
            return $candidateDirectory
        }
        catch {
            continue
        }
    }

    throw "Host acceptance runner smoke checks require a writable ASCII temporary directory. " +
        "Set up an ASCII checkout path or a writable public temporary location."
}

$script:RunnerPath = Join-Path $PSScriptRoot "Invoke-CadHostAcceptance.ps1"
$script:PowerShellExecutable = (Get-Process -Id $PID).Path
$scenarioRoot = Join-Path $PSScriptRoot "scenarios"
$tempRoot = New-AsciiTemporaryDirectory

try {
    $inputDirectory = Join-Path $tempRoot "inputs with spaces"
    New-Item -ItemType Directory -Path $inputDirectory | Out-Null
    $fakeAssembly = Join-Path $inputDirectory "Test Host.dll"
    New-Item -ItemType File -Path $fakeAssembly | Out-Null
    $sharedScenario = Join-Path $scenarioRoot "shared-smoke.json"
    $dynamicScenario = Join-Path $scenarioRoot "dynamic-block-visibility.json"
    $progressScenario = Join-Path $scenarioRoot "progress-meter.json"
    $zwcadEnvironmentScenario = Join-Path $scenarioRoot "zwcad-environment.json"
    $invalidScenario = Join-Path $tempRoot "invalid-scenario.json"
    @'
{
  "schemaVersion": 1,
  "name": "invalid-extra-property",
  "description": "Proves that the complete JSON Schema is enforced.",
  "products": ["AutoCAD"],
  "requiresDrawing": false,
  "commands": [
    {
      "name": "Synthetic command",
      "command": "Test_Synthetic",
      "expectedText": "Synthetic command passed."
    }
  ],
  "unexpectedProperty": true
}
'@ | Set-Content -LiteralPath $invalidScenario -Encoding UTF8

    foreach ($product in @("AutoCAD", "ZWCAD")) {
        $generateOutput = Join-Path $tempRoot ("generate-" + $product)
        $fakeExecutable = Join-Path $tempRoot ("not-installed-" + $product + ".exe")
        $exitCode = Invoke-RunnerProcess -Arguments @(
            "-Product", $product,
            "-Scenario", $sharedScenario,
            "-CadExecutable", $fakeExecutable,
            "-TestAssembly", $fakeAssembly,
            "-OutputDirectory", $generateOutput,
            "-GenerateOnly"
        )
        Assert-Equal -Expected 0 -Actual $exitCode -Message "$product generation exit code."

        $generatedResult = Get-SingleResult -Directory $generateOutput
        Assert-Equal -Expected "Generated" -Actual $generatedResult.status `
            -Message "$product generation status."
        Assert-Equal -Expected $product -Actual $generatedResult.host.product `
            -Message "$product result product."

        $generatedScript = Get-Content -LiteralPath $generatedResult.generatedScript -Raw
        if ($generatedScript -notmatch 'NETLOAD' -or
            $generatedScript -notmatch 'Test_JigDisposeSafety' -or
            $generatedScript -notmatch 'FSFOX_HOST_ACCEPTANCE_BEGIN' -or
            $generatedScript -notmatch 'FSFOX_HOST_ACCEPTANCE_END' -or
            $generatedScript -match 'SECURELOAD') {
            throw "$product generated script did not satisfy the expected safety contract."
        }
    }

    $invalidScenarioOutput = Join-Path $tempRoot "generate-invalid-scenario"
    $exitCode = Invoke-RunnerProcess -Arguments @(
        "-Product", "AutoCAD",
        "-Scenario", $invalidScenario,
        "-CadExecutable", (Join-Path $tempRoot "not-installed-AutoCAD.exe"),
        "-TestAssembly", $fakeAssembly,
        "-OutputDirectory", $invalidScenarioOutput,
        "-GenerateOnly"
    )
    Assert-Equal -Expected 1 -Actual $exitCode -Message "Invalid scenario exit code."
    $invalidScenarioResult = Get-SingleResult -Directory $invalidScenarioOutput
    Assert-Equal -Expected "InfrastructureError" -Actual $invalidScenarioResult.status `
        -Message "Invalid scenario status."
    if (($invalidScenarioResult.diagnostics -join "`n") -notmatch "scenario\.schema\.json") {
        throw "Invalid scenario diagnostics did not identify JSON Schema validation."
    }

    $notRunOutput = Join-Path $tempRoot "run-not-started"
    $exitCode = Invoke-RunnerProcess -Arguments @(
        "-Product", "AutoCAD",
        "-Scenario", $sharedScenario,
        "-CadExecutable", (Join-Path $tempRoot "not-installed-AutoCAD.exe"),
        "-TestAssembly", $fakeAssembly,
        "-OutputDirectory", $notRunOutput
    )
    Assert-Equal -Expected 1 -Actual $exitCode -Message "Not-run host exit code."
    $notRunResult = Get-SingleResult -Directory $notRunOutput
    Assert-Equal -Expected "InfrastructureError" -Actual $notRunResult.status `
        -Message "Not-run host status."
    foreach ($expectation in $notRunResult.expectations) {
        Assert-Equal -Expected "Not run" -Actual $expectation.status `
            -Message "Unexecuted expectation status."
    }

    $missingDrawingOutput = Join-Path $tempRoot "generate-missing-drawing"
    $exitCode = Invoke-RunnerProcess -Arguments @(
        "-Product", "AutoCAD",
        "-Scenario", $dynamicScenario,
        "-CadExecutable", (Join-Path $tempRoot "not-installed-AutoCAD.exe"),
        "-TestAssembly", $fakeAssembly,
        "-OutputDirectory", $missingDrawingOutput,
        "-GenerateOnly"
    )
    Assert-Equal -Expected 1 -Actual $exitCode -Message "Missing drawing exit code."
    Assert-Equal -Expected "InfrastructureError" `
        -Actual (Get-SingleResult -Directory $missingDrawingOutput).status `
        -Message "Missing drawing status."

    $passedLog = Join-Path $tempRoot "passed.log"
    @(
        "Jig dispose safety passed.",
        "XData removal isolation passed.",
        "Block attribute write scope passed.",
        "Ordinary block visibility query passed.",
        "EntGet passed for LINE (1).",
        "EntMod/EntUpd passed for 2."
    ) | Set-Content -LiteralPath $passedLog -Encoding UTF8
    $passedOutput = Join-Path $tempRoot "analyze-passed"
    $exitCode = Invoke-RunnerProcess -Arguments @(
        "-Product", "AutoCAD",
        "-Scenario", $sharedScenario,
        "-LogFile", $passedLog,
        "-OutputDirectory", $passedOutput
    )
    Assert-Equal -Expected 0 -Actual $exitCode -Message "Passed log exit code."
    Assert-Equal -Expected "Passed" -Actual (Get-SingleResult -Directory $passedOutput).status `
        -Message "Passed log status."

    $failedLog = Join-Path $tempRoot "failed.log"
    "Jig dispose safety passed." | Set-Content -LiteralPath $failedLog -Encoding UTF8
    $failedOutput = Join-Path $tempRoot "analyze-failed"
    $exitCode = Invoke-RunnerProcess -Arguments @(
        "-Product", "ZWCAD",
        "-Scenario", $sharedScenario,
        "-LogFile", $failedLog,
        "-OutputDirectory", $failedOutput
    )
    Assert-Equal -Expected 1 -Actual $exitCode -Message "Failed log exit code."
    Assert-Equal -Expected "Failed" -Actual (Get-SingleResult -Directory $failedOutput).status `
        -Message "Failed log status."

    $skipLog = Join-Path $tempRoot "skip.log"
    "[SKIP] Block visibility scan requires at least one dynamic block reference." |
        Set-Content -LiteralPath $skipLog -Encoding UTF8
    $skipOutput = Join-Path $tempRoot "analyze-skipped"
    $exitCode = Invoke-RunnerProcess -Arguments @(
        "-Product", "ZWCAD",
        "-Scenario", $dynamicScenario,
        "-LogFile", $skipLog,
        "-OutputDirectory", $skipOutput
    )
    Assert-Equal -Expected 2 -Actual $exitCode -Message "Skipped log exit code."
    Assert-Equal -Expected "Skipped" -Actual (Get-SingleResult -Directory $skipOutput).status `
        -Message "Skipped log status."

    $partialSkipLog = Join-Path $tempRoot "partial-skip.log"
    @(
        "Progress meter completed and the status bar was restored.",
        "[SKIP] One command was not applicable."
    ) | Set-Content -LiteralPath $partialSkipLog -Encoding UTF8
    $partialSkipOutput = Join-Path $tempRoot "analyze-partial-skip"
    $exitCode = Invoke-RunnerProcess -Arguments @(
        "-Product", "AutoCAD",
        "-Scenario", $progressScenario,
        "-LogFile", $partialSkipLog,
        "-OutputDirectory", $partialSkipOutput
    )
    Assert-Equal -Expected 1 -Actual $exitCode -Message "Partial skip exit code."
    Assert-Equal -Expected "Failed" -Actual (Get-SingleResult -Directory $partialSkipOutput).status `
        -Message "A skip line must not hide multiple missing results."

    $ambiguousSkipLog = Join-Path $tempRoot "ambiguous-skip.log"
    @(
        "Jig dispose safety passed.",
        "[SKIP] Unstructured output.",
        "[SKIP] Unstructured output.",
        "[SKIP] Unstructured output.",
        "[SKIP] Unstructured output.",
        "[SKIP] Unstructured output."
    ) | Set-Content -LiteralPath $ambiguousSkipLog -Encoding UTF8
    $ambiguousSkipOutput = Join-Path $tempRoot "analyze-ambiguous-skip"
    $exitCode = Invoke-RunnerProcess -Arguments @(
        "-Product", "ZWCAD",
        "-Scenario", $sharedScenario,
        "-LogFile", $ambiguousSkipLog,
        "-OutputDirectory", $ambiguousSkipOutput
    )
    Assert-Equal -Expected 1 -Actual $exitCode -Message "Ambiguous skip exit code."
    Assert-Equal -Expected "Failed" -Actual (Get-SingleResult -Directory $ambiguousSkipOutput).status `
        -Message "Unstructured skip output must not hide multiple expectations."

    $progressPassedLog = Join-Path $tempRoot "progress-passed.log"
    @(
        "Progress meter completed and the status bar was restored.",
        "Progress meter exception path restored the status bar.",
        "Progress meter completed and the status bar was restored."
    ) | Set-Content -LiteralPath $progressPassedLog -Encoding UTF8
    $progressPassedOutput = Join-Path $tempRoot "analyze-progress-passed"
    $exitCode = Invoke-RunnerProcess -Arguments @(
        "-Product", "AutoCAD",
        "-Scenario", $progressScenario,
        "-LogFile", $progressPassedLog,
        "-OutputDirectory", $progressPassedOutput
    )
    Assert-Equal -Expected 0 -Actual $exitCode -Message "Repeated expectation pass exit code."
    $progressPassedResult = Get-SingleResult -Directory $progressPassedOutput
    Assert-Equal -Expected "Passed" -Actual $progressPassedResult.status `
        -Message "Repeated expectation pass status."
    $repeatedExpectation = @($progressPassedResult.expectations | Where-Object {
            $_.text -eq "Progress meter completed and the status bar was restored."
        })
    Assert-Equal -Expected 1 -Actual $repeatedExpectation.Count `
        -Message "Repeated expectation result count."
    Assert-Equal -Expected 2 -Actual $repeatedExpectation[0].required `
        -Message "Repeated expectation required count."
    Assert-Equal -Expected 2 -Actual $repeatedExpectation[0].observed `
        -Message "Repeated expectation observed count."

    $progressFailedLog = Join-Path $tempRoot "progress-failed.log"
    @(
        "Progress meter completed and the status bar was restored.",
        "Progress meter exception path restored the status bar."
    ) | Set-Content -LiteralPath $progressFailedLog -Encoding UTF8
    $progressFailedOutput = Join-Path $tempRoot "analyze-progress-failed"
    $exitCode = Invoke-RunnerProcess -Arguments @(
        "-Product", "ZWCAD",
        "-Scenario", $progressScenario,
        "-LogFile", $progressFailedLog,
        "-OutputDirectory", $progressFailedOutput
    )
    Assert-Equal -Expected 1 -Actual $exitCode -Message "Repeated expectation failure exit code."
    $progressFailedResult = Get-SingleResult -Directory $progressFailedOutput
    Assert-Equal -Expected "Failed" -Actual $progressFailedResult.status `
        -Message "Repeated expectation failure status."
    $missingRepeatedExpectation = @($progressFailedResult.expectations | Where-Object {
            $_.text -eq "Progress meter completed and the status bar was restored."
        })
    Assert-Equal -Expected 1 -Actual $missingRepeatedExpectation[0].observed `
        -Message "Repeated expectation missing count."

    $productMismatchOutput = Join-Path $tempRoot "analyze-product-mismatch"
    $exitCode = Invoke-RunnerProcess -Arguments @(
        "-Product", "AutoCAD",
        "-Scenario", $zwcadEnvironmentScenario,
        "-LogFile", $passedLog,
        "-OutputDirectory", $productMismatchOutput
    )
    Assert-Equal -Expected 1 -Actual $exitCode -Message "Product restriction exit code."
    Assert-Equal -Expected "InfrastructureError" `
        -Actual (Get-SingleResult -Directory $productMismatchOutput).status `
        -Message "Product restriction status."

    $exceptionLog = Join-Path $tempRoot "exception.log"
    @(
        "Jig dispose safety passed.",
        "XData removal isolation passed.",
        "Block attribute write scope passed.",
        "Ordinary block visibility query passed.",
        "EntGet passed for LINE (1).",
        "EntMod/EntUpd passed for 2.",
        "System.EntryPointNotFoundException"
    ) | Set-Content -LiteralPath $exceptionLog -Encoding UTF8
    $exceptionOutput = Join-Path $tempRoot "analyze-exception"
    $exitCode = Invoke-RunnerProcess -Arguments @(
        "-Product", "AutoCAD",
        "-Scenario", $sharedScenario,
        "-LogFile", $exceptionLog,
        "-OutputDirectory", $exceptionOutput
    )
    Assert-Equal -Expected 1 -Actual $exitCode -Message "Exception log exit code."
    $exceptionResult = Get-SingleResult -Directory $exceptionOutput
    Assert-Equal -Expected "Failed" -Actual $exceptionResult.status `
        -Message "Exception log status."
    Assert-Equal -Expected 1 -Actual @($exceptionResult.matchedFailurePatterns).Count `
        -Message "Exception failure-pattern count."

    $explicitFailureLog = Join-Path $tempRoot "explicit-failure.log"
    @(
        "Jig dispose safety passed.",
        "XData removal isolation passed.",
        "Block attribute write scope passed.",
        "Ordinary block visibility query passed.",
        "EntGet passed for LINE (1).",
        "EntMod/EntUpd passed for 2.",
        "[FAIL] Synthetic host failure."
    ) | Set-Content -LiteralPath $explicitFailureLog -Encoding UTF8
    $explicitFailureOutput = Join-Path $tempRoot "analyze-explicit-failure"
    $exitCode = Invoke-RunnerProcess -Arguments @(
        "-Product", "ZWCAD",
        "-Scenario", $sharedScenario,
        "-LogFile", $explicitFailureLog,
        "-OutputDirectory", $explicitFailureOutput
    )
    Assert-Equal -Expected 1 -Actual $exitCode -Message "Explicit failure exit code."
    Assert-Equal -Expected "Failed" -Actual (Get-SingleResult -Directory $explicitFailureOutput).status `
        -Message "Explicit failure status."

    $scopedRunId = "offline-scope-test"
    $scopedLog = Join-Path $tempRoot "scoped.log"
    @(
        "[FAIL] Stale failure before this run.",
        "FSFOX_HOST_ACCEPTANCE_BEGIN $scopedRunId",
        "Jig dispose safety passed.",
        "XData removal isolation passed.",
        "Block attribute write scope passed.",
        "Ordinary block visibility query passed.",
        "EntGet passed for LINE (1).",
        "EntMod/EntUpd passed for 2.",
        "FSFOX_HOST_ACCEPTANCE_END $scopedRunId",
        "[FAIL] Stale failure after this run."
    ) | Set-Content -LiteralPath $scopedLog -Encoding UTF8

    $wholeLogOutput = Join-Path $tempRoot "analyze-whole-log"
    $exitCode = Invoke-RunnerProcess -Arguments @(
        "-Product", "AutoCAD",
        "-Scenario", $sharedScenario,
        "-LogFile", $scopedLog,
        "-OutputDirectory", $wholeLogOutput
    )
    Assert-Equal -Expected 1 -Actual $exitCode -Message "Whole-log stale failure exit code."
    Assert-Equal -Expected "Failed" -Actual (Get-SingleResult -Directory $wholeLogOutput).status `
        -Message "Whole-log analysis must retain configured failure patterns."

    $scopedLogOutput = Join-Path $tempRoot "analyze-scoped-log"
    $exitCode = Invoke-RunnerProcess -Arguments @(
        "-Product", "AutoCAD",
        "-Scenario", $sharedScenario,
        "-LogFile", $scopedLog,
        "-LogRunId", $scopedRunId,
        "-OutputDirectory", $scopedLogOutput
    )
    Assert-Equal -Expected 0 -Actual $exitCode -Message "Scoped log exit code."
    $scopedResult = Get-SingleResult -Directory $scopedLogOutput
    Assert-Equal -Expected "Passed" -Actual $scopedResult.status `
        -Message "Scoped log status."
    Assert-Equal -Expected "RunSegment" -Actual $scopedResult.logScope.mode `
        -Message "Scoped log mode."

    $timeoutHostScript = Join-Path $inputDirectory "Fake Slow Host.ps1"
    @'
param(
    [string] $BatchSwitch,
    [string] $GeneratedScript
)

Start-Sleep -Seconds 60
'@ | Set-Content -LiteralPath $timeoutHostScript -Encoding ASCII

    $timeoutOutput = Join-Path $tempRoot "run-timeout"
    Invoke-RunnerJob -Parameters @{
        Product             = "AutoCAD"
        Scenario            = $sharedScenario
        CadExecutable       = $script:PowerShellExecutable
        TestAssembly        = $fakeAssembly
        AdditionalArguments = @(
            "-NoProfile",
            "-ExecutionPolicy",
            "Bypass",
            "-File",
            $timeoutHostScript
        )
        OutputDirectory     = $timeoutOutput
        TimeoutSeconds      = 10
        TerminateOnTimeout  = $true
    } -WaitSeconds 30
    $timeoutResult = Get-SingleResult -Directory $timeoutOutput
    Assert-Equal -Expected "TimedOut" -Actual $timeoutResult.status `
        -Message "Timeout status."
    Assert-Equal -Expected $true -Actual $timeoutResult.process.exited `
        -Message "Timed-out fake host termination status."

    Write-Host "Host acceptance runner smoke checks passed."
}
finally {
    if ($KeepArtifacts) {
        Write-Host "Smoke artifacts retained at: $tempRoot"
    }
    elseif (Test-Path -LiteralPath $tempRoot) {
        Remove-Item -LiteralPath $tempRoot -Recurse -Force
    }
}

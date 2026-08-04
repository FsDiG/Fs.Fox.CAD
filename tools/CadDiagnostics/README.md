# Fs.Fox.CAD Diagnostics

> Status: current component guide. AutoCAD 2019/2025 SDK builds are enabled;
> runtime-host checks remain `Not run` until their results are recorded here or
> in the linked AutoCAD migration Issue.

`Fs.Fox.CAD.Diagnostics` is a developer inspection and diagnostic plug-in
derived from Autodesk MgdDbg. It is intentionally independent of
`Fs.Fox.AutoCad.dll`: each host project compiles the migrated source directly
against one AutoCAD SDK generation and produces its own DLL.

- Parent roadmap: [Issue #124](https://github.com/FsDiG/Fs.Fox.CAD/issues/124)
- AutoCAD migration: [Issue #125](https://github.com/FsDiG/Fs.Fox.CAD/issues/125)
- Source provenance: [UPSTREAM.md](UPSTREAM.md)
- Autodesk notice: [THIRD-PARTY-NOTICES.md](THIRD-PARTY-NOTICES.md)

## Structure and dependency boundary

```text
tools/CadDiagnostics/
  CADDiagnosticsShared/                  migrated models, commands, tests,
                                         collectors, WinForms and resources
  Platforms/AutoCad/                     AutoCAD bindings, compatibility
                                         adapters and host capability checks
  Fs.Fox.CAD.Diagnostics.AutoCad2019/    net48 / AutoCAD.NET 23.0.0
  Fs.Fox.CAD.Diagnostics.AutoCad2025/    net8.0-windows7.0 / AutoCAD.NET 25.0.1
```

Both host projects import the two shared projects as source. They have no
`ProjectReference`, package reference or assembly reference to an Fs.Fox.CAD
library. `PolySharp` is compile-time-only; Autodesk package runtime assets are
excluded so the build does not copy vendor SDK DLLs.

The directory split provides a place for later ZWCAD and GstarCAD adapters. It
does not claim that the current Autodesk-bound migrated source already compiles
for those products; those targets remain work under Issue #124.

## Build targets

| SDK baseline | Target framework | Constants | Output assembly | Runtime validation |
| --- | --- | --- | --- | --- |
| AutoCAD 2019 / `AutoCAD.NET` 23.0.0 | .NET Framework 4.8 | `ACAD;AC_2019;AC_NET48` | `Build/AC_2019_<Configuration>/Fs.Fox.CAD.Diagnostics.AutoCad2019.dll` | `Not run` |
| AutoCAD 2025 / `AutoCAD.NET` 25.0.1 | .NET 8 | `ACAD;AC_2025` | `Build/AC_2025_<Configuration>/Fs.Fox.CAD.Diagnostics.AutoCad2025.dll` | `Not run` |

Each output includes the same-named XML documentation file and follows the main
AutoCAD projects' no-PDB build policy. The SDK year identifies the compile-time
API baseline, not the actual AutoCAD product in which the DLL is loaded.

From a Visual Studio Developer PowerShell:

```powershell
# AutoCAD 2019 / .NET Framework
msbuild .\tools\CadDiagnostics\Fs.Fox.CAD.Diagnostics.AutoCad2019\Fs.Fox.CAD.Diagnostics.AutoCad2019.csproj `
  '/t:Restore;Build' /p:Configuration=Release '/p:Platform=x64'

# AutoCAD 2025 / .NET 8
dotnet build .\tools\CadDiagnostics\Fs.Fox.CAD.Diagnostics.AutoCad2025\Fs.Fox.CAD.Diagnostics.AutoCad2025.csproj `
  --configuration Release -p:Platform=x64
```

The projects are also included under `tools/CadDiagnostics` in `IFoxCAD.sln`
and in the normal Debug/Release CI build. They are not included in the NuGet
release workflow.

## Loading and stable commands

1. Choose the DLL for the intended AutoCAD API generation.
2. Open a blank or disposable drawing.
3. Run `NETLOAD` and select the versioned diagnostics DLL from `Build`.
4. Run `MgdDbgAbout` first to display the tool version, compile-time SDK,
   target framework, actual host product/version and loaded assembly path.
5. Use the `MgdDbg` context menu or the original `MgdDbg...` commands.

The migration preserves the original commands, including `MgdDbgSnoopEnts`,
`MgdDbgSnoopNEnts`, `MgdDbgSnoopByHandle`, `MgdDbgSnoopDb`, `MgdDbgSnoopEd`,
`MgdDbgEvents`, `MgdDbgTests` and the original diagnostic test commands. No new
aliases were introduced. `MgdDbgAbout` is the only added command.

Do not load an archived Autodesk MgdDbg DLL and an Fs.Fox.CAD.Diagnostics DLL
in the same AutoCAD process: both register the original command names. The tool
does not install itself, create a Bundle, modify CAD profiles, configure Trusted
Paths, edit the registry or add itself to a startup suite.

Initialization registers collector extensions, the context menu, test groups,
document events and class-browser filters. Termination removes those items in
reverse order and tolerates repeated or partially initialized cleanup.

## DWG statistics reports

`DwgStats` and `DwgStatsBatch` continue to write XML to the location selected by
the user. After a successful XML write, the embedded legacy report browser is
extracted beside it into `FsFoxCadDiagnostics.ReportBrowser/`, and the command
line prints both paths. Extraction overwrites only the fixed files owned by the
diagnostics assembly and never removes unrelated files.

All icons, WinForms resources, XSL, CSS, JavaScript and HTML report templates
are embedded in the diagnostic DLL. No resource directory is produced during a
build. The two legacy ImageList `.resx` entries still use the WinForms binary
resource format and can emit `MSB3825` when compiling the .NET 8 target. Their
actual AutoCAD 2025/2026 behavior is `Not run`; the warning is deliberately not
silenced or described as runtime success.

## Runtime validation boundary

Build and migration checks validate source coverage, assembly references,
embedded resources, output names and the limited public type surface. They do
not validate AutoCAD UI, transactions, native integration or object mutation.

| Host check | Intended host | Status |
| --- | --- | --- |
| Load AutoCAD 2019 binary and run `MgdDbgAbout` | AutoCAD 2020 | `Not run` |
| Load AutoCAD 2025 binary and run `MgdDbgAbout` | AutoCAD 2026 | `Not run` |
| Loading message and `MgdDbg` context menu | both | `Not run` |
| `MgdDbgSnoopEnts` on a simple entity | both | `Not run` |
| Open `MgdDbgEvents`, enable then disable one safe event | both | `Not run` |
| Open `MgdDbgTests` without executing write operations | both | `Not run` |
| Remaining reactors, write tests and batch reports | both | `Not run` |

Use only a blank or disposable drawing for initial host checks. Do not infer a
pass for any row from successful compilation, and do not automate host profile
or trust-setting changes as part of this component.

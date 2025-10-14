# CHANGELOG

This document records all significant changes since commit `ea73facda61fd47d87100f05067b35fa4995850f`.

## Overview

**Statistics:**
- Total commits: 42 commits
- Files changed: 853 files
- Lines added: 100,042 lines
- Lines deleted: 315 lines
- Time span: Approximately 10 months (from initial commit to latest cleanup)

## Major Changes by Category

### 1. Namespace Refactoring

#### 1.1 Rename Namespace to Fs.Fox (bf189c7)
**Date**: 10 months ago  
**Author**: ZhangChengbo

- Changed all namespaces from `IFoxCAD` to `Fs.Fox`
- Purpose:
  - Enable Fs team to use in production environment
  - Avoid DLL version conflicts
  - Better maintain and develop the library
- Scope: Affects 100+ files
- File types modified:
  - All `.cs` source files
  - Project configuration files
  - Documentation files

**Modified files include:**
- Algorithms: `QuadTree` series
- Extension methods: All files under `ExtensionMethod` directory
- Entities: `Entity/ArcEx.cs`, `Entity/CircleEx.cs`, `Entity/CurveEx.cs`, etc.
- Geometry: Files under `Geomerty` directory
- Initialization: Files under `Initialize` directory
- PE: Files under `PE` directory
- Selection filters: Files under `SelectionFilter` directory

#### 1.2 Project File Renaming (3850558)
**Date**: 10 months ago

- Renamed project file from `IFoxCAD.AutoCad.csproj` to `Fs.Fox.AutoCad.csproj`
- Updated solution file references
- Added modifications to WindowsAPI.cs

### 2. Project Structure Adjustments

#### 2.1 Branch Documentation (6bab122, 70104a4)
**Date**: About 9-10 months ago

- Created `Fs分支说明.md` and updated `README.md`
- Explained relationship between this branch and official IFoxCAD
- Added team usage instructions and online mind map links
- Documented branch maintenance strategy

#### 2.2 Structure Adjustments (8c1984d, 71ebd89, 6f2b379, f288f4a)
**Date**: About 9 months ago

- Project structure optimization
- Readme.md adjustments
- Code merge from v0.9 branch

### 3. New Features and Extensions

#### 3.1 BlockView.NET Project (d81155f, 28f70bf, b47a8cf)
**Date**: 3 months ago  
**Source**: https://github.com/15831944/CADDev/tree/master

Added BlockView.NET project containing:
- `BlockView.NET.csproj` - Project file
- `BlockViewDialog.cs` and `BlockViewDialog.Designer.cs` - Dialog interface (473 lines)
- `Commands.cs` - Command definitions (54 lines)
- `GsPreviewCtrl.cs` - Graphics preview control (1298 lines)
- `EnumToolStripMenuItem.cs` - Enum menu items (150 lines)
- `Helpers.cs` - Helper utilities (91 lines)
- `Properties/AssemblyInfo.cs` - Assembly information

**Subsequent updates:**
- Updated project to .NET Framework 4.8 and VS 2017 (28f70bf)
- Cleaned up unused using directives (b47a8cf)

#### 3.2 Geometry Extension Methods Library (e85b090)
**Date**: 3 months ago

Copied and added numerous geometry extension methods (approximately 3,747 new lines):
- `CircularArc2dExtensions.cs` (172 lines)
- `CircularArc3dExtensions.cs` (102 lines)
- `EditorExtensions.cs` (131 lines)
- `EllipseExtensions.cs` (71 lines)
- `GeomExt.cs` (143 lines)
- `Point2dCollectionExtensions.cs` (118 lines)
- `Point2dExtensions.cs` (113 lines)
- `Point3dCollectionExtensions.cs` (148 lines)
- `Point3dExtensions.cs` (228 lines)
- `Polyline2dExtensions.cs` (235 lines)
- `Polyline3dExtensions.cs` (39 lines)
- `PolylineExtensions.cs` (177 lines)
- `PolylineSegment.cs` (323 lines)
- `PolylineSegmentCollection.cs` (590 lines)
- `RegionExtensions.cs` (25 lines)
- `SplineExtensions.cs` (47 lines)
- `Triangle.cs` (234 lines)
- `Triangle2d.cs` (316 lines)
- `Triangle3d.cs` (340 lines)
- `Vector3dExtensions.cs` (20 lines)
- `ViewportExtensions.cs` (61 lines)

#### 3.3 ThMEP.Core Library Integration (de43cd4)
**Date**: 3 months ago  
**Source**: https://github.com/SBESIRC/ThMEP.Core/tree/main

Added extensive library collection from online sources, including multiple sub-projects:

**CADExtension Library:**
- Database services extensions: `DatabaseServices` series
- Editor input: `EditorInput` series
- Runtime support: `Runtime` series
- Geometry extensions: `Geometry` series
- Entity extensions: Extension methods for various CAD entities

**ThCADExtension Library:**
- `ThCurveExtension.cs` - Curve extensions
- `ThEditorExtension.cs` - Editor extensions
- `ThPolylineExtension.cs` - Polyline extensions
- `ThStringTools.cs` - String utilities (number to Chinese, etc.)
- `ThGeExtension.cs` - Geometry extensions
- `ThHatchTool.cs` - Hatch tools
- `ThLayerTool.cs` - Layer tools
- And more...

**ThMEPEngineCore:**
- Algorithm modules: Centerline services, collision detection, polygon partitioning, triangulation, etc.
- Beam information processing: BeamInfo related business logic
- CAD tools: Recognition engines, drawing tools, geometry tools
- Various extraction and recognition engines

**GeometryExtensions:**
- `Arc2d.cs`, `Arc3d.cs` - Arc extensions
- `Circle2d.cs`, `Circle3d.cs` - Circle extensions
- `Ellipse2d.cs`, `Ellipse3d.cs` - Ellipse extensions
- `Line2d.cs`, `Line3d.cs` - Line segment extensions
- `Polygon2d.cs`, `Polygon3d.cs` - Polygon extensions
- `Triangle.cs`, `Triangle2d.cs`, `Triangle3d.cs` - Triangle extensions
- And more...

**Statistics**: Added approximately 20,000+ lines of code

### 4. Code Refactoring and Optimization

#### 4.1 Bounding Box Refactoring (5148bd3)
**Date**: 3 months ago

- Renamed `BoundingInfo.cs` to `BoundingBox9.cs`
- Refactored bounding box structure and references
- Optimized bounding box implementation logic
- Modified files:
  - `CADShared/ExtensionMethod/Entity/BoundingBox9.cs`
  - `CADShared/ExtensionMethod/Entity/EntityBoundingInfo.cs`
  - `CADShared/ExtensionMethod/Entity/EntityEx.cs`

#### 4.2 CurveEx Method Renaming (796bf97)
**Date**: 3 months ago

- Renamed method in `CurveEx` class to `GetCurveLength`
- Improved method name clarity and consistency

### 5. Build and Deployment (CI/CD)

#### 5.1 Debug Configuration Optimization (2c7f0fb)
**Date**: 3 months ago

- Added `x64_2019_Debug` configuration for FoxCAD
- Output to DiG Build path for easier debugging
- Avoids conflicts with original CI/CD workflow

#### 5.2 GitHub Actions Workflows (bac580b, c752201, ab07868, etc.)
**Date**: About 7-8 months ago

Added and improved CI/CD pipeline:
- Added GitHub workflow configurations
- New issue and pull request templates
- Improved build and deployment documentation
- Updated NuGet package restore process
- Added CI build parameters and simplified file copy logic
- Introduced deployment flag mechanism
- Updated workflow to use Build repository

**Related commits:**
- `bac580b` - Added GitHub workflows and templates
- `c752201` - Updated build and deployment configuration
- `ab07868` - Updated NuGet package restore process
- `994e5d8` - Updated CustomCopyOutputForNet48 target copy logic
- `74c24cf` - Added CI build parameters and simplified file copy logic
- `f5026e0` - Updated workflow to use Build repository
- `496822b` - Introduced deployment flag mechanism
- `109fc5b` - Added target repository checkout and NuGet restore steps
- `373dcb7` - Updated workflows markdown documentation
- Test commits: `faf4c5a`, `ebbd051`, `50c8f4a`, `641aaa4` - Test commits with [deploy] tag

#### 5.3 Build Scripts (89b995b)
**Date**: About 9 months ago

- Added copy logic to `Build\DiGLib\DiGArchBase\x64_2019_Release\Fs.Fox.AutoCad.dll`
- Created `copyToDigBuild.ps1` PowerShell script

### 6. Documentation and Resources

#### 6.1 Online Mind Map (1dad4c2)
**Date**: About 9 months ago

- Organized mind map based on public documentation drafts
- File in boardmix link: ".NET ARX_Fox"
- Link: https://boardmix.cn/app/share/CAE.CMvmgA4gASoQHBGpsUGmGR9LipooomyTSDAGQAE/U41nx2

#### 6.2 Project Configuration and Documentation (27a052d, 2bce0f4)
**Date**: About 7-8 months ago

- Updated project configuration
- Updated README.md
- Improved documentation

### 7. Testing and Fixes

#### 7.1 FsCAD Synchronization (079f24f)
**Date**: About 9 months ago

- Synchronized updates from FsCAD

#### 7.2 Test Command Optimization (beba816)
**Date**: About 10 months ago

- Removed invalid spaces in test commands
- Added comments to Test_Rec

### 8. Other Changes

#### 8.1 No Logic Changes (ee31c0a)
**Date**: About 9 months ago

- Code formatting and cleanup with no functional changes

#### 8.2 Version Merges (a094aba, 0, f288f4a)
**Date**: About 9 months ago

- Merged code from v0.9 branch
- Various minor adjustments and fixes

## Detailed Commit List

### Recent 3 Months (July 2024 - October 2024)

1. **b47a8cf** - Clean up unused using directives
2. **28f70bf** - Update project to .NET Framework 4.8 and VS 2017
3. **d81155f** - Add BlockView.NET.csproj from external project
4. **796bf97** - Rename CurveEx class method to GetCurveLength
5. **5148bd3** - Refactor bounding box structure and references
6. **2c7f0fb** - Add x64_2019_Debug configuration for FoxCAD
7. **e85b090** - Copy geometry extension methods
8. **de43cd4** - Integrate online library collection

### 6-9 Months Ago (January 2024 - July 2024)

9. **2bce0f4** - Update README.md
10. **641aaa4** - Test passed [deploy]
11. **50c8f4a** - [deploy]
12. **ebbd051** - Test [deploy]
13. **b0cc06b** - [deploy]
14. **373dcb7** - Update workflows markdown documentation
15. **faf4c5a** - Test [deploy]
16. **136e67e** - Add target repository checkout and NuGet restore steps
17. **109fc5b** - 00
18. **496822b** - Introduce deployment flag mechanism to GitHub Actions
19. **f5026e0** - Update workflow to use Build repository
20. **74c24cf** - Add CI build parameters and simplify file copy logic
21. **994e5d8** - Update CustomCopyOutputForNet48 target copy logic
22. **ab07868** - Update NuGet package restore process
23. **c752201** - Update build and deployment configuration
24. **bac580b** - Add GitHub workflows and issue/PR templates
25. **27a052d** - Update project configuration and documentation
26. **ee31c0a** - No logic changes
27. **89b995b** - Add copy to Build\DiGLib\DiGArchBase path
28. **1dad4c2** - Mind map link
29. **6bab122** - Fs.Fox branch description
30. **70104a4** - README.md
31. **079f24f** - Sync FsCAD updates
32. **a094aba** - 00
33. **8c1984d** - Adjust structure
34. **71ebd89** - Adjust Readme.md
35. **6f2b379** - 0
36. **f288f4a** - Merge branch 'v0.9' into Fs0.9
37. **beba816** - Remove invalid spaces in test commands, add comments

### About 10 Months Ago (Initialization Phase)

38. **89c9638** - Change to Fs.Fox namespace on FoxCAD basis (major change description)
39. **3850558** - Project file renaming
40. **bf189c7** - Change namespace to Fs.Fox (large-scale refactoring)

## Technical Debt and TODO

TODO items extracted from README.md:

- [ ] Evaluate third-party IndexRange package for cleanup
  - C# native IndexRange was introduced in Standard 2.1
  - Framework 4.8 only supports Standard 2.0

## Impact Assessment

### Advantages
1. **Independence**: Through namespace changes, achieved independent maintenance from IFoxCAD
2. **Feature Enhancement**: Added numerous extension methods and utility classes
3. **Automation**: Complete CI/CD pipeline
4. **Documentation**: Fairly complete branch descriptions and online mind map

### Considerations
1. **Compatibility**: Namespace changes mean incompatibility with original IFoxCAD
2. **Maintenance Cost**: Integrated large amount of third-party code, requires ongoing maintenance
3. **Code Quality**: Some code from external sources needs further review and testing

## Contributors

Main contributor:
- **ZhangChengbo**: Author of all major commits

## References

1. Official IFoxCAD: https://gitee.com/inspirefunction/ifoxcad
2. BlockView.NET source: https://github.com/15831944/CADDev/tree/master
3. ThMEP.Core source: https://github.com/SBESIRC/ThMEP.Core/tree/main
4. Online mind map: https://boardmix.cn/app/share/CAE.CMvmgA4gASoQHBGpsUGmGR9LipooomyTSDAGQAE/U41nx2

---

**Document generation date**: 2025-10-14  
**Baseline commit**: ea73facda61fd47d87100f05067b35fa4995850f  
**Latest commit**: b47a8cf (Clean up unused using directives)

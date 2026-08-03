// ============================================================================
// IFoxCAD.Gcad GlobalUsings.cs - GStarCAD（浩辰CAD）平台
// GStarCAD 2022/2023 (net48): GrxCAD.* 命名空间
// GStarCAD 2026+  (net8.0):   Gssoft.Gscad.* 命名空间
// ============================================================================

#if GC_2026
// GStarCAD 2026 命名空间 (Gssoft.Gscad.*)
global using Gssoft.Gscad.ApplicationServices;
global using Gssoft.Gscad.DatabaseServices;
global using Gssoft.Gscad.EditorInput;
global using Gssoft.Gscad.Geometry;
global using Gssoft.Gscad.GraphicsInterface;
global using Gssoft.Gscad.Runtime;
global using Gssoft.Gscad.Windows;
global using Gssoft.Gscad.Colors;
global using Gssoft.Gscad.DatabaseServices.Filters;
global using Gssoft.Gscad.GraphicsSystem;

// ============================================================================
// Cad 前缀标准别名 - GStarCAD 2026
// ============================================================================
global using CadApp = Gssoft.Gscad.ApplicationServices.Application;
global using CadCoreApp = Gssoft.Gscad.ApplicationServices.Core.Application;
global using CadDbServices = Gssoft.Gscad.DatabaseServices;
global using CadGI = Gssoft.Gscad.GraphicsInterface;
global using CadGS = Gssoft.Gscad.GraphicsSystem;
global using CadRuntime = Gssoft.Gscad.Runtime;
global using CadWindow = Gssoft.Windows;
global using CadException = Gssoft.Gscad.Runtime.Exception;
global using CadErrorStatus = Gssoft.Gscad.Runtime.ErrorStatus;
global using CadDwgFiler = Gssoft.Gscad.DatabaseServices.DwgFiler;
global using CadDxfFiler = Gssoft.Gscad.DatabaseServices.DxfFiler;
global using CadOpenFileDialog = Gssoft.Gscad.Windows.OpenFileDialog;
global using Marshaler = Gssoft.Gscad.ApplicationServices.Marshaler;
global using Utils = Gssoft.Gscad.Internal.Utils;

// ============================================================================
// 解决命名冲突 - GStarCAD 2026
// ============================================================================
global using LineWeight = Gssoft.Gscad.DatabaseServices.LineWeight;
global using Viewport = Gssoft.Gscad.DatabaseServices.Viewport;
global using Color = Gssoft.Gscad.Colors.Color;
global using Polyline = Gssoft.Gscad.DatabaseServices.Polyline;
global using Group = Gssoft.Gscad.DatabaseServices.Group;
global using CursorType = Gssoft.Gscad.EditorInput.CursorType;
global using ColorDialog = Gssoft.Gscad.Windows.ColorDialog;
global using StatusBar = Gssoft.Gscad.Windows.StatusBar;
global using SystemVariableChangedEventArgs = Gssoft.Gscad.ApplicationServices.SystemVariableChangedEventArgs;
global using Region = Gssoft.Gscad.DatabaseServices.Region;
global using Exception = System.Exception;
global using DrawingColor = System.Drawing.Color;
global using Registry = Microsoft.Win32.Registry;
global using RegistryKey = Microsoft.Win32.RegistryKey;

#else
// GStarCAD 2022/2023 命名空间 (GrxCAD.*)
global using GrxCAD.ApplicationServices;
global using GrxCAD.DatabaseServices;
global using GrxCAD.EditorInput;
global using GrxCAD.Geometry;
global using GrxCAD.GraphicsInterface;
global using GrxCAD.Runtime;
global using GrxCAD.Windows;
global using GrxCAD.Colors;
global using GrxCAD.DatabaseServices.Filters;
global using GrxCAD.GraphicsSystem;

// ============================================================================
// Cad 前缀标准别名 - GStarCAD 2022/2023
// GrxCAD 没有 Core.Application，DocumentManager/GetSystemVariable 直接在 Application 上
// ============================================================================
global using CadApp = GrxCAD.ApplicationServices.Application;
global using CadCoreApp = GrxCAD.ApplicationServices.Application;
global using CadDbServices = GrxCAD.DatabaseServices;
global using CadGI = GrxCAD.GraphicsInterface;
global using CadGS = GrxCAD.GraphicsSystem;
global using CadRuntime = GrxCAD.Runtime;
global using CadWindow = GrxCAD.Windows;
global using CadException = GrxCAD.Runtime.Exception;
global using CadErrorStatus = GrxCAD.Runtime.ErrorStatus;
global using CadDwgFiler = GrxCAD.DatabaseServices.DwgFiler;
global using CadDxfFiler = GrxCAD.DatabaseServices.DxfFiler;
global using CadOpenFileDialog = GrxCAD.Windows.OpenFileDialog;
global using Marshaler = GrxCAD.ApplicationServices.Marshaler;
global using Utils = GrxCAD.Internal.Utils;

// ============================================================================
// 解决命名冲突 - GStarCAD 2022/2023
// ============================================================================
global using LineWeight = GrxCAD.DatabaseServices.LineWeight;
global using Viewport = GrxCAD.DatabaseServices.Viewport;
global using Color = GrxCAD.Colors.Color;
global using Polyline = GrxCAD.DatabaseServices.Polyline;
global using Group = GrxCAD.DatabaseServices.Group;
global using CursorType = GrxCAD.EditorInput.CursorType;
global using ColorDialog = GrxCAD.Windows.ColorDialog;
global using StatusBar = GrxCAD.Windows.StatusBar;
global using SystemVariableChangedEventArgs = GrxCAD.ApplicationServices.SystemVariableChangedEventArgs;
global using Region = GrxCAD.DatabaseServices.Region;
global using Exception = System.Exception;
global using DrawingColor = System.Drawing.Color;
global using Registry = Microsoft.Win32.Registry;
global using RegistryKey = Microsoft.Win32.RegistryKey;
#endif

// ============================================================================
// 系统命名空间
// ============================================================================
global using System;
global using System.Reflection;
global using System.Collections;
global using System.Collections.Generic;
global using System.IO;
global using System.Linq;
global using System.Threading;
global using System.Text;
global using System.Runtime.InteropServices;
global using System.ComponentModel;
global using Microsoft.Win32;
global using System.Linq.Expressions;
global using System.Collections.ObjectModel;
global using System.Text.RegularExpressions;
global using System.Runtime.CompilerServices;
global using System.Windows.Input;
global using System.Globalization;
global using System.Diagnostics;
global using System.Net;
global using System.Diagnostics.CodeAnalysis;

// IFoxCAD
global using Fs.Fox.Basal;

// ============================================================================
// IFoxCAD.AutoCad GlobalUsings.cs - AutoCAD 平台
// ============================================================================

// AutoCAD 命名空间
global using Autodesk.AutoCAD.ApplicationServices;
global using Autodesk.AutoCAD.DatabaseServices;
global using Autodesk.AutoCAD.EditorInput;
global using Autodesk.AutoCAD.Geometry;
global using Autodesk.AutoCAD.GraphicsInterface;
global using Autodesk.AutoCAD.Runtime;
global using Autodesk.AutoCAD.Windows;
global using Autodesk.AutoCAD.Colors;
global using Autodesk.AutoCAD.DatabaseServices.Filters;
global using Autodesk.AutoCAD.GraphicsSystem;

// ============================================================================
// Cad 前缀标准别名
// ============================================================================
global using CadApp = Autodesk.AutoCAD.ApplicationServices.Application;
global using CadCoreApp = Autodesk.AutoCAD.ApplicationServices.Core.Application;
global using CadDbServices = Autodesk.AutoCAD.DatabaseServices;
global using CadGI = Autodesk.AutoCAD.GraphicsInterface;
global using CadGS = Autodesk.AutoCAD.GraphicsSystem;
global using CadRuntime = Autodesk.AutoCAD.Runtime;
global using CadWindow = Autodesk.Windows;
global using CadException = Autodesk.AutoCAD.Runtime.Exception;
global using CadErrorStatus = Autodesk.AutoCAD.Runtime.ErrorStatus;
global using CadDwgFiler = Autodesk.AutoCAD.DatabaseServices.DwgFiler;
global using CadDxfFiler = Autodesk.AutoCAD.DatabaseServices.DxfFiler;
global using CadOpenFileDialog = Autodesk.AutoCAD.Windows.OpenFileDialog;

// ============================================================================
// 解决命名冲突
// ============================================================================
global using LineWeight = Autodesk.AutoCAD.DatabaseServices.LineWeight;
global using Viewport = Autodesk.AutoCAD.DatabaseServices.Viewport;
global using Color = Autodesk.AutoCAD.Colors.Color;
global using Polyline = Autodesk.AutoCAD.DatabaseServices.Polyline;
global using Group = Autodesk.AutoCAD.DatabaseServices.Group;
global using CursorType = Autodesk.AutoCAD.EditorInput.CursorType;
global using ColorDialog = Autodesk.AutoCAD.Windows.ColorDialog;
global using StatusBar = Autodesk.AutoCAD.Windows.StatusBar;
global using Utils = Autodesk.AutoCAD.Internal.Utils;
global using SystemVariableChangedEventArgs = Autodesk.AutoCAD.ApplicationServices.SystemVariableChangedEventArgs;
global using Region = Autodesk.AutoCAD.DatabaseServices.Region;
global using Exception = System.Exception;
global using DrawingColor = System.Drawing.Color;
global using Registry = Microsoft.Win32.Registry;
global using RegistryKey = Microsoft.Win32.RegistryKey;

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
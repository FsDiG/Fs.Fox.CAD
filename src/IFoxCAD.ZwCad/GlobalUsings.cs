﻿global using ZwSoft.ZwCAD.ApplicationServices;
global using ZwSoft.ZwCAD.DatabaseServices;
global using ZwSoft.ZwCAD.EditorInput;
global using ZwSoft.ZwCAD.Geometry;
global using ZwSoft.ZwCAD.GraphicsInterface;
global using ZwSoft.ZwCAD.Runtime;
global using ZwSoft.ZwCAD.Windows;
global using ZwSoft.ZwCAD.Colors;
global using ZwSoft.ZwCAD.DatabaseServices.Filters;
global using ZwSoft.ZwCAD.GraphicsSystem;
global using LineWeight = ZwSoft.ZwCAD.DatabaseServices.LineWeight;
global using Viewport = ZwSoft.ZwCAD.DatabaseServices.Viewport;
global using Color = ZwSoft.ZwCAD.Colors.Color;
global using Acap = ZwSoft.ZwCAD.ApplicationServices.Application;

#if z2025
global using Acaop = ZwSoft.ZwCAD.ApplicationServices.Core.Application;
#else
// ZWCAD 2022 及之前版本没有 Core 命名空间，直接使用 Application
global using Acaop = ZwSoft.ZwCAD.ApplicationServices.Application;
#endif

global using Polyline = ZwSoft.ZwCAD.DatabaseServices.Polyline;
global using Group = ZwSoft.ZwCAD.DatabaseServices.Group;
global using CursorType = ZwSoft.ZwCAD.EditorInput.CursorType;
global using ColorDialog = ZwSoft.ZwCAD.Windows.ColorDialog;
global using StatusBar = ZwSoft.ZwCAD.Windows.StatusBar;
global using Utils = ZwSoft.ZwCAD.Internal.Utils;
global using SystemVariableChangedEventArgs = ZwSoft.ZwCAD.ApplicationServices.SystemVariableChangedEventArgs;
global using AcException = ZwSoft.ZwCAD.Runtime.Exception;
global using Marshaler = ZwSoft.ZwCAD.Runtime.Marshaler;
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
global using Exception = System.Exception;
global using DrawingColor = System.Drawing.Color;
global using Registry = Microsoft.Win32.Registry;
global using RegistryKey = Microsoft.Win32.RegistryKey;
global using Region = ZwSoft.ZwCAD.DatabaseServices.Region;
global using Microsoft.Win32;
global using System.Linq.Expressions;
global using System.Collections.ObjectModel;
// 系统引用
global using System.Text.RegularExpressions;
global using System.Runtime.CompilerServices;
global using System.Windows.Input;
global using System.Globalization;
global using System.Diagnostics;

// global using System.Windows.Data;
global using System.Net;
global using System.Diagnostics.CodeAnalysis;
global using Fs.Fox.Basal;
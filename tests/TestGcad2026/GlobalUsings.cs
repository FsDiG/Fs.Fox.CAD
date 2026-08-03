/// 系统引用
global using System;
global using System.Collections;
global using System.Collections.Generic;
global using System.IO;
global using System.Linq;
global using System.Text;
global using System.Reflection;
global using System.Text.RegularExpressions;
global using Microsoft.Win32;
global using System.ComponentModel;
global using System.Runtime.InteropServices;
global using System.Collections.Specialized;
global using System.Threading;
global using System.Diagnostics;
global using Exception = System.Exception;
global using System.Runtime.CompilerServices;
global using Registry = Microsoft.Win32.Registry;
global using RegistryKey = Microsoft.Win32.RegistryKey;

// cad 引用 (GStarCAD 2026 - Gssoft.Gscad.*)
global using Gssoft.Gscad.ApplicationServices;
global using Gssoft.Gscad.EditorInput;
global using Gssoft.Gscad.Colors;
global using Gssoft.Gscad.DatabaseServices;
global using Gssoft.Gscad.Geometry;
global using Gssoft.Gscad.Runtime;
global using Gssoft.Gscad.DatabaseServices.Filters;
global using Gssoft.Gscad.GraphicsInterface;
global using Gssoft.Gscad.GraphicsSystem;
global using CadApp = Gssoft.Gscad.ApplicationServices.Application;
global using CadCoreApp = Gssoft.Gscad.ApplicationServices.Core.Application;
global using CadException = Gssoft.Gscad.Runtime.Exception;
global using CadErrorStatus = Gssoft.Gscad.Runtime.ErrorStatus;
global using CadGI = Gssoft.Gscad.GraphicsInterface;
global using CadDwgFiler = Gssoft.Gscad.DatabaseServices.DwgFiler;
global using CadDxfFiler = Gssoft.Gscad.DatabaseServices.DxfFiler;
// jig命名空间会引起Viewport/Polyline等等重义,最好逐个引入 using Gssoft.Gscad.GraphicsInterface
global using WorldDraw = Gssoft.Gscad.GraphicsInterface.WorldDraw;
global using Manager = Gssoft.Gscad.GraphicsSystem.Manager;
global using Group = Gssoft.Gscad.DatabaseServices.Group;
global using Viewport = Gssoft.Gscad.DatabaseServices.Viewport;
global using Polyline = Gssoft.Gscad.DatabaseServices.Polyline;
global using LineWeight = Gssoft.Gscad.DatabaseServices.LineWeight;


/// ifoxcad
global using Fs.Fox.Cad;
global using Fs.Fox.Basal;

global using Test;

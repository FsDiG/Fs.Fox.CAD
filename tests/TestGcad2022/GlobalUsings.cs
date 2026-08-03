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

// cad 引用 (GStarCAD 2022 - GrxCAD.*)
global using GrxCAD.ApplicationServices;
global using GrxCAD.EditorInput;
global using GrxCAD.Colors;
global using GrxCAD.DatabaseServices;
global using GrxCAD.Geometry;
global using GrxCAD.Runtime;
global using GrxCAD.DatabaseServices.Filters;
global using GrxCAD.GraphicsInterface;
global using GrxCAD.GraphicsSystem;
global using CadApp = GrxCAD.ApplicationServices.Application;
global using CadCoreApp = GrxCAD.ApplicationServices.Application;
global using CadException = GrxCAD.Runtime.Exception;
global using CadErrorStatus = GrxCAD.Runtime.ErrorStatus;
global using CadGI = GrxCAD.GraphicsInterface;
global using CadDwgFiler = GrxCAD.DatabaseServices.DwgFiler;
global using CadDxfFiler = GrxCAD.DatabaseServices.DxfFiler;
// jig命名空间会引起Viewport/Polyline等等重义,最好逐个引入 using GrxCAD.GraphicsInterface
global using WorldDraw = GrxCAD.GraphicsInterface.WorldDraw;
global using Manager = GrxCAD.GraphicsSystem.Manager;
global using Group = GrxCAD.DatabaseServices.Group;
global using Viewport = GrxCAD.DatabaseServices.Viewport;
global using Polyline = GrxCAD.DatabaseServices.Polyline;


/// ifoxcad
global using Fs.Fox.Cad;
global using Fs.Fox.Basal;

global using Test;

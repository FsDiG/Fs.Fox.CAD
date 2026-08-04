// AutoCAD-specific bindings for the shared diagnostic sources. Keep broad CAD
// namespaces local to each migrated file: several SDK namespaces expose types
// with the same short name (for example Viewport, Surface and Polyline).
global using System;
global using System.IO;
global using Fs.Fox.CAD.Diagnostics.AutoCad;
global using CadApplication = Autodesk.AutoCAD.ApplicationServices.Application;
global using CadException = Autodesk.AutoCAD.Runtime.Exception;
global using ContextMenu = Fs.Fox.CAD.Diagnostics.AutoCad.LegacyContextMenu;
global using MenuItem = Fs.Fox.CAD.Diagnostics.AutoCad.LegacyMenuItem;
global using LegacyContextMenu = Fs.Fox.CAD.Diagnostics.AutoCad.LegacyContextMenu;
global using LegacyMenuItem = Fs.Fox.CAD.Diagnostics.AutoCad.LegacyMenuItem;
global using LegacyMenuAdapter = Fs.Fox.CAD.Diagnostics.AutoCad.LegacyMenuAdapter;

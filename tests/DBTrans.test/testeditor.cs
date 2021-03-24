﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;
using IFoxCAD.Cad;
namespace test
{
    public class testeditor
    {
        [CommandMethod("tested")]
        public void tested()
        {
            var pts = new List<Point2d>
            {
                new Point2d(0,0),
                new Point2d(0,1),
                new Point2d(1,1),
                new Point2d(1,0)
            };
            var res = EditorEx.GetLines(pts, false);
            var res1 = EditorEx.GetLines(pts, true);
            var res2 = pts.Select(pt => new TypedValue((int)LispDataType.Point2d, pt)).ToList();

            Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;
            var pt = ed.GetPoint("qudiam", new Point3d(0, 0, 0));
            var d = ed.GetDouble("qudoule");
            var i = ed.GetInteger("quint");
            var s = ed.GetString("qustr");
            Env.Editor.WriteMessage("");
        }
    }
}

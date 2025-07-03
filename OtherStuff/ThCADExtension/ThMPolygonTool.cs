using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.ApplicationServices;
using System.Windows.Documents;
using System.Collections.Generic;
using System;
using Linq2Acad;
using AcHelper;
using System.Linq;

namespace ThCADExtension
{
    public static class ThMPolygonTool
    {
        public static void Initialize()
        {
            string ver = Application.GetSystemVariable("ACADVER").ToString().Substring(0, 2);
            SystemObjects.DynamicLinker.LoadModule("AcMPolygonObj" + ver + ".dbx", false, false);
        }
        public static MPolygon CreateMPolygon(Curve external, List<Curve> innerCurves)
        {
            MPolygon mPolygon = new MPolygon();
            if (external is Polyline polyline)
            {
                if(polyline.Area>0.0)
                {
                    mPolygon.AppendLoopFromBoundary(polyline, false, 0.0);
                }
            }
            else if(external is Polyline2d polyline2d)
            {
                mPolygon.AppendLoopFromBoundary(polyline2d, false, 0.0);
            }
            else if(external is Circle circle)
            {
                mPolygon.AppendLoopFromBoundary(circle, false, 0.0);
            }
            else
            {
                throw new NotSupportedException();
            }

            innerCurves.ForEach(o =>
            {
                if (o is Polyline innerPolyline)
                {
                    mPolygon.AppendLoopFromBoundary(innerPolyline, false, 0.0);
                }
                else if (o is Polyline2d innerPolyline2d)
                {
                    mPolygon.AppendLoopFromBoundary(innerPolyline2d, false, 0.0);
                }
                else if (o is Circle innerCircle)
                {
                    mPolygon.AppendLoopFromBoundary(innerCircle, false, 0.0);
                }
                else
                {
                    throw new NotSupportedException();
                }
            });

            mPolygon.SetLoopDirection(0, LoopDirection.Exterior);
            if (innerCurves.Count > 0)
            {
                for (int i = 1; i <= innerCurves.Count; i++)
                {
                    mPolygon.SetLoopDirection(i, LoopDirection.Interior);
                }
            }
            return mPolygon;
        }
    }
}

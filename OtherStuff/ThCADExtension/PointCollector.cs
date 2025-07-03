using System;
using Autodesk.AutoCAD.Geometry;
using System.Collections.Generic;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.GraphicsInterface;
using AcadApp = Autodesk.AutoCAD.ApplicationServices.Application;

namespace ThCADExtension
{
    public class PointMonitorEventHandlerOverride : IDisposable
    {
        private event PointMonitorEventHandler PointMonitor;

        public PointMonitorEventHandlerOverride(PointMonitorEventHandler handler)
        {
            PointMonitor = handler;
            AcadApp.DocumentManager.MdiActiveDocument.Editor.PointMonitor += PointMonitor;
        }

        public void Dispose()
        {
            AcadApp.DocumentManager.MdiActiveDocument.Editor.PointMonitor -= PointMonitor;
        }
    }

    public class PointCollector : IDisposable
    {
        public enum Shape
        {
            Window,
            Fence,
            Polygon,
            RegularPolygon,
            Circle,
        }

        private Shape mShape;

        private Autodesk.AutoCAD.DatabaseServices.Polyline mTempPline;
        private short mColorIndex = 7;
        private TransientDrawingMode mMode;
        private int mSegmentCount = 36;

        private Point3d m1stPoint;
        private double mDist;

        public List<string> Prompts { get; set; }
        public Point3dCollection CollectedPoints { get; private set; }

        public PointCollector(Shape shape, List<String> prompts)
        {
            mShape = shape;
            switch (mShape)
            {
                case Shape.Fence:
                    mMode = TransientDrawingMode.Highlight;
                    break;
                case Shape.Window:
                case Shape.Polygon:
                case Shape.RegularPolygon:
                case Shape.Circle:
                    mMode = TransientDrawingMode.DirectShortTerm;
                    break;
                default:
                    mMode = TransientDrawingMode.Main;
                    break;
            }

            Prompts = prompts;
            CollectedPoints = new Point3dCollection();
        }

        private void Editor_PointMonitor(object sender, PointMonitorEventArgs e)
        {
            if (mTempPline != null)
            {
                TransientManager.CurrentTransientManager.EraseTransient(mTempPline, new IntegerCollection());
                if (mTempPline != null && !mTempPline.IsDisposed)
                {
                    mTempPline.Dispose();
                    mTempPline = null;
                }
            }

            Point3d compPt = e.Context.ComputedPoint.TransformBy(AcadApp.DocumentManager.MdiActiveDocument.Editor.CurrentUserCoordinateSystem.Inverse());

            if (mShape == Shape.Window)
            {
                BuildupWindowVertices(m1stPoint, compPt);
            }
            else if (mShape == Shape.Fence)
            {
                BuildupFenceVertices(compPt);
            }
            else if (mShape == Shape.Polygon)
            {
                BuildupPolygonVertices(compPt);
            }
            else if (mShape == Shape.RegularPolygon)
            {
                BuildupRegularPolygonVertices(compPt);
            }
            else if (mShape == Shape.Circle)
            {
                BuildupRegularPolygonVertices(compPt);
            }

            mTempPline.TransformBy(AcadApp.DocumentManager.MdiActiveDocument.Editor.CurrentUserCoordinateSystem);
            TransientManager.CurrentTransientManager.AddTransient(mTempPline, mMode, 128, new IntegerCollection());
        }

        public Point3dCollection Collect()
        {
            if (mShape == Shape.Window)
            {
                CollectWindowPoints();
            }
            else if (mShape == Shape.Fence)
            {
                CollectFencePoints();
            }
            else if (mShape == Shape.Polygon)
            {
                CollectPolygonPoints();
            }
            else if (mShape == Shape.RegularPolygon)
            {
                mSegmentCount = GetRegularPolygonSideCount();
                CollectRegularPolygonPoints();
            }
            else if (mShape == Shape.Circle)
            {
                CollectRegularPolygonPoints();
            }

            return CollectedPoints;
        }

        private int GetRegularPolygonSideCount()
        {
            PromptIntegerResult prPntRes;
            PromptIntegerOptions prPntOpt = new PromptIntegerOptions("");
            prPntOpt.AllowNone = true;
            prPntOpt.AllowArbitraryInput = true;
            prPntOpt.AllowNegative = false;
            prPntOpt.AllowZero = false;
            prPntOpt.DefaultValue = 5;
            prPntOpt.LowerLimit = 3;
            prPntOpt.Message = "\nRegular polygon side";
            prPntOpt.UpperLimit = 36;
            prPntOpt.UseDefaultValue = true;

            prPntRes = AcadApp.DocumentManager.MdiActiveDocument.Editor.GetInteger(prPntOpt);
            if (prPntRes.Status == PromptStatus.OK)
                return prPntRes.Value;
            else
                throw new System.Exception("Regular polygon side input failed!");
        }

        private void BuildupRegularPolygonVertices(Point3d tempPt)
        {
            mTempPline = new Autodesk.AutoCAD.DatabaseServices.Polyline();
            mTempPline.SetDatabaseDefaults();
            mTempPline.Closed = true;
            mTempPline.ColorIndex = mColorIndex;

            mDist = m1stPoint.DistanceTo(tempPt);
            double angle = m1stPoint.GetVectorTo(tempPt).AngleOnPlane(new Plane(Point3d.Origin, Vector3d.ZAxis));
            CollectedPoints.Clear();
            for (int i = 0; i < mSegmentCount; i++)
            {
                Point3d pt = m1stPoint.Add(new Vector3d(mDist * (Math.Cos(angle + Math.PI * 2 * i / mSegmentCount)),
                                                        mDist * (Math.Sin(angle + Math.PI * 2 * i / mSegmentCount)),
                                                        m1stPoint.Z));
                CollectedPoints.Add(pt);
                mTempPline.AddVertexAt(mTempPline.NumberOfVertices, new Point2d(pt.X, pt.Y), 0, 1, 1);
            }
        }

        private void CollectRegularPolygonPoints()
        {
            PromptPointResult prPntRes1;
            PromptPointOptions prPntOpt = new PromptPointOptions("\nCenter");
            prPntOpt.AllowNone = true;

            prPntRes1 = AcadApp.DocumentManager.MdiActiveDocument.Editor.GetPoint(prPntOpt);
            if (prPntRes1.Status == PromptStatus.OK)
                m1stPoint = prPntRes1.Value;
            else
                throw new System.Exception("Center picking failed!");

            AcadApp.DocumentManager.MdiActiveDocument.Editor.PointMonitor += Editor_PointMonitor;

            PromptDistanceOptions prPntOpt2 = new PromptDistanceOptions("");
            prPntOpt2.AllowArbitraryInput = true;
            prPntOpt2.AllowNegative = false;
            prPntOpt2.AllowNone = true;
            prPntOpt2.AllowZero = false;
            prPntOpt2.BasePoint = m1stPoint;
            prPntOpt2.DefaultValue = 10.0;
            prPntOpt2.Message = "\nRadius";
            prPntOpt2.Only2d = true;
            prPntOpt2.UseBasePoint = true;
            prPntOpt2.UseDashedLine = true;
            prPntOpt2.UseDefaultValue = true;

            PromptDoubleResult prPntRes2 = AcadApp.DocumentManager.MdiActiveDocument.Editor.GetDistance(prPntOpt2);
            if (prPntRes2.Status != PromptStatus.OK)
                throw new System.Exception("Radius input failed!");

            mDist = prPntRes2.Value;

            AcadApp.DocumentManager.MdiActiveDocument.Editor.PointMonitor -= Editor_PointMonitor;
        }

        private void CollectPolygonPoints()
        {
            PromptPointResult prPntRes1;
            PromptPointOptions prPntOpt = new PromptPointOptions("\nPolygon point: (Enter to finish)");
            prPntOpt.AllowNone = true;

            AcadApp.DocumentManager.MdiActiveDocument.Editor.PointMonitor += Editor_PointMonitor;
            do
            {
                prPntRes1 = AcadApp.DocumentManager.MdiActiveDocument.Editor.GetPoint(prPntOpt);
                if (prPntRes1.Status == PromptStatus.OK)
                {
                    m1stPoint = prPntRes1.Value;
                    CollectedPoints.Add(m1stPoint);
                }
                else if (prPntRes1.Status != PromptStatus.None)
                {
                    throw new System.Exception("Polygon point picking failed!");
                }
            } while (prPntRes1.Status == PromptStatus.OK);
            AcadApp.DocumentManager.MdiActiveDocument.Editor.PointMonitor -= Editor_PointMonitor;
        }

        private void BuildupPolygonVertices(Point3d tempPt)
        {
            mTempPline = new Autodesk.AutoCAD.DatabaseServices.Polyline();
            mTempPline.SetDatabaseDefaults();
            mTempPline.Closed = true;
            mTempPline.ColorIndex = mColorIndex;

            foreach (Point3d pt in CollectedPoints)
            {
                mTempPline.AddVertexAt(mTempPline.NumberOfVertices, new Point2d(pt.X, pt.Y), 0, 1, 1);
            }
            mTempPline.AddVertexAt(mTempPline.NumberOfVertices, new Point2d(tempPt.X, tempPt.Y), 0, 1, 1);
        }

        private void CollectFencePoints()
        {
            PromptPointResult prPntRes1;
            PromptPointOptions prPntOpt = new PromptPointOptions("\nFence point (Enter to finish)");
            prPntOpt.AllowNone = true;

            AcadApp.DocumentManager.MdiActiveDocument.Editor.PointMonitor += Editor_PointMonitor;
            do
            {
                prPntRes1 = AcadApp.DocumentManager.MdiActiveDocument.Editor.GetPoint(prPntOpt);
                if (prPntRes1.Status == PromptStatus.OK)
                {
                    m1stPoint = prPntRes1.Value;
                    CollectedPoints.Add(m1stPoint);
                }
            } while (prPntRes1.Status == PromptStatus.OK);
            AcadApp.DocumentManager.MdiActiveDocument.Editor.PointMonitor -= Editor_PointMonitor;
        }

        private void BuildupFenceVertices(Point3d tempPt)
        {
            mTempPline = new Autodesk.AutoCAD.DatabaseServices.Polyline();
            mTempPline.SetDatabaseDefaults();
            mTempPline.Closed = false;
            mTempPline.ColorIndex = mColorIndex;

            foreach (Point3d pt in CollectedPoints)
            {
                mTempPline.AddVertexAt(mTempPline.NumberOfVertices, new Point2d(pt.X, pt.Y), 0, 1, 1);
            }
            mTempPline.AddVertexAt(mTempPline.NumberOfVertices, new Point2d(tempPt.X, tempPt.Y), 0, 1, 1);
        }

        private void CollectWindowPoints()
        {
            while (true)
            {
                var prompt1 = (Prompts.Count == 2) ?
                    string.Format("\n{0}", Prompts[0]) : string.Format("\n{0}", "请指定框选的第一个角点");
                PromptPointResult prPntRes1 = AcadApp.DocumentManager.MdiActiveDocument.Editor.GetPoint(prompt1);
                if (prPntRes1.Status != PromptStatus.OK)
                    throw new System.Exception("The 1st corner picking failed!");
                m1stPoint = prPntRes1.Value;
                CollectedPoints.Add(m1stPoint);
                BuildupWindowVertices(m1stPoint, m1stPoint);

                using (var ov = new PointMonitorEventHandlerOverride(Editor_PointMonitor))
                {
                    var prompt2 = (Prompts.Count == 2) ?
                        string.Format("\n{0}", Prompts[1]) : string.Format("\n{0}", "请指定框选的第二个角点");
                    PromptPointOptions options = new PromptPointOptions(prompt2)
                    {
                        AllowNone = false,
                    };
                    options.Keywords.Add("Undo", "Undo", "放弃(U)");
                    PromptPointResult prPntRes2 = AcadApp.DocumentManager.MdiActiveDocument.Editor.GetPoint(options);
                    if (prPntRes2.Status == PromptStatus.Keyword)
                    {
                        ClearVertices();
                        CollectedPoints.Clear();
                        m1stPoint = Point3d.Origin;
                        continue;
                    }
                    else if (prPntRes2.Status == PromptStatus.OK)
                    {
                        CollectedPoints.Add(prPntRes2.Value);
                        break;
                    }
                    else
                    {
                        throw new System.Exception("The 2nd corner picking failed!");
                    }
                }
            }
        }

        private void BuildupWindowVertices(Point3d corner1, Point3d corner2)
        {
            mTempPline = new Autodesk.AutoCAD.DatabaseServices.Polyline();
            mTempPline.SetDatabaseDefaults();
            mTempPline.Closed = true;
            mTempPline.ColorIndex = mColorIndex;

            mTempPline.AddVertexAt(mTempPline.NumberOfVertices, new Point2d(corner1.X, corner1.Y), 0, 1, 1);
            mTempPline.AddVertexAt(mTempPline.NumberOfVertices, new Point2d(corner2.X, corner1.Y), 0, 1, 1);
            mTempPline.AddVertexAt(mTempPline.NumberOfVertices, new Point2d(corner2.X, corner2.Y), 0, 1, 1);
            mTempPline.AddVertexAt(mTempPline.NumberOfVertices, new Point2d(corner1.X, corner2.Y), 0, 1, 1);
        }

        private void ClearVertices()
        {
            if (mTempPline != null && !mTempPline.IsDisposed)
            {
                TransientManager.CurrentTransientManager.EraseTransient(mTempPline, new IntegerCollection());
                mTempPline.Dispose();
                mTempPline = null;
            }

        }

        public void Dispose()
        {
            AcadApp.DocumentManager.MdiActiveDocument.Editor.PointMonitor -= Editor_PointMonitor;

            if (mTempPline != null && !mTempPline.IsDisposed)
            {
                TransientManager.CurrentTransientManager.EraseTransient(mTempPline, new IntegerCollection());
                mTempPline.Dispose();
                mTempPline = null;
            }

            CollectedPoints.Dispose();
        }
    }
}
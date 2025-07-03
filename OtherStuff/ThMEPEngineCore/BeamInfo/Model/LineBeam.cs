using System;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.DatabaseServices;
using System.Collections.Generic;
using System.Linq;
using ThMEPEngineCore.CAD;

namespace ThMEPEngineCore.BeamInfo.Model
{
    public class LineBeam : Beam
    {
        public double Width
        {
            get
            {
                var p1 = new Point2d(UpStartPoint.X, UpStartPoint.Y);
                var p2 = new Point2d(DownStartPoint.X, DownStartPoint.Y);
                return p1.GetDistanceTo(p2);
            }
        }

        public LineBeam(Curve newUpLine, Curve newDownLine)
        {
            if(newUpLine is Line firstLine && newDownLine is Line secondLine)
            {
                BeamNormal = ResetBeamDirection(firstLine.StartPoint.GetVectorTo(firstLine.EndPoint).GetNormal());
                List<Line> lines = CreateOutLine(firstLine, secondLine, BeamNormal);
                if(lines.Count==2)
                {
                    UpStartPoint = lines[0].StartPoint;
                    UpEndPoint = lines[0].EndPoint;
                    DownStartPoint = lines[1].StartPoint;
                    DownEndPoint = lines[1].EndPoint;
                    BeamSPointSolid = CreatePolyline(UpStartPoint, DownStartPoint, -BeamNormal, 10);
                    BeamEPointSolid = CreatePolyline(UpEndPoint, DownEndPoint, BeamNormal, 10);
                    StartPoint = ThGeometryTool.GetMidPt(UpStartPoint, DownStartPoint);
                    EndPoint = ThGeometryTool.GetMidPt(UpEndPoint, DownEndPoint);
                    lines.ForEach(o => o.Dispose());
                }               
            }
        }
        private Vector3d ResetBeamDirection(Vector3d originBeamDir)
        {
            double angle = Vector3d.XAxis.GetAngleTo(originBeamDir, Vector3d.ZAxis) /Math.PI*180.0;
            angle %= 360.0;
            if (Vector3d.XAxis.IsParallelTo(originBeamDir))
            {
                return Vector3d.XAxis;
            }
            else if(Vector3d.YAxis.IsParallelTo(originBeamDir))
            {
                return Vector3d.YAxis;
            }
            else if((angle>0.0 && angle < 90.0) || (angle > 180.0 && angle < 270.0))
            {
                //第一、第三象限
                double rotateAng = Vector3d.XAxis.GetAngleTo(originBeamDir, Vector3d.ZAxis) % Math.PI;
                return Vector3d.XAxis.RotateBy(rotateAng, Vector3d.ZAxis);
            }
            else
            {
                //第二、第四象限
                double rotateAng = Vector3d.XAxis.GetAngleTo(originBeamDir, Vector3d.ZAxis) % Math.PI;
                rotateAng += Math.PI;
                return Vector3d.XAxis.RotateBy(rotateAng, Vector3d.ZAxis);
            }
        }
        private List<Line> CreateOutLine(Line first, Line second, Vector3d beamDir)
        {
            Vector3d upRight = beamDir.RotateBy(Math.PI / 2.0, Vector3d.ZAxis);
            Plane plane = new Plane(first.StartPoint, beamDir);
            Matrix3d wcsToUcs = Matrix3d.WorldToPlane(plane);
            Matrix3d ucsToWcs = Matrix3d.PlaneToWorld(plane);
            Point3d firstStartPt = first.StartPoint.TransformBy(wcsToUcs);
            Point3d firstEndPt = first.EndPoint.TransformBy(wcsToUcs);
            Point3d secondStartPt = second.StartPoint.TransformBy(wcsToUcs);
            Point3d secondEndPt = second.EndPoint.TransformBy(wcsToUcs);
            double firstMinZ = Math.Min(firstStartPt.Z, firstEndPt.Z);
            double firstMaxZ = Math.Max(firstStartPt.Z, firstEndPt.Z);
            double secondMinZ = Math.Min(secondStartPt.Z, secondEndPt.Z);
            double secondMaxZ = Math.Max(secondStartPt.Z, secondEndPt.Z);
            if (secondMinZ >= firstMaxZ || secondMaxZ <= firstMinZ)
            {
                return new List<Line>();
            }
            List<double> zValues = new List<double> { firstStartPt.Z, firstEndPt.Z, secondStartPt.Z, secondEndPt.Z };
            double minZ = zValues.OrderBy(o => o).FirstOrDefault();
            double maxZ = zValues.OrderByDescending(o => o).FirstOrDefault();
            firstStartPt = new Point3d(firstStartPt.X, firstStartPt.Y, minZ);
            firstEndPt = firstStartPt + Vector3d.ZAxis.MultiplyBy(maxZ - minZ); 

            secondStartPt = new Point3d(secondStartPt.X, secondStartPt.Y, minZ);
            secondEndPt = secondStartPt + Vector3d.ZAxis.MultiplyBy(maxZ - minZ);
            firstStartPt = firstStartPt.TransformBy(ucsToWcs);
            firstEndPt = firstEndPt.TransformBy(ucsToWcs);
            secondStartPt = secondStartPt.TransformBy(ucsToWcs);
            secondEndPt = secondEndPt.TransformBy(ucsToWcs);
            plane.Dispose();
            Point3d firstMidPt = ThGeometryTool.GetMidPt(firstStartPt, firstEndPt);
            Point3d secondMidPt = ThGeometryTool.GetMidPt(secondStartPt, secondEndPt);
            Vector3d midVec = firstMidPt.GetVectorTo(secondMidPt);
            if (midVec.DotProduct(upRight) < 0.0)
            {
                return new List<Line>
                {
                    new Line(firstStartPt, firstEndPt),
                    new Line(secondStartPt, secondEndPt)
                };
            }
            else
            {
                return new List<Line>
                {
                    new Line(secondStartPt, secondEndPt) ,
                    new Line(firstStartPt, firstEndPt)
                };
            }
        }
        public override Polyline BeamBoundary
        {
            get
            {
                Polyline resPolyline = new Polyline(4)
                {
                    Closed = true,
                };
                resPolyline.AddVertexAt(0, new Point2d(UpStartPoint.X, UpStartPoint.Y), 0, 0, 0);
                resPolyline.AddVertexAt(1, new Point2d(DownStartPoint.X, DownStartPoint.Y), 0, 0, 0);
                resPolyline.AddVertexAt(2, new Point2d(DownEndPoint.X, DownEndPoint.Y), 0, 0, 0);
                resPolyline.AddVertexAt(3, new Point2d(UpEndPoint.X, UpEndPoint.Y), 0, 0, 0);

                return resPolyline;
            }
        }

        public void GetOrderPoints(out Point3d upLP, out Point3d upMP, out Point3d upRP, out Point3d downLP, out Point3d downMP, out Point3d downRP)
        {
            upLP = UpStartPoint;
            upRP = UpEndPoint;
            upMP = new Point3d((UpStartPoint.X + UpEndPoint.X) / 2, (UpStartPoint.Y + UpEndPoint.Y) / 2, 0);
            downLP = DownStartPoint;
            downRP = DownEndPoint;
            downMP = new Point3d((DownStartPoint.X + DownEndPoint.X) / 2, (DownStartPoint.Y + DownEndPoint.Y) / 2, 0);

            Vector3d moveDir = (upMP - downMP).GetNormal();
            if (Math.Abs(moveDir.Y) < 0.0001)
            {
                if (moveDir.X > 0)
                {
                    Point3d tempP = upMP;
                    upMP = downMP;
                    downMP = tempP;
                    Vector3d judgeDir = (upRP - upLP).GetNormal().CrossProduct(-moveDir);
                    if (judgeDir.Z < 0)
                    {
                        upRP = DownStartPoint;
                        upLP = DownEndPoint;
                        downRP = UpStartPoint;
                        downLP = UpEndPoint;
                    }
                    else
                    {
                        upLP = DownStartPoint;
                        upRP = DownEndPoint;
                        downLP = UpStartPoint;
                        downRP = UpEndPoint;
                    }
                }
                else
                {
                    Vector3d judgeDir = (upRP - upLP).GetNormal().CrossProduct(moveDir);
                    if (judgeDir.Z < 0)
                    {
                        upLP = UpEndPoint;
                        upRP = UpStartPoint;
                        downLP = DownEndPoint;
                        downRP = DownStartPoint;
                    }
                }
            }
            else if (moveDir.Y < 0)
            {
                Point3d tempP = upMP;
                upMP = downMP;
                downMP = tempP;
                Vector3d judgeDir = (upLP - upRP).GetNormal().CrossProduct(-moveDir);
                if (judgeDir.Z > 0)
                {
                    
                    upRP = DownStartPoint;
                    upLP = DownEndPoint;
                    downRP = UpStartPoint;
                    downLP = UpEndPoint;
                }
                else
                {
                    upLP = DownStartPoint;
                    upRP = DownEndPoint;
                    downLP = UpStartPoint;
                    downRP = UpEndPoint;
                }
            }
            else
            {
                Vector3d judgeDir = (upRP - upLP).GetNormal().CrossProduct(moveDir);
                if (judgeDir.Z < 0)
                {
                    upLP = UpEndPoint;
                    upRP = UpStartPoint;
                    downLP = DownEndPoint;
                    downRP = DownStartPoint;
                }
            }
        }
    }
}

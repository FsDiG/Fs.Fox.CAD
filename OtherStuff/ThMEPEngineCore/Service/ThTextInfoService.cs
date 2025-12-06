using System;
using System.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using ThCADExtension;
using ThMEPEngineCore.CAD;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.DatabaseServices;

namespace ThMEPEngineCore.Service
{
    public class ThTextInfoService
    {
        private static string DoorMarkPattern = @"[M]{1}\d+[a-z*]?";
        public static Line GetCenterLine(Entity ent)
        {
            if (ent is DBText dbText)
            {
                return GetCenterLine(dbText);
            }
            else if (ent is MText mText)
            {
                return GetCenterLine(mText);
            }
            else
            {
                return new Line();
            }
        }
        public static Line GetCenterLine(DBText dbText)
        {
            var boundary = dbText.TextOBB();
            return GetCenterLine(boundary,dbText.Rotation);
        }
        public static Line GetCenterLine(MText mText)
        {
            var boundary = mText.TextOBB();
            return GetCenterLine(boundary, mText.Direction.GetAngleTo(Vector3d.XAxis));
        }
        private static Line GetCenterLine(Polyline textObb,double rotation)
        {
            var lines = textObb.ToLines();
            lines = lines.Where(o => IsParallel(o.Angle, rotation)).ToList();

            if (lines[0].LineDirection().IsCodirectionalTo(lines[1].LineDirection(), new Tolerance(1, 1)))
            {
                var sp = lines[0].StartPoint.GetMidPt(lines[1].StartPoint);
                var ep = lines[0].EndPoint.GetMidPt(lines[1].EndPoint);
                return new Line(sp, ep);
            }
            else
            {
                var sp = lines[0].StartPoint.GetMidPt(lines[1].EndPoint);
                var ep = lines[0].EndPoint.GetMidPt(lines[1].StartPoint);
                return new Line(sp, ep);
            }
        }
        
        private static bool IsParallel(double firstAng,double secondAng)
        {
            firstAng %= Math.PI;
            secondAng %= Math.PI;
            firstAng = (firstAng / Math.PI) * 180.0;
            secondAng = (secondAng / Math.PI) * 180.0;
            return Math.Abs(firstAng - secondAng) <= 1.0 ||
                Math.Abs(Math.Abs(firstAng - secondAng) - 180.0) <= 1.0;
        }

        public static List<string> Parse(string content)
        {
            var results = new List<string>();
            var regex = new Regex(DoorMarkPattern);
            var matches = regex.Matches(content);
            if (matches.Count == 1)
            {
                if (matches[0].Value.Length == content.Length)
                {
                    results.Add("M");
                    string pattern2 = @"\d+";
                    var regex1 = new Regex(pattern2);
                    results.Add(regex1.Match(matches[0].Value).Value);
                    string pattern3 = @"[a-z]{1}$";
                    var regex2 = new Regex(pattern3);
                    var matches1 = regex2.Matches(matches[0].Value);
                    if (matches1.Count == 1)
                    {
                        results.Add(matches1[0].Value);
                    }
                    return results;
                }
            }
            return results;
        }
        public static bool IsDoorMark(string content)
        {
            var regex = new Regex(DoorMarkPattern);
            return regex.IsMatch(content);
        }       
        public static double GetLength(List<string> values)
        {
            double length = 0.0;
            if(values.Count >= 2 && values.Count <= 3)
            {
                string lengthStr = values[1];
                length = double.Parse(lengthStr.Substring(0, 2)) * 100.0;
                if (values.Count == 3)
                {
                    switch (values[2])
                    {
                        case "a":
                            length += 50.0;
                            break;
                        default:
                            length += 0.0;
                            break;
                    }
                }
            }          
            return length;
        }
        public static string GetText(Entity ent)
        {
            if(ent is DBText dbText)
            {
                return dbText.TextString;
            }
            else if(ent is MText mText)
            {
                return mText.Text;
            }
            else
            {
                return "";
            }
        }
        public static double GetHeight(Entity ent)
        {
            if (ent is DBText dbText)
            {
                return dbText.Height;
            }
            else if (ent is MText mText)
            {
                return mText.ActualHeight;
            }
            else
            {
                return Math.Abs(ent.GeometricExtents.MaxPoint.Y- ent.GeometricExtents.MinPoint.Y);
            }
        }
        public static double GetRotation(Entity ent)
        {
            if (ent is DBText dbText)
            {
                return dbText.Rotation;
            }
            else if (ent is MText mText)
            {
                return mText.Direction.GetAngleTo(Vector3d.XAxis);
            }
            else
            {
                return 0.0;
            }
        }
    }
}

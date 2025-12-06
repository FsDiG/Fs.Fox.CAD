using System;
using Linq2Acad;
using System.Linq;
using ThCADCore.NTS;
using ThCADExtension;
using ThMEPEngineCore.CAD;
using ThMEPEngineCore.Algorithm;
using Autodesk.AutoCAD.Geometry;
using System.Collections.Generic;
using Autodesk.AutoCAD.DatabaseServices;
using NetTopologySuite.Geometries;
using Dreambuild.AutoCAD;
using ThMEPEngineCore.Model;
using GeometryExtensions;

namespace ThMEPEngineCore.Service
{
    public class ThRoomDbExtension : ThDbExtension, IDisposable
    {
        public List<ThIfcSpace> Rooms { get; set; }
        public ThRoomDbExtension(Database db) : base(db)
        {
            LayerFilter = ThRoomLayerManager.CurveXrefLayers(db);
            Rooms = new List<ThIfcSpace>();
        }
        public void Dispose()
        {
        }

        public override void BuildElementCurves()
        {
            using (AcadDatabase acadDatabase = AcadDatabase.Use(HostDb))
            {
                foreach (var ent in acadDatabase.ModelSpace)
                {
                    if (ent is BlockReference blkRef)
                    {
                        if (blkRef.BlockTableRecord.IsNull)
                        {
                            continue;
                        }
                        BlockTableRecord btr = acadDatabase.Element<BlockTableRecord>(blkRef.BlockTableRecord);
                        var mcs2wcs = blkRef.BlockTransform.PreMultiplyBy(Matrix3d.Identity);
                        Rooms.AddRange(BuildRoomSpaces(blkRef, mcs2wcs));
                    }
                }
            }
        }

        private IEnumerable<ThIfcSpace> BuildRoomSpaces(BlockReference blockReference, Matrix3d matrix)
        {
            using (AcadDatabase acadDatabase = AcadDatabase.Use(HostDb))
            {
                var spaces = new List<ThIfcSpace>();
                if (IsBuildElementBlockReference(blockReference))
                {
                    var blockTableRecord = acadDatabase.Blocks.Element(blockReference.BlockTableRecord);
                    if (IsBuildElementBlock(blockTableRecord))
                    {
                        foreach (var objId in blockTableRecord)
                        {
                            var dbObj = acadDatabase.Element<Entity>(objId);
                            if (dbObj is BlockReference blockObj)
                            {
                                if (blockObj.BlockTableRecord.IsNull)
                                {
                                    continue;
                                }
                                if (IsBuildElementBlockReference(blockObj))
                                {
                                    var mcs2wcs = blockObj.BlockTransform.PreMultiplyBy(matrix);
                                    spaces.AddRange(BuildRoomSpaces(blockObj, mcs2wcs));
                                }
                            }
                            else if (dbObj is Polyline polyline)
                            {
                                if (IsBuildElement(polyline) && 
                                    CheckLayerValid(polyline))
                                {
                                    var roomInfo = GetRoomInfo(polyline);
                                    if(roomInfo!=null && roomInfo.Properties.Count>0)
                                    {
                                       //var boundary = polyline.GetTransformedCopy(matrix) as Polyline;
                                        var space = new ThIfcSpace();                                       
                                        foreach(var item in roomInfo.Properties)
                                        {
                                            if (item.Key == "边界")
                                            {
                                                var boundary = GetRoomBoundary(item.Value);
                                                space.Boundary = boundary.GetTransformedCopy(matrix) as Polyline;
                                            }
                                            else
                                            {
                                                space.Properties.Add(item.Key, item.Value);
                                            }
                                        }
                                       if(space.Boundary.Area>0)
                                        {
                                            spaces.Add(space);
                                        }
                                    }
                                }
                            }
                        }
                        var xclip = blockReference.XClipInfo();
                        if (xclip.IsValid)
                        {
                            xclip.TransformBy(matrix);
                            return spaces.Where(o => xclip.Contains(o.Boundary));
                        }
                    }
                }
                return spaces;
            }
        }
        public override void BuildElementTexts()
        {
            throw new NotImplementedException();
        }
        protected override bool IsBuildElement(Entity entity)
        {
            return entity.Hyperlinks.Count > 0;
        }
        private ThPropertySet GetRoomInfo(Entity entity)
        {
            return CreateWithHyperlink(entity.Hyperlinks[0].Description);
        }
        private ThPropertySet CreateWithHyperlink(string hyperlink)
        {
            var propertySet = new ThPropertySet();
            // 按分割符“__”分割属性
            var firstSplitChar = new string[] { "__"};
            var secondSplitChar = new string[] { "：" };
            var properties = hyperlink.Split(firstSplitChar,StringSplitOptions.RemoveEmptyEntries);
            foreach (var property in properties)
            {
                var keyValue = property.Split(secondSplitChar, StringSplitOptions.RemoveEmptyEntries);
                propertySet.Properties.Add(keyValue[0], keyValue[1]);
            }
            // 返回属性集
            return propertySet;
        }
        private Polyline GetRoomBoundary(string boundaries)
        {
            var boundary = new Polyline();
            var lines = new List<Line>();
            string[] segments = boundaries.Split(';');
            foreach (var segment in segments)
            {
                string[] coordinates = segment.Split('_');
                var ptList = new List<Point3d>();
                foreach (var coordinate in coordinates)
                {
                    string[] values = coordinate.Split(',');
                    if(values.Length==2)
                    {
                        var pt = new Point3d(double.Parse(values[0]), double.Parse(values[1]),0);
                        ptList.Add(pt);
                    }
                }
                if(ptList.Count==2)
                {
                    lines.Add(new Line(ptList[0], ptList[1]));
                }
            }
            var polylineSegments = new PolylineSegmentCollection();
            foreach(var line in lines)
            {
                polylineSegments.Add(new PolylineSegment(line.StartPoint.ToPoint2d(), line.EndPoint.ToPoint2d()));
            }
            return polylineSegments.ToPolyline();
        }
    }
}

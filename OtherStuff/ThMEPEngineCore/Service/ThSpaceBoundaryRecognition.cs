using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Linq2Acad;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThMEPEngineCore.Algorithm;
using ThMEPEngineCore.CAD;

namespace ThMEPEngineCore.Service
{
    public class ThSpaceBoundaryRecognition : ThDbExtension,IDisposable
    {
        public List<Curve> Curves { get; set; }
        public ThSpaceBoundaryRecognition(Database db):base(db)
        {
            Curves = new List<Curve>();
            LayerFilter = ThSpaceBoundarLayerManager.CurveXrefLayers(db);
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
                        Curves.AddRange(BuildElementCurves(blkRef, mcs2wcs));
                    }
                }
            }
        }
        public override void BuildElementTexts()
        {
            throw new NotImplementedException();
        }   
        private IEnumerable<Curve> BuildElementCurves(BlockReference blockReference, Matrix3d matrix)
        {            
            using (AcadDatabase acadDatabase = AcadDatabase.Use(HostDb))
            {
                List<Curve> curves = new List<Curve>();
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
                                    curves.AddRange(BuildElementCurves(blockObj, mcs2wcs));
                                }
                            }
                            else if (dbObj is Curve curve)
                            {
                                if (CheckLayerValid(curve))
                                {
                                    curves.Add(curve.GetTransformedCopy(matrix) as Curve);
                                }
                            }
                        }
                        var xclip = blockReference.XClipInfo();
                        if (xclip!=null && xclip.IsValid)
                        {
                            xclip.TransformBy(matrix);
                            return curves.Where(o => xclip.Contains(o) || xclip.Intersects(o));
                        }
                    }
                }
                return curves;
            }
        }
        public void Dispose()
        {
        }
    }
}

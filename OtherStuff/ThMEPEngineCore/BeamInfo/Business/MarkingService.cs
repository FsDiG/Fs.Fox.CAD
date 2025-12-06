using Linq2Acad;
using ThCADExtension;
using Autodesk.AutoCAD.Geometry;
using System.Collections.Generic;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.ApplicationServices;
using ThMEPEngineCore.BeamInfo.Model;
using ThMEPEngineCore.BeamInfo.Utils;

namespace ThMEPEngineCore.BeamInfo.Business
{
    public class MarkingService
    {
        private SelectionFilter filter;
        private string markLayerName;
        private Document _doc;
        private AcadDatabase _acdb;

        public MarkingService(Document doc, AcadDatabase acdb, string layerName)
        {
            filter = new SelectionFilter(new TypedValue[] { new TypedValue((int)DxfCode.LayerName, layerName) });
            markLayerName = layerName;
            _doc = doc;
            _acdb = acdb;
        }

        public void AddSelectFilter(List<TypedValue> types)
        {
            TypedValue[] tValue = new TypedValue[types.Count + 1];
            tValue[types.Count] = new TypedValue((int)DxfCode.LayerName, markLayerName);
            for (int i = 0; i < types.Count; i++)
            {
                tValue[i] = types[0];
            }
            filter = new SelectionFilter(tValue);
        }

        /// <summary>
        /// 获取特定图层下所有标注
        /// </summary>
        /// <returns></returns>
        public List<MarkingInfo> GetAllMarking()
        {
            List<MarkingInfo> allMarkings = new List<MarkingInfo>();
            Editor ed = _doc.Editor;
            PromptSelectionResult ProSset = ed.SelectAll(filter);
            if (ProSset.Status == PromptStatus.OK)
            {
                ObjectId[] oids = ProSset.Value.GetObjectIds();
                allMarkings = ClassificationMarking(oids, MarkingType.All);
            }
            
            return allMarkings;
        }

        /// <summary>
        /// 获取一定范围内的标注
        /// </summary>
        /// <param name="pt1"></param>
        /// <param name="pt2"></param>
        /// <param name="dir"></param>
        /// <param name="offset"></param>
        /// <param name="markingType"></param>
        /// <returns></returns>
        public List<MarkingInfo> GetMarking(Point3d pt1, Point3d pt2, Vector3d dir, double offset = 0, MarkingType markingType = MarkingType.All)
        {
            Vector3d moveDir = Vector3d.ZAxis.CrossProduct(dir);
            if (offset > 0)
            {
                Vector3d ptDir = (pt1 - pt2).GetNormal();
                if (moveDir.DotProduct(ptDir) > 0)
                {
                    pt1 = pt1 + moveDir * offset;
                    pt2 = pt2 - moveDir * offset;
                }
                else
                {
                    pt1 = pt1 - moveDir * offset;
                    pt2 = pt2 + moveDir * offset;
                }
            }

            List<MarkingInfo> markingInfos = new List<MarkingInfo>();
            var res = GetObjectUtils.GetObjectWithBounding(_doc.Editor, pt1, pt2, moveDir, filter);
            if (res.Status == PromptStatus.OK)
            {
                var objIds = res.Value.GetObjectIds();
                markingInfos = ClassificationMarking(objIds, markingType);
            }
            
            return markingInfos;
        }

        /// <summary>
        /// 将标注根据类型分类
        /// </summary>
        /// <param name="objectIds"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public List<MarkingInfo> ClassificationMarking(ObjectId[] objectIds, MarkingType type)
        {
            List<MarkingInfo> markLst = new List<MarkingInfo>();
            foreach (var objId in objectIds)
            {
                Entity entity = _acdb.Element<Entity>(objId);
                MarkingInfo markingInfo = new MarkingInfo();
                if (entity is Line line)
                {
                    if (type == MarkingType.All || type == MarkingType.Line)
                    {
                        Vector3d markDir = line.LineDirection();
                        markingInfo.Marking = entity;
                        markingInfo.Type = MarkingType.Line;
                        markingInfo.MarkingNormal = markDir;
                        markLst.Add(markingInfo);
                    }
                }
                else if (entity is DBText dBText)
                {
                    if (type == MarkingType.All || type == MarkingType.Text)
                    {
                        Vector3d markDir = Vector3d.XAxis.RotateBy(dBText.Rotation, Vector3d.ZAxis);
                        markingInfo.Marking = entity;
                        markingInfo.Type = MarkingType.Text;
                        markingInfo.MarkingNormal = markDir;
                        markingInfo.AlignmentPoint = dBText.AlignmentPoint;
                        markingInfo.Position = dBText.Position;
                        markLst.Add(markingInfo);
                    }
                }
            }

            return markLst;
        }
    }
}

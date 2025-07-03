using Autodesk.AutoCAD.DatabaseServices;
using Linq2Acad;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThMEPEngineCore.BeamInfo.Business;
using ThMEPEngineCore.BeamInfo.Model;

namespace ThMEPEngineCore.BeamInfo
{
    public class ThDistinguishBeamInfo
    {
        public List<Beam> CalBeamStruc(DBObjectCollection dBObjects)
        {
            using (AcadDatabase acdb = AcadDatabase.Active())
            {
                //0.预处理
                //  0.1 将多段线“炸”成线段
                var curves = ThBeamGeometryPreprocessor.ExplodeCurves(dBObjects);
                //  0.2 为了处理法向量和Z轴不平行的情况，需要将曲线投影到XY平面
                var beamCurves = ThBeamGeometryPreprocessor.ProjectXYCurves(curves);
                //  0.3 为了处理Z值不为0的情况，需要将曲线Z值设为0
                ThBeamGeometryPreprocessor.Z0Curves(ref beamCurves);
                //  0.4 过滤掉长度过短梁线
                var results = ThBeamGeometryPreprocessor.FilterCurves(beamCurves);

                //1.计算出匹配的梁
                CalBeamStruService calBeamService = new CalBeamStruService();
                var allBeam = calBeamService.GetBeamInfo(results);

                return allBeam;
            }
        }
    }
}

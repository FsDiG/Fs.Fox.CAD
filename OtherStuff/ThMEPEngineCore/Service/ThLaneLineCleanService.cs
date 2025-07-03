using Autodesk.AutoCAD.DatabaseServices;
using ThMEPEngineCore.LaneLine;

namespace ThMEPEngineCore.Service
{
    public class ThLaneLineCleanService
    {
        public double CollinearGap { get; set; }
        public double ExtendDistance { get; set; }

        public ThLaneLineCleanService()
        {
            CollinearGap = 2.0;
            ExtendDistance = 20.0;
        }

        public DBObjectCollection Clean(DBObjectCollection curves)
        {
            // 配置参数
            ThLaneLineEngine.extend_distance = ExtendDistance;
            ThLaneLineEngine.collinear_gap_distance = CollinearGap;

            // 合并处理
            var mergedLines = ThLaneLineEngine.Explode(curves);
            mergedLines = ThLaneLineMergeExtension.Merge(mergedLines);
            mergedLines = ThLaneLineEngine.Noding(mergedLines);
            mergedLines = ThLaneLineEngine.CleanZeroCurves(mergedLines);

            // 延伸处理
            var extendedLines = ThLaneLineExtendEngine.Extend(mergedLines);
            extendedLines = ThLaneLineMergeExtension.Merge(extendedLines);
            extendedLines = ThLaneLineEngine.Noding(extendedLines);
            extendedLines = ThLaneLineEngine.CleanZeroCurves(extendedLines);

            // 合并处理
            return ThLaneLineMergeExtension.Merge(mergedLines, extendedLines);
        }
    }
}

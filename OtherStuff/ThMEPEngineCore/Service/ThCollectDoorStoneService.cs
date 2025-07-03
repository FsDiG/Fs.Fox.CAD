using System;
using System.Linq;
using System.Collections.Generic;
using ThCADCore.NTS;
using ThCADExtension;
using Dreambuild.AutoCAD;
using ThMEPEngineCore.CAD;
using Autodesk.AutoCAD.DatabaseServices;

namespace ThMEPEngineCore.Service
{
    public class ThCollectDoorStoneService
    {
        public double FindRatio { get; set; } = 2.0;
        public List<Tuple<Entity, List<Polyline>, double>> Results { get; private set; }
        private DBObjectCollection DoorMarks { get; set; }
        private ThCADCoreNTSSpatialIndex DoorStoneSpatialIndex { get; set; }
        public ThCollectDoorStoneService(
            DBObjectCollection doorStones,
            DBObjectCollection doorMarks)
        {
            DoorMarks = doorMarks;
            DoorStoneSpatialIndex = new ThCADCoreNTSSpatialIndex(doorStones);
            Results = new List<Tuple<Entity, List<Polyline>, double>>();
        }
        public void Build()
        {
            DoorMarks
                .Cast<Entity>()
                .Where(o => ThTextInfoService.IsDoorMark(ThTextInfoService.GetText(o)))
                .ForEach(o =>
                {
                    var strList = ThTextInfoService.Parse(ThTextInfoService.GetText(o));
                    double length = ThTextInfoService.GetLength(strList);
                    if (length > 0)
                    {
                        var center = ThTextInfoService.GetCenterLine(o);
                        var height = ThTextInfoService.GetHeight(o);
                        var envelope = CreateEnvelope(center, height, length);
                        Results.Add(Tuple.Create(o, FindStones(envelope),length));
                    }
                });
        }
        private Polyline CreateEnvelope(Line center,double height,double length)
        {
            var vec = center.LineDirection();
            var midPt = center.StartPoint.GetMidPt(center.EndPoint);
            var sp = midPt - vec.MultiplyBy(length / 2.0);
            var ep = midPt + vec.MultiplyBy(length / 2.0);
            return ThDrawTool.ToOutline(sp, ep, FindRatio * height*2.0);
        }       
       
        private List<Polyline> FindStones(Polyline envelope)
        {
            return DoorStoneSpatialIndex.SelectCrossingPolygon(envelope).Cast<Polyline>().ToList();
        }
    }
}

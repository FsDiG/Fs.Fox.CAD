using System;
using System.Linq;
using System.Collections.Generic;
using NetTopologySuite.Index.KdTree;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.DatabaseServices;

namespace ThCADCore.NTS
{
    public class ThCADCoreNTSKdTree : IDisposable
    {
        private KdTree<object> Tree { get; set; }
        private Dictionary<KdNode<object>, Point3dCollection> Nodes { get; set; }

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public ThCADCoreNTSKdTree(double tolerance)
        {
            Tree = new KdTree<object>(tolerance);
            Nodes = new Dictionary<KdNode<object>, Point3dCollection>();
        }

        public void InsertLine(Line line)
        {
            InsertPoint(line.StartPoint);
            InsertPoint(line.EndPoint);
        }

        public void InsertPoint(Point3d pt)
        {
            var node = Tree.Insert(pt.ToNTSCoordinate(), null);
            if (!Nodes.ContainsKey(node))
            {
                Nodes[node] = new Point3dCollection();
            }
            if (!Nodes[node].Contains(pt))
            {
                Nodes[node].Add(pt);
            }
        }

        public Point3d Query(Point3d pt)
        {
            return Nodes.Where(o => o.Value.Contains(pt)).First().Key.Coordinate.ToAcGePoint3d();
        }
    }
}

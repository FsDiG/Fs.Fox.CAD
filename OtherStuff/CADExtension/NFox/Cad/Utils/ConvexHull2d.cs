using System.Collections.Generic;
using System.Linq;


using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.DatabaseServices.Filters;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;


using NFox.Collections;

namespace NFox.Cad
{
    /// <summary>
    /// 2d的凸包类
    /// </summary>
    public class ConvexHull2d : IEnumerable<Point2d>
    {
        private LoopList<Point2d> _ptlst = new LoopList<Point2d>();

        /// <summary>
        /// 凸包端点的数量
        /// </summary>
        public int Count
        {
            get { return _ptlst.Count; }
        }

        /// <summary>
        /// 创建凸包类型
        /// </summary>
        /// <param name="pnts">点集</param>
        internal ConvexHull2d(List<Point2d> pnts)
        {
            //按坐标排序,保证方向性
            var q1 =
                from p in pnts
                orderby p.X
                select p;
            List<Point2d> ptlst = q1.ToList();

            switch (ptlst.Count)
            {
                case 0:
                    return;

                case 1:
                    _ptlst.Add(ptlst[0]);
                    return;

                default:
                    _ptlst.Add(ptlst[0]);
                    _ptlst.Add(ptlst[1]);
                    break;
            }

            //如果共线
            int i = 2;
            if (_ptlst.First.Value.X == _ptlst.Last.Value.X)
            {
                for (; i < ptlst.Count; i++)
                {
                    if (ptlst[i].X == _ptlst.Last.Value.X)
                    {
                        double y = ptlst[i].Y;
                        if (y > _ptlst.Last.Value.Y)
                            _ptlst.Last.Value = ptlst[i];
                        else if (y < _ptlst.First.Value.Y)
                            _ptlst.First.Value = ptlst[i];

                        i++;
                    }
                    else
                    {
                        break;
                    }
                }
            }

            for (; i < ptlst.Count; i++)
            {
                if (GetArea(_ptlst.First.Value, _ptlst.Last.Value, ptlst[i]) == 0)
                    _ptlst.Last.Value = ptlst[i];
                else
                    break;
            }

            if (i == ptlst.Count)
                return;

            //保证逆时针方向
            if (IsClockWise(_ptlst.First.Value, _ptlst.Last.Value, ptlst[i]))
                _ptlst.Swap(_ptlst.First, _ptlst.Last);

            _ptlst.AddFirst(ptlst[i]);

            //依次比较
            for (i++; i < ptlst.Count; i++)
            {
                Point2d pnt = ptlst[i];
                int num = 0;
                LoopListNode<Point2d> from = _ptlst.First, to = _ptlst.First;
                //做左链
                while (IsClockWise(to.Next.Value, pnt, to.Value))
                {
                    to = to.Next;
                    num++;
                }
                //做右链
                while (IsClockWise(from.Previous.Value, from.Value, pnt))
                {
                    from = from.Previous;
                    num++;
                }
                _ptlst.LinkTo(from, to, num - 1);
                _ptlst.AddFirst(pnt);
            }
        }

        /// <summary>
        /// 判断点是否在凸包的外部
        /// </summary>
        /// <param name="pnt">点</param>
        /// <returns>在外部返回true，反之返回false</returns>
        public bool IsOutside(Point2d pnt)
        {
            foreach (var node in _ptlst.GetNodes())
            {
                if (IsClockWise(node.Value, node.Next.Value, pnt))
                    return true;
            }
            return false;
        }

        /// <summary>
        /// 判断点是否在凸包的内部
        /// </summary>
        /// <param name="pnt">点</param>
        /// <returns>在内部返回true，反之返回false</returns>
        public bool IsInside(Point2d pnt)
        {
            foreach (var node in _ptlst.GetNodes())
            {
                if (IsClockWise(node.Value, pnt, node.Next.Value))
                    return true;
            }
            return false;
        }

        /// <summary>
        /// 判断点是否在凸包上
        /// </summary>
        /// <param name="pnt">点</param>
        /// <returns>在凸包的边上，返回true，反之返回false</returns>
        public bool IsOn(Point2d pnt)
        {
            foreach (var node in _ptlst.GetNodes())
            {
                using (var ls2d = new LineSegment2d(node.Value, node.Next.Value))
                {
                    if (ls2d.IsOn(pnt))
                        return true;
                }
            }
            return false;
        }

        //public double GetMaxDistance(out LoopList<Point2d> ptlst)
        //{
        //}

        private static double GetArea(Point2d ptBase, Point2d pt1, Point2d pt2)
        {
            return (pt2 - ptBase).DotProduct((pt1 - ptBase).GetPerpendicularVector());
        }

        private static bool IsClockWise(Point2d ptBase, Point2d pt1, Point2d pt2)
        {
            return GetArea(ptBase, pt1, pt2) <= 0;
        }

        private static double GetArea(Vector2d vecBase, Vector2d vec)
        {
            return vec.DotProduct(vecBase.GetPerpendicularVector());
        }

        private static bool IsClockWise(Vector2d vecBase, Vector2d vec)
        {
            return GetArea(vecBase, vec) <= 0;
        }

        #region IEnumerable<Point2d> 成员

        IEnumerator<Point2d> IEnumerable<Point2d>.GetEnumerator()
        {
            return _ptlst.GetEnumerator();
        }

        #region IEnumerable 成员

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return _ptlst.GetEnumerator();
        }

        #endregion IEnumerable 成员

        #endregion IEnumerable<Point2d> 成员
    }
}
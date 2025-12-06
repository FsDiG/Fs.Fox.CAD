using System;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.DatabaseServices;

namespace ThCADExtension
{
    public static class ThArcExtension
    {
        // 合并两段共圆心同半径的Arc
        public static Arc ArcMerge(this Arc arc1, Arc arc2)
        {
            // 变量初始化；
            var startAngle = new double();
            var endAngle = new double();

            var flag_1 = false;
            var flag_2 = false;
            var arc1_1 = new Arc(new Point3d(0.0, 0.0, 0.0), new double(), new double(), new double());
            var arc1_2 = new Arc(new Point3d(0.0, 0.0, 0.0), new double(), new double(), new double());
            var arc2_1 = new Arc(new Point3d(0.0, 0.0, 0.0), new double(), new double(), new double());
            var arc2_2 = new Arc(new Point3d(0.0, 0.0, 0.0), new double(), new double(), new double());

            if (arc1.StartAngle > arc1.EndAngle)
            {
                arc1_1 = new Arc(arc1.Center, arc1.Radius, 0.0, arc1.EndAngle);
                arc1_2 = new Arc(arc1.Center, arc1.Radius, arc1.StartAngle, 8 * Math.Atan(1));
                flag_1 = true;
            }
            if (arc2.StartAngle > arc2.EndAngle)
            {
                arc2_1 = new Arc(arc2.Center, arc2.Radius, 0.0, arc2.EndAngle);
                arc2_2 = new Arc(arc2.Center, arc2.Radius, arc2.StartAngle, 8 * Math.Atan(1));
                flag_2 = true;
            }
            // 两段弧均截断
            if (flag_1 && flag_2)
            {
                startAngle = Math.Min(arc1_2.StartAngle, arc2_2.StartAngle);
                endAngle = Math.Max(arc1_1.EndAngle, arc2_1.EndAngle);
            }
            // 仅arc1截断
            else if (flag_1 && !(flag_2))
            {
                if (arc2.StartAngle <= arc1_1.EndAngle)
                {
                    if (arc2.EndAngle <= arc1_2.StartAngle)
                    {
                        startAngle = arc1_2.StartAngle;
                        endAngle = Math.Max(arc1_1.EndAngle, arc2.EndAngle);
                    }
                    // arc1与arc2存在两段间断的重叠范围（如：c与镜像c）
                    else
                    {
                        startAngle = 0.0;
                        endAngle = 8 * Math.Atan(1);
                    }
                }
                else
                {
                    startAngle = Math.Min(arc1_2.StartAngle, arc2.StartAngle);
                    endAngle = arc1_1.EndAngle;
                }
            }
            // 仅arc2截断
            else if (!(flag_1) && flag_2)
            {
                if (arc1.StartAngle <= arc2_1.EndAngle)
                {
                    if (arc1.EndAngle <= arc2_2.StartAngle)
                    {
                        startAngle = arc2_2.StartAngle;
                        endAngle = Math.Max(arc2_1.EndAngle, arc1.EndAngle);
                    }
                    // arc1与arc2存在两段不相连的重叠区域（如：c与镜像c）
                    else
                    {
                        startAngle = 0.0;
                        endAngle = 8 * Math.Atan(1);
                    }
                }
                else
                {
                    startAngle = Math.Min(arc2_2.StartAngle, arc1.StartAngle);
                    endAngle = arc2_1.EndAngle;
                }
            }
            // 两段弧均不截断
            else
            {
                startAngle = Math.Min(arc1.StartAngle, arc2.StartAngle);
                endAngle = Math.Max(arc1.EndAngle, arc2.EndAngle);
            }
            return new Arc(arc1.Center, arc1.Radius, startAngle, endAngle);
        }

        // 计算两段弧的重合角度
        public static Tuple<bool, double, double, double> OverlapAngle(this Arc arc1, Arc arc2)
        {
            // 变量初始化；
            var startAngle = new double();
            var endAngle = new double();
            var AngleRange = new double();
            var isOverlap = false;

            var flag_1 = false;
            var flag_2 = false;
            var arc1_1 = new Arc(new Point3d(0.0, 0.0, 0.0), new double(), new double(), new double());
            var arc1_2 = new Arc(new Point3d(0.0, 0.0, 0.0), new double(), new double(), new double());
            var arc2_1 = new Arc(new Point3d(0.0, 0.0, 0.0), new double(), new double(), new double());
            var arc2_2 = new Arc(new Point3d(0.0, 0.0, 0.0), new double(), new double(), new double());

            if (arc1.StartAngle > arc1.EndAngle)
            {
                arc1_1 = new Arc(arc1.Center, arc1.Radius, 0.0, arc1.EndAngle);
                arc1_2 = new Arc(arc1.Center, arc1.Radius, arc1.StartAngle, 8 * Math.Atan(1));
                flag_1 = true;
            }
            if (arc2.StartAngle > arc2.EndAngle)
            {
                arc2_1 = new Arc(arc2.Center, arc2.Radius, 0.0, arc2.EndAngle);
                arc2_2 = new Arc(arc2.Center, arc2.Radius, arc2.StartAngle, 8 * Math.Atan(1));
                flag_2 = true;
            }
            // 两段弧均截断
            if (flag_1 && flag_2)
            {
                isOverlap = true;
                startAngle = Math.Max(arc1_2.StartAngle, arc2_2.StartAngle);
                endAngle = Math.Min(arc1_1.EndAngle, arc2_1.EndAngle);
                AngleRange = endAngle + 8 * Math.Atan(1) - startAngle;
            }
            // 仅arc1截断
            else if (flag_1 && !(flag_2))
            {
                isOverlap = !(arc2.StartAngle > arc1_1.EndAngle && arc2.EndAngle < arc1_2.StartAngle);
                if (isOverlap)
                {
                    if (arc2.StartAngle <= arc1_1.EndAngle)
                    {
                        if (arc2.EndAngle <= arc1_2.StartAngle)
                        {
                            startAngle = arc2.StartAngle;
                            endAngle = Math.Min(arc1_1.EndAngle, arc2.EndAngle);
                            AngleRange = endAngle - startAngle;
                        }
                        // arc1与arc2存在两段间断的重叠范围（如：c与镜像c）
                        else
                        {
                            throw new NotSupportedException();
                        }
                    }
                    else
                    {
                        startAngle = Math.Max(arc1_2.StartAngle, arc2.StartAngle);
                        endAngle = arc2.EndAngle;
                        AngleRange = endAngle - startAngle;
                    }
                }

            }
            // 仅arc2截断
            else if (!(flag_1) && flag_2)
            {
                isOverlap = !(arc1.StartAngle > arc2_1.EndAngle && arc1.EndAngle < arc2_2.StartAngle);
                if (isOverlap)
                {
                    if (arc1.StartAngle <= arc2_1.EndAngle)
                    {
                        if (arc1.EndAngle <= arc2_2.StartAngle)
                        {
                            startAngle = arc1.StartAngle;
                            endAngle = Math.Min(arc2_1.EndAngle, arc1.EndAngle);
                            AngleRange = endAngle - startAngle;
                        }
                        // arc1与arc2存在两段不相连的重叠区域（如：c与镜像c）
                        else
                        {
                            throw new NotSupportedException();
                        }
                    }
                    else
                    {
                        startAngle = Math.Max(arc2_2.StartAngle, arc1.StartAngle);
                        endAngle = arc1.EndAngle;
                        AngleRange = endAngle - startAngle;
                    }
                }
            }
            // 两段弧均不截断
            else
            {
                isOverlap = !(arc1.StartAngle > arc2.EndAngle || arc2.StartAngle > arc1.EndAngle);
                if (isOverlap)
                {
                    startAngle = Math.Max(arc1.StartAngle, arc2.StartAngle);
                    endAngle = Math.Min(arc1.EndAngle, arc2.EndAngle);
                    AngleRange = endAngle - startAngle;
                }
            }
            return Tuple.Create(isOverlap, startAngle, endAngle, AngleRange);
        }
    }
}

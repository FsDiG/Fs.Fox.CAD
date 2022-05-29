using System.Diagnostics;

namespace IFoxCAD.Cad;


/// <summary>
/// ������Ϣ
/// </summary>
public class CurveInfo : Rect
{
    #region ����
    /// <summary>
    /// ����ͼԪ
    /// </summary>
    public Curve Curve;
    /// <summary>
    /// ���߷ָ��Ĳ���
    /// </summary>
    public List<double> Paramss;
    /// <summary>
    /// ��ײ��
    /// </summary>
    public CollisionChain? CollisionChain;

    Edge? _Edge;
    /// <summary>
    /// �߽�
    /// </summary>
    public Edge Edge
    {
        get
        {
            if (_Edge == null)
                _Edge = new Edge(Curve.ToCompositeCurve3d()!);
            return _Edge;
        }
    }
    #endregion

    #region ����
    public CurveInfo(Curve curve!!)
    {
        Curve = curve;
        Paramss = new List<double>();

        //TODO �˴�����bug:����֮���û�й���
        var box = Curve.GeometricExtents;
        _X = box.MinPoint.X;
        _Y = box.MinPoint.Y;
        _Right = box.MaxPoint.X;
        _Top = box.MaxPoint.Y;
    }
    #endregion

    #region ����
    /// <summary>
    /// �ָ�����,��������
    /// </summary>
    /// <param name="pars1">���߷ָ��Ĳ���</param>
    /// <returns></returns>
    public List<Edge> Split(List<double> pars1)
    {
        var edges = new List<Edge>();
        var c3ds = Edge.GeCurve3d.GetSplitCurves(pars1);
        if (c3ds.Count > 0)
            edges.AddRange(c3ds.Select(c => new Edge(c)));
        return edges;
    }
    #endregion
}

/// <summary>
/// ��ײ��
/// </summary>
public class CollisionChain : List<CurveInfo> { }



/// <summary>
/// ���߱�
/// </summary>
[DebuggerDisplay("{DebuggerDisplay,nq}")]
[DebuggerTypeProxy(typeof(Edge))]
public class Edge : IEquatable<Edge>, IFormattable
{
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private string DebuggerDisplay => ToString();

    #region �ֶ�
    public CompositeCurve3d GeCurve3d;
    #endregion

    #region ����
    /// <summary>
    /// ����(û�а�Χ��,����ToCurve)
    /// </summary>
    public Edge(CompositeCurve3d geCurve3d)
    {
        GeCurve3d = geCurve3d;
    }
    #endregion

    #region ���������_�Ƚ�
    public override bool Equals(object? obj)
    {
        return this == obj as Edge;
    }
    public bool Equals(Edge? b)
    {
        return this == b;
    }
    public static bool operator !=(Edge? a, Edge? b)
    {
        return !(a == b);
    }
    public static bool operator ==(Edge? a, Edge? b)
    {
        //�˴��ط�������ʹ��==null,��Ϊ�˴��Ƕ���
        if (b is null)
            return a is null;
        else if (a is null)
            return false;
        if (ReferenceEquals(a, b))//ͬһ����
            return true;

        return a.GeCurve3d == b.GeCurve3d;
    }

    public override int GetHashCode()
    {
        return GeCurve3d.GetHashCode();
    }

    /// <summary>
    /// ������Ƚ�(������,����˳������)
    /// </summary>
    /// <param name="b"></param>
    /// <param name="splitNum">
    /// �и����߷���(����3����������Բ���ص�����ʧЧ,��˴�4��ʼ)
    ///  <a href="..\..\..\docs\Topo����˵��\���߲�������˵��.png">ͼ��˵���ڽ�������docs��</a>
    /// </param>
    /// <returns>�������ص�true</returns>
    public bool SplitPointEquals(Edge? b, int splitNum = 4)
    {
        if (b is null)
            return this is null;
        else if (this is null)
            return false;
        if (ReferenceEquals(this, b))//ͬһ����
            return true;

        //�����ȡ���߳��Ȼᾭ���ܶ��ƽ����,
        //���Բ�Ҫ������,ֱ�Ӳ�����֮���жϾ�����

        //���߲����ָ��Ҳһ��,����һ��
        Point3d[] sp1;
        Point3d[] sp2;

#if NET35
        sp1 = GeCurve3d.GetSamplePoints(splitNum);
        sp2 = b.GeCurve3d.GetSamplePoints(splitNum);
#else
        var tmp1 = GeCurve3d.GetSamplePoints(splitNum);
        var tmp2 = b.GeCurve3d.GetSamplePoints(splitNum);
        sp1 = tmp1.Select(a => a.Point).ToArray();
        sp2 = tmp2.Select(a => a.Point).ToArray();
#endif
        //��Ϊ�������߿����������,���Բ���֮����Ҫ������
        sp1 = sp1.OrderBy(a => a.X).ThenBy(a => a.Y).ThenBy(a => a.Z).ToArray();
        sp2 = sp2.OrderBy(a => a.X).ThenBy(a => a.Y).ThenBy(a => a.Z).ToArray();

        for (int i = 0; i < sp1.Length; i++)
        {
            if (!sp1[i].IsEqualTo(sp2[i], CadTolerance))
                return false;
        }
        return true;
    }
    #endregion

    #region ����
#pragma warning disable CA2211 // �ǳ����ֶ�Ӧ�����ɼ�
    public static Tolerance CadTolerance = new(1e-6, 1e-6);
#pragma warning restore CA2211 // �ǳ����ֶ�Ӧ�����ɼ�

    /// <summary>
    /// ����˳������ 
    /// </summary>
    /// <param name="edgesOut"></param>
    public static void Distinct(List<Edge> edgesOut)
    {
        if (edgesOut.Count == 0)
            return;

        //Edgeû�а�Χ��,�޷������ж�
        //����a����������n���ݽ����и�����,������ظ�����,��������������ͬ
        Basal.ArrayEx.Deduplication(edgesOut, (first, last) => {
            var pta1 = first.GeCurve3d.StartPoint;
            var pta2 = first.GeCurve3d.EndPoint;
            var ptb1 = last.GeCurve3d.StartPoint;
            var ptb2 = last.GeCurve3d.EndPoint;
            //˳�� || ����
            if ((pta1.IsEqualTo(ptb1, CadTolerance) && pta2.IsEqualTo(ptb2, CadTolerance))
                ||
                (pta1.IsEqualTo(ptb2, CadTolerance) && pta2.IsEqualTo(ptb1, CadTolerance)))
            {
                return first.SplitPointEquals(last);
            }
            return false;
        });
    }
    #endregion

    #region IFormattable
    public override string ToString()
    {
        return ToString(null, null);
    }

    /// <summary>
    /// ת��Ϊ�ַ���_��ʽ��ʵ��
    /// </summary>
    /// <param name="format"></param>
    /// <param name="formatProvider"></param>
    /// <returns></returns>
    string IFormattable.ToString(string? format, IFormatProvider? formatProvider)
    {
        return ToString(format, formatProvider);
    }

    /// <summary>
    /// ת��Ϊ�ַ���_�вε���
    /// </summary>
    /// <returns></returns>
    public string ToString(string? format, IFormatProvider? formatProvider = null)
    {
        var s = new StringBuilder();
        if (format is null)
        {
            s.Append(nameof(Edge));
            s.Append("{ ");
            s.Append("(StartPoint=" + GeCurve3d.StartPoint + "; EndPoint=" + GeCurve3d.EndPoint + ")");
            s.Append(" }\r\n");
        }
        return s.ToString();
    }
    #endregion
}

#if true2
/// <summary>
/// ��������(���պ�/�޷ֲ�/���Խ�)
/// </summary>
public class PolyEdge : LoopList<Edge>
{
    /// <summary>
    /// ��������(���պ�/�޷ֲ�/���Խ�)
    /// </summary>
    /// <param name="ps"></param>
    public PolyEdge(params Edge[] ps)
    {
        AddRange(ps);
    }

    // ��̬���������ڸ�Ƶ����㼯��ʱ�����ڴ泤��
    static List<Point3d> pts = new();

    public Point3d[] Points()
    {
        pts.Clear();

        //��Ϊ�����ʱ���Ӷ��������,���Ե���Ҳ�������
        var ge = GetEnumerator();
        while (ge.MoveNext())
        {
            var sp = ge.Current.GeCurve3d.StartPoint;
            if (pts.Count == 0)
                pts.Add(sp);
            else if (pts[pts.Count - 1] != sp)//�����Ӷ��غϵ�
                pts.Add(sp);

            var ep = ge.Current.GeCurve3d.EndPoint;
            if (pts.Count == 0)
                pts.Add(ep);
            else if (pts[pts.Count - 1] != ep)
                pts.Add(ep);
        }
        return pts.ToArray();
    }

    /// <summary>
    /// �����Ӷη��ض����
    /// </summary>
    /// <param name="lst">����߼���(������,�ṩ����ȱ���)</param>
    /// <param name="find">���ҵ��Ӷ�</param>
    /// <returns>���ض����</returns>
    public static PolyEdge? Contains(IEnumerable<PolyEdge> lst, Edge find)
    {
        //��̫��������Ч��
        //Parallel.ForEach(this, item => {
        //    Parallel.ForEach(item, item2 => {
        //        if (item2 == find)
        //            return;
        //    });
        //});

        //����߼���=>�����=>�Ӷ�
        var ge1 = lst.GetEnumerator();
        while (ge1.MoveNext())
        {
            var ge2 = ge1.Current.GetEnumerator();
            while (ge2.MoveNext())
            {
                //�Ӷ���ͬ:���ض����,���ڼ���
                if (ge2.Current == find)
                    return ge1.Current;
            }
        }
        return null;
    }
}

#endif



/// <summary>
/// �ڽӱ�ڵ�
/// </summary>
//[DebuggerDisplay("�ڵ� = {Number}; Count = {Count}; Color = {Color}; Distance = {Distance}; ���ڵ��� = {Parent?.Number}")]
[DebuggerDisplay("{DebuggerDisplay,nq}")]
[DebuggerTypeProxy(typeof(BoNode))]
public class BoNode : IFormattable
{
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private string DebuggerDisplay => ToString();

    #region �ֶ�
    /// <summary>
    /// ��ɫ
    /// </summary>
    public BoColor Color;
    /// <summary>
    /// ��ͷ���˲���
    /// </summary>
    public int Steps;
    /// <summary>
    /// ���ڵ�
    /// </summary>
    public BoNode? Parent;
    /// <summary>
    /// �����㼯��
    /// </summary>
    public List<BoNode>? Meet;
    /// <summary>
    /// �ڽ��ڵ�
    /// </summary>
    public List<BoNode> Neighbor;


    /// <summary>
    /// ����
    /// </summary>
    public Point3d Point;
    /// <summary>
    /// ���������(����������Ǳ���)
    /// </summary>
    public List<Edge> Edges;
    /// <summary>
    /// �ڵ���
    /// </summary>
    public int Number;
    #endregion

    #region ����
    /// <summary>
    /// �ڽӱ�ڵ�
    /// </summary>
    /// <param name="num">�ڵ���</param>
    /// <param name="point">����</param>
    /// <param name="edge">��</param>
    public BoNode(int num, Point3d point, Edge edge)
    {
        Number = num;
        Point = point;
        Edges = new();
        Edges.Add(edge);

        Neighbor = new();


        Color = BoColor.��;
        Steps = int.MaxValue;
        Parent = null;
        Meet?.Clear();
    }
    #endregion

    #region ����
    /// <summary>
    /// ��ȡ��ձ��߼���,����������
    /// </summary>
    /// <param name="nodes"></param>
    /// <returns>��������,����δ����</returns>
    /// <exception cref="ArgumentNullException"></exception>
    public static LoopList<Edge> GetEdges(LoopList<BoNode> nodes)
    {
        if (nodes == null || nodes.Count == 0)
            throw new ArgumentNullException(nameof(nodes));

        LoopList<Edge> result = new();
        //1-2 2-3 3-4 4-1
        var lp = nodes.First;
        do
        {
            var edge = GetEdge(lp!.Value, lp.Next!.Value);
            if (edge != null)
                result.Add(edge);
            lp = lp.Next;
        } while (lp != nodes.First);

        return result;
    }

    /// <summary>
    /// ��ȡa-b�ڵ�֮�����
    /// </summary>
    /// <param name="node1"></param>
    /// <param name="node2"></param>
    /// <returns></returns>
    static Edge? GetEdge(BoNode node1, BoNode node2)
    {
        if (node1 == null || node2 == null)
            return null;

        for (int i = 0; i < node1.Edges.Count; i++)
        {
            var node1Edge = node1.Edges[i];
            for (int j = 0; j < node2.Edges.Count; j++)
            {
                if (node1Edge == node2.Edges[j])
                    return node1Edge;
            }
        }
        return null;
    }
    #endregion

    #region IFormattable
    public override string ToString()
    {
        return ToString(null, null);
    }

    /// <summary>
    /// ת��Ϊ�ַ���_��ʽ��ʵ��
    /// </summary>
    /// <param name="format"></param>
    /// <param name="formatProvider"></param>
    /// <returns></returns>
    string IFormattable.ToString(string? format, IFormatProvider? formatProvider)
    {
        return ToString(format, formatProvider);
    }

    /// <summary>
    /// ת��Ϊ�ַ���_�вε���
    /// </summary>
    /// <returns></returns>
    public string ToString(string? format, IFormatProvider? formatProvider = null)
    {
        var s = new StringBuilder();
        //s.Append($"Count = {Count};");
        if (format is null)
        {
            s.Append(nameof(BoNode) + $"{Number}");
            s.Append("{ ");

            s.Append("�ڵ�:(");
            for (int i = 0; i < Neighbor.Count; i++)
            {
                s.Append(this.Neighbor[i].Number);
                if (i < Neighbor.Count - 1)
                    s.Append("--");
            }
            s.Append(") ");

            s.Append($"Neighbor.Count={Neighbor.Count}; Color={Color}; Distance={Steps}; ������={Parent?.Number}; ");
            s.Append($"Point={Point};");
            s.Append(" }\r\n");
        }
        return s.ToString();
    }
    #endregion

}

public enum BoColor
{
    ��,
    ��,
    ��,
    ��,
}

[DebuggerDisplay("{DebuggerDisplay,nq}")]
[DebuggerTypeProxy(typeof(CompositeCurve3ds))]
public class CompositeCurve3ds : List<CompositeCurve3d>, IFormattable
{
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private string DebuggerDisplay => ToString();

#pragma warning disable CA2211 // �ǳ����ֶ�Ӧ�����ɼ�
    // cad�ݲ���
    public static Tolerance CadTolerance = new(1e-6, 1e-6);
#pragma warning restore CA2211 // �ǳ����ֶ�Ӧ�����ɼ�

    /// <summary>
    /// �߽��������
    /// </summary>
    /// <param name="pl">��Ҫ�������кõ�</param>
    /// <returns></returns>
    public static CompositeCurve3ds OrderByPoints(LoopList<Edge> pl)
    {
        CompositeCurve3ds c3ds = new();
        var lp = pl.First;
        do
        {
            //��1���͵�2���Ƚ�,�������߲���
            var a1 = lp!.Value.GeCurve3d;
            var a2 = lp!.Next!.Value.GeCurve3d;

            if (!a1.EndPoint.IsEqualTo(a2.EndPoint, CadTolerance) && //β����ͬ����
                !a1.EndPoint.IsEqualTo(a2.StartPoint, CadTolerance))
                a1 = (CompositeCurve3d)a1.GetReverseParameterCurve();

            c3ds.Add(a1);
            lp = lp.Next;
        } while (lp != pl.First);

        return c3ds;
    }


    public CompositeCurve3d ToCompositeCurve3d()
    {
        return new CompositeCurve3d(ToArray());
    }

    #region IFormattable
    public override string ToString()
    {
        return ToString(null, null);
    }

    /// <summary>
    /// ת��Ϊ�ַ���_��ʽ��ʵ��
    /// </summary>
    /// <param name="format"></param>
    /// <param name="formatProvider"></param>
    /// <returns></returns>
    string IFormattable.ToString(string? format, IFormatProvider? formatProvider)
    {
        return ToString(format, formatProvider);
    }

    /// <summary>
    /// ת��Ϊ�ַ���_�вε���
    /// </summary>
    /// <returns></returns>
    public string ToString(string? format, IFormatProvider? formatProvider = null)
    {
        var s = new StringBuilder();
        if (format is null)
        {
            s.Append(nameof(CompositeCurve3ds) + $"{Count}");
            s.Append("{ ");
            for (int i = 0; i < Count; i++)
            {
                s.Append("\r\n(StartPoint=" + this[i].StartPoint + "; EndPoint=" + this[i].EndPoint + ")");
            }
            s.Append(" }\r\n");
        }
        return s.ToString();
    }

    #endregion
}
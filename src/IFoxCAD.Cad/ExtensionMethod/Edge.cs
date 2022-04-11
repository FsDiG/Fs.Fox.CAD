namespace IFoxCAD.Cad;

/// <summary>
/// ��
/// </summary>
public class Edge : IEquatable<Edge>
{
    #region �ֶ�
    public CompositeCurve3d GeCurve3d;
    public int StartIndex;//DESIGN0409 �������ֻ�������ڽӱ���˾�û����.
    public int EndIndex;  //DESIGN0409 �������ֻ�������ڽӱ���˾�û����.
    public static Tolerance CadTolerance = new(1e-6, 1e-6);
    #endregion

    #region ����
    /// <summary>
    /// ����(û�а�Χ��,����ToCurve)
    /// </summary>
    public Edge(CompositeCurve3d geCurve3d)
    {
        GeCurve3d = geCurve3d;
    }
    public Edge(Edge edge) : this(edge.GeCurve3d)
    {
        StartIndex = edge.StartIndex;
        EndIndex = edge.EndIndex;
    }
    #endregion

    #region ����
    /// <summary>
    /// ��ʼ����
    /// </summary>
    /// <returns></returns>
    public Vector3d GetStartVector()
    {
        //��ȡ���߲���
        var inter = GeCurve3d.GetInterval();
        var poc = new PointOnCurve3d(GeCurve3d, inter.LowerBound);
        //������
        return poc.GetDerivative(1);
    }

    /// <summary>
    /// ��������
    /// </summary>
    /// <returns></returns>
    public Vector3d GetEndVector()
    {
        var inter = GeCurve3d.GetInterval();
        var poc = new PointOnCurve3d(GeCurve3d, inter.UpperBound);
        return -poc.GetDerivative(1);
    }

    /// <summary>
    /// �жϽڵ�λ��
    /// </summary>
    /// <param name="edge">�߽�</param>
    /// <param name="startOrEndIndex">�߽��Ƿ�λ�ڴ˴�</param>
    /// <param name="vec">��������</param>
    /// <param name="forward">��������</param>
    /// <returns></returns>
    public bool IsNext(Edge edge, int startOrEndIndex, ref Vector3d vec, ref bool forward)
    {
        if (edge == this)
            return false;

        if (StartIndex == startOrEndIndex)
        {
            vec = GetStartVector();
            forward = true;
            return true;
        }
        else if (EndIndex == startOrEndIndex)
        {
            vec = GetEndVector();
            forward = false;
            return true;
        }
        return false;
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

        return a.GeCurve3d == b.GeCurve3d
            && a.StartIndex == b.StartIndex
            && a.EndIndex == b.EndIndex;
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
    public override int GetHashCode()
    {
        return GeCurve3d.GetHashCode() ^ StartIndex ^ EndIndex;
    }
    #endregion
}

/// <summary>
/// �߽ڵ�
/// </summary>
public class EdgeItem : Edge, IEquatable<EdgeItem>
{
    #region �ֶ�
    /// <summary>
    /// <param name="forward">���������־,<see langword="true"/>Ϊ��ǰ����,<see langword="false"/>Ϊ�������</param>
    /// </summary>
    public bool Forward;
    #endregion

    #region ����
    /// <summary>
    /// �߽ڵ�
    /// </summary>
    public EdgeItem(Edge edge, bool forward) : base(edge)
    {
        Forward = forward;
    }
    #endregion

    #region ����
    public CompositeCurve3d? GetCurve()
    {
        var cc3d = GeCurve3d;
        if (Forward)
            return cc3d;

        //�������߲���
        cc3d = cc3d.Clone() as CompositeCurve3d;
        return cc3d?.GetReverseParameterCurve() as CompositeCurve3d;
    }

    /// <summary>
    /// ��������
    /// </summary>
    /// <param name="edges">������ѯλ�õ�</param>
    /// <param name="regions_out">���ص�����</param>
    public void FindRegion(List<Edge> edges, List<LoopList<EdgeItem>> regions_out)
    {
        var result = new LoopList<EdgeItem>();//�µ�����
        var edgeItem = this;
        result.Add(edgeItem);
        var getEdgeItem = this.GetNext(edges);
        if (getEdgeItem is null)
            return;

        bool hasList = false;

        for (int i = 0; i < regions_out.Count; i++)
        {
            var edgeList2 = regions_out[i];
            var node = edgeList2.GetNode(e => e.Equals(edgeItem));
            if (node is not null && node != edgeList2.Last)
            {
                if (node.Next!.Value.Equals(getEdgeItem))
                {
                    hasList = true;
                    break;
                }
            }
        }
        if (!hasList)
        {
            //֮ǰ�����и���ѭ��,��ɵ�ԭ����8����b��(����ջ�),Ȼ������ջ���Զ�Ҳ�����ͷ
            while (getEdgeItem is not null)
            {
                if (result.Contains(getEdgeItem))
                {
                    //����ջ�,��ǰͷ��ʼ�޳�������ظ��ڵ�
                    while (result.First?.Value != getEdgeItem)
                        result.RemoveFirst();
                    break;
                }
                result.Add(getEdgeItem);
                getEdgeItem = getEdgeItem.GetNext(edges);
            }
            if (getEdgeItem == edgeItem)
                regions_out.Add(result);
        }
    }

    /// <summary>
    /// ��ȡ��һ��
    /// </summary>
    /// <param name="edges"></param>
    /// <returns></returns>
    public EdgeItem? GetNext(List<Edge> edges)
    {
        Vector3d vec;
        int next;
        if (Forward)
        {
            vec = GetEndVector();
            next = EndIndex;
        }
        else
        {
            vec = GetStartVector();
            next = StartIndex;
        }

        EdgeItem? edgeItem = null;
        Vector3d vec2, vec3 = new();
        double angle = 0;
        bool hasNext = false;
        bool forward = false;
        for (int i = 0; i < edges.Count; i++)
        {
            var edge = edges[i];
            if (this.IsNext(edge, next, ref vec3, ref forward))
            {
                if (hasNext)
                {
                    var angle2 = vec.GetAngleTo(vec3, Vector3d.ZAxis);
                    if (angle2 < angle)
                    {
                        vec2 = vec3;
                        angle = angle2;
                        edgeItem = new EdgeItem(edge, forward);
                    }
                }
                else
                {
                    hasNext = true;
                    vec2 = vec3;
                    angle = vec.GetAngleTo(vec2, Vector3d.ZAxis);
                    edgeItem = new EdgeItem(edge, forward);
                }
            }
        }
        return edgeItem;
    }
    #endregion

    #region ����ת��
    public override string ToString()
    {
        return Forward ?
               $"{StartIndex}-{EndIndex}" :
               $"{EndIndex}-{StartIndex}";
    }
    #endregion

    #region ���������_�Ƚ�
    public override bool Equals(object? obj)
    {
        return this == obj as EdgeItem;
    }
    public bool Equals(EdgeItem? b)
    {
        return this == b;
    }
    public static bool operator !=(EdgeItem? a, EdgeItem? b)
    {
        return !(a == b);
    }
    public static bool operator ==(EdgeItem? a, EdgeItem? b)
    {
        //�˴��ط�������ʹ��==null,��Ϊ�˴��Ƕ���
        if (b is null)
            return a is null;
        else if (a is null)
            return false;
        if (ReferenceEquals(a, b))//ͬһ����
            return true;

        return a.Forward == b.Forward && (a == b as Edge);
    }
    public override int GetHashCode()
    {
        return base.GetHashCode() ^ (Forward ? 0 : 1);
    }
    #endregion
}


/// <summary>
/// ������Ϣ
/// </summary>
public class CurveInfo : Rect
{
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

    public CurveInfo(Curve curve)
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
}

/// <summary>
/// ��ײ��
/// </summary>
public class CollisionChain : List<CurveInfo>
{
}


/// <summary>
/// ��������(���պ�/�޷ֲ�/���Խ�)
/// </summary>
public class PolyEdge : LoopList<Edge>
{
    public PolyEdge(params Edge[] ps)
    {
        for (int i = 0; i < ps.Length; i++)
            Add(ps[i]);
    }

    public PolyEdge(Knot kn)
    {
        AddRange(kn);
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


/// <summary>
/// һ�����㴢���֧��
/// </summary>
public class Knot : PolyEdge//����ı߼���(����������Ǳ���)
{
    public Point3d Point; //����

    public Knot(Point3d point, Edge edge)
    {
        Point = point;
        Add(edge);
    }

    /// <summary>
    /// ���оͷ��س�Ա
    /// </summary>
    /// <param name="knots">�ڵ㼯��</param>
    /// <param name="findPoint">���ҵ�</param>
    /// <returns></returns>
    public static Knot? Contains(IEnumerable<Knot> knots, Point3d findPoint)
    {
        var ge = knots.GetEnumerator();
        while (ge.MoveNext())
            if (ge.Current.Point == findPoint)
                return ge.Current;
        return null;
    }
}
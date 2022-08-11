namespace IFoxCAD.Cad;

/*
 *  ��ӵĵ�һ���߽��������߽�,�����ڶ���ͼ�����������ı߽硣
 *  Ҫ����ⲿ�߽�,��ʹ����ӻ�������Ϊ HatchLoopTypes.Outermost ������ AppendLoop ����,
 *  һ����߽类����,�Ϳ��Լ����������ı߽硣
 *  ����ڲ��߽���ʹ�ô� HatchLoopTypes.Default ������ AppendLoop ������
 *
 *  �����߽��ʱ��,��ӵ���(��߽�,��߽�,��߽�,��ͨ�߽�....)
 *  �����߽��ʱ��,��ӵ���(��߽�,��ͨ�߽�.....��߽�,��ͨ�߽�....)
 */

/// <summary>
/// ͼ�����
/// </summary>
public class HatchInfo
{
    #region ��Ա
    /// <summary>
    /// �߽�id(������ŵ�һ)
    /// </summary>
    readonly List<ObjectId> _boundaryIds;
    /// <summary>
    /// ���ͼԪ
    /// </summary>
    readonly Hatch _hatch;
    /// <summary>
    /// �߽����(�˴�����ֱ��=>������Ա,��Ϊ������뷴Ӧ��)
    /// </summary>
    readonly bool _boundaryAssociative;
    /// <summary>
    /// ��������:�û�����(�̶�����)/����/������ݶ����ļ�
    /// </summary>
    string? _hatchName;
    /// <summary>
    /// ���ģʽ����(Ԥ����/�û�����/�Զ���)
    /// </summary>
    HatchPatternType _patternTypeHatch;
    /// <summary>
    /// ����ģʽ����
    /// </summary>
    GradientPatternType _patternTypeGradient;
    /// <summary>
    /// ����/���
    /// </summary>
    double Scale => _hatch.PatternScale;
    /// <summary>
    /// �Ƕ�
    /// </summary>
    double Angle => _hatch.PatternAngle;
    #endregion

    #region ����
    HatchInfo()
    {
        _hatch = new Hatch();
        _hatch.SetDatabaseDefaults();
        _boundaryIds = new();
    }

    /// <summary>
    /// ͼ�����
    /// </summary>
    /// <param name="boundaryAssociative">�����߽�</param>
    /// <param name="hatchOrigin">���ԭ��</param>
    /// <param name="hatchScale">����</param>
    /// <param name="hatchAngle">�Ƕ�</param>
    public HatchInfo(bool boundaryAssociative = true,
                     Point2d? hatchOrigin = null,
                     double hatchScale = 1,
                     double hatchAngle = 0) : this()
    {
        if (hatchScale <= 0)
            throw new ArgumentException("������������С�ڵ���0");

        _hatch.PatternScale = hatchScale;//������
        _hatch.PatternAngle = hatchAngle;//���Ƕ�
        _boundaryAssociative = boundaryAssociative;

        hatchOrigin ??= Point2d.Origin;
        _hatch.Origin = hatchOrigin.Value; //���ԭ��
    }

    /// <summary>
    /// ͼ�����
    /// </summary>
    /// <param name="boundaryIds">�߽�</param>
    /// <param name="boundaryAssociative">�����߽�</param>
    /// <param name="hatchOrigin">���ԭ��</param>
    /// <param name="hatchScale">����</param>
    /// <param name="hatchAngle">�Ƕ�</param>
    public HatchInfo(IEnumerable<ObjectId> boundaryIds,
                     bool boundaryAssociative = true,
                     Point2d? hatchOrigin = null,
                     double hatchScale = 1,
                     double hatchAngle = 0)
        : this(boundaryAssociative, hatchOrigin, hatchScale, hatchAngle)
    {
        _boundaryIds.AddRange(boundaryIds);
    }

    #endregion

    #region ����
    /// <summary>
    /// ģʽ1:Ԥ����
    /// </summary>
    public HatchInfo Mode1PreDefined(string name)
    {
        _hatchName = name;
        _hatch.HatchObjectType = HatchObjectType.HatchObject; //��������(���/����)
        _patternTypeHatch = HatchPatternType.PreDefined;
        return this;
    }

    /// <summary>
    /// ģʽ2:�û�����
    /// </summary>
    /// <param name="patternDouble">�Ƿ�˫��</param>
    public HatchInfo Mode2UserDefined(bool patternDouble = true)
    {
        _hatchName = "_USER";
        _hatch.HatchObjectType = HatchObjectType.HatchObject; //��������(���/����)
        _patternTypeHatch = HatchPatternType.UserDefined;

        _hatch.PatternDouble = patternDouble; //�Ƿ�˫�򣨱���д�� SetHatchPattern ֮ǰ��
        _hatch.PatternSpace = Scale;         //��ࣨ����д�� SetHatchPattern ֮ǰ��
        return this;
    }

    /// <summary>
    /// ģʽ3:�Զ���
    /// </summary>
    /// <param name="name"></param>
    public HatchInfo Mode3UserDefined(string name)
    {
        _hatchName = name;
        _hatch.HatchObjectType = HatchObjectType.HatchObject; //��������(���/����)
        _patternTypeHatch = HatchPatternType.CustomDefined;
        return this;
    }

    /// <summary>
    /// ģʽ4:�������
    /// </summary>
    /// <param name="name">�����������</param>
    /// <param name="colorStart">����ɫ��ʼ��ɫ</param>
    /// <param name="colorEnd">����ɫ������ɫ</param>
    /// <param name="gradientShift">�����ƶ�</param>
    /// <param name="shadeTintValue">ɫ��ֵ</param>
    /// <param name="gradientOneColorMode">��ɫ<see langword="true"/>˫ɫ<see langword="false"/></param>
    public HatchInfo Mode4Gradient(GradientName name, Color colorStart, Color colorEnd,
        float gradientShift = 0,
        float shadeTintValue = 0,
        bool gradientOneColorMode = false)
    {
        //entget��������ֱ�Ȼ��"SOLID",����������Ϊ"����"��,������"���"��
        _hatchName = name.ToString();
        _hatch.HatchObjectType = HatchObjectType.GradientObject;      //��������(���/����)
        _patternTypeGradient = GradientPatternType.PreDefinedGradient;//ģʽ4:����
        //_patternTypeGradient = GradientPatternType.UserDefinedGradient;//ģʽ5:����..����ģʽ��ɶ����

        //���ý���ɫ������ʼ�ͽ�����ɫ
        var gColor1 = new GradientColor(colorStart, 0);
        var gColor2 = new GradientColor(colorEnd, 1);
        _hatch.SetGradientColors(new GradientColor[] { gColor1, gColor2 });

        _hatch.GradientShift = gradientShift;              //�ݶ�λ��
        _hatch.ShadeTintValue = shadeTintValue;            //��Ӱɫֵ
        _hatch.GradientOneColorMode = gradientOneColorMode;//���䵥ɫ/˫ɫ
        _hatch.GradientAngle = Angle;                      //����Ƕ�

        return this;
    }

    /// <summary>
    /// ����
    /// </summary>
    /// <param name="btrOfAddEntitySpace">��������˿ռ�</param>
    public ObjectId Build(BlockTableRecord btrOfAddEntitySpace)
    {
        //�������ݿ�
        var hatchId = btrOfAddEntitySpace.AddEntity(_hatch);

        //����ģʽ:����/���
        if (_hatch.HatchObjectType == HatchObjectType.GradientObject)
            _hatch.SetGradient(_patternTypeGradient, _hatchName);
        else
            _hatch.SetHatchPattern(_patternTypeHatch, _hatchName);

        //�����߽�,�������������ݿ�ռ��ھͻ����
        //Ϊ true ����뷴Ӧ��,��˱Ƚ���(��ά�뽫��ʮ��������ɺ�),���������.
        _hatch.Associative = _boundaryAssociative;

        //���� AppendLoop ���ؼ���,����Ͳ�����
        if (_boundaryIds.Count > 0)
            AppendLoop(_boundaryIds, HatchLoopTypes.Default);

        //������䲢��ʾ(���߽����,�����쳣)
        _hatch.EvaluateHatch(true);

        return hatchId;
    }

    /// <summary>
    /// ִ��ͼԪ�������޸�
    /// </summary>
    /// <param name="action">�ӳ����ʵ��</param>
    public HatchInfo Action(Action<Hatch> action)
    {
        action(_hatch);
        return this;
    }

    /// <summary>
    /// ��ձ߽缯��
    /// </summary>
    public HatchInfo ClearBoundary()
    {
        _boundaryIds.Clear();
        return this;
    }

    /// <summary>
    /// ɾ���߽�ͼԪ
    /// </summary>
    public HatchInfo EraseBoundary()
    {
        for (int i = 0; i < _boundaryIds.Count; i++)
            _boundaryIds[i].Erase();
        return this;
    }

    /// <summary>
    /// ����߽�
    /// </summary>
    /// <param name="boundaryIds">�߽�id</param>
    /// <param name="hatchLoopTypes">���뷽ʽ</param>
    void AppendLoop(IEnumerable<ObjectId> boundaryIds,
                    HatchLoopTypes hatchLoopTypes = HatchLoopTypes.Default)
    {
        var obIds = new ObjectIdCollection();
        //�߽��Ǳպϵ�,�����Ѿ��������ݿ�
        //���պϻ�����.������
        foreach (var border in boundaryIds)
        {
            obIds.Clear();
            obIds.Add(border);
            _hatch.AppendLoop(hatchLoopTypes, obIds);
        }
        obIds.Dispose();
    }

    /// <summary>
    /// ����߽�(�¸߰汾����亯��)
    /// </summary>
    /// <param name="pts">�㼯</param>
    /// <param name="bluges">͹�ȼ�</param>
    /// <param name="btrOfAddEntitySpace">����˿ռ�</param>
    /// <param name="hatchLoopTypes">���뷽ʽ</param>
    /// <returns></returns>
    public HatchInfo AppendLoop(Point2dCollection pts!!,
                             DoubleCollection bluges,
                             BlockTableRecord btrOfAddEntitySpace,
                             HatchLoopTypes hatchLoopTypes = HatchLoopTypes.Default)
    {
        var ptsEnd2End = pts.End2End();
#if NET35
        _boundaryIds.Add(CreateAddBoundary(ptsEnd2End, bluges, btrOfAddEntitySpace));
#else
        //2011����API,���Բ�����ͼԪ������¼���߽�,
        //ͨ���������Ļ�,�߽� _boundaryIds �ǿյ�,��ô Build() ʱ�����Ҫ���˿յ�
        _hatch.AppendLoop(hatchLoopTypes, ptsEnd2End, bluges);
#endif
        return this;
    }

#if NET35
    /// <summary>
    /// ͨ���㼯��͹�����ɱ߽�Ķ����
    /// </summary>
    /// <param name="pts">�㼯</param>
    /// <param name="bluges">͹�ȼ�</param>
    /// <param name="btrOfAddEntitySpace">����˿ռ�</param>
    /// <returns>�����id</returns>
    static ObjectId CreateAddBoundary(Point2dCollection? pts,
        DoubleCollection? bluges,
        BlockTableRecord btrOfAddEntitySpace)
    {
        if (pts is null)
            throw new ArgumentException(null, nameof(pts));
        if (bluges is null)
            throw new ArgumentException(null, nameof(bluges));

        var bvws = new List<BulgeVertexWidth>();

        var itor1 = pts.GetEnumerator();
        var itor2 = bluges.GetEnumerator();
        while (itor1.MoveNext() && itor2.MoveNext())
            bvws.Add(new BulgeVertexWidth(itor1.Current, itor2.Current));

        return btrOfAddEntitySpace.AddPline(bvws);
    }
#endif
    #endregion

    #region ö��
    /// <summary>
    /// ����ɫ����ͼ������
    /// </summary>
    public enum GradientName
    {
        /// <summary>
        /// ��״����
        /// </summary>
        Linear,
        /// <summary>
        /// Բ��״����
        /// </summary>
        Cylinder,
        /// <summary>
        /// ��Բ��״����
        /// </summary>
        Invcylinder,
        /// <summary>
        /// ��״����
        /// </summary>
        Spherical,
        /// <summary>
        /// ����״����
        /// </summary>
        Invspherical,
        /// <summary>
        /// ����״����
        /// </summary>
        Hemisperical,
        /// <summary>
        /// ������״����
        /// </summary>
        InvHemisperical,
        /// <summary>
        /// ������״����
        /// </summary>
        Curved,
        /// <summary>
        /// ��������״����
        /// </summary>
        Incurved
    }
    #endregion
}
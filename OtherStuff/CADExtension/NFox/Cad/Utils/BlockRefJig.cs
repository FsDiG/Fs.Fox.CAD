using System.Collections.Generic;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;


namespace NFox.Cad
{
    internal class BlockRefJig : EntityJig
    {
        private Editor _ed;

        private Point3d _position, _basePoint;
        private double _angle;
        private int _promptCounter;
        private Matrix3d _ucs;

        //键值对集合(属性定义/引用)
        private Dictionary<AttributeDefinition, AttributeReference> _attribs;

        public BlockRefJig(Editor ed, BlockReference bref, Dictionary<AttributeDefinition, AttributeReference> attribs)
            : base(bref)
        {
            _ed = ed;
            _position = new Point3d();
            _angle = 0;
            _ucs = _ed.CurrentUserCoordinateSystem;
            _attribs = attribs;
            Update();
        }

        protected override SamplerStatus Sampler(JigPrompts prompts)
        {
            switch (_promptCounter)
            {
                case 0:
                    {
                        JigPromptPointOptions jigOpts = new JigPromptPointOptions("\n请输入基点:");
                        jigOpts.UserInputControls =
                            UserInputControls.Accept3dCoordinates |
                            UserInputControls.NoZeroResponseAccepted |
                            UserInputControls.NoNegativeResponseAccepted;
                        PromptPointResult res = prompts.AcquirePoint(jigOpts);

                        Point3d pnt = res.Value;
                        if (pnt != _position)
                        {
                            _position = pnt;
                            _basePoint = _position;
                        }
                        else
                        {
                            return SamplerStatus.NoChange;
                        }

                        if (res.Status == PromptStatus.Cancel)
                            return SamplerStatus.Cancel;
                        else
                            return SamplerStatus.OK;
                    }
                case 1:
                    {
                        JigPromptAngleOptions jigOpts = new JigPromptAngleOptions("\n请输入旋转角度:");
                        jigOpts.UserInputControls =
                            UserInputControls.Accept3dCoordinates |
                            UserInputControls.NoNegativeResponseAccepted |
                            UserInputControls.GovernedByUCSDetect |
                            UserInputControls.UseBasePointElevation;
                        jigOpts.Cursor = CursorType.RubberBand;
                        jigOpts.UseBasePoint = true;
                        jigOpts.BasePoint = _basePoint;
                        PromptDoubleResult res = prompts.AcquireAngle(jigOpts);

                        double angleTemp = res.Value;
                        if (angleTemp != _angle)
                            _angle = angleTemp;
                        else
                            return SamplerStatus.NoChange;

                        if (res.Status == PromptStatus.Cancel)
                            return SamplerStatus.Cancel;
                        else
                            return SamplerStatus.OK;
                    }
                default:
                    return SamplerStatus.NoChange;
            }
        }

        protected override bool Update()
        {
            try
            {
                /*Ucs下Jig的流程:
                 * 1.先将图元在Wcs下摆正,即//xy平面
                 * 2.将获取点坐标转换到Ucs下
                 * 3.将图元在Ucs下摆正
                 * 4.矩阵变换到Wcs
                 */
                BlockReference bref = (BlockReference)Entity;
                bref.Normal = Vector3d.ZAxis;

                bref.Position = _position.TransformBy(_ucs.Inverse());
                bref.Rotation = _angle;
                bref.TransformBy(_ucs);

                //将属性引用按块引用的Ocs矩阵变换
                if (_attribs != null)
                {
                    var mat = bref.BlockTransform;
                    foreach (var att in _attribs)
                    {
                        AttributeReference attref = att.Value;
                        string s = attref.TextString;
                        attref.SetAttributeFromBlock(att.Key, mat);
                        attref.TextString = s;
                    }
                }
            }
            catch
            {
                return false;
            }

            return true;
        }

        public PromptResult DragByMove()
        {
            _promptCounter = 0;
            return _ed.Drag(this);
        }

        public PromptResult DragByRotation()
        {
            _promptCounter = 1;
            return _ed.Drag(this);
        }
    }
}
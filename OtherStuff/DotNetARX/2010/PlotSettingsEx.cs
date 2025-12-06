using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.PlottingServices;

namespace DotNetARX
{
    /// <summary>
    /// 增强型打印设置类，支持数据绑定
    /// </summary>
    public class PlotSettingsEx : PlotSettings, INotifyPropertyChanged
    {
        #region INotifyPropertyChanged接口的实现
        /// <summary>
        /// 在更改属性值时发生的事件
        /// </summary>
        public event PropertyChangedEventHandler  PropertyChanged;

        /// <summary>
        /// 处理属性更改事件的函数
        /// </summary>
        /// <param name="info">属性名</param>
        private void NotifyPropertyChanged(String info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        } 
        #endregion

        //获取当前打印设置验证类
        private PlotSettingsValidator validator=PlotSettingsValidator.Current;

        /// <summary>
        /// 复制构造函数，从已有打印设置中获取打印设置
        /// </summary>
        /// <param name="ps">已有打印设置</param>
        public PlotSettingsEx(PlotSettings ps)
            : base(ps.ModelType)
        {
            this.CopyFrom(ps);//从已有打印设置中获取打印设置
            //更新打印设备、图纸尺寸和打印样式表信息
            validator.RefreshLists(this);
        }

        #region 覆盖基类属性
        /// <summary>
        /// 获取或设置当前图纸尺寸（中文表示）
        /// </summary>
        public new string CanonicalMediaName
        {
            get
            {
                //System.Windows.Forms.MessageBox.Show(base.CanonicalMediaName);
                //System.Windows.Forms.MessageBox.Show("父类");
                //将图纸尺寸从英文名改为中文名
                string mediaLocal=base.CanonicalMediaName.Replace("_", " ").Replace("MM", "毫米").Replace("Inches", "英寸").Replace("Pixels", "像素");
                //System.Windows.Forms.MessageBox.Show(mediaLocal);
                return mediaLocal;//返回中文名的图纸尺寸 
            }
            set
            {
                if (value != CanonicalMediaName)//如果图纸尺寸有变化
                {
                    //System.Windows.Forms.MessageBox.Show(value);
                    //System.Windows.Forms.MessageBox.Show("转换");
                    //将图纸尺寸从中文名改为英文名
                    string mediaName=value.Replace(" ", "_").Replace("毫米", "MM").Replace("英寸", "Inches").Replace("像素", "Pixels");
                    //System.Windows.Forms.MessageBox.Show(mediaName);
                    //设置当前图纸尺寸
                    validator.SetCanonicalMediaName(this, mediaName);
                    //向客户端发出某一属性值已更改的通知
                    NotifyPropertyChanged("CanonicalMediaName");
                }
            }
        }




        /// <summary>
        /// 获取或设置当前打印样式表
        /// </summary>
        public new string CurrentStyleSheet
        {
            get { return base.CurrentStyleSheet; }
            set
            {
                if (value != CurrentStyleSheet)
                {
                    if (value != "无")
                    {
                        validator.SetCurrentStyleSheet(this, value);
                    }
                    else validator.SetCurrentStyleSheet(this, "");
                    NotifyPropertyChanged("CurrentStyleSheet");
                }
            }
        }
        /// <summary>
        /// 获取或设置自定义打印比例
        /// </summary>
        public new CustomScale CustomPrintScale
        {
            get { return base.CustomPrintScale; }
            set
            {
                if (value != CustomPrintScale)
                {
                    validator.SetCustomPrintScale(this, value);
                    NotifyPropertyChanged("CustomPrintScale");
                }
            }
        }
        /// <summary>
        /// 获取或设置是否居中打印
        /// </summary>
        public new bool PlotCentered
        {
            get { return base.PlotCentered; }
            set
            {
                if (value != PlotCentered)
                {
                    validator.SetPlotCentered(this, value);
                    NotifyPropertyChanged("PlotCentered");
                }
            }
        }
        /// <summary>
        /// 获取或设置当前打印设备名称
        /// </summary>
        public new string PlotConfigurationName
        {
            get { return base.PlotConfigurationName; }
            set
            {
                if (value != PlotConfigurationName)
                {
                    validator.SetPlotConfigurationName(this, value, null);
                    NotifyPropertyChanged("PlotConfigurationName");
                }
            }
        }
        /// <summary>
        /// 获取或设置当前打印原点（原点是从图纸边界的介质边缘偏移而来的）。
        /// </summary>
        public new Point2d PlotOrigin
        {
            get { return base.PlotOrigin; }
            set
            {
                if (value != PlotOrigin)
                {
                    validator.SetPlotOrigin(this, value);
                    NotifyPropertyChanged("PlotOrigin");
                }
            }
        }
        /// <summary>
        /// 获取或设置打印比例
        /// </summary>
        public new PlotPaperUnit PlotPaperUnits
        {
            get { return base.PlotPaperUnits; }
            set
            {
                if (value != PlotPaperUnits)
                {
                    validator.SetPlotPaperUnits(this, value);
                    NotifyPropertyChanged("PlotPaperUnits");
                }
            }
        }
        /// <summary>
        /// 获取或设置图形方向
        /// </summary>
        public new PlotRotation PlotRotation
        {
            get { return base.PlotRotation; }
            set
            {
                if (value != PlotRotation)
                {
                    validator.SetPlotRotation(this, value);
                    NotifyPropertyChanged("PlotRotation");
                }
            }
        }
        /// <summary>
        /// 获取或设置打印范围
        /// </summary>
        public new Autodesk.AutoCAD.DatabaseServices.PlotType PlotType
        {
            get { return base.PlotType; }
            set
            {
                if (value != PlotType)
                {
                    validator.SetPlotType(this, value);
                    NotifyPropertyChanged("PlotType");
                }
            }
        }
        /// <summary>
        /// 获取或设置打印视口名
        /// </summary>
        public new string PlotViewName
        {
            get { return base.PlotViewName; }
            set
            {
                if (value != PlotViewName)
                {
                    validator.SetPlotViewName(this, value);
                    NotifyPropertyChanged("PlotViewName");
                }
            }
        }
        /// <summary>
        /// 获取或设置打印窗口的范围
        /// </summary>
        public new Extents2d PlotWindowArea
        {
            get { return base.PlotWindowArea; }
            set
            {
                if (value != PlotWindowArea)
                {
                    validator.SetPlotWindowArea(this, value);
                    NotifyPropertyChanged("PlotWindowArea");
                }
            }
        }
        /// <summary>
        /// 获取或设置当前标准比例（实数形式）
        /// </summary>
        public new double StdScale
        {
            get { return base.StdScale; }
            set
            {
                if (value != StdScale)
                {
                    validator.SetStdScale(this, value);
                    NotifyPropertyChanged("StdScale");
                }
            }
        }
        /// <summary>
        /// 获取或设置当前标准比例（枚举形式）
        /// </summary>
        public new StdScaleType StdScaleType
        {
            get { return base.StdScaleType; }
            set
            {
                if (value != StdScaleType)
                {
                    validator.SetStdScaleType(this, value);
                    NotifyPropertyChanged("StdScaleType");
                }
            }
        }
        /// <summary>
        /// 获取或设置是否选用标准比例
        /// </summary>
        public new bool UseStandardScale
        {
            get { return base.UseStandardScale; }
            set
            {
                if (value != UseStandardScale)
                {
                    validator.SetUseStandardScale(this, value);
                    NotifyPropertyChanged("UseStandardScale");
                }
            }
        }
        #endregion
        /// <summary>
        /// 获取或设置当前图形单位
        /// </summary>
        public double Denominator
        {
            get { return base.CustomPrintScale.Denominator; }
            set
            {
                if (value != Denominator)
                {
                    CustomScale newScale=new CustomScale(CustomPrintScale.Numerator, value);
                    validator.SetCustomPrintScale(this, newScale);
                    NotifyPropertyChanged("Denominator");
                }
            }
        }
        /// <summary>
        /// 获取或设置当前图纸单位
        /// </summary>
        public double Numerator
        {
            get { return base.CustomPrintScale.Numerator; }
            set
            {
                if (value != Numerator)
                {
                    CustomScale newScale=new CustomScale(value, CustomPrintScale.Denominator);
                    validator.SetCustomPrintScale(this, newScale);
                    NotifyPropertyChanged("Numerator");
                }
            }
        }
        /// <summary>
        /// 获取或设置当前X方向的打印偏移
        /// </summary>
        public double PlotOriginX
        {
            get { return base.PlotOrigin.X; }
            set
            {
                if (value != PlotOriginX)
                {
                    Point2d newOrigin=new Point2d(value, PlotOrigin.Y);
                    validator.SetPlotOrigin(this, newOrigin);
                    NotifyPropertyChanged("PlotOriginX");
                }
            }
        }
        /// <summary>
        /// 获取或设置当前Y方向的打印偏移
        /// </summary>
        public double PlotOriginY
        {
            get { return base.PlotOrigin.Y; }
            set
            {
                if (value != PlotOriginY)
                {
                    Point2d newOrigin=new Point2d(PlotOrigin.X, value);
                    validator.SetPlotOrigin(this, newOrigin);
                    NotifyPropertyChanged("PlotOriginY");
                }
            }
        }
        /// <summary>
        /// 获取或设置打印时是否布满图纸
        /// </summary>
        public bool PlotExtent
        {
            get
            {
                if (this.UseStandardScale && this.StdScaleType == StdScaleType.ScaleToFit)
                    return true;
                else return false;
            }
            set
            {
                if (value != PlotExtent)
                {
                    if (value)
                    {
                        validator.SetUseStandardScale(this, true);
                        validator.SetStdScaleType(this, StdScaleType.ScaleToFit);
                    }
                    else validator.SetUseStandardScale(this, false);
                    NotifyPropertyChanged("PlotExtent");
                }
            }
        }
        /// <summary>
        /// 获取或设置是否反向打印
        /// </summary>
        public bool PlotReverse
        {
            get
            {
                if (PlotRotation == PlotRotation.Degrees180 || PlotRotation == PlotRotation.Degrees270) return true;
                else return false;
            }
            set
            {
                if (value != PlotReverse)
                {
                    int numRotation=(int)PlotRotation;
                    if (value == true)
                    {
                        numRotation += 2;
                    }
                    else
                        numRotation -= 2;
                    validator.SetPlotRotation(this, (Autodesk.AutoCAD.DatabaseServices.PlotRotation)numRotation);
                    NotifyPropertyChanged("PlotReverse");
                }
            }
        }
        #region 列表类属性
        /// <summary>
        /// 获取当前打印设备列表
        /// </summary>
        public List<string> DeviceList
        {
            get
            {
                StringCollection deviceCollection=validator.GetPlotDeviceList();
                List<string> deviceList=new List<string>();
                foreach (string device in deviceCollection)
                {
                    deviceList.Add(device);
                }
                return deviceList;
            }
        }
        /// <summary>
        /// 获取当前图纸尺寸列表（中文表示）
        /// </summary>
        public List<string> MediaList
        {
            get
            {
                StringCollection mediaCollection=validator.GetCanonicalMediaNameList(this);
                List<string> mediaList=new List<string>();
                foreach (string media in mediaCollection)
                {
                    string mediaLocal=media.Replace("_", " ").Replace("MM", "毫米").Replace("Inches", "英寸").Replace("Pixels", "像素");
                    mediaList.Add(mediaLocal);
                }
                return mediaList; ;
            }
        }
        /// <summary>
        /// 获取当前打印范围列表（中文表示）
        /// </summary>
        public List<string> PlotTypeList
        {
            get
            {
                if (ModelType)//模型空间
                    return (new List<string> { "窗口", "图形界限", "显示" });
                else//图纸空间
                    return (new List<string> { "布局", "窗口", "范围", "显示" });
            }
        }
        /// <summary>
        /// 获取当前打印单位列表（中文表示）
        /// </summary>
        public List<string> PlotUnitList
        {
            get
            {
                if (PlotPaperUnits == PlotPaperUnit.Pixels)
                    return (new List<string> { "像素" });
                else
                    return (new List<string> { "毫米", "英寸" });
            }
        }
        /// <summary>
        /// 获取颜色相关打印样式列表
        /// </summary>
        public List<string> ColorDependentPlotStyles
        {
            get
            {
                List<string> styleSheetList=(from string style in validator.GetPlotStyleSheetList()
                                             where style.EndsWith("ctb")
                                             select style).ToList();
                styleSheetList.Insert(0, "无");
                return styleSheetList;
            }
        }
        /// <summary>
        /// 获取命名打印样式列表
        /// </summary>
        public List<string> NamedPlotStyles
        {
            get
            {
                List<string> styleSheetList=(from string style in validator.GetPlotStyleSheetList()
                                             where style.EndsWith("stb")
                                             select style).ToList();
                styleSheetList.Insert(0, "无");
                return styleSheetList;
            }
        }
        /// <summary>
        /// 获取标准打印比例列表
        /// </summary>
        public Dictionary<StdScaleType, string> StdScaleTypeList
        {
            get
            {
                var scaleTypeDict=new Dictionary<StdScaleType, string>();
                List<string> scaleTypeList=new List<string>() 
                {
                    "自定义","1/128'' = 1'-0''","1/64'' = 1'-0''","1/32'' = 1'-0''","1/16'' = 1'-0''","3/32'' = 1'-0''",
                    "1/8'' = 1'-0''","3/16'' = 1'-0''","1/4'' = 1'-0''","3/8'' = 1'-0''","1/2'' = 1'-0''","3/4'' = 1'-0''",
                    "1'' = 1'-0''","3'' = 1'-0''","6'' = 1'-0''","1'-0'''' = 1'-0''",
                    "1:1","1:2","1:4","1:8","1:10","1:16","1:20","1:30","1:40","1:50","1:100",
                    "2:1","4:1","8:1","10:1","100:1","1000:1"
                };
                scaleTypeDict.Add(StdScaleType.ScaleToFit, scaleTypeList[0]);
                for (int i = 16; i <= 32; i++)
                {
                    scaleTypeDict.Add((StdScaleType)i, scaleTypeList[i]);
                }
                for (int i = 1; i < 16; i++)
                {
                    scaleTypeDict.Add((StdScaleType)i, scaleTypeList[i]);
                }
                return scaleTypeDict;
            }
        }
        #endregion
        #region 中文属性
        /// <summary>
        /// 获取或设置打印范围（中文）
        /// </summary>
        public string PlotTypeLocal
        {
            get
            {
                string plotTypeLocal="";
                switch (base.PlotType)
                {
                    case Autodesk.AutoCAD.DatabaseServices.PlotType.Display:
                        plotTypeLocal = "显示";
                        break;
                    case Autodesk.AutoCAD.DatabaseServices.PlotType.Extents:
                        plotTypeLocal = "范围";
                        break;
                    case Autodesk.AutoCAD.DatabaseServices.PlotType.Layout:
                        plotTypeLocal = "布局";
                        break;
                    case Autodesk.AutoCAD.DatabaseServices.PlotType.Limits:
                        plotTypeLocal = "图形界限";
                        break;
                    case Autodesk.AutoCAD.DatabaseServices.PlotType.View:
                        plotTypeLocal = "视图";
                        break;
                    case Autodesk.AutoCAD.DatabaseServices.PlotType.Window:
                        plotTypeLocal = "窗口";
                        break;
                }
                return plotTypeLocal;
            }
            set
            {
                if (value != PlotTypeLocal)
                {
                    switch (value)
                    {
                        case "显示":
                            PlotType = Autodesk.AutoCAD.DatabaseServices.PlotType.Display;
                            break;
                        case "范围":
                            PlotType = Autodesk.AutoCAD.DatabaseServices.PlotType.Extents;
                            break;
                        case "布局":
                            PlotType = Autodesk.AutoCAD.DatabaseServices.PlotType.Layout;
                            break;
                        case "图形界限":
                            PlotType = Autodesk.AutoCAD.DatabaseServices.PlotType.Limits;
                            break;
                        case "视图":
                            PlotType = Autodesk.AutoCAD.DatabaseServices.PlotType.View;
                            break;
                        case "窗口":
                            PlotType = Autodesk.AutoCAD.DatabaseServices.PlotType.Window;
                            break;
                    }
                    NotifyPropertyChanged("PlotTypeLocal");
                }
            }
        }

        /// <summary>
        /// 获取或设置打印单位（中文）
        /// </summary>
        public string PlotPaperUnitsLocal
        {
            get
            {
                string plotUnitLocal="";
                switch (base.PlotPaperUnits)
                {
                    case PlotPaperUnit.Inches: plotUnitLocal = "英寸"; break;
                    case PlotPaperUnit.Millimeters: plotUnitLocal = "毫米"; break;
                    case PlotPaperUnit.Pixels: plotUnitLocal = "像素"; break;
                }
                return plotUnitLocal;
            }
            set
            {
                if (value != PlotPaperUnitsLocal)
                {
                    switch (value)
                    {
                        case "英寸": validator.SetPlotPaperUnits(this, PlotPaperUnit.Inches); break;
                        case "毫米": validator.SetPlotPaperUnits(this, PlotPaperUnit.Millimeters); break;
                        case "像素": validator.SetPlotPaperUnits(this, PlotPaperUnit.Pixels); break;
                    }
                    NotifyPropertyChanged("PlotPaperUnitsLocal");
                }
            }
        }

        /// <summary>
        /// 获取或设置当前标准打印比例（中文）
        /// </summary>
        public string StdScaleTypeLocal
        {
            get
            {
                if (!UseStandardScale) return "自定义";
                else
                    return StdScaleTypeList[StdScaleType];
            }
            set
            {
                if (value != StdScaleTypeLocal)
                {

                    StdScaleType newScaleType=StdScaleTypeList.First(s => s.Value == value).Key;
                    validator.SetStdScaleType(this, newScaleType);
                    NotifyPropertyChanged("StdScaleTypeLocal");
                }
            }
        }
        #endregion
        /// <summary>
        /// 是否打印到文件
        /// </summary>
        public bool IsPlotToFile
        {
            get
            {
                //如果未选择打印设备，则直接返回
                if (PlotConfigurationName == "无") return false;
                //获取当前打印配置，并返回其是否打印到文件属性
                PlotConfig config=PlotConfigManager.CurrentConfig;
                return config.IsPlotToFile;
            }
        }
        /// <summary>
        /// 可以不打印到文件
        /// </summary>
        public bool NoPlotToFile
        {
            get
            {
                //如果未选择打印设备，则直接返回
                if (PlotConfigurationName == "无") return false;
                //获取当前打印配置，并返回其是否必须打印到文件
                PlotConfig config=PlotConfigManager.CurrentConfig;
                if (config.PlotToFileCapability == PlotToFileCapability.MustPlotToFile)
                    return false;
                else return true;
            }
        }
        /// <summary>
        /// 获取或设置是否横向打印
        /// </summary>
        public bool PlotHorizontal
        {
            get
            {
                if (base.PlotRotation == PlotRotation.Degrees090 || base.PlotRotation == PlotRotation.Degrees270) return true;
                else return false;
            }
            set
            {
                if (value != PlotHorizontal)
                {
                    if (value == true)
                    {
                        if (PlotReverse)
                            validator.SetPlotRotation(this, PlotRotation.Degrees270);
                        else
                            validator.SetPlotRotation(this, PlotRotation.Degrees090);
                    }
                    else
                    {
                        if (PlotReverse)
                            validator.SetPlotRotation(this, PlotRotation.Degrees180);
                        else
                            validator.SetPlotRotation(this, PlotRotation.Degrees000);
                    }
                }
                NotifyPropertyChanged("PlotHorizontal");
            }
        }

        /// <summary>
        /// 更新其它的打印设置
        /// </summary>
        /// <param name="psId">打印设置对象</param>
        public void UpdatePlotSettings(ObjectId psId)
        {
            Document doc=Application.DocumentManager.MdiActiveDocument;
            using (doc.LockDocument())
            using (Transaction trans = doc.TransactionManager.StartTransaction())
            {
                PlotSettings ps=psId.GetObject(OpenMode.ForWrite) as PlotSettings;
                if (ps != null)
                {
                    ps.CopyFrom(this);//复制当前打印设置
                }
                trans.Commit();
            }
        }
    }
}

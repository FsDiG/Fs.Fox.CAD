using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autodesk.AutoCAD.PlottingServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.ApplicationServices;
using System.Xml.Linq;
using Autodesk.AutoCAD.EditorInput;
using System.Collections.Specialized;
using Autodesk.AutoCAD.Geometry;

namespace DotNetARX
{
    /// <summary>
    /// 打印操作类
    /// </summary>
    public static class PlotTools
    {
        /// <summary>
        /// 执行单一布局打印
        /// </summary>
        /// <param name="engine">打印引擎对象</param>
        /// <param name="layout">要打印的布局</param>
        /// <param name="ps">打印设置</param>
        /// <param name="fileName">打印文件名</param>
        /// <param name="copies">打印份数</param>
        /// <param name="isPreview">是否预览打印</param>
        /// <param name="showProgressDialog">是否显示打印进度框</param>
        /// <param name="plotToFile">是否打印到文件</param>
        /// <returns>返回打印状态</returns>
        public static PreviewEndPlotStatus Plot(this PlotEngine engine, Layout layout, PlotSettings ps, string fileName, int copies, bool isPreview, bool showProgressDialog, bool plotToFile)
        {
            PlotProgressDialog plotDlg=null;//声明一个打印进度框对象
            if (showProgressDialog)//如果需要显示打印进度框，则创建
            {
                plotDlg = new PlotProgressDialog(isPreview, 1, true);
                //获取去除扩展名后的文件名（不含路径）
                string plotFileName=SymbolUtilityServices.GetSymbolNameFromPathName(fileName, "dwg");
                //在打印进度框中显示的图纸名称：当前打印布局的名称（含文档名）
                plotDlg.set_PlotMsgString(PlotMessageIndex.SheetName, "正在处理图纸：" + layout.LayoutName + "(" + plotFileName + ".dwg)");
            }
            //设置打印的状态为停止
            PreviewEndPlotStatus status=PreviewEndPlotStatus.Cancel;
            PlotInfo pi=new PlotInfo();//创建打印信息
            pi.Layout = layout.ObjectId;//要打印的布局
            pi.OverrideSettings = ps;//使用ps中的打印设置
            //验证打印信息是否有效
            PlotInfoValidator piv=new PlotInfoValidator();
            piv.MediaMatchingPolicy = MatchingPolicy.MatchEnabled;
            piv.Validate(pi);
            //启动打印进度框
            if (showProgressDialog) plotDlg.StartPlotProgress(isPreview);
            engine.BeginPlot(plotDlg, null);//开启打印引擎进行打印任务
            //开始打印文档
            engine.BeginDocument(pi, layout.Database.GetDocument().Name, null, copies, plotToFile, fileName);
            //启动打印图纸进程
            if (plotDlg != null) plotDlg.StartSheetProgress();
            //开始打印页面
            PlotPageInfo ppi=new PlotPageInfo();
            engine.BeginPage(ppi, pi, true, null);
            engine.BeginGenerateGraphics(null);//开始打印内容
            //设置打印进度
            if (plotDlg != null) plotDlg.SheetProgressPos = 50;
            engine.EndGenerateGraphics(null);//内容打印结束
            //结束页面打印
            PreviewEndPlotInfo pepi=new PreviewEndPlotInfo();
            engine.EndPage(pepi);
            status = pepi.Status;//打印预览结束时的状态
            //终止显示打印图纸进程
            if (plotDlg != null) plotDlg.EndSheetProgress();
            engine.EndDocument(null);//文档打印结束
            //终止显示打印进度框
            if (plotDlg != null) plotDlg.EndPlotProgress();
            engine.EndPlot(null);//结束打印任务
            return status;//打印预览结束时的状态
        }
        /// <summary>
        /// 执行多布局打印
        /// </summary>
        /// <param name="engine">打印引擎对象</param>
        /// <param name="layouts">要打印的布局列表</param>
        /// <param name="ps">打印设置</param>
        /// <param name="fileName">打印文件名</param>
        /// <param name="previewNum">预览的布局号，小于1表示打印</param>
        /// <param name="copies">打印份数</param>
        /// <param name="showProgressDialog">是否显示打印进度框</param>
        /// <param name="plotToFile">是否打印到文件</param>
        /// <returns>返回打印状态</returns>
        public static PreviewEndPlotStatus MPlot(this PlotEngine engine, List<Layout> layouts, PlotSettings ps, string fileName, int previewNum, int copies, bool showProgressDialog, bool plotToFile)
        {
            int numSheet=1;//表示当前打印的图纸序号
            //设置是否为预览
            bool isPreview=previewNum >= 1 ? true : false;
            Document doc=Application.DocumentManager.MdiActiveDocument;
            PlotProgressDialog plotDlg=null;//声明一个打印进度框对象
            if (showProgressDialog)//如果需要显示打印进度框，则创建
                plotDlg = new PlotProgressDialog(isPreview, layouts.Count, true);
            //设置打印的状态为停止
            PreviewEndPlotStatus status=PreviewEndPlotStatus.Cancel;
            //重建一个布局列表，因为打印预览只能是单页的
            List<Layout> layoutList=new List<Layout>();
            if (isPreview && previewNum >= 1)//如果为打印预览
                layoutList.Add(layouts[previewNum - 1]);//只对预览的布局进行操作
            else layoutList.AddRange(layouts);//如果为打印，则需对所有的布局进行操作
            foreach (Layout layout in layoutList)//遍历布局
            {
                PlotInfo pi=new PlotInfo();//创建打印信息
                pi.Layout = layout.ObjectId;//要打印的布局
                //要多文档打印，必须将要打印的布局设置为当前布局
                LayoutManager.Current.CurrentLayout = layout.LayoutName;
                pi.OverrideSettings = ps;//使用ps中的打印设置
                //验证打印信息是否有效
                PlotInfoValidator piv=new PlotInfoValidator();
                piv.MediaMatchingPolicy = MatchingPolicy.MatchEnabled;
                piv.Validate(pi);
                if (plotDlg != null)//如果显示打印进度框
                {
                    //获取去除扩展名后的文件名（不含路径）
                    string plotFileName=SymbolUtilityServices.GetSymbolNameFromPathName(doc.Name, "dwg");
                    //在打印进度框中显示的图纸名称：当前打印布局的名称（含文档名）
                    plotDlg.set_PlotMsgString(PlotMessageIndex.SheetName, plotFileName + "-" + layout.LayoutName);
                }
                //如果打印的是第一张图纸则启动下面的操作，以后就不再需要再次进行
                if (numSheet == 1)
                {
                    //启动打印进度框
                    if (showProgressDialog) plotDlg.StartPlotProgress(isPreview);
                    engine.BeginPlot(plotDlg, null);//开启打印引擎进行打印任务
                    //开始打印文档
                    engine.BeginDocument(pi, doc.Name, null, copies, plotToFile, fileName);
                }
                //启动打印图纸进程
                if (plotDlg != null) plotDlg.StartSheetProgress();
                //开始打印页面
                PlotPageInfo ppi=new PlotPageInfo();
                engine.BeginPage(ppi, pi, (numSheet == layoutList.Count), null);
                engine.BeginGenerateGraphics(null);//开始打印内容
                //设置打印进度
                if (plotDlg != null) plotDlg.SheetProgressPos = 50;
                engine.EndGenerateGraphics(null);//内容打印结束
                //结束页面打印
                PreviewEndPlotInfo pepi=new PreviewEndPlotInfo();
                engine.EndPage(pepi);
                status = pepi.Status;//打印预览结束时的状态
                //终止显示打印图纸进程
                if (plotDlg != null) plotDlg.EndSheetProgress();
                numSheet++;//将要打印的图纸序号设置为下一个
                //更新打印进程
                if (plotDlg != null)
                    plotDlg.PlotProgressPos += (100 / layouts.Count);
            }
            engine.EndDocument(null);//文档打印结束
            if (plotDlg != null) plotDlg.EndPlotProgress();//终止显示打印进度框
            engine.EndPlot(null);//结束打印任务
            return status;//返回打印预览结束时的状态
        }
        /// <summary>
        /// 启动打印图纸进程
        /// </summary>
        /// <param name="plotDlg">打印进度框对象</param>
        public static void StartSheetProgress(this PlotProgressDialog plotDlg)
        {
            plotDlg.LowerSheetProgressRange = 0;//开始的打印进度
            plotDlg.UpperSheetProgressRange = 100;//线束时的打印进度
            plotDlg.SheetProgressPos = 0;//当前进度为0，表示开始
            plotDlg.OnBeginSheet();//图纸打印开始，进度框开始工作            
        }
        /// <summary>
        /// 终止打印图纸进程
        /// </summary>
        /// <param name="plotDlg">打印进度框对象</param>
        public static void EndSheetProgress(this PlotProgressDialog plotDlg)
        {
            plotDlg.SheetProgressPos = 100;//设置当前进度为100
            plotDlg.OnEndSheet();//图纸打印结束,进度框停止工作
        }
        /// <summary>
        /// 启动打印进度框
        /// </summary>
        /// <param name="plotDlg">打印进度框对象</param>
        /// <param name="isPreview">是否为预览打印</param>
        public static void StartPlotProgress(this PlotProgressDialog plotDlg, bool isPreview)
        {
            Document doc=Application.DocumentManager.MdiActiveDocument;
            //获取去除扩展名后的文件名（不含路径）
            string plotFileName=SymbolUtilityServices.GetSymbolNameFromPathName(doc.Name, "dwg");
            //设置打印进度框中的提示信息
            string dialogTitle=isPreview ? "预览作业进度" : "打印作业进度";
            plotDlg.set_PlotMsgString(PlotMessageIndex.DialogTitle, dialogTitle);
            plotDlg.set_PlotMsgString(PlotMessageIndex.SheetProgressCaption, "正在处理图纸:" + plotFileName);
            plotDlg.set_PlotMsgString(PlotMessageIndex.CancelJobButtonMessage, "取消打印任务");
            plotDlg.set_PlotMsgString(PlotMessageIndex.CancelSheetButtonMessage, "取消文档");
            plotDlg.set_PlotMsgString(PlotMessageIndex.SheetProgressCaption, "进度：");
            plotDlg.LowerPlotProgressRange = 0;//开始的打印进度
            plotDlg.UpperPlotProgressRange = 100;//线束时的打印进度
            plotDlg.PlotProgressPos = 0;//当前进度为0，表示开始
            plotDlg.OnBeginPlot();//打印开始，进程框开始工作
            plotDlg.IsVisible = true;//显示打印进度框
        }
        /// <summary>
        /// 终止打印进度框
        /// </summary>
        /// <param name="plotDlg">打印进度框对象</param>
        public static void EndPlotProgress(this PlotProgressDialog plotDlg)
        {
            plotDlg.PlotProgressPos = 100;//设置当前进度为100
            plotDlg.OnEndPlot();//结束打印
            plotDlg.Dispose();//销毁打印进度框
        }
        /// <summary>
        /// 将打印设备及标准图纸尺寸清单存储到XML文件
        /// </summary>
        /// <param name="fileName">XML文件名</param>
        public static void DeviceMeidaToXML(string fileName)
        {
            XElement xroot=new XElement("Root");//创建一个XML根元素
            //获取当前打印设备列表
            PlotSettingsValidator psv=PlotSettingsValidator.Current;
            StringCollection devices=psv.GetPlotDeviceList();
            //创建打印设置对象，以获取设备拥有的图纸尺寸
            PlotSettings ps=new PlotSettings(true);
            foreach (string device in devices)//遍历打印设备
            {
                //创建一个名为Device的新元素
                XElement xDevice=new XElement("Device");
                //在Device元素下添加表示设备名称的属性
                xDevice.Add(new XAttribute("Name", device));
                //更新打印设备、图纸尺寸，以反映当前系统状态。
                psv.SetPlotConfigurationName(ps, device, null);
                psv.RefreshLists(ps);
                //获取打印设备的所有可用标准图纸尺寸的名称
                StringCollection medias=psv.GetCanonicalMediaNameList(ps);
                foreach (string media in medias)
                {
                    //如果为用户自定义图纸尺寸，则结束本次循环
                    if (media == "UserDefinedMetric") continue;
                    //创建一个名为Media的新元素
                    XElement xMedia=new XElement("Media");
                    //在Media元素下添加表示图纸尺寸的属性
                    xMedia.Add(new XAttribute("Name", media));
                    xDevice.Add(xMedia);//添加Media元素到Device元素中
                }
                xroot.Add(xDevice);//添加Device元素到根元素中
            }
            xroot.Save(fileName);//保存XML文件
        }
        /// <summary>
        /// 从XML文件中读取打印设备及标准图纸尺寸清单
        /// </summary>
        /// <param name="fileName">XML文件名</param>
        /// <param name="deviceName">打印设备名</param>
        /// <returns>返回打印设备及其对应的标准图纸尺寸清单</returns>
        public static List<string> MeidasFromXML(string fileName, string deviceName)
        {
            List<string> medias=new List<string>();
            XElement xroot=XElement.Load(fileName);//载入XML文件
            var devices=from d in xroot.Elements("Device")
                        where d.Attribute("Name").Value == deviceName
                        select d;
            if (devices.Count() != 1) return medias;
            medias = (from m in devices.First().Elements("Media")
                      select m.Attribute("Name").Value).ToList();
            return medias;//返回标准图纸尺寸清单的字典对象
        }

        /// <summary>
        /// 从XML文件读取打印设备名列表
        /// </summary>
        /// <param name="fileName">XML文件名</param>
        /// <returns>返回打印设备名列表</returns>
        public static List<string> DevicesFromXML(string fileName)
        {
            XElement xroot=XElement.Load(fileName);//载入XML文件
            List<string> devices=(from d in xroot.Elements("Device")
                                  select d.Attribute("Name").Value).ToList();
            return devices;
        }
        /// <summary>
        /// 从其它图形数据库中复制打印设置
        /// </summary>
        /// <param name="destdb">目的图形数据库</param>
        /// <param name="sourceDb">源图形数据库</param>
        /// <param name="plotSettingName">打印设置名称</param>
        public static void CopyPlotSettings(this Database destdb, Database sourceDb, string plotSettingName)
        {
            using (Transaction trSource = sourceDb.TransactionManager.StartTransaction())
            {
                using (Transaction trDest =destdb.TransactionManager.StartTransaction())
                {
                    //获取源图形数据库的打印设置字典
                    DBDictionary dict = trSource.GetObject(sourceDb.PlotSettingsDictionaryId, OpenMode.ForRead) as DBDictionary;
                    if (dict != null && dict.Contains(plotSettingName))
                    {
                        //获取指定名称的打印设置
                        ObjectId settingsId = dict.GetAt(plotSettingName);
                        PlotSettings settings = trSource.GetObject(settingsId, OpenMode.ForRead) as PlotSettings;
                        //新建一个打印设置对象
                        PlotSettings newSettings = new PlotSettings(settings.ModelType);
                        newSettings.CopyFrom(settings);//复制打印设置
                        //将新建的打印设置对象添加到目的数据库的打印设置字典中
                        newSettings.AddToPlotSettingsDictionary(destdb);
                        trDest.AddNewlyCreatedDBObject(newSettings, true);
                    }
                    trDest.Commit();
                }
                trSource.Commit();
            }
        }
        /// <summary>
        /// 复制另一个图形数据库中的所有打印设置
        /// </summary>
        /// <param name="destdb">目的图形数据库</param>
        /// <param name="sourceDb">源图形数据库</param>
        public static void CopyPlotSettings(this Database destdb, Database sourceDb)
        {
            using (Transaction trSource = sourceDb.TransactionManager.StartTransaction())
            {
                using (Transaction trDest=destdb.TransactionManager.StartTransaction())
                {
                    //获取源图形数据库的打印设置字典
                    DBDictionary dict = trSource.GetObject(sourceDb.PlotSettingsDictionaryId, OpenMode.ForRead) as DBDictionary;
                    //对打印设置字典中的条目进行遍历
                    foreach (DBDictionaryEntry entry in dict)
                    {
                        //获取指定名称的打印设置
                        ObjectId settingsId = entry.Value;
                        PlotSettings settings = trSource.GetObject(settingsId, OpenMode.ForRead) as PlotSettings;
                        //新建一个打印设置对象
                        PlotSettings newSettings = new PlotSettings(settings.ModelType);
                        newSettings.CopyFrom(settings);//复制打印设置
                        //将新建的打印设置对象添加到目的数据库的打印设置字典中
                        newSettings.AddToPlotSettingsDictionary(destdb);
                        trDest.AddNewlyCreatedDBObject(newSettings, true);
                    }
                    trDest.Commit();
                }
                trSource.Commit();
            }
        }
    }
}

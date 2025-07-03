using System.Collections.Specialized;
using System.IO;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.Customization;

namespace DotNetARX
{
    /// <summary>
    /// 操作CUI的类
    /// </summary>
    public static class CUITools
    {
        /// <summary>
        /// 获取并打开主CUI文件
        /// </summary>
        /// <param name="doc">AutoCAD文档对象</param>
        /// <returns>返回主CUI文件</returns>
        public static CustomizationSection GetMainCustomizationSection(this Document doc)
        {
            //获得主CUI文件所在的位置
            string mainCuiFile = Application.GetSystemVariable("MENUNAME") + ".cuix";
            //打开主CUI文件
            return new CustomizationSection(mainCuiFile);
        }

        /// <summary>
        /// 创建局部CUI文件
        /// </summary>
        /// <param name="doc">AutoCAD文档对象</param>
        /// <param name="cuiFile">CUI文件名</param>
        /// <param name="menuGroupName">菜单组的名称</param>
        /// <returns>返回创建的CUI文件</returns>
        public static CustomizationSection AddCui(this Document doc, string cuiFile, string menuGroupName)
        {
            CustomizationSection cs;//声明CUI文件对象
            if (!File.Exists(cuiFile))//如果要创建的文件不存在
            {
                cs = new CustomizationSection();//创建CUI文件对象
                cs.MenuGroupName = menuGroupName;//指定菜单组名称
                cs.SaveAs(cuiFile);//保存CUI文件
            }
            //如果已经存在指定的CUI文件，则打开该文件
            else cs = new CustomizationSection(cuiFile);
            return cs;//返回CUI文件对象
        }

        /// <summary>
        /// 装载指定的局部CUI文件
        /// </summary>
        /// <param name="cs">CUI文件</param>
        public static void LoadCui(this CustomizationSection cs)
        {
            if (cs.IsModified) cs.Save();//如果CUI文件被修改，则保存

            //获取当前活动文档
            Document doc = Application.DocumentManager.MdiActiveDocument;
            //获取主CUI文件
            CustomizationSection mainCs = doc.GetMainCustomizationSection();
            //如果已存在局部CUI文件，则先卸载
            if (mainCs.PartialCuiFiles.Contains(cs.CUIFileName))
            {
                Application.UnloadPartialMenu(cs.CUIFileName);
            }
            //装载CUI文件，注意文件名必须是带路径的
            Application.LoadPartialMenu(cs.CUIFileName);
        }

        /// <summary>
        /// 添加菜单项所要执行的宏
        /// </summary>
        /// <param name="source">CUI文件</param>
        /// <param name="name">宏的显示名称</param>
        /// <param name="command">宏的具体命令</param>
        /// <param name="tag">宏的标识符</param>
        /// <param name="helpString">宏的状态栏提示信息</param>
        /// <param name="imagePath">宏的图标</param>
        /// <returns>返回创建的宏</returns>
        public static MenuMacro AddMacro(this CustomizationSection source, string name, string command, string tag, string helpString, string imagePath)
        {
            MenuGroup menuGroup = source.MenuGroup;//获取CUI文件中的菜单组
            //判断菜单组中是否已经定义与菜单组名相同的宏集合
            MacroGroup mg = menuGroup.FindMacroGroup(menuGroup.Name);
            if (mg == null)//如果宏集合没有定义，则创建一个与菜单组名相同的宏集合
                mg = new MacroGroup(menuGroup.Name, menuGroup);
            //如果已经宏已经被定义，则返回
            foreach (MenuMacro macro in mg.MenuMacros)
                if (macro.ElementID == tag) return null;
            //在宏集合中创建一个命令宏
            MenuMacro MenuMacro = new MenuMacro(mg, name, command, tag);
            //指定命令宏的说明信息，在状态栏中显示
            MenuMacro.macro.HelpString = helpString;
            //指定命令宏的大小图像的路径
            MenuMacro.macro.LargeImage = MenuMacro.macro.SmallImage = imagePath;
            return MenuMacro;//返回命令宏
        }

        /// <summary>
        /// 添加下拉菜单
        /// </summary>
        /// <param name="menuGroup">包含菜单的菜单组</param>
        /// <param name="name">菜单名</param>
        /// <param name="aliasList">菜单的别名</param>
        /// <param name="tag">菜单的标识字符串</param>
        /// <returns>返回下拉菜单对象</returns>
        public static PopMenu AddPopMenu(this MenuGroup menuGroup, string name, StringCollection aliasList, string tag)
        {
            PopMenu pm = null;//声明下拉菜单对象
            //如果菜单组中没有名称为name的下拉菜单
            if (menuGroup.PopMenus.IsNameFree(name))
            {
                //为下拉菜单指定显示名称、别名、标识符和所属的菜单组
                pm = new PopMenu(name, aliasList, tag, menuGroup);
            }
            return pm;//返回下拉菜单对象
        }

        /// <summary>
        /// 为菜单添加菜单项
        /// </summary>
        /// <param name="parentMenu">菜单项所属的菜单</param>
        /// <param name="index">菜单项的位置</param>
        /// <param name="name">菜单项的显示名称</param>
        /// <param name="macroId">菜单项的命令宏的Id</param>
        /// <returns>返回添加的菜单项</returns>
        public static PopMenuItem AddMenuItem(this PopMenu parentMenu, int index, string name, string macroId)
        {
            PopMenuItem newPmi = null;
            //如果存在名为name的菜单项，则返回
            foreach (PopMenuItem pmi in parentMenu.PopMenuItems)
                if (pmi.Name == name) return newPmi;
            //定义一个菜单项对象，指定所属的菜单及位置
            newPmi = new PopMenuItem(parentMenu, index);
            ////如果name不为空，则指定菜单项的显示名为name，否则会使用命令宏的名称
            if (name != null) newPmi.Name = name;
            newPmi.MacroID = macroId;//菜单项的命令宏的ID
            return newPmi;//返回菜单项对象
        }

        /// <summary>
        /// 为下拉菜单添加子菜单
        /// </summary>
        /// <param name="parentMenu">下拉菜单</param>
        /// <param name="index">子菜单的位置</param>
        /// <param name="name">子菜单的显示名称</param>
        /// <param name="tag">子菜单的标识字符串</param>
        /// <returns>返回添加的子菜单</returns>
        public static PopMenu AddSubMenu(this PopMenu parentMenu, int index, string name, string tag)
        {
            PopMenu pm = null;//声明子菜单对象（属于下拉菜单类）
            //如果菜单组中没有名称为name的下拉菜单
            if (parentMenu.CustomizationSection.MenuGroup.PopMenus.IsNameFree(name))
            {
                //为子菜单指定显示名称、标识符和所属的菜单组，别名设为null
                pm = new PopMenu(name, null, tag, parentMenu.CustomizationSection.MenuGroup);
                //为子菜单指定其所属的菜单
                PopMenuRef menuRef = new PopMenuRef(pm, parentMenu, index);
            }
            return pm;//返回子菜单对象
        }

        /// <summary>
        /// 为菜单添加分隔条
        /// </summary>
        /// <param name="parentMenu">下拉菜单</param>
        /// <param name="index">分隔条的位置</param>
        /// <returns>返回添加的分隔条</returns>
        public static PopMenuItem AddSeparator(this PopMenu parentMenu, int index)
        {
            //定义一个分隔条并返回
            return new PopMenuItem(parentMenu, index);
        }

        /// <summary>
        /// 添加工具栏
        /// </summary>
        /// <param name="menuGroup">工具栏所属的菜单组</param>
        /// <param name="name">工具栏的显示名称</param>
        /// <returns>返回添加的工具栏</returns>
        public static Toolbar AddToolbar(this MenuGroup menuGroup, string name)
        {
            Toolbar tb = null;//声明一个工具栏对象
            //如果菜单组中没有名称为name的工具栏
            if (menuGroup.Toolbars.IsNameFree(name))
            {
                //为工具栏指定显示名称和所属的菜单组
                tb = new Toolbar(name, menuGroup);
                //设置工具栏为浮动工具栏
                tb.ToolbarOrient = ToolbarOrient.floating;
                //设置工具栏可见
                tb.ToolbarVisible = ToolbarVisible.show;
            }
            return tb;//返回工具栏对象
        }

        /// <summary>
        /// 向工具栏添加按钮
        /// </summary>
        /// <param name="parent">按钮所属的工具栏</param>
        /// <param name="index">按钮在工具栏上的位置</param>
        /// <param name="name">按钮的显示名称</param>
        /// <param name="macroId">按钮的命令宏的Id</param>
        /// <returns>返回工具栏按钮对象</returns>
        public static ToolbarButton AddToolbarButton(this Toolbar parent, int index, string name, string macroId)
        {
            //创建一个工具栏按钮对象，指定其命令宏Id、显示名称、所属的工具栏和位置
            ToolbarButton button = new ToolbarButton(macroId, name, parent, index);
            return button;//返回工具栏按钮对象
        }

        /// <summary>
        /// 向工具栏添加分隔条
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        public static ToolbarButton AddSeparator(this Toolbar parent, int index)
        {
            return new ToolbarButton(parent, index);
        }

        /// <summary>
        /// 向工具栏添加弹出式工具栏
        /// </summary>
        /// <param name="parent">工具栏所属的父工具栏</param>
        /// <param name="index">弹出式工具栏在父工具栏上的位置</param>
        /// <param name="toolbarRef">弹出式工具栏所引用的工具栏</param>
        public static void AttachToolbarToFlyout(this Toolbar parent, int index, Toolbar toolbarRef)
        {
            //创建一个弹出式工具栏，指定其所属的工具栏和位置
            ToolbarFlyout flyout = new ToolbarFlyout(parent, index);
            //指定弹出式工具栏所引用的工具栏 
            flyout.ToolbarReference = toolbarRef.Name;
            //引用的工具栏初始状态不可见
            toolbarRef.ToolbarVisible = ToolbarVisible.hide;
        }
    }
}

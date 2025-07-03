using Autodesk.AutoCAD.DatabaseServices;

namespace DotNetARX
{
    /// <summary>
    /// 扩展数据操作类
    /// </summary>
    public static class XDataTools
    {
        /// <summary>
        /// 为对象添加扩展数据
        /// </summary>
        /// <param name="id">对象的Id</param>
        /// <param name="regAppName">注册应用程序名</param>
        /// <param name="values">要添加的扩展数据项列表</param>
        public static void AddXData(this ObjectId id, string regAppName, TypedValueList values)
        {
            Database db=id.Database;//获取实体所属的数据库
            //获取数据库的注册应用程序表
            RegAppTable regTable=(RegAppTable)db.RegAppTableId.GetObject(OpenMode.ForWrite);
            //如里不存在名为regAppName的记录，则创建新的注册应用程序表记录
            if (!regTable.Has(regAppName))
            {
                //创建一个注册应用程序表记录用来表示扩展数据
                RegAppTableRecord regRecord=new RegAppTableRecord();
                regRecord.Name = regAppName;//设置扩展数据的名字
                //在注册应用程序表加入扩展数据，并通知事务处理
                regTable.Add(regRecord);
                db.TransactionManager.AddNewlyCreatedDBObject(regRecord, true);
            }
            //以写的方式打开要添加扩展数据的实体
            DBObject obj=id.GetObject(OpenMode.ForWrite);
            //将扩展数据的应用程序名添加到扩展数据项列表的第一项
            values.Insert(0, new TypedValue((int)DxfCode.ExtendedDataRegAppName, regAppName));
            obj.XData = values;//将新建的扩展数据附加到实体中 
            regTable.DowngradeOpen();//为了安全，将应用程序注册表切换为读的状态
        }

        /// <summary>
        /// 获取对象的扩展数据
        /// </summary>
        /// <param name="id">对象的Id</param>
        /// <param name="regAppName">注册应用程序名</param>
        /// <returns>返回获得的扩展数据</returns>
        public static TypedValueList GetXData(this ObjectId id, string regAppName)
        {
            TypedValueList values=new TypedValueList();
            //打开对象
            DBObject obj=id.GetObject(OpenMode.ForRead);
            //获取对象中名为regAppName的扩展数据
            values = obj.GetXDataForApplication(regAppName);
            return values;//返回获得的扩展数据
        }

        /// <summary>
        /// 修改扩展数据
        /// </summary>
        /// <param name="id">对象的Id</param>
        /// <param name="regAppName">注册应用程序名</param>
        /// <param name="code">扩展数据的类型</param>
        /// <param name="oldValue">原来的扩展数据值</param>
        /// <param name="newValue">新的扩展数据值</param>
        public static void ModXData(this ObjectId id, string regAppName, DxfCode code, object oldValue, object newValue)
        {
            // 以写的方式打开对象
            DBObject obj=id.GetObject(OpenMode.ForWrite);
            // 获取regAppName注册应用程序下的扩展数据列表
            TypedValueList xdata=obj.GetXDataForApplication(regAppName);
            for (int i = 0; i < xdata.Count; i++)// 遍历扩展数据列表
            {
                TypedValue tv=xdata[i]; //扩展数据
                //判断扩展数据的类型和值是否满足条件
                if (tv.TypeCode == (short)code && tv.Value.Equals(oldValue))
                {
                    // 设置新的扩展数据值
                    xdata[i] = new TypedValue(tv.TypeCode, newValue);
                    break; //要修改的数据已找到，跳出循环
                }
            }
            obj.XData = xdata; // 覆盖原来的扩展数据，达到修改的目的
            obj.DowngradeOpen();// 为了安全，切换对象为读的状态
        }

        /// <summary>
        /// 删除指定注册应用程序下的扩展数据
        /// </summary>
        /// <param name="id">对象的Id</param>
        /// <param name="regAppName">注册应用程序名</param>
        public static void RemoveXData(this ObjectId id, string regAppName)
        {
            // 以写的方式打开对象
            DBObject obj=id.GetObject(OpenMode.ForWrite);
            // 获取regAppName注册应用程序下的扩展数据列表
            TypedValueList xdata=obj.GetXDataForApplication(regAppName);
            if (xdata != null)// 如果有扩展数据
            {
                // 新建一个TypedValue列表，并只添加注册应用程序名扩展数据项
                TypedValueList newValues=new TypedValueList();
                newValues.Add(DxfCode.ExtendedDataRegAppName, regAppName);
                obj.XData = newValues; //为对象的XData属性重新赋值，从而删除扩展数据 
            }
            obj.DowngradeOpen();// 为了安全，切换对象为读的状态
        }
    }
}

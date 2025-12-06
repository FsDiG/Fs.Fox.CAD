using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Runtime;

namespace NFox.Cad
{
    /// <summary>
    /// 实体对象扩展类
    /// </summary>
    public static class DBObjectEx
    {
        /// <summary>
        /// 打开模式提权
        /// </summary>
        /// <param name="obj">实体对象</param>
        /// <returns>提权类对象</returns>
        public static UpgradeOpenManager UpgradeOpenAndRun(this DBObject obj)
        {
            return new UpgradeOpenManager(obj);
        }

        /// <summary>
        /// 批量克隆对象
        /// </summary>
        /// <typeparam name="T">RXObject</typeparam>
        /// <param name="objs">对象集合</param>
        /// <returns>RXObject类型的迭代器</returns>
        public static IEnumerable<T> Clone<T>(this IEnumerable<T> objs) where T : RXObject
        {
            return objs.Select(obj => obj.Clone() as T);
        }
    }

    /// <summary>
    /// 提权类
    /// </summary>
    public class UpgradeOpenManager : IDisposable
    {
        private DBObject _obj;
        private bool _isNotifyEnabled;
        private bool _isWriteEnabled;

        internal UpgradeOpenManager(DBObject obj)
        {
            _obj = obj;
            _isNotifyEnabled = _obj.IsNotifyEnabled;
            _isWriteEnabled = _obj.IsWriteEnabled;
            if (_isNotifyEnabled)
                _obj.UpgradeFromNotify();
            else if (!_isWriteEnabled)
                _obj.UpgradeOpen();
        }

        #region IDisposable 成员
        /// <summary>
        /// 注销函数
        /// </summary>
        public void Dispose()
        {
            if (_isNotifyEnabled)
                _obj.DowngradeToNotify(_isWriteEnabled);
            else if (!_isWriteEnabled)
                _obj.DowngradeOpen();
        }

        #endregion IDisposable 成员
    }
}
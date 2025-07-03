using System;
using Linq2Acad;
using System.Linq;
using Dreambuild.AutoCAD;
using System.Collections.Generic;
using Autodesk.AutoCAD.DatabaseServices;

namespace ThMEPEngineCore.Service.Hvac
{
    public class ThHvacDbModelManager : IDisposable
    {
        private bool OpenErased { get; set; }
        private Database HostDb { get; set; }
        public ObjectIdCollection Geometries { get; set; }
        public Dictionary<string, List<int>> Models { get; private set; }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="database"></param>
        public ThHvacDbModelManager(Database database, bool openErased = false)
        {
            HostDb = database;
            OpenErased = openErased;
            LoadFromDb(database);
        }

        /// <summary>
        /// 析构函数
        /// </summary>
        public void Dispose()
        {
        }

        /// <summary>
        /// 从图纸中提取风机图块
        /// </summary>
        /// <param name="database"></param>
        private void LoadFromDb(Database database)
        {
            using (AcadDatabase acadDatabase = AcadDatabase.Use(database))
            {
                Geometries = new ObjectIdCollection();
                Models = new Dictionary<string, List<int>>();
                acadDatabase.Database.ModelSpace()
                    .GetEntities<BlockReference>(OpenMode.ForRead, OpenErased)
                    .Where(o => o.IsModel())
                    .ForEachDbObject(o =>
                    {
                        Geometries.Add(o.ObjectId);
                        var number = o.GetModelNumber();
                        var identifier = o.GetModelIdentifier();
                        if (Models.ContainsKey(identifier))
                        {
                            Models[identifier].Add(number);
                        }
                        else
                        {
                            Models.Add(identifier, new List<int>() { number });
                        }
                    });
            }
        }

        /// <summary>
        /// 是否存在风机
        /// </summary>
        /// <param name="identifier"></param>
        /// <returns></returns>
        public bool Contains(string identifier)
        {
            return Models.ContainsKey(identifier);
        }

        /// <summary>
        /// 获取风机图块
        /// </summary>
        /// <param name="identifier"></param>
        /// <returns></returns>
        public ObjectIdCollection GetModels(string identifier)
        {
            var objs = new ObjectIdCollection();
            Geometries.Cast<ObjectId>()
                .Where(o => o.IsModel(identifier))
                .ForEach(o => objs.Add(o));
            return objs;
        }

        /// <summary>
        /// 获取风机编号
        /// </summary>
        /// <param name="identifier"></param>
        /// <returns></returns>
        public List<int> GetModelNumbers(string identifier)
        {
            var numbers = new List<int>();
            Geometries.Cast<ObjectId>()
                .Where(o => o.IsModel(identifier))
                .ForEach(o => numbers.Add(o.GetModelNumber()));
            return numbers;
        }

        /// <summary>
        /// 获取指定编号的风机图块
        /// </summary>
        /// <param name="identifier"></param>
        /// <param name="number"></param>
        /// <returns></returns>
        public ObjectId GetModel(string identifier, int number)
        {
            return GetModels(identifier).Cast<ObjectId>().Where(o => o.GetModelNumber() == number).FirstOrDefault();
        }

        /// <summary>
        /// 删除风机图块
        /// </summary>
        /// <param name="identifier"></param>
        /// <param name="erasing"></param>
        public void EraseModels(string identifier, bool erasing = true)
        {
            using (AcadDatabase acadDatabase = AcadDatabase.Use(HostDb))
            {
                GetModels(identifier).Cast<ObjectId>().ForEach(o =>
                {
                    o.EraseModel(erasing);
                });
            }
        }

        /// <summary>
        /// 清除风机图块
        /// </summary>
        /// <param name="identifier"></param>
        public void RemoveModels(string identifier)
        {
            using (AcadDatabase acadDatabase = AcadDatabase.Use(HostDb))
            {
                GetModels(identifier).Cast<ObjectId>().ForEach(o =>
                {
                    o.RemoveModel();
                });
            }
        }
    }
}

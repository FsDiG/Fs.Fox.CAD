using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace ThMEPEngineCore.CAD
{
    public class ThPropertySet
    {
        public string Section { get; set; }
        public Dictionary<string, string> Properties { get; set; }
        public ThPropertySet()
        {
            Section = string.Empty;
            Properties = new Dictionary<string, string>();
        }
        public static ThPropertySet CreateWithHyperlink(string hyperlink)
        {
            var propertySet = new ThPropertySet();

            // 首先获取第一个分割符“：”
            int index = hyperlink.IndexOf('：');
            if (index == -1)
            {
                return propertySet;
            }
            propertySet.Section = hyperlink.Substring(0, index);

            // 按分割符“__”分割属性
            var properties = Regex.Split(hyperlink.Substring(index + 1, hyperlink.Length - index - 1), "__");
            foreach(var property in properties)
            {
                var keyValue = Regex.Split(property, "：");
                if (ThMEPEngineCoreCommon.BUILDELEMENT_PROPERTIES.Contains(keyValue[0]))
                {
                    propertySet.Properties.Add(keyValue[0], keyValue[1]);
                }
            }

            // 返回属性集
            return propertySet;
        }

        public static ThPropertySet CreateWithHyperlink2(string hyperlink)
        {
            var propertySet = new ThPropertySet();

            // 按分割符“__”分割属性
            var properties = Regex.Split(hyperlink, "__");
            foreach (var property in properties)
            {
                var keyValue = Regex.Split(property, "：");
                if (ThMEPEngineCoreCommon.BUILDELEMENT_PROPERTIES.Contains(keyValue[0]))
                {
                    propertySet.Properties.Add(keyValue[0], keyValue[1]);
                }
            }

            // 返回属性集
            return propertySet;
        }

        /// <summary>
        /// 是否为建筑墙
        /// </summary>
        public bool IsArchWall
        {
            get
            {
                if (Properties.ContainsKey(ThMEPEngineCoreCommon.BUILDELEMENT_PROPERTY_CATEGORY)
                    && Properties.ContainsKey(ThMEPEngineCoreCommon.BUILDELEMENT_PROPERTY_LAYER))
                {
                    return Properties[ThMEPEngineCoreCommon.BUILDELEMENT_PROPERTY_CATEGORY] == ThMEPEngineCoreCommon.BUILDELEMENT_CATEGORY_WALL
                        && Properties[ThMEPEngineCoreCommon.BUILDELEMENT_PROPERTY_LAYER] == ThMEPEngineCoreCommon.BUILDELEMENT_LAYER_WALL;
                }
                return false;
            }
        }

        /// <summary>
        /// 是否为幕墙
        /// </summary>
        public bool IsCurtainWall
        {
            get
            {
                if (Properties.ContainsKey(ThMEPEngineCoreCommon.BUILDELEMENT_PROPERTY_CATEGORY)
                    && Properties.ContainsKey(ThMEPEngineCoreCommon.BUILDELEMENT_PROPERTY_LAYER))
                {
                    return Properties[ThMEPEngineCoreCommon.BUILDELEMENT_PROPERTY_CATEGORY] == ThMEPEngineCoreCommon.BUILDELEMENT_CATEGORY_WALL
                        && Properties[ThMEPEngineCoreCommon.BUILDELEMENT_PROPERTY_LAYER] == ThMEPEngineCoreCommon.BUILDELEMENT_LAYER_CURTAIN_WALL;
                }
                return false;
            }
        }

        /// <summary>
        /// 是否为结构墙（剪力墙）
        /// </summary>
        public bool IsShearWall
        {
            get
            {
                return Properties[ThMEPEngineCoreCommon.BUILDELEMENT_PROPERTY_CATEGORY] == ThMEPEngineCoreCommon.BUILDELEMENT_CATEGORY_S_WALL
                    && Properties[ThMEPEngineCoreCommon.BUILDELEMENT_PROPERTY_LAYER] == ThMEPEngineCoreCommon.BUILDELEMENT_LAYER_WALL;
            }
        }

        /// <summary>
        /// 是否为结构柱
        /// </summary>
        public bool IsSColumn
        {
            get
            {
                if (Properties.ContainsKey(ThMEPEngineCoreCommon.BUILDELEMENT_PROPERTY_CATEGORY)
                    && Properties.ContainsKey(ThMEPEngineCoreCommon.BUILDELEMENT_PROPERTY_LAYER))
                {
                    return Properties[ThMEPEngineCoreCommon.BUILDELEMENT_PROPERTY_CATEGORY] == ThMEPEngineCoreCommon.BUILDELEMENT_CATEGORY_S_COLUMN
                    && Properties[ThMEPEngineCoreCommon.BUILDELEMENT_PROPERTY_LAYER] == ThMEPEngineCoreCommon.BUILDELEMENT_LAYER_WALL;
                }
                return false;
            }
        }

        /// <summary>
        /// 是否为结构梁
        /// </summary>
        public bool IsSBeam
        {
            get
            {
                if (Properties.ContainsKey(ThMEPEngineCoreCommon.BUILDELEMENT_PROPERTY_CATEGORY)
                    && Properties.ContainsKey(ThMEPEngineCoreCommon.BUILDELEMENT_PROPERTY_LAYER))
                {
                    return Properties[ThMEPEngineCoreCommon.BUILDELEMENT_PROPERTY_CATEGORY] == ThMEPEngineCoreCommon.BUILDELEMENT_CATEGORY_S_COLUMN
                    && Properties[ThMEPEngineCoreCommon.BUILDELEMENT_PROPERTY_LAYER] == ThMEPEngineCoreCommon.BUILDELEMENT_LAYER_WALL;
                }
                return false;
            }
        }
        /// <summary>
        /// 是否为结构梁
        /// </summary>
        public bool IsDoor
        {
            get
            {
                if (Properties.ContainsKey(ThMEPEngineCoreCommon.BUILDELEMENT_PROPERTY_CATEGORY))
                {
                    return Properties[ThMEPEngineCoreCommon.BUILDELEMENT_PROPERTY_CATEGORY] == ThMEPEngineCoreCommon.BUILDELEMENT_CATEGORY_DOOR;
                }
                return false;
            }
        }
        /// <summary>
        /// 是否为窗户
        /// </summary>
        public bool IsWindow
        {
            get
            {
                if (Properties.ContainsKey(ThMEPEngineCoreCommon.BUILDELEMENT_PROPERTY_CATEGORY)
                    && Properties.ContainsKey(ThMEPEngineCoreCommon.BUILDELEMENT_PROPERTY_LAYER))
                {
                    return Properties[ThMEPEngineCoreCommon.BUILDELEMENT_PROPERTY_CATEGORY] == ThMEPEngineCoreCommon.BUILDELEMENT_CATEGORY_WINDOW
                        && Properties[ThMEPEngineCoreCommon.BUILDELEMENT_PROPERTY_LAYER] == ThMEPEngineCoreCommon.BUILDELEMENT_LAYER_WINDOW;
                }
                return false;
            }
        }

        /// <summary>
        /// 是否为楼板
        /// </summary>
        public bool IsSlab
        {
            get
            {
                if (Properties.ContainsKey(ThMEPEngineCoreCommon.BUILDELEMENT_PROPERTY_CATEGORY)
                    && Properties.ContainsKey(ThMEPEngineCoreCommon.BUILDELEMENT_PROPERTY_LAYER))
                {
                    return Properties[ThMEPEngineCoreCommon.BUILDELEMENT_PROPERTY_CATEGORY] == ThMEPEngineCoreCommon.BUILDELEMENT_CATEGORY_FLOOR
                        && Properties[ThMEPEngineCoreCommon.BUILDELEMENT_PROPERTY_LAYER] == ThMEPEngineCoreCommon.BUILDELEMENT_LAYER_FLOOR;
                }
                return false;
            }
        }
    }
}

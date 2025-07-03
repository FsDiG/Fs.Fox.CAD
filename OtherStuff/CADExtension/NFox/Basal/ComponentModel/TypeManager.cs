using System;
using System.Collections.Generic;
using System.Reflection;
using System.Xml;
using System.Xml.Serialization;

namespace NFox.ComponentModel
{
    internal class TypeId
    {
        public int Assembly
        { get; set; }

        public int Type
        { get; set; }

        public TypeId(int assembly, int type)
        {
            Assembly = assembly;
            Type = type;
        }

        public static TypeId Parse(string str)
        {
            int[] arr = Array.ConvertAll(str.Split(','), s => int.Parse(s));
            return new TypeId(arr[0], arr[1]);
        }

        public override string ToString()
        {
            return Assembly + "," + Type;
        }
    }

    internal class TypeManager : IXmlSerializable
    {
        private List<string> _assemblyNames = new List<string>();
        private List<string> _typeNames = new List<string>();

        public TypeId GetTypeId(Type type)
        {
            return
                new TypeId
                (
                    GetId(_assemblyNames, type.Assembly.FullName),
                    GetId(_typeNames, type.FullName)
                );
        }

        public Type GetType(TypeId typeId)
        {
            string name =
                Assembly.CreateQualifiedName
                (
                    _assemblyNames[typeId.Assembly],
                    _typeNames[typeId.Type]
                );

            return Type.GetType(name);
        }

        private int GetId(List<string> names, string name)
        {
            if (!names.Contains(name))
                names.Add(name);
            return names.IndexOf(name);
        }

        #region IXmlSerializable 成员

        public System.Xml.Schema.XmlSchema GetSchema()
        {
            return null;
        }

        public void ReadXml(System.Xml.XmlReader reader)
        {
            reader.ReadStartElement();
            if (reader.Name == "Assembly")
            {
                while (reader.Name == "Assembly")
                {
                    reader.ReadStartElement();
                    string name = reader.ReadContentAsString();
                    if (!_assemblyNames.Contains(name))
                        _assemblyNames.Add(name);
                    reader.ReadEndElement();
                }
            }
            if (reader.Name == "Type")
            {
                while (reader.Name == "Type")
                {
                    reader.ReadStartElement();
                    string name = reader.ReadContentAsString();
                    if (!_typeNames.Contains(name))
                        _typeNames.Add(name);
                    reader.ReadEndElement();
                }
            }
            if (reader.NodeType == XmlNodeType.EndElement)
                reader.ReadEndElement();
        }

        public void WriteXml(System.Xml.XmlWriter writer)
        {
            foreach (string name in _assemblyNames)
            {
                writer.WriteStartElement("Assembly");
                writer.WriteString(name);
                writer.WriteEndElement();
            }
            foreach (string name in _typeNames)
            {
                writer.WriteStartElement("Type");
                writer.WriteString(name);
                writer.WriteEndElement();
            }
        }

        #endregion IXmlSerializable 成员
    }
}
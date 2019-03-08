namespace AcadLib.UI.Properties
{
    using System;
    using System.Collections.Generic;
    using System.Xml.Serialization;
    using JetBrains.Annotations;

    /// <summary>
    /// Класс словаря сериализуемый в xml
    /// </summary>
    /// <typeparam name="TValue"></typeparam>
    [XmlRoot("Dictionary")]
    public class XmlSerializableDictionary<TValue> : Dictionary<string, TValue>, IXmlSerializable
    {
        public XmlSerializableDictionary() : base(StringComparer.OrdinalIgnoreCase)
        {
        }

        public XmlSerializableDictionary([NotNull] Dictionary<string, TValue> dict) : base(StringComparer.OrdinalIgnoreCase)
        {
            foreach (var item in dict)
            {
                Add(item.Key, item.Value);
            }
        }

        public System.Xml.Schema.XmlSchema GetSchema()
        {
            return null;
        }

        public void ReadXml(System.Xml.XmlReader reader)
        {
            var keySerializer = new XmlSerializer(typeof(string));
            var valueSerializer = new XmlSerializer(typeof(TValue));
            var wasEmpty = reader.IsEmptyElement;
            reader.Read();
            if (wasEmpty)
                return;
            while (reader.NodeType != System.Xml.XmlNodeType.EndElement)
            {
                reader.ReadStartElement("item");
                reader.ReadStartElement("key");
                var key = (string)keySerializer.Deserialize(reader);
                reader.ReadEndElement();
                reader.ReadStartElement("value");
                var value = (TValue)valueSerializer.Deserialize(reader);
                reader.ReadEndElement();
                Add(key, value);
                reader.ReadEndElement();
                reader.MoveToContent();
            }

            reader.ReadEndElement();
        }

        public void WriteXml(System.Xml.XmlWriter writer)
        {
            var keySerializer = new XmlSerializer(typeof(string));
            var valueSerializer = new XmlSerializer(typeof(TValue));
            foreach (var key in Keys)
            {
                writer.WriteStartElement("item");
                writer.WriteStartElement("key");
                keySerializer.Serialize(writer, key);
                writer.WriteEndElement();
                writer.WriteStartElement("value");
                var value = this[key];
                valueSerializer.Serialize(writer, value);
                writer.WriteEndElement();
                writer.WriteEndElement();
            }
        }
    }
}
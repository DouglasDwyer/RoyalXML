using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Xml;
using System.Xml.Linq;

namespace DouglasDwyer.RoyalXml.Rules
{
    public class KeyValuePairSerializationRule : RoyalSerializationRule
    {
        public override Type SupportedType => typeof(KeyValuePair<,>);

        public override object Deserialize(XElement node, Type type, RoyalXmlSerializer serializer)
        {
            object key = GetChildObject(node.Element("Key"), serializer), value = GetChildObject(node.Element("Value"), serializer);
            return Activator.CreateInstance(type, key, value);
        }

        public override void Serialize(object data, XmlWriter writer, RoyalXmlSerializer serializer)
        {
            Type keyValueType = data.GetType();
            object key = keyValueType.GetProperty("Key").GetValue(data), value = keyValueType.GetProperty("Value").GetValue(data);
            writer.WriteStartElement("Key");
            if(key is null)
            {
                writer.WriteAttributeString("Type", "null");
            }
            else
            {
                writer.WriteAttributeString("Type", key.GetType().AssemblyQualifiedName);
                WriteObjectXml(key, writer, serializer);
            }
            writer.WriteEndElement();
            writer.WriteStartElement("Value");
            if (value is null)
            {
                writer.WriteAttributeString("Type", "null");
            }
            else
            {
                writer.WriteAttributeString("Type", value.GetType().AssemblyQualifiedName);
                WriteObjectXml(value, writer, serializer);
            }
            writer.WriteEndElement();
        }

        private object GetChildObject(XElement node, RoyalXmlSerializer serializer)
        {
            string type = node.Attribute("Type").Value;
            if(type == "null")
            {
                return null;
            }
            else
            {
                return ReadObjectXml(node, Type.GetType(type), serializer);
            }
        }
    }
}

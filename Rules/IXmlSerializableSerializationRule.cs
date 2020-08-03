using System;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace DouglasDwyer.RoyalXml.Rules
{
    public class IXmlSerializableSerializationRule : RoyalSerializationRule
    {
        public override Type SupportedType => typeof(IXmlSerializable);

        public override object Deserialize(XElement node, Type type, RoyalXmlSerializer serializer)
        {
            IXmlSerializable toReturn = (IXmlSerializable)Activator.CreateInstance(type);
            using (XmlReader reader = node.CreateReader())
            {
                toReturn.ReadXml(reader);
                return toReturn;
            }
        }

        public override void Serialize(object data, XmlWriter writer, RoyalXmlSerializer serializer)
        {
            IXmlSerializable serializable = (IXmlSerializable)data;
            serializable.WriteXml(writer);
        }
    }
}

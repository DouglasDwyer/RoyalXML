using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml;
using System.Xml.Linq;

namespace DouglasDwyer.RoyalXml.Rules
{
    public class ICollectionSerializationRule : RoyalSerializationRule
    {
        public override Type SupportedType => typeof(ICollection<>);

        public override object Deserialize(XElement node, Type type, RoyalXmlSerializer serializer)
        {
            Type collectionType = type.GetInterfaces().Where(generic => generic.IsGenericType && generic.GetGenericTypeDefinition() == typeof(ICollection<>)).Single();
            object collection = Activator.CreateInstance(type);
            MethodInfo addToCollectionMethod = collectionType.GetMethod("Add", BindingFlags.Public | BindingFlags.Instance);
            foreach(XElement subnode in node.Nodes())
            {
                string subType = subnode.Attribute("Type").Value;
                if (subType == "null")
                {
                    addToCollectionMethod.Invoke(collection, new object[] { null });
                }
                else
                {
                    addToCollectionMethod.Invoke(collection, new object[] { ReadObjectXml(subnode, Type.GetType(subType), serializer) });
                }
            }
            return collection;
        }

        public override void Serialize(object data, XmlWriter writer, RoyalXmlSerializer serializer)
        {
            foreach(object o in (IEnumerable)data)
            {
                writer.WriteStartElement("ICollectionElement");
                if(o is null)
                {
                    writer.WriteAttributeString("Type", "null");
                }
                else
                {
                    writer.WriteAttributeString("Type", o.GetType().AssemblyQualifiedName);
                    WriteObjectXml(o, writer, serializer);
                }
                writer.WriteEndElement();
            }
        }
    }
}

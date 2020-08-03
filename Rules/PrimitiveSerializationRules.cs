using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Xml;
using System.Xml.Linq;

namespace DouglasDwyer.RoyalXml.Rules
{
    public class ObjectSerializationRule : RoyalSerializationRule
    {
        public override Type SupportedType => typeof(object);

        public override object Deserialize(XElement node, Type type, RoyalXmlSerializer serializer)
        {
            object toReturn = Activator.CreateInstance(type);
            foreach(XElement field in node.Elements())
            {
                type.GetField(field.Name.LocalName, BindingFlags.Public | BindingFlags.Instance)
                    .SetValue(toReturn, ReadObjectXml(field, Type.GetType(field.Attribute("Type").Value), serializer));
            }
            return toReturn;
        }

        public override void Serialize(object data, XmlWriter writer, RoyalXmlSerializer serializer)
        {
            foreach(FieldInfo info in data.GetType().GetFields(BindingFlags.Public | BindingFlags.Instance)) {
                writer.WriteStartElement(info.Name);
                object value = info.GetValue(data);
                if(value is null)
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
        }
    }

    public class BooleanSerializationRule : RoyalSerializationRule
    {
        public override Type SupportedType => typeof(bool);

        public override object Deserialize(XElement node, Type type, RoyalXmlSerializer serializer)
        {
            return bool.Parse(node.Value);
        }

        public override void Serialize(object data, XmlWriter writer, RoyalXmlSerializer serializer)
        {
            writer.WriteString(data.ToString());
        }
    }

    public class ByteSerializationRule : RoyalSerializationRule
    {
        public override Type SupportedType => typeof(byte);

        public override object Deserialize(XElement node, Type type, RoyalXmlSerializer serializer)
        {
            return byte.Parse(node.Value);
        }

        public override void Serialize(object data, XmlWriter writer, RoyalXmlSerializer serializer)
        {
            writer.WriteString(data.ToString());
        }
    }

    public class SignedByteSerializationRule : RoyalSerializationRule
    {
        public override Type SupportedType => typeof(sbyte);

        public override object Deserialize(XElement node, Type type, RoyalXmlSerializer serializer)
        {
            return sbyte.Parse(node.Value);
        }

        public override void Serialize(object data, XmlWriter writer, RoyalXmlSerializer serializer)
        {
            writer.WriteString(data.ToString());
        }
    }

    public class CharSerializationRule : RoyalSerializationRule
    {
        public override Type SupportedType => typeof(char);

        public override object Deserialize(XElement node, Type type, RoyalXmlSerializer serializer)
        {
            return char.Parse(node.Value);
        }

        public override void Serialize(object data, XmlWriter writer, RoyalXmlSerializer serializer)
        {
            writer.WriteString(data.ToString());
        }
    }

    public class DecimalSerializationRule : RoyalSerializationRule
    {
        public override Type SupportedType => typeof(decimal);

        public override object Deserialize(XElement node, Type type, RoyalXmlSerializer serializer)
        {
            return decimal.Parse(node.Value);
        }

        public override void Serialize(object data, XmlWriter writer, RoyalXmlSerializer serializer)
        {
            writer.WriteString(data.ToString());
        }
    }

    public class DoubleSerializationRule : RoyalSerializationRule
    {
        public override Type SupportedType => typeof(double);

        public override object Deserialize(XElement node, Type type, RoyalXmlSerializer serializer)
        {
            return double.Parse(node.Value);
        }

        public override void Serialize(object data, XmlWriter writer, RoyalXmlSerializer serializer)
        {
            writer.WriteString(data.ToString());
        }
    }

    public class FloatSerializationRule : RoyalSerializationRule
    {
        public override Type SupportedType => typeof(float);

        public override object Deserialize(XElement node, Type type, RoyalXmlSerializer serializer)
        {
            return float.Parse(node.Value);
        }

        public override void Serialize(object data, XmlWriter writer, RoyalXmlSerializer serializer)
        {
            writer.WriteString(data.ToString());
        }
    }

    public class IntSerializationRule : RoyalSerializationRule
    {
        public override Type SupportedType => typeof(int);

        public override object Deserialize(XElement node, Type type, RoyalXmlSerializer serializer)
        {
            return int.Parse(node.Value);
        }

        public override void Serialize(object data, XmlWriter writer, RoyalXmlSerializer serializer)
        {
            writer.WriteString(data.ToString());
        }
    }

    public class UnsignedIntSerializationRule : RoyalSerializationRule
    {
        public override Type SupportedType => typeof(uint);

        public override object Deserialize(XElement node, Type type, RoyalXmlSerializer serializer)
        {
            return uint.Parse(node.Value);
        }

        public override void Serialize(object data, XmlWriter writer, RoyalXmlSerializer serializer)
        {
            writer.WriteString(data.ToString());
        }
    }

    public class LongSerializationRule : RoyalSerializationRule
    {
        public override Type SupportedType => typeof(long);

        public override object Deserialize(XElement node, Type type, RoyalXmlSerializer serializer)
        {
            return long.Parse(node.Value);
        }

        public override void Serialize(object data, XmlWriter writer, RoyalXmlSerializer serializer)
        {
            writer.WriteString(data.ToString());
        }
    }

    public class UnsignedLongSerializationRule : RoyalSerializationRule
    {
        public override Type SupportedType => typeof(ulong);

        public override object Deserialize(XElement node, Type type, RoyalXmlSerializer serializer)
        {
            return ulong.Parse(node.Value);
        }

        public override void Serialize(object data, XmlWriter writer, RoyalXmlSerializer serializer)
        {
            writer.WriteString(data.ToString());
        }
    }

    public class ShortSerializationRule : RoyalSerializationRule
    {
        public override Type SupportedType => typeof(short);

        public override object Deserialize(XElement node, Type type, RoyalXmlSerializer serializer)
        {
            return short.Parse(node.Value);
        }

        public override void Serialize(object data, XmlWriter writer, RoyalXmlSerializer serializer)
        {
            writer.WriteString(data.ToString());
        }
    }

    public class UnsignedShortSerializationRule : RoyalSerializationRule
    {
        public override Type SupportedType => typeof(ushort);

        public override object Deserialize(XElement node, Type type, RoyalXmlSerializer serializer)
        {
            return ushort.Parse(node.Value);
        }

        public override void Serialize(object data, XmlWriter writer, RoyalXmlSerializer serializer)
        {
            writer.WriteString(data.ToString());
        }
    }

    public class StringSerializationRule : RoyalSerializationRule
    {
        public override Type SupportedType => typeof(string);

        public override object Deserialize(XElement node, Type type, RoyalXmlSerializer serializer)
        {
            return node.Value;
        }

        public override void Serialize(object data, XmlWriter writer, RoyalXmlSerializer serializer)
        {
            writer.WriteString(data.ToString());
        }
    }
}

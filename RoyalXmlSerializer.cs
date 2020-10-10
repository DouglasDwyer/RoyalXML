using DouglasDwyer.RoyalXml.Rules;
using RoyalXML;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;

namespace DouglasDwyer.RoyalXml
{
    /// <summary>
    /// Represents an extensible, customizable XML serializer.
    /// </summary>
    public class RoyalXmlSerializer
    {
        /// <summary>
        /// The default ruleset to use during serialization if a ruleset is not specified in the <see cref="RoyalXmlSerializer"/> constructor.
        /// </summary>
        public static List<RoyalSerializationRule> DefaultRuleset = new List<RoyalSerializationRule>() {
            new ObjectSerializationRule(),
            new BooleanSerializationRule(),
            new ByteSerializationRule(),
            new SignedByteSerializationRule(),
            new CharSerializationRule(),
            new DateTimeSerializationRule(),
            new DateTimeOffsetSerializationRule(),
            new DecimalSerializationRule(),
            new DoubleSerializationRule(),
            new FloatSerializationRule(),
            new GuidSerializationRule(),
            new IntSerializationRule(),
            new UnsignedIntSerializationRule(),
            new LongSerializationRule(),
            new UnsignedLongSerializationRule(),
            new ShortSerializationRule(),
            new UnsignedShortSerializationRule(),
            new StringSerializationRule(),
            new ICollectionSerializationRule(),
            new IXmlSerializableSerializationRule(),
            new KeyValuePairSerializationRule(),
            new TimeSpanSerializationRule()
        };

        /// <summary>
        /// The ruleset that this serializer is using.
        /// </summary>
        public ICollection<RoyalSerializationRule> CurrentRuleset => IndexedRuleset.Values;
        private readonly Dictionary<Type, RoyalSerializationRule> IndexedRuleset;

        /// <summary>
        /// Creates a new <see cref="RoyalXmlSerializer"/> instance with the default ruleset.
        /// </summary>
        public RoyalXmlSerializer() : this(DefaultRuleset) { }

        /// <summary>
        /// Creates a new <see cref="RoyalXmlSerializer"/> instance with the specified ruleset.
        /// </summary>
        /// <param name="ruleset">A collection of rules to use during serialization.</param>
        public RoyalXmlSerializer(ICollection<RoyalSerializationRule> ruleset)
        {
            IndexedRuleset = ruleset.ToDictionary(x => x.SupportedType);
        }

        /// <summary>
        /// Serializes an object to XML using the current ruleset.
        /// </summary>
        /// <param name="data">The object to convert to XML.</param>
        /// <returns>The string representation of the object.</returns>
        public string Serialize(object data)
        {
            StringBuilder builder = new StringBuilder();
            XmlWriterSettings settings = new XmlWriterSettings { ConformanceLevel = ConformanceLevel.Fragment, Encoding = Encoding.UTF8, Indent = true, IndentChars = "    ", NewLineChars = "\r\n", NewLineHandling = NewLineHandling.Replace, NewLineOnAttributes = true };
            using (XmlWriter writer = XmlWriter.Create(builder, settings))
            {
                writer.WriteStartElement("RoyalXml");
                writer.WriteAttributeString("Version", typeof(RoyalXmlSerializer).Assembly.GetName().Version.ToString());
                if (data is null)
                {
                    writer.WriteAttributeString("Type", "null");
                }
                else
                {
                    writer.WriteAttributeString("Type", data.GetType().AssemblyQualifiedName);
                    WriteObjectXml(data, writer);
                }
                writer.WriteEndElement();
            }
            return builder.ToString();
        }

        /// <summary>
        /// Deserializes an object from XML using the current ruleset.
        /// </summary>
        /// <param name="xml">The XML to convert to an object.</param>
        /// <returns>The deserialized object.</returns>
        public object Deserialize(string xml)
        {
            XDocument document = XDocument.Load(new StringReader(xml));
            if (document.Root.Attribute("Version") is null)
            {
                return RoyalSerializer.XMLToObject(xml);
            }
            else
            {
                string typeName = document.Root.Attribute("Type").Value;
                return typeName == "null" ? null : ReadObjectXml(document.Root, Type.GetType(typeName));
            }
        }

        /// <summary>
        /// Deserializes an object from XML using the current ruleset.
        /// </summary>
        /// <typeparam name="T">The type of object to return.</typeparam>
        /// <param name="xml">The XML to convert to an object.</param>
        /// <returns>The deserialized object.</returns>
        public T Deserialize<T>(string xml)
        {
            return (T)Deserialize(xml);
        }

        /// <summary>
        /// Calculates the rule that should be used to serialize the specified type.
        /// This method iterates over the type's inheritance hierarchy.
        /// If the type exactly matches the supported type of a serialization rule, or its generic variant, that rule is returned.
        /// If not, the method checks for matches between any interfaces that the type implements, and returns the first matching rule.
        /// If it cannot find any such interfaces, it checks the generic definition of each interface, and attempts to match that.
        /// If it cannot find any such interfaces, it proceeds to do the same matching checks with base classes, and base generic classes.
        /// </summary>
        /// <param name="type">The type to check for.</param>
        /// <returns>The rule which best matches.</returns>
        protected RoyalSerializationRule CalculateBestRuleForType(Type type)
        {
            return CheckRulesetForClassMatch(type) ?? CheckRulesetForInterfaceMatch(type) ?? CheckRulesetForInheritedClassMatch(type);
        }

        /// <summary>
        /// Calculates the best serilization rule uses it to convert the specified object to XML.
        /// </summary>
        /// <param name="data">The object to convert to XML.</param>
        /// <param name="writer">The XML writer to use.</param>
        protected void WriteObjectXml(object data, XmlWriter writer)
        {
            CalculateBestRuleForType(data.GetType()).Serialize(data, writer, this);
        }

        /// <summary>
        /// Calculates the best serialization rule for the specified type and uses it to convert the specified XML to an object.
        /// </summary>
        /// <param name="node">The XML node to convert to an object.</param>
        /// <param name="type">The type of object to create.</param>
        /// <returns>The deserialized object.</returns>
        protected object ReadObjectXml(XElement node, Type type)
        {
            return CalculateBestRuleForType(type).Deserialize(node, type, this);
        }

        internal void WriteObject(object data, XmlWriter writer)
        {
            WriteObjectXml(data, writer);
        }

        internal object ReadObject(XElement node, Type type)
        {
            return ReadObjectXml(node, type);
        }

        private RoyalSerializationRule CheckRulesetForClassMatch(Type type)
        {
            if (IndexedRuleset.ContainsKey(type))
            {
                return IndexedRuleset[type];
            }
            else if(type.IsGenericType)
            {
                type = type.GetGenericTypeDefinition();
                if(IndexedRuleset.ContainsKey(type))
                {
                    return IndexedRuleset[type];
                }
            }
            return null;
        }

        private RoyalSerializationRule CheckRulesetForInterfaceMatch(Type type)
        {
            RoyalSerializationRule generic = null;
            foreach(Type inter in type.GetInterfaces())
            {
                if (IndexedRuleset.ContainsKey(type))
                {
                    return IndexedRuleset[type];
                }
                else if(generic is null && inter.IsGenericType)
                {
                    Type g = inter.GetGenericTypeDefinition();
                    if(IndexedRuleset.ContainsKey(g))
                    {
                        generic = IndexedRuleset[g];
                    }
                }
            }
            return generic;
        }

        private RoyalSerializationRule CheckRulesetForInheritedClassMatch(Type type)
        {
            type = type.BaseType;
            while (type != null)
            {
                if(IndexedRuleset.ContainsKey(type))
                {
                    return IndexedRuleset[type];
                }
                else if(type.IsGenericType)
                {
                    Type genericType = type.GetGenericTypeDefinition();
                    if(IndexedRuleset.ContainsKey(genericType))
                    {
                        return IndexedRuleset[genericType];
                    }
                }
                type = type.BaseType;
            }
            return null;
        }
    }
}

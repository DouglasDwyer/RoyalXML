using System;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Linq;

namespace DouglasDwyer.RoyalXml
{
    /// <summary>
    /// Represents a rule used during object serialization/deserialization, dictating how objects of a certain type (and potentially their derivatives) are converted to XML.
    /// </summary>
    public abstract class RoyalSerializationRule
    {
        /// <summary>
        /// The type that this rule is meant to serialize. The type may be generic.
        /// </summary>
        public abstract Type SupportedType { get; }
        /// <summary>
        /// Serializes an object of type <see cref="SupportedType"/>, converting it to XML.
        /// </summary>
        /// <param name="data">The object to serialize.</param>
        /// <param name="writer">The <see cref="XmlWriter"/> to write data to.</param>
        /// <param name="serializer">The serializer that is currently using this rule to perform serialization.</param>
        public abstract void Serialize(object data, XmlWriter writer, RoyalXmlSerializer serializer);
        /// <summary>
        /// Deserializes an object of type <paramref name="type"/>, converting it from XML.
        /// </summary>
        /// <param name="node">The XML node which contains data about the object.</param>
        /// <param name="type">The type of object to create. This object will be assignable from <see cref="SupportedType"/>.</param>
        /// <param name="serializer">The serializer that is currently using this rule to perform deserialization.</param>
        /// <returns>The deserialized object.</returns>
        public abstract object Deserialize(XElement node, Type type, RoyalXmlSerializer serializer);

        /// <summary>
        /// Converts an object to XML using the specified serializer and its ruleset. This should be called when it is necessary to serialize child objects.
        /// </summary>
        /// <param name="data">The child object to write.</param>
        /// <param name="writer">The XML writer to use, currently pointing to the subnode that this object should write its data to.</param>
        /// <param name="serializer">The serializer to use.</param>
        protected void WriteObjectXml(object data, XmlWriter writer, RoyalXmlSerializer serializer)
        {
            serializer.WriteObject(data, writer);
        }

        /// <summary>
        /// Converts XML to an object using the specified serializer and its ruleset. This should be called when it is necessary to deserialize child objects.
        /// </summary>
        /// <param name="node">The node which contains the child object data.</param>
        /// <param name="type">The type of the child object.</param>
        /// <param name="serializer">The serializer to use.</param>
        /// <returns>The deserialized child object.</returns>
        protected object ReadObjectXml(XElement node, Type type, RoyalXmlSerializer serializer)
        {
            return serializer.ReadObject(node, type);
        }
    }
}
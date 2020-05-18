using System;
using System.Reflection;
using System.Text;
using System.Xml;
using System.Collections.Generic;
using System.Xml.Serialization;
using System.IO;
using System.Xml.Linq;
using System.Linq;

namespace RoyalXML
{
    /// <summary>
    /// Provides methods for the serialization of objects to RoyalXML.
    /// </summary>
    public class RoyalSerializer
    {
        /// <summary>
        /// Converts an object to RoyalXML format, preserving types, arrays, and collections.
        /// </summary>
        /// <param name="toSerialize">The object to serialize.</param>
        /// <returns>The serialized data.</returns>
        public static string ObjectToXML(object toSerialize)
        {
            StringBuilder builder = new StringBuilder();
            XmlWriterSettings settings = new XmlWriterSettings { ConformanceLevel = ConformanceLevel.Fragment, Encoding = Encoding.UTF8, Indent = true, IndentChars = "    ", NewLineChars = "\r\n", NewLineHandling = NewLineHandling.Replace, NewLineOnAttributes = true };
            using (XmlWriter writer = XmlWriter.Create(builder, settings))
            {
                writer.WriteStartElement("RoyalXML");
                Dictionary<Type, string> typeDictionary = new Dictionary<Type, string>();
                WriteObjectXML(new RootType(toSerialize), writer, typeDictionary);
                writer.WriteStartElement("Types");
                foreach(Type type in typeDictionary.Keys)
                {
                    writer.WriteStartElement("TypeInfo");
                    writer.WriteAttributeString("type", type.AssemblyQualifiedName);
                    writer.WriteString(typeDictionary[type]);
                    writer.WriteEndElement();
                }
                writer.WriteEndElement();
                writer.WriteEndElement();
            }
            return builder.ToString();
        }

        private static void WriteObjectXML(object data, XmlWriter writer, Dictionary<Type, string> typeDictionary)
        {
            if(data is null)
            {
                return;
            }
            Type dataType = data.GetType();
            if(IsTypeStringConvertable(dataType))
            {
                writer.WriteString(data.ToString());
            }
            else
            {
                if (data is Array)
                {
                    Type arrayType = data.GetType().GetElementType();
                    Array dataArray = data as Array;
                    int[] rank = GetRanks(dataArray);
                    for (int i = 0; i < rank.Length; i++)
                    {
                        writer.WriteAttributeString("ranksize" + i, rank[i].ToString());
                    }
                    int arraySize = Product(i => rank[i], 0, rank.Length - 1);
                    for (int i = 0; i < arraySize; i++)
                    {
                        writer.WriteStartElement("ArrayElement");
                        object subObject = dataArray.GetValue(OneDimensionToLDimensions(i, rank));
                        if (!arrayType.IsValueType)
                        {
                            writer.WriteAttributeString("type", GetShortTypeName(subObject, typeDictionary));
                        }
                        WriteObjectXML(subObject, writer, typeDictionary);
                        writer.WriteEndElement();
                    }
                }
                else if (data is IXmlSerializable)
                {
                    (data as IXmlSerializable).WriteXml(writer);
                }
                else
                {
                    if (dataType.GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(ICollection<>)))
                    {
                        Type collectionType = dataType.GetInterfaces().Where(generic => generic.IsGenericType && generic.GetGenericTypeDefinition() == typeof(ICollection<>)).Single();
                        Array array = Array.CreateInstance(collectionType.GetGenericArguments()[0], (int)collectionType.GetProperty("Count").GetValue(data));

                        collectionType.GetMethod("CopyTo").Invoke(data, new object[] { array, 0 });

                        writer.WriteStartElement("ICollectionData");
                        writer.WriteAttributeString("data", "ICollection");
                        writer.WriteAttributeString("type", GetShortTypeName(array, typeDictionary));
                        WriteObjectXML(array, writer, typeDictionary);
                        writer.WriteEndElement();
                    }
                    foreach (FieldInfo field in dataType.GetFields(BindingFlags.Public | BindingFlags.Instance))
                    {
                        if (field.IsNotSerialized)
                        {
                            continue;
                        }
                        writer.WriteStartElement(field.Name);
                        object value = field.GetValue(data);
                        if (!field.FieldType.IsValueType)
                        {
                            writer.WriteAttributeString("type", GetShortTypeName(value, typeDictionary));
                        }
                        WriteObjectXML(value, writer, typeDictionary);
                        writer.WriteEndElement();
                    }
                }
            }
        }

        private static bool IsTypeStringConvertable(Type type)
        {
            return type.IsPrimitive || type.IsEnum || type == typeof(string) || type == typeof(Guid) || type == typeof(DateTime) || type == typeof(TimeSpan) || type == typeof(DateTimeOffset);
        }

        private static int[] GetRanks(Array array)
        {
            int[] toReturn = new int[array.Rank];
            for(int i = 0; i < toReturn.Length; i++)
            {
                toReturn[i] = array.GetLength(i);
            }
            return toReturn;
        }

        private static int LDimensionsToOneDimension(int[] index, int[] rank)
        {
            return Summation(i => index[i] * Product(n => rank[n], i + 1, rank.Length - 1), 0, rank.Length - 1);
        }

        private static int[] OneDimensionToLDimensions(int index, int[] rank)
        {
            int[] toReturn = new int[rank.Length];
            for(int i = 0; i < toReturn.Length; i++)
            {
                toReturn[i] = OneDimensionToNthDimension(index, i, rank);
            }
            return toReturn;
        }

        private static int OneDimensionToNthDimension(int index, int n, int[] rank)
        {
            return (index / Product(i => rank[i], n + 1, rank.Length - 1)) % rank[n];
        }

        private static int Summation(Func<int, int> function, int start, int end)
        {
            int toReturn = 0;
            for(int i = start; i <= end; i++)
            {
                toReturn += function(i);
            }
            return toReturn;
        }

        private static int Product(Func<int, int> function, int start, int end)
        {
            int toReturn = 1;
            for(int i = start; i <= end; i++)
            {
                toReturn *= function(i);
            }
            return toReturn;
        }

        private static string GetShortTypeName(object type, Dictionary<Type, string> typeDictionary)
        {
            if(type is null)
            {
                return "null";
            }
            Type dataType = type.GetType();
            if (typeDictionary.ContainsKey(dataType))
            {
                return typeDictionary[dataType];
            }
            else if(typeDictionary.ContainsValue(dataType.Name))
            {
                int i = 0;
                while (typeDictionary.ContainsValue(dataType.Name + i))
                {
                    i++;
                }
                return typeDictionary[dataType] = dataType.Name + i;
            }
            else
            {
                return typeDictionary[dataType] = dataType.Name;
            }
        }

        /// <summary>
        /// Converts RoyalXML to an object.
        /// </summary>
        /// <param name="xml">The XML to deserialize.</param>
        /// <returns>The deserialized object.</returns>
        public static object XMLToObject(string xml)
        {
            XDocument document = XDocument.Load(new StringReader(xml));
            Dictionary<string, Type> typeDictionary = new Dictionary<string, Type>();
            foreach(XElement element in document.Root.Element("Types").Descendants())
            {
                if(element.Name != "TypeInfo")
                {
                    throw new XmlException("Input data invalid; expected TypeInfo in Types tag but got " + element.Name);
                }
                typeDictionary.Add(element.Value, Type.GetType(element.Attribute("type").Value));
            }
            return LoadObject(document.Root.Element("Root"), typeof(object), typeDictionary);
        }

        private static object LoadObject(XElement currentDocument, Type fieldType, Dictionary<string, Type> typeDictionary)
        {
            if(IsTypeStringConvertable(fieldType) && !currentDocument.HasElements)
            {
                return StringToValueType(currentDocument.Value, fieldType);
            }
            else
            {
                XAttribute attribute = currentDocument.Attribute("type");
                if (attribute != null && attribute.Value == "null")
                {
                    return null;
                }
                else
                {
                    Type type = attribute is null ? fieldType : typeDictionary[attribute.Value];
                    if (IsTypeStringConvertable(type))
                    {
                        return StringToValueType(currentDocument.Value, type);
                    }
                    else if(type.IsArray)
                    {
                        IEnumerable<XElement> subElements = currentDocument.Elements();
                        int rankSize = type.GetArrayRank();
                        int[] rank = new int[rankSize];
                        for(int i = 0; i < rank.Length; i++)
                        {
                            rank[i] = int.Parse(currentDocument.Attribute("ranksize" + i).Value);
                        }
                        Array toReturn = Array.CreateInstance(type.GetElementType(), rank);
                        int totalSize = Product(i => rank[i], 0, rank.Length - 1);
                        for(int i = 0; i < totalSize; i++)
                        {
                            toReturn.SetValue(LoadObject(subElements.ElementAt(i), type.GetElementType(), typeDictionary), OneDimensionToLDimensions(i, rank));
                        }
                        return toReturn;
                    }
                    else
                    {
                        object toReturn = Activator.CreateInstance(type);

                        foreach (XElement element in currentDocument.Elements())
                        {
                            XAttribute dataAttribute = element.Attribute("data");
                            if(element.Name == "ICollectionData" && dataAttribute != null && element.Attribute("data").Value == "ICollection")
                            {
                                Type collectionType = type.GetInterfaces().Where(generic => generic.IsGenericType && generic.GetGenericTypeDefinition() == typeof(ICollection<>)).Single();
                                Array data = (Array)LoadObject(element, collectionType.GetGenericArguments()[0], typeDictionary);
                                for(int i = 0; i < data.Length; i++)
                                {
                                    collectionType.GetMethod("Add").Invoke(toReturn, new object[] { data.GetValue(i) });
                                }
                            }
                            else
                            {
                                FieldInfo info = type.GetField(element.Name.ToString(), BindingFlags.Public | BindingFlags.Instance);
                                if (info != null)
                                {
                                    info.SetValue(toReturn, LoadObject(element, info.FieldType, typeDictionary));
                                }
                            }
                        }
                        return toReturn;
                    }
                }
            }
        }

        private static object StringToValueType(string data, Type type)
        {
            if (type == typeof(string))
            {
                return data;
            }
            else if (type.IsPrimitive)
            {
                if (type == typeof(bool))
                    return bool.Parse(data);
                else if (type == typeof(byte))
                    return byte.Parse(data);
                else if (type == typeof(sbyte))
                    return sbyte.Parse(data);
                else if (type == typeof(char))
                    return char.Parse(data);
                else if (type == typeof(decimal))
                    return decimal.Parse(data);
                else if (type == typeof(double))
                    return double.Parse(data);
                else if (type == typeof(float))
                    return float.Parse(data);
                else if (type == typeof(int))
                    return int.Parse(data);
                else if (type == typeof(uint))
                    return uint.Parse(data);
                else if (type == typeof(long))
                    return long.Parse(data);
                else if (type == typeof(ulong))
                    return ulong.Parse(data);
                else if (type == typeof(short))
                    return short.Parse(data);
                else if (type == typeof(ushort))
                    return ushort.Parse(data);
            }
            else if (type.IsEnum)
                return Enum.Parse(type, data);
            else if (type == typeof(Guid))
                return Guid.Parse(data);
            else if (type == typeof(DateTime))
                return DateTime.Parse(data);
            else if (type == typeof(DateTimeOffset))
                return DateTimeOffset.Parse(data);
            else if (type == typeof(TimeSpan))
                return TimeSpan.Parse(data);
            throw new InvalidOperationException("The program has reached an unsupported state.");
        }

        /// <summary>
        /// Converts RoyalXML to an object.
        /// </summary>
        /// <typeparam name="T">The type to return.</typeparam>
        /// <param name="xml">The XML to deserialize.</param>
        /// <returns>The deserialized object.</returns>
        public static T XMLToObject<T>(string xml)
        {
            return (T)XMLToObject(xml);
        }

        private sealed class RootType
        {
            public object Root;

            public RootType(object root)
            {
                Root = root;
            }
        }
    }
}

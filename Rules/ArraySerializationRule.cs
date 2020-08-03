using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;

namespace DouglasDwyer.RoyalXml.Rules
{
    public class ArraySerializationRule : RoyalSerializationRule
    {
        public override Type SupportedType => typeof(Array);

        public override object Deserialize(XElement node, Type type, RoyalXmlSerializer serializer)
        {
            IEnumerable<XElement> subElements = node.Elements();
            int rankSize = type.GetArrayRank();
            int[] rank = new int[rankSize];
            for (int i = 0; i < rank.Length; i++)
            {
                rank[i] = int.Parse(node.Attribute("RankSize" + i).Value);
            }
            Array toReturn = Array.CreateInstance(type.GetElementType(), rank);
            int totalSize = Product(i => rank[i], 0, rank.Length - 1);
            for (int i = 0; i < totalSize; i++)
            {
                XElement elementNode = subElements.ElementAt(i);
                string typeString = elementNode.Attribute("Type").Value;
                object toSet = null;
                if(typeString != "null")
                {
                    toSet = ReadObjectXml(elementNode, Type.GetType(typeString), serializer);
                }
                toReturn.SetValue(toSet, OneDimensionToLDimensions(i, rank));
            }
            return toReturn;
        }

        public override void Serialize(object data, XmlWriter writer, RoyalXmlSerializer serializer)
        {
            Type arrayType = data.GetType().GetElementType();
            Array dataArray = data as Array;
            int[] rank = GetRanks(dataArray);
            for (int i = 0; i < rank.Length; i++)
            {
                writer.WriteAttributeString("RankSize" + i, rank[i].ToString());
            }
            int arraySize = Product(i => rank[i], 0, rank.Length - 1);
            for (int i = 0; i < arraySize; i++)
            {
                writer.WriteStartElement("Element");
                object subObject = dataArray.GetValue(OneDimensionToLDimensions(i, rank));
                if(subObject is null)
                {
                    writer.WriteAttributeString("Type", "null");
                }
                else
                {
                    writer.WriteAttributeString("Type", subObject.GetType().AssemblyQualifiedName);
                    WriteObjectXml(subObject, writer, serializer);
                }
                writer.WriteEndElement();
            }
        }

        private static int[] GetRanks(Array array)
        {
            int[] toReturn = new int[array.Rank];
            for (int i = 0; i < toReturn.Length; i++)
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
            for (int i = 0; i < toReturn.Length; i++)
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
            for (int i = start; i <= end; i++)
            {
                toReturn += function(i);
            }
            return toReturn;
        }

        private static int Product(Func<int, int> function, int start, int end)
        {
            int toReturn = 1;
            for (int i = start; i <= end; i++)
            {
                toReturn *= function(i);
            }
            return toReturn;
        }
    }
}

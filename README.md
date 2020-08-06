[![Nuget](https://img.shields.io/nuget/v/DouglasDwyer.RoyalXml)](https://www.nuget.org/packages/DouglasDwyer.RoyalXml)
[![Downloads](https://img.shields.io/nuget/dt/DouglasDwyer.RoyalXml)](https://www.nuget.org/packages/DouglasDwyer.RoyalXml)

# RoyalXml
RoyalXml is a simple XML serializer for .NET that supports polymorphism, multidimensional arrays, and collections.

### Installation
RoyalXml can be obtained as a Nuget package. To import it into your project, either download `DouglasDwyer.RoyalXml` from the Visual Studio package manager or run the command `Install-Package DouglasDwyer.RoyalXml` using the package manager console.

### How to use
Usage of RoyalXml with the default ruleset is quite simple. The following code snippet serializes and then deserializes an object using the default serialization ruleset:
```csharp
RoyalXmlSerializer serializer = new RoyalXmlSerializer();
Dictionary<string, string> c = new Dictionary<string, string>() { { "hi", "bye" }, { "why", "cry" } };
string xml = serializer.Serialize(c);
Dictionary<string, string> d = serializer.Deserialize<Dictionary<string, string>>(xml);
```
Because different serializers can have different rulesets, it is necessary that serializers be instance-based and not static or singletons.

### The default serialization ruleset

By default, RoyalXml comes with a builtin serialization ruleset. The default ruleset supports primitive types, objects in hierarchies, polymorphism, multidimensional arrays, collections, and dictionaries. Object references are not preserved during serialization, though it is possible to implement such behavior. This means that cyclical references are also unsupported.

### Serialization rulesets
To perform serialization/deserialization in a configurable manner, RoyalXml centers around the use of serialization rulesets, which are passed into the constructor of `RoyalXmlSerializer`. If no ruleset is specified, then `RoyalXmlSerializer.DefaultRuleset` is used. Serialization rulesets are collections of `RoyalSerializationRule`s which specify how to convert various types (and potentially their derived types). An example of a serialization rule, the `ICollectionSerializationRule` class, is given below:
```csharp
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
```
The `SupportedType` property specifies that this rule can be applied to any `ICollection<>`. Note that this is a generic type - generic types like these are supported. During serialization/deseriation, the elements of the collection are enumerated, and `WriteObjectXml` and `ReadObjectXml` are called to serialize/deserialize subobjects. These methods make calls to the `RoyalXmlSerializer`, which determines the best serialization rule to use for any given object/type.

### The best-fitting serialization rule
When a `RoyalXmlSerializer` is asked to serialize/deserialize a given type, it iterates through the type's inheritance hierarchy, attempting to find a type contained in the serialization ruleset. The serializer attempts to match a best-fitting rule by checking the given type's inheritance chain in the following order:
+ If the given type matches a rule's type, that rule is used
+ If the given type is generic, and a rule's type matches the generic definition, that rule is used
+ If one of the given type's interfaces matches a rule's type, that rule is used
+ If one of the given type's interfaces is generic, and a rule's type matches the generic definition, that rule is used
+ For each of the given type's base types, if the base type matches a rule, then that rule is used. If the base type is generic, and the generic type definition matches a rule, then that rule is used
These rules make RoyalXml quite powerful, as it is easy to customize or extend behavior to various classes.

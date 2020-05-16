# RoyalXML
RoyalXML is a simple XML serializer for .NET that supports polymorphism, multidimensional arrays, and collections.

### How to use
Reference `RoyalXML.dll` (which can be found the `Debug` folder) in your project.
Use `RoyalXML.RoyalSerializer.ObjectToXML(obj)` to convert `obj` to an XML-formatted `string`.
Use `RoyalXML.RoyalSerializer.XMLToObject(str)` to convert string `str` back to an object or `RoyalXML.RoyalSerializer.XMLToObject<T>(str)` to convert to type `T`.

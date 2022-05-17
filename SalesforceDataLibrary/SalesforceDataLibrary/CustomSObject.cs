using System;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace SalesforceDataLibrary
{
    public class CustomSObject : Dictionary<string, object>, IXmlSerializable
    {
        public XmlSchema GetSchema() => null;

        public void ReadXml(XmlReader reader) => throw new NotImplementedException();

        public void WriteXml(XmlWriter writer)
        {
            writer.WriteRaw("<sObject>");
            foreach (var entry in this)
            {
                if (entry.Value is IXmlSerializable value)
                {
                    writer.WriteRaw($"<{entry.Key}>");
                    value.WriteXml(writer);
                    writer.WriteRaw($"</{entry.Key}>");
                }
                else
                {
                    writer.WriteRaw(entry.Value == null
                        ? $@"<{entry.Key} xsi:nil=""true""/>"
                        : $"<{entry.Key}>{entry.Value}</{entry.Key}>");
                }
            }
            writer.WriteRaw("</sObject>");
        }
    }
}

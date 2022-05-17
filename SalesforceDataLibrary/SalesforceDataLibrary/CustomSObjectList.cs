using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using Salesforce.Common.Models.Xml;

namespace SalesforceDataLibrary
{
    [XmlRoot(ElementName = "sObjects", Namespace = "http://www.force.com/2009/06/asyncapi/dataload")]
    public sealed class CustomSObjectList<T> : List<T>, ISObjectList<T>, IList<T>, ICollection<T>, IEnumerable<T>, IEnumerable, IXmlSerializable
    {
        public XmlSchema GetSchema() => (XmlSchema)null;

        //public StringBuilder Output; 

        public void ReadXml(XmlReader reader) => throw new NotImplementedException();

        public void WriteXml(XmlWriter writer)
        {
            //Output = new StringBuilder();

            XmlSerializer xmlSerializer;
            if (!XmlSerializerCache.Instance.XmlSerializerDictionary.TryGetValue(typeof(T).FullName, out xmlSerializer))
            {
                xmlSerializer = new XmlSerializer(typeof(T), new XmlRootAttribute("sObject"));
                XmlSerializerCache.Instance.XmlSerializerDictionary.Add(typeof(T).FullName, xmlSerializer);
            }

            writer.WriteAttributeString("xmlns", "xsi", null, "http://www.w3.org/2001/XMLSchema-instance");
            foreach (T obj in this)
            {      
                StringBuilder output = new StringBuilder();
                XmlWriterSettings settings = new XmlWriterSettings() { OmitXmlDeclaration = true };
                XmlSerializerNamespaces namespaces = new XmlSerializerNamespaces();

                if (obj is IXmlSerializable xmlSerializable)
                {
                    xmlSerializable.WriteXml(writer);

                    //StringBuilder output = new StringBuilder();
                    ////XmlSerializer xmlSerializer = new XmlSerializer(typeof(T), new XmlRootAttribute("sObject"));
                    //XmlWriterSettings settings = new XmlWriterSettings() { OmitXmlDeclaration = true };
                    //XmlSerializerNamespaces namespaces = new XmlSerializerNamespaces();
                    namespaces.Add(string.Empty, string.Empty);
                    using (XmlWriter xmlWriter = XmlWriter.Create(output, settings))
                        xmlSerializer.Serialize(xmlWriter, obj, namespaces);
                    //Output.AppendLine(output.ToString());

                }
                else
                {
                    //XmlSerializer xmlSerializer = new XmlSerializer(typeof(T), new XmlRootAttribute("sObject"));
                    

                    namespaces.Add(string.Empty, string.Empty);
                    using (XmlWriter xmlWriter = XmlWriter.Create(output, settings))
                        xmlSerializer.Serialize(xmlWriter, obj, namespaces);
                    writer.WriteRaw(output.ToString());
                    //Output.AppendLine(output.ToString());
                }
            }
        }
    }
}

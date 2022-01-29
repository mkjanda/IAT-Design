﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using System.Xml.Schema;

namespace IATClient.ResultData
{
    [Serializable]
    [XmlElement(ElementName ="ItemSlideData", Form = XmlSchemaForm.Unqualified, IsNullable = false, Type = typeof(ItemSlideData))]
    public class ItemSlideData
    {
        [XmlElement("SessionId")]
        public String SessionId { get; set; }
        [XmlElement("DeploymentId")]
        public String DeploymentId { get; set; }
        [XmlArray("Resources")]
        [XmlArrayItem("Resource", Type=typeof(ResourceEntry))]
        public ResourceEntry[] Resources { get; set; }
        [XmlElement("Reference", Type=typeof(ReferenceEntry))]
        public ReferenceEntry[] ReferenceEntries { get; set; }
    }

    [Serializable]
    public class ResourceEntry
    {
        [XmlElement(ElementName = "ResourceName", IsNullable = false, Form = XmlSchemaForm.Unqualified, Type = typeof(String))]
        public String ResourceName { get; set; }
        [XmlAttribute(AttributeName = "Size", Form = XmlSchemaForm.Unqualified, Type=typeof(int))]
        public int Size { get; set; }
    }

    [Serializable]
    public class ReferenceEntry
    {
        [XmlElement("ResourceName", Type = typeof(String))]
        public String ResourceName { get; set; }
        [XmlElement("ReferenceName", Type = typeof(String))]
        public String[] ReferenceName { get; set; }
    }

}

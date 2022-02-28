using System.Xml;

namespace IATClient
{
    public interface IStoredInXml
    {
        /// <summary>
        /// Writes the objects data to an XmlTextWriter
        /// </summary>
        /// <param name="writer">The XmlTextWriter object to use for output</param>
        void WriteToXml(XmlTextWriter writer);

        /// <summary>
        /// Load's the object's data from an XmlNode
        /// </summary>
        /// <param name="node">The XmlNode object to load data from</param>
        /// <returns>"true" on success, otherwise "false"</returns>
        bool LoadFromXml(XmlNode node);
    }
}

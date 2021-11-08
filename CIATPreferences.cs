using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.IO.Packaging;
using System.Text;
using System.Xml;
using System.Xml.Linq;

namespace IATClient
{
    /// <summary>
    /// CIATPreferences contains selections the user makes that are remembered within a given configuration file.
    /// </summary>
    public class CIATPreferences : IPackagePart
    {
        // the default font family
        static private String DefaultFontFamily = System.Drawing.SystemFonts.DialogFont.FontFamily.Name;

        // the default font color
        static private System.Drawing.Color DefaultFontColor = System.Drawing.Color.FromName(Properties.Resources.sDefaultTextFontColor);

        // the default font size, in points
        static private float DefaultFontSize = Convert.ToSingle(Properties.Resources.sDefaultTextFontSize);

        static private float DefaultConjunctionFontSize = Convert.ToSingle(Properties.Resources.sDefaultConjunctionFontSize);

        static private System.Drawing.Color DefaultConjunctionFontColor = System.Drawing.Color.FromName(Properties.Resources.sDefaultConjunctionFontColor);

        static private float DefaultStimulusFontSize = Convert.ToSingle(Properties.Resources.sDefaultStimulusFontSize);

        /// <summary>
        /// a class to store font preferences
        /// </summary>
        public class CFontPreferences 
        {
            public DIText.UsedAs UsedAs { get; private set; }
            public String FontFamily;
            public float FontSize;
            public System.Drawing.Color FontColor;

            public CFontPreferences() { }

            public CFontPreferences(DIText.UsedAs usedAs)
            {
                UsedAs = usedAs;
                if (usedAs == DIText.UsedAs.Conjunction)
                {
                    FontSize = CIATPreferences.DefaultConjunctionFontSize;
                    FontFamily = CIATPreferences.DefaultFontFamily;
                    FontColor = CIATPreferences.DefaultFontColor;
                }
                else if (usedAs == DIText.UsedAs.Stimulus)
                {
                    FontSize = CIATPreferences.DefaultStimulusFontSize;
                    FontFamily = CIATPreferences.DefaultFontFamily;
                    FontColor = CIATPreferences.DefaultFontColor;
                }
                else
                {
                    FontSize = CIATPreferences.DefaultFontSize;
                    FontFamily = CIATPreferences.DefaultFontFamily;
                    FontColor = CIATPreferences.DefaultFontColor;
                }
            }
            public void Load(XElement element)
            {
                UsedAs = DIText.UsedAs.FromString(element.Attribute("UsedAs").Value);
                FontFamily = element.Element(XName.Get("FontFamily")).Value;
                FontSize = Convert.ToSingle(element.Element(XName.Get("FontSize")).Value);
                FontColor = Color.FromName(element.Element(XName.Get("FontColor")).Value);
            }

            public void Save(XElement elem)
            {
                XmlWriter xWriter = elem.CreateWriter();
                xWriter.WriteAttributeString("UsedAs", UsedAs.ToString());
                xWriter.WriteElementString("FontFamily", FontFamily);
                xWriter.WriteElementString("FontSize", FontSize.ToString());
                xWriter.WriteElementString("FontColor", FontColor.ToString());
                xWriter.WriteEndElement();
                xWriter.Close();
            }
        }

        public Dictionary<DIText.UsedAs, CFontPreferences> FontPreferences = new Dictionary<DIText.UsedAs, CFontPreferences>();

        public CIATPreferences()
        {
            FontPreferences[DIText.UsedAs.IatBlockInstructions] = new CFontPreferences(DIText.UsedAs.IatBlockInstructions);
            FontPreferences[DIText.UsedAs.Conjunction] = new CFontPreferences(DIText.UsedAs.Conjunction);
            FontPreferences[DIText.UsedAs.ContinueInstructions] = new CFontPreferences(DIText.UsedAs.ContinueInstructions);
            FontPreferences[DIText.UsedAs.MockItemInstructions] = new CFontPreferences(DIText.UsedAs.MockItemInstructions);
            FontPreferences[DIText.UsedAs.TextInstructionsScreen] = new CFontPreferences(DIText.UsedAs.TextInstructionsScreen);
            FontPreferences[DIText.UsedAs.ResponseKey] = new CFontPreferences(DIText.UsedAs.ResponseKey);
            FontPreferences[DIText.UsedAs.Stimulus] = new CFontPreferences(DIText.UsedAs.Stimulus);
            FontPreferences[DIText.UsedAs.KeyedInstructionsScreen] = new CFontPreferences(DIText.UsedAs.KeyedInstructionsScreen);
        }

        public CIATPreferences()
        {
            this.URI = CIAT.SaveFile.CreatePart()
        }

        public CIATPreferences(Uri uri)
        {
            this.URI = uri;
            Load(uri);
        }

        /// <summary>
        /// Does no work. Returns true. The preferences cannot be invalid.
        /// </summary>
        /// <returns>"true"</returns>
        public bool IsValid()
        {
            return true;
        }

        public bool LoadFromXml(XmlNode node)
        {
            return true;
            /*
            // ensure the correct node name and child node count
            if (node.Name != "Preferences")
                return false;
            if (node.ChildNodes.Count != 2)
                return false;

            // load font preferences
            for (int ctr = 0; ctr < node.ChildNodes[0].ChildNodes.Count; ctr++)
            {
                CFontPreferences.EUsedFor UsedFor = (CFontPreferences.EUsedFor)Convert.ToInt32(node.ChildNodes[0].ChildNodes[ctr].Attributes["UsedFor"].InnerText);
                FontPreferences[UsedFor].LoadFromXml(node.ChildNodes[0].ChildNodes[ctr]);
            }

            // load other preferences
            _DefaultCombinedStimulusVerticalPadding = Convert.ToInt32(node.ChildNodes[1].InnerText);
            // success
            return true;*/
        }

        public CFontPreferences this[DIText.UsedAs usedAs]
        {
            get
            {
                return FontPreferences[usedAs];
            }
        }

        public Type BaseType
        {
            get
            {
                return typeof(CIATPreferences);
            }
        }

        public Uri URI { get; set; }
        public String MimeType
        {
            get
            {
                return "text/xml+" + typeof(CIATPreferences).ToString();
            }
        }

        public void Save()
        {
            XDocument xDoc = new XDocument();
            xDoc.Document.Add(new XElement(typeof(CIATPreferences).ToString()));
            foreach (CFontPreferences fp in FontPreferences.Values)
            {
                XElement fpElem = new XElement(typeof(CFontPreferences).ToString());
                fp.Load(fpElem);
                xDoc.Root.Add(fpElem);
            }
            Stream s = CIAT.SaveFile.GetWriteStream(this);
            xDoc.Save(s);
            s.Close();
        }

        public void Load(Uri uri)
        {
            this.URI = uri;
            Stream s = CIAT.SaveFile.GetReadStream(this);
            XDocument xDoc = XDocument.Load(s);
            s.Close();
            FontPreferences.Clear();
            foreach (XElement elem in xDoc.Root.Elements(typeof(CFontPreferences).ToString()))
            {
                CFontPreferences fp = new CFontPreferences();
                fp.Load(elem);
                FontPreferences[fp.UsedAs] = fp;
            }
        }
    }
}

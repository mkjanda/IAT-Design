using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Linq;
using System.Net;
using Saxon.Api;
using IATClient.IATConfig;

namespace IATClient
{
    public class CSurvey : IContentsItem, IDisposable
    {
        public class ItemList : IEnumerable<CSurveyItem>
        {
            public readonly List<String> rItemIdList = new List<String>();
            private CSurvey Survey { get; set; }
            private readonly Dictionary<String, Uri> SurveyItemDictionary = new Dictionary<string, Uri>();

            public IEnumerator<CSurveyItem> GetEnumerator()
            {
                return rItemIdList.Select(rId => SurveyItemDictionary[rId]).Select(u => CIAT.SaveFile.GetSurveyItem(u)).ToList().GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return this.GetEnumerator();
            }

            public ItemList(CSurvey s)
            {
                Survey = s;
            }

            public int Count
            {
                get
                {
                    return rItemIdList.Count;
                }
            }

            public void AddItemByRel(String rId)
            {
                rItemIdList.Add(rId);
                SurveyItemDictionary[rId] = CIAT.SaveFile.GetRelationship(Survey, rId).TargetUri;
                CSurveyItem item = CIAT.SaveFile.GetSurveyItem(SurveyItemDictionary[rId]);
                if (item.rSurveyId == null)
                    item.rSurveyId = CIAT.SaveFile.GetRelationship(item, Survey);
            }

            public void Add(CSurveyItem si)
            {
                String rId = CIAT.SaveFile.CreateRelationship(typeof(CSurvey), si.BaseType, Survey.URI, si.URI);
                SurveyItemDictionary[rId] = si.URI;
                rItemIdList.Add(rId);
                si.rSurveyId = CIAT.SaveFile.CreateRelationship(si.BaseType, typeof(CSurvey), si.URI, Survey.URI);
            }

            public void AddRange(IEnumerable<CSurveyItem> sis)
            {
                foreach (CSurveyItem si in sis)
                {
                    String rId = CIAT.SaveFile.CreateRelationship(typeof(CSurvey), typeof(CSurveyItem), Survey.URI, si.URI);
                    SurveyItemDictionary[rId] = si.URI;
                    rItemIdList.Add(rId);
                    si.rSurveyId = CIAT.SaveFile.CreateRelationship(si.BaseType, typeof(CSurvey), si.URI, Survey.URI);
                }
            }

            public void Insert(int ndx, CSurveyItem si)
            {
                String rId = CIAT.SaveFile.CreateRelationship(typeof(CSurvey), typeof(CSurveyItem), Survey.URI, si.URI);
                SurveyItemDictionary[rId] = si.URI;
                rItemIdList.Insert(ndx, rId);
                si.rSurveyId = CIAT.SaveFile.CreateRelationship(si.BaseType, typeof(CSurvey), si.URI, Survey.URI);
            }

            public void Remove(CSurveyItem si)
            {
                String rId = SurveyItemDictionary.Where(kv => kv.Value.Equals(si.URI)).Select(kv => kv.Key).FirstOrDefault();
                if (rId == null)
                    return;
                CIAT.SaveFile.DeleteRelationship(Survey.URI, rId);
                CIAT.SaveFile.DeleteRelationship(si.URI, si.rSurveyId);
                si.rSurveyId = null;
                rItemIdList.Remove(rId);
                SurveyItemDictionary.Remove(rId);
            }

            public int IndexOf(CSurveyItem si)
            {
                String rId = SurveyItemDictionary.Where(kv => kv.Value.Equals(si.URI)).Select(kv => kv.Key).FirstOrDefault();
                if (rId == null)
                    return -1;
                return rItemIdList.IndexOf(rId);
            }

            public CSurveyItem this[int ndx]
            {
                get
                {
                    return CIAT.SaveFile.GetSurveyItem(SurveyItemDictionary[rItemIdList[ndx]]);
                }
            }

            public void Clear()
            {
                foreach (String rId in rItemIdList)
                {
                    CSurveyItem si = CIAT.SaveFile.GetSurveyItem(CIAT.SaveFile.GetRelationship(Survey, rId).TargetUri);
                    CIAT.SaveFile.DeleteRelationship(Survey.URI, rId);
                    CIAT.SaveFile.DeleteRelationship(si.URI, si.rSurveyId);
                    si.rSurveyId = null;
                    CIAT.SaveFile.GetSurveyItem(SurveyItemDictionary[rId]).Dispose();
                }
                rItemIdList.Clear();
                SurveyItemDictionary.Clear();
            }
        }

        public int Index { get; private set; }
        public Uri URI { get; set; }
        public Type BaseType { get { return typeof(CSurvey); } }
        public static String _MimeType { get { return "text/xml+" + typeof(CSurvey).ToString(); } }
        public String MimeType { get { return _MimeType; } }
        public bool IsSurvey { get { return true; } }
        private bool _IsDisposed = false;
        public enum EOrdinality { Before, After };
        public ItemList Items { get; private set; } = null;
        private List<Uri> SurveyImageUris = new List<Uri>();
        private String _SurveyName = String.Empty;
        private AlternationGroup AltGroup;
        public readonly String rIatId;
        public bool IsScored { get; set; }
        public decimal Timeout { get; set; }
        public bool IsHeaderItem { get { return true; } }
        public EOrdinality Ordinality { get; set; }
        public String Name { get; private set; }
        private List<Control> PreviewControls { get; set; } = new List<Control>();
        private static Processor xsltProcessor;
        private static XsltCompiler xsltCompiler;
        private static XsltExecutable xsltExecutable;
        private static XsltTransformer xsltTransformer;
        private static WebBrowser SurveyPreview = null;
        public static void CompileXSLT()
        {
            Task.Run(() =>
            {
                try
                {
                    xsltProcessor = new Processor();
                    xsltCompiler = xsltProcessor.NewXsltCompiler();
                    xsltExecutable = xsltCompiler.Compile(new StringReader(Properties.Resources.SurveyPage));
                    xsltTransformer = xsltExecutable.Load();
                }
                catch (Exception ex)
                {
                    int n = 1;
                }
            });
        }

        public bool HasCaption
        {
            get
            {
                if (Items.Count == 0)
                    return false;
                if (Items[0].IsCaption)
                    return true;
                return false;
            }
        }

        public String FileNameBase
        {
            get
            {
                String str = String.Empty;
                char[] cAry = Name.ToCharArray();
                byte[] b = new byte[1];
                for (int ctr = 0; ctr < cAry.Length; ctr++)
                {
                    if (!Char.IsLetterOrDigit(cAry[ctr]))
                    {
                        System.Text.Encoding.ASCII.GetEncoder().GetBytes(cAry, ctr, 1, b, 0, true);
                        str += b[0].ToString("X2");
                    }
                    else
                        str += cAry[ctr];
                }
                return str;
            }
        }

        public int NumOnceOnlyResponses
        {
            get
            {
                int nOnceOnly = 0;
                for (int ctr = 0; ctr < Items.Count; ctr++)
                    if (!Items[ctr].IsCaption)
                        if (Items[ctr].Response.ResponseType == CResponse.EResponseType.FixedDig)
                            if (Items[ctr].IsScored)
                                if (((CFixedDigitResponseObject)Items[ctr].DefinedResponse).IsOneUse)
                                    nOnceOnly++;
                return nOnceOnly;
            }
        }

        public CSurvey(Uri uri)
        {
            Items = new ItemList(this);
            this.URI = uri;
            rIatId = CIAT.SaveFile.GetRelationshipsByType(CIAT.SaveFile.IAT.URI, typeof(CIAT), BaseType).Where(pr => pr.TargetUri.Equals(uri)).Select(pr => pr.Id).First();
            Load();
        }

        public CSurvey(EOrdinality ordinality)
        {
            Items = new ItemList(this);
            URI = CIAT.SaveFile.Register(this);
            rIatId = CIAT.SaveFile.CreateRelationship(typeof(CIAT), typeof(CSurvey), CIAT.SaveFile.IAT.URI, this.URI);
            Ordinality = ordinality;
            Index = CIAT.SaveFile.IAT.SurveyIndicies.GetAvailableIndex(this);
            Name = String.Format("Survey #{0}", Index);
            CIAT.SaveFile.ActivityLog.LogEvent(ActivityLog.EventType.Create, URI);
        }

        public CSurvey(EOrdinality ordinality, int index)
        {
            URI = CIAT.SaveFile.Register(this);
            rIatId = CIAT.SaveFile.CreateRelationship(typeof(CIAT), typeof(CSurvey), CIAT.SaveFile.IAT.URI, this.URI);
            Ordinality = ordinality;
            Index = index;
            Name = String.Format("Survey #{0}", Index);
            CIAT.SaveFile.IAT.SurveyIndicies.AddSurvey(URI, index);
            CIAT.SaveFile.ActivityLog.LogEvent(ActivityLog.EventType.Create, URI);
        }

        public void Save()
        {
            XDocument xDoc = new XDocument();
            xDoc.Add(new XElement("Survey", new XAttribute("Name", Name), new XAttribute("Index", Index.ToString()), new XAttribute("IndexInContents", IndexInContents.ToString()), new XAttribute("Timeout", Timeout.ToString()),
                new XAttribute("Ordinality", Ordinality.ToString())));
            foreach (String rId in Items.rItemIdList)
                xDoc.Root.Add(new XElement("rSurveyItemId", rId));
            Stream s = CIAT.SaveFile.GetWriteStream(this);
            xDoc.Save(s);
            s.Dispose();
            CIAT.SaveFile.ReleaseWriteStreamLock();
            foreach (CSurveyItem si in Items)
                si.Save();
        }

        public void Load()
        {
            Stream s = CIAT.SaveFile.GetReadStream(this);
            XDocument xDoc = XDocument.Load(s);
            s.Dispose();
            CIAT.SaveFile.ReleaseReadStreamLock();
            Name = xDoc.Root.Attribute("Name").Value;
            if (xDoc.Root.Attribute("Index") == null)
            {
                Regex exp = new Regex(@"[^0-9]+([0-9]+)");
                Index = Convert.ToInt32(exp.Match(Name).Groups[1].Value);
                CIAT.SaveFile.IAT.SurveyIndicies.AddSurvey(URI, Index);
            }
            else
                Index = Convert.ToInt32(xDoc.Root.Attribute("Index").Value);
//            _IndexInContents = Convert.ToInt32(xDoc.Root.Attribute("IndexInContents").Value);
            Ordinality = (CSurvey.EOrdinality)Enum.Parse(typeof(CSurvey.EOrdinality), xDoc.Root.Attribute("Ordinality").Value);
            Timeout = Convert.ToDecimal(xDoc.Root.Attribute("Timeout").Value.ToString());
            if (CVersion.Compare(CIAT.SaveFile.Version, new CVersion("1.1.0.14")) >= 0)
            {
                foreach (XElement elem in xDoc.Root.Element("Items").Elements())
                {
                    if (elem.Name.LocalName == SurveyItemType.Caption.Name)
                    {
                        CSurveyCaption cap = new CSurveyCaption();
                        cap.Load(elem);
                    }
                    else if (elem.Name.LocalName == "Question")
                    {
                        CSurveyItem si = new CSurveyItem();
                        si.Load(elem);
                    }
                    else if (elem.Name.LocalName == SurveyItemType.SurveyImage.Name)
                    {
                        CSurveyItemImage sii = new CSurveyItemImage();
                        sii.Load(elem);
                    }
                }
            }
            else
            {
                foreach (XElement surveyItemElem in xDoc.Root.Elements("rSurveyItemId"))
                {
                    Items.AddItemByRel(surveyItemElem.Value);
                }
            }
        }
        /*
                public void WriteToXml(XmlTextWriter writer)
                {
                    writer.WriteStartElement("Survey");
                    writer.WriteElementString("Name", Name);
                    writer.WriteElementString("NumItems", Items.Count.ToString());
                    writer.WriteElementString("IndexInContents", IndexInContents.ToString());
                    writer.WriteElementString("Timeout", Timeout.ToString());
                    writer.WriteStartElement("Items");
                    for (int ctr = 0; ctr < Items.Count; ctr++)
                        Items[ctr].WriteToXml(writer);
                    writer.WriteEndElement();
                    writer.WriteEndElement();
                }
        */
        public bool IsValid()
        {
            return (Name != String.Empty);
        }

        public int GetIndexOf(CSurveyItem item)
        {
            if (Items[0].IsCaption)
                return Items.IndexOf(item) - 1;
            return Items.IndexOf(item);
        }

        public CSurveyItem GetItemNumbered(int itemNum)
        {
            int itemCtr = 0, ctr = 0;
            while (itemCtr < itemNum)
            {
                if (Items[ctr++].Response == null)
                    continue;
                if (Items[ctr - 1].Response.ResponseType == CResponse.EResponseType.Instruction)
                    continue;
                itemCtr++;
            }
            return Items[ctr - 1];
        }

        public bool HasAlternateItem
        {
            get
            {
                return (AltGroup != null);
            }
        }

        public ContentsItemType Type
        {
            get
            {
                if (Ordinality == EOrdinality.Before)
                    return ContentsItemType.BeforeSurvey;
                else
                    return ContentsItemType.AfterSurvey;
            }
        }

        public AlternationGroup AlternationGroup
        {
            get
            {
                return AltGroup;
            }
            set
            {
                if ((AltGroup != null) && (value != null))
                    throw new Exception("Dispose of the alternation group and instantiate a new one.  Do not try to change the value of a IContentsItem alternation group.");
                AltGroup = value;
            }
        }

        public int IndexInContainer
        {
            get
            {
                if (Ordinality == EOrdinality.Before)
                    return CIAT.SaveFile.IAT.BeforeSurvey.IndexOf(this);
                else
                    return CIAT.SaveFile.IAT.AfterSurvey.IndexOf(this);
            }
        }

        public void DeleteFromIAT()
        {
            CIAT.SaveFile.IAT.DeleteContentsItem(this);
        }

        public void AddToIAT(int InsertionNdx)
        {
            if (Ordinality == EOrdinality.Before)
                CIAT.SaveFile.IAT.InsertBeforeSurvey(this, InsertionNdx);
            if (Ordinality == EOrdinality.After)
                CIAT.SaveFile.IAT.InsertAfterSurvey(this, InsertionNdx);
        }

        public int IndexInContents
        {
            get
            {
                return CIAT.SaveFile.IAT.GetIndexInTest(this);
            }
        }

        public int GetItemNum(CSurveyItem item)
        {
            int itemNum = 0;
            for (int ctr = 0; ctr < Items.Count; ctr++)
            {
                if (Items[ctr] == item)
                    return itemNum + 1;
                if (Items[ctr].IsCaption)
                    continue;
                if (Items[ctr].Response.ResponseType != CResponse.EResponseType.Instruction)
                    itemNum++;
            }
            return -1;
        }

        public void Preview(IImageDisplay p)
        {
            if (p.Tag != null)
                (p.Tag as IPreviewableItem).EndPreview(p);
            Graphics g = Graphics.FromHwnd(p.Handle);
            int scrollWidth = System.Windows.Forms.ScrollBarRenderer.GetSizeBoxSize(g, System.Windows.Forms.VisualStyles.ScrollBarState.Normal).Width + 1;
            g.Dispose();
            Panel preview = new Panel();
            preview.HorizontalScroll.Visible = false;
            preview.HorizontalScroll.Enabled = false;
            preview.AutoScroll = true;
            preview.Dock = DockStyle.Fill;
            p.Controls.Add(preview);
            SurveyPreview = new WebBrowser();
            SurveyPreview.BackColor = Color.White;
            preview.Controls.Add(SurveyPreview);
            String html = SurveyPreviewHTML;
            SurveyPreview.DocumentCompleted += (sender, args) =>
            {
                SurveyPreview.Size = new Size((SurveyPreview.Document.Body.ScrollRectangle.Height > preview.Height) ? (preview.Width - scrollWidth) : preview.Width,
                    (SurveyPreview.Document.Body.ScrollRectangle.Height < preview.Height) ? preview.Height : (SurveyPreview.Document.Body.ScrollRectangle.Height));
                preview.AutoScroll = true;
                preview.HorizontalScroll.Enabled = false;
                preview.HorizontalScroll.Visible = false;
                SurveyPreview.ScrollBarsEnabled = false;
            };
            SurveyPreview.DocumentText = SurveyPreviewHTML;
        }

        private void XBrowser_NavigateComplete2(object pDisp, ref object URL)
        {
            SHDocVw.WebBrowser xBrowser = (SHDocVw.WebBrowser)SurveyPreview.ActiveXInstance;
        }

        public void WriteXml(XmlWriter xWriter)
        {
            xWriter.WriteStartElement("Survey");
            xWriter.WriteAttributeString("TimeoutMillis", (Timeout * 60000).ToString());
            if (CIAT.SaveFile.IAT.UniqueResponse.SurveyUri != null)
            {
                if (CIAT.SaveFile.IAT.UniqueResponse.SurveyUri.Equals(this.URI))
                    xWriter.WriteAttributeString("UniqueResponseItem", CIAT.SaveFile.IAT.UniqueResponse.ItemNum.ToString());
                else
                    xWriter.WriteAttributeString("UniqueResponseItem", "-1");
            }
            else
                xWriter.WriteAttributeString("UniqueResponseItem", "-1");
            xWriter.WriteElementString("IAT", CIAT.SaveFile.IAT.Name);
            xWriter.WriteElementString("FileName", WebUtility.UrlEncode(Name + ".xml"));
            xWriter.WriteElementString("SurveyName", Name);
            xWriter.WriteElementString("InitialPosition", IndexInContents.ToString());
            xWriter.WriteElementString("AlternationGroup", (AlternationGroup == null) ? "0" : AlternationGroup.ToString());
            foreach (var si in Items)
            {
                si.WriteXml(xWriter);
            }
            xWriter.WriteEndElement();
        }

        private String SurveyPreviewHTML
        {
            get
            {
                if (Items.Count == 0)
                    return String.Empty;
                MemoryStream memStream = new MemoryStream();
                XmlWriter xWriter = new XmlTextWriter(memStream, System.Text.Encoding.Unicode);
                xWriter.WriteStartDocument();
                WriteXml(xWriter);
                xWriter.WriteEndDocument();
                xWriter.Flush();
                memStream.Seek(0, SeekOrigin.Begin);
                XmlDocument xDoc = new XmlDocument();
                xDoc.Load(memStream);
                TextWriter txtWriter = new StringWriter();
                XdmNode inputNode = xsltProcessor.NewDocumentBuilder().Build(xDoc.DocumentElement);
                Serializer ser = xsltProcessor.NewSerializer(txtWriter);
                xsltTransformer.InitialContextNode = inputNode;
                xsltTransformer.Run(ser);
                return txtWriter.ToString();
            }
        }

        public void EndPreview(IImageDisplay previewPanel)
        {
            SurveyPreview.Dispose();
            if (previewPanel.Tag == this)
                foreach (Control c in PreviewControls)
                {
                    previewPanel.Controls.Remove(c);
                    c.Dispose();
                }
        }

        public void OpenItem(IATConfigMainForm mainForm)
        {
            mainForm.ActiveItem = this;
            mainForm.FormContents = IATConfigMainForm.EFormContents.Survey;
        }

        public List<IPreviewableItem> SubContentsItems
        {
            get
            {
                return new List<IPreviewableItem>();
            }
        }
        public String PreviewText
        {
            get
            {
                return Name;
            }
        }

        public void Dispose()
        {
            if (_IsDisposed)
                return;
            _IsDisposed = true;
            if (AlternationGroup != null)
                AlternationGroup.Remove(this);
            List<Uri> surveyImageUris = CIAT.SaveFile.GetRelationshipsByType(this.URI, BaseType, typeof(DISurveyImage)).Select(pr => pr.TargetUri).ToList();
            foreach (Uri u in surveyImageUris)
            {
                CIAT.SaveFile.DeleteRelationship(this.URI, u);
                CIAT.SaveFile.GetDI(u).Dispose();
            }
            CIAT.SaveFile.ActivityLog.LogEvent(ActivityLog.EventType.Delete, URI);
            CIAT.SaveFile.IAT.SurveyIndicies.RemoveSurvey(URI);
            CIAT.SaveFile.DeletePart(this.URI);
        }

        public bool IsDisposed
        {
            get
            {
                return _IsDisposed;
            }
        }

        public Button GUIButton { get; set; }
    }
}

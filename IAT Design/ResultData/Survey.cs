using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using System.Windows.Forms;

namespace IATClient.ResultData
{
    [Serializable]
    public class Survey
    {
        [XmlElement(ElementName = "Name", Form = System.Xml.Schema.XmlSchemaForm.Unqualified, IsNullable = false)]
        public String Name { get; set; }
        [XmlElement(ElementName = "Timeout", Form = System.Xml.Schema.XmlSchemaForm.Unqualified, IsNullable = false)]
        public int Timeout { get; set; }
        [XmlElement(ElementName = "Caption", Form = System.Xml.Schema.XmlSchemaForm.Unqualified, Type = typeof(SurveyCaption))]
        public SurveyCaption Caption { get; set; }
        [XmlElement(ElementName = "SurveyItems", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public SurveyItem[] SurveyItems { get; set; }
        [XmlAttribute(AttributeName = "NumItems", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public int NumItems { get; set; }
        [XmlAttribute(AttributeName = "HasCaption", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public bool HasCaption { get; set; }

        private Panel PreviewPanel = null;
        private SurveyItem[] _Questions = null;
        public Survey() { }

        public SurveyItem[] Questions
        {
            get
            {
                if ((_Questions == null) && (NumQuestions > 0))
                {
                    _Questions = new SurveyItem[NumQuestions];
                    int ndx = 0;
                    for (int ctr = 0; ctr < SurveyItems.Length; ctr++)
                        if ((SurveyItems[ctr].Response.ResponseType != ResponseType.None) && (SurveyItems[ctr].Response.ResponseType != ResponseType.Picture))
                            _Questions[ndx++] = SurveyItems[ctr];
                }
                else if (NumQuestions == 0)
                    return new SurveyItem[0];
                return _Questions;
            }
        }

        public Survey(String name)
        {
            Name = name;
        }

        public int NumQuestions
        {
            get
            {
                int nQuests = 0;
                if (SurveyItems == null)
                    return 0;
                for (int ctr = 0; ctr < SurveyItems.Length; ctr++)
                    if ((SurveyItems[ctr].Response.ResponseType != ResponseType.None) && (SurveyItems[ctr].Response.ResponseType != ResponseType.Picture))
                        nQuests++;
                return nQuests;
            }
        }

        public void SetCaption(CSurveyItem si)
        {
            CSurveyCaption c = (CSurveyCaption)si;
            Caption = new SurveyCaption();
            Caption.BackColor = new Color();
            Caption.BackColor.Red = c.BackColor.R;
            Caption.BackColor.Green = c.BackColor.G;
            Caption.BackColor.Blue = c.BackColor.B;
            Caption.FontColor = new Color();
            Caption.FontColor.Red = c.FontColor.R;
            Caption.FontColor.Green = c.FontColor.G;
            Caption.FontColor.Blue = c.FontColor.B;
            Caption.BorderColor = new Color();
            Caption.BorderColor.Red = c.BorderColor.R;
            Caption.BorderColor.Green = c.BorderColor.G;
            Caption.BorderColor.Blue = c.BorderColor.B;
            Caption.Text = c.Text;
            Caption.FontSize = c.FontSize;
            Caption.BorderWidth = c.BorderWidth;
            Caption.Text = c.Text;
        }

        public void SetItems(CSurveyItem[] surveyItems)
        {
            SurveyItems = new SurveyItem[surveyItems.Length];
            for (int ctr = 0; ctr < surveyItems.Length; ctr++)
            {
                SurveyItems[ctr] = new SurveyItem(this, ctr);
                SurveyItems[ctr].Text = surveyItems[ctr].Text;
                SurveyItems[ctr].Response = surveyItems[ctr].Response.GenerateSerializableResponse(SurveyItems[ctr]);
            }
        }


        public void PostSerialize()
        {
            int itemCtr = 1;
            for (int ctr = 0; ctr < SurveyItems.Length; ctr++)
            {
                if (SurveyItems[ctr].Response.ResponseType != ResponseType.None)
                    SurveyItems[ctr].PostSerialize(this, itemCtr++);
                else
                    SurveyItems[ctr].PostSerialize(this, -1);
            }
        }

        public Panel GeneratePreview(int width)
        {
            PreviewPanel = new Panel();
            PreviewPanel.BackColor = System.Drawing.Color.White;
            List<Panel> itemPreviewPanels = new List<Panel>();
            if (HasCaption)
                itemPreviewPanels.Add(Caption.GeneratePreviewPanel(width));
            for (int ctr = 0; ctr < SurveyItems.Length; ctr++)
                itemPreviewPanels.Add(SurveyItems[ctr].GeneratePreviewPanel(width));
            int height = 0;
            for (int ctr = 0; ctr < itemPreviewPanels.Count; ctr++)
            {
                itemPreviewPanels[ctr].Location = new Point(0, height);
                height += itemPreviewPanels[ctr].Height;
                PreviewPanel.Controls.Add(itemPreviewPanels[ctr]);
            }
            PreviewPanel.Size = new Size(width, height);
            return PreviewPanel;
        }

        public void DisplayValues(ISurveyResponse responses)
        {
            int respCtr = 0;
            for (int ctr = 0; ctr < SurveyItems.Length; ctr++)
                if (SurveyItems[ctr].Response.ResponseType != ResponseType.None)
                    SurveyItems[ctr].SetDisplayResponse(responses[respCtr++]);
        }
    }
}

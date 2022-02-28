using System;
using System.Windows.Forms;
using System.Xml.Serialization;

namespace IATClient.ResultData
{

    [Serializable]
    public class SurveyCaption : SurveyItem
    {
        [XmlElement(ElementName = "FontColor", Form = System.Xml.Schema.XmlSchemaForm.Unqualified, IsNullable = false, Type = typeof(Color))]
        public Color FontColor { get; set; }
        [XmlElement(ElementName = "BackColor", Form = System.Xml.Schema.XmlSchemaForm.Unqualified, IsNullable = false, Type = typeof(Color))]
        public Color BackColor { get; set; }
        [XmlElement(ElementName = "BorderColor", Form = System.Xml.Schema.XmlSchemaForm.Unqualified, IsNullable = false, Type = typeof(Color))]
        public Color BorderColor { get; set; }
        [XmlElement(ElementName = "BorderWidth", Form = System.Xml.Schema.XmlSchemaForm.Unqualified, IsNullable = false)]
        public int BorderWidth { get; set; }
        [XmlElement(ElementName = "FontSize", Form = System.Xml.Schema.XmlSchemaForm.Unqualified, IsNullable = false)]
        public int FontSize { get; set; }

        private Survey ParentItem = null;
        private int ItemNum = -1;
        private Panel PreviewPanel = null;

        public SurveyCaption(Survey parent, int itemNum)
        {
            ParentItem = parent;
            ItemNum = itemNum;
            Response = null;
        }

        public SurveyCaption()
        {
            Response = null;
        }

        public override Panel GeneratePreviewPanel(int nWidth)
        {
            var f = new System.Drawing.Font(System.Drawing.FontFamily.GenericSerif, (float)FontSize, System.Drawing.FontStyle.Bold);
            var bmp = new System.Drawing.Bitmap(nWidth, (int)(f.Height * 1.5) + BorderWidth, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            var g = System.Drawing.Graphics.FromImage(bmp);
            var br = new System.Drawing.SolidBrush(BackColor.ToSystemColor());
            g.FillRectangle(br, new System.Drawing.Rectangle(0, 0, nWidth, (int)(f.Height * 1.5)));
            br.Dispose();
            br = new System.Drawing.SolidBrush(FontColor.ToSystemColor());
            System.Drawing.SizeF sz = g.MeasureString(Text, f);
            g.DrawString(Text, f, br, new System.Drawing.PointF((int)(nWidth - sz.Width) >> 1, .25F * f.Height));
            br.Dispose();
            br = new System.Drawing.SolidBrush(BorderColor.ToSystemColor());
            g.FillRectangle(br, new System.Drawing.Rectangle(0, (int)(1.5 * f.Height), nWidth, BorderWidth));
            br.Dispose();
            g.Dispose();
            PreviewPanel = new Panel();
            PreviewPanel.BackgroundImage = bmp;
            PreviewPanel.Size = bmp.Size;
            return PreviewPanel;
        }

    }
}

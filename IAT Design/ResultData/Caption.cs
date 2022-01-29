using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using System.Drawing;
using System.Windows.Forms;

namespace IATClient.ResultData
{
    [Serializable]
    public class Caption : SurveyItem
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

        public Caption(Survey parent, int itemNum)
        {
            ParentItem = parent;
            ItemNum = itemNum;
            Response = null;
        }

        public Caption()
        {
            Response = null;
        }

        public override Panel GeneratePreviewPanel(int nWidth)
        {
            Font f = new Font(FontFamily.GenericSerif, (float)FontSize, FontStyle.Bold);
            Bitmap bmp = new Bitmap(nWidth, (int)(f.Height * 1.5) + BorderWidth, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            Graphics g = Graphics.FromImage(bmp);
            SolidBrush br = new SolidBrush(BackColor.ToSystemColor());
            g.FillRectangle(br, new Rectangle(0, 0, nWidth, (int)(f.Height * 1.5)));
            br.Dispose();
            br = new SolidBrush(FontColor.ToSystemColor());
            SizeF sz = g.MeasureString(Text, f);
            g.DrawString(Text, f, br, new PointF((int)(nWidth - sz.Width) >> 1, .25F * f.Height));
            br.Dispose();
            br = new SolidBrush(BorderColor.ToSystemColor());
            g.FillRectangle(br, new Rectangle(0, (int)(1.5 * f.Height), nWidth, BorderWidth));
            br.Dispose();
            g.Dispose();
            PreviewPanel = new Panel();
            PreviewPanel.BackgroundImage = bmp;
            PreviewPanel.Size = bmp.Size;
            return PreviewPanel;
        }

    }
}

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace IATClient
{
    class ItemSlideDisplayPanel : Panel
    {
        private PictureBox DisplayPanel;
        public static Padding DisplayPadding = new Padding(10);
        private Button CloseButton;
        private List<long> Latencies = new List<long>();
        private double MeanLatency, MeanNumErrors;
        private int ResultSet = 0;
        public delegate void CloseEventHandler();
        private CloseEventHandler CloseHandler;

        public ItemSlideDisplayPanel(CloseEventHandler closeHandler, Size sz)
        {
            CloseHandler = closeHandler;
            this.Paint += new PaintEventHandler(ItemSlideDisplayPanel_Paint);
            this.Size = new Size(sz.Width + DisplayPadding.Horizontal, sz.Height + DisplayPadding.Vertical + (4 * System.Drawing.SystemFonts.DefaultFont.Height));
            CloseButton = new Button();
            CloseButton.Size = TextRenderer.MeasureText("X", System.Drawing.SystemFonts.CaptionFont) + new Size(15, 10);
            CloseButton.BackColor = System.Drawing.Color.White;
            CloseButton.FlatStyle = FlatStyle.Flat;
            CloseButton.Font = System.Drawing.SystemFonts.CaptionFont;
            CloseButton.ForeColor = System.Drawing.Color.Red;
            CloseButton.Text = "X";
            CloseButton.Location = new Point((this.Right - CloseButton.Width) >> 1, DisplayPadding.Top);
            CloseButton.Click += new EventHandler(Close_Click);
            Controls.Add(CloseButton);
            DisplayPanel = new PictureBox();
            DisplayPanel.BackColor = System.Drawing.Color.White;
            DisplayPanel.BorderStyle = BorderStyle.None;
            DisplayPanel.Size = sz;
            DisplayPanel.Location = new Point(DisplayPadding.Left, DisplayPadding.Top);
            DisplayPanel.Image = null;
            DisplayPanel.SizeMode = PictureBoxSizeMode.CenterImage;
            Controls.Add(DisplayPanel);
        }

        private void Close_Click(object sender, EventArgs e)
        {
            CloseHandler();
        }

        void ItemSlideDisplayPanel_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            Rectangle rect = ClientRectangle;
            rect.Inflate(-(DisplayPadding.Horizontal >> 1), -(DisplayPadding.Vertical >> 1));
            g.DrawRectangle(Pens.Gray, rect);
            String str;
            if (Latencies.Count == 0)
                str = "No presentations for this subject";
            else if (Latencies.Count == 1)
                str = String.Format("Response latency for subject #{0}: {1}", ResultSet, Latencies[0]);
            else
            {
                str = String.Format("Response latencies for subject #{0}: ", ResultSet);
                for (int ctr = 0; ctr < Latencies.Count - 1; ctr++)
                    str += String.Format("{0}, ", Latencies[ctr]);
                str += Latencies.Last().ToString();
            }
            SizeF szF = g.MeasureString(str, System.Drawing.SystemFonts.DefaultFont);
            PointF ptF = new PointF((ClientRectangle.Width - szF.Width) / 2, ClientRectangle.Height - 3.5F * System.Drawing.SystemFonts.DefaultFont.Height - DisplayPadding.Bottom);
            g.DrawString(str, System.Drawing.SystemFonts.DefaultFont, Brushes.Black, ptF);
            str = String.Format("Mean latency for item: {0:F4}", MeanLatency);
            szF = g.MeasureString(str, System.Drawing.SystemFonts.DefaultFont);
            ptF = new PointF((ClientRectangle.Width - szF.Width) / 2, ClientRectangle.Height - 2.25F * System.Drawing.SystemFonts.DefaultFont.Height - DisplayPadding.Bottom);
            g.DrawString(str, System.Drawing.SystemFonts.DefaultFont, Brushes.Black, ptF);
            str = String.Format("Mean # of errors across subjects: {0:F2}", MeanNumErrors);
            szF = g.MeasureString(str, System.Drawing.SystemFonts.DefaultFont);
            ptF = new PointF((ClientRectangle.Width - szF.Width) / 2, ClientRectangle.Height - 1F * System.Drawing.SystemFonts.DefaultFont.Height - DisplayPadding.Bottom);
            g.DrawString(str, System.Drawing.SystemFonts.DefaultFont, Brushes.Black, ptF);
        }

        public void SetResultData(List<long> latencies, double meanLatency, double meanErrors, int resultSet)
        {
            Latencies.Clear();
            Latencies.AddRange(latencies);
            MeanLatency = meanLatency;
            ResultSet = resultSet;
            MeanNumErrors = meanErrors;
        }

        public void SetImage(Image img)
        {
            DisplayPanel.BorderStyle = BorderStyle.None;
            if (DisplayPanel.BackgroundImage != null)
                DisplayPanel.BackgroundImage.Dispose();
            DisplayPanel.BackgroundImage = img;
        }
    }
}

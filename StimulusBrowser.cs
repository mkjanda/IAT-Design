using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace IATClient
{
    /*
    public partial class StimulusBrowser : UserControl
    {
        protected CIATBlock _IATBlock;
        protected Button LeftButton, RightButton;
        protected Panel StimuliPanel;
        protected Padding StimulusPadding = new Padding(5);
        public static Size StimulusSize = new Size(96, 96);
        protected int StartStimulusNdx = 0;
        protected List<Image> StimuliThumbnails;
        protected Color StimulusOutlineColor;
        protected Padding StimulusContainerPadding = new Padding(10, 0, 10, 0);

        public int NumStimuliDisplayed
        {
            get
            {
                return this.Width / (StimulusSize.Width + StimulusPadding.Horizontal);
            }
        }

        public CIATBlock IATBlock
        {
            get
            {
                return _IATBlock;
            }
            set
            {
                _IATBlock = value;
            }
        }

        public StimulusBrowser()
        {
            InitializeComponent();
            LeftButton = new Button();
            RightButton = new Button();
            StimuliPanel = new Panel();
            LeftButton.Image = Properties.Resources.LeftButtonArrow;
            RightButton.Image = Properties.Resources.RightButtonArrow;
            LeftButton.ImageAlign = ContentAlignment.MiddleCenter;
            RightButton.ImageAlign = ContentAlignment.MiddleCenter;
            StimuliThumbnails = new List<Image>();
            this.BackColor = Color.Black;
            StimulusOutlineColor = Color.LimeGreen;
        }

        private void StimulusBrowser_Load(object sender, EventArgs e)
        {
            LeftButton.Image = Properties.Resources.LeftButtonArrow;
            RightButton.Image = Properties.Resources.RightButtonArrow;
            LeftButton.ImageAlign = ContentAlignment.MiddleCenter;
            RightButton.ImageAlign = ContentAlignment.MiddleCenter;
            this.Controls.Add(LeftButton);
            this.Controls.Add(RightButton);
            LeftButton.Size = new Size(24, this.Height);
            RightButton.Size = new Size(24, this.Height);
            LeftButton.Dock = DockStyle.Left;
            RightButton.Dock = DockStyle.Right;
            LeftButton.Click += new EventHandler(LeftButton_Click);
            RightButton.Click += new EventHandler(RightButton_Click);
            for (int ctr = 0; ctr < IATBlock.Items.Count; ctr++)
                StimuliThumbnails.Add(IATBlock.Items[ctr].GetStimulusThumbnail());
            LeftButton.Enabled = false;
            StartStimulusNdx = 0;
            if (NumStimuliDisplayed <= IATBlock.Items.Count)
                RightButton.Enabled = false;
            StimuliPanel.Size = new Size(this.Size.Width - LeftButton.Width - RightButton.Width, this.Size.Height);
            Controls.Add(StimuliPanel);
            StimuliPanel.MouseUp += new MouseEventHandler(StimuliPanel_MouseUp);
        }

        void StimuliPanel_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left)
                return;

        }

        void RightButton_Click(object sender, EventArgs e)
        {
            StartStimulusNdx++;
            if (StartStimulusNdx + NumStimuliDisplayed >= IATBlock.Items.Count)
                RightButton.Enabled = false;
            InvalidateStimuliPanel();
        }

        void LeftButton_Click(object sender, EventArgs e)
        {
            StartStimulusNdx--;
            if (StartStimulusNdx == 0)
                LeftButton.Enabled = false;
            InvalidateStimuliPanel();
        }

        protected void InvalidateStimuliPanel()
        {
            Brush backBr = new SolidBrush(this.BackColor);
            Bitmap StimuliBMP = new Bitmap(StimuliPanel.Width, StimuliPanel.Height, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
            Graphics g = Graphics.FromImage(StimuliBMP);
            g.FillRectangle(backBr, new Rectangle(0, 0, StimuliPanel.Width, StimuliPanel.Height));
            Brush outlineBr = new SolidBrush(StimulusOutlineColor);
            Pen outlinePen = new Pen(outlineBr);
            Point pt = new Point(StimulusContainerPadding.Left, StimulusContainerPadding.Top);
            for (int ctr = 0; ctr < NumStimuliDisplayed; ctr++)
            {
                if (ctr + StartStimulusNdx < StimuliThumbnails.Count)
                {
                    g.DrawRectangle(outlinePen, new Rectangle(pt.X + StimulusPadding.Left - 1, pt.Y + StimulusPadding.Top - 1, StimulusSize.Width + 2, StimulusSize.Height + 2));
                    g.DrawImage(StimuliThumbnails[ctr + StartStimulusNdx], pt.X + StimulusPadding.Left, pt.Y + StimulusPadding.Top);
                }
                pt.X += StimulusSize.Width + StimulusPadding.Horizontal;
            }
            g.Dispose();
            outlinePen.Dispose();
            outlineBr.Dispose();
            backBr.Dispose();
            StimuliPanel.BackgroundImage = StimuliBMP;
        }
    }*/
}

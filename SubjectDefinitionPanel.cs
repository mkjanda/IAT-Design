using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;

namespace IATClient
{
    class SubjectDefinitionPanel : Panel
    {
        class NumberPanel : Panel
        {
            private static Dictionary<NumberPanel, int> ValueDictionary = new Dictionary<NumberPanel, int>();
            private static Size _NumberSize = new Size(22, 22);
            private static Font NumberFont;
            private static Padding ValuePadding = new Padding(3);

            public static Size NumberSize
            {
                get
                {
                    return _NumberSize;
                }
                set
                {
                    _NumberSize = value;
                    Size sz;
                    Font f = null;
                    float fSize = 16.5F;
                    do
                    {
                        fSize -= .5F;
                        if (f != null)
                            f.Dispose();
                        f = new Font(System.Drawing.SystemFonts.DefaultFont.FontFamily, 16);
                        sz = TextRenderer.MeasureText("8", f) + new Size(ValuePadding.Horizontal, ValuePadding.Vertical);
                    } while ((sz.Width > value.Width) && (sz.Height > value.Height));
                    NumberFont = f;
                }
            }

            public static int MaxValue
            {
                get
                {
                    int maxVal = 0;
                    foreach (NumberPanel np in ValueDictionary.Keys)
                        if (ValueDictionary[np] != -1)
                            maxVal++;
                    return maxVal;
                }
            }

            public int Value
            {
                get {
                    return ValueDictionary[this];
                }
            }

            public NumberPanel(Size sz)
            {
                this.Size = sz;
                this.BackColor = System.Drawing.Color.White;
                this.BorderStyle = BorderStyle.FixedSingle;
                this.ForeColor = System.Drawing.Color.Black;
                this.Paint += new PaintEventHandler(NumberPanel_Paint);
                this.Click += new EventHandler(NumberPanel_Click);
                ValueDictionary[this] = -1;
            }

            private void NumberPanel_Click(object sender, EventArgs e)
            {
                if (ValueDictionary[this] == -1)
                    ValueDictionary[this] = MaxValue + 1;
                else
                {
                    foreach (NumberPanel np in ValueDictionary.Keys)
                        if (ValueDictionary[np] > ValueDictionary[this])
                        {
                            ValueDictionary[np] -= 1;
                            np.Invalidate();
                        }
                    ValueDictionary[this] = -1;
                }
                this.Invalidate();
            }

            private void NumberPanel_Paint(object sender, PaintEventArgs e)
            {
                int val = ValueDictionary[this];
                e.Graphics.FillRectangle(Brushes.White, this.ClientRectangle);
                if (val == -1)
                    return;
                Size sz = TextRenderer.MeasureText(val.ToString(), NumberFont);
                e.Graphics.DrawString(val.ToString(), NumberFont, Brushes.Black, new PointF((this.ClientRectangle.Width - sz.Width) >> 1, (this.ClientRectangle.Height - sz.Height) >> 1));
            }
        }

        private Dictionary<IATSurveyFile.SurveyItem, NumberPanel> NumberPanelDictionary = new Dictionary<IATSurveyFile.SurveyItem, NumberPanel>();
        private List<CSurveyOutline> SurveyOutlines = new List<CSurveyOutline>();
        private Padding OutlinePadding = new Padding(35, 8, 35, 8);
        private Padding SurveyPadding = new Padding(6, 6, 6, 6);
        private Padding ButtonPadding = new Padding(8, 0, 8, 0);
        private ResultSetDescriptor Descriptor = null;
        private Panel DonePanel, CancelPanel;
        private Button DoneButton, CancelButton;

        public CSubjectID SubjectID
        {
            get
            {
                CSubjectID result = new CSubjectID();
                int ordinal = 1;
                bool bDone = false;
                while (!bDone)
                {
                    bDone = true;
                    foreach (IATSurveyFile.SurveyItem si in NumberPanelDictionary.Keys)
                    {
                        if (NumberPanelDictionary[si].Value == ordinal++)
                        {
                            bDone = false;
                            bool bFound = false;
                            for (int ctr = 0; ctr < Descriptor.BeforeSurveys.Count; ctr++) 
                                if (Descriptor.BeforeSurveys[ctr].SurveyItems.Contains(si))
                                {
                                    result.SurveyNumList.Add(ctr);
                                    result.ItemNumList.Add(si.GetItemNum());
                                    bFound = true;
                                    break;
                                }
                            if (bFound)
                                break;
                            for (int ctr = 0; ctr < Descriptor.AfterSurveys.Count; ctr++)
                                if (Descriptor.AfterSurveys[ctr].SurveyItems.Contains(si))
                                {
                                    result.SurveyNumList.Add(ctr + Descriptor.BeforeSurveys.Count);
                                    result.ItemNumList.Add(si.GetItemNum());
                                    bFound = true;
                                }
                            if (bFound)
                                break;
                        }
                    }
                }
                return result;
            }
        }
                        

        public SubjectDefinitionPanel(ResultSetDescriptor descriptor, int width)
        {
            Descriptor = descriptor;
            for (int ctr = 0; ctr < descriptor.BeforeSurveys.Count; ctr++)
                SurveyOutlines.Add(new CSurveyOutline(descriptor.BeforeSurveys[ctr]));
            for (int ctr = 0; ctr < descriptor.AfterSurveys.Count; ctr++)
                SurveyOutlines.Add(new CSurveyOutline(descriptor.AfterSurveys[ctr]));

            NumberPanel.NumberSize = new Size(26, 26);
            int yOffset = OutlinePadding.Top + SurveyPadding.Top;
            List<Label> labelList = new List<Label>();
            for (int ctr = 0; ctr < SurveyOutlines.Count; ctr++)
            {
                SurveyOutlines[ctr].CalcOutline(OutlinePadding.Left, width - OutlinePadding.Horizontal, System.Drawing.SystemFonts.DefaultFont, CSurveyOutline.EResponseLabel.alphabetical);
                labelList.Clear();
                labelList.AddRange(SurveyOutlines[ctr].GetSurveyLabels());
                foreach (Label l in labelList)
                {
                    l.Location = new Point(l.Left, l.Top + yOffset);
                    Controls.Add(l);
                }
                if (ctr < descriptor.BeforeSurveys.Count)
                    for (int ctr2 = 0; ctr2 < descriptor.BeforeSurveys[ctr].NumItems; ctr2++)
                    {
                        Label siLabel = SurveyOutlines[ctr].SurveyItemLabels[descriptor.BeforeSurveys[ctr].SurveyItems[ctr2]].First();
                        NumberPanel np = new NumberPanel(NumberPanel.NumberSize);
                        np.Location = new Point(siLabel.Left - np.Width - SurveyPadding.Right, (siLabel.Height > np.Height) ? (siLabel.Top + ((siLabel.Height - np.Height) >> 1)) :
                            (siLabel.Top - ((np.Height - siLabel.Height) >> 1)));
                        Controls.Add(np);
                    }
                else
                    for (int ctr2 = 0; ctr2 < descriptor.AfterSurveys[ctr - descriptor.BeforeSurveys.Count].NumItems; ctr2++)
                    {
                        Label siLabel = SurveyOutlines[ctr].SurveyItemLabels[descriptor.AfterSurveys[ctr - descriptor.BeforeSurveys.Count].SurveyItems[ctr2]].First();
                        NumberPanel np = new NumberPanel(NumberPanel.NumberSize);
                        np.Location = new Point(siLabel.Left - np.Width - SurveyPadding.Right, (siLabel.Height > np.Height) ? (siLabel.Top + ((siLabel.Height - np.Height) >> 1)) :
                            (siLabel.Top - ((np.Height - siLabel.Height) >> 1)));
                        Controls.Add(np);
                    }                        
                yOffset = labelList.Last().Bottom + SurveyPadding.Vertical;
            }
            this.Size = new Size(width, yOffset - SurveyPadding.Top + OutlinePadding.Bottom);
            this.AutoScroll = true;

            int buttonWidth = TextRenderer.MeasureText("Done", System.Drawing.SystemFonts.DefaultFont).Width;
            int cancelWidth = TextRenderer.MeasureText("Cancel", System.Drawing.SystemFonts.DefaultFont).Width;
            if (buttonWidth < cancelWidth)
                buttonWidth = cancelWidth;

            CancelPanel = new Panel();
            CancelButton = new Button();
            CancelButton.Text = "Cancel";
            CancelButton.Width = buttonWidth + ButtonPadding.Horizontal;
            CancelButton.Location = new Point(0, 0);
            CancelPanel.Controls.Add(CancelButton);
            CancelPanel.Size = new Size(CancelButton.Width, CancelButton.Height);
            CancelPanel.Location = new Point(this.ClientRectangle.Right - OutlinePadding.Left, this.ClientRectangle.Bottom - OutlinePadding.Bottom - DonePanel.Height);
            CancelPanel.BackColor = System.Drawing.Color.Transparent;
            Controls.Add(CancelPanel);
            CancelPanel.BringToFront();

            DonePanel = new Panel();
            DoneButton = new Button();
            DoneButton.Text = "Done";
            DoneButton.Width = buttonWidth + ButtonPadding.Horizontal;
            DoneButton.Location = new Point(0, 0);
            DonePanel.Controls.Add(DoneButton);
            DonePanel.Size = new Size(DoneButton.Width, DoneButton.Height);
            DonePanel.Location = new Point(CancelPanel.Left - OutlinePadding.Left, this.ClientRectangle.Bottom - OutlinePadding.Bottom - DonePanel.Height);
            DonePanel.BackColor = System.Drawing.Color.Transparent;
            Controls.Add(DonePanel);
            DonePanel.BringToFront();

            this.Scroll += new ScrollEventHandler(SubjectDefinitionPanel_Scroll);
        }

        private void SubjectDefinitionPanel_Scroll(object sender, ScrollEventArgs e)
        {
            SuspendLayout();
            CancelPanel.Location = new Point(CancelButton.Left, CancelButton.Top + e.NewValue - e.OldValue);
            DonePanel.Location = new Point(DonePanel.Left, DonePanel.Top + e.NewValue - e.OldValue);
            ResumeLayout(true);
        }
    }
}

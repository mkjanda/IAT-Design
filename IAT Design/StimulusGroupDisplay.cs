/*
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace IATClient
{
    class StimulusGroupDisplay : Form
    {
        private GroupBox OutlineGroup;
        private List<ScrollingPreviewPanelPane> StimulusPanes;
        private List<RadioButton> LeftRadios;
        private List<RadioButton> RightRadios;
        private List<Panel> RadioPanels;
        private const int StimuliPerRow = 5;
        private static Size StimulusSize = new Size(112, 112);
        private static Padding StimulusPaneMargin = new Padding(5);
        private static Padding RadioPadding = new Padding(3);
        private static Padding OutlineGroupPadding = new Padding(3, 15, 3, 3);
        public delegate void OnClosedHandler(List<CIATItem> IATItems);
        new public OnClosedHandler OnClosed;
        public DynamicIATPanel.RetrieveIATItemWithNdx RetrieveIATItem;

        public StimulusGroupDisplay()
        {
            StimulusPanes = new List<ScrollingPreviewPanelPane>();
            LeftRadios = new List<RadioButton>();
            RightRadios = new List<RadioButton>();
            RadioPanels = new List<Panel>();
            OutlineGroup = new GroupBox();
            OutlineGroup.AllowDrop = true;
            OutlineGroup.Text = "Stimuli"; 
            TopMost = true;
            FormBorderStyle = FormBorderStyle.None;
            BackColor = System.Drawing.Color.White;
            AllowDrop = true;
            ShowInTaskbar = false;
            
            this.Load += new EventHandler(StimulusGroupDisplay_Load);
            this.Shown += new EventHandler(StimulusGroupDisplay_Shown);
            this.MouseLeave += new EventHandler(StimulusGroupDisplay_MouseLeave);
            this.DragLeave += new EventHandler(StimulusGroupDisplay_MouseLeave);

            OutlineGroup.MouseLeave += new EventHandler(StimulusGroupDisplay_MouseLeave);
            OutlineGroup.DragLeave += new EventHandler(StimulusGroupDisplay_MouseLeave);
            this.Closed += new EventHandler(StimulusGroupDisplay_Closed);
        }

        public String CaptionText
        {
            get
            {
                return OutlineGroup.Text;
            }
            set
            {
                OutlineGroup.Text = value;
            }
        }

        private void StimulusGroupDisplay_Closed(object sender, EventArgs e)
        {
            Invoke(OnClosed, Stimuli);
        }

        private void StimulusGroupDisplay_MouseLeave(object sender, EventArgs e)
        {
            Point ptForm = PointToScreen(new Point(0, 0));
            Rectangle formRect = new Rectangle(ptForm, this.Size);
            if (!ClientRectangle.Contains(PointToClient(MousePosition)))
                this.Close();
        }

        private List<CIATItem> LoadStimuli = new List<CIATItem>();

        public List<CIATItem> Stimuli
        {
            get
            {
                List<CIATItem> stim = new List<CIATItem>();
                foreach (ScrollingPreviewPanelPane p in StimulusPanes)
                    if (p.PreviewedItem != null)
                        stim.Add(p.PreviewedItem);
                return stim;
            }
            set
            {
                StimulusPanes.Clear();
                LeftRadios.Clear();
                RightRadios.Clear();
                ScrollingPreviewPanelPane StimulusPane;
                RadioButton radio;
                Panel panel;
                int ctr = 0;
                while ((ctr < value.Count) || (ctr % StimuliPerRow != 0) || ((value.Count == 0) && (ctr == 0)))
                {
                    StimulusPane = new ScrollingPreviewPanelPane();
                    StimulusPane.Size = StimulusSize;
                    StimulusPane.OnDragStart += new ScrollingPreviewPanelPane.DragStartHandler(StimulusPane_DragStart);
                    StimulusPane.OnDragEnd += new ScrollingPreviewPanelPane.DragEndHandler(StimulusPane_DragEnd);
                    StimulusPane.OnDragLeave += new ScrollingPreviewPanelPane.DragLeaveHandler(StimulusPane_DragLeave);
                    StimulusPane.DragLeave += new EventHandler(StimulusGroupDisplay_MouseLeave);
                    StimulusPane.ParentOrientation += new ScrollingPreviewPanelPane.ParentOrientationCallback(StimulusPane_ParentOrientation);
                    StimulusPane.IsDragging += new ScrollingPreviewPanelPane.IsDraggingCallback(StimulusPane_IsDragging);
                    StimulusPane.MouseLeave += new EventHandler(StimulusGroupDisplay_MouseLeave);
                    StimulusPane.BackColor = System.Drawing.Color.White;
                    StimulusPane.AllowDrop = true;
                    StimulusPane.PreviewedItem = null;
                    StimulusPanes.Add(StimulusPane);
                    panel = new Panel();
                    panel.BackColor = System.Drawing.Color.White;
                    panel.Size = new Size(StimulusSize.Width, 20);
//                    panel.MouseLeave += new EventHandler(StimulusGroupDisplay_MouseLeave);
  //                  panel.DragLeave += new EventHandler(StimulusGroupDisplay_MouseLeave);
                    RadioPanels.Add(panel);
                    radio = new RadioButton();
                    radio.Text = "Left";
                    radio.Size = new Size(StimulusSize.Width >> 1, 20);
                    radio.Checked = false;
                    radio.Enabled = false;
                    if (ctr < value.Count)
                    {
                        if (value[ctr].KeyedDir == CIATItem.EKeyedDir.DynamicLeft)
                            radio.Checked = true;
                        radio.Enabled = true;
                    }
                    radio.CheckedChanged += new EventHandler(LeftKeyRadio_CheckedChanged);
    //                radio.MouseLeave += new EventHandler(StimulusGroupDisplay_MouseLeave);
      //              radio.DragLeave += new EventHandler(StimulusGroupDisplay_MouseLeave);
                    LeftRadios.Add(radio);
                    radio.Location = new Point(0, 0);
                    panel.Controls.Add(radio);
                    radio = new RadioButton();
                    radio.Text = "Right";
                    radio.Size = new Size(StimulusSize.Width >> 1, 20);
                    radio.Checked = false;
                    radio.Enabled = false;
                    if (ctr < value.Count)
                    {
                        if (value[ctr].KeyedDir == CIATItem.EKeyedDir.DynamicRight)
                            radio.Checked = true;
                        radio.Enabled = true;
                    }
                    radio.CheckedChanged += new EventHandler(RightKeyRadio_CheckedChanged);
//                    radio.MouseLeave += new EventHandler(StimulusGroupDisplay_MouseLeave);
  //                  radio.DragLeave += new EventHandler(StimulusGroupDisplay_MouseLeave);
                    RightRadios.Add(radio);
                    radio.Location = new Point(StimulusSize.Width >> 1, 0);
                    panel.Controls.Add(radio);
                    ctr++;
                }
                LoadStimuli.Clear();
                LoadStimuli.AddRange(value);
            }

        }
    
        private void StimulusPane_DragStart(ScrollingPreviewPanelPane sender)
        {
            if (sender.PreviewedItem != null)
            {
                Clipboard.SetData("IATItemNdx", StimulusPanes.IndexOf(sender));
                DoDragDrop(sender, DragDropEffects.Move);
            }
        }

        private void StimulusPane_DragEnd(ScrollingPreviewPanelPane sender, bool bInsertBefore)
        {
            if (Clipboard.ContainsData("IATItemNdx"))
            {
                int ndx = Convert.ToInt32(Clipboard.GetData("IATItemNdx"));
                CIATItem i = (CIATItem)Invoke(RetrieveIATItem, ndx);
                foreach (ScrollingPreviewPanelPane p in StimulusPanes)
                    if (p.PreviewedItem == i)
                        return;
                if (i.KeySpecifierID != -1)
                {
                    if (MessageBox.Show(this, Properties.Resources.sStimulusAlreadyDynamicallyKeyed, "Stimulus Already Dynamically Keyed", MessageBoxButtons.YesNo) == DialogResult.No)
                        return;
                    else
                        i.ClearKeySpecifier();
                }
                sender.PreviewedItem = i;
                int radioNdx = StimulusPanes.IndexOf(sender);
                LeftRadios[radioNdx].Enabled = true;
                LeftRadios[radioNdx].Checked = false;
                RightRadios[radioNdx].Enabled = true;
                RightRadios[radioNdx].Checked = false;
                sender.PreviewedItem.KeyedDir = CIATItem.EKeyedDir.None;
            }

        }

        private bool StimulusPane_DragLeave(ScrollingPreviewPanelPane sender, bool IsDragOriginator)
        {
            if (IsDragOriginator)
            {
                sender.PreviewedItem = null;
            }
            return IsDragOriginator;
        }

        private ScrollingPreviewPanel.EOrientation StimulusPane_ParentOrientation()
        {
            return ScrollingPreviewPanel.EOrientation.horizontal;
        }

        private bool StimulusPane_IsDragging()
        {
            return Clipboard.ContainsData("IATItemNdx");
        }

        private void StimulusGroupDisplay_Load(object sender, EventArgs e)
        {
            OutlineGroup.Size = StimulusSize + new Size(6, 18);
            OutlineGroup.Location = new Point(0, 0);
            this.Size = OutlineGroup.Size;
            Controls.Add(OutlineGroup);
        }


        private void StimulusGroupDisplay_Shown(object sender, EventArgs e)
        {
            Size szGrid = new Size(StimulusPanes.Count < StimuliPerRow ? StimulusPanes.Count : StimuliPerRow, 
                ((StimulusPanes.Count) / StimuliPerRow) + (((StimulusPanes.Count) % StimuliPerRow == 0) ? 0 : 1));
            Size szForm = new Size(szGrid.Width * (StimulusSize.Width + StimulusPaneMargin.Horizontal) + OutlineGroupPadding.Horizontal,
                szGrid.Height * (StimulusSize.Height + StimulusPaneMargin.Vertical + LeftRadios[0].Height + RadioPadding.Vertical) + OutlineGroupPadding.Vertical);
            Size szDelta = new Size(20, 20);
            this.Size = szForm;
            this.Location = PointToScreen(new Point(-(StimulusPanes[0].Size.Width * (StimuliPerRow - 1)), 0));
            while ((this.Width < szForm.Width) || (this.Height < szForm.Height))
            {
                SuspendLayout();
                if (this.Width + szDelta.Width > szForm.Width)
                    szDelta.Width = szForm.Width - this.Width;
                if (this.Height + szDelta.Height > szForm.Height)
                    szDelta.Height = szForm.Height - this.Height;
                Point ptLocation = PointToScreen(new Point(-szDelta.Width, -szDelta.Height));
                SetDesktopBounds(ptLocation.X, ptLocation.Y, this.Width + szDelta.Width, this.Height + szDelta.Height);
                ResumeLayout(true);
            }
            
            OutlineGroup.Size = szForm;
            SuspendLayout();
            for (int ctr = 0; ctr < StimulusPanes.Count; ctr++)
            {
                StimulusPanes[ctr].Location = new Point(OutlineGroupPadding.Left + ((StimulusSize.Width + StimulusPaneMargin.Horizontal) * ((StimuliPerRow - ctr) % StimuliPerRow)) + StimulusPaneMargin.Left,
                    OutlineGroupPadding.Top + ((StimulusPaneMargin.Vertical + RadioPadding.Vertical + LeftRadios[0].Height + StimulusSize.Height) * (ctr / StimuliPerRow)) + 
                    StimulusPaneMargin.Top);
                OutlineGroup.Controls.Add(StimulusPanes[ctr]);
                RadioPanels[ctr].Location = new Point(StimulusPanes[ctr].Left, StimulusPanes[ctr].Bottom + RadioPadding.Top);
                OutlineGroup.Controls.Add(RadioPanels[ctr]);
            }
            ResumeLayout(true);
            for (int ctr = 0; ctr < LoadStimuli.Count; ctr++)
                StimulusPanes[ctr].PreviewedItem = LoadStimuli[ctr];
        }

        private void LeftKeyRadio_CheckedChanged(object sender, EventArgs e)
        {
            if (((RadioButton)sender).Checked == true)
                StimulusPanes[LeftRadios.IndexOf((RadioButton)sender)]).PreviewedItem.KeyedDir = CIATItem.EKeyedDir.DynamicLeft;
        }

        private void RightKeyRadio_CheckedChanged(object sender, EventArgs e)
        {
            if (((RadioButton)sender).Checked == true)
                StimulusPanes[RightRadios.IndexOf((RadioButton)sender)].PreviewedItem.KeyedDir = CIATItem.EKeyedDir.DynamicRight;
        }
    }
}
*/
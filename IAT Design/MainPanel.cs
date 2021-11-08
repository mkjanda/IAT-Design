using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace IATClient
{
    public partial class MainPanel : UserControl
    {
        // the size of the main panel
 //       public static Size MainPanelSize = new Size(1010, 645);
        private Panel ContentsPanel = new Panel();
        private Dictionary<IContentsItem, CollapsableTreeButton> ContentsDictionary = new Dictionary<IContentsItem, CollapsableTreeButton>();
        private ImageDisplay PreviewPanel = new ImageDisplay();
        private Panel MessagePanel = new Panel();
        private Label AddContentsLabel = new Label();
        private Label ClickToPreviewLabel = new Label();
        private IPreviewableItem SelectedItem { get; set; } = null;
        private Panel EditPanel = new Panel();
        private Button EditButton = new Button();
        private Panel MovePanel = new Panel();
        private Panel DeletePanel = new Panel();
        private Button DeleteButton = new Button();
        private Button MoveUpButton = new Button();
        private Button MoveDownButton = new Button();
        private Dictionary<LinkLabel, CValidationException> ErrorDictionary = new Dictionary<LinkLabel, CValidationException>();
        private System.Drawing.Color DeleteColor = System.Drawing.Color.FromArgb(80, System.Drawing.Color.Red);
        private System.Drawing.Color MoveColor = System.Drawing.Color.FromArgb(100, System.Drawing.Color.SkyBlue);
        private System.Drawing.Color EditColor = System.Drawing.Color.FromArgb(100, System.Drawing.Color.SkyBlue);
        private Size MaxPreviewSize;
        private IATConfigMainForm _MainForm = null;

        /*
        public String IATName
        {
            get
            {
                return PackagePanel.IATName;
            }
            set
            {
                PackagePanel.IATName = value;
            }
        }

        public String PackagedIATTargetDirectory
        {
            get
            {
                return PackagePanel.PackagedIATTargetDirectory;
            }
            set
            {
                PackagePanel.PackagedIATTargetDirectory = value;
            }
        }
*/
        private IATConfigMainForm MainForm
        {
            get
            {
                return _MainForm;
            }
        }

        public MainPanel(IATConfigMainForm mainForm)
        {
            _MainForm = mainForm;
            InitializeComponent();
            ContentsPanel.Dock = DockStyle.Fill;
            ContentsPanel.AutoScroll = true;
            TestContentsGroup.Controls.Add(ContentsPanel);

            PreviewPanel.BackgroundImageLayout = ImageLayout.Stretch;
            PreviewGroup.Controls.Add(PreviewPanel);
            MaxPreviewSize = new Size(PreviewGroup.Size.Width - 6, PreviewGroup.Size.Height - 18);

            MessagePanel.Dock = DockStyle.Fill;
            MessagePanel.AutoScroll = true;
            MessageBoxGroup.Controls.Add(MessagePanel);

            AddContentsLabel.Text = Properties.Resources.sContentsEmptyMessage;
            AddContentsLabel.Font = new Font(System.Drawing.SystemFonts.DefaultFont.FontFamily, 18);
            AddContentsLabel.Dock = DockStyle.Fill;
            AddContentsLabel.TextAlign = ContentAlignment.MiddleCenter;

            ClickToPreviewLabel.Text = Properties.Resources.sClickToPreview;
            ClickToPreviewLabel.Font = new Font(System.Drawing.SystemFonts.DefaultFont.FontFamily, 18);
            ClickToPreviewLabel.ForeColor = System.Drawing.Color.White;
            ClickToPreviewLabel.Dock = DockStyle.Fill;
            ClickToPreviewLabel.TextAlign = ContentAlignment.MiddleCenter;

            EditPanel.BackColor = System.Drawing.Color.Transparent;
            EditButton.Text = "Edit";
            EditButton.Size = new Size(60, 26);
            EditButton.ForeColor = System.Drawing.Color.Blue;
            EditButton.Location = new Point(0, 0);
            EditButton.Click += new EventHandler(EditButton_Click);
            EditButton.BackColor = EditColor;
            EditPanel.Controls.Add(EditButton);

            MovePanel.BackColor = System.Drawing.Color.Transparent;
            MoveUpButton.Text = "Move Up";
            MoveUpButton.Size = new Size(60, 26);
            MoveUpButton.Location = new Point(0, 0);
            MoveUpButton.Click += new EventHandler(MoveUpButton_Click);
            MoveUpButton.BackColor = MoveColor;
            MoveUpButton.ForeColor = System.Drawing.Color.Blue;
            MovePanel.Controls.Add(MoveUpButton);
            MoveDownButton.Text = "Move Down";
            MoveDownButton.Location = new Point(60, 0);
            MoveDownButton.Size = new Size(76, 26);
            MoveDownButton.Click += new EventHandler(MoveDownButton_Click);
            MoveDownButton.BackColor = MoveColor;
            MoveDownButton.ForeColor = System.Drawing.Color.Blue;
            MovePanel.Controls.Add(MoveDownButton);

            DeletePanel.BackColor = System.Drawing.Color.Transparent;
            DeleteButton.Text = "Delete";
            DeleteButton.Location = new Point(0, 0);
            DeleteButton.Size = new Size(48, 26);
            DeleteButton.Click += new EventHandler(DeleteButton_Click);
            DeleteButton.BackColor = DeleteColor;
            DeleteButton.ForeColor = System.Drawing.Color.Red;
            DeletePanel.Controls.Add(DeleteButton);
            PreviewPanel.Size = Images.ImageMediaType.FullWindow.ImageSize;
            PreviewPanel.Location = new Point(3, 15);
            KeyDynamicButton.Enabled = false;
        }

        private void DisplayValidationResults()
        {
            Point loc = new Point(15, 5);
            ErrorDictionary.Clear();
            MessagePanel.Controls.Clear();
            foreach (CValidationException ve in CItemValidator.ErrorDictionary.Values.ToList())
            {
                LinkLabel ll = new LinkLabel();
                ll.Size = TextRenderer.MeasureText(ve.ErrorText, ll.Font);
                ll.Location = loc;
                ll.Text = ve.ErrorText;
                ll.Click += new EventHandler(this.ErrorLabel_Click);
                MessagePanel.Controls.Add(ll);
                ErrorDictionary[ll] = ve;
                loc += new Size(0, ll.Height);
            }
        }

        private void ValidateTest()
        {
            CItemValidator.StartValidation();
            foreach (CIATBlock b in CIAT.SaveFile.IAT.Blocks)
                CItemValidator.ValidateItem(b);
            foreach (CInstructionBlock ib in CIAT.SaveFile.IAT.InstructionBlocks)
                CItemValidator.ValidateItem(ib);
            DisplayValidationResults();
        }

        private new void Validate()
        {
            if (IATConfigMainForm.FirstValidation)
            {
                ValidateTest();
                if (CItemValidator.HasErrors)
                    Generate7BlockButton.Enabled = false;
                else if (CIAT.SaveFile.IAT.Blocks.Count == 2)
                    Generate7BlockButton.Enabled = true;
                else
                    Generate7BlockButton.Enabled = false;
                IATConfigMainForm.FirstValidation = false;
            }
            else
            {
                if (IsDisposed)
                    return;
                this.BeginInvoke(new Action(() =>
                {
                    ValidateTest();
                    if (CItemValidator.HasErrors)
                        Generate7BlockButton.Enabled = false;
                    else if (CIAT.SaveFile.IAT.Blocks.Count == 2)
                        Generate7BlockButton.Enabled = true;
                    else
                        Generate7BlockButton.Enabled = false;
                    IATConfigMainForm.FirstValidation = false;
                }));
            }
        }

        private void MainPanel_ParentChanged(object sender, EventArgs e)
        {
            if (Parent != null)
            {
                if (ContentsDictionary.Count == 0)
                {
                    SuspendLayout();
                    ContentsPanel.Controls.Add(AddContentsLabel);
                    GetFreshPreviewPanel(null);
                    PreviewPanel.BackColor = System.Drawing.Color.Black;
                    ResumeLayout();
                }
                else if ((SelectedItem != null) && !SelectedItem.IsDisposed)
                {
                    SelectedItem.Preview(GetFreshPreviewPanel(SelectedItem));
                }
                else
                {
                    SelectedItem = null;
                    SuspendLayout();
                    GetFreshPreviewPanel(null);
                    ResumeLayout(false);
                }
                bScrollBarsVisible = false;
                if (CIAT.SaveFile.IAT.Blocks.Count < 2)
                    AddIATBlockButton.Enabled = true;
                else
                    AddIATBlockButton.Enabled = false;
                Validate();
            }
        }

        public void EndPreview()
        {
            if (SelectedItem != null)
                if (!SelectedItem.IsDisposed)
                    SelectedItem.EndPreview(PreviewPanel);

        }

        private void ErrorLabel_Click(object sender, EventArgs e)
        {
            ErrorDictionary[(LinkLabel)sender].LocationDescriptor.OpenItem(MainForm);
        }
        /*
        private void PackageButton_Click(object sender, EventArgs e)
        {
            CItemValidator.StartValidation();
            CIAT theIAT = ((IATConfigMainForm)Parent).IAT;
            foreach (CIATBlock b in theIAT.Blocks)
                CItemValidator.ValidateItem(b);
            foreach (CInstructionBlock ib in theIAT.InstructionBlocks)
                CItemValidator.ValidateItem(ib);
            if (CItemValidator.HasErrors)
            {
                CItemValidator.DisplayErrors((IATConfigMainForm)Parent);
                return;
            }
            PackagerForm packagerForm = new PackagerForm((IATConfigMainForm)Parent);
            packagerForm.theIAT = ((IATConfigMainForm)Parent).IAT;
            packagerForm.ClientSize = PackagerForm.ChildControlSize;
            packagerForm.ShowDialog(this);
        }
         * */

        private bool Upload_IATExists()
        {
            if (MessageBox.Show(this, Properties.Resources.sIATExistsCaption, Properties.Resources.sIATExistsMsg, MessageBoxButtons.YesNo) == DialogResult.Yes)
                return true;
            return false;
        }

        public void UpdateContentsItem(IContentsItem item)
        {
            SuspendLayout();
            ContentsDictionary[item].SetChildItems(item.SubContentsItems);
            RecalcLayout();
            ResumeLayout(false);
        }

        public void PopulateContents(ContentsList Contents)
        {
            SuspendLayout();
            CollapsableTreeButton treeButton;
            Point loc = new Point(0, 0);
            ContentsDictionary.Clear();
            ContentsPanel.Controls.Clear();
            foreach (IContentsItem ci in Contents)
            {
                treeButton = new CollapsableTreeButton(ci, (IATConfigMainForm)Parent, ContentsPanel.Width);
                treeButton.RecalcLayout += new CollapsableTreeButton.RecalcLayoutHandler(ContentsButton_StateChange);
                treeButton.OnRetrievePreviewPanel += new CollapsableTreeButton.RetrievePreviewPanelCallback(GetFreshPreviewPanel);
                ContentsDictionary[ci] = treeButton;
                treeButton.Location = loc;
                ContentsPanel.Controls.Add(treeButton);
                loc.Y += treeButton.Height;
            }
            if (Contents.Count == 0)
                ContentsPanel.Controls.Add(AddContentsLabel);
            RecalcLayout();
            ResumeLayout(false);
            Validate();
        }

        private void ContentsButton_StateChange(IContentsItem sender, Size sz)
        {
            SuspendLayout();
            for (int ctr = sender.IndexInContents + 1; ctr < CIAT.SaveFile.IAT.Contents.Count; ctr++)
                ContentsDictionary[CIAT.SaveFile.IAT.Contents[ctr]].Location = new Point(0, ContentsDictionary[CIAT.SaveFile.IAT.Contents[ctr - 1]].Bottom);
            RecalcLayout();
            ResumeLayout(false);
        }

        private ImageDisplay GetFreshPreviewPanel(IPreviewableItem UpdatingItem)
        {
            SuspendLayout();
            PreviewPanel.Controls.Clear();
            PreviewPanel.ClearImage();
            if (UpdatingItem == null)
            {
                double arPreview = (double)CIAT.SaveFile.Layout.InteriorSize.Width / (double)CIAT.SaveFile.Layout.InteriorSize.Height;
                if (arPreview > 1)
                {
                    PreviewPanel.Size = new Size(MaxPreviewSize.Width, (int)(MaxPreviewSize.Height / arPreview));
                    PreviewPanel.Location = new Point(0, ((PreviewGroup.Height - PreviewPanel.Height) >> 1));
                }
                else
                {
                    PreviewPanel.Size = new Size((int)(MaxPreviewSize.Width * arPreview), MaxPreviewSize.Height);
                    PreviewPanel.Location = new Point((PreviewGroup.Width - PreviewPanel.Width) >> 1, 0);
                }
                return PreviewPanel;
            }
            if (!UpdatingItem.IsSurvey)
            {
                double arPreview = (double)CIAT.SaveFile.Layout.InteriorSize.Width / (double)CIAT.SaveFile.Layout.InteriorSize.Height;
                if (arPreview > 1)
                {
                    PreviewPanel.Size = new Size(MaxPreviewSize.Width, (int)(MaxPreviewSize.Height / arPreview));
                    PreviewPanel.Location = new Point(0, ((PreviewGroup.Height - PreviewPanel.Height) >> 1));
                }
                else
                {
                    PreviewPanel.Size = new Size((int)(MaxPreviewSize.Width * arPreview), MaxPreviewSize.Height);
                    PreviewPanel.Location = new Point((PreviewGroup.Width - PreviewPanel.Width) >> 1, 0);
                }
            }
            else
                PreviewPanel.Dock = DockStyle.Fill;
            PreviewPanel.Controls.Clear();

            EditPanel.Size = new Size(60, 26);
            EditPanel.Location = new Point((PreviewPanel.Width >> 1) + (EditPanel.Width), (PreviewPanel.Height >> 1) + (EditPanel.Height << 1));

            MovePanel.Size = new Size(136, 26);
            MovePanel.Location = new Point((PreviewPanel.Width >> 1) - MovePanel.Width, (PreviewPanel.Height >> 1) + (MovePanel.Height << 1));


            DeletePanel.Size = new Size(48, 26);
            DeletePanel.Location = new Point((PreviewPanel.Width - EditPanel.Width) >> 1, PreviewPanel.Height - EditPanel.Height);

            if ((UpdatingItem == null) && (ContentsDictionary.Count > 0))
                PreviewPanel.Controls.Add(ClickToPreviewLabel);
            if (SelectedItem != null)
            {
                if (SelectedItem.IsHeaderItem)
                {
                    if (((IContentsItem)SelectedItem).AlternationGroup == null)
                        SelectedItem.GUIButton.BackColor = CollapsableTreeButton.HeaderBaseColor;
                    else
                        SelectedItem.GUIButton.BackColor = CollapsableTreeButton.AlternateBaseColors[((IContentsItem)SelectedItem).AlternationGroup.GroupID % 10];
                }
                else
                    SelectedItem.GUIButton.BackColor = CollapsableTreeButton.ChildBaseColor;
            } 
            SelectedItem = UpdatingItem;
            if (SelectedItem != null)
            {
                if (SelectedItem.IsHeaderItem)
                {
                    EditPanel.Location = new Point((PreviewPanel.Width >> 1) + (EditPanel.Width), (PreviewPanel.Height >> 1) + (EditPanel.Height << 1));
                    PreviewPanel.Controls.Add(EditPanel);
                    PreviewPanel.Controls.Add(DeletePanel);
                    PreviewPanel.Controls.Add(MovePanel);
                    if (((IContentsItem)SelectedItem).AlternationGroup == null)
                        SelectedItem.GUIButton.BackColor = CollapsableTreeButton.HeaderHighlightColor;
                    else
                        SelectedItem.GUIButton.BackColor = CollapsableTreeButton.AlternateHighlightColors[((IContentsItem)SelectedItem).AlternationGroup.GroupID % 10];
                }
                else
                {
                    EditPanel.Location = new Point((PreviewPanel.Width - EditPanel.Width) >> 1, PreviewPanel.Height >> 1);
                    PreviewPanel.Controls.Add(EditPanel);
                    SelectedItem.GUIButton.BackColor = CollapsableTreeButton.ChildHighlightColor;
                }
            }
            ResumeLayout(false);
            return PreviewPanel;
        }

        private bool bScrollBarsVisible = false;

        private void RecalcLayout()
        {
            int currScrollPos = ContentsPanel.VerticalScroll.Value;
            Size docSize = new Size(ContentsPanel.Width, 0);
            foreach (CollapsableTreeButton ctb in ContentsDictionary.Values)
                docSize.Height += ctb.Height;
            ContentsPanel.VerticalScroll.Minimum = 0;
            ContentsPanel.VerticalScroll.Maximum = docSize.Height;
            ContentsPanel.VerticalScroll.SmallChange = docSize.Height >> 16;
            ContentsPanel.VerticalScroll.LargeChange = docSize.Height >> 4;
            if (docSize.Height > ContentsPanel.Height)
            {
                if (!bScrollBarsVisible)
                {
                    Graphics g = Graphics.FromHwnd(ContentsPanel.Handle);
                    int width = ContentsPanel.Width - System.Windows.Forms.ScrollBarRenderer.GetSizeBoxSize(g, System.Windows.Forms.VisualStyles.ScrollBarState.Normal).Width - 1;
                    g.Dispose();
                    foreach (CollapsableTreeButton ctb in ContentsDictionary.Values)
                        ctb.Width = width;
                    bScrollBarsVisible = true;
                }
            }
            else
            {
                if (bScrollBarsVisible)
                    foreach (CollapsableTreeButton ctb in ContentsDictionary.Values)
                        ctb.Width = ContentsPanel.Width;
                bScrollBarsVisible = false;
            }
            if (currScrollPos > docSize.Height)
                currScrollPos = docSize.Height;
            AdjustFormScrollbars(false);
        }

        private void ContentsPanel_Scroll(object sender, ScrollEventArgs args)
        {/*
            ContentsPanel.SuspendLayout();
            EditPanel.Location += new Size(0, args.NewValue);
            ContentsPanel.ResumeLayout();
          * */
        }


        private void AddIATBlockButton_Click(object sender, EventArgs e)
        {
            if (ContentsPanel.Controls.Contains(AddContentsLabel))
                ContentsPanel.Controls.Remove(AddContentsLabel);
            String name = String.Format("IAT Block #{0}", CIAT.SaveFile.IAT.Blocks.Count + 1);
            CIATBlock block = new CIATBlock(CIAT.SaveFile.IAT);
            MainForm.Modified = true;
            MainForm.ConstructViewMenu(true);
            if (CIAT.SaveFile.IAT.Blocks.Count == 2)
                Generate7BlockButton.Enabled = true;
            SuspendLayout();
            CollapsableTreeButton treeButton = new CollapsableTreeButton(block, MainForm, ContentsPanel.ClientSize.Width);
            ContentsPanel.Controls.Add(treeButton);
            treeButton.RecalcLayout += new CollapsableTreeButton.RecalcLayoutHandler(ContentsButton_StateChange);
            treeButton.OnRetrievePreviewPanel += new CollapsableTreeButton.RetrievePreviewPanelCallback(GetFreshPreviewPanel);
            if (CIAT.SaveFile.IAT.Contents.Count == 0)
            {
                treeButton.Location = new Point(0, 0);
                block.AddToIAT(0);
            }
            else if (CIAT.SaveFile.IAT.AfterSurvey.Count == 0)
            {
                treeButton.Location = new Point(0, ContentsDictionary[CIAT.SaveFile.IAT.Contents.Last()].Bottom);
                block.AddToIAT(CIAT.SaveFile.IAT.Contents.Count);
            }
            else
            {
                int afterPos = int.MaxValue;
                foreach (CSurvey s in CIAT.SaveFile.IAT.AfterSurvey)
                    if (s.IndexInContents < afterPos)
                        afterPos = s.IndexInContents;
                foreach (IContentsItem item in ContentsDictionary.Keys)
                    if (item.IndexInContents >= afterPos)
                        ContentsDictionary[item].Location += new Size(0, treeButton.Height);
                treeButton.Location = new Point(0, ContentsDictionary[CIAT.SaveFile.IAT.Contents[afterPos - 1]].Bottom);
                block.AddToIAT(afterPos);
            }
            ContentsDictionary[block] = treeButton;
            RecalcLayout();
            ResumeLayout(false);
            ContentsPanel.PerformLayout();
            if (CIAT.SaveFile.IAT.Blocks.Count >= 2)
                AddIATBlockButton.Enabled = false;
            Validate();
        }

        private void EditButton_Click(object sender, EventArgs e)
        {
            SelectedItem.OpenItem(MainForm);
        }

        private void DeleteButton_Click(object sender, EventArgs e)
        {
            SuspendLayout();
            CollapsableTreeButton deletedItem = ContentsDictionary[(IContentsItem)SelectedItem];
            foreach (CollapsableTreeButton ctb in ContentsDictionary.Values)
            {
                if (ctb.Location.Y >= deletedItem.Bottom)
                    ctb.Location -= new Size(0, deletedItem.Height);
            }
            ContentsPanel.Controls.Remove(deletedItem);
            ContentsDictionary.Remove((IContentsItem)SelectedItem);
            ((IContentsItem)SelectedItem).DeleteFromIAT();
            if (CIAT.SaveFile.IAT.Blocks.Count < 2)
            {
                AddIATBlockButton.Enabled = true;
            }
            ((IContentsItem)SelectedItem).Dispose();
            if (ContentsDictionary.Count == 0)
                ContentsPanel.Controls.Add(AddContentsLabel);
            RecalcLayout();
            ResumeLayout();
            GetFreshPreviewPanel(null);
            Validate();
        }

        private void MoveUpButton_Click(object sender, EventArgs e)
        {
            IContentsItem item = (IContentsItem)SelectedItem;
            int pos = item.IndexInContents;
            ContentsPanel.SuspendLayout();
            int downshift = ContentsDictionary[item].Height, upshift = 0;
            CIAT.SaveFile.IAT.MoveContentsItem(item, -1);
            for (int ctr = pos; ctr >= item.IndexInContents; ctr--)
            {
                upshift += ContentsDictionary[CIAT.SaveFile.IAT.Contents[ctr]].Height;
                ContentsDictionary[CIAT.SaveFile.IAT.Contents[ctr]].Location =
                    new Point(ContentsDictionary[CIAT.SaveFile.IAT.Contents[ctr]].Location.X, ContentsDictionary[CIAT.SaveFile.IAT.Contents[ctr]].Location.Y + downshift);
            }
            ContentsDictionary[item].Location = ContentsDictionary[item].Location + new Size(0, -upshift);
            ContentsPanel.ResumeLayout(false);
        }

        private void MoveDownButton_Click(object sender, EventArgs e)
        {
            IContentsItem item = (IContentsItem)SelectedItem;
            int pos = item.IndexInContents;
            CIAT.SaveFile.IAT.MoveContentsItem(item, 1);
            ContentsPanel.SuspendLayout();
            int upshift = ContentsDictionary[item].Height, downshift = 0;
            for (int ctr = pos; ctr <= item.IndexInContents; ctr++) {
                downshift += ContentsDictionary[CIAT.SaveFile.IAT.Contents[ctr]].Height;
                ContentsDictionary[CIAT.SaveFile.IAT.Contents[ctr]].Location = 
                    new Point(ContentsDictionary[CIAT.SaveFile.IAT.Contents[ctr]].Location.X, ContentsDictionary[CIAT.SaveFile.IAT.Contents[ctr]].Location.Y - upshift);
            }
            ContentsDictionary[item].Location = ContentsDictionary[item].Location + new Size(0, downshift); 
            ContentsPanel.ResumeLayout();
        }

        private void AddInstructionBlockButton_Click(object sender, EventArgs e)
        {
            if (ContentsPanel.Controls.Contains(AddContentsLabel))
                ContentsPanel.Controls.Remove(AddContentsLabel);
            String name = String.Format("Instruction Block #{0}", CIAT.SaveFile.IAT.InstructionBlocks.Count + 1);
            CInstructionBlock iBlock = new CInstructionBlock(CIAT.SaveFile.IAT);
            iBlock.Name = name;
            MainForm.Modified = true;
            MainForm.ConstructViewMenu(true);
            SuspendLayout();
            CollapsableTreeButton treeButton = new CollapsableTreeButton(iBlock, MainForm, ContentsPanel.ClientSize.Width);
            ContentsPanel.Controls.Add(treeButton);
            treeButton.RecalcLayout += new CollapsableTreeButton.RecalcLayoutHandler(ContentsButton_StateChange);
            treeButton.OnRetrievePreviewPanel += new CollapsableTreeButton.RetrievePreviewPanelCallback(GetFreshPreviewPanel);
            if (CIAT.SaveFile.IAT.Contents.Count == 0)
            {
                treeButton.Location = new Point(0, 0);
                iBlock.AddToIAT(0);
            }
            else if (CIAT.SaveFile.IAT.AfterSurvey.Count == 0)
            {
                treeButton.Location = new Point(0, ContentsDictionary[CIAT.SaveFile.IAT.Contents.Last()].Bottom);
                iBlock.AddToIAT(CIAT.SaveFile.IAT.Contents.Count);
            }
            else
            {
                int afterPos = int.MaxValue;
                foreach (CSurvey s in CIAT.SaveFile.IAT.AfterSurvey)
                    if (s.IndexInContents < afterPos)
                        afterPos = s.IndexInContents;
                foreach (IContentsItem item in ContentsDictionary.Keys)
                    if (item.IndexInContents >= afterPos)
                        ContentsDictionary[item].Location += new Size(0, treeButton.Height);
                treeButton.Location = new Point(0, ContentsDictionary[CIAT.SaveFile.IAT.Contents[afterPos - 1]].Bottom);
                iBlock.AddToIAT(afterPos);
            }
            ContentsDictionary[iBlock] = treeButton;
            RecalcLayout();
            ResumeLayout(false);
            ContentsPanel.PerformLayout();
            Validate();
        }

        private void AddPreSurveyButton_Click(object sender, EventArgs e)
        {
            if (ContentsPanel.Controls.Contains(AddContentsLabel))
                ContentsPanel.Controls.Remove(AddContentsLabel);
            CSurvey iSurvey = new CSurvey(CSurvey.EOrdinality.Before);
            MainForm.Modified = true;
            MainForm.ConstructViewMenu(true);
            SuspendLayout();
            CollapsableTreeButton treeButton = new CollapsableTreeButton(iSurvey, MainForm, ContentsPanel.ClientSize.Width);
            ContentsPanel.Controls.Add(treeButton);
            treeButton.RecalcLayout += new CollapsableTreeButton.RecalcLayoutHandler(ContentsButton_StateChange);
            treeButton.OnRetrievePreviewPanel += new CollapsableTreeButton.RetrievePreviewPanelCallback(GetFreshPreviewPanel);
            if (CIAT.SaveFile.IAT.Contents.Count == 0)
            {
                treeButton.Location = new Point(0, 0);
                iSurvey.AddToIAT(0);
            }
            else if (CIAT.SaveFile.IAT.BeforeSurvey.Count == 0)
            {
                foreach (CollapsableTreeButton ctb in ContentsDictionary.Values)
                    ctb.Location += new Size(0, treeButton.Height);
                treeButton.Location = new Point(0, 0);
                iSurvey.AddToIAT(0);
            }
            else
            {
                treeButton.Location = new Point(0, ContentsDictionary[CIAT.SaveFile.IAT.BeforeSurvey.Last()].Bottom);
                foreach (IContentsItem item in ContentsDictionary.Keys)
                    if (item.IndexInContents >= CIAT.SaveFile.IAT.BeforeSurvey.Count)
                        ContentsDictionary[item].Location += new Size(0, treeButton.Height);
                iSurvey.AddToIAT(CIAT.SaveFile.IAT.BeforeSurvey.Count);
            }
            ContentsDictionary[iSurvey] = treeButton;
            RecalcLayout();
            ResumeLayout(false);
            ContentsPanel.PerformLayout();
        }

        private void AddPostSurveyButton_Click(object sender, EventArgs e)
        {
            if (ContentsPanel.Controls.Contains(AddContentsLabel))
                ContentsPanel.Controls.Remove(AddContentsLabel);
            CSurvey iSurvey = new CSurvey(CSurvey.EOrdinality.After);
            MainForm.Modified = true;
            MainForm.ConstructViewMenu(true);
            SuspendLayout();
            CollapsableTreeButton treeButton = new CollapsableTreeButton(iSurvey, MainForm, ContentsPanel.ClientSize.Width);
            ContentsPanel.Controls.Add(treeButton);
            treeButton.RecalcLayout += new CollapsableTreeButton.RecalcLayoutHandler(ContentsButton_StateChange);
            treeButton.OnRetrievePreviewPanel += new CollapsableTreeButton.RetrievePreviewPanelCallback(GetFreshPreviewPanel);
            if (CIAT.SaveFile.IAT.Contents.Count == 0)
            {
                treeButton.Location = new Point(0, 0);
                iSurvey.AddToIAT(0);
            }
            else
            {
                treeButton.Location = new Point(0, ContentsDictionary[CIAT.SaveFile.IAT.Contents.Last()].Bottom);
                iSurvey.AddToIAT(CIAT.SaveFile.IAT.Contents.Count);
            }
            ContentsDictionary[iSurvey] = treeButton;
            RecalcLayout();
            ResumeLayout(false);
            ContentsPanel.PerformLayout();
        }

        private void Generate7BlockButton_Click(object sender, EventArgs e)
        {
            ContentsPanel.SuspendLayout();
            C7BlockIATGenerator generator = new C7BlockIATGenerator(CIAT.SaveFile.IAT);
            generator.Generate(true);
            PopulateContents(CIAT.SaveFile.IAT.Contents);
            Generate7BlockButton.Enabled = false;
            ContentsPanel.ResumeLayout(false);
        }

        private void SpecifyAlternatingItemsButton_Click(object sender, EventArgs e)
        {
            SpecifyAlterateBlocksDlg alternationSpecifier = new SpecifyAlterateBlocksDlg();
            alternationSpecifier.theIAT = CIAT.SaveFile.IAT;
            alternationSpecifier.ShowDialog();
            PopulateContents(CIAT.SaveFile.IAT.Contents);
        }

        private void PackageIATButton_Click(object sender, EventArgs e)
        {/*
            PackagerForm packageForm = new PackagerForm(MainForm);
            packageForm.Size = new Size(330, 370);
            packageForm.ShowDialog(this);
          */
        }

        private void ServerInterfaceButton_Click(object sender, EventArgs e)
        {
            MainForm.FormContents = IATConfigMainForm.EFormContents.ServerInterface;
        }

        private void KeyDynamicButton_Click(object sender, EventArgs e)
        {
            MainForm.Modified = true;
            if (((IContentsItem)SelectedItem).Type == ContentsItemType.IATBlock)
            {
                MainForm.ActiveItem = (IContentsItem)SelectedItem;
                MainForm.FormContents = IATConfigMainForm.EFormContents.DynamicallyKey;
            }

        }

        private void PurchaseAdministrationsButton_MouseClick(object sender, MouseEventArgs e)
        {
            MainForm.FormContents = IATConfigMainForm.EFormContents.PurchasePage;
        }

    }
}

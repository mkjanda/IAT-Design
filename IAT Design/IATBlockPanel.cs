using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
namespace IATClient
{
    public partial class IATBlockPanel : UserControl
    {
        // the size of the control and child controls
        public static Size IATBlockPanelSize = new Size(1010, 645);
        private ImageDisplay PreviewPanel;
        private GroupBox PreviewGroup;
        private static Size ScrollingPreviewSize = new Size(1010, 122);
        private static Size ScrollingPreviewPanelSize = new Size(112, 112);
        private static Point ScrollingPreviewPos = new Point(0, 525);
        private ScrollingPreviewPanel ScrollingPreview;
        private StimulusDefinitionPanel StimulusPanel;
        private static Point StimulusPanelPos = new Point(530, 80);
        private static Size StimulusPanelSize = StimulusDefinitionPanel.StimulusDefinitionPanelSize;
        private int _SelectedItemNdx = -1;

        public int SelectedItemNdx
        {
            get
            {
                return _SelectedItemNdx;
            }
            set
            {
                try
                {
                    CIATBlock b = CIAT.SaveFile.GetIATBlock(BlockUri);
                    if (_SelectedItemNdx != -1)
                    {
                        b[_SelectedItemNdx].GetPreview(BlockUri).PreviewPanel = null;
                        PreviewPanel.ClearImage();
                    }
                    if (value == -1)
                        return;
                    _SelectedItemNdx = value;
                    b[SelectedItemNdx].SetPreviewPane(BlockUri, PreviewPanel);
                    StimulusPanel.IATItem = b[value];
                    b[SelectedItemNdx].GetPreview(BlockUri).ResumeLayout(true);
                    if (b[SelectedItemNdx].ThumbnailPreviewPanel == null)
                        return;
                    int previewRight = b[SelectedItemNdx].ThumbnailPreviewPanel.Location.X + b[SelectedItemNdx].ThumbnailPreviewPanel.Width + ScrollingPreviewPanel.PreviewPadding.Horizontal;
                    int previewLeft = b[SelectedItemNdx].ThumbnailPreviewPanel.Location.X - ScrollingPreviewPanel.PreviewPadding.Left;
                    ScrollingPreview.ScrollDiff = 10;
                    if (previewRight > ScrollingPreview.PreviewBarWidth)
                        ScrollingPreview.ScrollBy(previewRight - ScrollingPreview.PreviewBarWidth);
                    if (previewLeft < 0)
                        ScrollingPreview.ScrollBy(previewLeft);
                }
                finally
                {
                    this.Enabled = true;
                }
            }
        }

        public delegate bool IsDynamicallyKeyedCallback();

        public static Point InstructionsEditPosition = new Point(530, 375);
        public static Size InstructionsEditSize = new Size(350, 145);

        // the child item controls
        private TextEditControl InstructionsEdit;

        public Uri BlockUri { get; private set; }

        public CIATItem ActiveItem
        {
            get
            {
                return StimulusPanel.IATItem;
            }
        }

        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.AddStimulus = new System.Windows.Forms.Button();
            this.InsertStimulus = new System.Windows.Forms.Button();
            this.DeleteStimulus = new System.Windows.Forms.Button();
            this.Done = new System.Windows.Forms.Button();
            this.ResponseKeyLabel = new System.Windows.Forms.Label();
            this.ResponseKeyDrop = new System.Windows.Forms.ComboBox();
            this.ManageKeys = new System.Windows.Forms.Button();
            this.DynamicallyKeyedCheck = new System.Windows.Forms.CheckBox();
            // 
            // AddStimulus
            // 
            this.AddStimulus.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.AddStimulus.Name = "AddStimulus";
            this.AddStimulus.Size = new System.Drawing.Size(108, 28);
            this.AddStimulus.TabIndex = 4;
            this.AddStimulus.Text = "Add Stimulus";
            this.AddStimulus.UseVisualStyleBackColor = true;
            this.AddStimulus.Click += new System.EventHandler(this.AddStimulus_Click);
            // 
            // InsertStimulus
            // 
            this.InsertStimulus.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.InsertStimulus.Name = "InsertStimulus";
            this.InsertStimulus.Size = new System.Drawing.Size(108, 28);
            this.InsertStimulus.TabIndex = 5;
            this.InsertStimulus.Text = "Insert Stimulus";
            this.InsertStimulus.UseVisualStyleBackColor = true;
            this.InsertStimulus.Click += new System.EventHandler(this.InsertStimulus_Click);
            // 
            // DeleteStimulus
            // 
            this.DeleteStimulus.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.DeleteStimulus.Name = "DeleteStimulus";
            this.DeleteStimulus.Size = new System.Drawing.Size(108, 28);
            this.DeleteStimulus.TabIndex = 6;
            this.DeleteStimulus.Text = "Delete Stimulus";
            this.DeleteStimulus.UseVisualStyleBackColor = true;
            this.DeleteStimulus.Click += new System.EventHandler(this.DeleteStimulus_Click);
            // 
            // Done
            // 
            this.Done.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.Done.Name = "Done";
            this.Done.Size = new System.Drawing.Size(108, 28);
            this.Done.TabIndex = 7;
            this.Done.Text = "Done";
            this.Done.UseVisualStyleBackColor = true;
            this.Done.Click += new System.EventHandler(this.Done_Click);
            // 
            // ResponseKeyLabel
            // 
            this.ResponseKeyLabel.AutoSize = true;
            this.ResponseKeyLabel.Location = new System.Drawing.Point(755, 36);
            this.ResponseKeyLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.ResponseKeyLabel.Name = "ResponseKeyLabel";
            this.ResponseKeyLabel.Size = new System.Drawing.Size(104, 17);
            this.ResponseKeyLabel.TabIndex = 0;
            this.ResponseKeyLabel.Text = "Response Key:";
            // 
            // ResponseKeyDrop
            // 
            this.ResponseKeyDrop.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.ResponseKeyDrop.FormattingEnabled = true;
            this.ResponseKeyDrop.Location = new System.Drawing.Point(868, 32);
            this.ResponseKeyDrop.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.ResponseKeyDrop.Name = "ResponseKeyDrop";
            this.ResponseKeyDrop.Size = new System.Drawing.Size(204, 24);
            this.ResponseKeyDrop.TabIndex = 9;
            this.ResponseKeyDrop.SelectedIndexChanged += new System.EventHandler(this.ResponseKeyDrop_SelectedIndexChanged);
            // 
            // ManageKeys
            // 
            this.ManageKeys.Location = new System.Drawing.Point(1081, 32);
            this.ManageKeys.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.ManageKeys.Name = "ManageKeys";
            this.ManageKeys.Size = new System.Drawing.Size(183, 28);
            this.ManageKeys.TabIndex = 10;
            this.ManageKeys.Text = "Create / Manage Keys";
            this.ManageKeys.UseVisualStyleBackColor = true;
            this.ManageKeys.Click += new System.EventHandler(this.ManageKeys_Click);
            // 
            // DynamicallyKeyedCheck
            // 
            this.DynamicallyKeyedCheck.AutoSize = true;
            this.DynamicallyKeyedCheck.Location = new System.Drawing.Point(868, 65);
            this.DynamicallyKeyedCheck.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.DynamicallyKeyedCheck.Name = "DynamicallyKeyedCheck";
            this.DynamicallyKeyedCheck.Size = new System.Drawing.Size(372, 21);
            this.DynamicallyKeyedCheck.TabIndex = 11;
            this.DynamicallyKeyedCheck.Text = "Dynamically key IAT block based on survey responses";
            this.DynamicallyKeyedCheck.UseVisualStyleBackColor = true;
            this.DynamicallyKeyedCheck.CheckedChanged += new System.EventHandler(this.DynamicallyKeyedCheck_CheckedChanged);
            // 
            // IATBlockPanel
            // 
            this.Controls.Add(this.DynamicallyKeyedCheck);
            this.Controls.Add(this.ManageKeys);
            this.Controls.Add(this.ResponseKeyDrop);
            this.Controls.Add(this.ResponseKeyLabel);
            this.Controls.Add(this.Done);
            this.Controls.Add(this.DeleteStimulus);
            this.Controls.Add(this.InsertStimulus);
            this.Controls.Add(this.AddStimulus);
            this.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.Name = "IATBlockPanel";
            //            this.ParentChanged += new System.EventHandler(this.IATBlockPanel_ParentChanged);

        }

        private System.Windows.Forms.Button AddStimulus;
        private System.Windows.Forms.Button InsertStimulus;
        private System.Windows.Forms.Button DeleteStimulus;
        private System.Windows.Forms.Button Done;
        private System.Windows.Forms.Label ResponseKeyLabel;
        private System.Windows.Forms.ComboBox ResponseKeyDrop;
        private System.Windows.Forms.Button ManageKeys;
        private System.Windows.Forms.CheckBox DynamicallyKeyedCheck;



        public bool IsDynamicallyKeyed()
        {
            return DynamicallyKeyedCheck.Checked;
        }

        protected bool IsUpdating;

        /// <summary>
        /// the default constructor
        /// </summary>
        public IATBlockPanel(CIATBlock block)
        {
            this.Name = "IATBlockPanel"; 
            this.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top;
            this.Dock = DockStyle.Fill;
            SuspendLayout();
            InitializeComponent();
            if (CIAT.SaveFile.IAT.Is7Block)
                ResponseKeyDrop.Enabled = false;
            BlockUri = block.URI;
            PreviewPanel = new ImageDisplay();
            PreviewPanel.BackColor = Color.Black;
            StimulusPanel = new StimulusDefinitionPanel(BlockUri);
            StimulusPanel.Size = new Size(StimulusPanelSize.Width, StimulusPanelSize.Height);
            StimulusPanel.Location = new Point(StimulusPanelPos.X, StimulusPanelPos.Y);
            StimulusPanel.BlockDynamicallyKeyed = new IsDynamicallyKeyedCallback(IsDynamicallyKeyed);
            Controls.Add(StimulusPanel);
            PreviewPanel.BackgroundImageLayout = ImageLayout.Zoom;
            PreviewPanel.Size = Images.ImageMediaType.FullPreview.ImageSize;
            PreviewPanel.Location = new Point(10, 12);
            Controls.Add(PreviewPanel);
            PreviewGroup = new GroupBox();
            PreviewGroup.ClientSize = new Size(500, 500) + new Size(12, 18);
            PreviewGroup.Location = new Point(0, 0);
            PreviewGroup.Controls.Add(PreviewPanel);
            Controls.Add(PreviewGroup);
            ScrollingPreview = new ScrollingPreviewPanel();
            ScrollingPreview.Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Right;
            ScrollingPreview.Orientation = ScrollingPreviewPanel.EOrientation.horizontal;
            ScrollingPreview.Location = new Point(ScrollingPreviewPos.X, ScrollingPreviewPos.Y);
            ScrollingPreview.Width = this.Width;
            Controls.Add(ScrollingPreview);
            
            ScrollingPreview.PreviewClickCallback = new Action<int>((newNdx) => { SelectedItemNdx = newNdx; });
            ScrollingPreview.OnMoveContainerItem = new Action<int, int>((startNdx, endNdx) =>
            {
                CIATBlock b = CIAT.SaveFile.GetIATBlock(BlockUri);
                b.MoveItem(startNdx, endNdx);
                SelectedItemNdx = endNdx;
                ScrollingPreview.SelectedPreview = endNdx;
            });
            ScrollingPreview.ResetScroll();
            block.SuspendPreviewLayouts();
            for (int ctr = 0; ctr < block.NumItems; ctr++)
                ScrollingPreview.InsertPreview(ctr, block[ctr]);
            //            CIAT.ImageManager.ForceFetch();
            for (int ctr = 0; ctr < block.NumItems; ctr++)
                block[ctr].GetPreview(BlockUri).SuspendLayout();
            if (block.NumItems == 0)
            {
                CIATItem newItem = new CIATItem();
                block.AddItem(newItem, KeyedDirection.None);
                //              SelectedItemNdx = 0;
                ScrollingPreview.InsertPreview(0, newItem);
            }
            if (CIAT.SaveFile.IAT.Is7Block && (block.IndexInContainer >= 2))
            {
                AddStimulus.Enabled = false;
                InsertStimulus.Enabled = false;
                DeleteStimulus.Enabled = false;
            }
            Controls.Add(ScrollingPreview);
            CreateInstructionsEdit();
            AddStimulus.Location = new Point(InstructionsEdit.Right + 10, InstructionsEdit.Top);
            InsertStimulus.Location = new Point(AddStimulus.Left, AddStimulus.Bottom + (InsertStimulus.Height >> 2));
            DeleteStimulus.Location = new Point(AddStimulus.Left, InsertStimulus.Bottom + (DeleteStimulus.Height >> 2));
            Done.Location = new Point(AddStimulus.Left, DeleteStimulus.Bottom + (Done.Height >> 2));
            this.HandleCreated += (sender, args) =>
            {
                PopulateResponseKeyDrop();
            };
            Controls.Remove(DynamicallyKeyedCheck);
            ResumeLayout(true);

            this.Enabled = false;
        }

        public new Size Size
        {
            get
            {
                return base.Size;
            }
            set
            {
                ScrollingPreview.Width = value.Width;
                base.Size = value;
            }
        }

        private void PopulateResponseKeyDrop()
        {
            ResponseKeyDrop.Items.Clear();
            Uri selectedKeyUri = CIAT.SaveFile.GetRelationshipsByType(BlockUri, typeof(CIATBlock), typeof(CIATKey)).Select(pr => pr.TargetUri).FirstOrDefault();
            foreach (CIATKey key in CIAT.SaveFile.GetAllIATKeyUris().Select(u => CIAT.SaveFile.GetIATKey(u)))
            {
                ResponseKeyDrop.Items.Add(key);
            }
            ResponseKeyDrop.SelectedItem = ResponseKeyDrop.Items.Cast<CIATKey>().Where(k => k.URI.Equals(selectedKeyUri)).FirstOrDefault();
        }

        public void SetActiveItem(CIATItem i)
        {
            CIATBlock b = CIAT.SaveFile.GetIATBlock(BlockUri);
            if (!b.Contains(i))
                throw new Exception("Attempt made to set active item in a block that does not contain that item.");
            ScrollingPreview.SelectedPreview = b.GetItemIndex(i);
        }

        private void CreateInstructionsEdit()
        {
            InstructionsEdit = new TextEditControl(InstructionsEditSize.Height, InstructionsEditSize.Width, DIText.UsedAs.IatBlockInstructions, true);
            InstructionsEdit.Location = new Point(InstructionsEditPosition.X, InstructionsEditPosition.Y);
            CIATBlock b = CIAT.SaveFile.GetIATBlock(BlockUri);
            InstructionsEdit.TextDisplayItemUri = b.InstructionsUri;
            Controls.Add(InstructionsEdit);
            InstructionsEdit.Size = new Size(InstructionsEdit.Width, InstructionsEdit.Height);
        }

        private void OKButton_Click(object sender, EventArgs e)
        {
            ((IATConfigMainForm)Parent).FormContents = IATConfigMainForm.EFormContents.Main;
        }

        private void ManageKeys_Click(object sender, EventArgs e)
        {
            ResponseKeyDialog dlg = new ResponseKeyDialog();
            dlg.ShowDialog(this);
            PopulateResponseKeyDrop();
            CIAT.SaveFile.GetIATBlock(BlockUri)[SelectedItemNdx].GetPreview(BlockUri).ScheduleInvalidation();
        }

        private void ResponseKeyDrop_SelectedIndexChanged(object sender, EventArgs e)
        {
            CIATKey oldKey = CIAT.SaveFile.GetIATBlock(BlockUri).Key;
            CIATKey newKey = ResponseKeyDrop.SelectedItem as CIATKey;
            List<CIATReversedKey> owningRKeys = new List<CIATReversedKey>();
            List<CIATDualKey> owningDKeys = new List<CIATDualKey>();
            if (oldKey != null)
            {
                if (oldKey.URI.Equals(newKey.URI))
                    return;
                if (oldKey.KeyType == IATKeyType.SimpleKey)
                {
                    owningRKeys.AddRange(CIAT.SaveFile.GetRelationshipsByType(oldKey.URI, typeof(CIATKey), typeof(CIATKey), "owned-by").Select(pr => CIAT.SaveFile.GetIATKey(pr.TargetUri)).Where(k => k.KeyType == IATKeyType.ReversedKey).Cast<CIATReversedKey>().ToList());
                    owningDKeys.AddRange(CIAT.SaveFile.GetRelationshipsByType(oldKey.URI, typeof(CIATKey), typeof(CIATKey), "owned-by").Select(pr => CIAT.SaveFile.GetIATKey(pr.TargetUri)).Where(k => k.KeyType == IATKeyType.DualKey).Cast<CIATDualKey>().ToList());
                    foreach (CIATReversedKey rk in owningRKeys)
                        rk.BaseKey = newKey;
                    foreach (CIATDualKey dk in owningDKeys)
                    {
                        if (dk.BaseKey1Uri.Equals(oldKey.URI))
                            dk.BaseKey1Uri = newKey.URI;
                        else if (dk.BaseKey2Uri.Equals(oldKey.URI))
                            dk.BaseKey2Uri = newKey.URI;
                    }
                }
                else if (oldKey.KeyType == IATKeyType.ReversedKey)
                {
                    owningDKeys.AddRange(CIAT.SaveFile.GetRelationshipsByType(oldKey.URI, typeof(CIATKey), typeof(CIATKey), "owned-by").Select(pr => CIAT.SaveFile.GetIATKey(pr.TargetUri)).Where(k => k.KeyType == IATKeyType.DualKey).Cast<CIATDualKey>().ToList());
                    foreach (CIATDualKey dk in owningDKeys)
                    {
                        if (dk.BaseKey1Uri.Equals(oldKey.URI))
                            dk.BaseKey1Uri = newKey.URI;
                        else if (dk.BaseKey2Uri.Equals(oldKey.URI))
                            dk.BaseKey2Uri = newKey.URI;
                    }
                }
            }
            CIATBlock b = CIAT.SaveFile.GetIATBlock(BlockUri);
            b.Key = newKey;
            b[SelectedItemNdx].GetPreview(BlockUri).ResumeLayout(true);
        }

        private void AddStimulus_Click(object sender, EventArgs e)
        {
            SuspendLayout();
            CIATBlock b = CIAT.SaveFile.GetIATBlock(BlockUri);
            b.AddItem(new CIATItem(), StimulusPanel.KeyedDir);
            if (IsDynamicallyKeyed())
                b[b.NumItems - 1].SetKeyedDirection(BlockUri, KeyedDirection.DynamicNone);
            ScrollingPreview.InsertPreview(b.NumItems - 1, b[b.NumItems - 1]);
            ScrollingPreview.SelectedPreview = b.NumItems - 1;
            ResumeLayout(false);
        }

        private void InsertStimulus_Click(object sender, EventArgs e)
        {
            SuspendLayout();
            CIATBlock b = CIAT.SaveFile.GetIATBlock(BlockUri);
            int currentNdx = ScrollingPreview.SelectedPreview;
            SelectedItemNdx = -1;
            b.InsertItem(currentNdx, new CIATItem());
            StimulusPanel.IATItem = b[currentNdx];
            if (IsDynamicallyKeyed())
                b[currentNdx].SetKeyedDirection(BlockUri, KeyedDirection.DynamicNone);
            ScrollingPreview.InsertPreview(currentNdx, b[currentNdx]);
            ScrollingPreview.SelectedPreview = currentNdx;
            SelectedItemNdx = currentNdx;
            ResumeLayout(false);
        }

        private void DeleteStimulus_Click(object sender, EventArgs e)
        {
            CIATBlock b = CIAT.SaveFile.GetIATBlock(BlockUri);
            SuspendLayout();
            if (b.NumItems == 1)
                b.AddItem(new CIATItem(), KeyedDirection.None);
            int currentNdx = ScrollingPreview.SelectedPreview;
            ScrollingPreview.DeletePreview(currentNdx);
            b[currentNdx].Dispose();
            if (currentNdx >= b.NumItems)
                ScrollingPreview.SelectedPreview = currentNdx - 1;
            else
                ScrollingPreview.SelectedPreview = currentNdx;
            ResumeLayout(false);
        }

        private void Done_Click(object sender, EventArgs e)
        {
            ((IATConfigMainForm)Parent).FormContents = IATConfigMainForm.EFormContents.Main;
        }


        public new bool Validate()
        {
            try
            {
                CIATBlock b = CIAT.SaveFile.GetIATBlock(BlockUri);
                b.Validate();
            }
            catch (KeyNotFoundException ex)
            {
                return true;
            }
            catch (CValidationException ex)
            {
                if (MessageBox.Show(this, ex.Message + "\n" + Properties.Resources.sErrorsExistProceed, Properties.Resources.sErrorsExistCaption,
                    MessageBoxButtons.YesNo) == DialogResult.Yes)
                    return true;
                else
                    return false;
            }
            return true;
        }

        private void DynamicallyKeyedCheck_CheckedChanged(object sender, EventArgs e)
        {
            CIATBlock b = CIAT.SaveFile.GetIATBlock(BlockUri);
            for (int ctr = 0; ctr < b.NumItems; ctr++)
            {
                if (DynamicallyKeyedCheck.Checked == true)
                    b[ctr].SetKeyedDirection(BlockUri, KeyedDirection.DynamicNone);
                else
                    b[ctr].SetKeyedDirection(BlockUri, KeyedDirection.None);
            }
            StimulusPanel.IATItem = ActiveItem;
        }

        public new void Dispose()
        {
            try
            {
                if (SelectedItemNdx != -1)
                {
                    CIAT.SaveFile.GetIATBlock(BlockUri)[SelectedItemNdx].SetPreviewPane(BlockUri, null);
                    CIAT.SaveFile.GetIATBlock(BlockUri)[SelectedItemNdx].GetPreview(BlockUri).SuspendLayout();
                }
                base.Dispose();
            }
            catch (KeyNotFoundException ex)
            { }
        }
    }
}

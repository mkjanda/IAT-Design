using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Linq;
using System.Windows.Forms;
using System.Threading.Tasks;
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
        
        public static Point IATInstructionsPanelPosition = new Point(530, 400);
        public static Size IATInstructionsPanelSize = new Size(375, 120);

        // the child item controls
        private TextEditControl InstructionsEdit;

        public Uri BlockUri { get; private set; }
        
        /// <summary>
        /// gets the currently active IAT item
        /// </summary>
        public CIATItem ActiveItem
        {
            get
            {
                return StimulusPanel.IATItem;
            }
        }

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
            InitializeComponent();
            SuspendLayout();
            if (CIAT.SaveFile.IAT.Is7Block)
                ResponseKeyDrop.Enabled = false;
            BlockUri = block.URI;
            PreviewPanel = new ImageDisplay();
            PreviewPanel.BackColor = Color.Black;
            StimulusPanel = new StimulusDefinitionPanel(BlockUri);
            StimulusPanel.AutoScaleMode = AutoScaleMode.Dpi;
            StimulusPanel.Size = new Size(StimulusPanelSize.Width, StimulusPanelSize.Height);
            StimulusPanel.Location = new Point(StimulusPanelPos.X, StimulusPanelPos.Y);
            StimulusPanel.BlockDynamicallyKeyed = new IsDynamicallyKeyedCallback(IsDynamicallyKeyed);
            Controls.Add(StimulusPanel);
            PreviewPanel.BackgroundImageLayout = ImageLayout.Stretch;
            PreviewPanel.Size = Images.ImageMediaType.FullPreview.ImageSize;
            PreviewPanel.Location = new Point(10, 12);
            PreviewGroup = new GroupBox();
            PreviewGroup.ClientSize = new Size(500, 500) + new Size(12, 18);
            PreviewGroup.Location = new Point(0, 0);
            PreviewGroup.Controls.Add(PreviewPanel);
            Controls.Add(PreviewGroup);
            ScrollingPreview = new ScrollingPreviewPanel();
            ScrollingPreview.Orientation = ScrollingPreviewPanel.EOrientation.horizontal;
            ScrollingPreview.Location = new Point(ScrollingPreviewPos.X, ScrollingPreviewPos.Y);
            ScrollingPreview.Size = new Size(this.ClientRectangle.Width, Images.ImageManager.ThumbnailSize.Height + 10);
            ScrollingPreview.AutoScaleMode = AutoScaleMode.Font;
            ScrollingPreview.PreviewSize = Images.ImageManager.ThumbnailSize;
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
            this.HandleCreated += (sender, args) => PopulateResponseKeyDrop();
            ResumeLayout(false);
            this.PerformLayout();
            this.PerformAutoScale();
            Controls.Remove(DynamicallyKeyedCheck);
            this.Enabled = false;

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
            InstructionsEdit = new TextEditControl(IATInstructionsPanelSize.Height, IATInstructionsPanelSize.Width, DIText.UsedAs.IatBlockInstructions, true);
            InstructionsEdit.AutoScaleMode = AutoScaleMode.Dpi;
            InstructionsEdit.Location = new Point(IATInstructionsPanelPosition.X, IATInstructionsPanelPosition.Y);
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

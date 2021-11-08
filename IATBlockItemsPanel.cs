using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;

namespace IATClient
{
    public partial class IATBlockItemsPanel : UserControl
    {
        /// <summary>
        /// Indicates that the control is being updated by code
        /// </summary>
        private bool IsUpdating;
        private StimulusPanel stimulusPanel;

        /// <summary>
        /// get the CIATBlock object being edited
        /// </summary>
        public CIATBlock Block
        {
            get
            {
                return ((IATBlockPanel)Parent).Block;
            }
        }

        public CIATItem ActiveItem
        {
            get
            {
                CIATItem item = new CIATItem();
                item.Stimulus = stimulusPanel.ActiveStimulus;
                item.KeyedDir = stimulusPanel.ActiveStimulusKeyedDir;
                return item;
            }
        }

        /// <summary>
        /// gets the currently active key
        /// </summary>
        public CIATKey ActiveKey
        {
            get
            {
                if (ResponseKeyDrop.SelectedIndex == -1)
                    return null;
                return CIATKey.KeyDictionary[ResponseKeyDrop.SelectedItem.ToString()];
            }
        }

        public IATItemPreviewPanel PreviewPanel
        {
            get
            {
                return ((IATBlockPanel)Parent).PreviewPanel;
            }
        }

        public IATBlockPanel ParentControl
        {
            get
            {
                return (IATBlockPanel)Parent;
            }
        }

        public IATConfigMainForm MainForm
        {
            get 
            {
                return (IATConfigMainForm)ParentControl.Parent;
            }
        }

        public bool ValidateInput()
        {
            for (int ctr = 0; ctr < Block.Items.Count; ctr++)
            {
                if (Block.Items[ctr].Stimulus == null)
                {
                    MainForm.ErrorMsg = String.Format(Properties.Resources.sNullStimulusInRowException, ctr + 1);
                    return false;
                }
                if (Block.Items[ctr].Stimulus.Type == CDisplayItem.EType.stimulusImage)
                {
                    if (((CStimulusImageItem)Block.Items[ctr].Stimulus).Description == String.Empty)
                    {
                        MainForm.ErrorMsg = String.Format(Properties.Resources.sNoImageAssignedToImageStimulusException, ctr + 1);
                        return false;
                    }
                }
                if (Block.Items[ctr].Stimulus.Type == CDisplayItem.EType.text)
                {
                    if (((CTextDisplayItem)Block.Items[ctr].Stimulus).Phrase == String.Empty)
                    {
                        MainForm.ErrorMsg = String.Format(Properties.Resources.sNoTextAssignedToTextStimulusException, ctr + 1);
                        return false;
                    }
                }
                if (Block.Items[ctr].KeyedDir == CIATItem.EKeyedDir.None)
                {
                    MainForm.ErrorMsg = String.Format(Properties.Resources.sNoKeyedDirAssignedToStimulusException, ctr + 1);
                    return false;
                }
            }
            MainForm.ErrorMsg = String.Empty;
            return true;
        }


        /// <summary>
        /// The default constructor
        /// </summary>
        public IATBlockItemsPanel()
        {
            InitializeComponent();
            this.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            KeyGroup.Anchor = AnchorStyles.Top | AnchorStyles.Right | AnchorStyles.Left;
            stimulusPanel = new StimulusPanel();
            stimulusPanel.OnKeyedDirectionChanged = new StimulusPanel.KeyedDirectionChangedHandler(KeyedDirectionChanged);
            stimulusPanel.OnStimulusChanged = new StimulusPanel.StimulusChangedHandler(StimulusChanged);
            stimulusPanel.DataFont = new Font("Times New Roman", 12);
            stimulusPanel.LabelFont = new Font("Arial", 10);
            stimulusPanel.InstructionsFont = new Font("Tahoma", 10);
            stimulusPanel.InstructionsFontColor = Color.SteelBlue;
            stimulusPanel.LabelFontColor = Color.DarkOliveGreen;
            stimulusPanel.DataFontColor = Color.DarkSlateBlue;
            stimulusPanel.ChildBackColor = Color.Azure;
            stimulusPanel.BackColor = Color.Azure;
            stimulusPanel.SubControlForeColor = Color.Navy;
            stimulusPanel.StimulusNameColor = Color.Firebrick;
            stimulusPanel.OnStimulusDeleted = new StimulusPanel.StimulusDeletedHandler(Stimulus_Deleted);
            IATBlockItemsGroup.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Right | AnchorStyles.Left;
            IsUpdating = false;
        }

        private void StimulusChanged(CDisplayItem Stimulus, int nItemNdx)
        {
            Block.Items[nItemNdx].Stimulus = Stimulus;
            ParentControl.ValidateInput();
        }

        private void KeyedDirectionChanged(CIATItem.EKeyedDir keyedDir, int nItemNdx)
        {
            Block.Items[nItemNdx].KeyedDir = keyedDir;
            ParentControl.ValidateInput();
        }

        /// <summary>
        /// updates the stimuli data grid view
        /// </summary>
        public void UpdateStimuliView()
        {
            IsUpdating = true;
            stimulusPanel.SuspendLayout();
            for (int ctr = 0; ctr < Block.Items.Count; ctr++)
                stimulusPanel.AddStimulusEdit(Block.Items[ctr].Stimulus, Block.Items[ctr].KeyedDir);
            IsUpdating = false;
            ParentControl.ValidateInput();
        }


        public CIATItem[] GetIATItems()
        {
            if (stimulusPanel == null)
                throw new Exception("Attempt to retrieve an array of IAT items from an IAT items panel with a null stimulus panel.");
            CIATItem[] items = new CIATItem[stimulusPanel.StimulusEdits.Count];
            for (int ctr = 0; ctr < stimulusPanel.StimulusEdits.Count; ctr++)
            {
                items[ctr] = new CIATItem();
                items[ctr].KeyedDir = stimulusPanel.StimulusEdits[ctr].KeyedDir;
                items[ctr].Stimulus = stimulusPanel.StimulusEdits[ctr].Stimulus;
            }
            return items;
        }

        private void AddStimulusButton_Click(object sender, EventArgs e)
        {
            CIATItem item = new CIATItem();
            Block.Items.Add(item);
            IsUpdating = true;
            stimulusPanel.AddStimulusEdit();
            IsUpdating = false;
            ((IATConfigMainForm)Parent.Parent).Modified = true;
            ParentControl.ValidateInput();
        }

        private void DeleteStimulusButton_Click(object sender, EventArgs e)
        {
            stimulusPanel.DeleteActiveStimulus();
        }

        private void Stimulus_Deleted(int ndx)
        {
            ((IATConfigMainForm)Parent.Parent).Modified = true;
            Block.Items.RemoveAt(ndx);
            ParentControl.ValidateInput();
        }
/*        
        private void ReorderButton_Click(object sender, EventArgs e)
        {
            if (stimulusPanel.StimulusEdits.Count < 2)
                return;

            ((IATBlockPanel)Parent).ShowReorderPanel();
        }
*/
        private void RandomizeButton_Click(object sender, EventArgs e)
        {
            IsUpdating = true;
            stimulusPanel.RandomizeOrder();

            IsUpdating = false;
        }

        private void IATBlockItemsPanel_ParentChanged(object sender, EventArgs e)
        {
            if (Parent != null)
            {
                stimulusPanel.PreviewPanel = ParentControl.PreviewPanel;
                SuspendLayout();
                IATBlockItemsGroup.Controls.Add(stimulusPanel);
                stimulusPanel.Dock = DockStyle.Fill;
                ResumeLayout(false);
            }
        }

        private void MangeKeysButton_Click(object sender, EventArgs e)
        {
            ((IATConfigMainForm)Parent.Parent).ShowResponseKeyPanel();
            PopulateResponseKeyDrop();
        }

        /// <summary>
        /// Populates the response key drop down list with all the response keys in CIATKey.KeyDictionary.  If a key
        /// is currently selected, the selection is preserved
        /// </summary>
        public void PopulateResponseKeyDrop()
        {
            String selKeyName;
            if (Block.Key != null)
                selKeyName = Block.Key.Name;
            else
                selKeyName = String.Empty;

            ResponseKeyDrop.Items.Clear();
            foreach (CIATKey key in CIATKey.KeyDictionary.Values)
                ResponseKeyDrop.Items.Add(key.Name);

            if (selKeyName != String.Empty)
                for (int ctr = 0; ctr < ResponseKeyDrop.Items.Count; ctr++)
                    if (ResponseKeyDrop.Items[ctr].ToString() == selKeyName)
                        ResponseKeyDrop.SelectedIndex = ctr;
        }

        private void ResponseKeyDrop_SelectedIndexChanged(object sender, EventArgs e)
        {
            PreviewPanel.InvalidateKey();
            PreviewPanel.InvalidateKeyedDirection();
            ((IATConfigMainForm)Parent.Parent).Modified = true;
            Block.Key = ActiveKey;
            ParentControl.ValidateInput();
        }

        public void ClearChildControls()
        {
            stimulusPanel.Clear();
        }
    }
}

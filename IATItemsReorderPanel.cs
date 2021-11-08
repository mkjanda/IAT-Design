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
    /// <summary>
    /// IATItemsReorderPanel provides a control that allows users to reorder the items in an IAT block
    /// </summary>
    public partial class IATItemsReorderPanel : UserControl
    {
        /// <summary>
        /// gets the parent IATBlockPanel
        /// </summary>
        private IATBlockPanel BlockPanel
        {
            get 
            {
                return (IATBlockPanel)Parent;
            }
        }

        /// <summary>
        /// gets the preview panel
        /// </summary>
        private IATItemPreviewPanel PreviewPanel
        {
            get
            {
                return (IATItemPreviewPanel)BlockPanel.PreviewPanel;
            }
        }

        /// <summary>
        /// gets the currently selected IAT item
        /// </summary>
        public CIATItem ActiveItem
        {
            get
            {
                if (ReorderView.SelectedItems.Count == 0)
                    return null;
                else return (CIATItem)ReorderView.SelectedItems[0].Tag;
            }
        }

        /// <summary>
        /// The default constructor
        /// </summary>
        public IATItemsReorderPanel()
        {
            InitializeComponent();
        }

        private void IATItemsReorderPanel_Load(object sender, EventArgs e)
        {
            // set the instruction text above the reorder view
            ReorderInstructionsBox.Text = Properties.Resources.sReorderInstructions;
        }

        private void IATItemsReorderPanel_ParentChanged(object sender, EventArgs e)
        {
            // if the parent is being changed to a non-null value, initialize the reorder view
            if (Parent != null)
            {
                InitializeReorderView();
            }
        }

        /// <summary>
        /// Initializes the Reorder View with the items in the IAT block
        /// </summary>
        private void InitializeReorderView()
        {
            ReorderView.Items.Clear();
            ListViewItem item;
            for (int ctr = 0; ctr < BlockPanel.Block.Items.Count; ctr++)
            {
                item = new ListViewItem(BlockPanel.Block.Items[ctr].Description);
                item.Tag = BlockPanel.Block.Items[ctr];
                ReorderView.Items.Add(item);
            }
            PreviewPanel.InvalidateStimulus();
        }

        private void ReorderView_ItemDrag(object sender, ItemDragEventArgs e)
        {
            ReorderView.DoDragDrop(e.Item, DragDropEffects.Move);
        }

        private void ReorderView_DragEnter(object sender, DragEventArgs e)
        {
            e.Effect = e.AllowedEffect;
        }

        private void ReorderView_DragOver(object sender, DragEventArgs e)
        {
            Point targetPt = ReorderView.PointToClient(new Point(e.X, e.Y));
            int targetNdx = ReorderView.InsertionMark.NearestIndex(targetPt);
            if (targetNdx != -1)
            {
                Rectangle itemRect = ReorderView.GetItemRect(targetNdx);
                if (targetPt.Y > itemRect.Top + (itemRect.Height >> 1))
                    ReorderView.InsertionMark.AppearsAfterItem = true;
                else
                    ReorderView.InsertionMark.AppearsAfterItem = false;
            }
            ReorderView.InsertionMark.Index = targetNdx;
        }

        private void ReorderView_DragLeave(object sender, EventArgs e)
        {
            ReorderView.InsertionMark.Index = -1;
        }

        private void ReorderView_DragDrop(object sender, DragEventArgs e)
        {
            int targetNdx = ReorderView.InsertionMark.Index;

            if (targetNdx == -1)
                return;

            if (ReorderView.InsertionMark.AppearsAfterItem == true)
                targetNdx++;

            ListViewItem item = (ListViewItem)e.Data.GetData(typeof(ListViewItem));
            ReorderView.Items.Insert(targetNdx, (ListViewItem)item.Clone());
            ReorderView.Items.Remove(item);
            ReorderView.SelectedIndices.Add(targetNdx);
        }

        private void ReorderView_SelectedIndexChanged(object sender, EventArgs e)
        {
            PreviewPanel.InvalidateStimulus();
        }

        private void ResetButton_Click(object sender, EventArgs e)
        {
            InitializeReorderView();
        }

        private void CancelButton_Click(object sender, EventArgs e)
        {
            BlockPanel.ShowItemsPanel();
        }

        private void OKButton_Click(object sender, EventArgs e)
        {
            BlockPanel.Block.Items.Clear();
            for (int ctr = 0; ctr < ReorderView.Items.Count; ctr++)
                BlockPanel.Block.Items.Add((CIATItem)ReorderView.Items[ctr].Tag);
            BlockPanel.ShowItemsPanel();
        }





    }
}

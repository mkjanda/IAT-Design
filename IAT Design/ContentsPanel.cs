using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;

using System.Text;
using System.Windows.Forms;

namespace IATClient
{
    partial class ContentsPanel : UserControl
    {
        private Point LastRMouseButtonDownPt;

        private ListViewItem _ClipboardItem;

        private IContentsItem DraggedItem = null;

        public ListViewItem ClipboardItem
        {
            get
            {
                return _ClipboardItem;
            }
            set
            {
                if (value == null)
                    ContextPaste.Enabled = false;
                else
                    ContextPaste.Enabled = true;
                _ClipboardItem = value;
            }
        }

        protected IATConfigMainForm MainForm
        {
            get
            {
                return (IATConfigMainForm)Parent.Parent;
            }
        }

        public ContentsPanel()
        {
            InitializeComponent();
            ContentsView.Activation = ItemActivation.Standard;
            ImageList Symbols = new ImageList();
//            ContentsView.Columns.Add("Test Contents", ContentsView.ClientSize.Width);
            Symbols.Images.Add(Properties.Resources.AlternateImage0);
            Symbols.Images.Add(Properties.Resources.AlternateImage1);
            Symbols.Images.Add(Properties.Resources.AlternateImage2);
            Symbols.Images.Add(Properties.Resources.AlternateImage3);
            Symbols.Images.Add(Properties.Resources.AlternateImage4);
            Symbols.Images.Add(Properties.Resources.AlternateImage5);
            Symbols.Images.Add(Properties.Resources.AlternateImage6);
            Symbols.Images.Add(Properties.Resources.AlternateImage7);
            Symbols.Images.Add(Properties.Resources.AlternateImage8);
            Symbols.Images.Add(Properties.Resources.AlternateImage9);
            Symbols.Images.Add(Properties.Resources.AlternateImage10);
            ContentsView.SmallImageList = Symbols;
            ClipboardItem = null;
            ContextRename.Enabled = false;
            ContextModify.Enabled = false;
            ContextCut.Enabled = false;
            ContextDelete.Enabled = false;
            LastRMouseButtonDownPt = new Point(-1, -1);
        }

        private void AddContentsItem(IContentsItem cItem, int nInsertNdx)
        {
            ListViewItem lvi = new ListViewItem();
            lvi.Text = cItem.Name;
            lvi.Tag = cItem;
            lvi.ImageIndex = 0;
            if (nInsertNdx == ContentsView.Items.Count)
                ContentsView.Items.Add(lvi);
            else
                ContentsView.Items.Insert(nInsertNdx, lvi);
        }

        private void AddIATBlock(String Name)
        {
            CIATBlock block = new CIATBlock(MainForm.IAT, false);
            IContentsItem item = block;
            item.Name = Name;
            item.AddToIAT(MainForm.IAT.Contents.Count - MainForm.IAT.AfterSurvey.Count);
            AddContentsItem(item, ContentsView.Items.Count - MainForm.IAT.AfterSurvey.Count);
            MainForm.Modified = true;
            MainForm.ConstructViewMenu(true);
            if (MainForm.IAT.NumNonDualKeyBlocks >= 2)
                GenerateIAT.Enabled = true;
        }

        private void AddPracticeIATBlock(String name)
        {
            CIATBlock block = new CIATBlock(MainForm.IAT, true);
            
            IContentsItem item = block;
            item.Name = name;
            if (MainForm.IAT.AfterSurvey != null)
            {
                item.AddToIAT(ContentsView.Items.Count - 1);
                AddContentsItem(item, ContentsView.Items.Count - 1);
            }
            else
            {
                item.AddToIAT(ContentsView.Items.Count);
                AddContentsItem(item, ContentsView.Items.Count);
            }
            MainForm.Modified = true;
            MainForm.ConstructViewMenu(true);
        }

        private void AddInstructionBlock(String name)
        {
            CInstructionBlock InstructionBlock = new CInstructionBlock(MainForm.IAT);
            IContentsItem item = InstructionBlock;
            item.Name = name;
            item.AddToIAT(MainForm.IAT.Contents.Count - MainForm.IAT.AfterSurvey.Count);
            AddContentsItem(item, ContentsView.Items.Count - MainForm.IAT.AfterSurvey.Count);
            MainForm.Modified = true;
            MainForm.ConstructViewMenu(true);
        }

        private void AddBlock_Click(object sender, EventArgs e)
        {
            AddIATBlock(String.Format("IAT Block #{0}", MainForm.IAT.Blocks.Count + 1));
        }

        private void AddInstructionScreen_Click(object sender, EventArgs e)
        {
            AddInstructionBlock(String.Format("Instruction Block #{0}", MainForm.IAT.InstructionBlocks.Count + 1));
        }

        private void ContentsView_ItemActivate(object sender, EventArgs e)
        {
            ListViewItem lvi = ContentsView.FocusedItem;
            IContentsItem item = (IContentsItem)lvi.Tag;
            MainForm.ActiveItem = item;
            switch (item.Type)
            {
                case ContentsItemType.AfterSurvey:
                    MainForm.FormContents = IATConfigMainForm.EFormContents.Survey;
                    break;

                case ContentsItemType.BeforeSurvey:
                    MainForm.FormContents = IATConfigMainForm.EFormContents.Survey;
                    break;

                case ContentsItemType.IATBlock:
                    MainForm.FormContents = IATConfigMainForm.EFormContents.IATBlock;
                    break;

                case ContentsItemType.IATPracticeBlock:
                    MainForm.FormContents = IATConfigMainForm.EFormContents.IATBlock;
                    break;

                case ContentsItemType.InstructionBlock:
                    MainForm.FormContents = IATConfigMainForm.EFormContents.Instructions;
                    break;
            }
        }

        private void AddPracticeBlock_Click(object sender, EventArgs e)
        {
            AddPracticeIATBlock(String.Format("IAT Practice Block #{0}", MainForm.IAT.PracticeBlocks.Count + 1));
        }

        private void AddBeforeSurvey_Click(object sender, EventArgs e)
        {
            ListViewItem lvi = new ListViewItem();
            lvi.ImageIndex = 0;
            CSurvey s = new CSurvey(MainForm.IAT, CSurvey.EOrdinality.Before);
            IContentsItem item = s;
            item.AddToIAT(MainForm.IAT.BeforeSurvey.Count);
            lvi.Tag = item;
            lvi.Text = item.Name;
            ContentsView.Items.Insert(MainForm.IAT.BeforeSurvey.Count - 1, lvi);
            MainForm.Modified = true;
            MainForm.ConstructViewMenu(true);
        }

        private void AddAfterSurvey_Click(object sender, EventArgs e)
        {
            ListViewItem lvi = new ListViewItem();
            lvi.ImageIndex = 0;
            CSurvey s = new CSurvey(MainForm.IAT, CSurvey.EOrdinality.After);
            IContentsItem item = s;
            item.AddToIAT(MainForm.IAT.Contents.Count);
            lvi.Tag = item;
            lvi.Text = item.Name;
            ContentsView.Items.Add(lvi);
            MainForm.Modified = true;
            MainForm.ConstructViewMenu(true);
        }

        private void ContentsView_AfterLabelEdit(object sender, LabelEditEventArgs e)
        {
            if (!e.CancelEdit)
            {
                ListViewItem lvi = ContentsView.Items[e.Item];
                if (e.Label == String.Empty)
                    e.CancelEdit = true;
                else
                {
                    IContentsItem item = (IContentsItem)lvi.Tag;
                    if ((item.Type == ContentsItemType.AfterSurvey) || (item.Type == ContentsItemType.BeforeSurvey))
                        ((CSurvey)item).Name = e.Label;
                    item.Name = e.Label; 
                    MainForm.ConstructViewMenu(true);
                }
            }
        }

        protected ListViewItem GetItemWithTag(object o)
        {
            foreach (ListViewItem i in ContentsView.Items)
                if (i.Tag == o)
                    return i;
            return null;
        }

        public void PopulateContents(List<IContentsItem> contents)
        {
            AddBeforeSurvey.Enabled = true;
            AddAfterSurvey.Enabled = true;
            ContentsView.Items.Clear();
            for (int ctr = 0; ctr < contents.Count; ctr++)
                AddContentsItem(contents[ctr], ContentsView.Items.Count);
            int AlternationImageNdx = 1;
            foreach (AlternationGroup g in AlternationGroup.Groups)
            {
                foreach (IContentsItem i in g.GroupMembers)
                {
                    ListViewItem lvi = GetItemWithTag(i);
                    lvi.ImageIndex = AlternationImageNdx;
                }
                AlternationImageNdx++;
            }
        }

        private void ContentsView_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete)
            {
                if (ContentsView.FocusedItem != null)
                    DeleteItemFromContents(ContentsView.FocusedItem);
            }
        }

        private void DeleteItemFromContents(ListViewItem lvi)
        {
            IContentsItem item = (IContentsItem)lvi.Tag;
            if (item.Type == ContentsItemType.AfterSurvey)
                AddAfterSurvey.Enabled = true;
            if (item.Type == ContentsItemType.BeforeSurvey)
                AddBeforeSurvey.Enabled = true;
            item.DeleteFromIAT();
            if (item.HasAlternateItem)
                item.AlternationGroup.Dispose();
            ContentsView.Items.Clear();
            PopulateContents(MainForm.IAT.Contents);
            MainForm.ConstructViewMenu(true);
            if (MainForm.IAT.NumNonDualKeyBlocks < 2)
                GenerateIAT.Enabled = false;
        }

        private void DeleteSelectedItem_Click(object sender, EventArgs e)
        {
            if (ContentsView.FocusedItem != null)
            {
                DeleteItemFromContents(ContentsView.FocusedItem);
            }
        }

        private void ContentsView_ItemDrag(object sender, ItemDragEventArgs e)
        {
            DraggedItem = (IContentsItem)((ListViewItem)e.Item).Tag;
            ContentsView.DoDragDrop(e.Item, DragDropEffects.Move);
        }

        private void ContentsView_DragEnter(object sender, DragEventArgs e)
        {
            e.Effect = e.AllowedEffect;
        }

        private void ContentsView_DragOver(object sender, DragEventArgs e)
        {
            Point targetPt = ContentsView.PointToClient(new Point(e.X, e.Y));
            int targetNdx = ContentsView.InsertionMark.NearestIndex(targetPt);
            if (targetNdx != -1)
            {
                Rectangle itemRect = ContentsView.GetItemRect(targetNdx);
                if (targetPt.Y > itemRect.Top + (itemRect.Height >> 1))
                    ContentsView.InsertionMark.AppearsAfterItem = true;
                else
                    ContentsView.InsertionMark.AppearsAfterItem = false;
                if (DraggedItem.Type == ContentsItemType.BeforeSurvey)
                {
                    if (targetNdx >= MainForm.IAT.BeforeSurvey.Count)
                    {
                        targetNdx = MainForm.IAT.BeforeSurvey.Count - 1;
                        ContentsView.InsertionMark.AppearsAfterItem = true;
                    }
                }
                else if (DraggedItem.Type == ContentsItemType.AfterSurvey)
                {
                    if (targetNdx < MainForm.IAT.Contents.Count - MainForm.IAT.AfterSurvey.Count - 1)
                    {
                        targetNdx = MainForm.IAT.Contents.Count - MainForm.IAT.AfterSurvey.Count - 1;
                        ContentsView.InsertionMark.AppearsAfterItem = true;
                    }
                }
            }
            ContentsView.InsertionMark.Index = targetNdx;
        }

        private void ContentsView_DragLeave(object sender, EventArgs e)
        {
            ContentsView.InsertionMark.Index = -1;
        }

        private void ContentsView_DragDrop(object sender, DragEventArgs e)
        {
            int targetNdx = ContentsView.InsertionMark.Index;

            if (targetNdx == -1)
                return;

            if (ContentsView.InsertionMark.AppearsAfterItem == true)
                targetNdx++;
            ListViewItem item = (ListViewItem)e.Data.GetData(typeof(ListViewItem));
            IContentsItem cItem = (IContentsItem)item.Tag;
            ListViewItem newLVI = (ListViewItem)item.Clone();
            ContentsView.Items.Insert(targetNdx, newLVI);
            cItem.DeleteFromIAT();
            ContentsView.Items.Remove(item);
            cItem.AddToIAT(targetNdx);
            MainForm.ConstructViewMenu(true);
            ContentsView.SelectedIndices.Add(targetNdx);
        }

        private void ContentsView_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Right)
                return;
            LastRMouseButtonDownPt = new Point(e.X, e.Y);
            ListViewHitTestInfo htInfo = ContentsView.HitTest(new Point(e.X, e.Y));
            ListViewItem lvi = htInfo.Item;
            if (lvi != null)
                ContentsView.FocusedItem = lvi;
        }

        private void ContextRename_Click(object sender, EventArgs e)
        {
            ContentsView.FocusedItem.BeginEdit();
        }

        private void ContextModify_Click(object sender, EventArgs e)
        {
            ListViewItem lvi = ContentsView.FocusedItem;
            IContentsItem item = (IContentsItem)lvi.Tag;
            MainForm.ActiveItem = item;
            switch (item.Type)
            {
                case ContentsItemType.AfterSurvey:
                    MainForm.FormContents = IATConfigMainForm.EFormContents.Survey;
                    break;

                case ContentsItemType.BeforeSurvey:
                    MainForm.FormContents = IATConfigMainForm.EFormContents.Survey;
                    break;

                case ContentsItemType.IATBlock:
                    MainForm.FormContents = IATConfigMainForm.EFormContents.IATBlock;
                    break;

                case ContentsItemType.IATPracticeBlock:
                    MainForm.FormContents = IATConfigMainForm.EFormContents.IATBlock;
                    break;

                case ContentsItemType.InstructionBlock:
                    MainForm.FormContents = IATConfigMainForm.EFormContents.Instructions;
                    break;
            }
        }

        private void ContextCut_Click(object sender, EventArgs e)
        {
            ListViewItem lvi = ContentsView.FocusedItem;
            ClipboardItem = lvi;
            DeleteItemFromContents(lvi);
            MainForm.Modified = true;
            MainForm.ConstructViewMenu(true);
        }

        private void ContextDelete_Click(object sender, EventArgs e)
        {
            DeleteItemFromContents(ContentsView.FocusedItem);
        }

        private void ContentsView_ItemSelectionChanged(object sender, ListViewItemSelectionChangedEventArgs e)
        {
            if (!e.IsSelected)
            {
                ContextRename.Enabled = false;
                ContextModify.Enabled = false;
                ContextCut.Enabled = false;
                ContextDelete.Enabled = false;
                DynamicallyKeyButton.Enabled = false; 
            }
            else
            {
                ContextRename.Enabled = true;
                ContextModify.Enabled = true;
                ContextCut.Enabled = true;
                ContextDelete.Enabled = true;
                if ((((IContentsItem)e.Item.Tag).Type == ContentsItemType.IATBlock) || (((IContentsItem)e.Item.Tag).Type == ContentsItemType.IATPracticeBlock))
                    DynamicallyKeyButton.Enabled = true;
            }
            
        }

        private void ContextPaste_Click(object sender, EventArgs e)
        {
            Point PasteLocation = LastRMouseButtonDownPt;
            ListViewHitTestInfo htInfo = ContentsView.HitTest(PasteLocation);
            int insertNdx;
            if (htInfo.Item != null)
            {
                IContentsItem cItem = (IContentsItem)htInfo.Item.Tag;
                if (cItem.Type == ContentsItemType.BeforeSurvey)
                    insertNdx = 1;
                else
                    insertNdx = ContentsView.Items.IndexOf(htInfo.Item);
            }
            else
            {
                if (AddAfterSurvey.Enabled == false)
                    insertNdx = ContentsView.Items.Count - 1;
                else
                    insertNdx = ContentsView.Items.Count;
            }
            IContentsItem ContentsItem = (IContentsItem)ClipboardItem.Tag;
            ContentsItem.AddToIAT(insertNdx);
            ContentsView.Items.Insert(insertNdx, ClipboardItem);
            ClipboardItem = null;
            MainForm.Modified = true;
            MainForm.ConstructViewMenu(true);
            if (MainForm.IAT.NumNonDualKeyBlocks >= 2)
                GenerateIAT.Enabled = true;
        }

        private void GenerateIAT_Click(object sender, EventArgs e)
        {
            CItemValidator.StartValidation();
            CItemValidator.StartValidation();
            foreach (CIATBlock b in MainForm.IAT.Blocks)
                CItemValidator.ValidateItem(b);
            if (CItemValidator.HasErrors)
            {
                CItemValidator.DisplayErrors(MainForm);
                return;
            }

            IATGenerateBlockSelect dlg = new IATGenerateBlockSelect();
            dlg.IAT = MainForm.IAT;
            if (dlg.ShowDialog(this) == DialogResult.OK)
            {
                C7BlockIATGenerator generator = new C7BlockIATGenerator(MainForm.IAT, dlg.FirstBlock, dlg.SecondBlock, MainForm);
                generator.Generate(dlg.RandomizeGeneratedBlocks, dlg.EnableAlternation);
                ContentsView.Items.Clear();
                PopulateContents(MainForm.IAT.Contents);
                MainForm.ConstructViewMenu(true);
            }
        }

        private void AlternateBlockButton_Click(object sender, EventArgs e)
        {
            SpecifyAlterateBlocksDlg dlg = new SpecifyAlterateBlocksDlg();
            dlg.theIAT = MainForm.IAT;
            dlg.ShowDialog(this);
            ContentsView.Clear();
            PopulateContents(MainForm.IAT.Contents);
        }

        private void DynamiclyKeyButton_Click(object sender, EventArgs e)
        {
            MainForm.ActiveItem = (IContentsItem)ContentsView.SelectedItems[0].Tag;
            MainForm.FormContents = IATConfigMainForm.EFormContents.DynamicallyKey;
        }

        private void ContentsPanel_ParentChanged(object sender, EventArgs e)
        {
            DynamicallyKeyButton.Enabled = false;
        }
    }
}

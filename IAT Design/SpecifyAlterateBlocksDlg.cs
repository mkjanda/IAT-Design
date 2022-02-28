using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace IATClient
{
    partial class SpecifyAlterateBlocksDlg : Form
    {
        public CIAT theIAT;
        private bool bIsUpdatingInternally = false;
        private int nInstructionsChecked, nIATBlocksChecked, nPracticeBlocksChecked, nSurveysChecked;
        private ListViewItem ChangingItem;

        public SpecifyAlterateBlocksDlg()
        {
            InitializeComponent();
            theIAT = null;
        }

        private void SpecifyAlterateBlocksDlg_Load(object sender, EventArgs e)
        {
            if (theIAT == null)
                throw new Exception("Cannot load Alternate Block Specification Dialog with a null IAT reference.");
            ListViewItem lvi;

            nInstructionsChecked = nIATBlocksChecked = nPracticeBlocksChecked = 0;

            for (int ctr = 0; ctr < theIAT.Contents.Count; ctr++)
            {
                IContentsItem cItem = theIAT.Contents[ctr];
                if (cItem.Type == ContentsItemType.InstructionBlock)
                {
                    lvi = new ListViewItem();
                    if (cItem.HasAlternateItem)
                        lvi.ForeColor = System.Drawing.Color.Gray;
                    else
                        lvi.ForeColor = System.Drawing.Color.Black;
                    lvi.Text = cItem.Name;
                    lvi.Tag = cItem;
                    lvi.Checked = false;
                    InstructionsList.Items.Add(lvi);
                }
                else if (cItem.Type == ContentsItemType.IATBlock)
                {
                    lvi = new ListViewItem();
                    if (cItem.HasAlternateItem)
                        lvi.ForeColor = System.Drawing.Color.Gray;
                    else
                        lvi.ForeColor = System.Drawing.Color.Black;
                    lvi.Text = cItem.Name;
                    lvi.Tag = cItem;
                    lvi.Checked = false;
                    BlockList.Items.Add(lvi);
                }
                else if ((cItem.Type == ContentsItemType.BeforeSurvey) || (cItem.Type == ContentsItemType.AfterSurvey))
                {
                    lvi = new ListViewItem();
                    if (cItem.HasAlternateItem)
                        lvi.ForeColor = System.Drawing.Color.Gray;
                    else
                        lvi.ForeColor = System.Drawing.Color.Black;
                    lvi.Text = cItem.Name;
                    lvi.Tag = cItem;
                    lvi.Checked = false;
                    SurveyList.Items.Add(lvi);
                }
            }
            PrependedRadio.Enabled = false;
            PostpendedRadio.Enabled = false;
        }

        private void OK_Click(object sender, EventArgs e)
        {
            Close();
        }

        private ListViewItem FindItemWithTag(object item, ListView view)
        {
            for (int ctr = 0; ctr < view.Items.Count; ctr++)
                if (view.Items[ctr].Tag == item)
                    return view.Items[ctr];
            return null;
        }

        private void ClearAllChecksBut(ListViewItem item, ListView view)
        {
            for (int ctr = 0; ctr < view.Items.Count; ctr++)
                if (view.Items[ctr] != item)
                {
                    if (view.Items[ctr].Checked)
                        view.Items[ctr].Checked = false;
                }
        }

        private void InstructionsList_ItemChecked(object sender, ItemCheckedEventArgs e)
        {/*
            if (e.Item.Checked == true)
            {
                if (((IHasAlternateItem)e.Item.Tag).HasAlternateItem)
                {
                    ListViewItem lvi = FindItemWithTag(((IHasAlternateItem)e.Item.Tag).Alternate, InstructionsList);
                    if (!lvi.Checked)
                    {
                        ClearAllChecksBut(e.Item, InstructionsList);
                        lvi.Checked = true;
                        AlternateButton.Enabled = false;
                        UndoAlternationButton.Enabled = true;
                        nInstructionsChecked = -1;
                        return;
                    }
                }
                if (nInstructionsChecked == -1)
                {
                    ClearAllChecksBut(e.Item, InstructionsList);
                    nInstructionsChecked = 1;
                }
                else
                    nInstructionsChecked++;
            }
            else
            {
                if (((IHasAlternateItem)e.Item.Tag).HasAlternateItem)
                {
                    ListViewItem lvi = FindItemWithTag(((IHasAlternateItem)e.Item.Tag).Alternate, InstructionsList);
                    if (lvi.Checked)
                        lvi.Checked = false;
                    nInstructionsChecked = 0;
                    UndoAlternationButton.Enabled = false;
                    return;
                }
                nInstructionsChecked--;
            }
            if (nInstructionsChecked == 2)
                AlternateButton.Enabled = true;
            else
                AlternateButton.Enabled = false;
          */
        }

        private void BlockList_ItemChecked(object sender, ItemCheckedEventArgs e)
        {
            /*
            if (e.Item.Checked == true)
            {
                if (((IHasAlternateItem)e.Item.Tag).HasAlternateItem)
                {
                    ListViewItem lvi = FindItemWithTag(((IHasAlternateItem)e.Item.Tag).Alternate, BlockList);
                    if (!lvi.Checked)
                    {
                        ClearAllChecksBut(e.Item, BlockList);
                        lvi.Checked = true;
                        AlternateButton.Enabled = false;
                        UndoAlternationButton.Enabled = true;
                        nIATBlocksChecked = -1;
                        return;
                    }
                }
                if (nIATBlocksChecked == -1)
                {
                    ClearAllChecksBut(e.Item, BlockList);
                    nInstructionsChecked = 1;
                }
                else
                    nIATBlocksChecked++;
            }
            else
            {
                if (((IHasAlternateItem)e.Item.Tag).HasAlternateItem)
                {
                    ListViewItem lvi = FindItemWithTag(((IHasAlternateItem)e.Item.Tag).Alternate, BlockList);
                    if (lvi.Checked)
                        lvi.Checked = false;
                    nIATBlocksChecked = 0;
                    UndoAlternationButton.Enabled = false;
                    return;
                }
                nIATBlocksChecked--;
            }
            if (nIATBlocksChecked == 2)
                AlternateButton.Enabled = true;
            else
                AlternateButton.Enabled = false;
            */
        }

        private void PracticeBlocksList_ItemChecked(object sender, ItemCheckedEventArgs e)
        {
            /*
            if (e.Item.Checked == true)
            {
                if (((IHasAlternateItem)e.Item.Tag).HasAlternateItem)
                {
                    ListViewItem lvi = FindItemWithTag(((IHasAlternateItem)e.Item.Tag).Alternate, PracticeBlocksList);
                    if (!lvi.Checked)
                    {
                        ClearAllChecksBut(e.Item, PracticeBlocksList);
                        lvi.Checked = true;
                        nPracticeBlocksChecked = -1;
                        AlternateButton.Enabled = false;
                        UndoAlternationButton.Enabled = true;
                        return;
                    }
                }
                if (nPracticeBlocksChecked == -1)
                {
                    ClearAllChecksBut(e.Item, PracticeBlocksList);
                    nInstructionsChecked = 1;
                }
                else
                    nPracticeBlocksChecked++;
            }
            else
            {
                if (((IHasAlternateItem)e.Item.Tag).HasAlternateItem)
                {
                    ListViewItem lvi = FindItemWithTag(((IHasAlternateItem)e.Item.Tag).Alternate, PracticeBlocksList);
                    if (!lvi.Checked)
                    {
                        ClearAllChecksBut(e.Item, PracticeBlocksList);
                        lvi.Checked = true;
                        nPracticeBlocksChecked = 0;
                        UndoAlternationButton.Enabled = false;
                        return;
                    }
                }
                nPracticeBlocksChecked--;
            }
            if (nPracticeBlocksChecked == 2)
                AlternateButton.Enabled = true;
            else
                AlternateButton.Enabled = false;
             */
        }

        private void AlternateButton_Click(object sender, EventArgs e)
        {
            List<IContentsItem> alternatingItems = new List<IContentsItem>();
            if (BlockTypeTab.SelectedTab == IATBlockTab)
            {
                for (int ctr = 0; ctr < BlockList.Items.Count; ctr++)
                    if (BlockList.Items[ctr].Checked)
                    {
                        alternatingItems.Add((IContentsItem)BlockList.Items[ctr].Tag);
                        BlockList.Items[ctr].ForeColor = System.Drawing.Color.Gray;
                        BlockList.Items[ctr].Checked = false;
                    }
                new AlternationGroup(alternatingItems.ToArray());
            }
            if (BlockTypeTab.SelectedTab == InstructionsTab)
            {
                for (int ctr = 0; ctr < InstructionsList.Items.Count; ctr++)
                    if (InstructionsList.Items[ctr].Checked)
                    {
                        alternatingItems.Add((IContentsItem)InstructionsList.Items[ctr].Tag);
                        InstructionsList.Items[ctr].ForeColor = System.Drawing.Color.Gray;
                        InstructionsList.Items[ctr].Checked = false;
                    }
                new AlternationGroup(alternatingItems.ToArray());
            }
            if (BlockTypeTab.SelectedTab == PracticeBlocksTab)
            {
                for (int ctr = 0; ctr < PracticeBlocksList.Items.Count; ctr++)
                    if (PracticeBlocksList.Items[ctr].Checked)
                    {
                        alternatingItems.Add((IContentsItem)PracticeBlocksList.Items[ctr].Tag);
                        PracticeBlocksList.Items[ctr].ForeColor = System.Drawing.Color.Gray;
                        PracticeBlocksList.Items[ctr].Checked = false;
                    }
                new AlternationGroup(alternatingItems.ToArray());
            }
            if (BlockTypeTab.SelectedTab == SurveyTab)
            {
                for (int ctr = 0; ctr < SurveyList.Items.Count; ctr++)
                    if (SurveyList.Items[ctr].Checked)
                    {
                        alternatingItems.Add((IContentsItem)SurveyList.Items[ctr].Tag);
                        SurveyList.Items[ctr].Checked = false;
                        SurveyList.Items[ctr].ForeColor = System.Drawing.Color.Gray;
                    }
                new AlternationGroup(alternatingItems.ToArray());
            }
        }

        private void BlockTypeTab_Selected(object sender, TabControlEventArgs e)
        {
            if (e.TabPage == IATBlockTab)
            {
                if (nIATBlocksChecked == 2)
                {
                    AlternateButton.Enabled = true;
                    UndoAlternationButton.Enabled = false;
                }
                else if (nIATBlocksChecked == -1)
                {
                    AlternateButton.Enabled = false;
                    UndoAlternationButton.Enabled = true;
                }
                else
                {
                    AlternateButton.Enabled = false;
                    UndoAlternationButton.Enabled = false;
                }
            }
            else if (e.TabPage == PracticeBlocksTab)
            {
                if (nPracticeBlocksChecked == 2)
                {
                    AlternateButton.Enabled = true;
                    UndoAlternationButton.Enabled = false;
                }
                else if (nPracticeBlocksChecked == -1)
                {
                    AlternateButton.Enabled = false;
                    UndoAlternationButton.Enabled = true;
                }
                else
                {
                    AlternateButton.Enabled = false;
                    UndoAlternationButton.Enabled = false;
                }
            }
            else
            {
                if (nInstructionsChecked == 2)
                {
                    AlternateButton.Enabled = true;
                    UndoAlternationButton.Enabled = false;
                }
                else if (nInstructionsChecked == -1)
                {
                    AlternateButton.Enabled = false;
                    UndoAlternationButton.Enabled = true;
                }
                else
                {
                    AlternateButton.Enabled = false;
                    UndoAlternationButton.Enabled = false;
                }
            }
        }

        private void InstructionsList_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            if (bIsUpdatingInternally)
                return;
            bIsUpdatingInternally = true;
            ListViewItem item = InstructionsList.Items[e.Index];
            if (ChangingItem == null)
                ChangingItem = item;
            if (e.NewValue == CheckState.Checked)
            {
                if (((IContentsItem)item.Tag).HasAlternateItem)
                {
                    if (item == ChangingItem)
                    {
                        nInstructionsChecked = 1;
                        ClearAllChecksBut(item, InstructionsList);
                        AlternateButton.Enabled = false;
                        UndoAlternationButton.Enabled = true;
                        foreach (IContentsItem i in ((IContentsItem)item.Tag).AlternationGroup.GroupMembers)
                        {
                            if (i != (IContentsItem)ChangingItem.Tag)
                            {
                                ListViewItem lvi = FindItemWithTag(i, InstructionsList);
                                lvi.Checked = true;
                            }
                        }
                    }
                    else
                        nInstructionsChecked++;
                }
                else
                {
                    nInstructionsChecked++;
                    if (nInstructionsChecked == 2)
                        AlternateButton.Enabled = true;
                    else
                        AlternateButton.Enabled = false;
                }
            }
            else
            {
                if (((IContentsItem)item.Tag).HasAlternateItem)
                {
                    nInstructionsChecked = 0;
                    AlternateButton.Enabled = false;
                    UndoAlternationButton.Enabled = false;

                    List<IContentsItem> aItems = ((IContentsItem)item.Tag).AlternationGroup.GroupMembers;
                    foreach (IContentsItem i in aItems)
                    {
                        if (i != (IContentsItem)ChangingItem.Tag)
                        {
                            ListViewItem lvi = FindItemWithTag(i, InstructionsList);
                            lvi.Checked = false;
                            lvi.ForeColor = System.Drawing.Color.Black;
                        }
                    }
                }
                else
                {
                    nInstructionsChecked--;
                    if (nInstructionsChecked != 2)
                        AlternateButton.Enabled = false;
                    else
                        AlternateButton.Enabled = true;
                }
            }
            if (item == ChangingItem)
                ChangingItem = null;
            bIsUpdatingInternally = false;
        }

        private void BlockList_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            if (bIsUpdatingInternally)
                return;
            bIsUpdatingInternally = true;
            ListViewItem item = BlockList.Items[e.Index];
            if (ChangingItem == null)
                ChangingItem = item;
            if (e.NewValue == CheckState.Checked)
            {
                if (((IContentsItem)item.Tag).HasAlternateItem)
                {
                    if (item == ChangingItem)
                    {
                        nIATBlocksChecked = 1;
                        ClearAllChecksBut(item, BlockList);
                        AlternateButton.Enabled = false;
                        UndoAlternationButton.Enabled = true;
                        foreach (IContentsItem i in ((IContentsItem)item.Tag).AlternationGroup.GroupMembers)
                        {
                            if (i != (IContentsItem)ChangingItem.Tag)
                            {
                                ListViewItem lvi = FindItemWithTag(i, BlockList);
                                lvi.Checked = true;
                            }
                        }
                    }
                    else
                        nIATBlocksChecked++;
                }
                else
                {
                    nIATBlocksChecked++;
                    if (nIATBlocksChecked == 2)
                        AlternateButton.Enabled = true;
                    else
                        AlternateButton.Enabled = false;
                }
            }
            else
            {
                if (((IContentsItem)item.Tag).HasAlternateItem)
                {
                    nIATBlocksChecked = 0;
                    AlternateButton.Enabled = false;
                    UndoAlternationButton.Enabled = false;

                    List<IContentsItem> aItems = ((IContentsItem)item.Tag).AlternationGroup.GroupMembers;
                    foreach (IContentsItem i in aItems)
                    {
                        if (i != (IContentsItem)ChangingItem.Tag)
                        {
                            ListViewItem lvi = FindItemWithTag(i, BlockList);
                            lvi.ForeColor = System.Drawing.Color.Black;
                            lvi.Checked = false;
                        }
                    }
                }
                else
                {
                    nIATBlocksChecked--;
                    if (nIATBlocksChecked != 2)
                        AlternateButton.Enabled = false;
                    else
                        AlternateButton.Enabled = true;
                }
            }
            if (ChangingItem == item)
                ChangingItem = null;
            bIsUpdatingInternally = false;
        }

        private void PracticeBlocksList_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            if (bIsUpdatingInternally)
                return;
            bIsUpdatingInternally = false;
            ListViewItem item = PracticeBlocksList.Items[e.Index];
            if (ChangingItem == null)
                ChangingItem = item;
            if (e.NewValue == CheckState.Checked)
            {
                if (((IContentsItem)item.Tag).HasAlternateItem)
                {
                    if (item == ChangingItem)
                    {
                        nPracticeBlocksChecked = 1;
                        ClearAllChecksBut(item, PracticeBlocksList);
                        AlternateButton.Enabled = false;
                        UndoAlternationButton.Enabled = true;
                        foreach (IContentsItem i in ((IContentsItem)item.Tag).AlternationGroup.GroupMembers)
                        {
                            if (i != (IContentsItem)ChangingItem.Tag)
                            {
                                ListViewItem lvi = FindItemWithTag(i, SurveyList);
                                lvi.Checked = true;
                            }
                        }
                    }
                    else
                        nPracticeBlocksChecked++;
                }
                else
                {
                    nPracticeBlocksChecked++;
                    if (nPracticeBlocksChecked == 2)
                        AlternateButton.Enabled = true;
                    else
                        AlternateButton.Enabled = false;
                }
            }
            else
            {
                if (((IContentsItem)item.Tag).HasAlternateItem)
                {
                    nPracticeBlocksChecked = 0;
                    AlternateButton.Enabled = false;
                    UndoAlternationButton.Enabled = false;

                    List<IContentsItem> aItems = ((IContentsItem)item.Tag).AlternationGroup.GroupMembers;
                    foreach (IContentsItem i in aItems)
                    {
                        if (i != (IContentsItem)ChangingItem.Tag)
                        {
                            ListViewItem lvi = FindItemWithTag(i, SurveyList);
                            lvi.ForeColor = System.Drawing.Color.Black;
                            lvi.Checked = false;
                        }
                    }
                }
                else
                {
                    nPracticeBlocksChecked--;
                    if (nPracticeBlocksChecked != 2)
                        AlternateButton.Enabled = false;
                    else
                        AlternateButton.Enabled = true;
                }
            }
            if (ChangingItem == item)
                ChangingItem = null;
            bIsUpdatingInternally = false;
        }

        private void UndoAlternationButton_Click(object sender, EventArgs e)
        {
            AlternationGroup group = null;
            if (BlockTypeTab.SelectedTab == IATBlockTab)
            {
                for (int ctr = 0; ctr < BlockList.Items.Count; ctr++)
                {
                    if (BlockList.Items[ctr].Checked)
                    {
                        if (group == null)
                            group = ((IContentsItem)BlockList.Items[ctr].Tag).AlternationGroup;
                        BlockList.Items[ctr].ForeColor = System.Drawing.Color.Black;
                        BlockList.Items[ctr].Checked = false;
                    }
                }
                group.Dispose();
            }
            if (BlockTypeTab.SelectedTab == InstructionsTab)
            {
                for (int ctr = 0; ctr < InstructionsList.Items.Count; ctr++)
                {
                    if (InstructionsList.Items[ctr].Checked)
                    {
                        if (group == null)
                            group = ((IContentsItem)InstructionsList.Items[ctr].Tag).AlternationGroup;
                        InstructionsList.Items[ctr].ForeColor = System.Drawing.Color.Black;
                        InstructionsList.Items[ctr].Checked = false;
                    }
                }
                group.Dispose();
            }
            if (BlockTypeTab.SelectedTab == PracticeBlocksTab)
            {
                for (int ctr = 0; ctr < PracticeBlocksList.Items.Count; ctr++)
                {
                    if (PracticeBlocksList.Items[ctr].Checked)
                    {
                        if (group == null)
                            group = ((IContentsItem)PracticeBlocksList.Items[ctr].Tag).AlternationGroup;
                        PracticeBlocksList.Items[ctr].ForeColor = System.Drawing.Color.Black;
                        PracticeBlocksList.Items[ctr].Checked = false;
                    }
                }
                group.Dispose();
            }
            if (BlockTypeTab.SelectedTab == SurveyTab)
            {
                ListViewItem lvi = null;
                int ctr = -1;
                while (lvi == null)
                    if (SurveyList.Items[++ctr].Checked)
                        lvi = SurveyList.Items[ctr];
                group = ((IContentsItem)lvi.Tag).AlternationGroup;
                foreach (IContentsItem ci in group.GroupMembers)
                {
                    for (ctr = 0; ctr < SurveyList.Items.Count; ctr++)
                        if (((IContentsItem)SurveyList.Items[ctr].Tag) == ci)
                        {
                            SurveyList.Items[ctr].Checked = false;
                            SurveyList.Items[ctr].ForeColor = System.Drawing.Color.Black;
                        }
                }
                group.Dispose();
            }
            UndoAlternationButton.Enabled = false;
        }

        private void SurveyList_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            if (bIsUpdatingInternally)
                return;
            bIsUpdatingInternally = false;
            ListViewItem item = SurveyList.Items[e.Index];
            if (ChangingItem == null)
                ChangingItem = item;
            if (e.NewValue == CheckState.Checked)
            {
                if (((IContentsItem)item.Tag).AlternationGroup != null)
                {
                    if (item == ChangingItem)
                    {
                        nSurveysChecked = 1;
                        ClearAllChecksBut(item, SurveyList);
                        AlternateButton.Enabled = false;
                        UndoAlternationButton.Enabled = true;

                        List<IContentsItem> aItems = ((IContentsItem)item.Tag).AlternationGroup.GroupMembers;
                        foreach (IContentsItem i in aItems)
                        {
                            if (i != (IContentsItem)ChangingItem.Tag)
                            {
                                ListViewItem lvi = FindItemWithTag(i, SurveyList);
                                lvi.Checked = true;
                            }
                        }
                    }
                    else
                        nSurveysChecked++;
                }
                else
                {
                    nSurveysChecked++;
                    if (nSurveysChecked > 1)
                        AlternateButton.Enabled = true;
                    else if ((nSurveysChecked == 1) && (PrependedRadio.Checked || PostpendedRadio.Checked))
                        AlternateButton.Enabled = true;
                }
            }
            else
            {
                if (((IContentsItem)item.Tag).AlternationGroup != null)
                {
                    if (item == ChangingItem)
                    {
                        nSurveysChecked = 0;
                        ClearAllChecksBut(item, SurveyList);
                        AlternateButton.Enabled = false;
                        UndoAlternationButton.Enabled = false;

                        List<IContentsItem> aItems = ((IContentsItem)item.Tag).AlternationGroup.GroupMembers;
                        foreach (IContentsItem i in aItems)
                        {
                            if (i != (IContentsItem)ChangingItem.Tag)
                            {
                                ListViewItem lvi = FindItemWithTag(i, SurveyList);
                                lvi.Checked = false;
                            }
                        }
                    }
                }
                else
                {
                    nSurveysChecked--;
                    if (nSurveysChecked < 2)
                        AlternateButton.Enabled = false;
                }
            }
            if (nSurveysChecked == 1)
            {
                PrependedRadio.Enabled = true;
                PostpendedRadio.Enabled = true;
            }
            else
            {
                PrependedRadio.Enabled = false;
                PrependedRadio.Checked = false;
                PostpendedRadio.Enabled = false;
                PostpendedRadio.Checked = false;
            }
            if (ChangingItem == item)
                ChangingItem = null;
            bIsUpdatingInternally = false;
        }

        private void SurveyList_ItemChecked(object sender, ItemCheckedEventArgs e)
        {

        }
        /*
        private void PrependedRadio_CheckedChanged(object sender, EventArgs e)
        {
            if (PrependedRadio.Checked == true)
            {
                AlternationGroup.PrefixSelfAlternatingSurveys = true;
                if (nSurveysChecked == 1)
                    AlternateButton.Enabled = true;
            }
        }

        private void PostpendedRadio_CheckedChanged(object sender, EventArgs e)
        {
            if (PostpendedRadio.Checked == true)
            {
                AlternationGroup.PrefixSelfAlternatingSurveys = false;
                if (nSurveysChecked == 1)
                    AlternateButton.Enabled = true;
            }
        }
        */
    }
}

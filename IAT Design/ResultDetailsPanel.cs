using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;

namespace IATClient
{
    class ResultDetailsPanel : Panel
    {
        private ToolStrip Header = new ToolStrip();
        private Padding ToolStripItemPadding = new Padding(5, 3, 5, 2);
        private ToolStripButton CloseButton, SplitButton, NextButton, PreviousButton;
        private ToolStripComboBox TestItemCombo, ResultSetCombo;
        private int _CurrResultSet;
        private CResultData ResultData;
        private CItemSlideContainer ItemSlideContainer;
        private IATSurveyFile.Survey []BeforeSurveys;
        private IATSurveyFile.Survey []AfterSurveys;
        private int _ResultPartNdx = -1;
        private Panel ContainerPanel = null, PreviewPanel = null;
        private Action<Panel> OnClose;
        private Action OnSplit;

        public int CurrResultSet
        {
            get
            {
                return _CurrResultSet;
            }
            set
            {
                _CurrResultSet = value;
                if (ResultPartNdx < BeforeSurveys.Length)
                    BeforeSurveys[ResultPartNdx].DisplayValues(ResultData.IATResults[value].BeforeSurveys[ResultPartNdx]);
                else if (ResultPartNdx > BeforeSurveys.Length)
                    AfterSurveys[ResultPartNdx - BeforeSurveys.Length - 1].DisplayValues(ResultData.IATResults[value].AfterSurveys[ResultPartNdx - BeforeSurveys.Length - 1]);
                else
                    ((ItemSlidePanel)PreviewPanel).ResultSet = value;
            }
        }

        public int ResultPartNdx
        {
            get
            {
                return _ResultPartNdx;
            }
            set
            {
                if (value == _ResultPartNdx)
                    return;
                _ResultPartNdx = value;
                GeneratePreview();
                CurrResultSet = _CurrResultSet;
            }
        }

        public ResultDetailsPanel(int nWidth, CResultData resultData, CItemSlideContainer itemSlideContainer, int nResultPart, Action<Panel> onClose, Action onSplit)
        {
            this.Width = nWidth;
            OnClose = onClose;
            OnSplit = onSplit;
            this.BackColor = Color.White;
            _CurrResultSet = -1;
            ResultData = resultData;
            ItemSlideContainer = itemSlideContainer;
            _ResultPartNdx = nResultPart;
            Header.Dock = DockStyle.Top;
            Header.Height = 40;
            CloseButton = new ToolStripButton();
            CloseButton.Text = "Close";
            CloseButton.Size = TextRenderer.MeasureText(CloseButton.Text, CloseButton.Font) + new Size(ToolStripItemPadding.Horizontal, ToolStripItemPadding.Vertical);
            CloseButton.Click += new EventHandler(CloseButton_Click);
            Header.Items.Add(CloseButton);
            SplitButton = new ToolStripButton();
            SplitButton.Text = "Split";
            SplitButton.Size = TextRenderer.MeasureText(SplitButton.Text, SplitButton.Font) + new Size(ToolStripItemPadding.Horizontal, ToolStripItemPadding.Vertical);
            SplitButton.Click += new EventHandler(SplitButton_Click);
            Header.Items.Add(SplitButton);
            ToolStripLabel jumpToLabel = new ToolStripLabel();
            jumpToLabel.Text = "Jump to result #";
            jumpToLabel.Size = TextRenderer.MeasureText(jumpToLabel.Text, jumpToLabel.Font) + new Size(ToolStripItemPadding.Horizontal, ToolStripItemPadding.Vertical);
            Header.Items.Add(jumpToLabel);
            ResultSetCombo = new ToolStripComboBox();
            ResultSetCombo.DropDownStyle = ComboBoxStyle.DropDownList;
            Size szMaxItem = new Size(0, 0);
            for (int ctr = 0; ctr < ResultData.IATResults.NumResultSets; ctr++)
            {
                Size sz = TextRenderer.MeasureText((ctr + 1).ToString(), ResultSetCombo.Font);
                if (sz.Width > szMaxItem.Width)
                    szMaxItem = sz;
                ResultSetCombo.Items.Add((ctr + 1).ToString());
            }
            ResultSetCombo.Size = szMaxItem + new Size(ToolStripItemPadding.Horizontal, ToolStripItemPadding.Vertical);
            ResultSetCombo.SelectedIndex = 0;
            _CurrResultSet = 0;
            ResultSetCombo.SelectedIndexChanged += new EventHandler(ResultSetCombo_SelectedIndexChanged);
            Header.Items.Add(ResultSetCombo);
            PreviousButton = new ToolStripButton();
            PreviousButton.Text = "Prev";
            PreviousButton.Size = TextRenderer.MeasureText(PreviousButton.Text, PreviousButton.Font) + new Size(ToolStripItemPadding.Horizontal, ToolStripItemPadding.Vertical);
            PreviousButton.Click += new EventHandler(PreviousButton_Click);
            Header.Items.Add(PreviousButton);
            NextButton = new ToolStripButton();
            NextButton.Text = "Next";
            NextButton.Size = TextRenderer.MeasureText(NextButton.Text, NextButton.Font) + new Size(ToolStripItemPadding.Horizontal, ToolStripItemPadding.Vertical);
            NextButton.Click += new EventHandler(NextButton_Click);
            Header.Items.Add(NextButton);
            TestItemCombo = new ToolStripComboBox();
            szMaxItem = new Size(0, 0);
            for (int ctr = 0; ctr < ResultData.ResultDescriptor.BeforeSurveys.Count + ResultData.ResultDescriptor.AfterSurveys.Count; ctr++)
            {
                Size sz = TextRenderer.MeasureText(String.Format("Survey #{0}", ctr + 1), TestItemCombo.Font);
                if (sz.Width > szMaxItem.Width)
                    szMaxItem = sz;
                sz = TextRenderer.MeasureText("IAT Score", TestItemCombo.Font);
                if (sz.Width > szMaxItem.Width)
                    szMaxItem = sz;
            }
            ToolStripLabel testItemLabel = new ToolStripLabel();
            testItemLabel.Text = "Test Item:";
            testItemLabel.Size = TextRenderer.MeasureText(testItemLabel.Text, testItemLabel.Font) + new Size(ToolStripItemPadding.Horizontal, ToolStripItemPadding.Vertical);
            Header.Items.Add(testItemLabel);
            TestItemCombo.Size = szMaxItem + new Size(ToolStripItemPadding.Horizontal, ToolStripItemPadding.Vertical);
            for (int ctr = 0; ctr < ResultData.ResultDescriptor.BeforeSurveys.Count; ctr++)
                TestItemCombo.Items.Add(String.Format("Survey #{0}", ctr + 1));
            TestItemCombo.Items.Add("IAT Score");
            for (int ctr = 0; ctr < ResultData.ResultDescriptor.AfterSurveys.Count; ctr++)
                TestItemCombo.Items.Add(String.Format("Survey #{0}", ctr + ResultData.ResultDescriptor.BeforeSurveys.Count + 1));
            TestItemCombo.DropDownStyle = ComboBoxStyle.DropDownList;
            TestItemCombo.SelectedIndex = nResultPart;
            TestItemCombo.SelectedIndexChanged += new EventHandler(TestItemCombo_SelectedIndexChanged);
            Header.Items.Add(TestItemCombo);
            Controls.Add(Header);
            ContainerPanel = new Panel();
            ContainerPanel.Location = new Point(0, Header.Height);
            ContainerPanel.Width = this.Width;
            ContainerPanel.Anchor = AnchorStyles.Bottom | AnchorStyles.Top;
            Controls.Add(ContainerPanel);
            this.Resize += new EventHandler(ResultsPanel_OnResize);
        }

        private void ResultsPanel_OnResize(object sender, EventArgs e)
        {
            if (ContainerPanel == null)
                PreviewPanel.Height = this.Height - Header.Height;
        }

        public void GeneratePreview()
        {
            ContainerPanel.Location = new Point(0, Header.Height);
            ContainerPanel.Height = this.Height - Header.Height;
            ContainerPanel.Controls.Clear();
            BeforeSurveys = ResultData.ResultDescriptor.BeforeSurveys.ToArray();
            AfterSurveys = ResultData.ResultDescriptor.AfterSurveys.ToArray();
            if (ResultPartNdx < BeforeSurveys.Length)
            {
                PreviewPanel = BeforeSurveys[ResultPartNdx].GeneratePreview(this.Width - 20);
                PreviewPanel.Dock = DockStyle.Fill;
                PreviewPanel.AutoScroll = true;
                ContainerPanel.Controls.Add(PreviewPanel);
            }
            else if (ResultPartNdx > BeforeSurveys.Length)
            {
                PreviewPanel = AfterSurveys[ResultPartNdx - BeforeSurveys.Length - 1].GeneratePreview(this.Width - 20);
                PreviewPanel.Dock = DockStyle.Fill;
                PreviewPanel.AutoScroll = true;
                ContainerPanel.Controls.Add(PreviewPanel);
            }
            else
            {
                PreviewPanel = new ItemSlidePanel(ContainerPanel.Size, ResultData, CurrResultSet);
                PreviewPanel.Dock = DockStyle.Fill;
                PreviewPanel.Anchor = AnchorStyles.Bottom | AnchorStyles.Top;
                PreviewPanel.Location = new Point(0, Header.Height);
                PreviewPanel.Height = this.Height - Header.Height;
                Controls.Add(PreviewPanel);
                Controls.Remove(ContainerPanel);
                ((ItemSlidePanel)PreviewPanel).Initialize(ItemSlideContainer);
         //       PreviewPanel.AutoScroll = true;
//                ContainerPanel.Controls.Add(PreviewPanel);
            }
/*            
            if (PreviewPanel.Height > this.Height - Header.Height)
            {
             
                PreviewPanel.VerticalScroll.Enabled = true;
                PreviewPanel.VerticalScroll.Visible = true;
                PreviewPanel.VerticalScroll.Minimum = 0;
                PreviewPanel.VerticalScroll.Maximum = PreviewPanel.Height - p.Height;
                PreviewPanel.VerticalScroll.Value = 0;
                PreviewPanel.VerticalScroll.SmallChange = PreviewPanel.Height >> 4;
                PreviewPanel.VerticalScroll.LargeChange = PreviewPanel.Height;
                PreviewPanel.Scroll += new ScrollEventHandler(PreviewPanel_Scroll);
            }
  */          
            Invalidate();
            CurrResultSet = _CurrResultSet;
        }

        private void ResultDetailsPanel_Invalidate(object sender, EventArgs e)
        {
            if (PreviewPanel.Height > this.Height - Header.Height)
            {
                PreviewPanel.VerticalScroll.Enabled = true;
                PreviewPanel.VerticalScroll.Visible = true;
                PreviewPanel.VerticalScroll.Minimum = 0;
                PreviewPanel.VerticalScroll.Maximum = PreviewPanel.Height - this.Height - Header.Height;
                PreviewPanel.VerticalScroll.Value = -PreviewPanel.Top;
                PreviewPanel.VerticalScroll.SmallChange = PreviewPanel.Height >> 4;
                PreviewPanel.VerticalScroll.LargeChange = PreviewPanel.Height;
            }
        }

        void PreviewPanel_Scroll(object sender, ScrollEventArgs e)
        {
            PreviewPanel.Location = new Point(0, -e.NewValue);
//            Invalidate();
        }

        private void PreviousButton_Click(object sender, EventArgs e)
        {
            if (CurrResultSet != 0)
                ResultSetCombo.Text = (CurrResultSet).ToString();
        }

        private void NextButton_Click(object sender, EventArgs e)
        {
            if (CurrResultSet + 1 < ResultData.ResultDescriptor.NumResults)
                ResultSetCombo.Text = (CurrResultSet + 2).ToString();
        }

        private void ResultSetCombo_SelectedIndexChanged(object sender, EventArgs e)
        {
            CurrResultSet = ResultSetCombo.SelectedIndex;
        }

        private void TestItemCombo_SelectedIndexChanged(object sender, EventArgs e)
        {
            ResultPartNdx = TestItemCombo.SelectedIndex;
        }

        private void CloseButton_Click(object sender, EventArgs e)
        {
            OnClose(this);
        }

        private void SplitButton_Click(object sender, EventArgs e)
        {
            OnSplit();
        }
    }
}

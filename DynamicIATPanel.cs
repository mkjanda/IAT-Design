/*using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;

using System.Text;
using System.Windows.Forms;

namespace IATClient
{
    public partial class DynamicIATPanel : UserControl
    {
        private ScrollingPreviewPanel StimulusScroll;
        private static Size PanelSize = new Size(1010, 645);
        private CIATBlock _Block;
        private List<CSurvey> _Surveys;
        private Panel SpecifierPanel;
        private GroupBox SpecifierPanelGroup;
        private List<SpecifierPanel> SpecifierPanelList = new List<SpecifierPanel>();
        private List<int> InitialSpecifierIDs = new List<int>();
        public delegate CIATItem RetrieveIATItemWithNdx(int ndx);

        public CIATBlock Block
        {
            get
            {
                return _Block;
            }
            set
            {
                if (InitialSpecifierIDs.Count > 0)
                    foreach (int id in InitialSpecifierIDs)
                        CDynamicSpecifier.DeleteSpecifier(id);
                foreach (SpecifierPanel sp in SpecifierPanelList)
                {
                    List<CDynamicSpecifier> specifiers = sp.GetDefinedSpecifiers();
                    foreach (CDynamicSpecifier s in specifiers)
                        CDynamicSpecifier.AddSpecifier(s);
                }
                _Block = value;
                SuspendLayout();
                StimulusScroll.Clear();
                for (int ctr = 0; ctr < value.NumItems; ctr++)
                    StimulusScroll.InsertIATItemPreview(ctr, value[ctr]);
                ResumeLayout();
                InitialSpecifierIDs.Clear();
                for (int ctr = 0; ctr < Block.NumItems; ctr++)
                    if (Block[ctr].KeySpecifierID != -1)
                        InitialSpecifierIDs.Add(Block[ctr].KeySpecifierID);
            }
        }

        private CIATItem GetIATItemWithNdx(int ndx)
        {
            return Block[ndx];
        }

        public List<CSurvey> Surveys
        {
            get
            {
                return _Surveys;
            }
            set
            {
                _Surveys = value;
                SurveyDrop.Items.Clear();
                for (int ctr = 0; ctr < value.Count; ctr++)
                    SurveyDrop.Items.Add(value[ctr].Name);
            }
        }

        public DynamicIATPanel()
        {
            InitializeComponent();
            StimulusScroll = new ScrollingPreviewPanel();
            StimulusScroll.Orientation = ScrollingPreviewPanel.EOrientation.vertical;
            StimulusScroll.Size = new Size(122, 645);
            StimulusScroll.PreviewSize = ImageManager.CImageManager.ThumbnailSize;
            StimulusScroll.Location = new Point(0, 0);
            Controls.Add(StimulusScroll);

            SurveyDrop.SelectedIndexChanged += new EventHandler(SurveyDrop_SelectedIndexChanged);

            SpecifierPanelGroup = new GroupBox();
            SpecifierPanelGroup.Location = new Point(122, 25);
            SpecifierPanelGroup.Size = new Size(888, 620);
            Controls.Add(SpecifierPanelGroup);

            SpecifierPanel = new Panel();
            SpecifierPanel.AutoScroll = true;
            SpecifierPanel.Location = new Point(0, 0);
            SpecifierPanel.Dock = DockStyle.Fill;
            SpecifierPanel.Size = SpecifierPanelGroup.Size;
            SpecifierPanel.BackColor = System.Drawing.Color.White;
            SpecifierPanelGroup.Controls.Add(SpecifierPanel);

            this.ParentChanged += new EventHandler(DynamicIATPanel_ParentChanged);
        }

        void DynamicIATPanel_ParentChanged(object sender, EventArgs e)
        {
            if (Parent == null)
            {
                StimulusScroll.Clear();
                StimulusScroll.StopTimer();
                SpecifierPanel.Controls.Clear();
                foreach (int i in InitialSpecifierIDs)
                    CDynamicSpecifier.DeleteSpecifier(i);
                InitialSpecifierIDs.Clear();
                List<CDynamicSpecifier> specifiers;
                foreach (SpecifierPanel p in SpecifierPanelList)
                {
                    specifiers = p.GetDefinedSpecifiers();
                    foreach (CDynamicSpecifier ds in specifiers)
                        CDynamicSpecifier.AddSpecifier(ds);
                }
            }
            else
            {
            }
        }

        private void SurveyDrop_SelectedIndexChanged(object sender, EventArgs e)
        {
            PopulateSpecifierPanel(Surveys[SurveyDrop.SelectedIndex]);
        }

        private void PopulateSpecifierPanel(CSurvey survey)
        {
            SpecifierPanel.SuspendLayout();
            SpecifierPanel.Controls.Clear();
            SpecifierPanel.Size = new Size(SpecifierPanelGroup.Width - 6, SpecifierPanelGroup.Height - 19);
            int totalHeight = 5;
            SpecifierPanel p;
            for (int ctr = 0; ctr < survey.Items.Count; ctr++)
            {
                if (survey.Items[ctr].IsCaption)
                    continue;
                p = null;
                switch (survey.Items[ctr].Response.ResponseType)
                {
                    case CResponse.EResponseType.Boolean:
                        p = new TrueFalseSpecifierControl(survey, survey.Items[ctr], SpecifierPanel.Width - 45);
                        break;

                    case CResponse.EResponseType.BoundedLength:
                        p = new InertSpecifierControl(survey, survey.Items[ctr], SpecifierPanel.Width - 45);
                        break;

                    case CResponse.EResponseType.BoundedNum:
                        p = new InertSpecifierControl(survey, survey.Items[ctr], SpecifierPanel.Width - 45);
                        break;

                    case CResponse.EResponseType.Date:
                        p = new InertSpecifierControl(survey, survey.Items[ctr], SpecifierPanel.Width - 45);
                        break;

                    case CResponse.EResponseType.FixedDig:
                        p = new InertSpecifierControl(survey, survey.Items[ctr], SpecifierPanel.Width - 45);
                        break;

                    case CResponse.EResponseType.Instruction:
                        p = new InertSpecifierControl(survey, survey.Items[ctr], SpecifierPanel.Width - 45);
                        break;

                    case CResponse.EResponseType.Likert:
                        p = new RangeSpecifierControl(survey, survey.Items[ctr], SpecifierPanel.Width - 45);
                        break;

                    case CResponse.EResponseType.MultiBoolean:
                        p = new MaskSpecifierControl(survey, survey.Items[ctr], SpecifierPanel.Width - 45);
                        break;

                    case CResponse.EResponseType.Multiple:
                        p = new SelectionSpecifierControl(survey, survey.Items[ctr], SpecifierPanel.Width - 45);
                        break;

                    case CResponse.EResponseType.RegEx:
                        p = new InertSpecifierControl(survey, survey.Items[ctr], SpecifierPanel.Width - 45);
                        break;

                    case CResponse.EResponseType.WeightedMultiple:
                        p = new SelectionSpecifierControl(survey, survey.Items[ctr], SpecifierPanel.Width - 45);
                        break;
                }
                p.IATItemRetriever = new RetrieveIATItemWithNdx(GetIATItemWithNdx);
                p.Location = new Point(15, totalHeight);
                SpecifierPanel.Controls.Add(p);
                SpecifierPanelList.Add(p);
                totalHeight += p.Height + 10;
            }
            if (totalHeight > SpecifierPanel.Height)
            {
                SpecifierPanel.VerticalScroll.Enabled = true;
                SpecifierPanel.VerticalScroll.Visible = true;
                SpecifierPanel.VerticalScroll.Minimum = 0;
                SpecifierPanel.VerticalScroll.Maximum = totalHeight;
                SpecifierPanel.VerticalScroll.Value = 0;
            }
            else
                SpecifierPanel.VerticalScroll.Enabled = false;
            SpecifierPanel.ResumeLayout(true);
        }

        private void DoneButton_Click(object sender, EventArgs e)
        {
             ((IATConfigMainForm)Parent).FormContents = IATConfigMainForm.EFormContents.Main;
        }

    }
}
*/
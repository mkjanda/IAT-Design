using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Globalization;
using System.Text;
using System.Windows.Forms;

namespace IATClient
{
    public partial class NewSurveyPanel : UserControl
    {
        public enum EOrdinality { before, after, none };
        private Button UniqueResponseButton, EditUniqueResponseButton;
        private EOrdinality _Ordinality;
        private List<String> _UniqueResponseValues = new List<String>();
        private List<CSurveyItem> Clipboard;
        private bool UpdatingFromCode;
        private int _SurveyNdx = -1;
        private bool bUniqueResponseOnClipboard = false;

        public List<String> UniqueResponseValues
        {
            get
            {
                return _UniqueResponseValues;
            }
        }

        public int UniqueResponseItemNum
        {
            get
            {
                return SurveyView.UniqueResponseItemNum;
            }
        }

        public int SurveyNdx
        {
            get
            {
                return _SurveyNdx;
            }
            set
            {
                _SurveyNdx = value;
            }
        }

        private bool ContainsUniqueResponse
        {
            get
            {
                IATConfigMainForm mainForm = (IATConfigMainForm)Application.OpenForms[Properties.Resources.sMainFormName];
                if (mainForm.IAT.UniqueResponse.SurveyName == mainForm.ActiveItem.Name)
                    return true;
                return false;
            }
        }

        public decimal SurveyTimeout
        {
            get
            {
                if (TimeoutText.Text == String.Empty)
                    return 0;
                return Convert.ToDecimal(TimeoutText.Text);
            }
            set {
                if (value == 0)
                    TimeoutText.Text = String.Empty;
                else
                    TimeoutText.Text = value.ToString();
            }
        }

        public List<CSurveyItem> SurveyItems
        {
            get
            {
                if (this.DesignMode)
                    return null;
                return SurveyView.SurveyItems;
            }
            set
            {
                SurveyView.SurveyItems = value;
                DiscernUniqueResponse();
            }
        }


        private bool ContainsUniqueResponseCandidate()
        {
            IATConfigMainForm mainForm = (IATConfigMainForm)Application.OpenForms[Properties.Resources.sMainFormName];
            String surveyName;
            if (Ordinality == EOrdinality.before)
                surveyName = mainForm.IAT.BeforeSurvey[SurveyNdx].Name;
            else
                surveyName = mainForm.IAT.AfterSurvey[SurveyNdx].Name;
            if (mainForm.IAT.UniqueResponse.ItemNum == -1)
            {
                UniqueResponseButton.Text = "Designate Unique Resp";
                foreach (CSurveyItem si in SurveyItems)
                {
                    if (si.Response != null)
                    {
                        if ((si.Response.ResponseType == CResponse.EResponseType.BoundedLength) || (si.Response.ResponseType == CResponse.EResponseType.FixedDig) ||
                            (si.Response.ResponseType == CResponse.EResponseType.RegEx))
                            return true;
                    }
                }
            }
            return false;
        }

        private void DiscernUniqueResponse()
        {
            IATConfigMainForm mainForm = (IATConfigMainForm)Application.OpenForms[Properties.Resources.sMainFormName];
            String surveyName;
            if (Ordinality == EOrdinality.before)
                surveyName = mainForm.IAT.BeforeSurvey[SurveyNdx].Name;
            else
                surveyName = mainForm.IAT.AfterSurvey[SurveyNdx].Name;
            if (mainForm.IAT.UniqueResponse.ItemNum == -1)
            {
                UniqueResponseButton.Text = "Designate Unique Resp";
                UniqueResponseButton.Enabled = ContainsUniqueResponseCandidate();
            }
            else if (surveyName == mainForm.IAT.UniqueResponse.SurveyName)
            {
                UniqueResponseButton.Text = "Remove Unique Resp";
                UniqueResponseButton.Enabled = true;
                int uNdx = mainForm.IAT.UniqueResponse.ItemNum;
                int ctr = 0, itemNdx = 0;
                while (itemNdx < uNdx) {
                    if (SurveyItems[ctr++].Response == null)
                        continue;
                    if (SurveyItems[ctr - 1].Response.ResponseType == CResponse.EResponseType.Instruction)
                        continue;
                    itemNdx++;
                }
                SurveyView.SetUniqueResponse(ctr - 1);
            }
            else
            {
                UniqueResponseButton.Text = "Unique Resp Assigned";
                UniqueResponseButton.Enabled = false;
            }
        }

        public EOrdinality Ordinality
        {
            get
            {
                return _Ordinality;
            }
            set
            {
                _Ordinality = value;
            }
        }

        public NewSurveyPanel(EOrdinality ord, int ndx)
        {
            _Ordinality = ord;
            SurveyNdx = ndx;
            InitializeComponent();
            AppendRadio.Checked = true;
            InsertRadio.Enabled = false;
            Clipboard = new List<CSurveyItem>();
            SurveyView.EnableInsert = new SurveyDisplay.InsertEnabler(EnableInsert);
            ReturnButton.Click += new EventHandler(ReturnButton_Click);
            SurveyView.SetCaptionCheck = new SurveyDisplay.CaptionCheckSetter(CaptionCheck_Set);
            UniqueResponseButton = new Button();
            UniqueResponseButton.Size = new Size(ReturnButton.Width, ReturnButton.Height);
            UniqueResponseButton.Location = new Point(ReturnButton.Left, ReturnButton.Top - (UniqueResponseButton.Height << 1) - 10);
            Controls.Add(UniqueResponseButton);
            UniqueResponseButton.Click += new EventHandler(UniqueResponseButton_Click);
            UniqueResponseButton.Anchor = AnchorStyles.Right | AnchorStyles.Bottom;
            EditUniqueResponseButton = new Button();
            EditUniqueResponseButton.Size = new Size(ReturnButton.Width, ReturnButton.Height);
            EditUniqueResponseButton.Location = new Point(ReturnButton.Left, ReturnButton.Top - (UniqueResponseButton.Height) - 5);
            EditUniqueResponseButton.Click += new EventHandler(EditUniqueResponseButton_Click);
            EditUniqueResponseButton.Anchor = AnchorStyles.Right | AnchorStyles.Bottom;
            EditUniqueResponseButton.Text = "Edit Unique";
            EditUniqueResponseButton.Enabled = ContainsUniqueResponse;
            Controls.Add(EditUniqueResponseButton);
        }

        private void EditUniqueResponseButton_Click(object sender, EventArgs e)
        {
            UniqueResponseForm urf = new UniqueResponseForm(null);
            urf.UniqueResponseValues.Clear();
            urf.UniqueResponseValues = UniqueResponseValues;
            urf.UniqueResponseItem = SurveyView.GetUniqueResponseItem();
            if (urf.ShowDialog() == DialogResult.OK)
            {
                UniqueResponseValues.Clear();
                UniqueResponseValues.AddRange(urf.UniqueResponseValues);
            }
        }

        void ReturnButton_Click(object sender, EventArgs e)
        {
            CultureInfo ci = new CultureInfo("en-us");
            IATConfigMainForm mainForm;
            if ((Parent == null) || (Ordinality == EOrdinality.none))
                return;
            try
            {
                if (TimeoutText.Text != String.Empty)
                    Convert.ToDecimal(TimeoutText.Text, ci);
            }
            catch (Exception)
            {
                MessageBox.Show("Survey Timeout value must either be a decimal number or empty.");
                return;
            }
            mainForm = (IATConfigMainForm)Parent;
            mainForm.FormContents = IATConfigMainForm.EFormContents.Main;
        }

        public void EnableInsert(bool Enable, bool SelectInsert)
        {
            InsertRadio.Enabled = Enable;
            if (Enable == false)
                AppendRadio.Checked = true;
            else if (SelectInsert)
                InsertRadio.Checked = true;
        }

        public void CaptionCheck_Set(bool Checked)
        {
            UpdatingFromCode = true;
            CaptionCheck.Checked = Checked;
            UpdatingFromCode = false;
        }

        private void NewSurveyPanel_Load(object sender, EventArgs e)
        {
            SurveyView.BackColor = System.Drawing.Color.White;
        }

        private void AddTrueFalseItem_Click(object sender, EventArgs e)
        {
            SurveyView.AddItem(CResponse.EResponseType.Boolean, InsertRadio.Checked);
        }

        private void AddLikertItem_Click(object sender, EventArgs e)
        {
            SurveyView.AddItem(CResponse.EResponseType.Likert, InsertRadio.Checked);
        }

        private void AddMultiChoiceItem_Click(object sender, EventArgs e)
        {
            SurveyView.AddItem(CResponse.EResponseType.Multiple, InsertRadio.Checked);
        }

        private void AddWeightedMultiChoiceItem_Click(object sender, EventArgs e)
        {
            SurveyView.AddItem(CResponse.EResponseType.WeightedMultiple, InsertRadio.Checked);
        }

        private void AddMutliSelectionItem_Click(object sender, EventArgs e)
        {
            SurveyView.AddItem(CResponse.EResponseType.MultiBoolean, InsertRadio.Checked);
        }

        private void AddBoundedLengthItem_Click(object sender, EventArgs e)
        {
            SurveyView.AddItem(CResponse.EResponseType.BoundedLength, InsertRadio.Checked);
            IATConfigMainForm mainForm = (IATConfigMainForm)Application.OpenForms[Properties.Resources.sMainFormName];
            if (mainForm.IAT.UniqueResponse.ItemNum == -1)
                UniqueResponseButton.Enabled = true;
        }

        private void AddBoundedNumberItem_Click(object sender, EventArgs e)
        {
            SurveyView.AddItem(CResponse.EResponseType.BoundedNum, InsertRadio.Checked);
        }

        private void AddFixedDigitItem_Click(object sender, EventArgs e)
        {
            SurveyView.AddItem(CResponse.EResponseType.FixedDig, InsertRadio.Checked);
            IATConfigMainForm mainForm = (IATConfigMainForm)Application.OpenForms[Properties.Resources.sMainFormName];
            if (mainForm.IAT.UniqueResponse.ItemNum == -1)
                UniqueResponseButton.Enabled = true;
        }

        private void AddDateItem_Click(object sender, EventArgs e)
        {
            SurveyView.AddItem(CResponse.EResponseType.Date, InsertRadio.Checked);
        }

        private void AddRegExItem_Click(object sender, EventArgs e)
        {
            SurveyView.AddItem(CRegExResponse.EResponseType.RegEx, InsertRadio.Checked);
            IATConfigMainForm mainForm = (IATConfigMainForm)Application.OpenForms[Properties.Resources.sMainFormName];
            if (mainForm.IAT.UniqueResponse.ItemNum == -1)
                UniqueResponseButton.Enabled = true;
        }

        private void Cut_Click(object sender, EventArgs e)
        {
            if (bUniqueResponseOnClipboard)
            {
                UniqueResponseButton.Text = "Designate Unique Resp";
                UniqueResponseButton.Enabled = ContainsUniqueResponseCandidate();
                bUniqueResponseOnClipboard = false;
            }
            if (SurveyView.SelectionContainsUniqueResponse())
            {
                bUniqueResponseOnClipboard = true;
                EditUniqueResponseButton.Enabled = false;
            }
            SurveyView.CutSelected();
        }

        private void Copy_Click(object sender, EventArgs e)
        {
            SurveyView.CopySelected();
        }

        private void Paste_Click(object sender, EventArgs e)
        {
            if (bUniqueResponseOnClipboard)
            {
                bUniqueResponseOnClipboard = false;
                EditUniqueResponseButton.Enabled = true;
            }
            if (InsertRadio.Checked)
                SurveyView.PasteInsert();
            else
                SurveyView.PasteAppend();
        }

        private void Delete_Click(object sender, EventArgs e)
        {
            if (SurveyView.SelectionContainsUniqueResponse()) {
                UniqueResponseButton.Text = "Designate Unique Resp";
                UniqueResponseButton.Enabled = ContainsUniqueResponseCandidate();
                EditUniqueResponseButton.Enabled = false;
            }
            SurveyView.DeleteSelected();
        }

        private void CaptionCheck_CheckedChanged(object sender, EventArgs e)
        {
            if (UpdatingFromCode)
                return;
            if (CaptionCheck.Checked)
                SurveyView.AddCaption();
            else
                SurveyView.RemoveCaption();
        }

        private void AddInstructions_Click(object sender, EventArgs e)
        {
            SurveyView.AddInstructions(InsertRadio.Checked);
        }

        private void UniqueResponseButton_Click(object sender, EventArgs e)
        {
            if (SurveyView.UniqueResponseItemNum != -1)
            {
                ((IATConfigMainForm)Application.OpenForms[Properties.Resources.sMainFormName]).IAT.UniqueResponse.Clear();
                UniqueResponseButton.Text = "Designate Unique Resp";
                UniqueResponseButton.Enabled = ContainsUniqueResponseCandidate();
                SurveyView.ClearUniqueResponseItem();
            }
            else
            {
                List<CSurveyItem> siList = SurveyItems;
                List<CSurveyItem> urc = new List<CSurveyItem>();
                foreach (CSurveyItem si in siList)
                {
                    if (si.Response != null)
                    {
                        if ((si.Response.ResponseType == CResponse.EResponseType.BoundedLength) || (si.Response.ResponseType == CResponse.EResponseType.FixedDig) ||
                            (si.Response.ResponseType == CResponse.EResponseType.RegEx))
                            urc.Add(si);
                    }
                }
                UniqueResponseForm urf = new UniqueResponseForm(urc);
                if (urf.ShowDialog() == DialogResult.OK)
                {
                    _UniqueResponseValues.Clear();
                    _UniqueResponseValues.AddRange(urf.UniqueResponseValues);
                    UniqueResponseButton.Text = "Unique Resp Assigned";
                    SurveyView.SetUniqueResponse(siList.IndexOf(urf.UniqueResponseItem));
                    EditUniqueResponseButton.Enabled = true;
                }
            }
        }
    }
}

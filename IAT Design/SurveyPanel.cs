using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Forms;

namespace IATClient
{
    public partial class SurveyPanel : UserControl
    {
        public enum EOrdinality { before, after, none };
        private Button UniqueResponseButton, EditUniqueResponseButton;
        private EOrdinality _Ordinality;
        private List<CSurveyItem> Clipboard;
        private bool UpdatingFromCode;
        private int _SurveyNdx = -1;
        private bool bUniqueResponseOnClipboard = false;
        private Control[] SurveyControls = new Control[] { };
        private SurveyTextFormatPanel TextFormatPanel = null;
        private QuestionDisplay QuestionBeingFormatted = null;

        public int UniqueResponseItemNum
        {
            get
            {
                return SurveyView.UniqueResponseItemNum;
            }
        }

        private bool ContainsUniqueResponse
        {
            get
            {
                IATConfigMainForm mainForm = (IATConfigMainForm)Application.OpenForms[Properties.Resources.sMainFormName];
                if (CIAT.SaveFile.IAT.UniqueResponse.SurveyUri == null)
                    return false;
                if (CIAT.SaveFile.IAT.UniqueResponse.SurveyUri.Equals(mainForm.ActiveItem.URI))
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
            set
            {
                if (value == 0)
                    TimeoutText.Text = String.Empty;
                else
                    TimeoutText.Text = value.ToString();
            }
        }


        private Int32 SurveyViewRefreshStatus = 0;

        public CSurvey Survey
        {
            get
            {
                if (this.DesignMode)
                    return null;
                return SurveyView.Survey;
            }
            set
            {
                Action<bool, bool> a = new Action<bool, bool>((a, b) => EnableInsert(a, b));
                var t = this;
                Action<CSurvey> action = new Action<CSurvey>((value) =>
                {
                    int ScrollBarWidth;
                    using (Graphics g = Graphics.FromHwnd(this.Handle))
                        ScrollBarWidth = System.Windows.Forms.ScrollBarRenderer.GetSizeBoxSize(g, System.Windows.Forms.VisualStyles.ScrollBarState.Normal).Width;
                    if (this.SurveyView != null)
                    {
                        if (SurveyDisplayPanel.Controls.Contains(SurveyView))
                        {
                            SurveyDisplayPanel.Controls.Remove(SurveyView);
                            this.SurveyView.Dispose();
                        }
                    }
                    this.SurveyView = new SurveyDisplay()
                    {
                        BackColor = Color.White,
                        Width = SurveyDisplayPanel.Width
                    };
                    SurveyView.EnableInsert = (a, b) => EnableInsert(a, b);
                    SurveyView.Resize += (sender, args) =>
                    {
                        if (Interlocked.Equals(SurveyViewRefreshStatus, 3) || Interlocked.Equals(SurveyViewRefreshStatus, 2))
                            return;
                        if (Interlocked.CompareExchange(ref SurveyViewRefreshStatus, 2, 3) == 3)
                            return;
                        if (Interlocked.CompareExchange(ref SurveyViewRefreshStatus, 1, 2) == 2)
                            return;
                        Action OnResize = () =>
                        {
                            if (SurveyView.Height > SurveyDisplayPanel.Size.Height - SurveyDisplayPanel.Margin.Vertical)
                                SurveyView.QuestionDisplayWidth = SurveyDisplayPanel.Size.Width - ScrollBarWidth - SurveyDisplayPanel.Margin.Horizontal;
                            else if (SurveyView.Height < SurveyDisplayPanel.Size.Height)
                                SurveyView.QuestionDisplayWidth = SurveyDisplayPanel.Size.Width - SurveyDisplayPanel.Margin.Horizontal;
                            SurveyDisplayPanel.HorizontalScroll.Enabled = false;
                            SurveyDisplayPanel.HorizontalScroll.Visible = false;
                            SurveyDisplayPanel.HorizontalScroll.Maximum = 0;
                            SurveyDisplayPanel.AutoScroll = true;
                            //                            SurveyView.RecalcSize(true);
                            SurveyDisplayPanel.Invalidate();
                        };
                        OnResize();
                        if (Interlocked.CompareExchange(ref SurveyViewRefreshStatus, 3, 2) == 2)
                            OnResize();
                        SurveyViewRefreshStatus = 0;
                    };
                    SurveyDisplayPanel.Controls.Add(SurveyView);
                    SurveyDisplayPanel.AutoScroll = true;
                    if (IsHandleCreated)
                    {
                        SurveyView.Survey = value;
                        SurveyView.SetCaptionCheck = (a) => CaptionCheck_Set(a);
                        DiscernUniqueResponse();
                    }
                    else
                        HandleCreated += (sender, args) =>
                        {
                            SurveyView.Survey = value;
                            SurveyView.SetCaptionCheck = (a) => CaptionCheck_Set(a);
                            DiscernUniqueResponse();
                        };
                });
                if (IsHandleCreated)
                    HandleCreated += (sender, obj) => action.Invoke(value);
                else
                    action.Invoke(value);

            }
        }


        private bool ContainsUniqueResponseCandidate()
        {
            if (CIAT.SaveFile.IAT.UniqueResponse.ItemNum == -1)
            {
                UniqueResponseButton.Text = "Designate Unique Resp";
                foreach (CSurveyItem si in Survey.Items)
                {
                    if (si == null)
                        continue;
                    if (CUniqueResponse.UniqueResponseTypes.Contains(si.Response.ResponseType))
                        return true;
                }
            }
            return false;
        }

        private void DiscernUniqueResponse()
        {
            IATConfigMainForm mainForm = (IATConfigMainForm)Application.OpenForms[Properties.Resources.sMainFormName];
            String surveyName = Survey.Name;
            if ((CIAT.SaveFile.IAT.UniqueResponse.SurveyItemUri == null) || (CIAT.SaveFile.IAT.UniqueResponse.SurveyUri == null))
            {
                UniqueResponseButton.Text = "Designate Unique Resp";
                UniqueResponseButton.Enabled = ContainsUniqueResponseCandidate();
            }
            else if (CIAT.SaveFile.IAT.UniqueResponse.SurveyUri.Equals(Survey.URI) && (CIAT.SaveFile.IAT.UniqueResponse.SurveyItemUri != null))
            {
                UniqueResponseButton.Text = "Remove Unique Resp";
                UniqueResponseButton.Enabled = true;
                int uNdx = CIAT.SaveFile.IAT.UniqueResponse.ItemNum;
                int ctr = 0, itemNdx = 0;
                while (itemNdx < uNdx)
                {
                    if (Survey.Items[ctr++].Response == null)
                        continue;
                    if (Survey.Items[ctr - 1].Response.ResponseType == CResponse.EResponseType.Instruction)
                        continue;
                    itemNdx++;
                }
                SurveyView.UpdateUniqueResponse();
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

        public SurveyPanel(EOrdinality ord)
        {
            InitializeComponent();
            SuspendLayout();
            _Ordinality = ord;

            AppendRadio.Checked = true;
            InsertRadio.Enabled = false;
            Clipboard = new List<CSurveyItem>();
            ReturnButton.Click += new EventHandler(ReturnButton_Click);
            UniqueResponseButton = new Button();
            UniqueResponseButton.Size = new Size(ReturnButton.Width, ReturnButton.Height);
            UniqueResponseButton.Location = new Point(ReturnButton.Left, ReturnButton.Top - (UniqueResponseButton.Height << 1) - 10);
            if (CIAT.SaveFile.IAT.UniqueResponse.SurveyUri != null)
            {
                if (CIAT.SaveFile.IAT.UniqueResponse.URI.Equals((Application.OpenForms[Properties.Resources.sMainFormName] as IATConfigMainForm).ActiveItem.URI))
                {
                    UniqueResponseButton.Text = "Remove Unique Resp";
                    UniqueResponseButton.Enabled = true;
                }
                else
                {
                    UniqueResponseButton.Text = "Unique Resp Assigned";
                    UniqueResponseButton.Enabled = false;
                }
            }
            else
                UniqueResponseButton.Text = "Designate Unique Resp";
            Controls.Add(UniqueResponseButton);
            UniqueResponseButton.Click += new EventHandler(UniqueResponseButton_Click);
            UniqueResponseButton.Anchor = AnchorStyles.Right | AnchorStyles.Bottom;
            EditUniqueResponseButton = new Button();
            EditUniqueResponseButton.Size = new Size(ReturnButton.Width, ReturnButton.Height);
            EditUniqueResponseButton.Location = new Point(ReturnButton.Left, ReturnButton.Top - (UniqueResponseButton.Height) - 5);
            EditUniqueResponseButton.Click += new EventHandler(EditUniqueResponseButton_Click);
            EditUniqueResponseButton.Anchor = AnchorStyles.Right | AnchorStyles.Bottom;
            EditUniqueResponseButton.Text = "Edit Unique Resp";
            EditUniqueResponseButton.Enabled = ContainsUniqueResponse;
            Controls.Add(EditUniqueResponseButton);
            SurveyControls = new Control[] { this.EditPanel, this.ItemsPanel, this.AddInsertPanel, this.CaptionCheck, this.TimeoutLabel, this.TimeoutText, this.MinLabel,
                this.ReturnButton, this.UniqueResponseButton, this.EditUniqueResponseButton };
            CIAT.Dispatcher.AddListener<IFormatSurveyItemText>(BeginFormatSurveyItemText);
            SurveyDisplayPanel.BackColor = Color.White;
            SurveyDisplayPanel.HorizontalScroll.Enabled = false;
            SurveyDisplayPanel.HorizontalScroll.Visible = false;
            SurveyDisplayPanel.AutoScroll = false;
            ResumeLayout(false);
        }

        ~SurveyPanel()
        {
            CIAT.Dispatcher.RemoveListener<IFormatSurveyItemText>(BeginFormatSurveyItemText);
        }

        private void BeginFormatSurveyItemText(IFormatSurveyItemText e)
        {
            SuspendLayout();
            foreach (Control c in SurveyControls)
                Controls.Remove(c);
            if (TextFormatPanel == null)
            {
                TextFormatPanel = new SurveyTextFormatPanel(e.Display.ItemType);
            }
            else
            {
                TextFormatPanel.ItemType = e.Display.ItemType;
            }
            if (QuestionBeingFormatted != null)
                QuestionBeingFormatted.EndSurveyItemTextFormat();
            QuestionBeingFormatted = e.Display;
            TextFormatPanel.OnDone += (s, args) =>
            {
                Controls.Remove(TextFormatPanel);
                Controls.AddRange(SurveyControls);
                QuestionBeingFormatted.EndSurveyItemTextFormat();
                this.Invalidate();
            };
            TextFormatPanel.ItemFormat = e.Display.Format;
            if (e.Display.ItemType != CResponse.EResponseType.Instruction)
                TextFormatPanel.ResponseFormat = e.Display.ResponseFormat;
            TextFormatPanel.Location = new Point(this.SurveyDisplayPanel.Right + 10, 0);
            TextFormatPanel.Size = new Size(this.Width - this.SurveyDisplayPanel.Right - 5, this.Height);
            Controls.Add(TextFormatPanel);
            ResumeLayout(false);
        }

        private void EditUniqueResponseButton_Click(object sender, EventArgs e)
        {
            UniqueResponseForm urf = new UniqueResponseForm(null);
            urf.UniqueResponseValues.Clear();
            urf.UniqueResponseValues = CIAT.SaveFile.IAT.UniqueResponse.Values;
            urf.UniqueResponseItem = SurveyView.GetUniqueResponseItem();
            if (urf.ShowDialog() == DialogResult.OK)
            {
                CIAT.SaveFile.IAT.UniqueResponse.Values.Clear();
                CIAT.SaveFile.IAT.UniqueResponse.Values.AddRange(urf.UniqueResponseValues);
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
            SurveyView.RefreshSurveyItems();
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
            if (CIAT.SaveFile.IAT.UniqueResponse.ItemNum == -1)
                UniqueResponseButton.Enabled = true;
        }

        private void AddBoundedNumberItem_Click(object sender, EventArgs e)
        {
            SurveyView.AddItem(CResponse.EResponseType.BoundedNum, InsertRadio.Checked);
            if (CIAT.SaveFile.IAT.UniqueResponse.ItemNum == -1)
                UniqueResponseButton.Enabled = true;
        }

        private void AddFixedDigitItem_Click(object sender, EventArgs e)
        {
            SurveyView.AddItem(CResponse.EResponseType.FixedDig, InsertRadio.Checked);
            IATConfigMainForm mainForm = (IATConfigMainForm)Application.OpenForms[Properties.Resources.sMainFormName];
            if (CIAT.SaveFile.IAT.UniqueResponse.ItemNum == -1)
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
            if (CIAT.SaveFile.IAT.UniqueResponse.ItemNum == -1)
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
            else if (SurveyView.SelectionContainsUniqueResponseCandidate())
                EditUniqueResponseButton.Enabled = false;
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
            if (SurveyView.SelectionContainsUniqueResponse())
            {
                UniqueResponseButton.Text = "Designate Unique Resp";
                UniqueResponseButton.Enabled = ContainsUniqueResponseCandidate();
                EditUniqueResponseButton.Enabled = false;
                CIAT.SaveFile.IAT.UniqueResponse.Dispose();
            }
            else if ((SurveyView.SelectionContainsUniqueResponseCandidate()) && (CIAT.SaveFile.IAT.UniqueResponse.SurveyUri == null))
            {
                UniqueResponseButton.Text = "Designate Unique Resp";
                UniqueResponseButton.Enabled = true;
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
            SurveyView.AddItem(CResponse.EResponseType.Instruction, InsertRadio.Checked);
        }

        private void AddImage_Click(object sender, EventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.Filter = Properties.Resources.sImageFileFilter;
            dlg.Title = Properties.Resources.sOpenImageFileDialogTitle;
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                if (new FileInfo(dlg.FileName).Length >= 4194304)
                {
                    MessageBox.Show("Only pictures of 4MB or smaller are allowed.", "File Too Large");
                    return;
                }
                SurveyView.AddImage(InsertRadio.Checked, dlg.FileName);
            }
        }

        private void UniqueResponseButton_Click(object sender, EventArgs e)
        {
            if (SurveyView.UniqueResponseItemNum != -1)
            {
                CIAT.SaveFile.IAT.UniqueResponse.Clear();
                UniqueResponseButton.Text = "Designate Unique Resp";
                UniqueResponseButton.Enabled = ContainsUniqueResponseCandidate();
                SurveyView.ClearUniqueResponseItem();
                EditUniqueResponseButton.Enabled = false;
            }
            else
            {
                List<CSurveyItem> siList = Survey.Items.ToList();
                List<CSurveyItem> urc = new List<CSurveyItem>();
                foreach (CSurveyItem si in siList)
                {
                    if (si == null)
                        continue;
                    if (CUniqueResponse.UniqueResponseTypes.Contains(si.Response.ResponseType))
                        urc.Add(si);
                }
                UniqueResponseForm urf = new UniqueResponseForm(urc);
                if (urf.ShowDialog() == DialogResult.OK)
                {
                    UniqueResponseButton.Text = "Unique Resp Assigned";
                    UniqueResponseButton.Enabled = false;
                    CIAT.SaveFile.IAT.UniqueResponse.Values.Clear();
                    CIAT.SaveFile.IAT.UniqueResponse.Values.AddRange(urf.UniqueResponseValues);
                    CIAT.SaveFile.IAT.UniqueResponse.SurveyItemUri = urf.UniqueResponseItem.URI;
                    CIAT.SaveFile.IAT.UniqueResponse.SurveyUri = Survey.URI;
                    SurveyView.UpdateUniqueResponse();
                    EditUniqueResponseButton.Enabled = true;
                }
            }
        }
        public new void Dispose()
        {
            SurveyView.Dispose();
            base.Dispose();
        }

    }
}

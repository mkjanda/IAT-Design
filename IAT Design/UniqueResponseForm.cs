using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;

namespace IATClient
{
    partial class UniqueResponseForm : Form
    {
        private List<CSurveyItem> SurveyItems = new List<CSurveyItem>();
        private List<String> Values = new List<String>();
        private bool Closed = false;
        private ManualResetEvent ShutdownSignal = new ManualResetEvent(true);
        private object lockObject = new object();

        public List<String> UniqueResponseValues
        {
            get
            {
                return Values;
            }
            set
            {
                Values.Clear();
                if (value.Count > 0)
                {
                    Values.AddRange(value);
                    ResponseBox.Text = "Populating . . .\r\n";
                    ResponseBox.Enabled = false;
                }
                ResponseBox.BackColor = Color.White;
            }
        }

        private void UpdateResponseBox()
        {
            if (Values.Count == 0)
                return;
            String str = String.Empty;
            foreach (String val in Values)
            {
                str += val + "\r\n";
                if (str.Length > 10000)
                {
                    IAsyncResult async = this.BeginInvoke((Func<String, Boolean>)AppendResponseBoxText, str);
                    if (Closed)
                    {
                        ShutdownSignal.Set();
                        return;
                    }
                    EndInvoke(async);
                    str = String.Empty;
                }
                else
                    ShutdownSignal.Set();
            }
            this.Invoke((Action<String>)FinalAppendResponseBoxText, str);
        }

        private Boolean AppendResponseBoxText(String str)
        {
            ResponseBox.Text += str;
            return true;
        }

        private void FinalAppendResponseBoxText(String str)
        {
            ResponseBox.Text += str;
            ResponseBox.Text = ResponseBox.Text.Substring("Populating . . .\r\n".Length);
            ResponseBox.Enabled = true;
            OKButton.Enabled = true;
        }

        public CSurveyItem UniqueResponseItem
        {
            get
            {
                if (SurveyItemDrop.SelectedIndex == -1)
                    return null;
                return SurveyItems[SurveyItemDrop.SelectedIndex];
            }
            set
            {
                SurveyItemDrop.Items.Add(value.Text);
                SurveyItemDrop.SelectedIndex = 0;
                SurveyItems.Add(value);
            }
        }

        public UniqueResponseForm(List<CSurveyItem> surveyItems)
        {
            this.Name = "UniqueResponseForm";
            InitializeComponent();
            if (surveyItems != null)
            {
                foreach (CSurveyItem si in surveyItems)
                {
                    SurveyItemDrop.Items.Add(si.Text);
                    SurveyItems.Add(si);
                }
                SurveyItemDrop.SelectedIndexChanged += new EventHandler(SurveyItemDrop_SelectedIndexChanged);
            }
            OKButton.Enabled = false;
            OKButton.Click += new EventHandler(OKButton_Click);
            CancelButton.Click += new EventHandler(CancelButton_Click);
            ResponseBox.ScrollBars = ScrollBars.Vertical;
            ResponseBox.MaxLength = Int32.MaxValue;
            ResponseBox.Enabled = false;
            InstructionsText.ForeColor = Color.Black;
            this.Shown += new EventHandler(UniqueResponseForm_Shown);
            this.FormClosing += new FormClosingEventHandler(UniqueResponseForm_Closing);
        }

        void UniqueResponseForm_Closing(object sender, CancelEventArgs e)
        {
            if (!ResponseBox.Enabled)
            {
                ShutdownSignal.WaitOne();
            }
        }

        void UniqueResponseForm_Shown(object sender, EventArgs e)
        {
            if (Values.Count > 0)
            {
                ThreadStart proc = new ThreadStart(UpdateResponseBox);
                Thread th = new Thread(proc);
                th.Start();
            }
        }



        private void CancelButton_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void OKButton_Click(object sender, EventArgs e)
        {
            int nVals = ParseValues();
            if (nVals == 0)
            {
                if (MessageBox.Show(Properties.Resources.sEmptyUniqueResponseBoxMsg, Properties.Resources.sEmptyUniqueResponseBoxCaption, MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    this.DialogResult = DialogResult.OK;
                    this.Close();
                }
            }
            else
            {
                if (MessageBox.Show(String.Format(Properties.Resources.sUniqueResponseBoxMsg, nVals), Properties.Resources.sUniqueResponseBoxCaption, MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    this.DialogResult = DialogResult.OK;
                    this.Close();
                }
            }
        }

        void SurveyItemDrop_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (SurveyItemDrop.SelectedIndex != -1)
            {
                OKButton.Enabled = true;
                ResponseBox.Enabled = true;
            }
        }

        private int ParseValues()
        {
            Values.Clear();
            if (SurveyItems[SurveyItemDrop.SelectedIndex].Response.ResponseType == CResponse.EResponseType.FixedDig)
            {
                Regex regex = new Regex("[0-9]{" + ((CFixedDigResponse)SurveyItems[SurveyItemDrop.SelectedIndex].Response).NumDigs.ToString() + "}([^0-9]|$)");
                int offset = 0;
                Match m = regex.Match(ResponseBox.Text, offset);
                if (m.Success)
                {
                    Values.Add(m.Value.Substring(0, ((CFixedDigResponse)SurveyItems[SurveyItemDrop.SelectedIndex].Response).NumDigs));
                    while (m.Success)
                    {
                        m = m.NextMatch();
                        if (m.Success)
                            Values.Add(m.Value.Substring(0, ((CFixedDigResponse)SurveyItems[SurveyItemDrop.SelectedIndex].Response).NumDigs));
                    }
                }
            }
            else if (SurveyItems[SurveyItemDrop.SelectedIndex].Response.ResponseType == CResponse.EResponseType.BoundedNum)
            {
                Regex regex = new Regex(@"(([1-9][0-9]*(\.[0-9]+)?)|(\.[0-9]+))([^0-9]|$)");
                foreach (String responseLine in ResponseBox.Lines)
                {
                    MatchCollection mc = regex.Matches(responseLine);
                    foreach (Match m in mc)
                        Values.Add(m.Groups[0].Value);
                }
            }
            else
            {
                Regex regex = new Regex("^\\.+$");
                for (int ctr = 0; ctr < ResponseBox.Lines.Length; ctr++)
                {
                    Match m = regex.Match(ResponseBox.Lines[ctr]);
                    if (!m.Success)
                        Values.Add(ResponseBox.Lines[ctr]);
                }
            }
            return Values.Count;
        }
    }
}

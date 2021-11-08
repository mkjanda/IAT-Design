using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Windows.Forms;
using System.Xml;

namespace IATClient
{
    class CMultipleResponseObject : CResponseObject
    {
        protected bool[] Responses = null;
        private TextBox []ChoiceBoxList = null;
        private CheckBox[] ChoiceCheckList = null;
        private RadioButton []ChoiceRadioList = null;
        private Func<int> GetNumStatements = null;
        private Func<int, String> GetStatement = null;
        /*
        public CMultipleResponseObject DisplayedResponse
        {
            get
            {
                CMultipleResponseObject definedResponse;
                CResponseObject resp = null;
                if (Type == EType.actual)
                {
                    for (int ctr = 0; ctr < ChoiceRadioList.Count; ctr++)
                        if (ChoiceRadioList[ctr].Checked)
                        {
                            definedResponse = new CResponseObject((ctr + 1).ToString(), this);
                            return definedResponse;
                        }
                    definedResponse = new CResponseObject("NULL", this);
                    return definedResponse;
                }
                if ((Type == EType.search) || (Type == EType.correct))
                {
                    resp = new CMultipleResponseObject(this, Type);
                    for (int ctr = 0; ctr < ChoiceCheckList.Count; ctr++)
                        if (ChoiceCheckList[ctr].Checked)
                            resp.Check(ctr + 1);
                    return resp;
                }
                throw new InvalidOperationException();
            }
            set
            {
                if (Type != value.Type)
                    throw new InvalidOperationException();
                if (Type == EType.dummy)
                    return;
                List<String> respList = value.ToStringList();
                if (Type == EType.actual)
                {
                    ChoiceRadioList[Convert.ToInt32(respList[0]) - 1].Checked = true;
                    return;
                }
                for (int ctr = 0; ctr < ChoiceCheckList.Count; ctr++)
                {
                    if (respList.Contains((ctr + 1).ToString()))
                        ChoiceCheckList[ctr].Checked = true;
                    else
                        ChoiceCheckList[ctr].Checked = false;
                }
            }
        }
        */

        public CMultipleResponseObject(EType type, IATSurveyFile.Response theResp)
            : base (type, theResp)
        {
            IATSurveyFile.Multiple resp = (IATSurveyFile.Multiple)theResp;
            GetNumStatements = new Func<int>(resp.GetNumStatements);
            GetStatement = new Func<int, String>(resp.GetStatement);
            UpdateResponseObject();
        }

        public CMultipleResponseObject(EType type, ResultSetDescriptor rsd) : base(type, rsd) { }

        public CMultipleResponseObject(CMultipleResponseObject obj, IATSurveyFile.Multiple resp)
            : base(obj.Type, resp)
        {
            GetNumStatements = new Func<int>(resp.GetNumStatements);
            GetStatement = new Func<int, String>(resp.GetStatement);
            UpdateResponseObject();
            for (int ctr = 0; ctr < GetNumStatements(); ctr++)
                Responses[ctr] = obj.Responses[ctr];
        }

        public CMultipleResponseObject(EType type, CSurveyItem csi)
            : base(type, csi)
        {
            CMultipleResponse resp = (CMultipleResponse)csi.Response;
            GetNumStatements = new Func<int>(resp.GetNumStatements);
            GetStatement = new Func<int, String>(resp.GetStatement);
            UpdateResponseObject();
        }

        public override void UpdateResponseObject()
        {
            int nChoices = GetNumStatements();
            Responses = new bool[nChoices];
            ChoiceBoxList = new TextBox[nChoices];
            ChoiceCheckList = new CheckBox[nChoices];
            ChoiceRadioList = new RadioButton[nChoices];
            for (int ctr = 0; ctr < nChoices; ctr++)
                Responses[ctr] = false;
        }

        public void Check(int n)
        {
            Responses[n - 1] = true;
        }

        public void Uncheck(int n)
        {
            Responses[n - 1] = false;
        }

        public override ISurveyItemResponse Response
        {
            get
            {
                return _Response;
            }
            set
            {
                _Response = value;
                int nChoice = -1;
                if (value.IsAnswered)
                    nChoice = Convert.ToInt32(value.Value);
                for (int ctr = 0; ctr < Responses.Length; ctr++)
                    Responses[ctr] = false;
                if (nChoice >= 0)
                    Responses[nChoice - 1] = true;
                UpdateView();
            }
        }


        private void UpdateView()
        {
            if (ChoiceRadioList != null)
                for (int ctr = 0; ctr < Responses.Length; ctr++)
                    ChoiceRadioList[ctr].Checked = Responses[ctr];
        }

        public override bool IsSearchMatch(String val)
        {
            if (val == "NULL")
            {
                bool bNulled = true;
                foreach (bool b in Responses)
                {
                    if (b == true)
                    {
                        bNulled = false;
                        break;
                    }
                }
                if (bNulled == true)
                    return true;
                return false;
            }
            int nChoice = Convert.ToInt32(val) - 1;
            if (Responses[nChoice])
                return true;
            return false;
        }

        protected override List<CResponseSpecifier> ResponseSpecifiers
        {
            get
            {
                List<CResponseSpecifier> specList = new List<CResponseSpecifier>();
                if ((Type == EType.actual) || (Type == EType.dummy))
                {
                    int ndx = 0;
                    while (ndx < Responses.Length)
                        if (Responses[ndx++])
                            break;
                    specList.Add(new CSingleton(ndx.ToString()));
                }
                else
                {
                    int minNdx = 0, maxNdx = 0;
                    while (minNdx < Responses.Length)
                    {
                        while (maxNdx < Responses.Length)
                        {
                            if (!Responses[maxNdx])
                                break;
                            else
                                maxNdx++;
                        }
                        if (maxNdx == minNdx)
                            specList.Add(new CSingleton((minNdx + 1).ToString()));
                        else
                            specList.Add(new CRange((minNdx + 1).ToString(), (maxNdx + 1).ToString()));
                        minNdx = maxNdx + 1;
                        while (minNdx < Responses.Length)
                        {
                            if (Responses[minNdx])
                                break;
                            else
                                minNdx++;
                        }
                        maxNdx = minNdx;
                    }
                }
                return specList;
            }
        }

        public override void DisposeOfControls()
        {
            for (int ctr = 0; ctr < Responses.Length; ctr++)
            {
                Responses[ctr] = false;
                ChoiceBoxList[ctr].Dispose();
                if ((Type == EType.correct) || (Type == EType.search))
                {
                    ChoiceCheckList[ctr].Dispose();
                    ChoiceCheckList[ctr] = null;
                }
                if ((Type == EType.actual) || (Type == EType.dummy))
                {
                    ChoiceRadioList[ctr].Dispose();
                    ChoiceRadioList[ctr] = null;
                }
            }
        }


        public override Panel GenerateResponseObjectPanel(System.Drawing.Color backColor, System.Drawing.Color foreColor, string fontFamily, float fontSize, int clientWidth)
        {
            Panel PreviewPanel = new Panel();
            if (!bIsNew)
                DisposeOfControls();
            bIsNew = false;
            UpdateResponseObject();
            Point loc = new Point(0, 0);
            Font choiceFont = new Font(fontFamily, fontSize);
            TextBox choiceBox = null;
            RadioButton rb = null;
            CheckBox cb = null;
            int nMaxWidth = 0;
            int nChoices = GetNumStatements();
            for (int ctr = 0; ctr < nChoices; ctr++)
            {
                choiceBox = new TextBox();
                choiceBox.Text = GetStatement(ctr);
                ChoiceBoxList[ctr] = choiceBox;
                choiceBox.BackColor = backColor;
                choiceBox.ForeColor = foreColor;
                choiceBox.ReadOnly = true;
                choiceBox.Multiline = true;
                choiceBox.Font = choiceFont;
                choiceBox.BorderStyle = BorderStyle.None;
                Size szChoice = System.Windows.Forms.TextRenderer.MeasureText(choiceBox.Text, choiceBox.Font,
                    new Size(clientWidth - RadioSize.Width - RadioPadding.Right, 0), TextFormatFlags.WordBreak | TextFormatFlags.TextBoxControl);
                Point choiceLoc, radioLoc;
                if (szChoice.Height > RadioSize.Height)
                {
                    choiceLoc = new Point(loc.X + RadioSize.Width + RadioPadding.Right, loc.Y);
                    radioLoc = new Point(loc.X, loc.Y + ((szChoice.Height - RadioSize.Height) >> 1));
                }
                else
                {
                    radioLoc = new Point(loc.X, loc.Y);
                    choiceLoc = new Point(loc.X + RadioSize.Width + RadioPadding.Right, loc.Y + ((RadioSize.Height - szChoice.Height) >> 1));
                }
                Control selector;
                if ((Type == EType.dummy) || (Type == EType.actual))
                {
                    rb = new RadioButton();
                    ChoiceRadioList[ctr] = rb;
                    selector = rb;
                    if (Type == EType.actual)
                    {
                        rb.Enabled = false;
                    }
                }
                else
                {
                    cb = new CheckBox();
                    ChoiceCheckList[ctr] = cb;
                    cb.CheckedChanged += new EventHandler(ChoiceCheck_Changed);
                    selector = cb;
                }
                selector.Location = radioLoc;
                selector.Size = RadioSize;
                selector.BackColor = backColor;
                selector.ForeColor = foreColor;
                PreviewPanel.Controls.Add(selector);
                choiceBox.Size = szChoice;
                choiceBox.Location = choiceLoc;
                PreviewPanel.Controls.Add(choiceBox);
                loc.Y += (szChoice.Height > RadioSize.Height) ? szChoice.Height : RadioSize.Height;
                loc.Y += RadioPadding.Vertical;
                if (nMaxWidth < szChoice.Width + RadioPadding.Horizontal + RadioSize.Width)
                    nMaxWidth = szChoice.Width + RadioPadding.Horizontal + RadioSize.Width;
            }
            PreviewPanel.Size = new Size(nMaxWidth, loc.Y);
            bIsNew = false;
            return PreviewPanel;
        }

        public void ChoiceCheck_Changed(object sender, EventArgs e)
        {
            CheckBox cb = (CheckBox)sender;
            int ndx = 0;
            while (ChoiceCheckList[ndx] != cb)
                ndx++;
            if (cb.Checked)
                Check(ndx + 1);
            else
                Uncheck(ndx + 1);
        }

    }
}

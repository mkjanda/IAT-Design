using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using System.Xml;

namespace IATClient
{
    class CLikertResponseObject : CResponseObject
    {
        private bool []Responses = null;
        private CheckBox[] ChoiceChecks = null;
        private RadioButton[] ChoiceRadios = null;
        private TextBox[] ChoiceBoxes = null;
        private Func<int, String> GetStatement;
        private Func<bool> GetReverseScored;
        private Func<int> GetNumStatements;

        public CLikertResponseObject(EType type, IATSurveyFile.Response response) : base(type, response)
        {
            IATSurveyFile.Likert resp = (IATSurveyFile.Likert)response;
            GetStatement = new Func<int, String>(resp.GetStatement);
            GetReverseScored = new Func<bool>(resp.IsReverseScored);
            GetNumStatements = new Func<int>(resp.GetNumStatements);
            UpdateResponseObject();
        }

        public CLikertResponseObject(EType type, ResultSetDescriptor rsd) : base(type, rsd) { }

        public CLikertResponseObject(CResponseObject obj, IATSurveyFile.Likert resp) : base(obj.Type, resp)
        {
            CLikertResponseObject lObj = (CLikertResponseObject)obj;
            GetStatement = new Func<int, String>(resp.GetStatement);
            GetReverseScored = new Func<bool>(resp.IsReverseScored);
            GetNumStatements = new Func<int>(resp.GetNumStatements);
            UpdateResponseObject();
            for (int ctr = 0; ctr < Responses.Length; ctr++)
                Responses[ctr] = lObj.Responses[ctr];
        }

        public CLikertResponseObject(EType type, CSurveyItem csi)
            : base(type, csi)
        {
            CLikertResponse resp = (CLikertResponse)csi.Response;
            GetStatement = new Func<int, String>(resp.GetStatement);
            GetNumStatements = new Func<int>(resp.GetNumStatements);
            GetReverseScored = new Func<bool>(resp.IsReverseScored);
            UpdateResponseObject();
        }

        public override void UpdateResponseObject()
        {
            int nStatements = GetNumStatements();
            Responses = new bool[nStatements];
            ChoiceChecks = new CheckBox[nStatements];
            ChoiceRadios = new RadioButton[nStatements];
            ChoiceBoxes = new TextBox[nStatements];
            for (int ctr = 0; ctr < nStatements; ctr++)
                Responses[ctr] = false;
        }

        public override bool IsSearchMatch(String val)
        {
            String str = val;
            int ndx = 0;
            while ((ndx = str.IndexOf(",")) != -1)
            {
                String num = str.Substring(0, ndx - 1).Trim();
                int respVal = Convert.ToInt32(num);
                for (int ctr = 0; ctr < Responses.Length; ctr++)
                    if (Responses[respVal - 1])
                        return true;
            }
            return false;
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
                int nChoice;
                if (value.IsAnswered)
                {
                    nChoice = Convert.ToInt32(value.Value);
                    if (GetReverseScored())
                        nChoice = GetNumStatements() + 1 - nChoice;
                }
                else
                    nChoice = -1;
                for (int ctr = 0; ctr < Responses.Length; ctr++)
                    Responses[ctr] = false;
                if (nChoice != -1)
                    Responses[nChoice - 1] = true;
                for (int ctr = 0; ctr < Responses.Length; ctr++)
                    ChoiceRadios[ctr].Checked = Responses[ctr];
            }
        }

        protected override List<CResponseSpecifier> ResponseSpecifiers
        {
            get
            {
                int minNdx = 0, maxNdx = 0;
                bool bReverseScored = GetReverseScored();
                int nChoices = GetNumStatements();
                List<CResponseSpecifier> specList = new List<CResponseSpecifier>();
                while (minNdx < Responses.Length)
                {
                    while (Responses[maxNdx])
                    {
                        if (++maxNdx >= Responses.Length)
                            break;
                    }
                    if (minNdx == --maxNdx)
                    {
                        if (bReverseScored)
                            specList.Add(new CSingleton((nChoices - minNdx + 1).ToString()));
                        else
                            specList.Add(new CSingleton((minNdx + 1).ToString()));
                    }
                    else 
                    {
                        if (bReverseScored)
                            specList.Add(new CRange((nChoices - minNdx + 1).ToString(), (nChoices - maxNdx + 1).ToString()));
                        else
                            specList.Add(new CRange((minNdx + 1).ToString(), (maxNdx + 1).ToString()));
                    }
                    minNdx = maxNdx + 1;
                    while (!Responses[minNdx])
                        if (++minNdx >= Responses.Length)
                            break;
                    maxNdx = minNdx;
                }
                return specList;
            }
        }

        public override void DisposeOfControls()
        {
            if ((Type == EType.correct) || (Type == EType.search))
            {
                foreach (CheckBox c in ChoiceChecks)
                    c.Dispose();
            }
            if ((Type == EType.actual) || (Type == EType.dummy))
            {
                foreach (RadioButton rb in ChoiceRadios)
                    rb.Dispose();
            }
            foreach (TextBox tb in ChoiceBoxes)
                tb.Dispose();
        }
        /*
        public CLikertResponseObject DisplayedResponse
        {
            get
            {
                CLikertResponseObject resp = new CLikertResponseObject(Type);
                resp.Responses.CopyRange(Responses);
                resp.Choices.CopyRange(Choices);
                resp.bReverseScored = bReverseScored;
                return true;
            }
            set
            {
                this.Responses.Clear();
                this.Choices.Clear();
                this.bReverseScored = value.bReverseScored;
                this.Responses.AddRange(value.Responses);
                this.Choices.AddRange(value.Choices);
                if (ChoiceBoxes.Count == 0)
                    for (int ctr = 0; ctr < value.ChoiceBoxes.Count; ctr++)
                        ChoiceBoxes.Add(new TextBox());
                for (int ctr = 0; ctr < value.ChoiceBoxes.Count; ctr++)
                    ChoicesBoxes[ctr].Text = value.ChoiceBoxes[ctr].Text;
                if ((Type == EType.actual) || (Type == EType.dummy))
                {
                    if (ChoiceRadios.Count == 0)
                        for (int ctr = 0; ctr < value.ChoiceRadios.Count; ctr++)
                            ChoiceRadios.Add(new RadioButton());
                    else if (ChoiceRadios.Count != value.ChoiceRadios.Count)
                        throw new InvalidOperationException();
                    for (int ctr = 0; ctr < Responses.Count; ctr++)
                    {
                        if (Responses[ctr])
                            ChoiceRadios[ctr].Checked = true;
                        else
                            ChoiceRadios[ctr].Checked = false;
                    }
                }
                else if ((Type == EType.correct) || (Type == EType.search))
                {
                    if (ChoiceChecks.Count == 0)
                        for (int ctr = 0; ctr < value.ChoiceChecks.Count; ctr++)
                            ChoiceChecks.Add(new CheckBox());
                    else if (ChoiceChecks.Count != value.ChoiceChecks.Count)
                        throw new InvalidOperationException();
                    for (int ctr = 0; ctr < ChoiceChecks.Count; ctr++)
                    {
                        if (Responses[ctr])
                            ChoiceChecks[ctr].Checked = true;
                        else
                            ChoiceChecks[ctr].Checked = false;
                    }
                }

            }
        }
        */
        private void ChoiceCheck_CheckedChanged(object sender, EventArgs e)
        {
            CheckBox cb = (CheckBox)sender;
            for (int ctr = 0; ctr < ChoiceChecks.Length; ctr++)
                if (ChoiceChecks[ctr] == cb)
                    Responses[ctr] = cb.Checked;
        }

        public override Panel GenerateResponseObjectPanel(System.Drawing.Color backColor, System.Drawing.Color foreColor, string fontFamily, float fontSize, int clientWidth)
        {
            if (!bIsNew)
                DisposeOfControls();
            bIsNew = false;
            UpdateResponseObject();
            Panel PreviewPanel = new Panel();
            Point loc = new Point(0, 0);
            Font choiceFont = new Font(fontFamily, fontSize);
            TextBox choiceBox = null;
            RadioButton rb = null;
            CheckBox cb = null;
            Control con = null;
            int nChoices = GetNumStatements();
            for (int ctr = 0; ctr < nChoices; ctr++)
            {
                choiceBox = new TextBox();
                ChoiceBoxes[ctr] = choiceBox;
                choiceBox.BackColor = backColor;
                choiceBox.ForeColor = foreColor;
                choiceBox.ReadOnly = true;
                choiceBox.Font = choiceFont;
                choiceBox.Multiline = true;
                choiceBox.BorderStyle = BorderStyle.None;
                Size szChoice = System.Windows.Forms.TextRenderer.MeasureText(GetStatement(ctr), choiceFont,
                    new Size(clientWidth - RadioSize.Width - RadioPadding.Right, 0), TextFormatFlags.WordBreak | TextFormatFlags.TextBoxControl);
                Point choiceLoc, radioLoc;
                if ((Type == EType.actual) || (Type == EType.dummy))
                {
                    rb = new RadioButton();
                    if (Type == EType.actual)
                        rb.Enabled = false;
                    rb.Size = RadioSize;
                    con = rb;
                    if (ctr < ChoiceRadios.Length)
                        ChoiceRadios[ctr] = rb;
                }
                else if ((Type == EType.correct) || (Type == EType.search))
                {
                    cb = new CheckBox();
                    cb.CheckedChanged += new EventHandler(ChoiceCheck_CheckedChanged);
                    cb.Size = CheckSize;
                    con = cb;
                    if (ctr < ChoiceChecks.Length)
                        ChoiceChecks[ctr] = cb;
                }
                if (szChoice.Height > con.Height)
                {
                    choiceLoc = new Point(con.Width + RadioPadding.Right, loc.Y);
                    radioLoc = new Point(0, loc.Y + ((szChoice.Height - RadioSize.Height) >> 1));
                }
                else
                {
                    radioLoc = new Point(loc.X, loc.Y);
                    choiceLoc = new Point(loc.X + RadioSize.Width + RadioPadding.Right, loc.Y + ((RadioSize.Height - szChoice.Height) >> 1));
                }
                con.BackColor = backColor;
                con.ForeColor = foreColor;
                con.Location = radioLoc;
                if (!PreviewPanel.Controls.Contains(con))
                    PreviewPanel.Controls.Add(con);
                choiceBox.Size = szChoice;
                choiceBox.Location = choiceLoc;
                choiceBox.Text = GetStatement(ctr);
                if (!PreviewPanel.Contains(choiceBox))
                    PreviewPanel.Controls.Add(choiceBox);
                loc.Y += (szChoice.Height > RadioSize.Height) ? szChoice.Height : RadioSize.Height;
                loc.Y += ElementPadding.Vertical;
            }
            PreviewPanel.Height = loc.Y;
            return PreviewPanel;
        }
    }
}


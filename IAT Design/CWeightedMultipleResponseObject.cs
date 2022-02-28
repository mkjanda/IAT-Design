using IATClient.ResultData;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace IATClient
{
    class CWeightedMultipleResponseObject : CResponseObject
    {
        private CheckBox[] AnswerChecks = null;
        private RadioButton[] AnswerRadios = null;
        private TextBox[] AnswerBoxes = null;
        private bool[] Responses = null;
        private Func<int> GetNumChoices = null;
        private Func<int, String> GetChoice = null;
        private Func<int, int> GetWeight = null;
        /*
        public CWeightedMultipleResponseObject DisplayedResponse
        {
            get
            {
                CWeightedMultipleResponseObject resp = new CWeightedMultipleResponseObject();

                if ((Type == EType.actual) || (Type == EType.dummy))
                    for (int ctr = 0; ctr < AnswerRadios.Count; ctr++)
                        resp.AnswerRadios.Add(new RadioButton());
                else if ((Type == EType.correct) || (Type == EType.search))
                    for (int ctr = 0; ctr < AnswerChecks.Count; ctr++)
                        resp.AnswerChecks.Add(new CheckBox());
                for (int ctr = 0; ctr < AnswerBoxes.Count; ctr++)
                    resp.AnswerBoxes.Add(new TextBox());
                resp.Responses.AddRange(Responses);
                resp.AnswerBoxes.AddRange(AnswerBoxes);
                resp.AnswerChecks.AddRange(AnswerChecks);
                resp.ResponseWeights = new int[resp.Responses.Count];
                System.Array.Copy(ResponseWeights, resp.ResponseWeights, Responses.Count);
                return resp;
            }
            set
            {
                CWeightedMultipleResponseObject resp = (CWeightedMultipleResponseObject)value;
                if ((AnswerChecks.Count == 0) && ((Type == EType.actual) || (Type == EType.dummy)))
                    for (int ctr = 0; ctr < resp.AnswerChecked; ctr++)
                        AnswerChecks.Add(new CheckBox());
                if ((AnswerRadios.Count == 0) && ((Type == EType.actual) || (Type == EType.dummy)))
                    for (int ctr = 0; ctr < resp.AnswerRadios.Count; ctr++)
                        AnswerRadios.Add(new AnswerRadio());
                for (int ctr = 0; ctr < resp.AnswerBoxes.Count; ctr++)
                    AnswerBoxes.Add(new TextBox());
                if ((Type == EType.dummy) || (Type == EType.correct))
                {
                    for (int ctr = 0; ctr < AnswerChecks.Count; ctr++)
                    {
                        AnswerChecks[ctr].Checked = resp.AnswerChecks[ctr].Checked;
                        AnswerBoxes[ctr].Text = resp.AnswerChecks[ctr].Checked;
                    }
                }
                else if ((Type == EType.correct) || (Type == EType.search))
                    for (int ctr = 0; ctr < AnswerRadios.Count; ctr++)
                    {
                        AnswerRadios[ctr].Checked = resp.AnswerRadios[ctr].Checked;
                        AnswerBoxes[ctr].Text = resp.AnswerBoxes[ctr].Text;
                    }
                Responses.Clear();
                Responses.CopyRange(resp.Responses);
                ResponseWeights = new int[ResponseWeights];
                System.Array.Copy(ResponseWeights, resp.ResponseWeights, ResponseWeights.Count);
                return resp;
            }
        }
        */
        private void ChoiceRadio_CheckedChanged(object sender, EventArgs e)
        {
            RadioButton rb = (RadioButton)sender;
            int ndx = 0;
            while (AnswerRadios[ndx] != (RadioButton)sender)
                ndx++;
            this[ndx] = rb.Checked;
        }

        private void ChoiceCheck_CheckedChanged(object sender, EventArgs e)
        {
            CheckBox cb = (CheckBox)sender;
            int ndx = 0;
            while (AnswerChecks[ndx] != (CheckBox)sender)
                ndx++;
            this[ndx] = cb.Checked;
        }

        public override void DisposeOfControls()
        {
            if ((Type == EType.actual) || (Type == EType.dummy))
                for (int ctr = 0; ctr < AnswerRadios.Length; ctr++)
                    AnswerRadios[ctr].Dispose();
            else if ((Type == EType.search) || (Type == EType.correct))
                for (int ctr = 0; ctr < AnswerChecks.Length; ctr++)
                    AnswerChecks[ctr].Dispose();
            for (int ctr = 0; ctr < AnswerBoxes.Length; ctr++)
                AnswerBoxes[ctr].Dispose();
        }

        public override Panel GenerateResponseObjectPanel(System.Drawing.Color backColor, System.Drawing.Color foreColor, string fontFamily, float fontSize, int clientWidth)
        {
            if (!bIsNew)
                DisposeOfControls();
            UpdateResponseObject();
            Panel PreviewPanel = new Panel();
            Font choiceFont = new Font(fontFamily, fontSize);
            Point loc = new Point();
            TextBox choiceBox = null;
            RadioButton rb = null;
            CheckBox cb = null;
            Control con = null;
            int maxWidth = 0;
            int nChoices = GetNumChoices();
            for (int ctr = 0; ctr < nChoices; ctr++)
            {
                choiceBox = new TextBox();
                choiceBox.BackColor = backColor;
                choiceBox.ForeColor = foreColor;
                choiceBox.ReadOnly = true;
                choiceBox.Multiline = true;
                choiceBox.Text = GetChoice(ctr);
                choiceBox.Font = choiceFont;
                choiceBox.BorderStyle = BorderStyle.None;
                Size szChoice = System.Windows.Forms.TextRenderer.MeasureText(choiceBox.Text, choiceFont,
                    new Size(clientWidth - RadioSize.Width - RadioPadding.Right, 0), TextFormatFlags.WordBreak | TextFormatFlags.TextBoxControl);
                Point choiceLoc, radioLoc;
                choiceBox.Size = szChoice;
                AnswerBoxes[ctr] = choiceBox;
                if ((Type == EType.actual) || (Type == EType.dummy))
                {
                    rb = new RadioButton();
                    rb.CheckedChanged += new EventHandler(ChoiceRadio_CheckedChanged);
                    rb.Size = RadioSize;
                    AnswerRadios[ctr] = rb;
                    con = rb;
                    if (Type == EType.actual)
                        rb.Enabled = false;
                }
                else if ((Type == EType.correct) || (Type == EType.search))
                {
                    cb = new CheckBox();
                    cb.Size = CheckSize;
                    cb.CheckedChanged += new EventHandler(ChoiceCheck_CheckedChanged);
                    AnswerChecks[ctr] = cb;
                    con = cb;
                }
                else
                    throw new InvalidOperationException();
                if (szChoice.Height > RadioSize.Height)
                {
                    choiceLoc = new Point(con.Width + RadioPadding.Right, loc.Y);
                    radioLoc = new Point(0, loc.Y + ((szChoice.Height - con.Height) >> 1));
                }
                else
                {
                    radioLoc = new Point(0, loc.Y);
                    choiceLoc = new Point(con.Width + RadioPadding.Right, loc.Y + ((RadioSize.Height - szChoice.Height) >> 1));
                }
                choiceBox.Location = choiceLoc;
                con.Location = radioLoc;
                con.Size = RadioSize;
                con.BackColor = backColor;
                con.ForeColor = foreColor;
                PreviewPanel.Controls.Add(con);
                PreviewPanel.Controls.Add(choiceBox);
                if ((con.Size.Width + RadioPadding.Right + szChoice.Width) > maxWidth)
                    maxWidth = con.Size.Width + RadioPadding.Right + szChoice.Width;
                loc.Y += (szChoice.Height > RadioSize.Height) ? szChoice.Height : RadioSize.Height;
                loc.Y += RadioPadding.Vertical;
            }
            PreviewPanel.Size = new Size(maxWidth, loc.Y);
            bIsNew = false;
            return PreviewPanel;
        }

        public CWeightedMultipleResponseObject(EType type, Response resp) : base(type, resp)
        {
            WeightedMultiple wResp = (WeightedMultiple)resp;
            GetNumChoices = new Func<int>(wResp.GetNumStatements);
            GetChoice = new Func<int, String>(wResp.GetChoice);
            GetWeight = new Func<int, int>(wResp.GetChoiceWeight);
            UpdateResponseObject();
        }

        public CWeightedMultipleResponseObject(EType type, ResultSetDescriptor rsd) : base(type, rsd) { }

        public CWeightedMultipleResponseObject(EType type, CSurveyItem csi)
            : base(type, csi)
        {
            CWeightedMultipleResponse resp = (CWeightedMultipleResponse)csi.Response;
            GetNumChoices = new Func<int>(resp.GetNumStatements);
            GetChoice = new Func<int, String>(resp.GetChoice);
            GetWeight = new Func<int, int>(resp.GetChoiceWeight);
            UpdateResponseObject();
        }

        public CWeightedMultipleResponseObject(CWeightedMultipleResponseObject obj, WeightedMultiple resp)
            : base(obj.Type, resp)
        {
            GetNumChoices = new Func<int>(resp.GetNumStatements);
            GetChoice = new Func<int, String>(resp.GetChoice);
            GetWeight = new Func<int, int>(resp.GetChoiceWeight);
            UpdateResponseObject();
            for (int ctr = 0; ctr < Responses.Length; ctr++)
                Responses[ctr] = obj.Responses[ctr];
        }

        public override void UpdateResponseObject()
        {
            int nChoices = GetNumChoices();
            Responses = new bool[nChoices];
            for (int ctr = 0; ctr < nChoices; ctr++)
                Responses[ctr] = false;
            AnswerBoxes = new TextBox[nChoices];
            if ((Type == EType.actual) || (Type == EType.dummy))
                AnswerRadios = new RadioButton[nChoices];
            else
                AnswerChecks = new CheckBox[nChoices];
        }

        public bool this[int n]
        {
            get
            {
                return Responses[n];
            }
            set
            {
                Responses[n] = value;
            }
        }

        public override bool IsSearchMatch(String val)
        {
            int weight = Convert.ToInt32(val);
            for (int ctr = 0; ctr < Responses.Length; ctr++)
            {
                if ((GetWeight(ctr) == weight) && (Responses[ctr]))
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
                int nResp = -1;
                if (value.IsAnswered)
                {
                    for (int ctr = 0; ctr < Responses.Length; ctr++)
                        if (GetWeight(ctr) == Convert.ToInt32(value.Value))
                        {
                            nResp = ctr;
                            break;
                        }
                }
                for (int ctr = 0; ctr < Responses.Length; ctr++)
                {
                    AnswerRadios[ctr].Checked = false;
                    Responses[ctr] = false;
                }
                if (nResp >= 0)
                {
                    AnswerRadios[nResp].Checked = true;
                    Responses[nResp] = true;
                }
            }
        }

        protected override List<CResponseObject.CResponseSpecifier> ResponseSpecifiers
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
    }
}

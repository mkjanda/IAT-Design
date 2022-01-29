using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using IATClient.ResultData;

namespace IATClient
{
    class CMultiBooleanResponseObject : CResponseObject
    {
        private Panel AnswerSpecPanel = null;
        private Panel[] ChoicePanels = null;
        private RadioButton[] AnswerSpecTypeRadios = new RadioButton[6];
        private EAnswerState ActiveAnswerSpec = EAnswerState.Selected;
        private CheckBox[] ChoiceChecks = null;
        private Panel[] ChoiceStatePanels = null;
        private TrackBar AnswerSpecTrack = null;
        private Label TrackLabel = null;
        public enum EAnswerState : int { Unselected, Selected, SomeSelected, Irrelavent, SomeUnselected, Undefined };
        private int _NumSomeSelected = 0, _NumSomeUnselected = 0;
        public EAnswerState[] AnswerStates = null;
        private Func<int> GetNumStatements = null;
        private Func<int, String> GetStatement = null;


        public override void DisposeOfControls()
        {
            if (AnswerSpecPanel != null)
            {
                foreach (Control c in AnswerSpecPanel.Controls)
                    c.Dispose();
                AnswerSpecPanel.Dispose();
                AnswerSpecPanel = null;
                AnswerSpecTypeRadios = null;
                AnswerSpecTrack = null;
                TrackLabel = null;
            }
            if (ChoicePanels == null)
            {
                for (int ctr = 0; ctr < ChoicePanels.Length; ctr++)
                {
                    foreach (Control c in ChoicePanels[ctr].Controls)
                        c.Dispose();
                }
            }
        }

        public override Panel GenerateResponseObjectPanel(System.Drawing.Color backColor, System.Drawing.Color foreColor, string fontFamily, float fontSize, int clientWidth)
        {
            if (!bIsNew)
                DisposeOfControls();
            bIsNew = false;
            Panel PreviewPanel = new Panel();
            UpdateResponseObject();
            Point loc, checkLoc, choiceLoc;
            Size choiceSize1, choiceSize2;
            Size choicePanelSize = new Size((int)(clientWidth * .4), 0);
            Font choiceFont = new Font(fontFamily, fontSize);
            CheckBox cb;
            TextBox choiceBox = null;
            String[] LabelList = new String[GetNumStatements()];
            for (int ctr = 0; ctr < LabelList.Length; ctr++)
                LabelList[ctr] = GetStatement(ctr);

            loc = new Point(0, 0);
            Panel choicePanel = null;
            int nFirstCol = (int)Math.Ceiling((float)GetNumStatements() / 2F);
            for (int ctr = 0; ctr < (int)Math.Ceiling(((float)GetNumStatements() / 2F)); ctr++)
            {
                // calc panel size
                choiceSize1 = TextRenderer.MeasureText(LabelList[ctr], choiceFont, new Size((int)((clientWidth * .4)) - CheckSize.Width - CheckPadding.Horizontal, 0),
                    TextFormatFlags.TextBoxControl | TextFormatFlags.WordBreak);
                if (ctr + Math.Ceiling((float)LabelList.Length / 2.0) < LabelList.Length)
                    choiceSize2 = TextRenderer.MeasureText(LabelList[ctr + (int)(Math.Ceiling((float)LabelList.Length / 2.0))], choiceFont, new Size((int)((clientWidth * .4)) - CheckSize.Width - CheckPadding.Right, 0),
                        TextFormatFlags.TextBoxControl | TextFormatFlags.WordBreak);
                else
                    choiceSize2 = Size.Empty;
                if ((choiceSize2 != Size.Empty) && (choiceSize2.Height > choiceSize1.Height))
                    choicePanelSize.Height = choiceSize2.Height;
                else
                    choicePanelSize.Height = choiceSize1.Height;
                if ((choiceSize2 != Size.Empty) && (choiceSize2.Width > choiceSize1.Width))
                    choicePanelSize.Width = choiceSize2.Width + RadioSize.Width + RadioPadding.Right;
                else
                    choicePanelSize.Width = choiceSize1.Width + RadioSize.Width + RadioPadding.Right;
                // place left-hand choice
                choicePanel = new Panel();
                choicePanel.Size = choicePanelSize;
                choicePanel.BackColor = backColor;
                Control con = null;
                if ((Type == EType.search) || (Type == EType.correct))
                {
                    Panel p = new Panel();
                    p.Size = CheckSize;
                    p.BorderStyle = BorderStyle.Fixed3D;
                    if ((Type == EType.correct) || (Type == EType.search))
                    {
                        switch (this[ctr])
                        {
                            case EAnswerState.Unselected:
                                p.BackColor = System.Drawing.Color.White;
                                break;

                            case EAnswerState.Selected:
                                p.BackColor = System.Drawing.Color.Black;
                                break;

                            case EAnswerState.SomeSelected:
                                p.BackColor = System.Drawing.Color.DarkGray;
                                break;

                            case EAnswerState.Irrelavent:
                                p.BackColor = System.Drawing.Color.Gray;
                                break;

                            case EAnswerState.SomeUnselected:
                                p.BackColor = System.Drawing.Color.LightGray;
                                break;

                            case EAnswerState.Undefined:
                                p.BackColor = System.Drawing.Color.Gray;
                                break;
                        }
                    }
                    p.Tag = ctr.ToString();
                    p.Click += new EventHandler(ChoiceBox_Click);
                    ChoiceStatePanels[ctr] = p;
                    con = p;
                }
                else
                {
                    cb = new CheckBox();
                    if (AnswerStates[ctr] == EAnswerState.Selected)
                        cb.Checked = true;
                    else if (AnswerStates[ctr] == EAnswerState.Unselected)
                        cb.Checked = false;
                    if (Type == EType.actual)
                        cb.Enabled = false;
                    ChoiceChecks[ctr] = cb;
                    con = cb;
                }
                choicePanel.Location = loc;
                if (choiceSize1.Height > CheckSize.Height)
                {
                    choiceLoc = new Point(CheckSize.Width + CheckPadding.Right, (choicePanelSize.Height - choiceSize1.Height) >> 1);
                    checkLoc = new Point(0, choiceLoc.Y + ((choiceSize1.Height - CheckSize.Height) >> 1));
                }
                else
                {
                    checkLoc = new Point(0, (choicePanelSize.Height - CheckSize.Height) >> 1);
                    choiceLoc = new Point(CheckSize.Width + CheckPadding.Right, checkLoc.Y + ((CheckSize.Height - choiceSize1.Height) >> 1));
                }
                con.Location = checkLoc;
                con.BackColor = backColor;
                con.ForeColor = foreColor;
                con.Size = CheckSize;
                choicePanel.Controls.Add(con);
                choiceBox = new TextBox();
                choiceBox.BorderStyle = BorderStyle.None;
                choiceBox.Multiline = true;
                choiceBox.ReadOnly = true;
                choiceBox.Location = choiceLoc;
                choiceBox.Size = choiceSize1;
                choiceBox.BackColor = backColor;
                choiceBox.ForeColor = foreColor;
                choiceBox.Font = choiceFont;
                choiceBox.Text = LabelList[ctr];
                choicePanel.Controls.Add(choiceBox);
                ChoicePanels[ctr] = choicePanel;
                PreviewPanel.Controls.Add(choicePanel);


                // place right-hand choice
                if (ctr + Math.Ceiling((float)GetNumStatements() / 2.0) < GetNumStatements())
                {
                    choicePanel = new Panel();
                    choicePanel.BackColor = backColor;
                    if ((Type == EType.search) || (Type == EType.correct))
                    {
                        Panel p = new Panel();
                        p.Size = CheckSize;
                        p.BorderStyle = BorderStyle.Fixed3D;
                        if (Type == EType.correct)
                        {
                            switch (this[ctr + nFirstCol])
                            {
                                case EAnswerState.Selected:
                                    p.BackColor = System.Drawing.Color.Black;
                                    break;

                                case EAnswerState.SomeSelected:
                                    p.BackColor = System.Drawing.Color.DarkGray;
                                    break;

                                case EAnswerState.Irrelavent:
                                    p.BackColor = System.Drawing.Color.Gray;
                                    break;

                                case EAnswerState.SomeUnselected:
                                    p.BackColor = System.Drawing.Color.LightGray;
                                    break;

                                case EAnswerState.Unselected:
                                    p.BackColor = System.Drawing.Color.White;
                                    break;

                                case EAnswerState.Undefined:
                                    p.BackColor = System.Drawing.Color.Gray;
                                    break;
                            }
                        }
                        p.Tag = ctr.ToString();
                        p.Click += new EventHandler(ChoiceBox_Click);
                        ChoiceStatePanels[ctr + nFirstCol] = p;
                        con = p;
                    }
                    else
                    {
                        cb = new CheckBox();
                        if (AnswerStates[ctr + nFirstCol] == EAnswerState.Selected)
                            cb.Checked = true;
                        if (AnswerStates[ctr + nFirstCol] == EAnswerState.Unselected)
                            cb.Checked = false;
                        if (Type == EType.actual)
                            cb.Enabled = false;
                        ChoiceChecks[ctr + nFirstCol] = cb;
                        con = cb;
                    }
                    choicePanel.Size = choicePanelSize;
                    choicePanel.Location = loc + new Size(clientWidth >> 1, 0);
                    if (choiceSize2.Height > CheckSize.Height)
                    {
                        choiceLoc = new Point(CheckSize.Width + CheckPadding.Right, (choicePanelSize.Height - choiceSize2.Height) >> 1);
                        checkLoc = new Point(0, choiceLoc.Y + ((choiceSize2.Height - CheckSize.Height) >> 1));
                    }
                    else
                    {
                        checkLoc = new Point(0, (choicePanelSize.Height - CheckSize.Height) >> 1);
                        choiceLoc = new Point(CheckSize.Width + CheckPadding.Right, checkLoc.Y + ((CheckSize.Height - choiceSize2.Height) >> 1));
                    }
                    con.Location = checkLoc;
                    con.BackColor = backColor;
                    con.ForeColor = foreColor;
                    con.Size = CheckSize;
                    choicePanel.Controls.Add(con);
                    choiceBox = new TextBox();
                    choiceBox.BorderStyle = BorderStyle.None;
                    choiceBox.Multiline = true;
                    choiceBox.ReadOnly = true;
                    choiceBox.Location = choiceLoc;
                    choiceBox.Size = choiceSize2;
                    choiceBox.BackColor = backColor;
                    choiceBox.ForeColor = foreColor;
                    choiceBox.Font = choiceFont;
                    choiceBox.Text = LabelList[ctr + nFirstCol];
                    choicePanel.Controls.Add(choiceBox);
                    ChoicePanels[ctr + nFirstCol] = choicePanel;
                    PreviewPanel.Controls.Add(choicePanel);
                }
                loc.Y += choicePanelSize.Height + ElementPadding.Vertical;
            }

            if ((Type == EType.correct) || (Type == EType.search))
            {
                AnswerSpecPanel = new Panel();
                string[] radioLabels = { "Selected", "Some Selected", "Irrelavent", "Some Unselected", "Unselected" };
                Size maxSzLabel = new Size(0, 0);
                for (int ctr = 0; ctr < 5; ctr++)
                {
                    Size szLabel = TextRenderer.MeasureText(radioLabels[ctr], System.Drawing.SystemFonts.DialogFont);
                    if (szLabel.Width > maxSzLabel.Width)
                        maxSzLabel.Width = szLabel.Width;
                    if (szLabel.Height > maxSzLabel.Height)
                        maxSzLabel.Height = szLabel.Height;
                }
                maxSzLabel.Width += 10;
                maxSzLabel.Height += 8;
                int xOffset = 0;
                for (int ctr = 0; ctr < 5; ctr++)
                {
                    RadioButton rb = new RadioButton();
                    rb.Appearance = Appearance.Button;
                    if (ctr == 0)
                        rb.Checked = true;
                    else
                        rb.Checked = false;
                    rb.Size = maxSzLabel;
                    rb.Location = new Point(xOffset, 0);
                    xOffset += rb.Width;
                    AnswerSpecPanel.Controls.Add(rb);
                    rb.Click += new EventHandler(AnswerSpecRadio_Click);
                }
                AnswerSpecTrack = new TrackBar();
                AnswerSpecTrack.Size = new Size(xOffset, 26);
                AnswerSpecTrack.Minimum = 0;
                AnswerSpecTrack.Maximum = 0;
                AnswerSpecTrack.ValueChanged += new EventHandler(AnswerSpecTrack_ValueChanged);
                AnswerSpecTrack.Visible = false;
                AnswerSpecTrack.TickFrequency = 1;
                AnswerSpecTrack.Location = new Point(0, maxSzLabel.Height + 10);
                TrackLabel = new Label();
                TrackLabel.Visible = false;
                TrackLabel.Location = new Point(xOffset >> 1, AnswerSpecTrack.Bottom + 6);
                TrackLabel.Size = new Size(0, System.Drawing.SystemFonts.DialogFont.Height);
                AnswerSpecPanel.Size = new Size(xOffset, TrackLabel.Height);
                AnswerSpecPanel.Location = new Point((clientWidth - AnswerSpecPanel.Width) >> 1, choicePanel.Bottom);
                AnswerSpecPanel.Controls.Add(AnswerSpecTrack);
                AnswerSpecPanel.Controls.Add(TrackLabel);
                AnswerSpecPanel.BackColor = System.Drawing.Color.White;
                AnswerSpecPanel.BorderStyle = BorderStyle.None;
                PreviewPanel.Controls.Add(AnswerSpecPanel);
                PreviewPanel.Size = new Size(clientWidth, AnswerSpecPanel.Bottom);
            }
            else
            {
                if (ChoicePanels.Length % 2 == 0)
                    PreviewPanel.Size = new Size(clientWidth, ChoicePanels.Last().Bottom);
                else
                    PreviewPanel.Size = new Size(clientWidth, ChoicePanels[ChoicePanels.Length >> 1].Bottom);
            }
            bIsNew = false;
            return PreviewPanel;
        }

        private void ChoiceBox_Click(object sender, EventArgs e)
        {
            Panel p = (Panel)sender;
            int choiceNdx = 0;
            for (int ctr = 0; ctr < ChoiceStatePanels.Length; ctr++)
                if (p == ChoiceStatePanels[ctr])
                {
                    choiceNdx = ctr;
                    break;
                }
            
            if (this[choiceNdx] == ActiveAnswerSpec)
                return;
            this[choiceNdx] = ActiveAnswerSpec;
            switch (ActiveAnswerSpec)
            {
                case EAnswerState.Selected:
                    p.BackColor = System.Drawing.Color.Black;
                    break;

                case EAnswerState.Unselected:
                    AnswerSpecTrack.Value++;
                    p.BackColor = System.Drawing.Color.White;
                    break;

                case EAnswerState.Irrelavent:
                    p.BackColor = System.Drawing.Color.Gray;
                    break;

                case EAnswerState.SomeSelected:
                    AnswerSpecTrack.Value++;
                    p.BackColor = System.Drawing.Color.DarkGray;
                    break;

                case EAnswerState.SomeUnselected:
                    p.BackColor = System.Drawing.Color.LightGray;
                    break;
            }
        }

        private void AnswerSpecRadio_Click(object sender, EventArgs e)
        {
            RadioButton rb = (RadioButton)sender;
            int choiceNdx = 0;
            for (int ctr = 0; ctr < AnswerSpecTypeRadios.Length; ctr++)
                if (rb == AnswerSpecTypeRadios[ctr])
                {
                    choiceNdx = ctr;
                    break;
                }

            ActiveAnswerSpec = (EAnswerState)Enum.GetValues(typeof(EAnswerState)).GetValue(choiceNdx);
            if (ActiveAnswerSpec == EAnswerState.SomeSelected)
            {
                AnswerSpecPanel.SuspendLayout();
                AnswerSpecTrack.Minimum = 0;
                AnswerSpecTrack.Maximum = AnswersOfType(EAnswerState.SomeSelected);
                AnswerSpecTrack.TickFrequency = 1;
                TrackLabel.Text = String.Format("Must select at least {0} of the denoted choices.", NumSomeSelected);
                TrackLabel.Size = TextRenderer.MeasureText(TrackLabel.Text, System.Drawing.SystemFonts.DialogFont);
                TrackLabel.Location = new Point((AnswerSpecPanel.Width - TrackLabel.Size.Width) >> 1, TrackLabel.Top);
                AnswerSpecTrack.Visible = true;
                TrackLabel.Visible = true;
                AnswerSpecPanel.ResumeLayout(false);
            }
            else if (ActiveAnswerSpec == EAnswerState.SomeUnselected)
            {
                AnswerSpecPanel.SuspendLayout();
                AnswerSpecTrack.Minimum = 0;
                AnswerSpecTrack.Maximum = AnswersOfType(EAnswerState.SomeUnselected);
                AnswerSpecTrack.TickFrequency = 1;
                TrackLabel.Text = String.Format("Can select at most {0} of the denoted choices.",
                    AnswersOfType(EAnswerState.SomeUnselected) - NumSomeSelected);
                TrackLabel.Size = TextRenderer.MeasureText(TrackLabel.Text, System.Drawing.SystemFonts.DialogFont);
                TrackLabel.Location = new Point((AnswerSpecPanel.Width - TrackLabel.Size.Width) >> 1, TrackLabel.Top);
                AnswerSpecTrack.Visible = true;
                TrackLabel.Visible = true;
                AnswerSpecPanel.ResumeLayout(false);
            }
            else
            {
                AnswerSpecTrack.Visible = false;
                TrackLabel.Visible = false;
            }
        }

        private void AnswerSpecTrack_ValueChanged(object sender, EventArgs e)
        {
            AnswerSpecPanel.SuspendLayout();
            if (ActiveAnswerSpec == EAnswerState.SomeSelected)
            {
                NumSomeSelected = AnswerSpecTrack.Value;
                TrackLabel.Text = String.Format("Must select at least {0} of the denoted choices.", NumSomeSelected);
                TrackLabel.Size = TextRenderer.MeasureText(TrackLabel.Text, System.Drawing.SystemFonts.DialogFont);
                TrackLabel.Location = new Point((AnswerSpecPanel.Width - TrackLabel.Size.Width) >> 1, TrackLabel.Top);
            }
            else if (ActiveAnswerSpec == EAnswerState.SomeUnselected)
            {
                NumSomeUnselected = AnswerSpecTrack.Value;
                TrackLabel.Text = String.Format("Can select at most {0} of the denoted choices.",
                    AnswersOfType(EAnswerState.SomeUnselected) - NumSomeSelected);
                TrackLabel.Size = TextRenderer.MeasureText(TrackLabel.Text, System.Drawing.SystemFonts.DialogFont);
                TrackLabel.Location = new Point((AnswerSpecPanel.Width - TrackLabel.Size.Width) >> 1, TrackLabel.Top);
            }
            AnswerSpecPanel.ResumeLayout(false);
        }

        public int NumSomeSelected
        {
            get
            {
                return _NumSomeSelected;
            }
            set
            {
                _NumSomeSelected = value;
            }
        }

        public int NumSomeUnselected
        {
            get
            {
                return _NumSomeUnselected;
            }
            set
            {
                _NumSomeUnselected = value;
            }
        }

        public CMultiBooleanResponseObject(EType type, Response theResp)
            : base(type, theResp)
        {
            MultiBoolean resp = (MultiBoolean)theResp;
            GetNumStatements = new Func<int>(resp.GetNumStatements);
            GetStatement = new Func<int, String>(resp.GetStatement);
            UpdateResponseObject();
        }

        public CMultiBooleanResponseObject(EType type, ResultSetDescriptor rsd) : base(type, rsd) { }

        public CMultiBooleanResponseObject(EType type, CSurveyItem csi)
            : base(type, csi)
        {
            CMultiBooleanResponse resp = (CMultiBooleanResponse)csi.Response;
            GetNumStatements = new Func<int>(resp.GetNumStatements);
            GetStatement = new Func<int, String>(resp.GetStatement);
            UpdateResponseObject();
        }

        public CMultiBooleanResponseObject(CMultiBooleanResponseObject obj, MultiBoolean resp)
            : base(obj.Type, resp)
        {
            GetNumStatements = new Func<int>(resp.GetNumStatements);
            GetStatement = new Func<int, String>(resp.GetStatement);
            UpdateResponseObject();
            _NumSomeSelected = obj._NumSomeSelected;
            _NumSomeUnselected = obj._NumSomeUnselected;
            for (int ctr = 0; ctr < AnswerStates.Length; ctr++)
                AnswerStates[ctr] = obj.AnswerStates[ctr];
        }

        public override void UpdateResponseObject()
        {
            int nStatements = GetNumStatements();
            ChoicePanels = new Panel[nStatements];
            AnswerStates = new EAnswerState[nStatements];
            for (int ctr = 0; ctr < nStatements; ctr++)
                AnswerStates[ctr] = EAnswerState.Unselected;
            if ((Type == EType.dummy) || (Type == EType.actual))
                ChoiceChecks = new CheckBox[nStatements];
            else if ((Type == EType.correct) || (Type == EType.search))
                ChoiceStatePanels = new Panel[nStatements];
        }


        public int AnswersOfType(EAnswerState state)
        {
            int nAnswers = 0;
            for (int ctr = 0; ctr < AnswerStates.Length; ctr++)
                if (AnswerStates[ctr] == state)
                    nAnswers++;
            return nAnswers;
        }

        protected override List<CResponseObject.CResponseSpecifier> ResponseSpecifiers
        {
            get
            {
                List<CResponseSpecifier> specList = new List<CResponseSpecifier>();
                if ((Type == EType.dummy) || (Type == EType.actual))
                {
                    int ndx = 0;
                    int startNdx = 0;
                    int endNdx = 0;
                    while (ndx < GetNumStatements())
                    {
                        while (ChoiceChecks[ndx++].Checked)
                        {
                            endNdx++;
                            if (ndx >= GetNumStatements())
                                break;
                        }
                        if (endNdx > startNdx)
                            specList.Add(new CRange(startNdx.ToString(), endNdx.ToString()));
                        else
                            specList.Add(new CSingleton(startNdx.ToString()));
                        if (ndx >= GetNumStatements())
                        {
                            while (!ChoiceChecks[ndx].Checked)
                            {
                                startNdx++;
                                if (ndx >= GetNumStatements())
                                    return specList;
                            }
                            endNdx = startNdx;
                        }
                    }
                }
                else if ((Type == EType.search) || (Type == EType.correct))
                {
                    for (int ctr = 0; ctr < GetNumStatements(); ctr++)
                        specList.Add(new CSingleton(((int)AnswerStates[ctr]).ToString()));
                }
                return specList;
            }
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
                String answer = String.Empty;
                AnswerStates = new EAnswerState[value.Value.Length];
                if (value.IsAnswered)
                {
                    for (int ctr = 0; ctr < value.Value.Length; ctr++)
                        AnswerStates[ctr] = (value.Value[ctr] == '1') ? EAnswerState.Selected : EAnswerState.Unselected;
                }
                else
                    AnswerStates.SetValue(EAnswerState.Unselected, AnswerStates.Select((s, ndx) => ndx).ToArray());
                for (int ctr = 0; ctr < AnswerStates.Length; ctr++)
                {
                    if (AnswerStates[ctr] == EAnswerState.Selected)
                        ChoiceChecks[ctr].Checked = true;
                    else
                        ChoiceChecks[ctr].Checked = false;
                }
            }
        }


        public EAnswerState this[int n]
        {
            get
            {
                return AnswerStates[n];
            }
            set
            {
                if (AnswerStates[n] == EAnswerState.SomeSelected)
                    _NumSomeSelected--;
                else if (AnswerStates[n] == EAnswerState.SomeUnselected)
                    _NumSomeUnselected--;
                AnswerStates[n] = value;
                if (AnswerStates[n] == EAnswerState.SomeSelected)
                    _NumSomeSelected++;
                else if (AnswerStates[n] == EAnswerState.SomeUnselected)
                    _NumSomeUnselected++;
            }
        }

        public override bool IsSearchMatch(String val)
        {
            int nSomeSelected = 0, nSomeUnselected = 0;
            for (int ctr = 0; ctr < val.Length; ctr++)
            {
                int nVal = Convert.ToInt32(val[ctr]);
                switch (AnswerStates[ctr])
                {
                    case EAnswerState.Unselected:
                        if (nVal != 0)
                            return false;
                        break;

                    case EAnswerState.Selected:
                        if (nVal != 1)
                            return false;
                        break;

                    case EAnswerState.SomeSelected:
                        if (nVal == 1)
                            nSomeSelected++;
                        break;

                    case EAnswerState.SomeUnselected:
                        if (nVal == 0)
                            nSomeUnselected++;
                        break;
                }
            }
            if (nSomeSelected < NumSomeSelected)
                return false;
            if (nSomeUnselected < NumSomeUnselected)
                return false;
            return true;
        }


    }
}


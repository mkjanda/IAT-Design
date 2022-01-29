using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Windows.Forms;
using IATClient.ResultData;

namespace IATClient
{
    class CBoolResponseObject : CResponseObject
    {
        public enum EAnswer { Both, True, False, None };
        private EAnswer _Answer = EAnswer.None;
        private Func<String> GetTrueStatement = null, GetFalseStatement = null;
        private new bool bIsNew = true;

        public EAnswer Answer
        {
            get
            {
                return _Answer;
            }
        }

        public CBoolResponseObject(EType type, Response response)
            : base(type, response)
        {
            GetTrueStatement = new Func<String>(((ResultData.Boolean)response).GetTrueStatement);
            GetFalseStatement = new Func<String>(((ResultData.Boolean)response).GetFalseStatement);
        }

        public CBoolResponseObject(CBoolResponseObject obj, ResultData.Boolean resp) : base(obj.Type, resp)
        {
            GetTrueStatement = new Func<String>(resp.GetTrueStatement);
            GetFalseStatement = new Func<String>(resp.GetFalseStatement);
            _Answer = obj._Answer;
       }

        public CBoolResponseObject(EType type, CSurveyItem csi)
            : base(type, csi)
        {
            CBoolResponse resp = (CBoolResponse)csi.Response;
            GetTrueStatement = new Func<String>(resp.GetTrueStatement);
            GetFalseStatement = new Func<String>(resp.GetFalseStatement);
        }

        public CBoolResponseObject(EType type, ResultSetDescriptor RSD)
            : base(type, RSD)
        {
        }

        private void TrueState_CheckedChanged(object sender, EventArgs e)
        {
            CheckBox cb = (CheckBox)sender;
            if (cb.Checked)
            {
                if (Answer == EAnswer.False)
                    _Answer = EAnswer.Both;
                else
                    _Answer = EAnswer.True;
            }
        }

        private void FalseState_CheckedChanged(object sender, EventArgs e)
        {
            CheckBox cb = (CheckBox)sender;
            if (cb.Checked)
            {
                if (Answer == EAnswer.True)
                    _Answer = EAnswer.Both;
                else
                    _Answer = EAnswer.False;
            }
        }

        public override bool IsSearchMatch(String val)
        {
            if (val == "1")
            {
                if ((Answer == EAnswer.Both) || (Answer == EAnswer.True))
                    return true;
                return false;
            }
            else if (val == "0")
            {
                if ((Answer == EAnswer.Both) || (Answer == EAnswer.False))
                    return true;
                return false;
            }
            else if ((Answer == EAnswer.None) || (val == "NULL"))
                return true;
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
                if (value.IsAnswered)
                {
                    int resp = Convert.ToInt32(value.Value);
                    if (resp == 1)
                        _Answer = EAnswer.True;
                    else if (resp == 0)
                        _Answer = EAnswer.False;
                    if (TrueRadio != null)
                        TrueRadio.Checked = (_Answer == EAnswer.True);
                    if (FalseRadio != null)
                        FalseRadio.Checked = (_Answer == EAnswer.False);
                }
                else
                {
                    _Answer = EAnswer.None;
                    if (TrueRadio != null)
                        TrueRadio.Checked = false;
                    if (FalseRadio != null)
                        FalseRadio.Checked = false;
                }
            }
        }

        protected override List<CResponseSpecifier> ResponseSpecifiers
        {
            get
            {
                CSingleton spec1 = null, spec2 = null;
                switch (_Answer)
                {
                    case EAnswer.Both:
                        spec1 = new CSingleton("1");
                        spec2 = new CSingleton("1");
                        break;

                    case EAnswer.True:
                        spec1 = new CSingleton("1");
                        spec2 = new CSingleton("0");
                        break;

                    case EAnswer.False:
                        spec1 = new CSingleton("0");
                        spec2 = new CSingleton("1");
                        break;

                    case EAnswer.None:
                        spec1 = new CSingleton("0");
                        spec2 = new CSingleton("0");
                        break;
                }
                List<CResponseSpecifier> specList = new List<CResponseSpecifier>();
                specList.Add(spec1);
                specList.Add(spec2);
                return specList;
            }
        }

        private Panel PreviewPanel = null;
        private TextBox TrueStatementBox = null, FalseStatementBox = null;
        private Font ControlFont = null;
        private RadioButton TrueRadio, FalseRadio;
        private CheckBox TrueCheck, FalseCheck;

        public override void DisposeOfControls()
        {
            if (PreviewPanel != null)
                foreach (Control c in PreviewPanel.Controls)
                    c.Dispose();
            PreviewPanel = null;
            if (ControlFont != null)
                ControlFont.Dispose();
            ControlFont = null;
        }


        public override Panel GenerateResponseObjectPanel(System.Drawing.Color backColor, System.Drawing.Color foreColor, string fontFamily, float fontSize, int clientWidth)
        {
            if (!bIsNew)
                DisposeOfControls();
            bIsNew = false;
            UpdateResponseObject();
            ControlFont = new Font(fontFamily, fontSize);

            PreviewPanel = new Panel();
            PreviewPanel.BackColor = backColor;
            PreviewPanel.ForeColor = foreColor;
            int maxWidth = 0;

            // add true statement
            TrueStatementBox = new TextBox();
            TrueStatementBox.BackColor = backColor;
            TrueStatementBox.ForeColor = foreColor;
            TrueStatementBox.ReadOnly = true;
            TrueStatementBox.Multiline = true;
            TrueStatementBox.Font = ControlFont;
            TrueStatementBox.BorderStyle = BorderStyle.None;
            TrueStatementBox.Text = GetTrueStatement();
            Size szChoice = System.Windows.Forms.TextRenderer.MeasureText(TrueStatementBox.Text, ControlFont, new Size(clientWidth - RadioSize.Width - RadioPadding.Right, 0), 
                TextFormatFlags.WordBreak | TextFormatFlags.TextBoxControl);
            Point choiceLoc, radioLoc;
            if (szChoice.Height > RadioSize.Height)
            {
                choiceLoc = new Point(RadioSize.Width + RadioPadding.Right, 0);
                radioLoc = new Point(0, (szChoice.Height - RadioSize.Height) >> 1);
            }
            else
            {
                radioLoc = new Point(0, 0);
                choiceLoc = new Point(RadioSize.Width + RadioPadding.Right, ((RadioSize.Height - szChoice.Height) >> 1));
            }
            Control selector = null;
            if ((Type == EType.actual) || (Type == EType.dummy))
            {
                TrueRadio = new RadioButton();
                if (Type == EType.actual)
                {
                    if (Answer == EAnswer.True)
                        TrueRadio.Checked = true;
                    else
                        TrueRadio.Checked = false;
                    TrueRadio.Enabled = false;
                }
                selector = TrueRadio;
            }
            else if ((Type == EType.search) || (Type == EType.correct))
            {
                TrueCheck = new CheckBox();
                selector = TrueCheck;
                    if ((Answer == EAnswer.Both) || (Answer == EAnswer.True))
                        TrueCheck.Checked = true;
                    else
                        TrueCheck.Checked = false;
                
                TrueCheck.CheckedChanged += new EventHandler(TrueState_CheckedChanged);
                selector = TrueCheck;
            }
            selector.Location = radioLoc;
            selector.Size = RadioSize;
            selector.BackColor = backColor;
            selector.ForeColor = foreColor;
            PreviewPanel.Controls.Add(selector);
            TrueStatementBox.Size = szChoice;
            TrueStatementBox.Location = choiceLoc;
            PreviewPanel.Controls.Add(TrueStatementBox);
            int trueBottom = (TrueStatementBox.Bottom > selector.Bottom) ? TrueStatementBox.Bottom : selector.Bottom;

            // add false statement
            FalseStatementBox = new TextBox();
            FalseStatementBox.BackColor = backColor;
            FalseStatementBox.ForeColor = foreColor;
            FalseStatementBox.ReadOnly = true;
            FalseStatementBox.Multiline = true;
            FalseStatementBox.Font = ControlFont;
            FalseStatementBox.BorderStyle = BorderStyle.None;
            FalseStatementBox.Text = GetFalseStatement();
            szChoice = System.Windows.Forms.TextRenderer.MeasureText(FalseStatementBox.Text, ControlFont,
                new Size(clientWidth - RadioSize.Width - RadioPadding.Right, 0), TextFormatFlags.WordBreak | TextFormatFlags.TextBoxControl);
            if (clientWidth - RadioSize.Width - RadioPadding.Right > maxWidth)
                maxWidth = clientWidth - RadioSize.Width - RadioPadding.Right;
            if (szChoice.Height > RadioSize.Height)
            {
                choiceLoc = new Point(RadioSize.Width + RadioPadding.Right, trueBottom + ElementPadding.Vertical);
                radioLoc = new Point(0, trueBottom + ElementPadding.Vertical + ((szChoice.Height - RadioSize.Height) >> 1));
            }
            else
            {
                radioLoc = new Point(0, trueBottom + ElementPadding.Vertical);
                choiceLoc = new Point(RadioSize.Width + RadioPadding.Right, trueBottom + ElementPadding.Vertical + ((RadioSize.Height - szChoice.Height) >> 1));
            }
            if ((Type == EType.dummy) || (Type == EType.actual))
            {
                FalseRadio = new RadioButton();
                if (Type == EType.actual)
                {
                    if (Answer == EAnswer.False)
                        FalseRadio.Checked = true;
                    else
                        FalseRadio.Checked = false;
                    FalseRadio.Enabled = false;
                }
                selector = FalseRadio;
            }
            else if ((Type == EType.correct) || (Type == EType.search))
            {
                FalseCheck = new CheckBox();
                selector = FalseCheck;
                if ((Answer == EAnswer.False) || (Answer == EAnswer.Both))
                    FalseCheck.Checked = true;
                else
                    FalseCheck.Checked = false;
                FalseCheck.CheckedChanged += new EventHandler(FalseState_CheckedChanged);
                selector = FalseCheck;
            }
            selector.Location = radioLoc;
            selector.Size = RadioSize;
            selector.BackColor = backColor;
            selector.ForeColor = foreColor;
            PreviewPanel.Controls.Add(selector);
            FalseStatementBox.Size = szChoice;
            FalseStatementBox.Location = choiceLoc;
            PreviewPanel.Controls.Add(FalseStatementBox);

            int falseBottom = (FalseStatementBox.Bottom > selector.Bottom) ? FalseStatementBox.Bottom : selector.Bottom;
            PreviewPanel.Size = new Size(clientWidth, falseBottom);
            return PreviewPanel;
        }
    }
}


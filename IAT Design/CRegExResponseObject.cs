using IATClient.ResultData;
using System;
using System.Drawing;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace IATClient
{
    class CRegExResponseObject : CResponseObject
    {
        private TextBox RegExBox = null, AnswerBox = null;
        private Label AnswerRegExLabel = null, SearchRegExLabel = null;
        private Func<String> GetRegEx = null;
        private String _Answer = String.Empty, _SearchVal = String.Empty, _CorrectAnswer = String.Empty;

        /*
        public CRegExResponseObject DisplayedResponse
        {
            get
            {
                if (Type == EType.dummy)
                    throw new InvalidOperationException();
                return new CRegExResponseObject(AnswerBox.Text, Type);
            }
            set
            {
                this.Type = value.Type;
                AnswerBox.Text = value.AnswerBox.Text;
            }
        }
        */

        public CRegExResponseObject(EType type, ResultSetDescriptor rsd) : base(type, rsd) { }

        public CRegExResponseObject(EType type, Response resp) : base(type, resp)
        {
            GetRegEx = new Func<String>(((RegEx)resp).GetRegEx);
        }

        public CRegExResponseObject(CRegExResponseObject obj, RegEx resp)
            : base(obj.Type, resp)
        {
            GetRegEx = new Func<String>(resp.GetRegEx);
            _Answer = obj._Answer;
        }

        public CRegExResponseObject(EType type, CSurveyItem csi) : base(type, csi)
        {
            CRegExResponse resp = (CRegExResponse)csi.Response;
            GetRegEx = new Func<String>(resp.GetRegEx);
        }

        public override void DisposeOfControls()
        {
            if ((Type == EType.dummy) || (Type == EType.actual))
            {
                if (AnswerBox != null)
                {
                    _Answer = AnswerBox.Text;
                    AnswerBox.Dispose();
                }
                AnswerBox = null;
            }
            else if ((Type == EType.correct) || (Type == EType.search))
            {
                AnswerRegExLabel.Dispose();
                AnswerRegExLabel = null;
                if (Type == EType.correct)
                    _CorrectAnswer = RegExBox.Text;
                else
                    _SearchVal = RegExBox.Text;
                RegExBox.Dispose();
                RegExBox = null;
            }
        }

        public override Panel GenerateResponseObjectPanel(System.Drawing.Color backColor, System.Drawing.Color foreColor, string fontFamily, float fontSize, int clientWidth)
        {
            if (!bIsNew)
                DisposeOfControls();
            UpdateResponseObject();
            Panel PreviewPanel = new Panel();
            Font DisplayFont = new Font(fontFamily, fontSize);
            if ((Type == EType.dummy) || (Type == EType.actual))
            {
                AnswerBox = new TextBox();
                AnswerBox.BackColor = backColor;
                AnswerBox.ForeColor = foreColor;
                AnswerBox.Font = DisplayFont;
                AnswerBox.Location = new Point(ElementPadding.Left, 0);
                AnswerBox.Width = (clientWidth - AnswerBox.Location.X) >> 1;
                if (Type == EType.actual)
                    AnswerBox.Text = _Answer;
                PreviewPanel.Controls.Add(AnswerBox);
            }
            else
            {
                AnswerRegExLabel = new Label();
                AnswerRegExLabel.ForeColor = foreColor;
                AnswerRegExLabel.BackColor = backColor;
                AnswerRegExLabel.Font = DisplayFont;
                AnswerRegExLabel.Text = "Answer Matches Regular Expression: " + GetRegEx();
                AnswerRegExLabel.Size = TextRenderer.MeasureText(AnswerRegExLabel.Text, DisplayFont);
                AnswerRegExLabel.Location = new Point(ElementPadding.Left, ((AnswerBox.Height - AnswerRegExLabel.Height) >> 1));

                SearchRegExLabel = new Label();
                SearchRegExLabel.ForeColor = foreColor;
                SearchRegExLabel.BackColor = backColor;
                SearchRegExLabel.Font = DisplayFont;
                if (Type == EType.correct)
                    SearchRegExLabel.Text = "Correct Answers Match the Regular Expression: ";
                else
                    SearchRegExLabel.Text = "Find Answers that Match the Regular Expression: ";
                SearchRegExLabel.Size = TextRenderer.MeasureText(SearchRegExLabel.Text, DisplayFont);

                RegExBox = new TextBox();
                RegExBox.ForeColor = foreColor;
                RegExBox.BackColor = backColor;
                RegExBox.Font = DisplayFont;
                RegExBox.Width = (clientWidth - ElementPadding.Left) >> 1;

                if (RegExBox.Height > SearchRegExLabel.Height)
                {
                    SearchRegExLabel.Location = new Point(ElementPadding.Left, AnswerRegExLabel.Bottom + ElementPadding.Vertical + ((RegExBox.Height - SearchRegExLabel.Height) >> 1));
                    RegExBox.Location = new Point(SearchRegExLabel.Right + ElementPadding.Horizontal, AnswerRegExLabel.Bottom + ElementPadding.Bottom);
                }
                else
                {
                    SearchRegExLabel.Location = new Point(ElementPadding.Left, AnswerRegExLabel.Bottom + ElementPadding.Vertical);
                    RegExBox.Location = new Point(SearchRegExLabel.Right + ElementPadding.Horizontal, AnswerRegExLabel.Bottom + ElementPadding.Vertical + ((SearchRegExLabel.Height - RegExBox.Height) >> 1));
                }
                if (Type == EType.search)
                {
                    RegExBox.Text = SearchVal;
                    _SearchVal = String.Empty;
                }
                else if (Type == EType.correct)
                {
                    RegExBox.Text = CorrectAnswer;
                    _CorrectAnswer = String.Empty;
                }
                PreviewPanel.Controls.Add(AnswerRegExLabel);
                PreviewPanel.Controls.Add(RegExBox);
            }
            bIsNew = false;
            return PreviewPanel;
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
                    AnswerBox.Text = value.Value;
                }
                else
                {
                    AnswerBox.Text = String.Empty;
                }
            }
        }

        public String CorrectAnswer
        {
            get
            {
                if (_CorrectAnswer != String.Empty)
                    return _CorrectAnswer;
                else
                    return RegExBox.Text;
            }
        }

        public String SearchVal
        {
            get
            {
                if (_SearchVal != String.Empty)
                    return _SearchVal;
                else
                    return RegExBox.Text;
            }
        }

        public override bool IsSearchMatch(String val)
        {
            Regex exp = new Regex(SearchVal);
            return exp.IsMatch(val);
        }

    }
}


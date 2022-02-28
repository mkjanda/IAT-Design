using IATClient.ResultData;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace IATClient
{
    class CBoundedNumResponseObject : CResponseObject
    {
        private List<CResponseSpecifier> _ResponseSpecifiers = new List<CResponseSpecifier>();
        private Nullable<decimal> _Answer = 0;
        private Panel PreviewPanel = null;
        private TextBox AnswerBox = null;
        private Func<CResponseObject.CResponseSpecifier> GetBounds = null;
        private new bool bIsNew = true;
        private bool bIsUnanswered = false, bIsNull = false;

        public CBoundedNumResponseObject(EType type, Response response) : base(type, response)
        {
            BoundedNumber resp = (BoundedNumber)response;
            GetBounds = new Func<CResponseObject.CResponseSpecifier>(resp.GetBounds);
        }

        public CBoundedNumResponseObject(EType type, ResultSetDescriptor rsd) : base(type, rsd) { }

        public CBoundedNumResponseObject(CBoundedNumResponseObject obj, BoundedNumber resp)
            : base(obj.Type, resp)
        {
            GetBounds = new Func<CResponseObject.CResponseSpecifier>(resp.GetBounds);
            _Answer = obj._Answer;
        }

        public CBoundedNumResponseObject(EType type, CSurveyItem csi)
            : base(type, csi)
        {
            GetBounds = new Func<CResponseObject.CResponseSpecifier>(((CBoundedNumResponse)csi.Response).GetBounds);
        }

        public decimal GetMinBound()
        {
            String str = GetBounds().Specifier;
            return Convert.ToDecimal(str.Substring(0, str.IndexOf('-')));
        }

        public decimal GetMaxBound()
        {
            String str = GetBounds().Specifier;
            return Convert.ToDecimal(str.Substring(str.IndexOf('-') + 1));
        }

        public override bool IsSearchMatch(string val)
        {
            foreach (CResponseSpecifier rs in ResponseSpecifiers)
                if (rs.IsSearchMatch(val))
                    return true;
            return false;
        }

        public Nullable<decimal> Answer
        {
            get
            {
                if (Type == EType.actual)
                    return _Answer;
                throw new InvalidOperationException();
            }
            set
            {
                if (Type == EType.actual)
                    _Answer = value;
                else
                    throw new InvalidOperationException();
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
                if (value.IsAnswered)
                {
                    _Answer = Convert.ToDecimal(value.Value);
                    if (AnswerBox != null)
                        AnswerBox.Text = value.Value;
                }
                else
                {
                    _Answer = null;
                    if (AnswerBox != null)
                        AnswerBox.Text = String.Empty;

                }
            }
        }

        public String Description
        {
            get
            {
                if (ResponseSpecifiers.Count == 0)
                    return String.Empty;
                else
                {
                    String str = ResponseSpecifiers[0].Specifier;
                    for (int ctr = 1; ctr < ResponseSpecifiers.Count; ctr++)
                        str += ", " + ResponseSpecifiers[0].Specifier;
                    return str;
                }
            }
        }

        public String ResponseRange
        {
            get
            {
                String resp = String.Empty;
                if ((Type == EType.search) || (Type == EType.correct))
                {
                    for (int ctr = 0; ctr < ResponseSpecifiers.Count; ctr++)
                    {
                        if (resp.Length != 0)
                            resp += ", ";
                        resp += ResponseSpecifiers[ctr].Specifier;
                    }
                    return resp;
                }
                else
                    throw new InvalidOperationException();
            }
        }

        public bool SetResponse(String specifiers)
        {
            try
            {
                String specs = (String)specifiers.Clone();
                int pos;
                decimal MinVal = GetMinBound();
                decimal MaxVal = GetMaxBound();
                while (specs != String.Empty)
                {
                    pos = specs.IndexOf(',');
                    String spec;
                    if (pos == -1)
                        spec = specs;
                    else
                        spec = specs.Substring(0, pos);
                    if (spec.Contains("-"))
                    {
                        String n1 = spec.Substring(0, spec.IndexOf('-'));
                        String n2 = spec.Substring(spec.IndexOf('-') + 1);
                        decimal d1 = Convert.ToDecimal(n1);
                        decimal d2 = Convert.ToDecimal(n2);
                        if (d2 <= d1)
                        {
                            MessageBox.Show(MainForm, String.Format(Properties.Resources.sBoundedNumRangeError, d1, d2), Properties.Resources.sInvalidInput, MessageBoxButtons.OK);
                            return false;
                        }
                        if ((d1 < MinVal) || (d2 < MinVal) || (d1 > MaxVal) || (d2 > MaxVal))
                        {
                            if (Type == EType.search)
                                MessageBox.Show(MainForm, String.Format(Properties.Resources.sBoundedNumSearchRangeError, d1, d2, GetMinBound(), GetMaxBound()), Properties.Resources.sInvalidInput, MessageBoxButtons.OK);
                            else if (Type == EType.correct)
                                MessageBox.Show(MainForm, String.Format(Properties.Resources.sBoundedNumCorrectRangeError, d1, d2, GetMinBound(), GetMaxBound()), Properties.Resources.sInvalidInput, MessageBoxButtons.OK);
                            return false;
                        }
                        ResponseSpecifiers.Add(new CRange(n1, n2));
                    }
                    else
                    {
                        decimal d = Convert.ToDecimal(spec);
                        if ((d < MinVal) || (d > MaxVal))
                        {
                            if (Type == EType.search)
                                MessageBox.Show(MainForm, String.Format(Properties.Resources.sBoundedNumSearchSingletonOutOfRangeError, d, GetMinBound(), GetMaxBound()), Properties.Resources.sInvalidInput, MessageBoxButtons.OK);
                            else if (Type == EType.correct)
                                MessageBox.Show(MainForm, String.Format(Properties.Resources.sBoundedNumCorrectSingletonOutOfRangeError, d, GetMinBound(), GetMaxBound()), Properties.Resources.sInvalidInput, MessageBoxButtons.OK);
                            return false;
                        }
                        ResponseSpecifiers.Add(new CSingleton(spec));
                    }
                    if (pos == -1)
                        specs = String.Empty;
                    else
                        specs = specs.Substring(pos + 1);
                }
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        protected override List<CResponseSpecifier> ResponseSpecifiers
        {
            get
            {
                return _ResponseSpecifiers;
            }
        }

        public override void DisposeOfControls()
        {
            if (AnswerBox != null)
            {
                AnswerBox.Dispose();
                AnswerBox = null;
            }
        }

        public void AnswerBox_LostFocus(object sender, EventArgs e)
        {

        }

        public override Panel GenerateResponseObjectPanel(System.Drawing.Color backColor, System.Drawing.Color foreColor, string fontFamily, float fontSize, int clientWidth)
        {
            if (!bIsNew)
                DisposeOfControls();
            bIsNew = true;
            UpdateResponseObject();
            PreviewPanel = new Panel();
            AnswerBox = new TextBox();
            Font PreviewFont = new Font(fontFamily, fontSize);
            AnswerBox.LostFocus += new EventHandler(AnswerBox_LostFocus);
            AnswerBox.BackColor = backColor;
            AnswerBox.ForeColor = foreColor;
            AnswerBox.Font = PreviewFont;
            AnswerBox.Location = new Point((int)(.15 * clientWidth), 0);
            AnswerBox.Width = (int)(.2 * clientWidth);
            if (Type == EType.actual)
            {
                AnswerBox.ReadOnly = true;
                AnswerBox.BackColor = backColor;
            }
            PreviewPanel.Size = new Size(clientWidth, AnswerBox.Height);
            PreviewPanel.Controls.Add(AnswerBox);
            return PreviewPanel;
        }

    }
}

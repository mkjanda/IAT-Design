using System;
using System.Drawing;
using System.Windows.Forms;

namespace IATClient
{
    class StatementPanel : Panel
    {
        private RadioButton LeftRadio, RightRadio;
        private TextBox StatementEdit;
        private CSurveyItem SurveyItem;
        private int StatementIndex;
        private String _Statement;
        private String _Value;
        static Size RadioSize = new Size(20, 20);
        private static Font DisplayFont = new Font("Arial", 9.75F);
        public delegate void RadioSelectionHandler(CSurveyItem question, int statementNdx);
        public delegate void SizeChangedHandler(StatementPanel sender);
        public RadioSelectionHandler OnLeftRadio, OnRightRadio;
        public SizeChangedHandler OnControlSizeChanged;

        public KeyedDirection KeyedDir
        {
            get
            {
                if (LeftRadio.Checked)
                    return KeyedDirection.Left;
                else if (RightRadio.Checked)
                    return KeyedDirection.Right;
                else
                    return KeyedDirection.None;
            }
            set
            {
                if (value == KeyedDirection.Left)
                {
                    LeftRadio.Checked = true;
                    RightRadio.Checked = false;
                }
                else if (value == KeyedDirection.Right)
                {
                    RightRadio.Checked = true;
                    LeftRadio.Checked = false;
                }
                else if (value == KeyedDirection.None)
                {
                    RightRadio.Checked = false;
                    LeftRadio.Checked = false;
                }
            }
        }

        public String Statement
        {
            get
            {
                return _Statement;
            }
        }

        public String Value
        {
            get
            {
                return _Value;
            }
        }

        public StatementPanel(CSurveyItem question, int statementIndex, String statement, String value, int width)
        {
            SurveyItem = question;
            StatementIndex = statementIndex;
            _Statement = statement;
            _Value = value;
            LeftRadio = new RadioButton();
            RightRadio = new RadioButton();
            StatementEdit = new TextBox();
            LeftRadio.Size = RadioSize;
            RightRadio.Size = RadioSize;
            LeftRadio.Location = new Point(0, 0);
            RightRadio.Location = new Point(RadioSize.Width << 1, 0);
            StatementEdit.Location = new Point(RadioSize.Width << 2 + 1, 1);
            StatementEdit.Multiline = true;
            StatementEdit.Size = TextRenderer.MeasureText(statement, DisplayFont, new Size((width - RadioSize.Width << 2) - 2 - StatementEdit.Margin.Horizontal, 0),
                TextFormatFlags.WordBreak | TextFormatFlags.Left | TextFormatFlags.NoPrefix) + new Size(StatementEdit.Margin.Horizontal + 2, StatementEdit.Margin.Vertical + 2);
            StatementEdit.Font = DisplayFont;
            StatementEdit.BorderStyle = BorderStyle.None;
            StatementEdit.BackColor = System.Drawing.Color.White;
            StatementEdit.ForeColor = System.Drawing.Color.DarkGray;
            Controls.Add(LeftRadio);
            Controls.Add(RightRadio);
            Controls.Add(StatementEdit);
            LeftRadio.CheckedChanged += new EventHandler(LeftRadio_CheckedChanged);
            RightRadio.CheckedChanged += new EventHandler(RightRadio_CheckedChanged);
            StatementEdit.TextChanged += new EventHandler(StatementEdit_TextChanged);
            StatementEdit.MouseEnter += new EventHandler(StatementEdit_MouseEnter);
            StatementEdit.MouseLeave += new EventHandler(StatementEdit_MouseLeave);
            StatementEdit.GotFocus += new EventHandler(StatementEdit_GotFocus);
            StatementEdit.LostFocus += new EventHandler(StatementEdit_LostFocus);
            this.Size = new Size(width, RadioSize.Height > StatementEdit.Size.Height ? RadioSize.Height : StatementEdit.Height);

        }

        private void StatementEdit_TextChanged(object sender, EventArgs e)
        {
            switch (SurveyItem.Response.ResponseType)
            {
                case CResponse.EResponseType.Boolean:
                    if (StatementIndex == 0)
                        ((CBoolResponse)SurveyItem.Response).TrueStatement = StatementEdit.Text;
                    else
                        ((CBoolResponse)SurveyItem.Response).FalseStatement = StatementEdit.Text;
                    break;

                case CResponse.EResponseType.Likert:
                    ((CLikertResponse)SurveyItem.Response).ChoiceDescriptions[StatementIndex] = StatementEdit.Text;
                    break;

                case CResponse.EResponseType.MultiBoolean:
                    ((CMultiBooleanResponse)SurveyItem.Response).LabelList[StatementIndex] = StatementEdit.Text;
                    break;

                case CResponse.EResponseType.Multiple:
                    ((CMultipleResponse)SurveyItem.Response).Choices[StatementIndex] = StatementEdit.Text;
                    break;

                case CResponse.EResponseType.WeightedMultiple:
                    ((CWeightedMultipleResponse)SurveyItem.Response).Choices[StatementIndex] = StatementEdit.Text;
                    break;
            }
            Size newSize = TextRenderer.MeasureText(StatementEdit.Text, DisplayFont, new Size((this.Width - RadioSize.Width << 2) - 2 - StatementEdit.Margin.Horizontal, 0),
                TextFormatFlags.WordBreak | TextFormatFlags.Left | TextFormatFlags.NoPrefix) + new Size(StatementEdit.Margin.Horizontal + 2, StatementEdit.Margin.Vertical + 2);
            if (newSize.Height < RadioSize.Height)
                newSize.Height = RadioSize.Height;
            if (newSize.Height != this.Size.Height)
            {
                this.Height = newSize.Height;
                OnControlSizeChanged(this);
            }
        }

        private void LeftRadio_CheckedChanged(object sender, EventArgs e)
        {
            OnLeftRadio(SurveyItem, StatementIndex);
        }

        private void RightRadio_CheckedChanged(object sender, EventArgs e)
        {
            OnRightRadio(SurveyItem, StatementIndex);
        }

        private void StatementEdit_MouseEnter(object sender, EventArgs e)
        {
            if (!StatementEdit.Focused)
                StatementEdit.BackColor = System.Drawing.Color.LightGray;
        }

        private void StatementEdit_MouseLeave(object sender, EventArgs e)
        {
            if (!StatementEdit.Focused)
                StatementEdit.BackColor = System.Drawing.Color.White;
        }

        private void StatementEdit_GotFocus(object sender, EventArgs e)
        {
            SuspendLayout();
            StatementEdit.Location -= new Size(1, 1);
            StatementEdit.BackColor = System.Drawing.Color.LightBlue;
            StatementEdit.ForeColor = System.Drawing.Color.Black;
            StatementEdit.BorderStyle = BorderStyle.FixedSingle;
            ResumeLayout(true);
        }

        private void StatementEdit_LostFocus(object sender, EventArgs e)
        {
            SuspendLayout();
            StatementEdit.Location += new Size(1, 1);
            StatementEdit.BackColor = System.Drawing.Color.White;
            StatementEdit.ForeColor = System.Drawing.Color.DarkGray;
            StatementEdit.BorderStyle = BorderStyle.None;
            ResumeLayout(true);
        }
    }
}

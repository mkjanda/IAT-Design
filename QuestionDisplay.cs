using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using System.Threading;
using System.Threading.Tasks;

namespace IATClient
{
    class QuestionDisplay : SurveyItemDisplay
    {
        private Font displayFont = null;
        protected static readonly Padding QuestionEditMargin = new Padding(6, 16, 6, 6), QuestionEditPadding = new Padding(5);
        protected static readonly Padding ResponseEditMargin = new Padding(10, 5, 10, 5);
        protected TextBox QuestionEdit;
        protected bool _IsSelected;
        protected bool _IsUniqueResponse = false;
        protected ResponseDisplay ResponseEdit;
        protected CResponse.EResponseType _ItemType;
        protected bool IsInitialized;
        protected int QuestionLines = 1;
        protected Rectangle OptionalRect = Rectangle.Empty;
        private Rectangle _FormatRect;
        protected virtual Rectangle FormatRect { get { return _FormatRect; } }
        protected bool bOptional, bFormatHover = false;
        protected Button CollapseButton, ExpandButton;
        protected bool bFormatting = false;
        protected virtual String DefaultQuestionEditText
        {
            get
            {
                return Properties.Resources.sDefaultQuestionText;
            }
        }

        private SurveyItemFormat _Format = null;

        private bool LayoutSuspended { get; set; } = false;

        public void SuspendLayoutCalculations()
        {
            LayoutSuspended = true;
        }

        public void ResumeLayoutCalculations()
        {
            LayoutSuspended = false;
        }

        public virtual SurveyItemFormat Format
        {
            get
            {
                if (_Format == null)
                {
                    _Format = new SurveyItemFormat(SurveyItemFormat.EFor.Item)
                    {
                        Bold = false,
                        Italic = false,
                        FontSize = "12px",
                        Color = Color.Black,
                        Font = SurveyItemFormat.EFont.genericSansSerif
                    };
                    DisplayFont = new Font(_Format.Font.Family, _Format.FontSizeAsPixels * CIATLayout.yDpi / 96F, _Format.FontStyle, GraphicsUnit.Pixel);
                    /*if (QuestionEdit != null)
                    {
                        if (!IsHandleCreated)
                            HandleCreated += (sender, args) => QuestionEdit.Font = DisplayFont;
                        else
                            this.Invoke(new Action(() => QuestionEdit.Font = DisplayFont));
                        RecalcSize();
                        SizeQuestionEdit(true);
                    }*/
                }
                return _Format;
            }
            protected set
            {
                _Format = value;
                DisplayFont = new Font(_Format.Font.Family, _Format.FontSizeAsPixels * CIATLayout.yDpi / 96F, _Format.FontStyle, GraphicsUnit.Pixel);
                if (QuestionEdit != null)
                {
                    if (!QuestionEdit.IsHandleCreated)
                        QuestionEdit.HandleCreated += (sender, args) =>
                        {
                            QuestionEdit.Font = DisplayFont;
                        };
                    else
                        this.Invoke(new Action(() => QuestionEdit.Font = DisplayFont));
                    RecalcSize();
                    SizeQuestionEdit(true);
                }
            }
        }

        public CResponse.EResponseType ItemType
        {
            get
            {
                return _ItemType;
            }
        }

        public override bool IsUnique
        {
            get
            {
                return _IsUniqueResponse;
            }
            set
            {
                _IsUniqueResponse = value;
            }
        }

        public override bool Selected
        {
            get
            {
                return _IsSelected;
            }
            set
            {
                _IsSelected = value;
            }
        }

        private object lockObj = new object();

        protected Font DisplayFont
        {
            get
            {
                lock (lockObj)
                {
                    if (displayFont == null)
                        displayFont = new Font(Format.Font.Family, Format.FontSizeAsPixels * CIATLayout.yDpi / 96F, Format.FontStyle, GraphicsUnit.Pixel);
                    return displayFont;
                }
            }
            private set
            {
                if (displayFont == value)
                    return;
                lock (lockObj)
                {
                    displayFont?.Dispose();
                    displayFont = value;
                }
            }
        }


        public bool Optional
        {
            get
            {
                return bOptional;
            }
        }

        public SurveyItemFormat QuestionFormat
        {
            get
            {
                return Format;
            }
        }


        CSurveyItem _SurveyItem = null;
        public override CSurveyItem SurveyItem
        {
            get
            {
                return _SurveyItem;
            }
            set
            {
                if (value == null)
                {
                    _SurveyItem?.Dispose();
                    _SurveyItem = value;
                    return;
                }
                if (_SurveyItem != null)
                {
                    if (_SurveyItem.URI.Equals(value.URI))
                        return;
                    if (CIAT.SaveFile.IAT.UniqueResponse.SurveyItemUri != null)
                        if (CIAT.SaveFile.IAT.UniqueResponse.SurveyItemUri.Equals(_SurveyItem.URI))
                            CIAT.SaveFile.IAT.UniqueResponse.SurveyItemUri = value.URI;
                }
                _SurveyItem?.Dispose();
                _SurveyItem = value;
                _ItemType = SurveyItem.Response.ResponseType;
                SuspendLayoutCalculations();
                Format = value.Format;
                if (value.Text == String.Empty)
                    QuestionEdit.Text = DefaultQuestionEditText;
                else
                    QuestionEdit.Text = value.Text;
                QuestionEdit.ForeColor = value.Format.Color;
                InitializeResponseEdit();
                SizeQuestionEdit(true);
                RecalcSize();
                ResumeLayoutCalculations();
            }
        }

        public SurveyItemFormat ResponseFormat
        {
            get
            {
                return ResponseEdit.Format;
            }
        }

        private void SetSurveyItemFormat(ISurveyItemFormatChanged e)
        {
            this.BeginInvoke(new Action(() =>
            {
                if (e.SurveyItemFormat.For == SurveyItemFormat.EFor.Item)
                {
                    Format = e.SurveyItemFormat;
                    if (SurveyItem != null)
                        SurveyItem.Format = e.SurveyItemFormat;
                    QuestionEdit.ForeColor = Format.Color;
                    SizeQuestionEdit(true);
                    ResumeLayout();
                }
                else
                {
                    ResponseEdit.Format = e.SurveyItemFormat;
                    if (SurveyItem != null)
                        if (SurveyItem.Response != null)
                            SurveyItem.Response.Format = e.SurveyItemFormat;
                }
            }));
        }

        public QuestionDisplay()
        {
            IsInitialized = false;
            ResponseEdit = null;
            Format = new SurveyItemFormat(SurveyItemFormat.EFor.Item);
            QuestionEdit = new TextBox()
            {
                Location = new Point(QuestionEditMargin.Left + QuestionEditPadding.Left, QuestionEditMargin.Top + QuestionEditPadding.Top),
                BorderStyle = BorderStyle.None,
                ForeColor = Format.Color,
                BackColor = Color.White,
                Multiline = true,
                AcceptsReturn = true,
                AcceptsTab = false,
                Text = DefaultQuestionEditText
            };
            MouseLeave += (sender, args) =>
            {
                if (bFormatHover == true)
                {
                    bFormatHover = false;
                    Invalidate();
                }
            };
            MouseMove += (sender, args) =>
            {
                if (FormatRect.Contains(args.Location)) {
                    if (bFormatHover == false)
                    {
                        bFormatHover = true;
                        Invalidate();
                    }
                } else if (bFormatHover == true)
                {
                    bFormatHover = false;
                    Invalidate();
                }
            };
            MouseClick += (sender, args) =>
            {
                ((SurveyDisplay)Parent)?.SelectionChanged(this, ModifierKeys);
                if (OptionalRect.Contains(args.Location)) {
                    bOptional = !bOptional;
                    Invalidate();
                }
                else if (FormatRect.Contains(args.Location))
                {
                    bFormatting = true;
                    Invalidate();
                    CIAT.Dispatcher.Dispatch<IFormatSurveyItemText>(new CFormatSurveyItemText(this));
                    CIAT.Dispatcher.AddListener<ISurveyItemFormatChanged>(SetSurveyItemFormat);
                }
            };
            QuestionEdit.MouseHover += (sender, args) =>
            {
                QuestionEdit.ForeColor = Format.Color;
                QuestionEdit.BorderStyle = BorderStyle.FixedSingle;
                QuestionEdit.BackColor = Color.LightBlue;
            };
            QuestionEdit.MouseLeave += (sender, args) =>
            {
                if (!QuestionEdit.Focused)
                {
                    QuestionEdit.ForeColor = Format.Color;
                    QuestionEdit.BorderStyle = BorderStyle.None;
                    QuestionEdit.BackColor = this.BackColor;
                }
            };
            QuestionEdit.Leave += (sender, arg) =>
            {
                QuestionEdit.BackColor = this.BackColor;
                QuestionEdit.ForeColor = Format.Color;
                QuestionEdit.BorderStyle = BorderStyle.None;
                if (QuestionEdit.Text == String.Empty)
                    QuestionEdit.Text = DefaultQuestionEditText;
            };
            QuestionEdit.Enter += (sender, args) =>
            {
                QuestionEdit.ForeColor = Format.Color;
                QuestionEdit.BackColor = System.Drawing.Color.LightBlue;
                QuestionEdit.BorderStyle = BorderStyle.FixedSingle;
                if (QuestionEdit.Text == DefaultQuestionEditText)
                    QuestionEdit.Text = String.Empty;
            };
            QuestionEdit.MouseClick += (sender, args) =>
            {
                ((SurveyDisplay)Parent)?.SelectionChanged(this, ModifierKeys);
                Point mouseLoc = new Point(args.X - QuestionEdit.Left, args.Y - QuestionEdit.Top);
                if (OptionalRect.Contains(mouseLoc))
                {
                    bOptional = !bOptional;
                    Invalidate();
                }
                else if (FormatRect.Contains(mouseLoc))
                {
                    CIAT.Dispatcher.Dispatch<IFormatSurveyItemText>(new CFormatSurveyItemText(this));
                    CIAT.Dispatcher.AddListener<ISurveyItemFormatChanged>(SetSurveyItemFormat);
                }
            };
            QuestionEdit.TextChanged += (sender, args) => {
                SizeQuestionEdit(false);
                if (SurveyItem != null)
                    SurveyItem.Text = QuestionEdit.Text;
            };
            QuestionEdit.ForeColor = Format.Color;
            QuestionEdit.BorderStyle = BorderStyle.None;
            QuestionEdit.Multiline = true;
            QuestionEdit.AcceptsReturn = true;
            QuestionEdit.AcceptsTab = false;
            QuestionEdit.Padding = QuestionEditPadding;
            Controls.Add(QuestionEdit);
            ExpandButton = new Button();
            ExpandButton.BackColor = this.BackColor;
            ExpandButton.Image = Properties.Resources.ExpandArrow;
            ExpandButton.Size = Properties.Resources.ExpandArrow.Size + new Size(4, 4);
            ExpandButton.Location = new Point(this.Size.Width - ExpandButton.Size.Width - QuestionEditMargin.Right, Padding.Top);
            ExpandButton.Click += (sender, args) =>
            {
                ResponseEdit.IsCollapsed = false;
                ExpandButton.Enabled = false;
                CollapseButton.Enabled = true;
                RecalcSize();
            };
            ExpandButton.Enabled = false;
            Controls.Add(ExpandButton);

            CollapseButton = new Button();
            CollapseButton.BackColor = this.BackColor;
            CollapseButton.Image = Properties.Resources.CollapseArrow;
            CollapseButton.Size = Properties.Resources.CollapseArrow.Size + new Size(4, 4);
            CollapseButton.Location = new Point(ExpandButton.Location.X - CollapseButton.Size.Width - QuestionEditMargin.Right, Padding.Top);
            CollapseButton.Click += (sender, args) =>
            {
                ResponseEdit.IsCollapsed = true;
                ExpandButton.Enabled = true;
                CollapseButton.Enabled = false;
                RecalcSize();
            };
            CollapseButton.Enabled = true;
            Controls.Add(CollapseButton);

            Size szOptional = TextRenderer.MeasureText("Optional", System.Drawing.SystemFonts.DialogFont) + new Size(10, 4);
            OptionalRect = new Rectangle(CollapseButton.Location.X - szOptional.Width - QuestionEditMargin.Right, 0, szOptional.Width, szOptional.Height);
            Size szFormat = TextRenderer.MeasureText("Text Format", System.Drawing.SystemFonts.DialogFont) + new Size(10, 4);
            _FormatRect = new Rectangle(OptionalRect.Left - szFormat.Width - QuestionEditMargin.Right, 0, szFormat.Width, szFormat.Height);
            this.SizeChanged += (sender, args) =>
            {
                RecalcSize();
            };
            this.HandleCreated += (sender, args) => RecalcSize();
            this.AutoScaleMode = AutoScaleMode.Dpi;
        }

        public void EndSurveyItemTextFormat()
        {
            CIAT.Dispatcher.RemoveListener<ISurveyItemFormatChanged>(SetSurveyItemFormat);
            bFormatting = false;
            Invalidate();
        }

        protected override void OnActivate(bool BecomingActive)
        {
            base.OnActivate(BecomingActive);
            ResponseEdit.OnActivate(BecomingActive);
            Parent.Invalidate();
        }

        private void CreateResponse(CResponse.EResponseType ResponseType)
        {
            switch (ResponseType)
            {
                case CResponse.EResponseType.Likert:
                    ResponseEdit = new LikertDisplay();
                    break;

                case CResponse.EResponseType.Boolean:
                    ResponseEdit = new TrueFalseDisplay();
                    break;

                case CResponse.EResponseType.Multiple:
                    ResponseEdit = new MultiChoiceDisplay();
                    break;

                case CResponse.EResponseType.WeightedMultiple:
                    ResponseEdit = new WeightedMultiChoiceDisplay();
                    break;

                case CResponse.EResponseType.MultiBoolean:
                    ResponseEdit = new MultipleSelectionDisplay();
                    break;

                case CResponse.EResponseType.BoundedLength:
                    ResponseEdit = new BoundedLengthDisplay();
                    break;

                case CResponse.EResponseType.BoundedNum:
                    ResponseEdit = new BoundedNumberDisplay();
                    break;

                case CResponse.EResponseType.FixedDig:
                    ResponseEdit = new FixedDigitDisplay();
                    break;

                case CResponse.EResponseType.Date:
                    ResponseEdit = new DateResponseDisplay();
                    break;

                case CResponse.EResponseType.RegEx:
                    ResponseEdit = new RegExResponseDisplay();
                    break;
            }
        }

        private void InitializeResponseEdit()
        {
            ResponseEdit?.Dispose();
            CreateResponse(SurveyItem.Response.ResponseType);
            Controls.Add(ResponseEdit);
            ResponseEdit.SetResponse(SurveyItem.Response);
            ResponseEdit.Format = SurveyItem.Response.Format;
            ResponseEdit.Size = new Size(this.Size.Width - ResponseEditMargin.Horizontal - Padding.Horizontal, 1);
            ResponseEdit.Location = new Point(Padding.Left + ResponseEditMargin.Left, QuestionEdit.Bottom + QuestionEditMargin.Bottom + ResponseEditMargin.Top);
            ResponseEdit.BorderStyle = BorderStyle.None;
            ResponseEdit.BackColor = this.BackColor;
            bOptional = SurveyItem.Optional;
            ResponseEdit.IsCollapsed = false;
            IsInitialized = true;
        }

        void ExpandButton_Click(object sender, EventArgs e)
        {
        }

        void CollapseButton_Click(object sender, EventArgs e)
        {
            ResponseEdit.IsCollapsed = true;
            Controls.Remove(ResponseEdit);
            CollapseButton.Enabled = false;
            ExpandButton.Enabled = true;
            RecalcSize();
        }

        protected void SizeQuestionEdit(bool force)
        {
            if (QuestionEdit == null)
                return;
            QuestionEdit.Width = this.ClientSize.Width - QuestionEditMargin.Horizontal - Padding.Horizontal - QuestionEditPadding.Horizontal;
            QuestionEdit.Font = DisplayFont;
            SizeF szF = TextRenderer.MeasureText((QuestionEdit.Text == String.Empty) ? "Qy" : QuestionEdit.Text, DisplayFont, new Size(Size.Width - QuestionEdit.Padding.Horizontal, 0),
                TextFormatFlags.TextBoxControl | TextFormatFlags.WordBreak);
            double LineHeight = ((double)DisplayFont.FontFamily.GetLineSpacing(DisplayFont.Style) / (double)DisplayFont.FontFamily.GetEmHeight(DisplayFont.Style)) *
                        Format.FontSizeAsPixels;
            int nLines = (int)Math.Round((double)szF.Height / LineHeight);
            if (nLines == 0)
                nLines = 1;
            if ((nLines != QuestionLines) || force)
            {
                QuestionLines = nLines;
                QuestionEdit.Size = new Size(QuestionEdit.Width, (int)Math.Ceiling(nLines * LineHeight) + QuestionEdit.Margin.Vertical + (QuestionEdit.Focused ? 2 : 0));
                if (!force && Interlocked.Equals(RecalculatingSize, 0))
                    RecalcSize();
            }
        }

        protected int RecalculatingSize = 0;
        public override void RecalcSize()
        {
            if ((QuestionEdit == null) || (ResponseEdit == null))
                return;
            Action recalcAction = () =>
            {
                if (Controls.Contains(ResponseEdit) && ResponseEdit.IsCollapsed)
                    Controls.Remove(ResponseEdit);
                else if (!Controls.Contains(ResponseEdit) && !ResponseEdit.IsCollapsed)
                    Controls.Add(ResponseEdit);
                SizeQuestionEdit(false);
                if (Controls.Contains(ResponseEdit))
                {
                    ResponseEdit.Width = this.Width - ResponseEditMargin.Horizontal;
                    this.Height = Padding.Vertical + QuestionEdit.Height + QuestionEditMargin.Vertical + ResponseEdit.Height + ResponseEditMargin.Vertical;
                    ResponseEdit.Location = new Point(Padding.Left + ResponseEditMargin.Left, QuestionEdit.Bottom);
                }
                else
                    this.Height = Padding.Vertical + QuestionEdit.Height + QuestionEditMargin.Vertical + ResponseEditMargin.Vertical;
                ExpandButton.Location = new Point(this.Size.Width - ExpandButton.Size.Width - QuestionEditMargin.Right, Padding.Top);
                CollapseButton.Location = new Point(ExpandButton.Location.X - CollapseButton.Size.Width - QuestionEditMargin.Right, Padding.Top);
                Size szOptional = TextRenderer.MeasureText("Optional", System.Drawing.SystemFonts.DialogFont) + new Size(10, 4);
                OptionalRect = new Rectangle(CollapseButton.Location.X - szOptional.Width - QuestionEditMargin.Right, 0, szOptional.Width, szOptional.Height);
                Size szFormat = TextRenderer.MeasureText("Format Text", System.Drawing.SystemFonts.DialogFont) + new Size(10, 4);
                _FormatRect = new Rectangle(OptionalRect.Left - szFormat.Width - QuestionEditMargin.Right, 0, szFormat.Width, szFormat.Height);
                ((SurveyDisplay)Parent)?.RecalcSize();
                Invalidate();
            };
            if (Interlocked.CompareExchange(ref RecalculatingSize, 1, 0) == 0)
            {
                if (!IsHandleCreated)
                {
                    HandleCreated += (s, args) =>
                    {
                        recalcAction();
                        if (Interlocked.Equals(RecalculatingSize, 2))
                            recalcAction();
                        RecalculatingSize = 0;
                    };
                }
                else
                {
                    Task.Run(() =>
                    {
                        this.Invoke(recalcAction);
                        if (Interlocked.Equals(RecalculatingSize, 2))
                            this.Invoke(recalcAction);
                        RecalculatingSize = 0;
                    });
                }
            }
            else
                Interlocked.CompareExchange(ref RecalculatingSize, 2, 1);
        }

        protected override void SurveyItemDisplay_Paint(object sender, PaintEventArgs e)
        {
            Brush br;
            Pen pen;
            System.Drawing.Color rectColor, outlineColor;
//            if (Active)
                outlineColor = Color.Chartreuse;
  //          else
    //            outlineColor = Color.Transparent;
            Brush backBr = new SolidBrush(this.BackColor);
            br = new SolidBrush(outlineColor);
            pen = new Pen(br);
            e.Graphics.DrawRectangle(pen, new Rectangle(0, Padding.Top + CollapseButton.Size.Height >> 1, this.Size.Width - 1, this.Size.Height - Padding.Top - CollapseButton.Size.Height));
     //       if (Active)
       //     {
                if (bFormatHover || bFormatting)
                    e.Graphics.FillRectangle(Brushes.CornflowerBlue, FormatRect);
                else
                    e.Graphics.FillRectangle(Brushes.White, FormatRect);
                e.Graphics.DrawRectangle(pen, FormatRect);
                e.Graphics.DrawString("Format Text", System.Drawing.SystemFonts.DialogFont, Brushes.Black, new PointF(FormatRect.Left + 5, FormatRect.Top + 2));
                if (!bOptional)
                    e.Graphics.FillRectangle(backBr, OptionalRect);
                else
                    e.Graphics.FillRectangle(Brushes.CornflowerBlue, OptionalRect);
                e.Graphics.DrawRectangle(pen, OptionalRect);
                e.Graphics.DrawString("Optional", System.Drawing.SystemFonts.DialogFont, Brushes.Black, new PointF(OptionalRect.Left + 5, OptionalRect.Top + 2));
         //   }
            if (bOptional)
            {
                e.Graphics.FillRectangle(Brushes.CornflowerBlue, OptionalRect);
                e.Graphics.DrawString("Optional", System.Drawing.SystemFonts.DialogFont, Brushes.Black, new PointF(OptionalRect.Left + 5, OptionalRect.Top + 2));
            }
            backBr.Dispose();
            br.Dispose();
            pen.Dispose();
        }
    }
}

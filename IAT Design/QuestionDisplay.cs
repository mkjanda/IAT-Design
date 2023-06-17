using System;
using System.Collections.Concurrent;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace IATClient
{
    class QuestionDisplay : SurveyItemDisplay
    {
        private Font displayFont = null;
        protected static readonly Padding QuestionEditMargin = new Padding(10, 16, 30, 6), QuestionEditPadding = new Padding(5, 5, 10, 5);
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
                    _Format = new SurveyItemFormat(SurveyItemFormat.EFor.Item);
                    DisplayFont = new Font(_Format.FontFamily, _Format.FontSizeAsPixels, _Format.FontStyle);
                }
                return _Format;
            }
            protected set
            {
                _Format = value;
                DisplayFont = new Font(_Format.FontFamily, _Format.FontSizeAsPixels, _Format.FontStyle);
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
                        displayFont = new Font(Format.FontFamily, Format.FontSizeAsPixels * CIATLayout.yDpi / 96F, Format.FontStyle, GraphicsUnit.Pixel);
                    return displayFont;
                }
            }
            private set
            {
                if (displayFont == value)
                    return;
                lock (lockObj)
                {
                    var f = displayFont;
                    displayFont = value;
                    if (IsHandleCreated)
                        this.Invoke(new Action(() =>
                        {
                            QuestionEdit.Font = displayFont;
                            SizeQuestionEdit(true);
                            RecalcSize(false);
                            f?.Dispose();
                            Invalidate();
                        }));
                    else
                    {
                        f?.Dispose();
                    }
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
                _ItemType = _SurveyItem.Response.ResponseType;
                SuspendLayoutCalculations();
                Format = value.Format;
                
                if (value.Text == String.Empty)
                    QuestionEdit.Text = DefaultQuestionEditText;
                else
                    QuestionEdit.Text = value.Text;
                QuestionEdit.ForeColor = value.Format.Color;
                InitializeResponseEdit();
                SizeQuestionEdit(true);
                RecalcSize(true);
                ResumeLayoutCalculations();
                ResponseEdit.Format = value.Response.Format;

            }
        }

        public SurveyItemFormat ResponseFormat
        {
            get
            {
                return ResponseEdit?.Format;
            }
            set
            {
                ResponseEdit.Format = value;
            }
        }

        private void SetSurveyItemFormat(ISurveyItemFormatChanged e)
        {
            this.BeginInvoke(new Action(() =>
            {
                if (e.SurveyItemFormat.For == SurveyItemFormat.EFor.Item)
                {
                    Format = e.SurveyItemFormat;
                    if (_SurveyItem != null)
                        _SurveyItem.Format = e.SurveyItemFormat;
                    QuestionEdit.ForeColor = Format.Color;
                    SizeQuestionEdit(true);
                    ResumeLayout();
                }
                else
                {
                    ResponseEdit.Format = e.SurveyItemFormat;
                    ResponseFormat = e.SurveyItemFormat;
                    if (_SurveyItem != null)
                        if (_SurveyItem.Response != null)
                            _SurveyItem.Response.Format = e.SurveyItemFormat;
                }
            }));
        }

        public QuestionDisplay()
        {
            IsInitialized = false;
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
            ResponseEdit = null;
            this.HorizontalScroll.Enabled = false;
            this.HorizontalScroll.Visible = false;
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
                if (FormatRect.Contains(args.Location))
                {
                    if (bFormatHover == false)
                    {
                        bFormatHover = true;
                        Invalidate();
                    }
                }
                else if (bFormatHover == true)
                {
                    bFormatHover = false;
                    Invalidate();
                }
            };
            MouseClick += (sender, args) =>
            {
                ((SurveyDisplay)Parent)?.SelectionChanged(this, ModifierKeys);
                if (OptionalRect.Contains(args.Location))
                {
                    bOptional = !bOptional;
                }
                else if (FormatRect.Contains(args.Location))
                {
                    bFormatting = true;
                    CIAT.Dispatcher.Dispatch<IFormatSurveyItemText>(new CFormatSurveyItemText(this));
                    CIAT.Dispatcher.AddListener<ISurveyItemFormatChanged>(SetSurveyItemFormat);
                }
                Invalidate();
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
            QuestionEdit.TextChanged += (sender, args) =>
            {
                if (IsHandleCreated)
                {
                    SuspendLayout();
                    RecalcSize(false);
                    if (_SurveyItem != null)
                        _SurveyItem.Text = QuestionEdit.Text;
                    ResumeLayout(false);
                }
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
            ExpandButton.Location = new Point(this.Size.Width - ExpandButton.Size.Width + 10, Padding.Top);
            ExpandButton.Click += (sender, args) =>
            {
                ResponseEdit.IsCollapsed = false;
                ExpandButton.Enabled = false;
                CollapseButton.Enabled = true;
                RecalcSize(true);
            };
            ExpandButton.Enabled = false;
            Controls.Add(ExpandButton);

            CollapseButton = new Button();
            CollapseButton.BackColor = this.BackColor;
            CollapseButton.Image = Properties.Resources.CollapseArrow;
            CollapseButton.Size = Properties.Resources.CollapseArrow.Size + new Size(4, 4);
            CollapseButton.Location = new Point(ExpandButton.Location.X - CollapseButton.Size.Width + 10, Padding.Top);
            CollapseButton.Click += (sender, args) =>
            {
                ResponseEdit.IsCollapsed = true;
                ExpandButton.Enabled = true;
                CollapseButton.Enabled = false;
                RecalcSize(false);
            };
            CollapseButton.Enabled = true;
            Controls.Add(CollapseButton);
            Size szOptional = TextRenderer.MeasureText("Optional", System.Drawing.SystemFonts.DialogFont) + new Size(10, 4);
            OptionalRect = new Rectangle(CollapseButton.Location.X - szOptional.Width - 10, 0, szOptional.Width, szOptional.Height);
            Size szFormat = TextRenderer.MeasureText("Text Format", System.Drawing.SystemFonts.DialogFont) + new Size(10, 4);
            _FormatRect = new Rectangle(OptionalRect.Left - szFormat.Width - 10, 0, szFormat.Width, szFormat.Height);
            Format = new SurveyItemFormat(SurveyItemFormat.EFor.Item);
            this.HandleCreated += (sender, args) => RecalcSize(true);
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
            ResponseEdit.Size = new Size(this.Size.Width - ResponseEditMargin.Horizontal - Padding.Horizontal, 1);
            ResponseEdit.Format = SurveyItem.Response.Format;
            ResponseEdit.Location = new Point(Padding.Left + ResponseEditMargin.Left, QuestionEdit.Bottom + QuestionEditMargin.Bottom + ResponseEditMargin.Top);
            ResponseEdit.BorderStyle = BorderStyle.None;
            ResponseEdit.BackColor = this.BackColor;
            ResponseEdit.SetResponse(SurveyItem.Response);
            bOptional = SurveyItem.Optional;
            ResponseEdit.IsCollapsed = false;
            IsInitialized = true;
        }

        protected void SizeQuestionEdit(bool bForce = true)
        {
            if (QuestionEdit == null)
                return;
            QuestionEdit.Width = this.Width - Padding.Horizontal - QuestionEditPadding.Horizontal - QuestionEditMargin.Horizontal;
            QuestionEdit.Font = DisplayFont;
            Size szQ = TextRenderer.MeasureText((QuestionEdit.Text == String.Empty) ? "Qy" : QuestionEdit.Text, DisplayFont, new Size(Size.Width - QuestionEdit.Padding.Horizontal, 0),
                TextFormatFlags.TextBoxControl | TextFormatFlags.WordBreak);
            if (QuestionEdit.Focused)
                szQ.Height += 2;
            double LineHeight = ((double)DisplayFont.FontFamily.GetLineSpacing(DisplayFont.Style) / (double)DisplayFont.FontFamily.GetEmHeight(DisplayFont.Style)) *
                        Format.FontSizeAsPixels;
            int nLines = (int)Math.Round((double)szQ.Height / LineHeight);
            if (nLines == 0)
                nLines = 1;
            //            if ((nLines != QuestionLines) || force)
            //          {
            bool resize = (nLines != QuestionLines);
            QuestionLines = nLines;
            //                QuestionEdit.Height = szQ.Height; 
            QuestionEdit.Height = TextRenderer.MeasureText((QuestionEdit.Text == String.Empty) ? "Qy" : QuestionEdit.Text, DisplayFont, new Size(QuestionEdit.Width, 0),
                TextFormatFlags.WordBreak | TextFormatFlags.TextBoxControl).Height + QuestionEditPadding.Top;


            //       if (resize)
            //   RecalcSize(false);
            //        }
        }

        protected int RecalculatingSize = 0;
        private ManualResetEvent recalcEvt = new ManualResetEvent(true);
        private readonly static object doRecalcChildren = new object(), doNotRecalcChildren = new object(), recalcProcessed = new object(),
            noRecalcsPending = new object();

        private readonly ConcurrentQueue<object> recalcQueue = new ConcurrentQueue<object>();
        private object RecalcChildren = noRecalcsPending;
        private object recalcLock = new object();
        public override Task<int> RecalcSize(bool recalcChildren)
        {
            if ((QuestionEdit == null) || (ResponseEdit == null))
                return Task.Run(() => 0);
            Func<int> f = () =>
            {
                this.Invoke(new Action(() =>
                {
                    this.Width = Parent.Width - QuestionEditMargin.Horizontal;
                    QuestionEdit.Width = Parent.Width - QuestionEditMargin.Horizontal - QuestionEditPadding.Horizontal - Padding.Horizontal;
                    RecalcChildren = recalcChildren ? doRecalcChildren : doNotRecalcChildren;
                    QuestionEdit.Font = DisplayFont;
                    if (ResponseEdit.IsCollapsed)
                    {
                        if (Controls.Contains(ResponseEdit))
                            Controls.Remove(ResponseEdit);
                        this.Height = Padding.Vertical + QuestionEdit.Height + QuestionEditMargin.Vertical + QuestionEditPadding.Vertical;
                        return;
                    }
                    else if (!Controls.Contains(ResponseEdit) && (!ResponseEdit.IsCollapsed))
                        Controls.Add(ResponseEdit);
                    SizeQuestionEdit(false);
                    if (RecalcChildren.Equals(doRecalcChildren))
                    {
                        ResponseEdit.Width = this.Width - ResponseEditMargin.Horizontal;
                        ResponseEdit.Location = new Point(Padding.Left + ResponseEditMargin.Left, QuestionEdit.Bottom);
                        ResponseEdit.LayoutControl();
                        this.Height = Padding.Vertical + QuestionEdit.Height + QuestionEditMargin.Vertical + ResponseEdit.Height + ResponseEditMargin.Vertical;
                    }
                    else if (RecalcChildren.Equals(doNotRecalcChildren))
                    {
                        ResponseEdit.Location = new Point(Padding.Left + ResponseEditMargin.Left, QuestionEdit.Bottom);
                        this.Height = Padding.Vertical + QuestionEdit.Height + QuestionEditMargin.Vertical + ResponseEdit.Height + ResponseEditMargin.Vertical;
                        Task.Run(() => (Parent as SurveyDisplay).RecalcSize(false));
                    }
                    RecalcChildren = recalcProcessed;
                    ExpandButton.Location = new Point(this.Size.Width - ExpandButton.Size.Width - QuestionEditMargin.Right, Padding.Top);
                    CollapseButton.Location = new Point(ExpandButton.Location.X - CollapseButton.Size.Width - 10, Padding.Top);
                    Size szOptional = TextRenderer.MeasureText("Optional", System.Drawing.SystemFonts.DialogFont) + new Size(10, 4);
                    OptionalRect = new Rectangle(CollapseButton.Location.X - szOptional.Width - 10, 0, szOptional.Width, szOptional.Height);
                    Size szFormat = TextRenderer.MeasureText("Format Text", System.Drawing.SystemFonts.DialogFont) + new Size(10, 4);
                    _FormatRect = new Rectangle(OptionalRect.Left - szFormat.Width - 10, 0, szFormat.Width, szFormat.Height);
                }));
                return Height;
            };
            return Task.Run(f);
        }

        protected override void SurveyItemDisplay_Paint(object sender, PaintEventArgs e)
        {
            Brush br;
            Pen pen;
            System.Drawing.Color rectColor, outlineColor;
            outlineColor = Color.Chartreuse;
            Brush backBr = new SolidBrush(this.BackColor);
            br = new SolidBrush(outlineColor);
            pen = new Pen(br);
            e.Graphics.DrawRectangle(pen, new Rectangle(0, Padding.Top + CollapseButton.Size.Height >> 1, this.Size.Width - Padding.Right, this.Size.Height - Padding.Top - CollapseButton.Size.Height));
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

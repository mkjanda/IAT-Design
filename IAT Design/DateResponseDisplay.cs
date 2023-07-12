using System;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace IATClient
{
    class DateResponseDisplay : ResponseDisplay
    {
        private MonthCalendar StartDate, EndDate;
        private Rectangle StartDateRect, EndDateRect, LabelRect;
        private CheckBox EnableStartDate, EnableEndDate;
        private String Label;
        private const int LinePadding = 5;
        private int DatePickerWidth = 125;
        private const int DatePickerOuterMargin = 100;
        private const int DateCheckOuterMargin = 100;
        private static string[] LabelMessages = { Properties.Resources.sDateResponseNeitherSpecified, Properties.Resources.sDateResponseStartSpecified,
                                                    Properties.Resources.sDateResponseEndSpecified, Properties.Resources.sDateResponseBothSpecified };

        protected override CResponse GetResponse()
        {
            CDateResponse r = new CDateResponse();
            if (EnableStartDate.Checked)
                r.StartDate = StartDate.SelectionStart;
            if (EnableEndDate.Checked)
                r.EndDate = EndDate.SelectionStart;
            return r;
        }

        public override void SetResponse(CResponse response)
        {
            CDateResponse dr = (CDateResponse)response;
            EnableStartDate.Checked = dr.HasStartDate;
            if (dr.HasStartDate)
            {
                StartDate.SelectionStart = dr.StartDate;
                StartDate.SelectionEnd = dr.StartDate;
            }
            else
            {
                StartDate.SelectionStart = DateTime.Now;
                StartDate.SelectionEnd = DateTime.Now;
            }
            EnableEndDate.Checked = dr.HasEndDate;
            if (dr.HasEndDate)
            {
                EndDate.SelectionStart = dr.EndDate;
                EndDate.SelectionEnd = dr.EndDate;
            }
            else
            {
                EndDate.SelectionStart = DateTime.Now;
                EndDate.SelectionEnd = DateTime.Now;
            }
        }

        protected override void ChangeResponseFont()
        {
            EnableStartDate.Font = DisplayFont;
            EnableEndDate.Font = DisplayFont;
            base.ChangeResponseFont();
        }

        public DateResponseDisplay()
        {
            StartDate = new MonthCalendar();
            EndDate = new MonthCalendar();
            EnableStartDate = new CheckBox();
            EnableEndDate = new CheckBox();
            // init start date controls
            EnableStartDate.Text = Properties.Resources.sEnableStartDateLabel;
            EnableStartDate.BackColor = Color.White;
            EnableStartDate.ForeColor = System.Drawing.Color.Gray;
            EnableStartDate.Location = new Point(Padding.Left + DateCheckOuterMargin, Padding.Top + DisplayFont.Height + LinePadding);
            EnableStartDate.AutoSize = true;
            EnableStartDate.TextAlign = ContentAlignment.MiddleLeft;
            EnableStartDate.Font = DisplayFont;
            Controls.Add(EnableStartDate);
            EnableStartDate.CheckedChanged += (sender, args) =>
            {
                CDateResponse resp = SurveyItem.Response as CDateResponse;
                resp.HasStartDate = EnableStartDate.Checked;
                if (!EnableStartDate.Checked)
                {
                    resp.StartDate = DateTime.MaxValue;
                    if (!resp.HasEndDate)
                        Label = LabelMessages[0];
                    else
                        Label = LabelMessages[2];
                }
                else
                {
                    if (!resp.HasEndDate)
                        Label = LabelMessages[1];
                    else
                        Label = LabelMessages[3];
                }
                Invalidate();
            };
            StartDate.BackColor = this.BackColor;
            StartDate.MaxSelectionCount = 1;
            Label = LabelMessages[0];
            StartDate.Location = new Point(Size.Width - Padding.Right - DatePickerOuterMargin - DatePickerWidth, Padding.Top + DisplayFont.Height + LinePadding);
            StartDate.Size = new Size(DatePickerWidth, StartDate.Padding.Vertical + DisplayFont.Height);
            StartDate.MouseLeave += new EventHandler(MonthCalendar_LostFocus);
            StartDateRect = new Rectangle(StartDate.Location, StartDate.Size);
            Controls.Add(StartDate);
            StartDate.Visible = false;
            StartDate.LostFocus += new EventHandler(MonthCalendar_LostFocus);

            // init end date controls
            EndDate.BackColor = Color.White;
            EnableEndDate.BackColor = Color.White;
            Controls.Add(EndDate);
            EndDate.MouseLeave += new EventHandler(MonthCalendar_LostFocus);
            EndDate.LostFocus += new EventHandler(MonthCalendar_LostFocus);
            EndDate.Visible = false;
            EndDate.MaxSelectionCount = 1;
            EndDate.Location = new Point(Size.Width - Padding.Right - DatePickerOuterMargin - DatePickerWidth, Padding.Top + DisplayFont.Height + EnableStartDate.Height + (LinePadding * 2));
            EndDate.Size = new Size(DatePickerWidth, EndDate.Padding.Vertical + DisplayFont.Height);
            EndDateRect = new Rectangle(EndDate.Location, EndDate.Size);
            EnableEndDate.Text = Properties.Resources.sEnableEndDateLabel;
            EnableEndDate.Font = DisplayFont;
            EnableEndDate.ForeColor = System.Drawing.Color.Gray;
            EnableEndDate.TextAlign = ContentAlignment.MiddleLeft;
            EnableEndDate.AutoSize = true;
            Controls.Add(EnableEndDate);
            EnableEndDate.Location = new Point(Padding.Left + DateCheckOuterMargin, EndDate.Location.Y);
            EnableEndDate.CheckedChanged += (sender, args) =>
            {
                CDateResponse resp = SurveyItem.Response as CDateResponse;
                resp.HasEndDate = EnableEndDate.Checked;
                if (!EnableEndDate.Checked)
                {
                    resp.EndDate = DateTime.Now;
                    if (!resp.HasEndDate)
                        Label = LabelMessages[0];
                    else
                        Label = LabelMessages[1];
                }
                else
                {
                    if (!resp.HasEndDate)
                        Label = LabelMessages[2];
                    else
                        Label = LabelMessages[3];
                }
                Invalidate();
            };
            this.MouseMove += new MouseEventHandler(DateResponseDisplay_MouseMove);
        }

        public override async void LayoutControl()
        {
            Action a = () =>
            {
                if (!Monitor.TryEnter(lockObj))
                    return;
                LayoutEvent.WaitOne();
                LayoutEvent.Reset();
                Monitor.Exit(lockObj);
                this.Invoke(new Action(() =>
                {
                    LabelRect = new Rectangle(new Point(InteriorPadding.Left, InteriorPadding.Top), TextRenderer.MeasureText(Label, DisplayFont, new Size(Size.Width - InteriorPadding.Horizontal, 0), TextFormatFlags.WordBreak));
                    EndDate.Location = new Point(Size.Width - Padding.Right - DatePickerOuterMargin - DatePickerWidth, LabelRect.Bottom + Padding.Top + EnableStartDate.Height + (LinePadding * 2));
                    EndDateRect = new Rectangle(EndDate.Location, EndDate.Size);
                    EnableStartDate.Location = new Point(Padding.Left + DateCheckOuterMargin, LabelRect.Bottom + LinePadding);
                    EnableStartDate.AutoSize = true;
                    EnableEndDate.Location = new Point(Padding.Left + DateCheckOuterMargin, LabelRect.Bottom + EnableStartDate.Height + LinePadding * 2);
                    EnableEndDate.AutoSize = true;
                    StartDate.Location = new Point(Size.Width - Padding.Right - DatePickerOuterMargin - DatePickerWidth, LabelRect.Bottom + LinePadding);
                    StartDateRect = new Rectangle(StartDate.Location, StartDate.Size);
                    Size sz = this.Size;
                    sz.Height = InteriorPadding.Vertical + LabelRect.Height + LinePadding;
                    if (StartDate.Visible)
                    {
                        sz.Height += StartDate.Height + LinePadding;
                        Point pt = EnableEndDate.Location;
                        pt.Y = sz.Height;
                        EnableEndDate.Location = pt;
                        pt = EndDate.Location;
                        pt.Y = sz.Height;
                        EndDate.Location = pt;
                        sz.Height += EnableEndDate.Height;
                    }
                    else
                    {
                        sz.Height += EnableStartDate.Height + LinePadding;
                        Point pt = EnableEndDate.Location;
                        pt.Y = sz.Height;
                        EnableEndDate.Location = pt;
                        pt = EndDate.Location;
                        pt.Y = sz.Height;
                        EndDate.Location = pt;
                        if (EndDate.Visible)
                            sz.Height += EndDate.Height;
                        else
                            sz.Height += EnableEndDate.Height;
                    }
                    this.Size = sz;
                    StartDateRect.Size = TextRenderer.MeasureText(StartDate.SelectionStart.ToString("MMMM d, yyyy"), DisplayFont);
                    EndDateRect.Size = TextRenderer.MeasureText(EndDate.SelectionStart.ToString("MMMM d, yyyy"), DisplayFont);
                    LayoutEvent.Set();
                }));
                (Parent as QuestionDisplay).RecalcSize(false);
            };
            await Task.Run(a);
        }

        void StartDate_MouseLeave(object sender, EventArgs e)
        {
            StartDate.Visible = false;
            LayoutControl();
        }

        private void On_MouseClick(object sender, MouseEventArgs e)
        {
            OnSelected(ModifierKeys);
        }

        void MonthCalendar_LostFocus(object sender, EventArgs e)
        {
            LayoutControl();
            CDateResponse resp = SurveyItem.Response as CDateResponse;
            if (sender == StartDate)
                resp.StartDate = StartDate.SelectionRange.Start;
            else
                resp.EndDate = EndDate.SelectionRange.Start;
        }

        void EnableDate_CheckedChanged(object sender, EventArgs e)
        {
            if (EnableStartDate.Checked)
            {
                if (EnableEndDate.Checked)
                    Label = LabelMessages[3];
                else
                    Label = LabelMessages[1];
            }
            else
            {
                if (EnableEndDate.Checked)
                    Label = LabelMessages[2];
                else
                    Label = LabelMessages[0];
            }
            LayoutControl();
            Invalidate();
        }

        void DateResponseDisplay_MouseMove(object sender, MouseEventArgs e)
        {
            bool Visible = StartDate.Visible;
            if ((StartDateRect.Contains(e.Location)) && EnableStartDate.Checked)
                StartDate.Visible = true;
            else
                StartDate.Visible = false;
            if (Visible != StartDate.Visible)
            {
                LayoutControl();
                Invalidate();
            }

            Visible = EndDate.Visible;
            if ((EndDateRect.Contains(e.Location)) && EnableEndDate.Checked)
                EndDate.Visible = true;
            else
                EndDate.Visible = false;
            if (Visible != EndDate.Visible)
            {
                LayoutControl();
                Invalidate();
            }
        }

        public override void OnActivate(bool BecomingActive)
        {
            if (!BecomingActive)
            {
                StartDate.Visible = false;
                EndDate.Visible = false;
                LayoutControl();
            }
        }

        protected override void ResponseDisplay_Paint(object sender, PaintEventArgs e)
        {
            Brush br = new SolidBrush(System.Drawing.Color.Gray);
            Brush dispBr = new SolidBrush(Format.Color);
            e.Graphics.DrawString(Label, DisplayFont, br, LabelRect, StringFormat.GenericTypographic);
            if (EnableStartDate.Checked)
                e.Graphics.DrawString("Start date:", DisplayFont, br, StartDate.Location - new Size(TextRenderer.MeasureText("Start date: ", DisplayFont).Width, 0));
            if ((EnableStartDate.Checked) && (!StartDate.Visible))
                e.Graphics.DrawString(StartDate.SelectionStart.ToString("MMMM d, yyyy"), DisplayFont, dispBr, StartDate.Location);
            if (EnableEndDate.Checked)
                e.Graphics.DrawString("End date:", DisplayFont, br, EndDate.Location - new Size(TextRenderer.MeasureText("End date: ", DisplayFont).Width, 0));
            if ((EnableEndDate.Checked) && (!EndDate.Visible))
                e.Graphics.DrawString(EndDate.SelectionStart.ToString("MMMM d, yyyy"), DisplayFont, dispBr, EndDate.Location);
            br.Dispose();
            dispBr.Dispose();
        }
    }
}

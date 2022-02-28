using System;
using System.Drawing;
using System.Windows.Forms;

namespace IATClient
{
    interface IReportableException
    {
        void Draw(Graphics g, Font f, Point location);
        String Caption { get; }
        String ExceptionMessage { get; }
        String GetText();
        Size MeasureText(Graphics g, Font f);
    }

    class ErrorReportDisplay : Form
    {
        private IReportableException TheException;
        private Label ErrorLabel = new Label();
        private Label ErrorText = new Label();
        private TextBox InstructionsTextBox;
        private Panel HistoryPanel = new Panel();
        private GroupBox HistoryGroup = new GroupBox();
        private Button OKButton, CopyToClipboardButton;
        private TextBox InstructionsBox;
        private Panel HistFace;
        private Size HistFaceSize = Size.Empty;
        private String[] StackLines = null;

        public ErrorReportDisplay(IReportableException ex)
        {
            InstructionsTextBox = new TextBox();
            this.Text = ex.Caption;
            TheException = ex;
            this.Shown += new EventHandler(ErrorReportDisplay_Shown);
            this.Size = new Size(700, 550);
            InstructionsTextBox.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            InstructionsTextBox.Text = "Please click the Copy to Clipboard button below and paste the following in an email addressed to michael@iatsoftware.net";
            InstructionsTextBox.Location = new Point(20, 3);
            InstructionsTextBox.ReadOnly = true;
            InstructionsTextBox.BorderStyle = BorderStyle.None;
            InstructionsTextBox.Multiline = true;
            InstructionsTextBox.Font = new Font(System.Drawing.SystemFonts.DialogFont.FontFamily, System.Drawing.SystemFonts.DialogFont.Size + 1, FontStyle.Bold);
            InstructionsTextBox.Size = TextRenderer.MeasureText("Please click the Copy to Clipboard button below and paste the following in an email addressed to admin@iatsoftware.net",
                InstructionsTextBox.Font, new Size(this.Width - 40, 0), TextFormatFlags.WordBreak | TextFormatFlags.TextBoxControl);
            InstructionsTextBox.AcceptsTab = false;
            InstructionsTextBox.TabStop = false;
            Controls.Add(InstructionsTextBox);
            ErrorLabel.Location = new Point(3, InstructionsTextBox.Bottom + 3);
            ErrorLabel.Text = "The following error occurred while attempting to fill your request:";
            ErrorLabel.Font = System.Drawing.SystemFonts.DialogFont;
            ErrorLabel.Size = TextRenderer.MeasureText(ErrorLabel.Text, ErrorLabel.Font);
            Controls.Add(ErrorLabel);
            ErrorText.Text = TheException.ExceptionMessage;
            ErrorText.Font = System.Drawing.SystemFonts.DialogFont;
            ErrorText.Location = new Point(18, ErrorLabel.Bottom + 3);
            ErrorText.Size = TextRenderer.MeasureText(ErrorText.Text, ErrorText.Font);
            Controls.Add(ErrorText);
            HistoryGroup = new GroupBox();
            HistoryGroup.Location = new Point(3, ErrorText.Bottom + 3);
            HistoryGroup.Text = "Transaction History";
            HistoryGroup.Size = new Size(this.ClientRectangle.Width - 12, this.ClientRectangle.Height - 50 - ErrorText.Bottom);
            HistoryGroup.Controls.Add(HistoryPanel);
            Controls.Add(HistoryGroup);
            HistoryGroup.Anchor = AnchorStyles.Bottom | AnchorStyles.Right | AnchorStyles.Left | AnchorStyles.Top;
            HistoryGroup.SizeChanged += new EventHandler(HistoryGroup_SizeChanged);
            CopyToClipboardButton = new Button();
            CopyToClipboardButton.Click += new EventHandler(CopyToClipboardButton_Click);
            CopyToClipboardButton.Text = "Copy to Clipboard";
            CopyToClipboardButton.Width = TextRenderer.MeasureText(CopyToClipboardButton.Text, System.Drawing.SystemFonts.DialogFont).Width + 10;
            OKButton = new Button();
            OKButton.Text = "OK";
            OKButton.Click += new EventHandler(OKButton_Click);
            OKButton.Width = CopyToClipboardButton.Width;
            int whiteSpace = this.Width - 2 * CopyToClipboardButton.Width;
            OKButton.Location = new Point((int)(whiteSpace * .75 / 3), this.ClientRectangle.Height - 36);
            OKButton.Anchor = AnchorStyles.Bottom;
            Controls.Add(OKButton);
            OKButton.TabIndex = 0;
            CopyToClipboardButton.Location = new Point((int)(OKButton.Right + .25 * whiteSpace), OKButton.Top);
            Controls.Add(CopyToClipboardButton);
            CopyToClipboardButton.Anchor = AnchorStyles.Bottom;
            CopyToClipboardButton.TabIndex = 1;
            InstructionsBox = new TextBox();
            Font measureFont = new Font(System.Drawing.SystemFonts.DialogFont.FontFamily, .975F * System.Drawing.SystemFonts.DialogFont.Size);
            InstructionsBox.Size = TextRenderer.MeasureText(Properties.Resources.sErrorReportInstructions, measureFont, new Size(HistoryGroup.Width, 0),
                TextFormatFlags.TextBoxControl | TextFormatFlags.WordBreak | TextFormatFlags.LeftAndRightPadding);
            measureFont.Dispose();
            HistFace = new Panel();
            HistoryPanel.Controls.Add(HistFace);
            HistFace.Paint += new PaintEventHandler(HistFace_Paint);
            HistFace.Size = new Size(100, 100);
            HistoryPanel.Location = new Point(3, 18);
            HistoryPanel.Anchor = AnchorStyles.Bottom | AnchorStyles.Right | AnchorStyles.Top | AnchorStyles.Left;
            HistFace.Location = new Point(0, 0);
            HistoryPanel.AutoScroll = true;
        }

        void HistoryGroup_SizeChanged(object sender, EventArgs e)
        {
            HistoryPanel.Size = new Size(HistoryGroup.Width - 6, HistoryGroup.Height - 21);
        }

        void HistFace_Paint(object sender, PaintEventArgs e)
        {
            PointF ptDraw = new PointF(5F, 5F);
            Brush br = new SolidBrush(HistFace.BackColor);
            e.Graphics.FillRectangle(br, HistFace.ClientRectangle);
            br.Dispose();
            if (HistFaceSize == Size.Empty)
            {
                HistFaceSize = TheException.MeasureText(e.Graphics, SystemFonts.DialogFont) + new Size(10, 10);
                HistFace.Size = HistFaceSize;
            }
            TheException.Draw(e.Graphics, SystemFonts.DialogFont, new Point(5, 5));
        }

        private void ErrorReportDisplay_Shown(object sender, EventArgs e)
        {
            HistoryPanel.Location = new Point(3, 18);
            HistoryPanel.Size = new Size(HistoryGroup.Width - 6, HistoryGroup.Height - 21);
            HistFace.Location = new Point(0, 0);
            HistFace.Invalidate();
        }

        void CopyToClipboardButton_Click(object sender, EventArgs e)
        {
            Clipboard.SetText(TheException.GetText());
        }

        void OKButton_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}

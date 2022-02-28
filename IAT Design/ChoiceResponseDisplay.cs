using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;
namespace IATClient
{
    abstract class ChoiceResponseDisplay : ResponseDisplay
    {
        protected List<TextBox> ChoiceEdits;
        protected List<Button> ChoiceDeleteButtons;
        private List<int> NumLinesInChoices;
        protected Button AddChoiceButton;
        private bool CanAddChoices;
        private Image DeleteButtonImage = new Bitmap(Properties.Resources.DeleteItem);
        protected Size ChoicesSize { get; set; } = Size.Empty;

        private Padding ChoiceEditPadding
        {
            get
            {
                return GetChoiceEditPadding();
            }
        }

        protected override void ChangeResponseFont()
        {
            foreach (TextBox tb in ChoiceEdits)
            {
                tb.Font = DisplayFont;
                tb.ForeColor = Format.Color;
            }
            base.ChangeResponseFont();
            Invalidate();
            (Parent as QuestionDisplay).RecalcSize(false);
        }

        protected abstract String GetChoiceDefaultText(TextBox sender);

        abstract protected Padding GetChoiceEditPadding();

        public ChoiceResponseDisplay(bool canAddChoices)
        {
            AddChoiceButton = new Button();
            ChoiceDeleteButtons = new List<Button>();
            ChoiceEdits = new List<TextBox>();
            CanAddChoices = canAddChoices;
            this.BorderStyle = BorderStyle.FixedSingle;
            this.ForeColor = System.Drawing.Color.Gray;
            Padding = InteriorPadding;
            this.TabStop = false;
            if (CanAddChoices)
            {
                AddChoiceButton.ForeColor = System.Drawing.Color.Blue;
                AddChoiceButton.Text = "Add Choice";
                AddChoiceButton.BackColor = this.BackColor;
                AddChoiceButton.Location = new Point(this.Size.Width - AddChoiceButton.Size.Width - this.Padding.Right,
                    this.Size.Height - AddChoiceButton.Size.Height - this.Padding.Bottom);
                Controls.Add(AddChoiceButton);
                AddChoiceButton.Click += new EventHandler(AddChoiceButton_Click);
            }
            else
            {
                AddChoiceButton.Size = new Size(0, 0);
                AddChoiceButton.Location = new Point(this.Size.Width - Padding.Right, this.Size.Height - Padding.Bottom);
            }
        }

        protected void CreateChoiceEdit(String str)
        {
            TextBox box = new TextBox();
            box.Font = DisplayFont;
            box.FontChanged += (sender, args) =>
            {
                box.Invalidate();
            };
            box.MouseHover += (sender, args) =>
            {
                TextBox b = (TextBox)sender;
                b.ForeColor = Format.Color;
                b.BackColor = Color.LightBlue;
                b.BorderStyle = BorderStyle.FixedSingle;
            };
            box.MouseLeave += (sender, args) =>
            {
                TextBox b = (TextBox)sender;
                if (!b.Focused)
                {
                    b.ForeColor = Format.Color;
                    b.BackColor = this.BackColor;
                    b.BorderStyle = BorderStyle.None;
                }
            };
            box.Leave += (sender, args) =>
            {
                TextBox b = (TextBox)sender;
                b.ForeColor = Format.Color;
                b.BackColor = this.BackColor;
                b.BorderStyle = BorderStyle.None;
                int ndx = ChoiceEdits.IndexOf(b);
                if (b.Text == String.Empty)
                    b.Text = GetChoiceDefaultText(b);
            };
            box.Enter += (sender, args) =>
            {
                TextBox b = sender as TextBox;
                b.ForeColor = Format.Color;
                b.BackColor = Color.LightBlue;
                int ndx = ChoiceEdits.IndexOf((TextBox)sender);
                if (b.Text == GetChoiceDefaultText(b))
                    b.Text = String.Empty;
                if (Parent != null)
                    ((QuestionDisplay)Parent).Active = true;
            };
            box.Multiline = true;
            box.TabStop = true;
            box.Text = str;
            box.TextChanged += new EventHandler(Choice_TextChanged);
            box.MouseClick += new MouseEventHandler(ResponseDisplay_MouseClick);
            box.ForeColor = Format.Color;
            box.BorderStyle = BorderStyle.None;
            ChoiceEdits.Add(box);
            /*            int boxWidth = Size.Width - InteriorPadding.Horizontal - ChoiceEditPadding.Horizontal;
                        int textHeight = TextRenderer.MeasureText(box.Text, DisplayFont, new Size(boxWidth, 1),
                                TextFormatFlags.WordBreak | TextFormatFlags.TextBoxControl).Height;
                        int lineHeight = (int)(DisplayFont.FontFamily.GetLineSpacing(DisplayFont.Style) * DisplayFont.Size / DisplayFont.FontFamily.GetEmHeight(FontStyle.Regular));
                        int nLines = (int)Math.Floor((double)textHeight / (double)lineHeight);
                        int boxHeight = nLines * lineHeight + 6;
                        int boxX = InteriorPadding.Left + ChoiceEditPadding.Left;
                        int boxY = this.Size.Height - InteriorPadding.Bottom - ChoiceEditPadding.Vertical - box.Size.Height;
                        box.Location = new Point(boxX, boxY);
                        box.Size = new Size(boxWidth, boxHeight); */
            Controls.Add(box);
            if (CanAddChoices)
                CreateDeleteButton();

            Invalidate();
        }


        private bool SizeChoiceEdit(TextBox box, bool force)
        {
            int ndx = ChoiceEdits.IndexOf(box);
            /*            int boxWidth = Size.Width - InteriorPadding.Horizontal - ChoiceEditPadding.Horizontal;
                        int textHeight = TextRenderer.MeasureText(box.Text, DisplayFont, new Size(boxWidth, 1),
                                TextFormatFlags.WordBreak | TextFormatFlags.TextBoxControl).Height;
                        int lineHeight = (int)(DisplayFont.FontFamily.GetLineSpacing(DisplayFont.Style) * DisplayFont.Size / DisplayFont.FontFamily.GetEmHeight(FontStyle.Regular));
                        int nLines = (int)Math.Floor((double)textHeight / (double)lineHeight);
                        int boxHeight = (int)(nLines * lineHeight * 1.428);
                        box.Size = new Size(boxWidth, boxHeight);
                        return true;
                    }
            */
            Size szF = Size.Empty;
            int nLines = 1;
            box.Width = Size.Width - InteriorPadding.Horizontal - ChoiceEditPadding.Horizontal;
            szF = (box.Text == String.Empty) ? new Size(box.Width, box.Height) : TextRenderer.MeasureText(box.Text, DisplayFont,
                new Size(box.Width - InteriorPadding.Horizontal - ChoiceEditPadding.Horizontal, 0),
                TextFormatFlags.TextBoxControl | TextFormatFlags.WordBreak);
            double lineHeight = ((double)(DisplayFont.FontFamily.GetCellAscent(DisplayFont.Style) + DisplayFont.FontFamily.GetCellDescent(DisplayFont.Style))
                / (double)DisplayFont.FontFamily.GetEmHeight(DisplayFont.Style)) * Format.FontSizeAsPixels;
            nLines = (int)Math.Floor((double)(szF.Height - box.Margin.Vertical) / (double)DisplayFont.Size);
            box.Width = Size.Width - InteriorPadding.Horizontal - ChoiceEditPadding.Horizontal;
            //                NumLinesInChoices[ndx] = nLines;
            box.Height = szF.Height + (box.Focused ? 4 : 0);
            box.ResumeLayout(false);
            box.Invalidate();
            return true;
        }

        private void Choice_TextChanged(object sender, EventArgs e)
        {
            TextBox box = (TextBox)sender;
            UpdateChoiceText(ChoiceEdits.IndexOf(box), box.Text);
            if (SizeChoiceEdit(box, false))
                LayoutControl();
        }

        private void CreateDeleteButton()
        {
            Button button = new Button();
            button.Image = DeleteButtonImage;
            button.BackColor = this.BackColor;
            button.ForeColor = System.Drawing.Color.Red;
            button.Click += new EventHandler(DeleteChoiceButton_Click);
            button.Size = button.Image.Size + new Size(2, 2);
            button.Location = new Point(this.Size.Width - button.Size.Width, ChoiceEdits[ChoiceEdits.Count - 1].Location.Y);
            ChoiceDeleteButtons.Add(button);
            Controls.Add(button);
            button.MouseClick += new MouseEventHandler(ResponseDisplay_MouseClick);
            if (Parent != null)
                button.Visible = ((QuestionDisplay)Parent).Active;
        }

        protected virtual void DeleteChoiceButton_Click(object sender, EventArgs e)
        {
            Button button = (Button)sender;
            int ndx = ChoiceDeleteButtons.IndexOf(button);
            Controls.Remove(button);
            ChoiceDeleteButtons.Remove(button);
            Controls.Remove(ChoiceEdits[ndx]);
            ChoiceEdits.RemoveAt(ndx);
            RemoveChoiceFromResponse(ndx);
            LayoutControl();
            Invalidate();
        }

        public override void OnActivate(bool BecomingActive)
        {
            for (int ctr = 0; ctr < ChoiceDeleteButtons.Count; ctr++)
                ChoiceDeleteButtons[ctr].Visible = BecomingActive;
        }

        protected void ClearChoices()
        {
            for (int ctr = 0; ctr < ChoiceEdits.Count; ctr++)
                RemoveChoiceFromResponse(ctr);
            foreach (var edit in ChoiceEdits)
            {
                Controls.Remove(edit);
                edit.Dispose();
            }
            foreach (var delete in ChoiceDeleteButtons)
            {
                Controls.Remove(delete);
                delete.Dispose();
            }
            ChoiceEdits.Clear();
            ChoiceDeleteButtons.Clear();
            LayoutControl();
            Invalidate();
        }

        protected async void LayoutChoices()
        {
            Action a = () =>
            {
                this.Invoke(new Action(() =>
                {
                    AddChoiceButton.Size = System.Windows.Forms.TextRenderer.MeasureText("Add Choice", AddChoiceButton.Font)
                        + new Size(16, 8);
                    int minChoiceHeight = int.MaxValue;
                    int height = 0;
                    for (int ctr = 0; ctr < ChoiceEdits.Count; ctr++)
                    {
                        ChoiceEdits[ctr].Location = new Point(InteriorPadding.Left + ChoiceEditPadding.Left,
                            height + ChoiceEditPadding.Vertical);
                        SizeChoiceEdit(ChoiceEdits[ctr], false);
                        height += ChoiceEdits[ctr].Height + ChoiceEditPadding.Vertical;
                        if (CanAddChoices)
                        {
                            ChoiceDeleteButtons[ctr].Location = new Point(Size.Width - InteriorPadding.Right - ChoiceDeleteButtons[ctr].Width,
                                ChoiceEdits[ctr].Top + ((ChoiceEdits[ctr].Size.Height - ChoiceDeleteButtons[ctr].Height) >> 1));
                            ChoiceEdits[ctr].TabIndex = ctr;
                        }

                        if (ChoiceEdits[ctr].Height < minChoiceHeight)
                            minChoiceHeight = ChoiceEdits[ctr].Height;
                    }
                    if (minChoiceHeight < DeleteButtonImage.Height)
                    {
                        DeleteButtonImage.Dispose();
                        DeleteButtonImage = new Bitmap(Properties.Resources.DeleteItem, new Size(minChoiceHeight, minChoiceHeight));
                        ChoiceDeleteButtons.ForEach((b) => { b.Size = DeleteButtonImage.Size + new Size(2, 2); b.Image = DeleteButtonImage; });
                    }
                    height += AddChoiceButton.Height + ChoiceEditPadding.Vertical;
                    if (ChoiceEdits.Count > 0)
                        AddChoiceButton.Location = new Point(this.Size.Width - AddChoiceButton.Size.Width - this.Padding.Right,
                            ChoiceEdits[ChoiceEdits.Count - 1].Bottom + ChoiceEditPadding.Vertical);
                    else
                        AddChoiceButton.Location = new Point(this.Width - AddChoiceButton.Width - Padding.Right, ChoiceEditPadding.Vertical + Padding.Top);
                    ChoicesSize = new Size(this.Width, height);
                }));
            };
            await Task.Run(a);
        }



        abstract protected void AddChoiceButton_Click(object sender, EventArgs e);
        abstract protected void UpdateChoiceText(int ndx, String text);
        abstract protected void RemoveChoiceFromResponse(int ndx);
    }
}

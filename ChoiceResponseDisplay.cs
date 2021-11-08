using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using System.Threading;

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
                SizeChoiceEdit(tb, true);
            }
            base.ChangeResponseFont();
        }

        protected abstract String GetChoiceDefaultText(TextBox sender);

        abstract protected Padding GetChoiceEditPadding();

        public ChoiceResponseDisplay(bool canAddChoices)
        {
            AddChoiceButton = new Button();
            ChoiceDeleteButtons = new List<Button>();
            ChoiceEdits = new List<TextBox>();
            NumLinesInChoices = new List<int>();
            CanAddChoices = canAddChoices;
            UpdatingFromCode = false;
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
            box.TextChanged += new EventHandler(Choice_TextChanged);
            box.MouseClick += new MouseEventHandler(ResponseDisplay_MouseClick);
            box.Font = DisplayFont;
            box.ForeColor = Format.Color;
            box.BorderStyle = BorderStyle.None;
            NumLinesInChoices.Add(1);
            ChoiceEdits.Add(box);
            Controls.Add(box);
            if (CanAddChoices)
                CreateDeleteButton();
            SizeChoiceEdit(box, true);
            box.Location = new Point(InteriorPadding.Left + ChoiceEditPadding.Left,
                this.Size.Height - InteriorPadding.Bottom - ChoiceEditPadding.Vertical - box.Size.Height);
            box.Text = str;
            Invalidate();
        }


        private bool SizeChoiceEdit(TextBox box, bool force)
        {
            int ndx = ChoiceEdits.IndexOf(box);
            SizeF szF = TextRenderer.MeasureText((box.Text == String.Empty) ? "Qy" : box.Text, DisplayFont, 
                new Size(Size.Width - InteriorPadding.Horizontal - ChoiceEditPadding.Horizontal, 0),
                TextFormatFlags.TextBoxControl | TextFormatFlags.WordBreak);
            double LineHeight = ((double)DisplayFont.FontFamily.GetLineSpacing(DisplayFont.Style) / (double)DisplayFont.FontFamily.GetEmHeight(DisplayFont.Style)) *
                        Format.FontSizeAsPixels;
            int nLines = (int)Math.Round((double)szF.Height / LineHeight);
            if (nLines == 0)
                nLines = 1;
            if ((nLines != NumLinesInChoices[ndx]) || force)
            {
                NumLinesInChoices[ndx] = nLines;
                box.Size = new Size(Size.Width - InteriorPadding.Horizontal - ChoiceEditPadding.Horizontal,
                    (int)Math.Ceiling(NumLinesInChoices[ndx] * LineHeight) + box.Margin.Vertical + (box.Focused ? 2 : 0));
                return true;
            }
            return false;
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
            NumLinesInChoices.RemoveAt(ndx);
            Controls.Remove(ChoiceEdits[ndx]);
            ChoiceEdits.RemoveAt(ndx);
            RemoveChoiceFromResponse(ndx);
            LayoutControl();
            Invalidate();
        }

        public override void OnActivate(bool BecomingActive)
        {
            SuspendLayout();
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
            NumLinesInChoices.Clear();
            if (!UpdatingFromCode)
            {
                LayoutControl();
                Invalidate();
            }
        }

        private int LayingOutChoices = 0;
        protected Size LayoutChoices()
        {
            Func<int> f = () =>
            {
                AddChoiceButton.Size = System.Windows.Forms.TextRenderer.MeasureText("Add Choice", AddChoiceButton.Font)
                    + new Size(8, 8);
                int minChoiceHeight = int.MaxValue;
                int height = 0;
                for (int ctr = 0; ctr < ChoiceEdits.Count; ctr++)
                {
                    ChoiceEdits[ctr].Location = new Point(InteriorPadding.Left + ChoiceEditPadding.Left, height + ChoiceEditPadding.Vertical);
                    SizeChoiceEdit(ChoiceEdits[ctr], false);
                    height += ChoiceEdits[ctr].Height + ChoiceEditPadding.Vertical;
                    ChoiceEdits[ctr].Width = Size.Width - InteriorPadding.Horizontal - ChoiceEditPadding.Horizontal;
                    if (CanAddChoices)
                        ChoiceDeleteButtons[ctr].Location = new Point(Size.Width - InteriorPadding.Right - ChoiceDeleteButtons[ctr].Width,
                            ChoiceEdits[ctr].Top + ((ChoiceEdits[ctr].Size.Height - ChoiceDeleteButtons[ctr].Height) >> 1));
                    ChoiceEdits[ctr].TabIndex = ctr;
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
                (Parent as QuestionDisplay).RecalcSize();
                return height;
            };
            if (Interlocked.CompareExchange(ref LayingOutChoices, 1, 0) == 0)
            {
                int height = f();
                if (Interlocked.Equals(2, LayingOutChoices))
                    height = f();
                LayingOutChoices = 0;
                return new Size(this.Width, height);
            }
            else
                Interlocked.CompareExchange(ref LayingOutChoices, 2, 1);
            return Size.Empty;
        }
    


        abstract protected void AddChoiceButton_Click(object sender, EventArgs e);
        abstract protected void UpdateChoiceText(int ndx, String text);
        abstract protected void RemoveChoiceFromResponse(int ndx);
    }
}

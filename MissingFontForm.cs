using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace IATClient
{
    public partial class MissingFontForm : Form
    {
        private MissingFontDisplay MissingFontDisplay;
        private Button ReplaceFontsButton = new Button();

        public MissingFontForm(CFontFile.FontItem []missingFonts)
        {
            InitializeComponent();
            this.AutoScroll = true;
            this.MissingFontDisplay = new MissingFontDisplay(missingFonts, 1000);
            this.Width = 1020;
            this.MissingFontDisplay.Location = new Point(0, Instructions.Bottom + 20);
            this.MissingFontDisplay.SizeChanged += new EventHandler(MissingFontDisplay_SizeChanged);
            Controls.Add(this.MissingFontDisplay);
            this.Controls.Add(ReplaceFontsButton);
            ReplaceFontsButton.Text = "Replace Fonts";
            ReplaceFontsButton.AutoSize = true;
            ReplaceFontsButton.Location = new Point((this.Width - ReplaceFontsButton.Width) >> 1, this.MissingFontDisplay.Bottom + 20);
            ReplaceFontsButton.DialogResult = DialogResult.OK;
            ReplaceFontsButton.Click += new EventHandler(ReplaceFontsButton_Click);
            this.FormClosing += new FormClosingEventHandler(MissingFontForm_Closing);
        }

        private void MissingFontDisplay_SizeChanged(object sender, EventArgs e)
        {
            this.ReplaceFontsButton.Top = this.MissingFontDisplay.Bottom + 20;
        }

        private void MissingFontForm_Closing(object sender, FormClosingEventArgs e)
        {
            if (this.MissingFontDisplay.ContainsDefaultFonts())
            {
                if (MessageBox.Show(Properties.Resources.sMissingFontFormContainsDefaultFont, "Confirm", MessageBoxButtons.YesNo) == DialogResult.No)
                {
                    e.Cancel = true;
                    return;
                }
            }
        }

        private void ReplaceFontsButton_Click(object sender, EventArgs e)
        {
                    return;
            this.Close();
        }

        public String[] GetReplacementFontFamilies()
        {
            return this.MissingFontDisplay.ReplacementFontFamilies;
        }
    }
}

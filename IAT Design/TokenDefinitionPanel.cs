using System;
using System.Drawing;
using System.Windows.Forms;

namespace IATClient
{
    public enum ETokenType
    {
        NONE, VALUE, HEX, BASE64, BASE64_UTF8
    }

    class TokenDefinitionPanel : Panel
    {
        private CheckBox UsesTokenCheck;
        private GroupBox TokenDefinitionGroup;
        private RadioButton ValueRadio, HexRadio, Base64Radio, Base64Utf8Radio;
        private TextBox TokenExplanation, TypesExplanation, TokenNameBox;
        private Label TokenNameLabel;
        private static readonly Size PanelSize = new Size(375, 200);

        public TokenDefinitionPanel()
        {
            TokenExplanation = new TextBox();
            TokenExplanation.ReadOnly = true;
            TokenExplanation.Multiline = true;
            TokenExplanation.BorderStyle = BorderStyle.None;
            TokenExplanation.Text = Properties.Resources.sTokenExplanation;
            TokenExplanation.Padding = new Padding(3, 3, 3, 3);
            TokenExplanation.AcceptsTab = false;
            TokenExplanation.Location = new Point(5, 0);
            TokenExplanation.Size = TextRenderer.MeasureText(Properties.Resources.sTokenExplanation, System.Drawing.SystemFonts.DialogFont, new Size(PanelSize.Width, 10), TextFormatFlags.WordBreak);
            TokenExplanation.ReadOnly = true;
            this.Controls.Add(TokenExplanation);

            UsesTokenCheck = new CheckBox();
            UsesTokenCheck.Text = "Includes Token";
            UsesTokenCheck.Checked = false;
            UsesTokenCheck.Location = new Point(15, TokenExplanation.Bottom + 5);
            UsesTokenCheck.AutoSize = true;
            UsesTokenCheck.CheckedChanged += (object sender, EventArgs e) => { if (UsesTokenCheck.Checked) TokenDefinitionGroup.Enabled = true; else TokenDefinitionGroup.Enabled = false; };
            this.Controls.Add(UsesTokenCheck);

            TokenDefinitionGroup = new GroupBox();
            TokenDefinitionGroup.Size = new Size(PanelSize.Width - 10, PanelSize.Height - UsesTokenCheck.Bottom - 15);
            TokenDefinitionGroup.Text = "Token Definition";
            TokenDefinitionGroup.Location = new Point(5, UsesTokenCheck.Bottom + 5);
            this.Controls.Add(TokenDefinitionGroup);

            TypesExplanation = new TextBox();
            TypesExplanation.ReadOnly = true;
            TypesExplanation.BorderStyle = BorderStyle.None;
            TypesExplanation.Text = Properties.Resources.sTokenTypesExplanation;
            TypesExplanation.Location = new Point(6, 18);
            TypesExplanation.AcceptsTab = false;
            TypesExplanation.Multiline = true;
            TypesExplanation.Size = TextRenderer.MeasureText(Properties.Resources.sTokenTypesExplanation, System.Drawing.SystemFonts.DialogFont, new Size(TokenDefinitionGroup.Width - 12, 10), TextFormatFlags.WordBreak);
            TokenDefinitionGroup.Controls.Add(TypesExplanation);

            TokenNameLabel = new Label();
            TokenNameLabel.Text = "Token name:";
            TokenNameLabel.Location = new Point(3, TypesExplanation.Bottom + 8);
            TokenNameLabel.Size = TextRenderer.MeasureText("Token name:", System.Drawing.SystemFonts.DialogFont) + new Size(4, 4);
            TokenDefinitionGroup.Controls.Add(TokenNameLabel);

            TokenNameBox = new TextBox();
            TokenNameBox.Location = new Point(TokenNameLabel.Right + 3, TypesExplanation.Bottom + 5);
            TokenNameBox.Width = 200;
            TokenDefinitionGroup.Controls.Add(TokenNameBox);

            ValueRadio = new RadioButton();
            ValueRadio.Text = "Value";
            ValueRadio.Location = new Point(18, TokenNameBox.Bottom + 5);
            ValueRadio.Checked = true;
            ValueRadio.Width = TextRenderer.MeasureText(ValueRadio.Text, System.Drawing.SystemFonts.DialogFont).Width + 30;
            TokenDefinitionGroup.Controls.Add(ValueRadio);
            /*
                        HexRadio = new RadioButton();
                        HexRadio.Text = "Hexadecimal";
                        HexRadio.Location = new Point(18, ValueRadio.Bottom + 5);
                        HexRadio.Width = TextRenderer.MeasureText(HexRadio.Text, System.Drawing.SystemFonts.DialogFont).Width + 30;
                        TokenDefinitionGroup.Controls.Add(HexRadio);
            */
            Base64Radio = new RadioButton();
            Base64Radio.Text = "Base 64";
            Base64Radio.Location = new Point(18, ValueRadio.Bottom + 5);
            Base64Radio.Width = TextRenderer.MeasureText(Base64Radio.Text, System.Drawing.SystemFonts.DialogFont).Width + 30;
            TokenDefinitionGroup.Controls.Add(Base64Radio);
            /*
                        Base64Utf8Radio = new RadioButton();
                        Base64Utf8Radio.Text = "Base 64 encoded UTF-8 text";
                        Base64Utf8Radio.Location = new Point(18, Base64Radio.Bottom + 5);
                        Base64Utf8Radio.Width = TextRenderer.MeasureText(Base64Utf8Radio.Text, System.Drawing.SystemFonts.DialogFont).Width + 30;
                        TokenDefinitionGroup.Controls.Add(Base64Utf8Radio);
            */
            TokenDefinitionGroup.Size = new Size(TokenDefinitionGroup.Width, Base64Radio.Bottom + 3);
            this.Size = new Size(PanelSize.Width, TokenDefinitionGroup.Bottom + 5);
            TokenDefinitionGroup.Enabled = false;
        }

        public bool UsesToken
        {
            get
            {
                return UsesTokenCheck.Checked;
            }
        }

        public ETokenType TokenType
        {
            get
            {
                if (!UsesTokenCheck.Checked)
                    return ETokenType.NONE;
                else if (ValueRadio.Checked)
                    return ETokenType.VALUE;
                else if (HexRadio.Checked)
                    return ETokenType.HEX;
                else if (Base64Radio.Checked)
                    return ETokenType.BASE64;
                else
                    return ETokenType.BASE64_UTF8;
            }
        }

        public String TokenName
        {
            get
            {
                return TokenNameBox.Text;
            }
        }
    }
}

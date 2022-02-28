using System;
using System.Drawing;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace IATClient
{
    partial class UploadForm : Form
    {
        public static Size ChildControlSize = new Size(750, 200);
        private NumPresentationsControl NumPresentations;
        private static String[] ResponseKeyValues = {"A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L",
                                                        "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W",
                                                        "X", "Y", "Z" };
        private TextBox IATNameBox = null, PasswordBox = null, RedirectOnCompleteBox = null;
        private Label IATNameLabel, PasswordLabel, LeftResponseKeyLabel, RightResponseKeyLabel;
        private const int FormWidth = 385;
        private static Size TextBoxSize = new Size(160, 20);
        private TokenDefinitionPanel TokenPanel;

        private CIAT IAT;

        public String IATName
        {
            get
            {
                if (IATNameBox != null)
                    return IATNameBox.Text;
                return String.Empty;
            }
        }

        public String Password
        {
            get
            {
                if (PasswordBox != null)
                    return PasswordBox.Text;
                return String.Empty;
            }
        }

        public ETokenType TokenType
        {
            get
            {
                return TokenPanel.TokenType;
            }
        }

        public String TokenName
        {
            get
            {
                return TokenPanel.TokenName;
            }
        }

        public UploadForm(CIAT iat, bool askNamePassword)
        {
            DialogResult = System.Windows.Forms.DialogResult.None;
            IAT = iat;
            InitializeComponent();
            Size szText;
            int yOffset = 12;
            if (askNamePassword)
            {
                IATNameBox = new TextBox();
                IATNameBox.Location = new Point(153, yOffset);
                IATNameBox.Size = TextBoxSize;
                Controls.Add(IATNameBox);
                szText = TextRenderer.MeasureText("IAT Name:", System.Drawing.SystemFonts.DialogFont);
                IATNameLabel = new Label();
                IATNameLabel.Location = new Point(145 - szText.Width, yOffset + 3);
                IATNameLabel.Size = szText;
                IATNameLabel.Text = "IAT Name:";
                Controls.Add(IATNameLabel);
                yOffset += IATNameBox.Height + System.Drawing.SystemFonts.DialogFont.Height;
                PasswordBox = new TextBox();
                PasswordBox.Location = new Point(153, yOffset);
                PasswordBox.Size = TextBoxSize;
                Controls.Add(PasswordBox);
                szText = TextRenderer.MeasureText("Password:", System.Drawing.SystemFonts.DialogFont);
                PasswordLabel = new Label();
                PasswordLabel.Location = new Point(145 - szText.Width, yOffset + 3);
                PasswordLabel.Size = szText;
                PasswordLabel.Text = "Password:";
                Controls.Add(PasswordLabel);
                yOffset += PasswordBox.Height + System.Drawing.SystemFonts.DialogFont.Height;
            }
            RedirectOnCompleteBox = new TextBox();
            RedirectOnCompleteBox.Location = new Point(153, yOffset);
            RedirectOnCompleteBox.Size = TextBoxSize;
            Controls.Add(RedirectOnCompleteBox);
            szText = TextRenderer.MeasureText("Redirect On Complete:", System.Drawing.SystemFonts.DialogFont);
            RedirectOnCompleteLabel = new Label();
            RedirectOnCompleteLabel.Location = new Point(145 - szText.Width, yOffset + 3);
            RedirectOnCompleteLabel.Text = "Redirect On Complete:";
            RedirectOnCompleteLabel.Size = szText;
            Controls.Add(RedirectOnCompleteLabel);
            yOffset += RedirectOnCompleteBox.Height + System.Drawing.SystemFonts.DialogFont.Height;
            LeftResponseKeyDrop = new ComboBox();
            LeftResponseKeyDrop.Location = new Point(118, yOffset);
            LeftResponseKeyDrop.Size = new Size(44, 21);
            LeftResponseKeyDrop.DropDownStyle = ComboBoxStyle.DropDownList;
            Controls.Add(LeftResponseKeyDrop);
            LeftResponseKeyLabel = new Label();
            szText = TextRenderer.MeasureText("Left Response Key:", System.Drawing.SystemFonts.DialogFont);
            LeftResponseKeyLabel.Size = szText;
            LeftResponseKeyLabel.Location = new Point(12, yOffset + 3);
            LeftResponseKeyLabel.Text = "Left Response Key:";
            Controls.Add(LeftResponseKeyLabel);
            RightResponseKeyDrop = new ComboBox();
            RightResponseKeyDrop.Location = new Point(301, yOffset);
            RightResponseKeyDrop.Size = new Size(44, 21);
            RightResponseKeyDrop.DropDownStyle = ComboBoxStyle.DropDownList;
            Controls.Add(RightResponseKeyDrop);
            szText = TextRenderer.MeasureText("Right Response Key:", System.Drawing.SystemFonts.DialogFont);
            RightResponseKeyLabel = new Label();
            RightResponseKeyLabel.Size = szText;
            RightResponseKeyLabel.Location = new Point(RightResponseKeyDrop.Left - szText.Width - (LeftResponseKeyDrop.Left - LeftResponseKeyLabel.Right), yOffset + 3);
            RightResponseKeyLabel.Text = "Right Response Key:";
            Controls.Add(RightResponseKeyLabel);
            yOffset += RightResponseKeyDrop.Height + System.Drawing.SystemFonts.DialogFont.Height;
            for (int ctr = 0; ctr < ResponseKeyValues.Length; ctr++)
            {
                LeftResponseKeyDrop.Items.Add(ResponseKeyValues[ctr]);
                RightResponseKeyDrop.Items.Add(ResponseKeyValues[ctr]);
            }
            LeftResponseKeyDrop.SelectedItem = ResponseKeyValues[4];
            RightResponseKeyDrop.SelectedItem = ResponseKeyValues[8];
            NumPresentations = new NumPresentationsControl(iat);
            NumPresentations.Size = new Size(325, ChildControlSize.Height);
            NumPresentations.Location = new Point(30, yOffset);
            Controls.Add(NumPresentations);
            yOffset += NumPresentations.Height + System.Drawing.SystemFonts.DialogFont.Height;
            NumPresentations.OnControlComplete = new Action<DialogResult>(OnControlComplete);
            Okay = new Button();
            Okay.Location = new Point(72, yOffset);
            Okay.Size = new Size(75, 23);
            Okay.Text = "OK";
            Okay.Click += new EventHandler(Okay_Click);
            Controls.Add(Okay);
            Cancel = new Button();
            Cancel.Location = new Point(222, yOffset);
            Cancel.Size = new Size(75, 23);
            Cancel.Text = "Cancel";
            Cancel.Click += new EventHandler(Cancel_Click);
            Controls.Add(Cancel);

            TokenPanel = new TokenDefinitionPanel();
            TokenPanel.Location = new Point(375, 0);
            Controls.Add(TokenPanel);

            this.ClientSize = new Size(ChildControlSize.Width, Math.Max(yOffset + Cancel.Height + System.Drawing.SystemFonts.DialogFont.Height, TokenPanel.Bottom));
            this.FormClosing += UploadForm_FormClosing;
        }

        void UploadForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (DialogResult == System.Windows.Forms.DialogResult.Retry)
                e.Cancel = true;
        }

        void Cancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            this.Close();
        }

        void Okay_Click(object sender, EventArgs e)
        {
            DialogResult = System.Windows.Forms.DialogResult.Retry;
            IAT.LeftResponseChar = (String)LeftResponseKeyDrop.SelectedItem;
            IAT.RightResponseChar = (String)RightResponseKeyDrop.SelectedItem;
            if ((IATNameBox != null) && (IATName == String.Empty))
            {
                MessageBox.Show("Please enter a name for your IAT.");
                return;
            }
            if ((PasswordBox != null) && (Password == String.Empty))
            {
                MessageBox.Show("Please enter a password for your IAT.");
                return;
            }
            for (int ctr = 0; ctr < IATName.Length; ctr++)
                if (!Char.IsLetterOrDigit(IATName[ctr]) && (IATName[ctr] != '_') && (IATName[ctr] != '-'))
                {
                    MessageBox.Show("Your IAT Name may contain only letter, numerical digits, underscores, and hyphens.", "Invalid IAT Name");
                    return;
                }
            if (TokenPanel.UsesToken)
            {
                Regex exp = new Regex(@"[A-Za-z0-9\-_]+");
                if (!exp.IsMatch(TokenPanel.TokenName))
                {
                    MessageBox.Show("Token names can consist only of alphanumeric characters, hyphens, and underscores.", "Invalid Token Name", MessageBoxButtons.OK);
                    return;
                }
            }
            String val = RedirectOnCompleteBox.Text;
            if (!val.StartsWith("http") && !val.StartsWith("https://"))
                IAT.RedirectionURL = "http://" + val;
            else
                IAT.RedirectionURL = val;
            IAT.TokenType = TokenType;
            IAT.TokenName = TokenName;
            for (int ctr = 0; ctr < NumPresentations.NumPresentations.Length; ctr++)
                IAT.Blocks[ctr].NumPresentations = NumPresentations.NumPresentations[ctr];
            DialogResult = DialogResult.OK;
            this.Close();
        }

        private void OnControlComplete(DialogResult result)
        {
            if (result == DialogResult.Cancel)
                this.DialogResult = DialogResult.Cancel;
            else
            {
                IAT.LeftResponseChar = (String)LeftResponseKeyDrop.SelectedItem;
                IAT.RightResponseChar = (String)RightResponseKeyDrop.SelectedItem;
                String val = RedirectOnCompleteBox.Text;
                if (!val.StartsWith("http") && !val.StartsWith("https://"))
                    IAT.RedirectionURL = "http://" + val;
                else
                    IAT.RedirectionURL = val;
                int[] numPresentations = NumPresentations.NumPresentations;
                for (int ctr = 0; ctr < numPresentations.Length; ctr++)
                    IAT.Blocks[ctr].NumPresentations = numPresentations[ctr];
                this.DialogResult = DialogResult.OK;
            }
            Close();
        }
    }
}

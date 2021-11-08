using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;


namespace IATClient
{
    class DataPasswordRequestPanel : Panel
    {
        private TextBox DataPasswordBox;
        private TextBox InstructionsBox;
        private Label DataPasswordLabel;
        private Padding PanelPadding = new Padding(6);
        private Button SubmitButton, CancelButton;
        private Label InvalidPasswordLabel;
        private String IATName;
        private DialogResult _Result;
        public DataRetrievalPanel.ChildControlCloseHandler OnClose = null;

        public String DataPassword
        {
            get
            {
                return DataPasswordBox.Text;
            }
        }

        public DialogResult Result
        {
            get
            {
                return _Result;
            }
        }


        public DataPasswordRequestPanel(String iatName)
        {
            DataPasswordBox = new TextBox();
            DataPasswordBox.Width = 175;
            DataPasswordLabel = new Label();
            DataPasswordLabel.Text = "Password: ";
            DataPasswordLabel.Size = TextRenderer.MeasureText(DataPasswordLabel.Text, DataPasswordLabel.Font);

            InvalidPasswordLabel = new Label();
            InvalidPasswordLabel.Text = "Invalid Password";
            InvalidPasswordLabel.Size = TextRenderer.MeasureText(InvalidPasswordLabel.Text, InvalidPasswordLabel.Font);
            InvalidPasswordLabel.ForeColor = System.Drawing.Color.Red;

            InstructionsBox.ReadOnly = true;
            InstructionsBox.Multiline = true;
            InstructionsBox.Text = "You provided the administrative password to your IAT which cannot be used to decrypt the result data. If you wish view the result data itself, please" 
                + " enter the data retrieval password below.";
            InstructionsBox.Size = TextRenderer.MeasureText(InstructionsBox.Text, InstructionsBox.Font);
            InstructionsBox.BorderStyle = BorderStyle.None;
            InstructionsBox.Size = TextRenderer.MeasureText(InstructionsBox.Text, InstructionsBox.Font, new Size(DataPasswordLabel.Width + DataPasswordBox.Width + PanelPadding.Horizontal, 0));

            InstructionsBox.Location = new Point(PanelPadding.Left, PanelPadding.Top);
            DataPasswordLabel.Location = new Point(PanelPadding.Left, InstructionsBox.Bottom + PanelPadding.Vertical);
            DataPasswordBox.Location = new Point(DataPasswordBox.Right - PanelPadding.Right, InstructionsBox.Bottom + PanelPadding.Vertical);

            this.Size = new Size(DataPasswordBox.Right + PanelPadding.Right, DataPasswordBox.Bottom + PanelPadding.Bottom);

            SubmitButton = new Button();
            SubmitButton.Width = (this.Size.Width - PanelPadding.Horizontal * 3) >> 1;
            SubmitButton.Text = "Submit";
            SubmitButton.Click += new EventHandler(SubmitButton_Click);
            SubmitButton.Location += new Size(PanelPadding.Horizontal, DataPasswordBox.Bottom + PanelPadding.Bottom);

            CancelButton = new Button();
            CancelButton.Width = (this.Size.Width - PanelPadding.Horizontal * 3) >> 1;
            CancelButton.Text = "Cancel";
            CancelButton.Click += new EventHandler(CancelButton_Click);
            CancelButton.Location = new Point(this.Size.Width - PanelPadding.Horizontal - CancelButton.Width, DataPasswordBox.Bottom + PanelPadding.Bottom);

            InvalidPasswordLabel.Location = new Point(this.Size.Width - InvalidPasswordLabel.Width, PanelPadding.Bottom + CancelButton.Bottom);

            this.Height = this.Height + CancelButton.Height + PanelPadding.Vertical;

            Controls.Add(DataPasswordBox);
            Controls.Add(DataPasswordLabel);
            Controls.Add(InstructionsBox);
            Controls.Add(SubmitButton);
            Controls.Add(CancelButton);
        }

        public void SubmitButton_Click(object sender, EventArgs e)
        {
            MySOAP.EstablishEncryption(Properties.Resources.sDataProviderServlet);
            MySOAP.ShakeHands(Properties.Resources.sDataProviderServlet, IATName);
            bool bPassCorrect = (MySOAP.VerifyPassword(IATName, CPartiallyEncryptedRSAKey.EKeyType.Data, DataPassword) == TransactionRequest.ETransaction.TransactionSuccess) ? true : false;
            MySOAP.TerminateConnection(Properties.Resources.sDataProviderServlet);
            if (bPassCorrect)
            {
                Controls.Add(InvalidPasswordLabel);
                return;
            }
            _Result = DialogResult.OK;
            if (OnClose != null)
                OnClose(DataRetrievalPanel.EDataRetrievalChild.DataPassword);
        }

        public void CancelButton_Click(object sender, EventArgs e)
        {
            _Result = DialogResult.Cancel;
            if (OnClose != null)
                OnClose(DataRetrievalPanel.EDataRetrievalChild.DataPassword);
        }
    }
}

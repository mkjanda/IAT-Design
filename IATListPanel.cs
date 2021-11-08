using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;

namespace IATClient
{
    class IATListPanel : Panel
    {
        private List<String> _IATList = new List<String>();
        private List<CIATList.CIATAuthorInfo> _AuthorList = new List<CIATList.CIATAuthorInfo>();
        private int LineHeight;
        private Padding LinePadding = new Padding(8, 3, 8, 3);
        private int _IATsRemaining;
        private decimal _MBRemaining;
        private Label MBLabel, IATLabel;
        private Padding ControlPaddingBottom = new Padding(6);
        private int LabelSpacing = 20;
        private Padding ControlPadding = new Padding(15, 25, 15, 25);
        private Size MinInteriorSize = new Size(175, 275);
        private const int DefaultMaxInteriorHeight = 450;
        private int MaxInteriorHeight;
        private bool bScroll;
        private ScrollBar VertScrollBar;
        private Button SubmitButton, CancelButton;
        private Label PasswordLabel;
        private TextBox PasswordBox;
        private Label AuthorLabel;
        private TextBox AuthorBox;
        private Label EMailLabel;
        private TextBox EMailBox;
        private Panel TestPanel;
        private int SelectedIATNdx = -1;
        public enum EVerificationResult { none, data, admin, both };
        public EVerificationResult _Result = EVerificationResult.none;
        public DataRetrievalPanel.ChildControlCloseHandler OnClose = null;
        private Panel AuthorPanel;


        public EVerificationResult Result
        {
            get {
                return _Result;
            }
        }

        public String IATPassword
        {
            get
            {
                return PasswordBox.Text;
            }
        }

        public String SelectedIAT
        {
            get
            {
                if (SelectedIATNdx == -1)
                    return String.Empty;
                return IATList[SelectedIATNdx];
            }
        }

        public int IATsRemaining
        {
            get {
                return _IATsRemaining;
            }
            set {
                _IATsRemaining = value;
                IATLabel.Text = value.ToString();
            }
        }

        public decimal MBRemaining
        {
            get
            {
                return _MBRemaining;
            }
            set
            {
                _MBRemaining = value;
                MBLabel.Text = value.ToString();
            }
        }

        public List<String> IATList
        {
            get {
                return _IATList;
            }
        }

        public List<CIATList.CIATAuthorInfo> AuthorList
        {
            get {
                return _AuthorList;
            }
        }

        public IATListPanel(List<String>iatList, List<CIATList.CIATAuthorInfo> authorList, int iatsRemaining, decimal mbRemaining)
        {
            MaxInteriorHeight += (System.Drawing.SystemFonts.DefaultFont.Height + LinePadding.Vertical) << 1;
            _IATList = iatList;
            _AuthorList = authorList;
            AuthorPanel = new Panel();
            IATsRemaining = _IATsRemaining;
            MBRemaining = mbRemaining;
            LineHeight = System.Drawing.SystemFonts.DefaultFont.Height + LinePadding.Vertical;
            MBLabel = new Label();
            IATLabel = new Label();
            MBLabel.Text = String.Format("Disk Alottment Remaining (MB): {0:F2}", mbRemaining);
            IATLabel.Text = String.Format("IATs Remaining: {0}", iatsRemaining);
            MBLabel.Size = TextRenderer.MeasureText(MBLabel.Text, MBLabel.Font);
            IATLabel.Size = TextRenderer.MeasureText(IATLabel.Text, IATLabel.Font);
            Size szText = new Size(((MBLabel.Size.Width > IATLabel.Size.Width) ? MBLabel.Size.Width : IATLabel.Size.Width) + LinePadding.Horizontal,
                (MBLabel.Size.Height + IATLabel.Size.Height) + (LinePadding.Vertical << 1));
            Size szIATList = MeasureControl();
            AuthorPanel.Size = new Size(szIATList.Width, ((System.Drawing.SystemFonts.DefaultFont.Height + LinePadding.Vertical) << 1) + (LinePadding.Vertical << 1));
            AuthorPanel.Location = new Point(ControlPadding.Left, TestPanel.Bottom + ControlPadding.Bottom);
            AuthorPanel.BorderStyle = BorderStyle.Fixed3D;
            AuthorLabel = new Label();
            AuthorLabel.Text = "Test Author: ";
            AuthorLabel.Size = TextRenderer.MeasureText(AuthorLabel.Text, System.Drawing.SystemFonts.DefaultFont);
            AuthorLabel.Location = new Point(ControlPadding.Left, LinePadding.Vertical + LinePadding.Top);
            AuthorPanel.Controls.Add(AuthorLabel);
            AuthorBox = new TextBox();
            AuthorBox.ReadOnly = true;
            AuthorBox.BorderStyle = BorderStyle.None;
            AuthorBox.Size = new Size(AuthorPanel.ClientRectangle.Width - ControlPadding.Horizontal - AuthorLabel.Width, System.Drawing.SystemFonts.DefaultFont.Height);
            AuthorBox.Location = new Point(AuthorLabel.Right, AuthorLabel.Top);
            AuthorBox.Text = "(No Test Selected)";
            AuthorPanel.Controls.Add(AuthorBox);
            EMailLabel = new Label();
            EMailLabel.Text = "Author EMail: ";
            EMailLabel.Size = TextRenderer.MeasureText(EMailLabel.Text, System.Drawing.SystemFonts.DefaultFont);
            EMailLabel.Location = new Point(ControlPadding.Left, AuthorLabel.Bottom + LinePadding.Vertical);
            AuthorPanel.Controls.Add(EMailLabel);
            EMailBox = new TextBox();
            EMailBox.ReadOnly = true;
            EMailBox.BorderStyle = BorderStyle.None;
            EMailBox.Size = new Size(AuthorPanel.ClientRectangle.Width - ControlPadding.Horizontal - EMailLabel.Width, System.Drawing.SystemFonts.DefaultFont.Height);
            EMailBox.Text = "(No Test Selected)";
            AuthorPanel.Controls.Add(EMailBox);
            MBLabel.Location = new Point(((this.Width - LabelSpacing - szText.Width) >> 1), LinePadding.Top);
            IATLabel.Location = new Point(((this.Width + LabelSpacing - szText.Width) >> 1), LinePadding.Top);
            this.Size = MeasureControl();
            SubmitButton = new Button();
            SubmitButton.Text = "Submit";
            SubmitButton.Width = this.Width - (3 * LabelSpacing);
            SubmitButton.Location = new Point(LabelSpacing, this.Bottom - SubmitButton.Height - 5);
            SubmitButton.Click += new EventHandler(SubmitButton_Click);
            CancelButton = new Button();
            CancelButton.Text = "Cancel";
            CancelButton.Width = this.Width - (3 * LabelSpacing);
            CancelButton.Location = new Point(SubmitButton.Width + (LabelSpacing * 2), this.Bottom - CancelButton.Height - 5);
            CancelButton.Click += new EventHandler(CancelButton_Click);
            PasswordLabel = new Label();
            PasswordLabel.Text = "Password:";
            szText = TextRenderer.MeasureText(PasswordLabel.Text, PasswordLabel.Font);
            PasswordBox = new TextBox();
            PasswordBox.Width = this.Width - (3 * LabelSpacing) - szText.Width;
            PasswordLabel.Size = szText;
            PasswordLabel.Location = new Point(LabelSpacing, CancelButton.Bottom + LabelSpacing + ((szText.Height < PasswordBox.Height) ? ((PasswordBox.Height - szText.Height) >> 1) : 0));
            PasswordBox.Location = new Point(PasswordLabel.Right + LabelSpacing, CancelButton.Bottom + LabelSpacing + ((szText.Height > PasswordBox.Height) ? ((szText.Height - PasswordBox.Height) >> 1) : 0));
            Controls.Add(SubmitButton);
            Controls.Add(CancelButton);
            Controls.Add(MBLabel);
            Controls.Add(IATLabel);
            Controls.Add(PasswordBox);
            Controls.Add(PasswordLabel);
        }

        void CancelButton_Click(object sender, EventArgs e)
        {
            _Result = EVerificationResult.none;
            if (OnClose != null)
                OnClose(DataRetrievalPanel.EDataRetrievalChild.IATList);
        }

        void SubmitButton_Click(object sender, EventArgs e)
        {
            if ((SelectedIATNdx == -1) || (PasswordBox.Text == String.Empty)) 
                MessageBox.Show("Please select an IAT from the list and enter either the administrative or data retrieval password before clicking Submit.", "Missing IAT or Password");
            MySOAP.EstablishEncryption(Properties.Resources.sDataProviderServlet);
            TransactionRequest inTrans = MySOAP.ShakeHands(Properties.Resources.sDataProviderServlet, IATList[SelectedIATNdx]);
            if (inTrans.Transaction == TransactionRequest.ETransaction.TransactionSuccess)
            {
                MessageBox.Show("Unable to negotiate communications with server.", "Server Error");
                return;
            }
            TransactionRequest trans = new TransactionRequest();
            trans.Transaction = TransactionRequest.ETransaction.AbortTransaction;
            if (MySOAP.VerifyPassword(IATList[SelectedIATNdx], CPartiallyEncryptedRSAKey.EKeyType.Admin, PasswordBox.Text) == TransactionRequest.ETransaction.TransactionSuccess)
                _Result = EVerificationResult.admin;
            if (MySOAP.VerifyPassword(IATList[SelectedIATNdx], CPartiallyEncryptedRSAKey.EKeyType.Data, PasswordBox.Text) == TransactionRequest.ETransaction.TransactionSuccess)
            {
                if (_Result == EVerificationResult.admin)
                    _Result = EVerificationResult.both;
                else
                    _Result = EVerificationResult.data;
                MySOAP.TerminateConnection(Properties.Resources.sDataProviderServlet);
                if (OnClose != null)
                    OnClose(DataRetrievalPanel.EDataRetrievalChild.IATList);
            }
            MySOAP.TerminateConnection(Properties.Resources.sDataProviderServlet);
            MessageBox.Show("The password you entered was not the correct administrative or data retrieval password for the selected IAT.", "Invalid Password");
        }

        public Size MeasureControl()
        {
            int maxWidth = 0;
            for (int ctr = 0; ctr < IATList.Count; ctr++)
            {
                int width = TextRenderer.MeasureText(IATList[ctr], System.Drawing.SystemFonts.DefaultFont).Width;
                width += LinePadding.Horizontal << 1;
                if (width > maxWidth)
                    maxWidth = width;
            }
            int height = (System.Drawing.SystemFonts.DialogFont.Height + LinePadding.Vertical) * IATList.Count;
            if (maxWidth < MinInteriorSize.Width)
                maxWidth = MinInteriorSize.Width;
            if (height < MinInteriorSize.Height)
                height = MinInteriorSize.Height;
            if (height > MaxInteriorHeight)
            {
                bScroll = true;
                VertScrollBar = new VScrollBar();
                VertScrollBar.Height = MaxInteriorHeight;
                VertScrollBar.Minimum = 0;
                VertScrollBar.Maximum = height;
                VertScrollBar.SmallChange = LineHeight;
                VertScrollBar.LargeChange = MaxInteriorHeight;
                VertScrollBar.Value = 0;
                VertScrollBar.ValueChanged += new EventHandler(VertScrollBar_LocationChanged);
                height = MaxInteriorHeight;
            }

            TestPanel = new Panel();
            TestPanel.Size = new Size(maxWidth, height);
            TestPanel.Location = new Point(ControlPadding.Left, ControlPadding.Top);
            if (bScroll)
                TestPanel.Controls.Add(VertScrollBar);
            TestPanel.Paint += new PaintEventHandler(TestPanel_Paint);
            TestPanel.MouseClick += new MouseEventHandler(TestPanel_MouseClick);

            return new Size(maxWidth + ControlPadding.Horizontal, height + ControlPadding.Vertical);
        }

        void TestPanel_MouseClick(object sender, MouseEventArgs e)
        {
            SelectedIATNdx = (int)Math.Floor((double)e.Y / (double)LineHeight);
            if (SelectedIATNdx > IATList.Count)
                SelectedIATNdx = -1;
            TestPanel.Invalidate();
        }

        void TestPanel_Paint(object sender, PaintEventArgs e)
        {
            e.ClipRectangle.Offset(new Point(0, -VertScrollBar.Value));
            e.Graphics.FillRectangle(Brushes.White, new Rectangle(new Point(0, ControlPadding.Bottom + MBLabel.Font.Height + LinePadding.Bottom), new Size(this.Width, this.Height)));
            int y = LineHeight;
            for (int ctr = 0; ctr < IATList.Count; ctr++)
            {
                if (ctr == SelectedIATNdx)
                    e.Graphics.FillRectangle(Brushes.Blue, new Rectangle(new Point(0, y - LineHeight), new Size(this.Width, LineHeight)));
                e.Graphics.DrawLine(Pens.Black, new Point(0, y), new Point(this.Width, y));
                e.Graphics.DrawString(IATList[ctr], System.Drawing.SystemFonts.DefaultFont, Brushes.Black, new PointF(LinePadding.Left, LinePadding.Top + y - LineHeight));
                e.Graphics.DrawString(AuthorList[ctr].FullName, System.Drawing.SystemFonts.DefaultFont, Brushes.Black, new PointF((this.Width >> 1) + LinePadding.Left, y + LinePadding.Top - LineHeight));
                y += LineHeight;
            }
            e.Graphics.DrawLine(Pens.Black, new Point(0, this.Width >> 1), new Point(this.Width >> 1, this.Height));
        }

        void VertScrollBar_LocationChanged(object sender, EventArgs e)
        {
            TestPanel.Invalidate();
        }
    }
}

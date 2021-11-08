using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using IATClient.IATResultSetNamespaceV2;

namespace IATClient
{
    class DataRetrievalPanel : Panel
    {
        public delegate void ChildControlCloseHandler(EDataRetrievalChild childType);

        private Panel TopPanel;
        private Button SyncButton, IATOptionsButton, RetrieveDataButton, SearchDataButton, ViewDataButton;
        private Label PasswordLabel;
        private TextBox PasswordBox;
        private Padding TopPanelElementPadding = new Padding(10, 2, 10, 2);
        private String AdminPassword = String.Empty, DataPassword = String.Empty;
        private const int ButtonHorizPadding = 16;
        private const int ButtonElementMargin = 36;
        private const int PasswordBoxWidth = 120;
        private List<String> IATNames = new List<String>();
        private List<String> AuthorNames = new List<String>();
        private IATListPanel IATList;
        private DataPasswordRequestPanel DataPasswordPanel;
        private IATResultSetList ResultSet;
        private ResultSetDescriptor ResultDescriptor = new ResultSetDescriptor();
        public enum EDataRetrievalChild { IATList, DataPassword };
        public enum EPrivilegeLevel { admin, data, both };
        private EPrivilegeLevel PrivilegeLevel;
        private String IATName;

        public DataRetrievalPanel()
        {
            this.Width = 1010;
            this.Height = 645;
            this.BackColor = System.Drawing.Color.DarkGray;
        }

        private IATConfigMainForm MainForm
        {
            get
            {
                return (IATConfigMainForm)Application.OpenForms[Properties.Resources.sMainFormName];
            }
        }


        public void SetupInitialState()
        {
            Controls.Clear();
            TopPanel = new Panel();
            TopPanel.Size = new Size(1010, 30);
            TopPanel.Location = new Point(0, 0);
            
            String []buttonLabels = {"Synchronize", "IAT Options", "Retrieve Data", "View Data", "Search Data" };
            
            int maxWidth = 0;
            Size sz;
            Size buttonSize = new Size(maxWidth + ButtonHorizPadding, this.Height - TopPanelElementPadding.Bottom);
            for (int ctr = 0; ctr < buttonLabels.Length; ctr++)
            {
                sz = TextRenderer.MeasureText(buttonLabels[ctr], System.Drawing.SystemFonts.DialogFont);
                if (sz.Width > maxWidth)
                    maxWidth = sz.Width;
            }

            SyncButton = new Button();
            SyncButton.Location = new Point(this.Right - buttonSize.Width - TopPanelElementPadding.Right);
            SyncButton.Size = buttonSize;
            SyncButton.Text = buttonLabels[0];
            TopPanel.Controls.Add(SyncButton);

            IATOptionsButton = new Button();
            IATOptionsButton.Location = new Point(SyncButton.Left - buttonSize.Width - ButtonElementMargin, this.Height - TopPanelElementPadding.Bottom);
            IATOptionsButton.Size = buttonSize;
            IATOptionsButton.Text = buttonLabels[1];
            TopPanel.Controls.Add(IATOptionsButton);

            RetrieveDataButton = new Button();
            RetrieveDataButton.Location = new Point(IATOptionsButton.Left - buttonSize.Width - ButtonElementMargin, this.Height - TopPanelElementPadding.Bottom);
            RetrieveDataButton.Size = buttonSize;
            RetrieveDataButton.Text = buttonLabels[2];
            TopPanel.Controls.Add(RetrieveDataButton);

            ViewDataButton = new Button();
            ViewDataButton.Location = new Point(RetrieveDataButton.Left - buttonSize.Width - ButtonElementMargin, this.Height - TopPanelElementPadding.Bottom);
            ViewDataButton.Size = buttonSize;
            ViewDataButton.Text = buttonLabels[3];
            TopPanel.Controls.Add(ViewDataButton);

            SearchDataButton = new Button();
            SearchDataButton.Location = new Point(ViewDataButton.Left - buttonSize.Width - ButtonElementMargin, this.Height - TopPanelElementPadding.Bottom);
            SearchDataButton.Size = buttonSize;
            SearchDataButton.Text = buttonLabels[3];
            TopPanel.Controls.Add(SearchDataButton);

            PasswordBox = new TextBox();
            PasswordBox.Location = new Point(SearchDataButton.Left - PasswordBoxWidth - ButtonElementMargin, this.Height - TopPanelElementPadding.Bottom);
            PasswordBox.Size = new Size(PasswordBoxWidth, TopPanel.Height - TopPanelElementPadding.Bottom);
            PasswordBox.Text = buttonLabels[4];
            TopPanel.Controls.Add(PasswordBox);

            PasswordLabel = new Label();
            sz = TextRenderer.MeasureText("Password: ", System.Drawing.SystemFonts.DefaultFont);
            PasswordLabel.Location = new Point(PasswordBox.Left - sz.Width - ButtonElementMargin, this.Height - TopPanelElementPadding.Bottom);
            PasswordLabel.Font = System.Drawing.SystemFonts.DialogFont;
            PasswordLabel.Size = sz;
            TopPanel.Controls.Add(PasswordLabel);
        }

        public void DoLoadIAT()
        {
            SuspendLayout();
            Controls.Clear();
            Controls.Add(TopPanel);
            ResumeLayout(false);

            MySOAP.EstablishEncryption(Properties.Resources.sDataProviderServlet);
            MySOAP.ShakeHands(Properties.Resources.sDataProviderServlet, String.Empty);
            CIATList iatList = new CIATList();
            TransactionRequest transRequest = new TransactionRequest();
            transRequest.Transaction = TransactionRequest.ETransaction.RequestIATList;
            transRequest.IsLastTransaction = false;
            MySOAP.CallSOAP(Properties.Resources.sDataProviderServlet, Convert.ToInt32(Properties.Resources.sDataProviderPort), MySOAP.ESoapAction.RequestIATList, transRequest, iatList);
            TransactionRequest inTrans = new TransactionRequest();
            transRequest.Transaction = TransactionRequest.ETransaction.RequestRemainingResources;
            transRequest.IsLastTransaction = true;
            MySOAP.CallSOAP(Properties.Resources.sDataProviderServlet, Convert.ToInt32(Properties.Resources.sDataProviderPort), MySOAP.ESoapAction.RequestRemainingResources, transRequest, inTrans);
            IATList = new IATListPanel(iatList.IATNames, iatList.Authors, inTrans.IntValue, inTrans.DecimalValue);
            IATList.Location = new Point((this.Width - IATList.Width) >> 1, TopPanel.Bottom + (this.Height - TopPanel.Bottom - IATList.Height) >> 1);
            IATList.OnClose += new ChildControlCloseHandler(Child_Close);
        }

        private void RetrieveData(String iatName, String password)
        {
            CResultData ResultData = new CResultData(Properties.Resources.sDataProviderServlet, iatName, password);
            ResultData.RetrieveData(this, new CResultData.DataRetrievalCompleteHandler(DataRetrievalComplete));
            ResultSet = ResultData.IATResults;
            ResultDescriptor = ResultData.ResultDescriptor;
        }

        private void ShowDataPasswordPanel(String iatName)
        {
            DataPasswordRequestPanel dataPanel = new DataPasswordRequestPanel(iatName);
            dataPanel.Location = new Point((this.Width - dataPanel.Width) >> 1, TopPanel.Bottom + (this.Height - dataPanel.Height) >> 1);
            Controls.Add(dataPanel);
        }


        private void Child_Close(EDataRetrievalChild childType)
        {
            switch (childType)
            {
                case EDataRetrievalChild.IATList:
                    IATName = IATList.SelectedIAT;
                    Controls.Remove(IATList);
                    if (IATList.Result == IATListPanel.EVerificationResult.admin)
                    {
                        PrivilegeLevel = EPrivilegeLevel.admin;
                        AdminPassword = IATList.IATPassword;
                        ShowDataPasswordPanel(IATName);
                    }
                    else if (IATList.Result == IATListPanel.EVerificationResult.both)
                    {
                        PrivilegeLevel = EPrivilegeLevel.both;
                        AdminPassword = IATList.IATPassword;
                        DataPassword = IATList.IATPassword;
                        RetrieveData(IATList.SelectedIAT, IATList.IATPassword);
                    }
                    else if (IATList.Result == IATListPanel.EVerificationResult.data)
                    {
                        PrivilegeLevel = EPrivilegeLevel.data;
                        DataPassword = IATList.IATPassword;
                        RetrieveData(IATList.SelectedIAT, IATList.IATPassword);
                    }
                    else
                        MainForm.FormContents = IATConfigMainForm.EFormContents.Main;
                    break;

                case EDataRetrievalChild.DataPassword:
                    Controls.Remove(DataPasswordPanel);
                    if (DataPasswordPanel.Result == DialogResult.OK)
                    {
                        PrivilegeLevel = EPrivilegeLevel.both;
                        RetrieveData(IATName, DataPasswordPanel.DataPassword);
                    }
                    else
                    {
                        PrivilegeLevel = EPrivilegeLevel.admin;
                    }
                    break;
            }
        }

        private void DataRetrievalComplete(bool bSuccess, CResultData results)
        {
            if (bSuccess)
                ShowDataPanels();
            else
                DoLoadIAT();
        }

        private void ShowDataPanels()
        {
            
        }
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace IATClient
{
    public partial class TestPackagerPanel : UserControl
    {
        private CIAT IAT;
        private static String[] ResponseKeys = {"A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M",
                                                  "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z" };

        public delegate void TextBoxChangedEventHandler(String str);
        public TextBoxChangedEventHandler OnHostURLChanged, OnIATNameChanged, OnDataRetrievalPasswordChanged;
        private bool bUpdatingFromCode;

        /*
        IATConfigMainForm MainForm
        {
            get
            {
                return (IATConfigMainForm)Parent.Parent;
            }
        }
        */

        public String IATName
        {
            get
            {
                return IATNameEdit.Text;
            }
            set
            {
                IATNameEdit.Text = value;
            }
        }

        IATConfigMainForm MainForm
        {
            get
            {
                if (Parent == null)
                    return null;
                if (Parent.Parent == null)
                    return null;
                return (IATConfigMainForm)Parent.Parent;
            }
        }

        private void DisableNumPresentationsGrid()
        {
            NumPresentationsGrid.Enabled = false;
        }

        private void EnableNumPresentationsGrid()
        {
            NumPresentationsGrid.Enabled = true;
        }

        public TestPackagerPanel()
        {
            OnHostURLChanged = null;
            OnIATNameChanged = null;
            OnDataRetrievalPasswordChanged = null;
            bUpdatingFromCode = true;
            InitializeComponent();
            LeftResponseKey.Items.AddRange(ResponseKeys);
            RightResponseKey.Items.AddRange(ResponseKeys);
            bUpdatingFromCode = false;
        }
        

        private void PackageButton_Click(object sender, EventArgs e)
        {
            if (RedirectEdit.Text.IndexOf("http://") == -1)
                IAT.Packager.RedirectionURL = "http://" + RedirectEdit.Text;
            else
                IAT.Packager.RedirectionURL = RedirectEdit.Text;

            IAT.Packager.DataRetrievalPassword = PasswordEdit.Text;
            IAT.Packager.OnDirectoryExists = new IATClient.IATConfigFile.CIATPackager.TargetDirectoryExistsHandler(TestPackagerPanel_TargetDirectoryExists);

            ProgressWindow progressWindow = new ProgressWindow();
            SaveFileDialog dlg = new SaveFileDialog();
            dlg.Title = Properties.Resources.sSavePackageFileDialogTitle;
            dlg.Filter = Properties.Resources.sPackageFileFilter;
            dlg.DefaultExt = Properties.Resources.sPackageFileExtension;
            dlg.AddExtension = true;
            if (dlg.ShowDialog(this) != DialogResult.OK)
                return;
            
            IAT.Packager.ProcessIAT(System.IO.Path.GetDirectoryName(dlg.FileName), IATName, System.IO.Path.GetFileName(dlg.FileName), progressWindow);
            progressWindow.ShowDialog(MainForm);
            IAT.Packager.OnDirectoryExists = null;

        }

        private bool TestPackagerPanel_IATExists()
        {
            if (MessageBox.Show(MainForm, Properties.Resources.sIATExistsMsg, Properties.Resources.sIATExistsCaption, MessageBoxButtons.YesNo)
                == DialogResult.Yes)
                return true;
            return false;
        }

        private bool TestPackagerPanel_TargetDirectoryExists(String directory)
        {
            if (MessageBox.Show(MainForm, String.Format(Properties.Resources.sPackageDirectoryExistsMsg, directory), Properties.Resources.sPackageDirectoryExistsCaption,
                MessageBoxButtons.YesNo) == DialogResult.Yes)
                return true;
            return false;
        }

        private void ConstructNumPresentationsGrid()
        {
            NumPresentationsGrid.SuspendLayout();
            NumPresentationsGrid.Rows.Clear();
            IContentsItem cItem;
            for (int ctr = 0; ctr < IAT.Contents.Count; ctr++)
            {
                cItem = IAT.Contents[ctr];
                if ((cItem.Type == ContentsItemType.IATBlock) || (cItem.Type == ContentsItemType.IATPracticeBlock))
                    NumPresentationsGrid.Rows.Add(cItem.Name, ((CIATBlock)cItem).NumPresentations.ToString());
            }
            NumPresentationsGrid.ResumeLayout();
        }

        private void InsertNumPresentationsGridRow(int index, IContentsItem item)
        {
            NumPresentationsGrid.Rows.Insert(index, item.Name, ((CIATBlock)item).NumPresentations.ToString());
        }

        public void TestPackagerPanel_OnIATBlockChange(IContentsItem item)
        {
            if (item == null)
                ConstructNumPresentationsGrid();
            int ndx;

            // check to see if deletion is required
            ndx = IAT.Contents.IndexOf(item);
            if (ndx == -1)
            {
                for (int ctr = 0; ctr < NumPresentationsGrid.Rows.Count; ctr++)
                    if (NumPresentationsGrid.Rows[ctr].Cells[0].Value.ToString() == item.Name)
                        NumPresentationsGrid.Rows.RemoveAt(ctr);
                return;
            }

            // check to see if an insertion is required
            /*
            if (NumPresentationsGrid.Rows.Count < IAT.Blocks.Count + IAT.PracticeBlocks.Count)
            {
                int prevItemNdx = ndx;
                bool bFound = false;
                while (prevItemNdx > 0)
                {
                    IContentsItem.EType itemType;
                    itemType = IAT.Contents[--prevItemNdx].Type;
                    if ((itemType == IContentsItem.EType.IATBlock) || (itemType == IContentsItem.EType.IATPracticeBlock))
                    {
                        bFound = true;
                        break;
                    }
                }
                if (bFound)
                    InsertNumPresentationsGridRow(prevItemNdx + 1, item);
                else
                    InsertNumPresentationsGridRow(0, item);
                return;
            }
            */
            // it's a rename or move operation -- rebuild the grid
            ConstructNumPresentationsGrid();
        }

        private void OrderedRadio_CheckedChanged(object sender, EventArgs e)
        {
            IAT.Packager.RandomizationType = IATConfigFile.ConfigFile.ERandomizationType.None;
            ConstructNumPresentationsGrid();
            DisableNumPresentationsGrid();
        }

        private void RandomRadio_CheckedChanged(object sender, EventArgs e)
        {
            IAT.Packager.RandomizationType = IATConfigFile.ConfigFile.ERandomizationType.RandomOrder;
            ConstructNumPresentationsGrid();
            DisableNumPresentationsGrid();
        }

        private void SetPresentationsRadio_CheckedChanged(object sender, EventArgs e)
        {
            IAT.Packager.RandomizationType = IATConfigFile.ConfigFile.ERandomizationType.SetNumberOfPresentations;
            ConstructNumPresentationsGrid();
            EnableNumPresentationsGrid();
        }

        private void NumPresentationsGrid_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            String blockName = NumPresentationsGrid.Rows[e.RowIndex].Cells[0].Value.ToString();
            for (int ctr = 0; ctr < IAT.Contents.Count; ctr++)
                if (IAT.Contents[ctr].Name == blockName)
                    ((CIATBlock)IAT.Contents[ctr]).NumPresentations = 
                        Convert.ToInt32(NumPresentationsGrid.Rows[e.RowIndex].Cells[e.ColumnIndex].Value.ToString());
        }

        public void InitFromIAT(CIAT IAT)
        {
            bUpdatingFromCode = true;
            this.IAT = IAT;
            IATNameEdit.Text = IAT.Packager.IATName;
            switch (IAT.Packager.RandomizationType)
            {
                case IATConfigFile.ConfigFile.ERandomizationType.None:
                    OrderedRadio.Checked = true;
                    break;

                case IATConfigFile.ConfigFile.ERandomizationType.RandomOrder:
                    RandomRadio.Checked = true;
                    break;

                case IATConfigFile.ConfigFile.ERandomizationType.SetNumberOfPresentations:
                    SetPresentationsRadio.Checked = true;
                    break;
            }
            LeftResponseKey.SelectedIndex = LeftResponseKey.Items.IndexOf(IAT.Packager.LeftResponseChar);
            RightResponseKey.SelectedIndex = RightResponseKey.Items.IndexOf(IAT.Packager.RightResponseChar);
            ConstructNumPresentationsGrid();
            bUpdatingFromCode = false;
        }

        public void UpdateNumPresentationsGrid()
        {
            if (IAT.Packager.RandomizationType != IATConfigFile.ConfigFile.ERandomizationType.SetNumberOfPresentations)
                ConstructNumPresentationsGrid();
        }

        private void IATNameEdit_TextChanged(object sender, EventArgs e)
        {
            if (!bUpdatingFromCode)
                IAT.Packager.IATName = IATNameEdit.Text;
            if (OnIATNameChanged != null)
                OnIATNameChanged(IATNameEdit.Text);

        }

        private void LeftResponseKey_SelectedIndexChanged(object sender, EventArgs e)
        {
            IAT.Packager.LeftResponseChar = LeftResponseKey.Items[LeftResponseKey.SelectedIndex].ToString();
        }

        private void RightResponseKey_SelectedIndexChanged(object sender, EventArgs e)
        {
            IAT.Packager.RightResponseChar = RightResponseKey.Items[RightResponseKey.SelectedIndex].ToString();
        }

        private void PasswordEdit_TextChanged(object sender, EventArgs e)
        {
            if (OnDataRetrievalPasswordChanged != null)
                OnDataRetrievalPasswordChanged(PasswordEdit.Text);
            if ((PasswordEdit.Text.Length < 6) || (PasswordEdit.Text.Length > 15))
                MainForm.ErrorMsg = Properties.Resources.sInvalidDataRetrievalPasswordException;
            else
            {
                MainForm.ErrorMsg = String.Empty;
                MainForm.IAT.Packager.DataRetrievalPassword = PasswordEdit.Text;
            }
        }

        private void RedirectEdit_TextChanged(object sender, EventArgs e)
        {
            IAT.Packager.RedirectionURL = RedirectEdit.Text;
        }

        private void UploadButton_Click(object sender, EventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.Title = Properties.Resources.sOpenPackageFileDlgTitle;
            dlg.Filter = Properties.Resources.sPackageFileFilter;
            dlg.DefaultExt = Properties.Resources.sPackageFileExtension;
            dlg.AddExtension = true;
            if (dlg.ShowDialog(MainForm) != DialogResult.OK)
                return;
            ProgressWindow progressWindow = new ProgressWindow();
            CIATUploader uploader = new CIATUploader(dlg.FileName, ServerURL.Text, PasswordEdit.Text, progressWindow);
            uploader.OnIATExists = new CIATUploader.IATExistsHandler(TestPackagerPanel_IATExists);
            uploader.DeployIAT();
            progressWindow.ShowDialog(MainForm);
            MessageBox.Show("Your IAT has been successfully uploaded to the server.");
        }
    }
}

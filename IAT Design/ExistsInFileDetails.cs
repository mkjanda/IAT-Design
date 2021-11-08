using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;

namespace IATClient
{
    /// <summary>
    /// ExistsInFileDetails provides a control that allows the user to attach a text file to the .iat configuration file
    /// which contains a list of either comma, tab, or linebreak delimited acceptable resposne values.  It also provides
    /// options for each response in the file to be permitted only once across test takers, and for the given text file
    /// to be copied to the same directory as the .iat configuration file when the configuration file is saved.
    /// </summary>
    public partial class ExistsInFileDetails : UserControl
    {
        /// <summary>
        /// A flag to indicate if the data in the list of valid responses loaded from the file is no longer valid
        /// </summary>
        private bool ResponseListValid;

        // error flags.  fatal errors prevent the control's data from being rendered in the survey view
        private bool _HasErrors, _HasFatalErrors;
        
        /// <summary>
        /// gets or sets whether the control's data contains errors which do not prevent it from being rendered in
        /// the survey view.  Note: setting this value to "false" will also set HasFatalErrors to "false"
        /// </summary>
        public bool HasErrors
        {
            get
            {
                return _HasErrors;
            }
            set
            {
                if (value == false)
                    _HasFatalErrors = false;
                _HasErrors = value;
            }
        }

        /// <summary>
        /// gets or sets whether the control's data contains errors which prevent it from being rendered in the survey
        /// view.  Note: setting this value to "true" will also set "HasErrors" to "true"
        /// </summary>
        public bool HasFatalErrors
        {
            get
            {
                return _HasFatalErrors;
            }
            set
            {
                if (value == true)
                    _HasErrors = true;
                _HasFatalErrors = value;
            }
        }

        /// <summary>
        /// Gets the main window
        /// </summary>
        private IATConfigMainForm MainForm
        {
            get 
            {
                return (IATConfigMainForm)Parent.Parent;
            }
        }


        /// <summary>
        /// A list of the acceptable responses
        /// </summary>
        private List<String> ResponseList;


        public CExistsInFileResponse Response
        {
            get
            {
                if (_HasFatalErrors)
                    return null;
                return GetResponse();
            }
            set
            {
                switch (value.Delimitation)
                {
                    case CExistsInFileResponse.EDelimitation.comma:
                        CommaRadio.Checked = true;
                        break;

                    case CExistsInFileResponse.EDelimitation.tab:
                        TabRadio.Checked = true;
                        break;

                    case CExistsInFileResponse.EDelimitation.linebreak:
                        LinebreakRadio.Checked = true;
                        break;
                }
                AllowEachResponseOnceCheck.Checked = value.EachResponseOnlyOnce;
                if (value.Directory != CExistsInFileResponse.sIATDirectory)
                {
                    CopyOnSaveCheck.Checked = value.CopyToOutputDirOnSave;
                    FilenameTextBox.Text = value.FullFilePath;
                }
                else
                {
                    CopyOnSaveCheck.Checked = true;
                    CopyOnSaveCheck.Enabled = false;
                    FilenameTextBox.Text = value.FileName;
                }
                if (value.ResponsesLoaded)
                {
                    ResponseList = value.ValidResponses;
                    ResponseListValid = true;
                    UpdateValueView();
                }
                else 
                {
                    LoadResponseFile();
                    UpdateValueView();
                }
            }
        }


        /// <summary>
        /// The default constructor
        /// </summary>
        public ExistsInFileDetails()
        {
            InitializeComponent();
            ResponseList = new List<String>();
            ResponseListValid = false;
            BrowseButton.Enabled = false;
        }

        private void CommaRadio_CheckedChanged(object sender, EventArgs e)
        {
            // a delimiter has been selected.  enable the browse button
            if (CommaRadio.Checked)
                BrowseButton.Enabled = true;

            // flag the data as invalid and update the currently selected item in the survey panel
            ResponseListValid = false;
            FilenameTextBox.Text = String.Empty;
            UpdateValueView();
            ValidateInput();
            MainForm.m_SurveyPanel.InvalidateSelectedItem();
        }

        private void TabRadio_CheckedChanged(object sender, EventArgs e)
        {
            // a delimiter has been selected.  enable the browse button
            if (TabRadio.Checked)
                BrowseButton.Enabled = true;

            // flag the data as invalid and update the currently selected item in the survey panel
            ResponseListValid = false;
            FilenameTextBox.Text = String.Empty;
            UpdateValueView();
            ValidateInput();
            MainForm.m_SurveyPanel.InvalidateSelectedItem();
       }

        private void LinebreakRadio_CheckedChanged(object sender, EventArgs e)
        {
            // a delimiter has been selected.  enable the browse button
            if (LinebreakRadio.Checked)
                BrowseButton.Enabled = true;

            // flag the data as invalid and update the currently selected item in the survey panel
            ResponseListValid = false;
            FilenameTextBox.Text = String.Empty;
            UpdateValueView();
            ValidateInput();
            MainForm.m_SurveyPanel.InvalidateSelectedItem();
        }

        private void BrowseButton_Click(object sender, EventArgs e)
        {
            // initialize and display the open file dialog, returning if the user doesn't click OK
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "Text Files|*.txt|Comma Separated Value Files|*.csv|All Files|*.*";
            dialog.AddExtension = true;
            dialog.FilterIndex = 0;
            dialog.Title = Properties.Resources.sAttachValidResponseFileDialogTitle;
            if (dialog.ShowDialog() != DialogResult.OK)
                return;

            // set the filename text and load the file
            FilenameTextBox.Text = dialog.FileName;
            if (LoadResponseFile())
            {
                UpdateValueView();
                ValidateInput();
                MainForm.m_SurveyPanel.InvalidateSelectedItem();
            }
        }

        /// <summary>
        /// Instantiates a new CExistsInFileResponse and fills it out with the control's data
        /// </summary>
        /// <returns>A new CExistsInFileResponse filled out with the control's data</returns>
        private CExistsInFileResponse GetResponse()
        {
            CExistsInFileResponse r = new CExistsInFileResponse();

            // set the filename and directory
            if ((CopyOnSaveCheck.Enabled == false) && (CopyOnSaveCheck.Checked == true))
            {
                r.FileName = FilenameTextBox.Text;
                r.Directory = CExistsInFileResponse.sIATDirectory;
            }
            else
                r.FullFilePath = FilenameTextBox.Text;

            // set the delimitation
            if (CommaRadio.Checked)
                r.Delimitation = CExistsInFileResponse.EDelimitation.comma;
            else if (TabRadio.Checked)
                r.Delimitation = CExistsInFileResponse.EDelimitation.tab;
            else if (LinebreakRadio.Checked)
                r.Delimitation = CExistsInFileResponse.EDelimitation.linebreak;
            else
                throw new Exception();

            // set misc. attributes
            r.CopyToOutputDirOnSave = CopyOnSaveCheck.Checked;
            r.EachResponseOnlyOnce = AllowEachResponseOnceCheck.Checked;

            // fill out the response list
            if (ResponseListValid)
                r.ValidResponses = ResponseList;

            // set the error flag
            r.HasErrors = HasErrors;

            // return the response
            return r;
        }

        /// <summary>
        /// Updates the ValueView from the ResponseList, creating two columns of responses and sizing the columns so each
        /// takes up half the view
        /// </summary>
        private void UpdateValueView()
        {
            // keep track of cell height.  they're all going to be the same, but test for greater values just in case
            int nHeight = 0;

            // completely empty the ValueView
            ValueView.Rows.Clear();
            ValueView.Columns.Clear();

            // leave view empty if response list is invalid
            if (ResponseListValid == false)
                return;

            // add the columns
            DataGridViewColumn col1, col2;
            DataGridViewColumnHeaderCell header1, header2;
            col1 = new DataGridViewColumn();
            col2 = new DataGridViewColumn();
            header1 = new DataGridViewColumnHeaderCell();
            header2 = new DataGridViewColumnHeaderCell();
            col1.HeaderCell = header1;
            col2.HeaderCell = header2;
            ValueView.Columns.Add(col1);
            ValueView.Columns.Add(col2);

            // add the rows
            DataGridViewRow row;
            DataGridViewTextBoxCell cell;
            int ctr;
            for (ctr = 0; ctr < ResponseList.Count >> 1; ctr++)
            {
                row = new DataGridViewRow();
                cell = new DataGridViewTextBoxCell();
                cell.Value = ResponseList[ctr << 1];
                row.Cells.Add(cell);
                cell = new DataGridViewTextBoxCell();
                cell.Value = ResponseList[(ctr << 1) + 1];
                row.Cells.Add(cell);
                ValueView.Rows.Add(row);
                if (cell.Size.Height > nHeight)
                    nHeight = cell.Size.Height;
            }
            // check for left over, odd numbered value
            if ((ctr << 1) < ResponseList.Count)
            {
                row = new DataGridViewRow();
                cell = new DataGridViewTextBoxCell();
                cell.Value = ResponseList[ResponseList.Count - 1];
                row.Cells.Add(cell);
                row.Cells.Add(new DataGridViewTextBoxCell());
                ValueView.Rows.Add(row);
            }

            if (ValueView.Rows.Count > 0)
            {
                col1.Width = (ValueView.GetRowDisplayRectangle(0, true).Width >> 1);
                col2.Width = (ValueView.GetRowDisplayRectangle(0, true).Width >> 1);
            }
            else
            {
                col1.Width = (ValueView.Width >> 1) - 2;
                col2.Width = (ValueView.Width >> 1) - 2;
            }
        }

        public bool LoadResponseFile()
        {
            String ValidResponse;
            String sLine;
            Char cDelim;
            System.IO.StreamReader sReader = null;

            // clear the list of valid responses
            ResponseList = new List<String>();
            
            // get the delimitation character
            if (CommaRadio.Checked)
                cDelim = ',';
            else if (TabRadio.Checked)
                cDelim = '\t';
            else if (LinebreakRadio.Checked)
                cDelim = '\n';
            else
                return false;
            
            // try reading the valid response file
            try
            {
                // open the file
                sReader = System.IO.File.OpenText(FilenameTextBox.Text);

                // read each line
                while ((sLine = sReader.ReadLine()) != null)
                {
                    // if delimitation is the line break, trim value for whitespace
                    if (LinebreakRadio.Checked)
                    {
                        ValidResponse = sLine;
                        ValidResponse.Trim();
                        ResponseList.Add(ValidResponse);
                    }
                    // else search through the line for each valid response
                    else
                    {
                        int startNdx = 0;
                        int ctr = 0;
                        while (ctr < sLine.Length)
                        {
                            while ((sLine[ctr] != cDelim) && (ctr + 1 < sLine.Length))
                                ctr++;
                            ValidResponse = sLine.Substring(startNdx, ctr - startNdx);
                            ValidResponse = ValidResponse.Trim(cDelim);
                            ValidResponse = ValidResponse.Trim();
                            if (ValidResponse.Length != 0)
                                ResponseList.Add(ValidResponse);
                            startNdx = ++ctr;
                        }
                    }
                }
            }
            catch (Exception)
            {
                MessageBox.Show(String.Format(Properties.Resources.sAttachedFileException, FilenameTextBox.Text));
                ResponseList.Clear();
                ResponseListValid = false;
                return false;
            }
            finally
            {
                if (sReader != null)
                    sReader.Dispose();
            }

            // success
            ResponseListValid = true;
            return true;
        }

        public void ValidateInput()
        {
            HasErrors = false;
            // check for no delimitation specified
            if ((!CommaRadio.Checked) && (!TabRadio.Checked) && (!LinebreakRadio.Checked))
            {
                HasFatalErrors = true;
                MainForm.ErrorMsg = Properties.Resources.sExistsInFileNoDelimitationException;
                return;
            }
            // check for no file loaded
            if (FilenameTextBox.Text == String.Empty)
            {
                HasFatalErrors = true;
                MainForm.ErrorMsg = Properties.Resources.sExistsInFileNoFileSpecifiedException;
                return;
            }

            // no errors
            MainForm.ErrorMsg = String.Empty;
        }

        private void ExistsInFileDetails_ParentChanged(object sender, EventArgs e)
        {
            if (Parent != null)
            {
                ValidateInput();
                MainForm.m_SurveyPanel.InvalidateSelectedItem();
            }
        }

        private void AllowEachResponseOnceCheck_CheckedChanged(object sender, EventArgs e)
        {
            ValidateInput();
            MainForm.m_SurveyPanel.InvalidateSelectedItem();
        }

        private void CopyOnSaveCheck_CheckedChanged(object sender, EventArgs e)
        {
            ValidateInput();
            MainForm.m_SurveyPanel.InvalidateSelectedItem();
        }
    }
}

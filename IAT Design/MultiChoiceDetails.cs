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
    /// MultiChoiceDetails provides a control for editing a multiple choice response type
    /// </summary>
    public partial class MultiChoiceDetails : UserControl
    {
        // error flags.  fatal errors prevent the control from rendering in the survey view
        private bool _HasErrors, _HasFatalErrors;

        /// <summary>
        /// gets or sets whether the control has errors that do not prevent its data from being rendered in the survey view.
        /// Note: Setting this flag to "false" will set HasFatalErrors to "false"
        /// </summary>
        public bool HasErrors
        {
            get
            {
                return _HasErrors;
            }
            set
            {
                _HasErrors = value;
                if (value == false)
                    _HasFatalErrors = false;
            }
        }

        /// <summary>
        /// gets or sets whether the control has errors that prevent its data from being rendered in the survey view.
        /// Note: Setting this flag to "true" will set HasErrors to "true"
        /// </summary>
        public bool HasFatalErrors
        {
            get
            {
                return _HasFatalErrors;
            }
            set
            {
                _HasFatalErrors = value;
                if (value == true)
                    _HasErrors = true;
            }
        }

        /// <summary>
        /// gets the main window
        /// </summary>
        private IATConfigMainForm MainForm
        {
            get
            {
                return (IATConfigMainForm)Parent.Parent;
            }
        }


        /// <summary>
        /// gets or sets the response object represented by the control's data.  get returns null if the control
        /// has fatal errors
        /// </summary>
        public CMultipleResponse Response
        {
            get
            {
                if (HasFatalErrors)
                    return null;
                return GetResponse();
            }
            set
            {
                CMultipleResponse r = value;
                m_ChoiceList.Clear();
                for (int ctr = 0; ctr < r.NumChoices; ctr++)
                    m_ChoiceList.Add(r.Choices[ctr]);
                FillChoiceView();       
            }
        }

        // the list of choices
        private List<String> m_ChoiceList;

        /// <summary>
        /// gets the list of choices
        /// </summary>
        protected List<String> Choices
        {
            get
            {
                return m_ChoiceList;
            }
        }

        /// <summary>
        /// The default constructor
        /// </summary>
        public MultiChoiceDetails()
        {
            InitializeComponent();
            m_ChoiceList = new List<String>();
        }

        /// <summary>
        /// Fills in the DataGridView that allows for the editing of the chocies with the strings contained in Choices.
        /// Also validates the control's data and invalidates the selectedd item in the survey view
        /// </summary>
        private void FillChoiceView()
        {
            DataGridViewRow row;
            DataGridViewTextBoxCell cell;
            ChoiceView.Rows.Clear();
            for (int ctr = 0; ctr < m_ChoiceList.Count; ctr++)
            {
                row = new DataGridViewRow();
                cell = new DataGridViewTextBoxCell();
                cell.Value = m_ChoiceList[ctr];
                row.Cells.Add(cell);
                ChoiceView.Rows.Add(row);
            }
            ValidateInput();
            MainForm.m_SurveyPanel.InvalidateSelectedItem();
        }


        private void AddItemButton_Click(object sender, EventArgs e)
        {
            // add a new, blank item to the ChoiceView
            DataGridViewRow row = new DataGridViewRow();
            DataGridViewTextBoxCell cell = new DataGridViewTextBoxCell();
            row.Cells.Add(cell);
            ChoiceView.Rows.Add(row);

            // validate the control and invalidate the selected item in the survey view
            ValidateInput();
            MainForm.m_SurveyPanel.InvalidateSelectedItem();
        }

        private void DeleteItemButton_Click(object sender, EventArgs e)
        {
            // delete the row with the currently selected cell
            if (ChoiceView.SelectedCells.Count == 0)
                return;
            DataGridViewCell cell = ChoiceView.SelectedCells[0];
            int ctr = 0;
            int nRow = -1;
            while (ctr < ChoiceView.Rows.Count)
            {
                if (ChoiceView.Rows[ctr].Cells.IndexOf(cell) != -1)
                {
                    nRow = ctr;
                    break;
                }
                ctr++;
            }
            if (nRow == -1)
                return;
            ChoiceView.Rows.RemoveAt(nRow);

            // validate the control and invalidate the selected item in the survey view
            ValidateInput();
            MainForm.m_SurveyPanel.InvalidateSelectedItem();
        }

        private void MultiChoiceDetails_Load(object sender, EventArgs e)
        {
            FillChoiceView();
        }

        /// <summary>
        /// Instantiates a new CMultipleResponse object with the control's data
        /// </summary>
        /// <returns>A new CMultipleResponse object with the form's data</returns>
        private CMultipleResponse GetResponse()
        {
            CMultipleResponse Response = new CMultipleResponse(ChoiceView.Rows.Count);
            for (int ctr = 0; ctr < ChoiceView.Rows.Count; ctr++)
                if (ChoiceView.Rows[ctr].Cells[0].Value != null)
                    Response.SetChoice(ctr, ChoiceView.Rows[ctr].Cells[0].Value.ToString());
                else
                    Response.SetChoice(ctr, String.Empty);
        
            Response.HasErrors = HasErrors;
            return Response;
        }

        private void ChoiceView_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            // validate the control and invalidate the selected item in the survey view
            ValidateInput();
            MainForm.m_SurveyPanel.InvalidateSelectedItem();
        }

        private void MultiChoiceDetails_ParentChanged(object sender, EventArgs e)
        {
            if (Parent != null)
            {
                // validate the control and invalidate the selected item in the survey view
                ValidateInput();
                MainForm.m_SurveyPanel.InvalidateSelectedItem();
            }
        }

        /// <summary>
        /// Validates the control's data, setting error flags and the error message in the main window as appropriate
        /// </summary>
        public void ValidateInput()
        {
            HasErrors = false;
            object o;
            // check for no choices
            if (ChoiceView.Rows.Count == 0)
            {
                MainForm.ErrorMsg = Properties.Resources.sMultiChoiceNoChoicesException;
                HasErrors = true;
                return;
            }
            // check for blank choices
            for (int ctr = 0; ctr < ChoiceView.Rows.Count; ctr++)
            {
                o = ChoiceView.Rows[ctr].Cells[0].Value;
                if (o == null)
                {
                    MainForm.ErrorMsg = String.Format(Properties.Resources.sMultiChoiceEmptyCellException, ctr + 1);
                    HasErrors = true;
                    return;
                }
            }
            // no errors
            MainForm.ErrorMsg = String.Empty;
        }
    }
}

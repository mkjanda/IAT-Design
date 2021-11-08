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
    /// LikertDetails is a control with a set of child controls for the definition of a likert response item
    /// </summary>
    public partial class LikertDetails : UserControl
    {
        // fatal errors indicate the control's data cannot be rendered in the survey view
        private bool _HasErrors, _HasFatalErrors;

        /// <summary>
        /// gets or sets a flag that indicates the control's data contains errors that do not prevent it
        /// from being rendered in the survey view.  Note: Setting this value to "false" sets HasFatalErrors
        /// to "false"
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
        /// gets or sets a flag that indicates the control's data contains errors that prevent it from being
        /// rendered in the survey view.  Note: Setting this value to "true" sets HasErrors to "true"
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
        /// gets or sets the response depicted by the control's data.  get returns null if the control's data
        /// has fatal errors
        /// </summary>
        public CLikertResponse Response
        {
            get
            {
                if (HasFatalErrors)
                    return null;
                return GetResponse();
            }
            set
            {
                CLikertResponse r = value;
                ReverseScoredCheck.Checked = r.ReverseScored;
                m_StatementList.Clear();
                for (int ctr = 0; ctr < r.NumChoices; ctr++)
                    m_StatementList.Add(r.ChoiceDescriptions[ctr]);
                FillLikertView();
            }
        }
       
        // the list of statements
        protected List<String> m_StatementList;
       
        /// <summary>
        /// The default constructor
        /// </summary>
        public LikertDetails()
        {
            InitializeComponent();

            // instantiate the statement list and fill it with dummy values
            m_StatementList = new List<String>();
            m_StatementList.Add(Properties.Resources.sLikertDefaultResponse1);
            m_StatementList.Add(Properties.Resources.sLikertDefaultResponse2);
            m_StatementList.Add(Properties.Resources.sLikertDefaultResponse3);
            m_StatementList.Add(Properties.Resources.sLikertDefaultResponse4);
            m_StatementList.Add(Properties.Resources.sLikertDefaultResponse5);
            m_StatementList.Add(Properties.Resources.sLikertDefaultResponse6);
            m_StatementList.Add(Properties.Resources.sLikertDefaultResponse7);
        }
                
        /// <summary>
        /// Fills the DataGridView that provides for the editing of the response statements with the values
        /// in m_StatementList.  This function validates the control and invalidates the selected item in 
        /// the survey view
        /// </summary>
        private void FillLikertView()
        {
            LikertView.Rows.Clear();
            if (ReverseScoredCheck.Checked)
            {
                for (int ctr = m_StatementList.Count; ctr > 0; ctr--)
                {
                    DataGridViewRow row = new DataGridViewRow();
                    DataGridViewTextBoxCell cell = new DataGridViewTextBoxCell();
                    cell.Value = ctr.ToString();
                    row.Cells.Add(cell);
                    cell = new DataGridViewTextBoxCell();
                    cell.Value = m_StatementList[m_StatementList.Count - ctr];
                    row.Cells.Add(cell);
                    LikertView.Rows.Add(row);
                }
            }
            else
            {
                for (int ctr = 0; ctr < m_StatementList.Count; ctr++)
                {
                    DataGridViewRow row = new DataGridViewRow();
                    DataGridViewTextBoxCell cell = new DataGridViewTextBoxCell();
                    cell.Value = (ctr + 1).ToString();
                    row.Cells.Add(cell);
                    cell = new DataGridViewTextBoxCell();
                    cell.Value = m_StatementList[ctr];
                    row.Cells.Add(cell);
                    LikertView.Rows.Add(row);
                }
            }
            ValidateInput();
            MainForm.m_SurveyPanel.InvalidateSelectedItem();
        }

        private void ReverseScoredCheck_CheckedChanged(object sender, EventArgs e)
        {
            // fill in the statement values in the DataGridView to reflect the change in reverse scoring,
            // validate the control, and invalidate the selected item in the SurveyView
            FillLikertViewValues();
            ValidateInput();
            MainForm.m_SurveyPanel.InvalidateSelectedItem();
        }

        /// <summary>
        /// Fills in only the statement values in the DataGridView.  This function neither validates the
        /// control nor invalidates the selected item in the survey view
        /// </summary>
        private void FillLikertViewValues()
        {
            for (int ctr = 0; ctr < LikertView.Rows.Count; ctr++)
            {
                if (ReverseScoredCheck.Checked)
                    LikertView.Rows[ctr].Cells[0].Value = (LikertView.Rows.Count - ctr).ToString();
                else
                    LikertView.Rows[ctr].Cells[0].Value = ctr + 1;
            }
        }

        public void LikertDetails_Load(object sender, EventArgs e)
        {
            // fill the likert view
            FillLikertView();
        }

        private void AddItemButton_Click(object sender, EventArgs e)
        {
            // add a blank statement to the DataGridView
            DataGridViewRow row = new DataGridViewRow();
            DataGridViewTextBoxCell cell = new DataGridViewTextBoxCell();
            cell.Value = (ReverseScoredCheck.Checked) ? 1 : LikertView.Rows.Count + 1;
            row.Cells.Add(cell);
            cell = new DataGridViewTextBoxCell();
            row.Cells.Add(cell);
            LikertView.Rows.Add(row);

            // if the response is reverse scored, fill in the values again to increment each value above the
            // new statement
            if (ReverseScoredCheck.Checked)
                FillLikertViewValues();

            // validate the control and invalidate the selected item in the survey view
            ValidateInput();
            MainForm.m_SurveyPanel.InvalidateSelectedItem();
        }

        private void DeleteItemButton_Click(object sender, EventArgs e)
        {
            // delete the row that contains the currently selected cell
            if (LikertView.SelectedCells.Count == 0)
                return;
            DataGridViewCell cell = LikertView.SelectedCells[0];
            int ctr = 0;
            int nRow = -1;
            while (ctr < LikertView.Rows.Count)
            {
                if (LikertView.Rows[ctr].Cells.IndexOf(cell) != -1)
                {
                    nRow = ctr;
                    break;
                }
                ctr++;
            }
            if (nRow == -1)
                return;
            LikertView.Rows.RemoveAt(nRow);
            FillLikertViewValues();

            // validate the control and invalidate the selected item in the survey view
            ValidateInput();
            MainForm.m_SurveyPanel.InvalidateSelectedItem();
        }

        /// <summary>
        /// Instantiates a new CLikertResponse object and fills it with the control's data
        /// </summary>
        /// <returns>A new CLikertResponse object on success, "null" on error</returns>
        private CLikertResponse GetResponse()
        {
            CLikertResponse Response = new CLikertResponse(LikertView.Rows.Count, ReverseScoredCheck.Checked);
            for (int ctr = 0; ctr < LikertView.Rows.Count; ctr++)
                if (LikertView.Rows[ctr].Cells[1].Value != null)
                    Response.SetChoiceDesc(ctr, LikertView.Rows[ctr].Cells[1].Value.ToString());
                else
                    Response.SetChoiceDesc(ctr, String.Empty);
            Response.HasErrors = HasErrors;
            return Response;
        }

        private void LikertView_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            // validate the control and invalidate the selected item in the survey view
            ValidateInput();
            MainForm.m_SurveyPanel.InvalidateSelectedItem();
        }

        /// <summary>
        /// Validates the controls data, setting or clearing the appropriate error flags and the error message
        /// in the main window
        /// </summary>
        public void ValidateInput()
        {
            HasErrors = false;
            // force an end edit
            LikertView.EndEdit();
            for (int ctr = 0; ctr < LikertView.Rows.Count; ctr++)
            {
                // check for a null value in the statement cell
                object o = LikertView.Rows[ctr].Cells[1].Value;
                if (o == null)
                {
                    MainForm.ErrorMsg = String.Format(Properties.Resources.sLikertEmptyCellException, LikertView.Rows[ctr].Cells[0].Value.ToString());
                    HasErrors = true;
                    return;
                }
            }
            // check for at least two statements
            if (LikertView.Rows.Count < 2)
            {
                MainForm.ErrorMsg = Properties.Resources.sLikertMinStatementCountException;
                HasErrors = true;
                return;
            }
            
            // no errors
            MainForm.ErrorMsg = String.Empty;
        }

        private void LikertDetails_ParentChanged(object sender, EventArgs e)
        {
            if (Parent != null)
            {
                // validate the control and invalidate the selected item in the survey view
                ValidateInput();
                MainForm.m_SurveyPanel.InvalidateSelectedItem();
            }
        }
    }
}

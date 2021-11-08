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
    /// MultiSelectionDetails provides a control that can be used to define a multiple selection response type
    /// </summary>
    public partial class MultiSelectionDetails : UserControl
    {
        // error flags.  fatal errors indicate that the control's data cannot be rendered in the survey view
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
        /// gets or sets the response object displayed by the control.  get returns null if the control has a fatal error.
        /// Note: This property does not perform control validation.
        /// </summary>
        public CMultiBooleanResponse Response
        {
            get
            {
                if (HasFatalErrors)
                    return null;
                return GetResponse();
            }
            set
            {
                CMultiBooleanResponse r = value;
                m_SelectionList.Clear();
                for (int ctr = 0; ctr < r.NumValues; ctr++)
                    m_SelectionList.Add(r.LabelList[ctr]);
                FillMultiSelectView();
            }
        }

        // the list of selections
        private List<String> m_SelectionList;
        
        /// <summary>
        /// gets the list of selections
        /// </summary>
        public List<String> Selections
        {
            get
            {
                return m_SelectionList;
            }
        }

        /// <summary>
        /// The default constructor
        /// </summary>
        public MultiSelectionDetails()
        {
            InitializeComponent();
            m_SelectionList = new List<String>();
        }

        /// <summary>
        /// Fills the DataGridView used to edit the statements with the statements contained in SelectionList,
        /// and then validates the control and invalidates the selected item in the survey view
        /// </summary>
        private void FillMultiSelectView()
        {
            DataGridViewRow row;
            DataGridViewTextBoxCell cell;
            for (int ctr = 0; ctr < Selections.Count; ctr++)
            {
                row = new DataGridViewRow();
                cell = new DataGridViewTextBoxCell();
                cell.Value = Selections[ctr];
                row.Cells.Add(cell);
                MultiSelectView.Rows.Add(row);
            }
            ValidateInput();
            MainForm.m_SurveyPanel.InvalidateSelectedItem();
        }

        private void AddItemButton_Click(object sender, EventArgs e)
        {
            // add an empty row to the DataGridView
            DataGridViewRow row = new DataGridViewRow();
            DataGridViewTextBoxCell cell = new DataGridViewTextBoxCell();
            row.Cells.Add(cell);
            MultiSelectView.Rows.Add(row);

            // validate the control and invalidate the selected item in the survey view
            ValidateInput();
            MainForm.m_SurveyPanel.InvalidateSelectedItem();
        }

        private void MultiSelectionDetails_Load(object sender, EventArgs e)
        {
            FillMultiSelectView();
        }

        private void DeleteItemButton_Click(object sender, EventArgs e)
        {
            // if a cell has been selected, find its row index and remove that row from the DataGridView
            if (MultiSelectView.SelectedCells.Count == 0)
                return;
            DataGridViewCell cell = MultiSelectView.SelectedCells[0];
            int ctr = 0;
            int nRow = -1;
            while (ctr < MultiSelectView.Rows.Count)
            {
                if (MultiSelectView.Rows[ctr].Cells.IndexOf(cell) != -1)
                {
                    nRow = ctr;
                    break;
                }
                ctr++;
            }
            if (nRow == -1)
                return;
            MultiSelectView.Rows.RemoveAt(nRow);

            // validate the control and invalidate the selected item in the survey view
            ValidateInput();
            MainForm.m_SurveyPanel.InvalidateSelectedItem();
        }

        /// <summary>
        /// Instantiates a new CMultiBooleanResponse and fills it out with the control's data
        /// </summary>
        /// <returns>A new CMultiBooleanResponse object that contains the data displayed in the control</returns>
        private CMultiBooleanResponse GetResponse()
        {
            CMultiBooleanResponse Response = new CMultiBooleanResponse(MultiSelectView.Rows.Count);
            for (int ctr = 0; ctr < MultiSelectView.Rows.Count; ctr++)
                if (MultiSelectView.Rows[ctr].Cells[0].Value != null)
                    Response.SetLabel(ctr, MultiSelectView.Rows[ctr].Cells[0].Value.ToString());
                else
                    Response.SetLabel(ctr, String.Empty);
        
            Response.HasErrors = HasErrors; 
            return Response;
        }

        private void MultiSelectView_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            // validate the control and invalidate the selected item in the survey view
            ValidateInput();
            MainForm.m_SurveyPanel.InvalidateSelectedItem();
        }

        private void MultiSelectionDetails_ParentChanged(object sender, EventArgs e)
        {
            if (Parent != null)
            {
                // validate the control and invalidate the selected item in the survey view
                ValidateInput();
                MainForm.m_SurveyPanel.InvalidateSelectedItem();
            }
        }

        /// <summary>
        /// Validates the control's data, setting error flags and the error message in the main window
        /// as appropriate
        /// </summary>
        public void ValidateInput()
        {
            HasErrors = false;
            object o;
            // check for an empty DataGridView
            if (MultiSelectView.Rows.Count == 0)
            {
                MainForm.ErrorMsg = Properties.Resources.sMultiSelectionNoChoicesException;
                HasErrors = true;
                return;
            }
            // check for empty cells
            for (int ctr = 0; ctr < MultiSelectView.Rows.Count; ctr++)
            {
                o = MultiSelectView.Rows[ctr].Cells[0].Value;
                if (o == null)
                {
                    MainForm.ErrorMsg = String.Format(Properties.Resources.sMultiSelectionEmptyCellException, ctr + 1);
                    HasErrors = true;
                    return;
                }
            }
            // no errors
            MainForm.ErrorMsg = String.Empty;
        }
    }
}

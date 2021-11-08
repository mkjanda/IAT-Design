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
    /// WeightedMultipleDetails provides a control that allows the user to define a weighted multiple choice response type
    /// </summary>
    public partial class WeightedMultipleDetails : UserControl
    {
        // error flags.  fatal errors prevent the control's data from being rendered in the SurveyView
        private bool _HasErrors, _HasFatalErrors;

        /// <summary>
        /// gets or sets a value that indicates whether the control's data contains errors that do not prevent
        /// it from being rendered in the SurveyView.  Note: Setting this value to "false" will set HasFatalErrors
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
        /// gets or sets a value that indicates whether the control's data contains errors that prevent it from 
        /// being rendered in the SurveyView.  Note: Setting this value to "true" will set HasErrors to "true"
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
        
        // lists of the choices and the corresponding weights
        private List<int> _Weights;
        private List<String> _Choices;

        /// <summary>
        /// gets the list of weights
        /// </summary>
        public List<int> Weights
        {
            get
            {
                return _Weights;
            }
        }

        /// <summary>
        /// gets the list of choices
        /// </summary>
        public List<String> Choices
        {
            get
            {
                return _Choices;
            }
        }

        /// <summary>
        /// gets or sets the response displayed by the control's data.  get returns "null" if the control's data has
        /// fatal errors
        /// </summary>
        public CWeightedMultipleResponse Response
        {
            get
            {
                if (HasFatalErrors)
                    return null;
                return GetResponse();
            }
            set
            {
                Choices.Clear();
                Weights.Clear();
                CWeightedMultipleResponse r = value;
                for (int ctr = 0; ctr < r.NumChoices; ctr++)
                {
                    Weights.Add(r.Weights[ctr]);
                    Choices.Add(r.Choices[ctr]);
                }
                FillWeightedMultiChoiceView();
            }
        }

        /// <summary>
        /// The default constructor.
        /// </summary>
        public WeightedMultipleDetails()
        {
            InitializeComponent();
            _Weights = new List<int>();
            _Choices = new List<String>();
        }

        /// <summary>
        /// Fills the DataGridView with the weights and choices contained by the weight and choice lists,
        /// then validates the control's data and invalidates the selected item in the Survey View
        /// </summary>
        private void FillWeightedMultiChoiceView()
        {
            WeightedMultiChoiceView.Rows.Clear();
            DataGridViewRow row;
            DataGridViewTextBoxCell cell;

            // fill the DataGridView with the weights and choices in the weight and choice lists
            for (int ctr = 0; ctr < Choices.Count; ctr++)
            {
                row = new DataGridViewRow();
                cell = new DataGridViewTextBoxCell();
                cell.Value = Weights[ctr].ToString();
                row.Cells.Add(cell);
                cell = new DataGridViewTextBoxCell();
                cell.Value = Choices[ctr];
                row.Cells.Add(cell);
                WeightedMultiChoiceView.Rows.Add(row);
            }

            // validate the control's input and invalidate the selected item in the survey view
            ValidateInput();
            MainForm.m_SurveyPanel.InvalidateSelectedItem();
        }

        private void AddItemButton_Click(object sender, EventArgs e)
        {
            // insert a blannk row in the DataGridView
            DataGridViewRow row = new DataGridViewRow();
            DataGridViewTextBoxCell cell = new DataGridViewTextBoxCell();
            cell.Value = String.Empty;
            row.Cells.Add(cell);
            cell = new DataGridViewTextBoxCell();
            cell.Value = String.Empty;
            row.Cells.Add(cell);
            WeightedMultiChoiceView.Rows.Add(row);

            // validate the control's input and invalidate the selected item in the survey view
            ValidateInput();
            MainForm.m_SurveyPanel.InvalidateSelectedItem();
        }

        private void DeleteItemButton_Click(object sender, EventArgs e)
        {
            // if a cell is selected, delete its row
            if (WeightedMultiChoiceView.SelectedCells.Count == 0)
                return;
            DataGridViewCell cell = WeightedMultiChoiceView.SelectedCells[0];
            int ctr = 0;
            int nRow = -1;
            while (ctr < WeightedMultiChoiceView.Rows.Count)
            {
                if (WeightedMultiChoiceView.Rows[ctr].Cells.IndexOf(cell) != -1)
                {
                    nRow = ctr;
                    break;
                }
                ctr++;
            }
            if (nRow == -1)
                return;
            WeightedMultiChoiceView.Rows.RemoveAt(nRow);

            // validate the control's input and invalidate the selected item in the survey view
            ValidateInput();
            MainForm.m_SurveyPanel.InvalidateSelectedItem();
        }

        /// <summary>
        /// Instantiates a new CWeightedMultipleResponse and fills it with the control's data
        /// </summary>
        /// <returns>The new CWeightedMultipleResponse object filled with the control's data</returns>
        private CWeightedMultipleResponse GetResponse()
        {
            CWeightedMultipleResponse Response = new CWeightedMultipleResponse(WeightedMultiChoiceView.Rows.Count);
            for (int ctr = 0; ctr < WeightedMultiChoiceView.Rows.Count; ctr++)
                if ((WeightedMultiChoiceView.Rows[ctr].Cells[0].Value != null)
                    && (WeightedMultiChoiceView.Rows[ctr].Cells[1].Value != null))
                    Response.SetChoice(ctr, WeightedMultiChoiceView.Rows[ctr].Cells[1].Value.ToString(),
                        Convert.ToInt32(WeightedMultiChoiceView.Rows[ctr].Cells[0].Value.ToString()));
                else if (WeightedMultiChoiceView.Rows[ctr].Cells[0].Value != null)
                    Response.SetChoice(ctr, String.Empty, Convert.ToInt32(WeightedMultiChoiceView.Rows[ctr].Cells[0].Value.ToString()));
                else if (WeightedMultiChoiceView.Rows[ctr].Cells[1].Value != null)
                    Response.SetChoice(ctr, WeightedMultiChoiceView.Rows[ctr].Cells[1].Value.ToString(), int.MinValue);
                else
                    Response.SetChoice(ctr, String.Empty, int.MinValue);

            Response.HasErrors = HasErrors;
            return Response;
        }

        private void WeightedMultiChoiceView_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            // validate the control's input and invalidate the selected item in the survey view
            ValidateInput();
            MainForm.m_SurveyPanel.InvalidateSelectedItem();
        }

        private void WeightedMultipleDetails_ParentChanged(object sender, EventArgs e)
        {
            if (Parent != null)
            {
                // validate the control's input and invalidate the selected item in the survey view
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

            // check for an empty DataGridView
            if (WeightedMultiChoiceView.Rows.Count == 0)
            {
                MainForm.ErrorMsg = Properties.Resources.sWeightedMultiNoChoicesException;
                HasErrors = true;
                return;
            }
            for (int ctr = 0; ctr < WeightedMultiChoiceView.Rows.Count; ctr++)
            {
                o = WeightedMultiChoiceView.Rows[ctr].Cells[0].Value;
                // check for an empty weight, but do not return if it is encountered as a fatal error
                // might follow
                if (o == null)
                {
                    MainForm.ErrorMsg = String.Format(Properties.Resources.sWeightedMultiWeightEmptyException, ctr + 1);
                    HasErrors = true;
                }
                // try to convert the weight to an integer
                try
                {
                    Convert.ToInt32(o.ToString());
                }
                // catch non-numerical data
                catch (FormatException)
                {
                    MainForm.ErrorMsg = String.Format(Properties.Resources.sWeightedMultiWeightFormatException, ctr + 1);
                    HasFatalErrors = true;
                    return;
                }
                // catch an out of range value
                catch (OverflowException)
                {
                    MainForm.ErrorMsg = String.Format(Properties.Resources.sWeightedMultiWeightOverflowException, ctr + 1);
                    HasFatalErrors = true;
                    return;
                }
                // check for an empty choice cell but, as with above, do not return if encountered
                o = WeightedMultiChoiceView.Rows[ctr].Cells[1].Value;
                if (o == null)
                {
                    MainForm.ErrorMsg = String.Format(Properties.Resources.sWeightedMultiChoiceEmptyException, ctr + 1);
                    HasErrors = true;
                }
            }

            // if no non-fatal errors were encountered, clear the error message
            if (!HasErrors)
                MainForm.ErrorMsg = String.Empty;
        }
    }
}

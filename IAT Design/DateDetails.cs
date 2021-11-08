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
    /// DateDetails is a control that allows for the definition of a CDateResponse response type
    /// </summary>
    public partial class DateDetails : UserControl
    {
        // has errors that do not prevent the rendering of the response type in the SurveyPanel
        private bool _HasErrors;
        /// <summary>
        /// gets or sets whether the form contains response data that has errors that do not prevent the response
        /// from being rendered in the SurveyPanel
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
        /// gets or sets the response object that is being displayed in the control.
        /// </summary>
        public CDateResponse Response
        {
            get
            {
                CDateResponse r = GetResponse();
                r.HasErrors = HasErrors;
                return r;
            }
            set
            {
                CDateResponse r = value;
                if (r.HasStartDate)
                {
                    EnableStartDateCheck.Checked = true;
                    StartDate.Enabled = true; 
                    StartDate.Value = r.StartDate;
                }
                else
                {
                    EnableStartDateCheck.Checked = false;
                    StartDate.Enabled = false;
                }
                if (r.HasEndDate)
                {
                    EnableEndDateCheck.Checked = true;
                    EndDate.Enabled = true;
                    EndDate.Value = r.EndDate;
                }
                else
                {
                    EnableEndDateCheck.Checked = false;
                    EndDate.Enabled = false;
                }
            }
        }

        /// <summary>
        /// Instantiates a DateDetails object
        /// </summary>
        public DateDetails()
        {
            InitializeComponent();
        }

        private void StartDateEnableCheck_CheckedChanged(object sender, EventArgs e)
        {
            // enable or diable the StartDate control depending on whether EnableStartDateCheck is checked
            if (EnableStartDateCheck.Checked)
                StartDate.Enabled = true;
            else
                StartDate.Enabled = false;

            // validate input and invalidate the currently selected node in the survey panel
            ValidateInput();
            MainForm.m_SurveyPanel.InvalidateSelectedItem();
        }

        private void EnableEndDateCheck_CheckedChanged(object sender, EventArgs e)
        {
            // enable or disable the EndDate control depending on whether EnableEndDateCheck is checked
            if (EnableEndDateCheck.Checked)
                EndDate.Enabled = true;
            else
                EndDate.Enabled = false;

            // validate the control and invalidate the currently selected node in the survey panel
            ValidateInput();            
            MainForm.m_SurveyPanel.InvalidateSelectedItem();
        }

        /// <summary>
        /// Builds a CDateResponse object and fills it with the data in the control
        /// </summary>
        /// <returns>A CDateResponse object that contains the data in the control</returns>
        private CDateResponse GetResponse()
        {
            CDateResponse Response = new CDateResponse();
            if (EnableStartDateCheck.Checked)
                Response.StartDate = StartDate.Value;
            if (EnableEndDateCheck.Checked)
                Response.EndDate = EndDate.Value;
            Response.HasErrors = false;
            return Response;
        }

        private void StartDate_ValueChanged(object sender, EventArgs e)
        {
            // validate the control and invalidate the currently selected node in the survey panel
            ValidateInput();
            MainForm.m_SurveyPanel.InvalidateSelectedItem();
        }

        private void EndDate_ValueChanged(object sender, EventArgs e)
        {
            // validate the control and invalidate the currently selected node in the survey panel
            ValidateInput();
            MainForm.m_SurveyPanel.InvalidateSelectedItem();
        }

        private void DateDetails_ParentChanged(object sender, EventArgs e)
        {
            // if the control is being shown, validate the control and invalidate the currently selected node in the survey panel
            if (Parent != null)
            {
                ValidateInput();
                MainForm.m_SurveyPanel.InvalidateSelectedItem();
            }
        }

        /// <summary>
        /// Validates the control's child controls, setting the error message in the main form if appropriate
        /// </summary>
        public void ValidateInput()
        {
            HasErrors = false;

            // check for an EndDate Prior to a StartDate
            if ((EnableStartDateCheck.Checked) && (EnableEndDateCheck.Checked))
                if (DateTime.Compare(StartDate.Value.Date, EndDate.Value.Date) > 0)
                {
                    MainForm.ErrorMsg = Properties.Resources.sInvertedDateOrderException;
                    HasErrors = true;
                    return;
                }

            // no errors
            MainForm.ErrorMsg = String.Empty;
        }

        private void DateDetails_Load(object sender, EventArgs e)
        {
            EnableStartDateCheck.Checked = false;
            EnableEndDateCheck.Checked = false;
            StartDate.Enabled = false;
            EndDate.Enabled = false;
        }
    }
}

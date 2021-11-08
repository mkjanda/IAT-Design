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
    /// TrueFalseDetails provides a control that allows the user to define a true/false or Boolean response type
    /// </summary>
    public partial class TrueFalseDetails : UserControl
    {
        // Error flags.  Fatal errors prevent the control's data from being rendered in the SurveyView
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

        /// <summary>
        /// gets of sets the response displayed in the control.  get returns "null" if the control data has fatal errors
        /// </summary>
        public CBoolResponse Response
        {
            get
            {
                if (HasFatalErrors)
                    return null;
                return GetResponse();
            }
            set
            {
                CBoolResponse r = value;
                TrueEdit.Text = r.TrueStatement;
                FalseEdit.Text = r.FalseStatement;
            }
        }

        /// <summary>
        /// The default constructor
        /// </summary>
        public TrueFalseDetails()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Instantiates a new CBoolResponse object, fills it with the control's data, and returns it
        /// </summary>
        /// <returns>A new CBoolResponse object filled with the control's data</returns>
        private CBoolResponse GetResponse()
        {
            CBoolResponse r = new CBoolResponse(TrueEdit.Text, FalseEdit.Text);
            r.HasErrors = HasErrors;
            return r;
        }

        private void TrueEdit_TextChanged(object sender, EventArgs e)
        {
            // validate the control's data and invalidate the currently selected item in the SurveyView
            ValidateInput();
            MainForm.m_SurveyPanel.InvalidateSelectedItem();
        }

        
        private void FalseEdit_TextChanged(object sender, EventArgs e)
        {
            // validate the control's data and invalidate the currently selected item in the SurveyView
            ValidateInput();
            MainForm.m_SurveyPanel.InvalidateSelectedItem();
        }

        private void TrueFalseDetails_ParentChanged(object sender, EventArgs e)
        {
            if (Parent != null)
            {
                // validate the control's data and invalidate the currently selected item in the SurveyView
                ValidateInput();
                MainForm.m_SurveyPanel.InvalidateSelectedItem();
            }
        }

        /// <summary>
        /// Validates the controls data, setting error flags and the error message in the main window as appropriate
        /// </summary>
        public void ValidateInput()
        {
            HasErrors = false;
            // check for a blank true statement
            if (TrueEdit.Text == String.Empty)
            {
                MainForm.ErrorMsg = Properties.Resources.sTrueStatementEmptyException;
                HasErrors = true;
                return;
            }
            // check for a blank false statement
            if (FalseEdit.Text == String.Empty)
            {
                MainForm.ErrorMsg = Properties.Resources.sFalseStatementEmptyException;
                HasErrors = true;
                return;
            }
            // check for duplicate values for the true and false statements
            if (TrueEdit.Text == FalseEdit.Text)
            {
                MainForm.ErrorMsg = Properties.Resources.sTrueFalseDuplicateValueException;
                HasErrors = true;
                return;
            }

            // no errors
            MainForm.ErrorMsg = String.Empty;
        }

    }
}

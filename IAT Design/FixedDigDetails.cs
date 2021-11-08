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
    /// FixedDigDetails is a control that allows for the definition of a CFixedDigResponse response type
    /// </summary>
    public partial class FixedDigDetails : UserControl
    {
        // error strings
        private static String sFormatException = Properties.Resources.sNumDigitsFormatException;
        private static String sOverflowException = Properties.Resources.sNumDigitsOverflowException;
        private static String sExceeded4000Exception = Properties.Resources.sNumDigitsExceeds4000Exception;
        
        // _HasErrors indicicates the form has errors that don't prevent the control's data from being rendered in
        // the SurveyView.  _HasFatal errors indiciates the form does have herrors that prevent the control's data
        // from being rendered in Survey View
        private bool _HasErrors, _HasFatalErrors;

        /// <summary>
        /// gets or sets whether the control's data has errors that don't prevent it from being rendered in SurveyView.
        /// Note:  Setting this value to "false" will set HasFatalErrors to "false"
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
        /// gets or sets whether the control's data has errors that prevent it from being rendered in SurveyView.
        /// Note:  Setting this value to "true" will set HasErrors to "true"
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
        /// gets or sets the response object depicted by the form's data
        /// </summary>
        public CFixedDigResponse Response
        {
            get
            {
                if (HasFatalErrors)
                    return null;
                return GetResponse();
            }
            set
            {
                CFixedDigResponse r = value;
                if (r.NumDigs > -1)
                    NumDigits.Text = r.NumDigs.ToString();
                else
                    NumDigits.Text = String.Empty;
            }
        }

        /// <summary>
        /// The default constructor
        /// </summary>
        public FixedDigDetails()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Consstructs a CFixedDigitResponse object and fill's it with the control's data.  Note: This function
        /// preforms no error checking.
        /// </summary>
        /// <returns>A CFixedDigitResponse objects that contains's this control's data</returns>
        private CFixedDigResponse GetResponse()
        {
            CFixedDigResponse r = new CFixedDigResponse(Convert.ToInt32(NumDigits.Text));
            r.HasErrors = HasErrors;
            return r;
        }

        private void NumDigits_TextChanged(object sender, EventArgs e)
        {
            // validate input and invalidate the currently selected item in the SurveyPanel
            ValidateInput();
            MainForm.m_SurveyPanel.InvalidateSelectedItem();
        }

        private void FixedDigDetails_ParentChanged(object sender, EventArgs e)
        {
            if (Parent != null)
            {
                // validate input and invalidate the currently selected item in the SurveyPanel
                ValidateInput();
                MainForm.m_SurveyPanel.InvalidateSelectedItem();
            }
        }

        /// <summary>
        /// Validates the control's data, setting the error string in the main window and the control's error
        /// flags as appropriate.
        /// </summary>
        public void ValidateInput()
        {
            HasErrors = false;
            int n;
            
            // try to convert NumDigits to an integer
            try
            {
                n = Convert.ToInt32(NumDigits.Text);
            }
            // catch non-numerical data
            catch (FormatException)
            {
                MainForm.ErrorMsg = sFormatException;
                HasFatalErrors = true;
                return;
            }
            // catch an out of range value
            catch (OverflowException)
            {
                MainForm.ErrorMsg = sOverflowException;
                HasFatalErrors = true;
                return;
            }
            // catch a non-positive value
            if (n < 1)
            {
                MainForm.ErrorMsg = sFormatException;
                HasFatalErrors = true;
                return;
            }
            // catch a value that exceeds 4000
            if (n > 4000)
            {
                MainForm.ErrorMsg = sExceeded4000Exception;
                HasErrors = true;
                return;
            }
            // no error
            MainForm.ErrorMsg = String.Empty;
        }
    }
}

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
    /// FixedLengthDetails is a control that allows for the definition of a CFixedLengthResponse response type
    /// </summary>
    public partial class FixedLengthDetails : UserControl
    {
        // fatal errors prevent the control's data from being rendered in the SurveyView
        private bool _HasErrors, _HasFatalErrors;

        /// <summary>
        /// gets or sets whether the control has errors that do not prevent its data from being rendered in the
        /// SurveyView.  Note: Setting this value to "false" sets HasFatalErrors to "false"
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
        /// gets or sets whether the control has errors that prevent its data from being rendered in the 
        /// SurveyView.  Note: Setting this value to "true" sets HasErrors to "true"
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
        /// gets or sets the response object depicted by the control.  Note: set returns null if the control
        /// contains fatal errors
        /// </summary>
        public CFixedLengthResponse Response
        {
            get
            {
                if (HasFatalErrors)
                    return null;
                return GetResponse();
            }
            set
            {
                CFixedLengthResponse r = value;
                if (r.Length > -1)
                    TextLength.Text = r.Length.ToString();
                else
                    TextLength.Text = String.Empty;
            }
        }

        /// <summary>
        /// the default contstructor
        /// </summary>
        public FixedLengthDetails()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Instantiates and returns a CFixedLengthResponse object that corresponds to the state of the control
        /// </summary>
        /// <returns>A new CFixedLengthResponse object that corresponds to the state of the control</returns>
        private CFixedLengthResponse GetResponse()
        {
            CFixedLengthResponse r = new CFixedLengthResponse(Convert.ToInt32(TextLength.Text));
            r.HasErrors = HasErrors;
            return r;
        }

        private void TextLength_TextChanged(object sender, EventArgs e)
        {
            // validate the control and invalidate the selected survey item in the SurveyPanel
            ValidateInput();
            MainForm.m_SurveyPanel.InvalidateSelectedItem();
        }

        private void FixedLengthDetails_ParentChanged(object sender, EventArgs e)
        {
            if (Parent != null)
            {
                // validate the control and invalidate the selected survey item in the SurveyPanel
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
            int n;
            // try to convert TextLength to an integer
            try
            {
                n = Convert.ToInt32(TextLength.Text);
            }
            // catch non-numerical data
            catch (FormatException)
            {
                MainForm.ErrorMsg = Properties.Resources.sTextLengthFormatException;
                HasFatalErrors = true;
                return;
            }
            // catch an out of range value
            catch (OverflowException)
            {
                MainForm.ErrorMsg = Properties.Resources.sTextLengthOverflowException;
                HasFatalErrors = true;
                return;
            }
            // catch a non-positive value
            if (n < 1)
            {
                MainForm.ErrorMsg = Properties.Resources.sTextLengthFormatException;
                HasFatalErrors = true;
                return;
            }
            // catch a value over 4000
            if (n > 4000)
            {
                MainForm.ErrorMsg = Properties.Resources.sTextLengthExceeds4000Exception;
                HasErrors = true;
                return;
            }
            // no errors
            MainForm.ErrorMsg = String.Empty;
        }
    }
}

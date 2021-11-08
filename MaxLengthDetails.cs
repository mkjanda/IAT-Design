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
    /// MaxLengthDetails is a control that provides for the definition of a maximum length of text response type
    /// </summary>
    public partial class MaxLengthDetails : UserControl
    {
        // error flags.  fatal errors indicate the control's data cannot be rendered in the survey view
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
        public CMaxLengthResponse Response
        {
            get
            {
                if (HasFatalErrors)
                    return null;
                return GetResponse();
            }
            set
            {
                CMaxLengthResponse r = value;
                if (r.MaxLength > -1)
                    MaxLength.Text = r.MaxLength.ToString();
                else
                    MaxLength.Text = String.Empty;
            }
        }

        /// <summary>
        /// The default constructor
        /// </summary>
        public MaxLengthDetails()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Instantiates and returns a new CMaxLengthResponse object with the control's data
        /// </summary>
        /// <returns>A new CMaxLengthResponse object on success, otherwise "null"</returns>
        private CMaxLengthResponse GetResponse()
        {
            CMaxLengthResponse r = new CMaxLengthResponse(Convert.ToInt32(MaxLength.Text));
            r.HasErrors = HasErrors;
            return r;
        }

        private void MaxLength_TextChanged(object sender, EventArgs e)
        {
            // validate the control and invalidate the selected item in the survey view
            ValidateInput();
            MainForm.m_SurveyPanel.InvalidateSelectedItem();
        }

        private void MaxLengthDetails_ParentChanged(object sender, EventArgs e)
        {
            if (Parent != null)
            {
                // validate the control and invalidate the selected item in the survey view
                ValidateInput();
                MainForm.m_SurveyPanel.InvalidateSelectedItem();
            }
        }

        /// <summary>
        /// Validates the control's data, setting the appropriate error flags and the error messagee in the main window
        /// </summary>
        public void ValidateInput()
        {
            HasErrors = false;
            int n;
            // try to convert MaxLength to an integer
            try
            {
                n = Convert.ToInt32(MaxLength.Text);
            }
            // catch non-numerical input
            catch (FormatException)
            {
                MainForm.ErrorMsg = Properties.Resources.sMaxLengthFormatException;
                HasFatalErrors = true;
                return;
            }
            // catch an out of range value
            catch (OverflowException)
            {
                MainForm.ErrorMsg = Properties.Resources.sMaxLengthOverflowException;
                HasFatalErrors = true;
                return;
            }
            // test for a value less than 1
            if (n < 1)
            {
                MainForm.ErrorMsg = Properties.Resources.sMaxLengthFormatException;
                HasErrors = true;
                return;
            }
            // test for a value greater than 4000
            if (n > 4000)
            {
                MainForm.ErrorMsg = Properties.Resources.sMaxLengthExceeds4000Exception;
                HasErrors = true;
                return;
            }
            // no error
            MainForm.ErrorMsg = String.Empty;
        }
    }
}

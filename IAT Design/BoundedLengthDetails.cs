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
    /// BoundedLengthDetails provides a control that enables the user to enter data for a CBoundedLength response type.
    /// A CBoundedLength response type provides for a response that consists of text that with a specified minimum
    /// and maximumm allowable length
    /// </summary>
    public partial class BoundedLengthDetails : UserControl
    {
        // fatal errors preclude the response from being rendered in the SurveyPanel class
        private bool _HasErrors, _HasFatalErrors;

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
        /// gets or sets whether to control has errors that do not prevent it from being rendered in the SurveyView.  Note:
        /// setting this value to "false" will set HasFatalErros to "false"
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
        /// gets or sets whether to control has errors that prevent it from being rendered in the SurveyView.  Note:
        /// setting this value to "true" will set HasErrors to "true"
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
        /// get returns null in the event of a fatal error, otherwise a new CBoundedLengthResponse is instantiated and returned
        /// set fills in the control's child controls with data from a given CBoundedLengthResponse object
        /// Note: the get value presupposes that user input has already been validated via the ValidateInput member function 
        /// </summary>
        public CBoundedLengthResponse Response
        {
            get
            {
                if (HasFatalErrors)
                    return null;
                return GetResponse();
            }
            set
            {
                CBoundedLengthResponse r = value;
                if (r.MinLength != CBoundedLengthResponse.InvalidValue)
                    MinLength.Text = r.MinLength.ToString();
                else
                    MinLength.Text = String.Empty;
                if (r.MaxLength != CBoundedLengthResponse.InvalidValue)
                    MaxLength.Text = r.MaxLength.ToString();
                else
                    MaxLength.Text = String.Empty;
            }
        }

        /// <summary>
        /// default constructor 
        /// </summary>
        public BoundedLengthDetails()
        {
            InitializeComponent();
        }

         
        /// <summary>
        /// instantiates a CBoundedResponse object and fills it in with data from the child controls
        /// performs no error-checking 
        /// </summary>
        /// <returns></returns>
        private CBoundedLengthResponse GetResponse()
        {
            CBoundedLengthResponse r = new CBoundedLengthResponse(Convert.ToInt32(MinLength.Text), Convert.ToInt32(MaxLength.Text));
            r.HasErrors = HasErrors;
            return r;
        }

        private void BoundedLengthDetails_ParentChanged(object sender, EventArgs e)
        {
            // if the control is being added to a parent window, validate the child control data and 
            // invalidate the currently selected survey item node in the CSurveyPanel object
            if (Parent != null)
            {
                ValidateInput();
                MainForm.m_SurveyPanel.InvalidateSelectedItem();
            }
        }

        
        private void MinLength_TextChanged(object sender, EventArgs e)
        {
            // validate the child control data and invalidate the currently selected survey item node in the CSurveyPanel object
            ValidateInput();
            MainForm.m_SurveyPanel.InvalidateSelectedItem();
        }

        
        private void MaxLength_TextChanged(object sender, EventArgs e)
        {
            // validate the child control data and invalidate the currently selected survey item node in the CSurveyPanel object
            ValidateInput();
            MainForm.m_SurveyPanel.InvalidateSelectedItem();
        }

 
        /// <summary>
        /// validates the child controls and, if an error is present, changes the error message at the bottom of the main window 
        /// </summary>
        public void ValidateInput()
        {
            // clear the error flags
            HasErrors = false;
            int min, max;

            // attempt to convert the given minimum length to a numerical value
            try
            {
                min = Convert.ToInt32(MinLength.Text);
            }
            // catch non-numerical data
            catch (FormatException)
            {
                MainForm.ErrorMsg = Properties.Resources.sMinLengthFormatException;
                HasFatalErrors = true;
                return;
            }
            // catch a number that exceeds the capacity of an Int32
            catch (OverflowException)
            {
                MainForm.ErrorMsg = Properties.Resources.sMinLengthOverflowException;
                HasFatalErrors = true;
                return;
            }
            // attempt to convert the given maximum length to a numerical value
            try
            {
                max = Convert.ToInt32(MaxLength.Text);
            }
            // catch non-numerical data
            catch (FormatException)
            {
                MainForm.ErrorMsg = Properties.Resources.sMaxLengthFormatException;
                HasFatalErrors = true;
                return;
            }
            // catch a nuumber that exceeds the capacity of an Int32
            catch (OverflowException)
            {
                MainForm.ErrorMsg = Properties.Resources.sMaxLengthOverflowException;
                HasFatalErrors = true;
                return;
            }
            // test for a minimum length less than 1
            if (min < 1)
            {
                MainForm.ErrorMsg = Properties.Resources.sMinLengthFormatException;
                HasFatalErrors = true;
                return;
            }
            // test for a maximum length less than 1
            if (max < 1)
            {
                MainForm.ErrorMsg = Properties.Resources.sMaxLengthFormatException;
                HasFatalErrors = true;
                return;
            }
            // test for a minimum length greater than the given maximum length
            if (min > max)
            {
                MainForm.ErrorMsg = Properties.Resources.sLengthValuesInvertedException;
                HasErrors = true;
                return;
            }
            // no errors.  set the main window's error message to the empty string
            MainForm.ErrorMsg = String.Empty;
        }
    }
}

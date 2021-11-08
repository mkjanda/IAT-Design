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
    /// BoundedNumberDetails provides a control that allows for the user to define a CBoundedNumResponse response type.
    /// CBoundedNumResponse is a response type that allows for the entry of an integer with a specified minimum
    /// and maximum allowable value.
    /// </summary>
    public partial class BoundedNumberDetails : UserControl
    {
        // fatal errors preclude the response from being rendered in the SurveyPanel class
        private bool _HasErrors, _HasFatalErrors;
        
        /// <summary>
        /// get the main window
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
        /// get returns null in the event of a fatal error, otherwise a new CBoundedNumResponse is instantiated and returned
        /// set fills in the control's child controls with data from a given CBoundedNumResponse object
        /// Note: the get value presupposes that user input has already been validated via the ValidateInput member function 
        /// </summary>
        public CBoundedNumResponse Response
        {
            get
            {
                if (HasFatalErrors)
                    return null;
                return GetResponse();
            }
            set
            {
                CBoundedNumResponse r = value;
                if (r.MinValue != int.MinValue)
                    MinValue.Text = r.MinValue.ToString();
                else
                    MinValue.Text = String.Empty;
                if (r.MaxValue != int.MinValue)
                    MaxValue.Text = r.MaxValue.ToString();
                else
                    MaxValue.Text = String.Empty;
            }
        }

        /// <summary>
        /// the default contstructor 
        /// </summary>
        public BoundedNumberDetails()
        {
            InitializeComponent();
        }

        /// <summary>
        /// instantiates and returns a CBoundedNumResponse object with the control's data.  
        /// Note: does not perform any error-checking 
        /// </summary>
        /// <returns>A CBoundedNumResponse object that contains the control's data</returns>
        private CBoundedNumResponse GetResponse()
        {
            CBoundedNumResponse r = new CBoundedNumResponse(Convert.ToInt32(MinValue.Text), Convert.ToInt32(MaxValue.Text));
            r.HasErrors = HasErrors;
            return r;
        }

        // validates user input and invalidates the selected survey item node in the CSurveyPanel object
        private void MinValue_TextChanged(object sender, EventArgs e)
        {
            ValidateInput();
            MainForm.m_SurveyPanel.InvalidateSelectedItem();
        }

        // validates user input and invalidates the selected survey item node in the CSurveyPanel object
        private void MaxValue_TextChanged(object sender, EventArgs e)
        {
            ValidateInput();
            MainForm.m_SurveyPanel.InvalidateSelectedItem();
        }

        // validates user input and invalidates the selected survey item node in the CSurveyPanel object
        private void BoundedNumberDetails_ParentChanged(object sender, EventArgs e)
        {
            if (Parent != null)
            {
                ValidateInput();
                MainForm.m_SurveyPanel.InvalidateSelectedItem();
            }
        }
 
        /// <summary>
        /// validates the user input, setting the appropriate error flag(s) and, in the event of an error,
        /// setting the error messsage at the bottom of the main window 
        /// </summary>
        public void ValidateInput()
        {
            // set both HasErrors and HasFatalErrors to false before error-check
            HasErrors = false;
            int min, max;
            // try to convert the Minimum Value to an integer
            try
            {
                min = Convert.ToInt32(MinValue.Text);
            }
            // catch non-numerical data
            catch (FormatException)
            {
                MainForm.ErrorMsg = Properties.Resources.sMinValueFormatException;
                HasFatalErrors = true;
                return;
            }
            // catch data that exceeds the capacity of an Int32
            catch (OverflowException)
            {
                MainForm.ErrorMsg = Properties.Resources.sMinValueOverflowException;
                HasFatalErrors = true;
                return;
            }
            // try to convert the Maximum Value to an integer
            try
            {
                max = Convert.ToInt32(MaxValue.Text);
            }
            // catch non-numerical data
            catch (FormatException)
            {
                MainForm.ErrorMsg = Properties.Resources.sMaxValueFormatException;
                HasFatalErrors = true;
                return;
            }
            // catch data that exceeds the capacity of an Int32
            catch (OverflowException)
            {
                MainForm.ErrorMsg = Properties.Resources.sMaxValueOverflowException;
                HasFatalErrors = true;
                return;
            }
            // check for a Minimum Value that is less than the given Maximum Value
            if (min > max)
            {
                MainForm.ErrorMsg = Properties.Resources.sBoundingValuesInvertedException;
                HasErrors = true;
                return;
            }
            // no error so set the error message in the main window to the empty string
            MainForm.ErrorMsg = String.Empty;
        }
    }
}

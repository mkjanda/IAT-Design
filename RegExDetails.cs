using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace IATClient
{
    /// <summary>
    /// RegExDetails provides a control for defining a regular expression validated response type
    /// </summary>
    public partial class RegExDetails : UserControl
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
        /// gets or sets the response object displayed by the control.  get returns null if the control has a fatal error.
        /// Note: This property does not perform control validation.
        /// </summary>
        public CRegExResponse Response
        {
            get
            {
                if (HasFatalErrors)
                    return null;
                return GetResponse();
            }
            set
            {
                CRegExResponse r = value;
                RegExEdit.Text = r.RegEx;
                TestInput.Text = String.Empty;
                Test();
            }
        }

        /// <summary>
        /// The default constructor
        /// </summary>
        public RegExDetails()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Tests the value in the ValueEdit TextBox against the regular expression in the RegExEdit TextBox and displays a 
        /// message on the control indicating whether the test value matches the regular expression, the test value does not
        /// match the regular expression, or the regular expression is invalid
        /// </summary>
        private void Test()
        {
            try
            {
                if (Regex.IsMatch(TestInput.Text, RegExEdit.Text, RegexOptions.Singleline))
                {
                    TestResultLabel.Text = "Valid Input";
                    TestResultLabel.ForeColor = Color.Green;
                }
                else
                {
                    TestResultLabel.Text = "Invalid Input";
                    TestResultLabel.ForeColor = Color.Red;
                }
            }
            catch (System.ArgumentException)
            {
                TestResultLabel.Text = "Invalid Expression";
                TestResultLabel.ForeColor = Color.Black;
            }
            TestResultLabel.Location = new Point((this.Width - TestResultLabel.Width) >> 1, TestResultLabel.Location.Y);
        }

        private void RegExEdit_TextChanged(object sender, EventArgs e)
        {
            // test the user supplied value
            Test();

            // validate the control and invalidate the selected item in the survey view
            ValidateInput();
            MainForm.m_SurveyPanel.InvalidateSelectedItem();
        }

        private void TestInput_TextChanged(object sender, EventArgs e)
        {
            // test the user supplied value
            Test();
        }

        /// <summary>
        /// Instantiates a new CRegExResponse object using the data in the control
        /// </summary>
        /// <returns>A new CRegResponse object that contains the data in the control</returns>
        private CRegExResponse GetResponse()
        {
            CRegExResponse r = new CRegExResponse(RegExEdit.Text);
            r.HasErrors = HasErrors;
            return r;
        }

        /// <summary>
        /// Validates the control's data, setting error flags and changing the error message in the main window
        /// as appropriate.
        /// </summary>
        public void ValidateInput()
        {
            HasErrors = false;
            // test for an empty regular expression
            if (RegExEdit.Text == String.Empty)
            {
                MainForm.ErrorMsg = Properties.Resources.sRegExEmptyException;
                HasErrors = true;
                return;
            }
            // try to test the user suppled input value against the regular expression to see if an ArgumentException is thrown,
            // indicating an invalid regular expression
            try
            {
                Regex.IsMatch(TestInput.Text, RegExEdit.Text, RegexOptions.Singleline);
            }
            catch (System.ArgumentException)
            {
                HasErrors = true;
                MainForm.ErrorMsg = Properties.Resources.sRegExInvalidException;
                return;
            }

            // no errors
            MainForm.ErrorMsg = String.Empty;
        }

        private void RegExDetails_ParentChanged(object sender, EventArgs e)
        {
            if (Parent != null)
            {
                ValidateInput();
                MainForm.m_SurveyPanel.InvalidateSelectedItem();
            }
        }

        private void RegExDetails_Load(object sender, EventArgs e)
        {
            TestResultLabel.Text = String.Empty;
        }

    }
}

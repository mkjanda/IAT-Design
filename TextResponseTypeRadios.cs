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
    /// TextResponseTypeRadios provides a control that allows the user to choose a type of text response.  Additionally,
    /// it makes request of its parent window to display the apporpriate child control for the modification of the
    /// selected type of text response.  Its parent must be of type ResponsePanel.  
    /// </summary>
    public partial class TextResponseTypeRadios : UserControl
    {
        /// <summary>
        /// An enumerated type of the various text response types
        /// </summary>
        public enum ETextResponseType { none, BoundedLength, BoundedNumber, FixedDigit, FixedLength, MaxLength, RegEx };
        
        // the subpanel that the parent ResponsePanel class has been asked to display
        private ETextResponseType _ActiveSubPanel;

        /// <summary>
        /// gets or sets the text response details control that the parent ResponsePanel displays
        /// </summary>
        public ETextResponseType ActiveSubPanel
        {
            get
            {
                return _ActiveSubPanel;
            }
            set
            {
                switch (value)
                {
                    case ETextResponseType.BoundedLength:
                        BoundedLengthRadio.Checked = true;
                        break;

                    case ETextResponseType.BoundedNumber:
                        BoundedNumberRadio.Checked = true;
                        break;

                    case ETextResponseType.FixedDigit:
                        FixedDigitRadio.Checked = true;
                        break;

                    case ETextResponseType.FixedLength:
                        FixedLengthRadio.Checked = true;
                        break;

                    case ETextResponseType.RegEx:
                        RegExRadio.Checked = true;
                        break;

                    case ETextResponseType.MaxLength:
                        MaxLengthRadio.Checked = true;
                        break;
                }
            }
        }

        /// <summary>
        /// The default constructor.
        /// </summary>
        public TextResponseTypeRadios()
        {
            InitializeComponent();
            _ActiveSubPanel = ETextResponseType.none;
        }

        private void BoundedLengthRadio_CheckedChanged(object sender, EventArgs e)
        {
            // Tell the parent ResponsePanel to show or hide the BoundedLengthDetails control depending on the checked
            // state of the radio button
            if (BoundedLengthRadio.Checked)
            {
                ((ResponsePanel)Parent).ShowBoundedLengthDetails();
                _ActiveSubPanel = ETextResponseType.BoundedLength;
            }
            else
                ((ResponsePanel)Parent).HideBoundedLengthDetails();
        }

        private void BoundedNumberRadio_CheckedChanged(object sender, EventArgs e)
        {
            // Tell the parent ResponsePanel to show or hide the BoundedNumberDetails control depending on the checked
            // state of the radio button
            if (BoundedNumberRadio.Checked)
            {
                ((ResponsePanel)Parent).ShowBoundedNumberDetails();
                _ActiveSubPanel = ETextResponseType.BoundedNumber;
            }
            else
                ((ResponsePanel)Parent).HideBoundedNumberDetails();
        }

        private void FixedDigitRadio_CheckedChanged(object sender, EventArgs e)
        {
            // Tell the parent ResponsePanel to show or hide the FixedDigDetails control depending on the checked
            // state of the radio button
            if (FixedDigitRadio.Checked)
            {
                ((ResponsePanel)Parent).ShowFixedDigitDetails();
                _ActiveSubPanel = ETextResponseType.FixedDigit;
            }
            else
                ((ResponsePanel)Parent).HideFixedDigitDetails();
        }

        private void FixedLengthRadio_CheckedChanged(object sender, EventArgs e)
        {
            // Tell the parent ResponsePanel to show or hide the FixedLengthDetails control depending on the checked
            // state of the radio button
            if (FixedLengthRadio.Checked)
            {
                ((ResponsePanel)Parent).ShowFixedLengthDetails();
                _ActiveSubPanel = ETextResponseType.FixedLength;
            }
            else
                ((ResponsePanel)Parent).HideFixedLengthDetails();
        }

        private void RegExRadio_CheckedChanged(object sender, EventArgs e)
        {
            // Tell the parent ResponsePanel to show or hide the RegExDetails control depending on the checked
            // state of the radio button
            if (RegExRadio.Checked)
            {
                ((ResponsePanel)Parent).ShowRegExDetails();
                _ActiveSubPanel = ETextResponseType.RegEx;
            }
            else
                ((ResponsePanel)Parent).HideRegExDetails();
        }

        
        private void MaxLengthRadio_CheckedChanged(object sender, EventArgs e)
        {
            // Tell the parent ResponsePanel to show or hide the MaxLengthDetails control depending on the checked
            // state of the radio button
            if (MaxLengthRadio.Checked)
            {
                ((ResponsePanel)Parent).ShowMaxLengthDetails();
                _ActiveSubPanel = ETextResponseType.MaxLength;
            }
            else
                ((ResponsePanel)Parent).HideMaxLengthDetails();
        }

        /// <summary>
        /// Tells the parent ResponsePanel to hide the currently active text type response details control
        /// </summary>
        public void HideSubPanel()
        {
            switch (ActiveSubPanel)
            {
                case ETextResponseType.BoundedLength:
                    ((ResponsePanel)Parent).HideBoundedLengthDetails();
                    break;

                case ETextResponseType.BoundedNumber:
                    ((ResponsePanel)Parent).HideBoundedNumberDetails();
                    break;

                case ETextResponseType.FixedDigit:
                    ((ResponsePanel)Parent).HideFixedDigitDetails();
                    break;

                case ETextResponseType.FixedLength:
                    ((ResponsePanel)Parent).HideFixedLengthDetails();
                    break;

                case ETextResponseType.RegEx:
                    ((ResponsePanel)Parent).HideRegExDetails();
                    break;

                case ETextResponseType.MaxLength:
                    ((ResponsePanel)Parent).HideMaxLengthDetails();
                    break;
            }
        }

        /// <summary>
        /// Tells the parent ResposnePanel to display the currently active text type response details control
        /// </summary>
        public void ShowSubPanel()
        {
            switch (ActiveSubPanel)
            {
                case ETextResponseType.BoundedLength:
                    ((ResponsePanel)Parent).ShowBoundedLengthDetails();
                    break;

                case ETextResponseType.BoundedNumber:
                    ((ResponsePanel)Parent).ShowBoundedNumberDetails();
                    break;

                case ETextResponseType.FixedDigit:
                    ((ResponsePanel)Parent).ShowFixedDigitDetails();
                    break;

                case ETextResponseType.FixedLength:
                    ((ResponsePanel)Parent).ShowFixedLengthDetails();
                    break;

                case ETextResponseType.RegEx:
                    ((ResponsePanel)Parent).ShowRegExDetails();
                    break;

                case ETextResponseType.MaxLength:
                    ((ResponsePanel)Parent).ShowMaxLengthDetails();
                    break;
            }
        }

    }
}

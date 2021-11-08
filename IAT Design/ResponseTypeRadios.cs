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
    /// ResponseTypeRadios provides a control with response radios for various response types and delegates that cause
    /// the appropriate child controls to be displayed in the ResponseTypeRadios parent ResponsePanel when the user
    /// selects a new response type
    /// </summary>
    public partial class ResponseTypeRadios : UserControl
    {
        /// <summary>
        /// An enumeration of the response types selectable in the ResponseTypeRadios object
        /// </summary>
        public enum EResponseType { none, Bool, Date, Likert, MultiChoice, MultiSelect, ExistsInFile, Text, WeightedMultiChoice, Instruction };
        
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
        
        // the currently selected response type radio button
        private EResponseType _ResponseType;

        /// <summary>
        /// gets or sets the currently selected ResponseType.  set also checks the appropriate response type radio button
        /// </summary>
        public EResponseType ResponseType 
        {
            get 
            {
                return _ResponseType;
            }
            set
            {
                switch (value)
                {
                    case EResponseType.Bool:
                        BoolRadio.Checked = true;
                        break;

                    case EResponseType.Date:
                        DateRadio.Checked = true;
                        break;

                    case EResponseType.Likert:
                        LikertRadio.Checked = true;
                        break;

                    case EResponseType.MultiChoice:
                        MultiChoiceRadio.Checked = true;
                        break;

                    case EResponseType.MultiSelect:
                        MultiSelectRadio.Checked = true;
                        break;

                    case EResponseType.ExistsInFile:
                        AttachFileRadio.Checked = true;
                        break;

                    case EResponseType.Text:
                        TextRadio.Checked = true;
                        break;

                    case EResponseType.WeightedMultiChoice:
                        WeightedMultiChoiceRadio.Checked = true;
                        break;

                    case EResponseType.Instruction:
                        InstructionRadio.Checked = true;
                        break;
                }
            }
        }

        /// <summary>
        /// The default constructor
        /// </summary>
        public ResponseTypeRadios()
        {
            InitializeComponent();
            _ResponseType = EResponseType.none;
        }

        private void AttachFileRadio_CheckedChanged(object sender, EventArgs e)
        {
            // set the response type and ask the parent control to display its ExistsInFileDetails object 
            // if the radio button is being checked, otherwise ask the parent to hide its ExistsInFileDetails 
            // object
            if (AttachFileRadio.Checked == true)
            {
                _ResponseType = EResponseType.ExistsInFile;
                ((ResponsePanel)Parent).ShowExistsInFileDetails();
            }
            else
                ((ResponsePanel)Parent).HideExistsInFileDetails();
        }

        private void BoolRadio_CheckedChanged(object sender, EventArgs e)
        {
            // set the response type and ask the parent control to display its TrueFalseDetails object 
            // if the radio button is being checked, otherwise ask the parent to hide its TrueFalseDetails 
            // object
            if (BoolRadio.Checked == true)
            {
                _ResponseType = EResponseType.Bool;
                ((ResponsePanel)Parent).ShowTrueFalseDetails();
            }
            else 
                ((ResponsePanel)Parent).HideTrueFalseDetails();
        }

        private void DateRadio_CheckedChanged(object sender, EventArgs e)
        {
            // set the response type and ask the parent control to display its DateDetails object 
            // if the radio button is being checked, otherwise ask the parent to hide its DateDetails 
            // object
            if (DateRadio.Checked == true)
            {
                _ResponseType = EResponseType.Date;
                ((ResponsePanel)Parent).ShowDateDetails();
            }
            else
                ((ResponsePanel)Parent).HideDateDetails();
        }

        private void LikertRadio_CheckedChanged(object sender, EventArgs e)
        {
            // set the response type and ask the parent control to display its LikertDetails object 
            // if the radio button is being checked, otherwise ask the parent to hide its LikertDetails 
            // object
            if (LikertRadio.Checked == true)
            {
                _ResponseType = EResponseType.Likert;
                ((ResponsePanel)Parent).ShowLikertDetails();
            }
            else
                ((ResponsePanel)Parent).HideLikertDetails();
        }

        private void MultiChoiceRadio_CheckedChanged(object sender, EventArgs e)
        {
            // set the response type and ask the parent control to display its MultiChoiceDetails object 
            // if the radio button is being checked, otherwise ask the parent to hide its MultiChoiceDetails 
            // object
            if (MultiChoiceRadio.Checked == true)
            {
                _ResponseType = EResponseType.MultiChoice;
                ((ResponsePanel)Parent).ShowMultiChoiceDetails();
            }
            else
                ((ResponsePanel)Parent).HideMultiChoiceDetails();
        }

        private void MultiSelectRadio_CheckedChanged(object sender, EventArgs e)
        {
            // set the response type and ask the parent control to display its MultiSelectionDetails object 
            // if the radio button is being checked, otherwise ask the parent to hide its MultiSelectionDetails 
            // object
            if (MultiSelectRadio.Checked == true)
            {
                _ResponseType = EResponseType.MultiSelect;
                ((ResponsePanel)Parent).ShowMultiSelectionDetails();
            }
            else
                ((ResponsePanel)Parent).HideMultiSelectionDetails();
        }

        private void TextRadio_CheckedChanged(object sender, EventArgs e)
        {
            // set the response type and ask the parent control to display its TextTypeRadios object 
            // if the radio button is being checked, then invalidate the currently selected item in 
            // the survey view, otherwise ask the parent to hide its TextTypeRadios object
            if (TextRadio.Checked == true)
            {
                _ResponseType = EResponseType.Text;
                ((ResponsePanel)Parent).ShowTextRadios();
                MainForm.ErrorMsg = String.Empty;
                MainForm.m_SurveyPanel.InvalidateSelectedItem();
            }
            else
                ((ResponsePanel)Parent).HideTextRadios();
        }

        private void WeightedMultiChoiceRadio_CheckedChanged(object sender, EventArgs e)
        {
            // set the response type and ask the parent control to display its WeightedMultipleDetails object 
            // if the radio button is being checked, otherwise ask the parent to hide its WeightedMultipleDetails 
            // object
            if (WeightedMultiChoiceRadio.Checked)
            {
                _ResponseType = EResponseType.WeightedMultiChoice;
                ((ResponsePanel)Parent).ShowWeightedMultipleDetails();
            }
            else
                ((ResponsePanel)Parent).HideWeightedMultipleDetails();
        }

        private void InstructionRadio_CheckedChanged(object sender, EventArgs e)
        {
            // set the response type if the radio button is being checked
            if (InstructionRadio.Checked)
            {
                _ResponseType = EResponseType.Instruction;
                MainForm.ErrorMsg = String.Empty;
                MainForm.m_SurveyPanel.InvalidateSelectedItem();
            }
        }

    }
}

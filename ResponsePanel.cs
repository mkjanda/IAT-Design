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
    /// ResponsePanel provides a control that can display the various other response controls as child
    /// controls on demand, including the ResponseTypeRadios and TextTypeRadios that allow the user
    /// to change the response type.
    /// </summary>
    public partial class ResponsePanel : UserControl
    {
        // control panel positions and sizes
        public static Size ResponsePanelSize = new Size(240, 505);
        private static Point ResponseTypeRadiosPosition = new Point(0, 0);
        private static Size ResponseTypeRadiosSize = new Size(ResponsePanelSize.Width, 234);
        private static Point TrueFalseDetailsPosition = new Point(0, ResponseTypeRadiosSize.Height);
        private static Size TrueFalseDetailsSize = new Size(ResponsePanelSize.Width, 112);
        private static Point DateDetailsPosition = TrueFalseDetailsPosition;
        private static Size DateDetailsSize = new Size(ResponsePanelSize.Width, 128);
        private static Point LikertDetailsPosition = TrueFalseDetailsPosition;
        private static Size LikertDetailsSize = new Size(ResponsePanelSize.Width, 275);
        private static Point MultiChoiceDetailsPosition = TrueFalseDetailsPosition;
        private static Size MultiChoiceDetailsSize = new Size(ResponsePanelSize.Width, 241);
        private static Point MultiSelectionDetailsPosition = TrueFalseDetailsPosition;
        private static Size MultiSelectionDetailsSize = new Size(ResponsePanelSize.Width, 241);
        private static Point ExistsInFileDetailsPosition = TrueFalseDetailsPosition;
        private static Size ExistsInFileDetailsSize = new Size(ResponsePanelSize.Width, 275);
        private static Point TextRadiosPosition = TrueFalseDetailsPosition;
        private static Size TextRadiosSize = new Size(ResponsePanelSize.Width, 166);
        private static Point BoundedLengthDetailsPosition = new Point(0, ResponseTypeRadiosSize.Height + TextRadiosSize.Height);
        private static Size BoundedLengthDetailsSize = new Size(ResponsePanelSize.Width, 83);
        private static Point BoundedNumberDetailsPosition = BoundedLengthDetailsPosition;
        private static Size BoundedNumberDetailsSize = new Size(ResponsePanelSize.Width, 83);
        private static Point FixedDigitDetailsPosition = BoundedLengthDetailsPosition;
        private static Size FixedDigitDetailsSize = new Size(ResponsePanelSize.Width, 55);
        private static Point FixedLengthDetailsPosition = BoundedLengthDetailsPosition;
        private static Size FixedLengthDetailsSize = new Size(ResponsePanelSize.Width, 55);
        private static Point MaxLengthDetailsPosition = BoundedLengthDetailsPosition;
        private static Size MaxLengthDetailsSize = new Size(ResponsePanelSize.Width, 55);
        private static Point WeightedMultipleDetailsPosition = TrueFalseDetailsPosition;
        private static Size WeightedMultipleDetailsSize = new Size(ResponsePanelSize.Width, 275);
        private static Point RegExDetailsPosition = BoundedLengthDetailsPosition;
        private static Size RegExDetailsSize = new Size(ResponsePanelSize.Width, 95);

        // the child controls displayed by ResponsePanel
        protected ResponseTypeRadios m_ResponseTypeRadios;
        protected TrueFalseDetails m_TrueFalseDetails;
        protected DateDetails m_DateDetails;
        protected LikertDetails m_LikertDetails;
        protected MultiChoiceDetails m_MultiChoiceDetails;
        protected MultiSelectionDetails m_MultiSelectionDetails;
        protected ExistsInFileDetails m_ExistsInFileDetails;
        protected TextResponseTypeRadios m_TextRadios;
        protected BoundedLengthDetails m_BoundedLengthDetails;
        protected BoundedNumberDetails m_BoundedNumberDetails;
        protected FixedDigDetails m_FixedDigitDetails;
        protected FixedLengthDetails m_FixedLengthDetails;
        protected MaxLengthDetails m_MaxLengthDetails;
        protected RegExDetails m_RegExDetails;
        protected WeightedMultipleDetails m_WeightedMultipleDetails;

        // set to "true" if Response is being set, otherwise "false"
        private bool _UpdatingPanel;

        /// <summary>
        /// gets a value that indicates if the ResponsePanel object is updating its child controls
        /// internally, rather than as a result of user input
        /// </summary>
        public bool UpdatingPanel
        {
            get
            {
                return _UpdatingPanel;
            }
        }

        /// <summary>
        /// gets or sets the Response displayed in the ResponsePanel object
        /// </summary>
        public CResponse Response
        {
            get
            {
                return GetResponse();
            }
            set
            {
                // begin update
                _UpdatingPanel = true;

                // set the appropriate radio button in ResponseTypeRadio and set the response of the appropriate child control
                switch (value.ResponseType)
                {
                    case CResponse.EResponseType.Boolean:
                        if (m_ResponseTypeRadios == null)
                            CreateResponseTypeRadios();
                        if (m_TrueFalseDetails == null)
                            CreateTrueFalseDetails();
                        m_ResponseTypeRadios.ResponseType = ResponseTypeRadios.EResponseType.Bool;
                        m_TrueFalseDetails.Response = (CBoolResponse)value;
                        break;

                    case CResponse.EResponseType.BoundedLength:
                        if (m_ResponseTypeRadios == null)
                            CreateResponseTypeRadios();
                        if (m_TextRadios == null)
                            CreateTextRadios();
                        if (m_BoundedLengthDetails == null)
                            CreateBoundedLengthDetails();
                        m_ResponseTypeRadios.ResponseType = ResponseTypeRadios.EResponseType.Text;
                        m_TextRadios.ActiveSubPanel = TextResponseTypeRadios.ETextResponseType.BoundedLength;
                        m_BoundedLengthDetails.Response = (CBoundedLengthResponse)value;
                        break;

                    case CResponse.EResponseType.BoundedNum:
                        if (m_ResponseTypeRadios == null)
                            CreateResponseTypeRadios();
                        if (m_TextRadios == null)
                            CreateTextRadios();
                        if (m_BoundedNumberDetails == null)
                            CreateBoundedNumberDetails();
                        m_ResponseTypeRadios.ResponseType = ResponseTypeRadios.EResponseType.Text;
                        m_TextRadios.ActiveSubPanel = TextResponseTypeRadios.ETextResponseType.BoundedNumber;
                        m_BoundedNumberDetails.Response = (CBoundedNumResponse)value;
                        break;

                    case CResponse.EResponseType.Date:
                        if (m_ResponseTypeRadios == null)
                            CreateResponseTypeRadios();
                        if (m_DateDetails == null)
                            CreateDateDetails();
                        m_ResponseTypeRadios.ResponseType = ResponseTypeRadios.EResponseType.Date;
                        m_DateDetails.Response = (CDateResponse)value;
                        break;

                    case CResponse.EResponseType.FixedDig:
                        if (m_ResponseTypeRadios == null)
                            CreateResponseTypeRadios();
                        if (m_TextRadios == null)
                            CreateTextRadios();
                        if (m_FixedDigitDetails == null)
                            CreateFixedDigitDetails();
                        m_ResponseTypeRadios.ResponseType = ResponseTypeRadios.EResponseType.Text;
                        m_TextRadios.ActiveSubPanel = TextResponseTypeRadios.ETextResponseType.FixedDigit;
                        m_FixedDigitDetails.Response = (CFixedDigResponse)value;
                        break;

                    case CResponse.EResponseType.FixedLength:
                        if (m_ResponseTypeRadios == null)
                            CreateResponseTypeRadios();
                        if (m_TextRadios == null)
                            CreateTextRadios();
                        if (m_FixedLengthDetails == null)
                            CreateFixedLengthDetails();
                        m_ResponseTypeRadios.ResponseType = ResponseTypeRadios.EResponseType.Text;
                        m_TextRadios.ActiveSubPanel = TextResponseTypeRadios.ETextResponseType.FixedLength;
                        m_FixedLengthDetails.Response = (CFixedLengthResponse)value;
                        break;

                    case CResponse.EResponseType.Instruction:
                        if (m_ResponseTypeRadios == null)
                            CreateResponseTypeRadios();
                        m_ResponseTypeRadios.ResponseType = ResponseTypeRadios.EResponseType.Instruction;
                        break;

                    case CResponse.EResponseType.Likert:
                        if (m_ResponseTypeRadios == null)
                            CreateResponseTypeRadios();
                        if (m_LikertDetails == null)
                            CreateLikertDetails();
                        m_ResponseTypeRadios.ResponseType = ResponseTypeRadios.EResponseType.Likert;
                        m_LikertDetails.Response = (CLikertResponse)value;
                        break;

                    case CResponse.EResponseType.MaxLength:
                        if (m_ResponseTypeRadios == null)
                            CreateResponseTypeRadios();
                        if (m_TextRadios == null)
                            CreateTextRadios();
                        if (m_MaxLengthDetails == null)
                            CreateMaxLengthDetails();
                        m_ResponseTypeRadios.ResponseType = ResponseTypeRadios.EResponseType.Text;
                        m_TextRadios.ActiveSubPanel = TextResponseTypeRadios.ETextResponseType.MaxLength;
                        m_MaxLengthDetails.Response = (CMaxLengthResponse)value;
                        break;

                    case CResponse.EResponseType.RegEx:
                        if (m_ResponseTypeRadios == null)
                            CreateResponseTypeRadios();
                        if (m_TextRadios == null)
                            CreateTextRadios();
                        if (m_RegExDetails == null)
                            CreateRegExDetails();
                        m_ResponseTypeRadios.ResponseType = ResponseTypeRadios.EResponseType.Text;
                        m_TextRadios.ActiveSubPanel = TextResponseTypeRadios.ETextResponseType.RegEx;
                        m_RegExDetails.Response = (CRegExResponse)value;
                        break;

                    case CResponse.EResponseType.MultiBoolean:
                        if (m_ResponseTypeRadios == null)
                            CreateResponseTypeRadios();
                        if (m_MultiSelectionDetails == null)
                            CreateMultiSelectionDetails();
                        m_ResponseTypeRadios.ResponseType = ResponseTypeRadios.EResponseType.MultiSelect;
                        m_MultiSelectionDetails.Response = (CMultiBooleanResponse)value;
                        break;

                    case CResponse.EResponseType.Multiple:
                        if (m_ResponseTypeRadios == null)
                            CreateResponseTypeRadios();
                        if (m_MultiChoiceDetails == null)
                            CreateMultiChoiceDetails();
                        m_ResponseTypeRadios.ResponseType = ResponseTypeRadios.EResponseType.MultiChoice;
                        m_MultiChoiceDetails.Response = (CMultipleResponse)value;
                        break;

                    case CResponse.EResponseType.ExistsInFile:
                        if (m_ResponseTypeRadios == null)
                            CreateResponseTypeRadios();
                        if (m_ExistsInFileDetails == null)
                            CreateExistsInFileDetails();
                        m_ResponseTypeRadios.ResponseType = ResponseTypeRadios.EResponseType.ExistsInFile;
                        m_ExistsInFileDetails.Response = (CExistsInFileResponse)value;
                        break;

                    case CResponse.EResponseType.WeightedMultiple:
                        if (m_ResponseTypeRadios == null)
                            CreateResponseTypeRadios();
                        if (m_WeightedMultipleDetails == null)
                            CreateWeightedMultipleDetails();
                        m_ResponseTypeRadios.ResponseType = ResponseTypeRadios.EResponseType.WeightedMultiChoice;
                        m_WeightedMultipleDetails.Response = (CWeightedMultipleResponse)value;
                        break;
                }

                // end update
                _UpdatingPanel = false;
            }
        }

        /// <summary>
        /// The default constructor
        /// </summary>
        public ResponsePanel()
        {
            InitializeComponent();
            _UpdatingPanel = false;
            m_ResponseTypeRadios = null;
            m_TrueFalseDetails = null;
            m_DateDetails = null;
            m_LikertDetails = null;
            m_MultiChoiceDetails = null;
            m_MultiSelectionDetails = null;
            m_ExistsInFileDetails = null;
            m_TextRadios = null;
            m_BoundedLengthDetails = null;
            m_BoundedNumberDetails = null;
            m_FixedDigitDetails = null;
            m_FixedLengthDetails = null;
            m_MaxLengthDetails = null;
            m_RegExDetails = null;
            m_WeightedMultipleDetails = null;
        }

        /// <summary>
        /// Instantiates a ResponseTypeRadios object for use as a child control
        /// </summary>
        private void CreateResponseTypeRadios()
        {
            m_ResponseTypeRadios = new ResponseTypeRadios();
            m_ResponseTypeRadios.Location = ResponseTypeRadiosPosition;
            m_ResponseTypeRadios.Size = ResponseTypeRadiosSize;
        }

        /// <summary>
        /// Instantiates a TrueFalseDetails object for use as a child control
        /// </summary>
        private void CreateTrueFalseDetails()
        {
            m_TrueFalseDetails = new TrueFalseDetails();
            m_TrueFalseDetails.Location = TrueFalseDetailsPosition;
            m_TrueFalseDetails.Size = TrueFalseDetailsSize;
        }

        /// <summary>
        /// Instantiates a DateDetails object for use as a child control
        /// </summary>
        private void CreateDateDetails()
        {
            m_DateDetails = new DateDetails();
            m_DateDetails.Location = DateDetailsPosition;
            m_DateDetails.Size = DateDetailsSize;
        }

        /// <summary>
        /// Instantiates a LikertDetails object for use as a child control
        /// </summary>
        private void CreateLikertDetails()
        {
            m_LikertDetails = new LikertDetails();
            m_LikertDetails.Location = LikertDetailsPosition;
            m_LikertDetails.Size = LikertDetailsSize;
        }

        /// <summary>
        /// Instantiates a MultiChoiceDetails object for use as a child control
        /// </summary>
        private void CreateMultiChoiceDetails()
        {
            m_MultiChoiceDetails = new MultiChoiceDetails();
            m_MultiChoiceDetails.Location = MultiChoiceDetailsPosition;
            m_MultiChoiceDetails.Size = MultiChoiceDetailsSize;
        }

        /// <summary>
        /// Instantiates a MultiSelectionDetails object for use as a child control
        /// </summary>
        private void CreateMultiSelectionDetails()
        {
            m_MultiSelectionDetails = new MultiSelectionDetails();
            m_MultiSelectionDetails.Location = MultiSelectionDetailsPosition;
            m_MultiSelectionDetails.Size = MultiSelectionDetailsSize;
        }

        /// <summary>
        /// Instantiates a RegExDetails object for use as a child control
        /// </summary>
        private void CreateExistsInFileDetails()
        {
            m_ExistsInFileDetails = new ExistsInFileDetails();
            m_ExistsInFileDetails.Location = ExistsInFileDetailsPosition;
            m_ExistsInFileDetails.Size = ExistsInFileDetailsSize;
        }

        /// <summary>
        /// Instantiates a TextTypeRadios object for use as a child control
        /// </summary>
        private void CreateTextRadios()
        {
            m_TextRadios = new TextResponseTypeRadios();
            m_TextRadios.Location = TextRadiosPosition;
            m_TextRadios.Size = TextRadiosSize;
        }

        /// <summary>
        /// Instantiates a BoundedLengthDetails object for use as a child control
        /// </summary>
        private void CreateBoundedLengthDetails()
        {
            m_BoundedLengthDetails = new BoundedLengthDetails();
            m_BoundedLengthDetails.Location = BoundedLengthDetailsPosition;
            m_BoundedLengthDetails.Size = BoundedLengthDetailsSize;
        }

        /// <summary>
        /// Instantiates a BoundedNumberDetails object for use as a child control
        /// </summary>
        private void CreateBoundedNumberDetails()
        {
            m_BoundedNumberDetails = new BoundedNumberDetails();
            m_BoundedNumberDetails.Location = BoundedNumberDetailsPosition;
            m_BoundedNumberDetails.Size = BoundedNumberDetailsSize;
        }

        /// <summary>
        /// Instantiates a FixedDigDetails object for use as a child control
        /// </summary>
        private void CreateFixedDigitDetails()
        {
            m_FixedDigitDetails = new FixedDigDetails();
            m_FixedDigitDetails.Location = FixedDigitDetailsPosition;
            m_FixedDigitDetails.Size = FixedDigitDetailsSize;
        }

        /// <summary>
        /// Instantiates a FixedLengthDetails obejct for use as a child control
        /// </summary>
        private void CreateFixedLengthDetails()
        {
            m_FixedLengthDetails = new FixedLengthDetails();
            m_FixedLengthDetails.Location = FixedLengthDetailsPosition;
            m_FixedLengthDetails.Size = FixedLengthDetailsSize;
        }

        /// <summary>
        /// Instantiates a RegExDetails object for use as a child control
        /// </summary>
        private void CreateRegExDetails()
        {
            m_RegExDetails = new RegExDetails();
            m_RegExDetails.Location = RegExDetailsPosition;
            m_RegExDetails.Size = RegExDetailsSize;
        }

        /// <summary>
        /// Instantiates a MaxLengthDetails object for use as a child control
        /// </summary>
        private void CreateMaxLengthDetails()
        {
            m_MaxLengthDetails = new MaxLengthDetails();
            m_MaxLengthDetails.Location = MaxLengthDetailsPosition;
            m_MaxLengthDetails.Size = MaxLengthDetailsSize;
        }

        /// <summary>
        /// Instantiates a WeightedMultipleDetails for use as a child control
        /// </summary>
        private void CreateWeightedMultipleDetails()
        {
            m_WeightedMultipleDetails = new WeightedMultipleDetails();
            m_WeightedMultipleDetails.Location = WeightedMultipleDetailsPosition;
            m_WeightedMultipleDetails.Size = WeightedMultipleDetailsSize;
        }
        
        /// <summary>
        /// Displays the TrueFalseDetails child control, instantiating a new object if necessary
        /// </summary>
        public void ShowTrueFalseDetails()
        {
            if (m_TrueFalseDetails == null)
                CreateTrueFalseDetails();
            Controls.Add(m_TrueFalseDetails);
        }

        /// <summary>
        /// Hides the TrueFalseDetails child control
        /// </summary>
        public void HideTrueFalseDetails()
        {
            Controls.Remove(m_TrueFalseDetails);
        }

        /// <summary>
        /// Displays the DateDetails child control, instantiating a new object if necessary
        /// </summary>
        public void ShowDateDetails()
        {
            if (m_DateDetails == null)
                CreateDateDetails();
            Controls.Add(m_DateDetails);
        }

        /// <summary>
        /// Hides the DateDetails child control
        /// </summary>
        public void HideDateDetails()
        {
            Controls.Remove(m_DateDetails);
        }

        /// <summary>
        /// Displays the LikertDetails child control, instantiating a new object if necessary
        /// </summary>
        public void ShowLikertDetails()
        {
            if (m_LikertDetails == null)
                CreateLikertDetails();
            Controls.Add(m_LikertDetails);
        }

        /// <summary>
        /// Hides the LikertDetails child control
        /// </summary>
        public void HideLikertDetails()
        {
            Controls.Remove(m_LikertDetails);
        }

        /// <summary>
        /// Displays the MultiChoiceDetails child control, instantiating a new object if necessary
        /// </summary>
        public void ShowMultiChoiceDetails()
        {
            if (m_MultiChoiceDetails == null)
                CreateMultiChoiceDetails();
            Controls.Add(m_MultiChoiceDetails);
        }

        /// <summary>
        /// Hides the MultiChoiceDetails child control
        /// </summary>
        public void HideMultiChoiceDetails()
        {
            Controls.Remove(m_MultiChoiceDetails);
        }

        /// <summary>
        /// Displays the MultiSelectionDetails child control, instantiating a new object if necessary
        /// </summary>
        public void ShowMultiSelectionDetails()
        {
            if (m_MultiSelectionDetails == null)
                CreateMultiSelectionDetails();
            Controls.Add(m_MultiSelectionDetails);
        }

        /// <summary>
        /// Hides the MultiSelectionDetails child control
        /// </summary>
        public void HideMultiSelectionDetails()
        {
            Controls.Remove(m_MultiSelectionDetails);
        }

        /// <summary>
        /// Displays the ExistsInFileDetails child control, instantiating a new object if necessary
        /// </summary>
        public void ShowExistsInFileDetails()
        {
            if (m_ExistsInFileDetails == null)
                CreateExistsInFileDetails();
            Controls.Add(m_ExistsInFileDetails);
        }

        /// <summary>
        /// Hides the ExistsInFileDetails child control
        /// </summary>
        public void HideExistsInFileDetails()
        {
            Controls.Remove(m_ExistsInFileDetails);
        }

        /// <summary>
        /// Displays the TextTypeRadios child control and the active text based response child control,
        /// instantiating a new object if necessary
        /// </summary>
        public void ShowTextRadios()
        {
            if (m_TextRadios == null)
                CreateTextRadios();
            Controls.Add(m_TextRadios);
            m_TextRadios.ShowSubPanel();
        }

        /// <summary>
        /// Hides the TextTypeRadios child control and the active text based respone child control
        /// </summary>
        public void HideTextRadios()
        {
            m_TextRadios.HideSubPanel();
            Controls.Remove(m_TextRadios);
        }

        /// <summary>
        /// Displays the BoundedLengthDetails child control, instantiating a new object if necessary
        /// </summary>
        public void ShowBoundedLengthDetails()
        {
            if (m_BoundedLengthDetails == null)
                CreateBoundedLengthDetails();
            Controls.Add(m_BoundedLengthDetails);
        }

        /// <summary>
        /// Hides the BoundedLengthDetails child control
        /// </summary>
        public void HideBoundedLengthDetails()
        {
            Controls.Remove(m_BoundedLengthDetails);
        }

        /// <summary>
        /// Displays the BoundedNumberDetails child control, instantiating a new object if necessary
        /// </summary>
        public void ShowBoundedNumberDetails()
        {
            if (m_BoundedNumberDetails == null)
                CreateBoundedNumberDetails();
            Controls.Add(m_BoundedNumberDetails);
        }

        /// <summary>
        /// Hides the BoundedNumberDetails child control
        /// </summary>
        public void HideBoundedNumberDetails()
        {
            Controls.Remove(m_BoundedNumberDetails);
        }

        /// <summary>
        /// Displays the FixedDigDetails child control, instantiating a new object if necessary
        /// </summary>
        public void ShowFixedDigitDetails()
        {
            if (m_FixedDigitDetails == null)
                CreateFixedDigitDetails();
            Controls.Add(m_FixedDigitDetails);
        }

        /// <summary>
        /// Hides the FixedDigDetails child control
        /// </summary>
        public void HideFixedDigitDetails()
        {
            Controls.Remove(m_FixedDigitDetails);
        }

        /// <summary>
        /// Displays the FixedLengthDetails child control, instantiating a new object if necessary
        /// </summary>
        public void ShowFixedLengthDetails()
        {
            if (m_FixedLengthDetails == null)
                CreateFixedLengthDetails();
            Controls.Add(m_FixedLengthDetails);
        }

        /// <summary>
        /// Hides the FixedLengthDetails child control
        /// </summary>
        public void HideFixedLengthDetails()
        {
            Controls.Remove(m_FixedLengthDetails);
        }

        /// <summary>
        /// Displays the RegExDetails child control, instantiating a new object if necessary
        /// </summary>
        public void ShowRegExDetails()
        {
            if (m_RegExDetails == null)
                CreateRegExDetails();
            Controls.Add(m_RegExDetails);
        }

        /// <summary>
        /// Hides the RegExDetails child control
        /// </summary>
        public void HideRegExDetails()
        {
            Controls.Remove(m_RegExDetails);
        }


        /// <summary>
        /// Displays the MaxLengthDetails child control, instantiating a new object if necessary
        /// </summary>
        public void ShowMaxLengthDetails()
        {
            if (m_MaxLengthDetails == null)
                CreateMaxLengthDetails();
            Controls.Add(m_MaxLengthDetails);
        }

        /// <summary>
        /// Hides the MaxLengthDetails child control
        /// </summary>
        public void HideMaxLengthDetails()
        {
            Controls.Remove(m_MaxLengthDetails);
        }

        /// <summary>
        /// Displays the WieghtedMultipleDetails child control, instantiating a new object if necessary
        /// </summary>
        public void ShowWeightedMultipleDetails()
        {
            if (m_WeightedMultipleDetails == null)
                CreateWeightedMultipleDetails();
            Controls.Add(m_WeightedMultipleDetails);
        }

        /// <summary>
        /// Hides the WeightedMultipleDetails child control
        /// </summary>
        public void HideWeightedMultipleDetails()
        {
            Controls.Remove(m_WeightedMultipleDetails);
        }

        
        private void ResponsePanel_Load(object sender, EventArgs e)
        {
            // create and display the ResponseTypeRadios control
            if (m_ResponseTypeRadios == null)
                CreateResponseTypeRadios();
            Controls.Add(m_ResponseTypeRadios);
        }       

        /// <summary>
        /// Constructs a CResponse object that corresponds to the response type defined in the ResponsePanel
        /// </summary>
        /// <returns>A new CResponse object, "null" on failure</returns>
        private CResponse GetResponse()
        {
            CResponse Response;
            if (m_ResponseTypeRadios == null)
                return null;

            switch (m_ResponseTypeRadios.ResponseType)
            {
                case ResponseTypeRadios.EResponseType.Bool:
                    Response = m_TrueFalseDetails.Response;
                    break;
            
                case ResponseTypeRadios.EResponseType.Date:
                    Response = m_DateDetails.Response;
                    break;

                case ResponseTypeRadios.EResponseType.Instruction:
                    Response = new CInstruction();
                    break;

                case ResponseTypeRadios.EResponseType.Likert:
                    Response = m_LikertDetails.Response;
                    break;

                case ResponseTypeRadios.EResponseType.MultiChoice:
                    Response = m_MultiChoiceDetails.Response;
                    break;

                case ResponseTypeRadios.EResponseType.MultiSelect:
                    Response = m_MultiSelectionDetails.Response;
                    break;

                case ResponseTypeRadios.EResponseType.ExistsInFile:
                    Response = m_ExistsInFileDetails.Response;
                    break;

                case ResponseTypeRadios.EResponseType.Text:
                    switch (m_TextRadios.ActiveSubPanel)
                    {
                        case TextResponseTypeRadios.ETextResponseType.BoundedLength:
                            Response = m_BoundedLengthDetails.Response;
                            break;

                        case TextResponseTypeRadios.ETextResponseType.BoundedNumber:
                            Response = m_BoundedNumberDetails.Response;
                            break;

                        case TextResponseTypeRadios.ETextResponseType.FixedDigit:
                            Response = m_FixedDigitDetails.Response;
                            break;

                        case TextResponseTypeRadios.ETextResponseType.FixedLength:
                            Response = m_FixedLengthDetails.Response;
                            break;

                        case TextResponseTypeRadios.ETextResponseType.RegEx:
                            Response = m_RegExDetails.Response;
                            break;

                        case TextResponseTypeRadios.ETextResponseType.MaxLength:
                            Response = m_MaxLengthDetails.Response;
                            break;

                        default:
                            Response = null;
                            break;
                    }
                    break;

                case ResponseTypeRadios.EResponseType.WeightedMultiChoice:
                    Response = m_WeightedMultipleDetails.Response;
                    break;

                default:
                    Response = null;
                    break;
            }
            return Response;
        }

        /// <summary>
        /// Validates the ResponsePanel object and its child controls, setting error flags in the child controls
        /// and the error message in the main window as appropriate
        /// </summary>
        /// <returns>"true" if the response is valid, otherwise "false"</returns>
        public bool ValidateResponse()
        {
            bool IsValid = false;

            switch (m_ResponseTypeRadios.ResponseType)
            {
                case ResponseTypeRadios.EResponseType.Bool:
                    m_TrueFalseDetails.ValidateInput();
                    IsValid = !m_TrueFalseDetails.HasErrors;
                    break;

                case ResponseTypeRadios.EResponseType.Date:
                    m_DateDetails.ValidateInput();
                    IsValid = !m_DateDetails.HasErrors;
                    break;

                case ResponseTypeRadios.EResponseType.Instruction:
                    IsValid = true;
                    break;

                case ResponseTypeRadios.EResponseType.Likert:
                    m_LikertDetails.ValidateInput();
                    IsValid = !m_LikertDetails.HasErrors;
                    break;

                case ResponseTypeRadios.EResponseType.MultiChoice:
                    m_MultiChoiceDetails.ValidateInput();
                    IsValid = !m_MultiChoiceDetails.HasErrors;
                    break;

                case ResponseTypeRadios.EResponseType.MultiSelect:
                    m_MultiSelectionDetails.ValidateInput();
                    IsValid = !m_MultiSelectionDetails.HasErrors;
                    break;

                case ResponseTypeRadios.EResponseType.ExistsInFile:
                    m_ExistsInFileDetails.ValidateInput();
                    IsValid = !m_ExistsInFileDetails.HasErrors;
                    break;

                case ResponseTypeRadios.EResponseType.Text:
                    switch (m_TextRadios.ActiveSubPanel)
                    {
                        case TextResponseTypeRadios.ETextResponseType.BoundedLength:
                            m_BoundedLengthDetails.ValidateInput();
                            IsValid = !m_BoundedLengthDetails.HasErrors;
                            break;

                        case TextResponseTypeRadios.ETextResponseType.BoundedNumber:
                            m_BoundedNumberDetails.ValidateInput();
                            IsValid = !m_BoundedNumberDetails.HasErrors;
                            break;

                        case TextResponseTypeRadios.ETextResponseType.FixedDigit:
                            m_FixedDigitDetails.ValidateInput();
                            IsValid = m_FixedDigitDetails.HasErrors;
                            break;

                        case TextResponseTypeRadios.ETextResponseType.FixedLength:
                            m_FixedLengthDetails.ValidateInput();
                            IsValid = m_FixedLengthDetails.HasErrors;
                            break;

                        case TextResponseTypeRadios.ETextResponseType.MaxLength:
                            m_MaxLengthDetails.ValidateInput();
                            IsValid = m_MaxLengthDetails.HasErrors;
                            break;
                    }
                    break;

                case ResponseTypeRadios.EResponseType.WeightedMultiChoice:
                    m_WeightedMultipleDetails.ValidateInput();
                    IsValid = m_WeightedMultipleDetails.HasErrors;
                    break;
            }
            return IsValid;
        }
    }
}

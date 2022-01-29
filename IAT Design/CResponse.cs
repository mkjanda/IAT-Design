using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using IATClient.ResultData;

namespace IATClient
{
    /// <summary>
    /// An abstract class the serves as a superclass for response type classes
    /// </summary>
    public abstract class CResponse : ICloneable
    {
        protected const int PreviewLeftIndent = 35;
        protected const int ChoiceVertPadding = 7;
        protected const int radioRightPadding = 10;
        protected const int CheckPaddingRight = 10;


        // constant strings that represent each response type
        protected const string sTypeBool = "Boolean";
        protected const string sTypeBoundedLength = "Bounded Length";
        protected const string sTypeBoundedNum = "Bounded Number";
        protected const string sTypeDate = "Date";
        protected const string sTypeExistsInFile = "Exists in File";
        protected const string sTypeFixedDig = "Fixed Digit";
        protected const string sTypeFixedLength = "Fixed Length";
        protected const string sTypeInstruction = "Instruction";
        protected const string sTypeLikert = "Likert";
        protected const string sTypeMaxLength = "Maximum Length";
        protected const string sTypeMultiBoolean = "Multiple Selection";
        protected const string sTypeMultiple = "Multiple Choice";
        protected const string sTypeRegEx = "Regular Expression";
        protected const string sTypeWeightedMultiple = "Weighted Multiple Choice";
        
        /// <summary>
        /// An enumeration of the available response types
        /// </summary>
        public enum EResponseType : int
        {
            Instruction, StaticImage, Boolean, Likert, Date, Multiple, WeightedMultiple, RegEx, MultiBoolean,
            FixedDig, BoundedNum, BoundedLength
        };

        // the type of the response
        private EResponseType _ResponseType;

        public SurveyItemFormat Format { get; set; }

        /// <summary>
        /// gets the response type
        /// </summary>
        public EResponseType ResponseType
        {
            get 
            {
                return _ResponseType;
            }
        }

        // set to "true" if the response type contains errors
        private bool _HasErrors;

        /// <summary>
        /// gets or sets whether the CResponse object contains errors
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
            }
        }

        /// <summary>
        /// Performs base class initialization of the CResponse-derived object
        /// </summary>
        /// <param name="responseType">the type of the response</param>
        public CResponse(EResponseType responseType)
        {
            _ResponseType = responseType;
            Format = new SurveyItemFormat(SurveyItemFormat.EFor.Response);
            _HasErrors = false;
        }

        public CResponse(EResponseType type, CResponse o)
        {
            _ResponseType = type;
            _HasErrors = o.HasErrors;
            Format = o.Format.Clone() as SurveyItemFormat;
        }


        public abstract XElement AsXElement();
        public abstract void Load(XElement elem);

        public abstract void WriteXml(XmlWriter writer);
       
        /// <summary>
        /// Test's the response's data members to ensure they are valid
        /// </summary>
        /// <returns>"true" if the data members are valid, "false" otherwise</returns>
        public abstract bool IsValid();

        /// <summary>
        /// Loads the response's data members from an XmlNode
        /// </summary>
        /// <param name="node">The XmlNode to load the response from</param>
        /// <returns>"true" on succeess, "false" otherwise</returns>

        public abstract String GetResponseDesc();

        public abstract Response GenerateSerializableResponse(SurveyItem parentItem);
        public abstract object Clone();
        /// <summary>
        /// Creates a blank response of the specified type
        /// </summary>
        /// <param name="rType">The type of the response to be created</param>
        /// <returns>A CResponse-derived object of the specified response type</returns>
        static public CResponse Create(EResponseType rType)
        {
            switch (rType)
            {
                case EResponseType.Boolean: return new CBoolResponse();
                case EResponseType.BoundedLength: return new CBoundedLengthResponse();
                case EResponseType.BoundedNum: return new CBoundedNumResponse();
                case EResponseType.Date: return new CDateResponse();
//                case EResponseType.ExistsInFile: return new CExistsInFileResponse();
                case EResponseType.FixedDig: return new CFixedDigResponse();
   //             case EResponseType.FixedLength: return new CFixedLengthResponse();
                case EResponseType.Instruction: return new CInstruction();
                case EResponseType.Likert: return new CLikertResponse();
   //             case EResponseType.MaxLength: return new CMaxLengthResponse();
                case EResponseType.MultiBoolean: return new CMultiBooleanResponse();
                case EResponseType.Multiple: return new CMultipleResponse();
                case EResponseType.RegEx: return new CRegExResponse();
                case EResponseType.WeightedMultiple: return new CWeightedMultipleResponse();
            }
            return null;
        }
/*        
        /// <summary>
        /// Creates a new response from the data contained in an XmlNode
        /// </summary>
        /// <param name="node">The XmlNode to the contains the data for the response</param>
        /// <returns>A CResponse-derives object initialized with data in the supplied XmlNode.  Returns "null" on failure</returns>
        static public CResponse CreateFromXml(XmlNode node)
        {
            // ensure the node defines a response
            if (node.Name != "Response")
                return null;
            CResponse r = null;

            // instantiate the appropriate CResponse-derived object based on the "Type" attribute of the root element of node
            switch (node.Attributes["Type"].InnerText)
            {
                case sTypeBool:
                    r = new CBoolResponse();
                    break;

                case sTypeBoundedLength:
                    r = new CBoundedLengthResponse();
                    break;

                case sTypeBoundedNum:
                    r = new CBoundedNumResponse();
                    break;

                case sTypeDate:
                    r = new CDateResponse();
                    break;

                case sTypeFixedDig:
                    r = new CFixedDigResponse();
                    break;

                case sTypeInstruction:
                    r = new CInstruction();
                    break;

                case sTypeLikert:
                    r = new CLikertResponse();
                    break;

                case sTypeMultiBoolean:
                    r = new CMultiBooleanResponse();
                    break;

                case sTypeMultiple:
                    r = new CMultipleResponse();
                    break;

                case sTypeRegEx:
                    r = new CRegExResponse();
                    break;

                case sTypeWeightedMultiple:
                    r = new CWeightedMultipleResponse();
                    break;

                default:
                    return null;
            }

            // attempt to load the response, returning null on failure
            if (r.LoadFromXml(node) == false)
                return null;
            if (CVersion.Compare(CIAT.SaveFile.SaveFileVersion, new CVersion("1.0.1.1")) < 0)
                if (r.ResponseType != EResponseType.Instruction)
                    r.Format.LoadFromXml(node.SelectSingleNode("./SurveyItemFormat"));

            // return the response
            return r;
        }
        */
        /*
        private CResponseObject _DefinedResponse;

        public CResponseObject DefinedResponse
        {
            get
            {
                return _DefinedResponse;
            }
            set
            {
                _DefinedResponse = value;
            }
        }
        */
        public abstract CSpecifierControlDefinition GetSpecifierControlDefinition();
    }
}

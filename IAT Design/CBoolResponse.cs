using IATClient.IATConfig;
using IATClient.ResultData;
using System;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Linq;

namespace IATClient
{
    /// <summary>
    /// CBoolResponse provides for a Boolean or True/False response type.  
    /// </summary>
    class CBoolResponse : CResponse
    {
        // the strings that contain the statements that correspond to true and false
        private String _TrueStatement, _FalseStatement;

        /// <summary>
        /// gets or sets the statement that corresponds to "true"
        /// </summary>
        public String TrueStatement
        {
            get
            {
                return _TrueStatement;
            }
            set
            {
                _TrueStatement = value;
            }
        }

        /// <summary>
        /// gets or sets the statement that corresponds to "false"
        /// </summary>
        public String FalseStatement
        {
            get
            {
                return _FalseStatement;
            }
            set
            {
                _FalseStatement = value;
            }
        }

        public String GetTrueStatement()
        {
            return TrueStatement;
        }

        public String GetFalseStatement()
        {
            return FalseStatement;
        }

        /// <summary>
        /// the value for TrueStatement and FalseStatement that indicates either contains an invalid value
        /// </summary>
        public static String InvalidValue = String.Empty;

        private TextBox TrueStatementBox = null, FalseStatementBox = null;
        private RadioButton TrueRadio = null, FalseRadio = null;
        private CheckBox TrueCheck = null, FalseCheck = null;


        /// <summary>
        /// The default constructor. 
        /// Sets the true and false statements to InvalidValue and calls the base class constructor
        /// with the appropriate enumerated response type
        /// </summary>
        public CBoolResponse()
            : base(CResponse.EResponseType.Boolean)
        {
            _TrueStatement = Properties.Resources.sTrueStatementDefault;
            _FalseStatement = Properties.Resources.sFalseStatementDefault;
        }

        /// <summary>
        /// This constructor sets the true and false statements to the given text values and calls the base
        /// class constructor with the appropriate enumerated response type 
        /// </summary>
        /// <param name="trueStatement">The statement that corresponds to "true"</param>
        /// <param name="falseStatement">The statement that corresponds to "false"</param>
        public CBoolResponse(String trueStatement, String falseStatement)
            : base(CResponse.EResponseType.Boolean)
        {
            _TrueStatement = trueStatement;
            _FalseStatement = falseStatement;
        }

        /// <summary>
        /// The copy constructor
        /// </summary>
        /// <param name="r">The CBoolResponse object to copy</param>
        public CBoolResponse(CBoolResponse r)
            : base(CResponse.EResponseType.Boolean, r)
        {
            _TrueStatement = r._TrueStatement;
            _FalseStatement = r._FalseStatement;
        }

        public override object Clone()
        {
            return new CBoolResponse(this);
        }

        /// <summary>
        /// Validates the object's data
        /// </summary>
        /// <returns>"true" if both the true and false statements have been assigned text values, otherwise "false"</returns>
        public override bool IsValid()
        {
            if ((TrueStatement == InvalidValue) || (FalseStatement == InvalidValue))
                return false;
            return true;
        }

        public override XElement AsXElement() => new XElement("Response", new XElement("TrueStatement", TrueStatement), 
            new XElement("FalseStatement", FalseStatement), Format.AsXElement());

        public override void Load(XElement elem)
        {
            TrueStatement = elem.Element("TrueStatement").Value;
            FalseStatement = elem.Element("FalseStatement").Value;
            Format.Load(elem.Element("Format"));

        }

        public override void WriteXml(XmlWriter writer)
        {
            writer.WriteStartElement("Boolean");
            Format.WriteXml(writer);
            writer.WriteElementString("TrueStatement", TrueStatement);
            writer.WriteElementString("FalseStatement", FalseStatement);
            writer.WriteEndElement();
        }



        public override string GetResponseDesc()
        {
            return String.Format("\t1 - {0}\r\n\t0 - {1}\r\n", TrueStatement, FalseStatement);
        }

        public override Response GenerateSerializableResponse(SurveyItem parentItem)
        {
            ResultData.Boolean r = new ResultData.Boolean(parentItem);
            r.TrueStatement = TrueStatement;
            r.FalseStatement = FalseStatement;
            return r;
        }

        public override CSpecifierControlDefinition GetSpecifierControlDefinition()
        {
            CSpecifierControlDefinition def = new CSpecifierControlDefinition(DynamicSpecifier.ESpecifierType.Selection);
            def.Statements.Add(TrueStatement);
            def.Statements.Add(FalseStatement);
            def.Values.Add("1");
            def.Values.Add("0");
            return def;
        }

        public void DisposeOfControls()
        {
            if (TrueStatementBox != null)
            {
                TrueStatementBox.Dispose();
                TrueStatementBox = null;
            }
            if (FalseStatementBox != null)
            {
                FalseStatementBox.Dispose();
                FalseStatementBox = null;
            }
            if (TrueCheck != null)
            {
                TrueCheck.Dispose();
                TrueCheck = null;
            }
            if (FalseCheck != null)
            {
                FalseCheck.Dispose();
                FalseCheck = null;
            }
            if (TrueRadio != null)
            {
                TrueRadio.Dispose();
                TrueRadio = null;
            }
            if (FalseRadio != null)
            {
                FalseRadio.Dispose();
                FalseRadio = null;
            }
        }

    }
}

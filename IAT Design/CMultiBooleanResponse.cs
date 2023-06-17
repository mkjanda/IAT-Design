using IATClient.IATConfig;
using IATClient.ResultData;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Xml;
using System.Xml.Linq;

namespace IATClient
{
    /// <summary>
    /// CMultiBooleanResponse provides a class that represents a definition for a response type that consists
    /// of multiple statements, zero or more of which can be selected
    /// </summary>
    class CMultiBooleanResponse : CResponse
    {
        // the number of statements and the statement list
        private int _nValues;
        private List<String> _LabelList;
        private int _MinSelections, _MaxSelections;
        private static Size CheckSize = new Size(16, 16);
        private const int CheckRightPadding = 10;
        private const int PreviewColumnPadding = 20;
        private const int PreviewRowPadding = 5;

        public int MinSelections
        {
            get
            {
                return _MinSelections;
            }
            set
            {
                if ((value >= 0) && (value <= NumValues))
                    _MinSelections = value;
            }
        }

        public int MaxSelections
        {
            get
            {
                return _MaxSelections;
            }
            set
            {
                if ((value >= 0) && (value <= NumValues))
                    _MaxSelections = value;
            }
        }

        /// <summary>
        /// gets the number of statements
        /// </summary>
        public int NumValues
        {
            get
            {
                return _nValues;
            }
        }

        /// <summary>
        /// gets an array of the choices
        /// </summary>
        public String[] LabelList
        {
            get
            {
                String[] strAry = new String[_LabelList.Count];
                for (int ctr = 0; ctr < _LabelList.Count; ctr++)
                    strAry[ctr] = _LabelList[ctr];
                return strAry;
            }
        }

        public int GetNumStatements()
        {
            return NumValues;
        }

        public String GetStatement(int n)
        {
            return LabelList[n];
        }

        /// <summary>
        /// Instantiates a CMultiBooleanResponse object with zero statements
        /// </summary>
        public CMultiBooleanResponse()
            : base(EResponseType.MultiBoolean)
        {
            _nValues = 0;
            _LabelList = new List<String>();
        }

        /// <summary>
        /// Instantiates a CMultiBooleanResponse object with nValues statements and adds nValues # of empty strings to
        /// the statement list
        /// </summary>
        /// <param name="nValues">The number of statements in the object</param>
        public CMultiBooleanResponse(int nValues)
            : base(EResponseType.MultiBoolean)
        {
            _nValues = nValues;
            _LabelList = new List<String>(nValues);
            for (int ctr = 0; ctr < nValues; ctr++)
                _LabelList.Add(String.Empty);
            _MinSelections = _MaxSelections = 0;
        }

        /// <summary>
        /// The copy constructor
        /// </summary>
        /// <param name="r">The CMultiBooleanResponse object to be copied</param>
        public CMultiBooleanResponse(CMultiBooleanResponse r)
            : base(EResponseType.MultiBoolean, r)
        {
            _nValues = r._nValues;
            _LabelList = new List<String>(_nValues);
            for (int ctr = 0; ctr < _nValues; ctr++)
                _LabelList.Add(r._LabelList[ctr]);
            _MinSelections = r._MinSelections;
            _MaxSelections = r._MaxSelections;
        }

        public override object Clone()
        {
            return new CMultiBooleanResponse(this);
        }

        /// <summary>
        /// Sets the text of a statement
        /// </summary>
        /// <param name="ndx">The zero-based index of the Label to set</param>
        /// <param name="sLabel">The value to set the label to</param>
        public void SetLabel(int ndx, String sLabel)
        {
            _LabelList[ndx] = sLabel;
        }

        /// <summary>
        /// Appends a statement to the end of the statement list and increments NumValues
        /// </summary>
        /// <param name="sLabel"></param>
        public void AddLabel(String sLabel)
        {
            _LabelList.Add(sLabel);
            _nValues++;
        }

        public void RemoveLabel(int ndx)
        {
            _LabelList.RemoveAt(ndx);
            _nValues--;
        }

        /// <summary>
        /// Validates the object's data
        /// </summary>
        /// <returns>"true" if the statement list contains at least one statement and if no statements
        /// in the list are equal to the empty string, "false" otherwise</returns>
        public override bool IsValid()
        {
            if (NumValues < 1)
                return false;
            for (int ctr = 0; ctr < NumValues; ctr++)
            {
                if (LabelList[ctr] == String.Empty)
                    return false;
            }
            return true;
        }


        public override XElement AsXElement()
        {
            XElement elem = new XElement("Response", new XAttribute("MinSelections", MinSelections.ToString()), new XAttribute("MaxSelections", MaxSelections.ToString()),
                new XAttribute("NumChoices", NumValues.ToString()));
            foreach (String choice in LabelList)
                elem.Add(new XElement("Label", choice));
            elem.Add(Format.AsXElement());
            return elem;
        }

        public override void Load(XElement elem)
        {
            _MinSelections = Convert.ToInt32(elem.Attribute("MinSelections").Value);
            _MaxSelections = Convert.ToInt32(elem.Attribute("MaxSelections").Value);
            _LabelList.Clear();
            foreach (XElement e in elem.Elements("Label"))
                _LabelList.Add(e.Value);
            _nValues = Convert.ToInt32(elem.Attribute("NumChoices").Value);
            Format.Load(elem.Element("Format"));
        }

        public override void WriteXml(XmlWriter writer)
        {
            // write the start of the "Response" element, signifying the beginning of a new response type
            writer.WriteStartElement("MultiBoolean");

            Format.WriteXml(writer);

            // write the number of statements
            writer.WriteElementString("NumValues", NumValues.ToString());
            writer.WriteElementString("MinSelections", MinSelections.ToString());
            writer.WriteElementString("MaxSelections", MaxSelections.ToString());

            // write the start of an element to contain the statements, followed by a list of elements that
            // contain each statement
            writer.WriteStartElement("Labels");
            for (int ctr = 0; ctr < NumValues; ctr++)
            {
                writer.WriteStartElement("Label");
                writer.WriteString(LabelList[ctr]);
                writer.WriteEndElement();
            }
            writer.WriteEndElement();

            // close the "Response" element
            writer.WriteEndElement();
        }

        public override string GetResponseDesc()
        {
            String desc = "\tEach zero or one in the response is a bit that specifies if a certain selection was made.\r\n";
            desc += "\tBelow is a synopsis of the bits included in the response in left-to-right order.\r\n";
            for (int ctr = 0; ctr < LabelList.Length; ctr++)
                desc += String.Format("\t\tbit #{0}:  {1}\r\n", ctr + 1, LabelList[ctr]);

            return desc;
        }

        public override Response GenerateSerializableResponse(SurveyItem parentItem)
        {
            MultiBoolean r = new MultiBoolean(parentItem);
            r.Choices = LabelList;
            return r;
        }

        public override CSpecifierControlDefinition GetSpecifierControlDefinition()
        {
            CSpecifierControlDefinition def = new CSpecifierControlDefinition(DynamicSpecifier.ESpecifierType.Mask);
            def.Statements.AddRange(LabelList);
            for (int ctr = 0; ctr < LabelList.Length; ctr++)
                def.Values.Add(((int)Math.Pow(10, LabelList.Length - (ctr - 1))).ToString());
            return def;
        }
    }
}

using IATClient.IATConfig;
using IATClient.ResultData;
using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Linq;

namespace IATClient
{
    /// <summary>
    /// CWeightedMultipleResponse provides for the definition of a weighted multiple choice response
    /// </summary>
    class CWeightedMultipleResponse : CResponse
    {
        // the number of choices, the list of choices, and the list of weights
        private List<String> _Choices;
        private List<int> _Weights;

        /// <summary>
        /// gets the number of choices
        /// </summary>
        public int NumChoices
        {
            get
            {
                return _Choices.Count;
            }
        }

        /// <summary>
        /// gets an array of the choices
        /// </summary>
        public String[] Choices
        {
            get
            {
                String[] strAry = new String[_Choices.Count];
                for (int ctr = 0; ctr < _Choices.Count; ctr++)
                    strAry[ctr] = _Choices[ctr];
                return strAry;
            }
        }

        /// <summary>
        /// gets an array of the weights
        /// </summary>
        public int[] Weights
        {
            get
            {
                int[] intAry = new int[_Weights.Count];
                for (int ctr = 0; ctr < _Weights.Count; ctr++)
                    intAry[ctr] = _Weights[ctr];
                return intAry;
            }
        }

        /// <summary>
        /// instantiates a CWeightedMultipleResponse response object with zero choices
        /// </summary>
        public CWeightedMultipleResponse()
            : base(EResponseType.WeightedMultiple)
        {
            _Choices = new List<String>();
            _Weights = new List<int>();
        }

        /// <summary>
        /// instantiates a CWeightedMultipleResponse object with a given number of choices, initializing
        /// the choice and weight lists with empty strings and int.MinValue values, respectively
        /// </summary>
        /// <param name="nChoices">The number of choices in the response</param>
        public CWeightedMultipleResponse(int nChoices)
            : base(EResponseType.WeightedMultiple)
        {
            _Choices = new List<String>(nChoices);
            _Weights = new List<int>(nChoices);
            for (int ctr = 0; ctr < nChoices; ctr++)
            {
                _Choices.Add(String.Empty);
                _Weights.Add(int.MinValue);
            }
        }

        /// <summary>
        /// The copy constructor
        /// </summary>
        /// <param name="r">The CWeightedMultipleChoice object to copy</param>
        public CWeightedMultipleResponse(CWeightedMultipleResponse r)
            : base(EResponseType.WeightedMultiple, r)
        {
            _Choices = new List<String>();
            _Weights = new List<int>();
            for (int ctr = 0; ctr < r.NumChoices; ctr++)
            {
                _Choices.Add(r._Choices[ctr]);
                _Weights.Add(r._Weights[ctr]);
            }
        }

        public override object Clone()
        {
            return new CWeightedMultipleResponse(this);
        }

        /// <summary>
        /// Sets the specified choice to a given value
        /// </summary>
        /// <param name="nChoice">The zero-based index of the choice</param>
        /// <param name="sChoice">The text of the choice</param>
        public void SetChoice(int nChoice, String sChoice)
        {
            _Choices[nChoice] = sChoice;
        }

        /// <summary>
        /// Sets thhe specified choice its weight to the given values
        /// </summary>
        /// <param name="nChoice">The zero-based index of the choice</param>
        /// <param name="sChoice">The text of the choice</param>
        /// <param name="nWeight">The weight of the choice</param>
        public void SetChoice(int nChoice, String sChoice, int nWeight)
        {
            _Choices[nChoice] = sChoice;
            _Weights[nChoice] = nWeight;
        }

        /// <summary>
        /// Sets the weight of the specified choice
        /// </summary>
        /// <param name="nChoice">The zero-based index of the choice</param>
        /// <param name="nWeight">The weight of the choice</param>
        public void SetWeight(int nChoice, int nWeight)
        {
            _Weights[nChoice] = nWeight;
        }

        /// <summary>
        /// Appends a choice to the response object
        /// </summary>
        /// <param name="sChoice">The text of the choice</param>
        /// <param name="nWeight">The weight of the choice</param>
        public void AddChoice(String sChoice, int nWeight)
        {
            _Choices.Add(sChoice);
            _Weights.Add(nWeight);
        }

        public void RemoveChoice(int ndx)
        {
            _Choices.RemoveAt(ndx);
            _Weights.RemoveAt(ndx);
        }

        /// <summary>
        /// Tests to see if the object data is valid
        /// </summary>
        /// <returns>"true" if the object contains a valid response definition, otherwise "false"</returns>
        public override bool IsValid()
        {
            if (NumChoices < 1)
                return false;
            for (int ctr = 0; ctr < NumChoices; ctr++)
            {
                if ((Choices[ctr] == String.Empty) || (Weights[ctr] == int.MinValue))
                    return false;
            }
            return true;
        }

        public override XElement AsXElement()
        {
            XElement elem = new XElement("Response");
            for (int ctr = 0; ctr < NumChoices; ctr++)
                elem.Add(new XElement("Choice", new XAttribute("Weight", Weights[ctr].ToString()), Choices[ctr]));
            elem.Add(Format.AsXElement());
            return elem;
        }

        public override void Load(XElement elem)
        {
            _Choices.Clear();
            _Weights.Clear();
            foreach (XElement e in elem.Elements("Choice"))
            {
                _Choices.Add(e.Value);
                _Weights.Add(Convert.ToInt32(e.Attribute("Weight").Value));
            }
            Format.Load(elem.Element("Format"));
        }

        public override void WriteXml(XmlWriter writer)
        {
            writer.WriteStartElement("WeightedMultipleResponse");
            writer.WriteAttributeString("Type", sTypeWeightedMultiple);
            writer.WriteAttributeString("NumChoices", NumChoices.ToString());
            writer.WriteAttributeString("Scored", false.ToString());

            Format.WriteXml(writer);

            writer.WriteStartElement("WeightedChoices");
            for (int ctr = 0; ctr < NumChoices; ctr++)
            {
                writer.WriteStartElement("WeightedChoice");
                writer.WriteElementString("Choice", _Choices[ctr]);
                writer.WriteElementString("Weight", _Weights[ctr].ToString());
                writer.WriteEndElement();
            }
            writer.WriteEndElement();
            writer.WriteEndElement();
        }


        public override string GetResponseDesc()
        {
            String desc = String.Empty;
            for (int ctr = 0; ctr < Choices.Length; ctr++)
                desc += String.Format("\t{0} - {1}\r\n", Weights[ctr], Choices[ctr]);
            return desc;
        }

        public override Response GenerateSerializableResponse(SurveyItem parentItem)
        {
            WeightedMultiple r = new WeightedMultiple(parentItem);
            WeightedChoice[] choices = new WeightedChoice[Choices.Length];
            for (int ctr = 0; ctr < Choices.Length; ctr++)
                choices[ctr] = new WeightedChoice(Weights[ctr], Choices[ctr]);
            r.Choices = choices;
            return r;
        }

        public override CSpecifierControlDefinition GetSpecifierControlDefinition()
        {
            CSpecifierControlDefinition def = new CSpecifierControlDefinition(DynamicSpecifier.ESpecifierType.Selection);
            def.Statements.AddRange(Choices);
            for (int ctr = 0; ctr < Weights.Length; ctr++)
                def.Values.Add(Weights[ctr].ToString());
            return def;
        }

        private List<CheckBox> AnswerChecks = new List<CheckBox>();
        private List<RadioButton> AnswerRadios = new List<RadioButton>();
        private List<TextBox> AnswerBoxes = new List<TextBox>();

        public int GetNumStatements()
        {
            return NumChoices;
        }

        public String GetChoice(int n)
        {
            return Choices[n];
        }

        public int GetChoiceWeight(int n)
        {
            return Weights[n];
        }

    }
}

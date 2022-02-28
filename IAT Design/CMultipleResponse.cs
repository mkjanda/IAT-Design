using IATClient.IATConfig;
using IATClient.ResultData;
using System;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Linq;

namespace IATClient
{
    /// <summary>
    /// CMultipleResponse provides a class that provides for a definition that of a multiple choice response
    /// </summary>
    class CMultipleResponse : CResponse
    {
        public int NumChoices
        {
            get
            {
                return ChoiceList.Count;
            }
        }

        public List<String> ChoiceList { get; private set; } = new List<string>();

        /// <summary>
        /// gets an array of the choices
        /// </summary>
        public String[] Choices
        {
            get
            {
                String[] strAry = new String[ChoiceList.Count];
                for (int ctr = 0; ctr < ChoiceList.Count; ctr++)
                    strAry[ctr] = ChoiceList[ctr];
                return strAry;
            }
        }

        public int GetNumStatements()
        {
            return Choices.Length;
        }

        public String GetStatement(int n)
        {
            return Choices[n];
        }

        /// <summary>
        /// Instantiates a CMultipleResponse object with zero choices
        /// </summary>
        public CMultipleResponse()
            : base(EResponseType.Multiple)
        {
        }

        /// <summary>
        /// Instantiates a CMultipleResponse object with nChoices choices and fills the choice list
        /// with NumChoices # of empty strings
        /// </summary>
        /// <param name="nChoices"></param>
        public CMultipleResponse(int nChoices)
            : base(EResponseType.Multiple)
        {
            for (int ctr = 0; ctr < nChoices; ctr++)
                ChoiceList.Add(String.Empty);
        }

        /// <summary>
        /// The copy constructor
        /// </summary>
        /// <param name="r">The CMultipleRespone object to be copied</param>
        public CMultipleResponse(CMultipleResponse r)
            : base(EResponseType.Multiple, r)
        {
            ChoiceList.AddRange(r.ChoiceList);
        }

        public override object Clone()
        {
            return new CMultipleResponse(this);
        }

        /// <summary>
        /// Sets the choice with zero-based index ndx to sChoice
        /// </summary>
        /// <param name="ndx">The zero-based index of the choice to be set</param>
        /// <param name="sChoice">The text of the choice</param>
        /// <returns>"true" on success, "false" on error</returns>
        public bool SetChoice(int ndx, String sChoice)
        {
            if (ndx > NumChoices)
                return false;
            ChoiceList[ndx] = sChoice;
            return true;
        }

        /// <summary>
        /// Appends a choice to the choice list and increments NumChoices
        /// </summary>
        /// <param name="sChoice">The choice to append</param>
        public void AddChoice(String sChoice)
        {
            ChoiceList.Add(sChoice);
        }

        /// <summary>
        /// Validates the object's data
        /// </summary>
        /// <returns>"true" if the object contains at least one choice and if no choices are equal to the empty string,
        /// otherwise "false"</returns>
        public override bool IsValid()
        {
            if (NumChoices < 0)
                return false;
            for (int ctr = 0; ctr < NumChoices; ctr++)
            {
                if (Choices[ctr] == String.Empty)
                    return false;
            }
            return true;
        }

        public override XElement AsXElement()
        {
            XElement elem = new XElement("Response");
            foreach (String str in ChoiceList)
                elem.Add(new XElement("Choice", str));
            elem.Add(Format.AsXElement());
            return elem;
        }

        public override void Load(XElement elem)
        {
            ChoiceList.Clear();
            foreach (XElement e in elem.Elements("Choice"))
                ChoiceList.Add(e.Value);
            Format.Load(elem.Element("Format"));
        }

        public override void WriteXml(XmlWriter writer)
        {
            // write the start of the "Response" element to signify a new response type
            writer.WriteStartElement("Response");

            // write the type of the response as an attribute of the "Response" element
            writer.WriteStartAttribute("Type");
            writer.WriteString(sTypeMultiple);
            writer.WriteEndAttribute();

            Format.WriteXml(writer);

            // write the number of choices
            writer.WriteElementString("NumChoices", NumChoices.ToString());

            // write each choice as a child of a "Choices" element
            writer.WriteStartElement("Choices");
            for (int ctr = 0; ctr < NumChoices; ctr++)
                writer.WriteElementString("Choice", Choices[ctr]);
            writer.WriteEndElement();

            // close the "Response" element
            writer.WriteEndElement();
        }

        public override string GetResponseDesc()
        {
            String desc = String.Empty;
            for (int ctr = 0; ctr < Choices.Length; ctr++)
                desc += String.Format("\t{0} - {1}", ctr + 1, Choices[ctr]);

            return desc;
        }

        public override Response GenerateSerializableResponse(SurveyItem parentItem)
        {
            Multiple r = new Multiple(parentItem);
            for (int ctr = 0; ctr < Choices.Length; ctr++)
                r.Choices = Choices;

            return r;
        }

        public override CSpecifierControlDefinition GetSpecifierControlDefinition()
        {
            CSpecifierControlDefinition def = new CSpecifierControlDefinition(DynamicSpecifier.ESpecifierType.Selection);
            def.Statements.AddRange(Choices);
            for (int ctr = 1; ctr <= Choices.Length; ctr++)
                def.Values.Add(ctr.ToString());
            return def;
        }
    }
}

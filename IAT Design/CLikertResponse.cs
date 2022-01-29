using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Windows.Forms;
using System.Drawing;
using System.Xml.Linq;
using IATClient.ResultData;
using IATClient.IATConfig;

namespace IATClient
{
    /// <summary>
    /// CLikertResponse provides a class that can hold a definition for a likert response type
    /// </summary>
    class CLikertResponse : CResponse
    {

        // the member variables for the number of choices, whether the response is reverse-scored,
        // and the list of choices 
        public List<String> ChoiceDescriptions { get; private set; } = new List<String>();

        public static string[] DefaultResponses = { Properties.Resources.sLikertDefaultResponse1, Properties.Resources.sLikertDefaultResponse2, 
                                                      Properties.Resources.sLikertDefaultResponse3, Properties.Resources.sLikertDefaultResponse4,
                                                      Properties.Resources.sLikertDefaultResponse5, Properties.Resources.sLikertDefaultResponse6,
                                                      Properties.Resources.sLikertDefaultResponse7 };

        /// <summary>
        /// gets the number of choices
        /// </summary>
        public int NumChoices
        {
            get
            {
                return ChoiceDescriptions.Count;
            }
        }


        public bool IsReverseScored()
        {
            return ReverseScored;
        }
        
        public int GetNumStatements()
        {
            return ChoiceDescriptions.Count;
        }

        public String GetStatement(int n)
        {
            return ChoiceDescriptions[n];
        }


        /// <summary>
        /// gets or sets whether the item is reverse-scored
        /// </summary>
        public bool ReverseScored { get; set; } = false;

        /// <summary>
        /// The default constructor.  Sets the statement list to the default list of likert statements
        /// and instantiates the list of responses.
        /// </summary>
        public CLikertResponse()
            : base(EResponseType.Likert)
        {
            ChoiceDescriptions = new List<String>();
            ReverseScored = false;

            // insert default likert statements
            ChoiceDescriptions.Add(Properties.Resources.sLikertDefaultResponse1);
            ChoiceDescriptions.Add(Properties.Resources.sLikertDefaultResponse2);
            ChoiceDescriptions.Add(Properties.Resources.sLikertDefaultResponse3);
            ChoiceDescriptions.Add(Properties.Resources.sLikertDefaultResponse4);
            ChoiceDescriptions.Add(Properties.Resources.sLikertDefaultResponse5);
            ChoiceDescriptions.Add(Properties.Resources.sLikertDefaultResponse6);
            ChoiceDescriptions.Add(Properties.Resources.sLikertDefaultResponse7);
        }


        /// <summary>
        /// This constructor sets the number of choices to the passed value, defaults ReverseSccored to "false",
        /// instantiates the list of responses and sets its capacity to the passed number of choices
        /// </summary>
        /// <param name="nChoices">The number of statements in the likert scoring system</param>
        public CLikertResponse(int nChoices) : base(EResponseType.Likert)
        {
            ChoiceDescriptions = new List<String>();
            for (int ctr = 0; ctr < nChoices; ctr++)
                ChoiceDescriptions[ctr] = String.Empty;
            ReverseScored = false;
        }

        /// <summary>
        /// Instantiates a CLikertResponse object with the passed number of choices and given scoring
        /// </summary>
        /// <param name="nChoices">The number of statements in the likert scoring system</param>
        /// <param name="bReverseScored">"true" for a reverse-scored item, otherwise "false"</param>
        public CLikertResponse(int nChoices, bool bReverseScored)
            : base(EResponseType.Likert)
        {
            ChoiceDescriptions = new List<String>();
            for (int ctr = 0; ctr < nChoices; ctr++)
                ChoiceDescriptions.Add(String.Empty);
            ReverseScored = bReverseScored;
        }

        /// <summary>
        /// The copy constructor
        /// </summary>
        /// <param name="r">The CLikertResponse object to be copy</param>
        public CLikertResponse(CLikertResponse r)
            : base(EResponseType.Likert, r)
        {
            ChoiceDescriptions = new List<String>();
            for (int ctr = 0; ctr < r.NumChoices; ctr++)
                ChoiceDescriptions.Add(r.ChoiceDescriptions[ctr]);
            ReverseScored = r.ReverseScored;
        }

        public override object Clone()
        {
            return new CLikertResponse(this);
        }

        /// <summary>
        /// Sets the specified choice to the specified statement
        /// </summary>
        /// <param name="nChoice">The zero-based index of the choice to be set</param>
        /// <param name="sDesc">The statement for this choice</param>
        public void SetChoiceDesc(int nChoice, String sDesc)
        {
            ChoiceDescriptions[nChoice] = sDesc;
        }

        /// <summary>
        /// Validates the object's data
        /// </summary>
        /// <returns>"true" if NumChoices is greater than 0 and each choice has been set, otherwise "false"</returns>
        public override bool IsValid()
        {
            if (NumChoices < 1)
                return false;
            for (int ctr = 0; ctr < NumChoices; ctr++)
            {
                if (ChoiceDescriptions[ctr] == String.Empty)
                    return false;
            }
            return true;
        }


        public override XElement AsXElement()
        {
            XElement elem = new XElement("Response", new XAttribute("IsReverseScored", ReverseScored.ToString()));
            foreach (String choice in ChoiceDescriptions)
                elem.Add(new XElement("Choice", choice));
            elem.Add(Format.AsXElement());
            return elem;
        }

        public override void Load(XElement elem)
        {
            ReverseScored = Convert.ToBoolean(elem.Attribute("IsReverseScored").Value);
            ChoiceDescriptions.Clear();
            foreach (XElement e in elem.Elements("Choice"))
                ChoiceDescriptions.Add(e.Value);
            Format.Load(elem.Element("Format"));
        }

        public override void WriteXml(XmlWriter writer)
        {
            // write the start of the "Response" element, signifying the start of a new response type
            writer.WriteStartElement("Response");

            // write the type of the response as an attribute of the "Resposne" element
            writer.WriteStartAttribute("Type");
            writer.WriteString(sTypeLikert);
            writer.WriteEndAttribute();

            Format.WriteXml(writer);

            // write number of choices, followed by the boolean value of ReverseScored
            writer.WriteElementString("NumChoices", NumChoices.ToString());
            writer.WriteElementString("IsReverseScored", ReverseScored.ToString());

            // write the start of the "ChoiceDescription" element followed by a series of "Choice" elements
            // that contain the text of each choice
            writer.WriteStartElement("ChoiceDescriptions");
            for (int ctr = 0; ctr < NumChoices; ctr++)
                writer.WriteElementString("Choice", ChoiceDescriptions[ctr]);
            writer.WriteEndElement();

            // close the "Response" element
            writer.WriteEndElement();
        }


        public override string GetResponseDesc()
        {
            if (ReverseScored)
                return String.Format("\tReverse scored Likert item with response range 1 to {0}\r\n", ChoiceDescriptions.Count);
            else
                return String.Format("\tLikert item with response range 1 to {0}\r\n", ChoiceDescriptions.Count);
        }

        public override Response GenerateSerializableResponse(SurveyItem parentItem)
        {
            Likert r = new Likert(parentItem);
            r.ReverseScored = ReverseScored;
            r.Choices = ChoiceDescriptions.ToArray();

            return r;
        }

        public override CSpecifierControlDefinition GetSpecifierControlDefinition()
        {
            CSpecifierControlDefinition def = new CSpecifierControlDefinition(DynamicSpecifier.ESpecifierType.Range);
            def.Statements.AddRange(ChoiceDescriptions);
            if (ReverseScored)
                for (int ctr = ChoiceDescriptions.Count; ctr > 0; ctr--)
                    def.Values.Add(ctr.ToString());
            else
                for (int ctr = 1; ctr <= ChoiceDescriptions.Count; ctr++)
                    def.Values.Add(ctr.ToString());
            return def;
        }
    }
}

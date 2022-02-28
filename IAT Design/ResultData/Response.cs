using System;
using System.Xml.Serialization;

namespace IATClient.ResultData
{
    [Serializable]
    [XmlInclude(typeof(BoundedLength))]
    [XmlInclude(typeof(BoundedNumber))]
    [XmlInclude(typeof(FixedDigit))]
    [XmlInclude(typeof(RegEx))]
    [XmlInclude(typeof(WeightedMultiple))]
    [XmlInclude(typeof(MultiBoolean))]
    [XmlInclude(typeof(Date))]
    [XmlInclude(typeof(Likert))]
    [XmlInclude(typeof(Multiple))]
    [XmlInclude(typeof(Boolean))]
    public class Response
    {
        [XmlAttribute(AttributeName = "ResponseType", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public ResponseType ResponseType { get; set; }

        protected SurveyItem ParentItem;


        public Response(SurveyItem parentItem)
        {
            ParentItem = parentItem;
            ResponseType = ResponseType.None;
        }

        public Response()
        { }

        public String GetSurveyName()
        {
            return ParentItem.GetSurveyName();
        }

        public int GetItemNum()
        {
            return ParentItem.GetItemNum();
        }

        public virtual void PostSerialize(SurveyItem si)
        {
            ParentItem = si;
        }

        public virtual String GetResponseDesc()
        {
            return String.Empty;
        }

        public virtual int GetNumDescriptionSubItems()
        {
            throw new NotImplementedException();
        }

        public virtual String GetDescriptionSubItem(int ndx)
        {
            throw new NotImplementedException();
        }
    }
}

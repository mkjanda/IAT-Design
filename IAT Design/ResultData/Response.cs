﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace IATClient.ResultData
{
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

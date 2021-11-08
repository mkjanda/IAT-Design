using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace IATClient
{
    class SerializedSurveyDisplay : Panel
    {
        private Survey s;

        public SerializedSurveyDisplay(Survey s)
        {
            XmlDocument doc = new XmlDocument();
            for (int ctr = 0; ctr < s.NumItems; ctr++)
                doc.AppendChild(s.SurveyItems[ctr].CreateSourceNode());
        }
    }
}

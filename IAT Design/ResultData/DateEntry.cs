﻿using System;
using System.Xml.Serialization;

namespace IATClient.ResultData
{
    [Serializable]
    public class DateEntry
    {
        [XmlElement(ElementName = "Year", Form = System.Xml.Schema.XmlSchemaForm.Unqualified, IsNullable = false)]
        public int Year { get; set; }
        [XmlElement(ElementName = "Month", Form = System.Xml.Schema.XmlSchemaForm.Unqualified, IsNullable = false)]
        public int Month { get; set; }
        [XmlElement(ElementName = "Day", Form = System.Xml.Schema.XmlSchemaForm.Unqualified, IsNullable = false)]
        public int Day { get; set; }

        public DateEntry()
        {
            Year = Month = Day = -1;
        }

        public DateEntry(DateTime date)
        {
            Year = date.Year;
            Month = date.Month;
            Day = date.Day;
        }
    }
}



using System;
using System.Collections.Generic;
using System.Drawing;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace IATClient
{
    [Serializable]
    public class CReportableException : IReportableException
    {
        [Serializable]
        public class CInnerException
        {
            private String _ExceptionMessage;
            private List<String> _StackTrace = new List<String>();

            [XmlElement(ElementName = "ExceptionMessage", Form = XmlSchemaForm.Unqualified, IsNullable = false)]
            public String ExceptionMessage
            {
                get
                {
                    return _ExceptionMessage;
                }
                set
                {
                    _ExceptionMessage = value;
                }
            }

            [XmlElement(ElementName = "StackTraceElement", Form = XmlSchemaForm.Unqualified)]
            public List<String> StackTrace
            {
                get
                {
                    return _StackTrace;
                }
                set { }
            }
        }

        [XmlIgnore]
        public String Caption { get; protected set; } = String.Empty;


        [XmlElement(ElementName = "ExceptionMessage", Form = XmlSchemaForm.Unqualified, IsNullable = false)]
        public String ExceptionMessage { get; set; } = String.Empty;
        protected List<String> _StackTrace = new List<String>();
        protected List<CInnerException> _InnerExceptions = new List<CInnerException>();
        private const int LinePadding = 2;

        [XmlElement(ElementName = "StackTraceElement", Form = XmlSchemaForm.Unqualified)]
        public List<String> StackTrace
        {
            get
            {
                return this._StackTrace;
            }
            set { }
        }

        [XmlElement(ElementName = "InnerException", Form = XmlSchemaForm.Unqualified, IsNullable = true, Type = typeof(CInnerException))]
        public List<CInnerException> InnerExceptions
        {
            get
            {
                return this._InnerExceptions;
            }
            set { }
        }

        public CReportableException()
        { }

        public CReportableException(String caption, Exception ex)
        {
            Caption = caption;
            ExceptionMessage = ex.Message;
            if (ex.StackTrace != null)
                _StackTrace.AddRange(ex.StackTrace.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries));
            while (ex.InnerException != null)
            {
                CInnerException innerEx = new CInnerException();
                innerEx.ExceptionMessage = ex.InnerException.Message;
                if (ex.InnerException.StackTrace != null)
                    innerEx.StackTrace.AddRange(ex.InnerException.StackTrace.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries));
                _InnerExceptions.Add(innerEx);
                ex = ex.InnerException;
            }
        }


        public Size MeasureText(Graphics g, Font f)
        {
            float height = 0F;
            float width = 0F;
            SizeF lineSZ;
            Font boldF = new Font(f, FontStyle.Bold);

            lineSZ = g.MeasureString(Caption, boldF);
            if (lineSZ.Width > width)
                width = lineSZ.Width;
            height += lineSZ.Height + LinePadding;

            lineSZ = g.MeasureString(ExceptionMessage, f);
            if (lineSZ.Width > width)
                width = lineSZ.Width;
            height += lineSZ.Height + LinePadding;

            foreach (String str in _StackTrace)
            {
                lineSZ = g.MeasureString(str, f) + new SizeF(3 * f.Height, LinePadding);
                if (lineSZ.Width > width)
                    width = lineSZ.Width;
                height += lineSZ.Height + LinePadding;
            }

            foreach (CInnerException ex in InnerExceptions)
            {
                lineSZ = g.MeasureString("Caused by . . .", f);
                if (lineSZ.Width > width)
                    width = lineSZ.Width;
                height += lineSZ.Height + LinePadding;

                lineSZ = g.MeasureString(ex.ExceptionMessage, f);
                if (lineSZ.Width > width)
                    width = lineSZ.Width;
                height += lineSZ.Height + LinePadding;

                foreach (String s in ex.StackTrace)
                {
                    lineSZ = g.MeasureString(s, f);
                    if (lineSZ.Width > width)
                        width = lineSZ.Width;
                    height += lineSZ.Height + LinePadding;
                }
            }
            return new Size((int)Math.Ceiling(width), (int)Math.Ceiling(height));
        }

        public void Draw(Graphics g, Font f, Point location)
        {
            float fX = location.X;
            float fY = location.Y;
            Font boldF = new Font(f, FontStyle.Bold);
            SizeF lineSz = g.MeasureString(Caption, boldF);
            g.DrawString(Caption, boldF, Brushes.Black, new PointF(fX, fY));
            fY += lineSz.Height + 2;
            lineSz = g.MeasureString(ExceptionMessage, f);
            g.DrawString(ExceptionMessage, f, Brushes.Black, new PointF(fX, fY));
            fY += lineSz.Height + 2;
            foreach (String s in _StackTrace)
            {
                lineSz = g.MeasureString(s, f);
                g.DrawString(s, f, Brushes.Black, new PointF(fX + f.Height * 3, fY));
                fY += lineSz.Height + 2;
            }
            foreach (CInnerException ie in InnerExceptions)
            {
                lineSz = g.MeasureString("Caused by . . .", f);
                g.DrawString("Caused by . . .", f, Brushes.Black, new PointF(fX, fY));
                fY += lineSz.Height + 2;
                lineSz = g.MeasureString(ie.ExceptionMessage, f);
                g.DrawString(ie.ExceptionMessage, f, Brushes.Black, new PointF(fX, fY));
                fY += lineSz.Height + 2;
                foreach (String s in ie.StackTrace)
                {
                    lineSz = g.MeasureString(s, f);
                    g.DrawString(s, f, Brushes.Black, new PointF(fX + f.Height * 3, fY));
                    fY += lineSz.Height + 2;
                }
            }
        }

        public String GetText()
        {
            List<String> lines = new List<String>();
            lines.Add(Caption);
            lines.Add(ExceptionMessage);
            lines.Add("\t" + String.Join("\n\t", _StackTrace.ToArray()));
            foreach (CInnerException ie in InnerExceptions)
            {
                lines.Add("Caused by . . .");
                lines.Add("\t" + String.Join("\n\t", ie.StackTrace.ToArray()));
            }
            return String.Join("\n", lines.ToArray());
        }
    }
}

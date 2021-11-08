using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Drawing;

namespace IATClient
{
    class CDeploymentException : INamedXmlSerializable
    {
        class CInnerException
        {
            private String _ExceptionMessage;
            private List<String> _StackTrace = new List<String>();

            public String ExceptionMessage {
                get {
                    return _ExceptionMessage;
                }
                set {
                    _ExceptionMessage = value;
                }
            }

            public List<String> StackTrace
            {
                get
                {
                    return _StackTrace;
                }
            }
        }

        private String _Caption, _ExceptionMessage;
        List<String> _StackTrace = new List<String>();
        List<CInnerException> InnerExceptions = new List<CInnerException>();
        private const int LinePadding = 2;
        public String Caption
        {
            get
            {
                return _Caption;
            }
        }

        public String ExceptionMessage {
            get
            {
                return _ExceptionMessage;
            }
        }

        public CDeploymentException()
        {
        }

        public void WriteXml(XmlWriter writer)
        {
            throw new NotImplementedException();
        }

        public void ReadXml(XmlReader reader)
        {
            reader.ReadStartElement();
            _Caption = reader.ReadElementString();
            _ExceptionMessage = reader.ReadElementString();
            while (reader.Name == "StackTraceElement")
                _StackTrace.Add(reader.ReadElementString());
            while (reader.Name == "InnerException")
            {
                CInnerException ex = new CInnerException();
                reader.ReadStartElement();
                ex.ExceptionMessage = reader.ReadElementString();
                while (reader.Name == "StackTraceElement")
                    ex.StackTrace.Add(reader.ReadElementString());
                InnerExceptions.Add(ex);
                reader.ReadEndElement();
            }
        }

        public String GetName()
        {
            return "DeploymentException";
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
                lineSZ = g.MeasureString(str, f) + new SizeF(5 * f.Height, LinePadding);
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

        public void Draw(Graphics g, Font f, int xOffset, int yOffset)
        {
            float fX = xOffset;
            float fY = yOffset;
            Font boldF = new Font(f, FontStyle.Bold);
            SizeF lineSz = g.MeasureString(_Caption, boldF);
            g.DrawString(_Caption, boldF, Brushes.Black, new PointF(fX, fY));
            fY += lineSz.Height + 2;
            lineSz = g.MeasureString(_ExceptionMessage, f);
                g.DrawString(_ExceptionMessage, f, Brushes.Black, new PointF(fX, fY));
            fY += lineSz.Height + 2;
            foreach (String s in _StackTrace)
            {
                lineSz = g.MeasureString(s, f);
                g.DrawString(s, f, Brushes.Black, new PointF(fX + f.Height * 5, fY));
                fY += lineSz.Height + 2;
            }
            foreach (CInnerException ie in InnerExceptions) {
                lineSz = g.MeasureString("Caused by . . .", f);
                g.DrawString("Caused by . . .", f, Brushes.Black, new PointF(fX, fY));
                fY += lineSz.Height + 2;
                lineSz = g.MeasureString(ie.ExceptionMessage, f);
                g.DrawString(ie.ExceptionMessage, f, Brushes.Black, new PointF(fX, fY));
                fY += lineSz.Height + 2;
                foreach (String s in ie.StackTrace)
                {
                    lineSz = g.MeasureString(s, f);
                    g.DrawString(s, f, Brushes.Black, new PointF(fX + f.Height * 5, fY));
                    fY += lineSz.Height + 2;
                }
            }
        }

        public String GetText()
        {
            List<String> lines = new List<String>();
            lines.Add(Caption);
            lines.Add(ExceptionMessage);
            lines.Add(String.Join("\t", _StackTrace.ToArray()));
            foreach (CInnerException ie in InnerExceptions)
            {
                lines.Add("Caused by . . .");
                lines.Add(String.Join("\t", ie.StackTrace.ToArray()));
            }
            return String.Join("\n\r", lines.ToArray());
        }
    }
}

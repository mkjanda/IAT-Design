using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Drawing;

namespace IATClient
{
    class CXmlSerializationException : Exception
    {
        private String _ExceptionType;
        private List<String> _StackTrace = new List<String>();
        public enum EErrorType { fatal, exception, message, terminateProcess };
        private EErrorType _ErrorType;
        private TransactionProgress _TransProgress;
        private String _Message, _Caption;
        private CDeploymentProgressUpdate _DeploymentProgress = null;
        private CDeploymentProgressUpdate.EStage _DeploymentPhase;

        public CDeploymentProgressUpdate DeploymentProgress
        {
            get
            {
                return _DeploymentProgress;
            }
        }

        public CDeploymentProgressUpdate.EStage DeploymentPhase
        {
            get
            {
                return _DeploymentPhase;
            }
        }

        new public String Message
        {
            get
            {
                return _Message;
            }
        }

        public String Caption
        {
            get
            {
                return _Caption;
            }
        }
        
        public TransactionProgress TransProgress
        {
            get
            {
                return _TransProgress;
            }
        }

        public EErrorType ErrorType
        {
            get
            {
                return _ErrorType;
            }
        }

        public String ExceptionType
        {
            get
            {
                return _ExceptionType;
            }
        }

        public new List<String> StackTrace
        {
            get
            {
                return _StackTrace;
            }
        }

        public String GetStringRepresentation()
        {
            String str = (Message == "NULL") ? ExceptionType : Message;
            foreach (String s in _StackTrace)
                str += "\r\n" + s;

            return str;
        }

        public Size MeasureOutput()
        {
            Size szTotal = new Size(0, 0);
            Size szLine;
            String exMsg = (Message == "NULL") ? ExceptionType : Message;
            szLine = System.Windows.Forms.TextRenderer.MeasureText(exMsg, System.Drawing.SystemFonts.DialogFont);
            if (szLine.Width > szTotal.Width)
                szTotal.Width = szLine.Width;
            szTotal.Height += szLine.Height;
            for (int ctr = 0; ctr < StackTrace.Count; ctr++)
            {
                szLine = System.Windows.Forms.TextRenderer.MeasureText(StackTrace[ctr], System.Drawing.SystemFonts.DialogFont);
                if (szLine.Width > szTotal.Width)
                    szTotal.Width = szLine.Width;
                szTotal.Height += szLine.Height;
            }

            return szTotal;
        }

        public float Draw(Graphics g, PointF ptDraw)
        {
            Font f = System.Drawing.SystemFonts.DialogFont;
            String exMsg;
            if ((Boolean)Data["IATServerException"] == true)
                exMsg = (Message == "NULL") ? ExceptionType : Message;
            else
                exMsg = InnerException.Message;
            float height = g.MeasureString(exMsg, f).Height;
            g.DrawString(exMsg, f, System.Drawing.Brushes.Black, ptDraw);
            ptDraw.Y += height;
            for (int ctr = 0; ctr < StackTrace.Count; ctr++)
            {
                height = g.MeasureString(StackTrace[ctr], f).Height;
                g.DrawString(StackTrace[ctr], f, System.Drawing.Brushes.Black, ptDraw);
                ptDraw.Y += height;
            }

              
            return ptDraw.Y;
        }

        public CXmlSerializationException(XmlNode node)
        {
            _ErrorType = (EErrorType)Enum.Parse(typeof(EErrorType), node.SelectSingleNode("ErrorType").InnerText);
            _DeploymentPhase = (CDeploymentProgressUpdate.EStage)Enum.Parse(typeof(CDeploymentProgressUpdate.EStage), node.SelectSingleNode("DeploymentPhase").InnerText);
            _ExceptionType = node.SelectSingleNode("Exception/Exception").InnerText;
            _Message = node.SelectSingleNode("Exception/ExceptionMessage").InnerText;
            foreach (XmlNode n in node.SelectNodes("Exception/StackTrace/TraceLine"))
                StackTrace.Add(n.InnerText);
            if (_ErrorType == EErrorType.terminateProcess)
            {
                _DeploymentProgress = new CDeploymentProgressUpdate();
                DeploymentProgress.ReadXml(node.SelectSingleNode("DeploymentProgressUpdate"));
            }
        }

        public CXmlSerializationException(XmlReader reader)
        {
            Data["IATServerException"] = true;
            reader.ReadStartElement();
            _ErrorType = (EErrorType)Enum.Parse(typeof(EErrorType), reader.ReadElementString());
            _DeploymentPhase = (CDeploymentProgressUpdate.EStage)Enum.Parse(typeof(CDeploymentProgressUpdate.EStage), reader.ReadElementString());
            reader.ReadStartElement();
            _ExceptionType = reader.ReadElementString();
            _Message = reader.ReadElementString();
            int nLines = Convert.ToInt32(reader["NumLines"]);
            reader.ReadStartElement();
            for (int ctr = 0; ctr < nLines; ctr++)
                StackTrace.Add(reader.ReadElementString());
            reader.ReadEndElement();
            if (_ErrorType == EErrorType.terminateProcess)
            {
                reader.ReadStartElement();
                _DeploymentProgress = new CDeploymentProgressUpdate();
                _DeploymentProgress.ReadXml(reader);
                reader.ReadEndElement();
            }
            reader.ReadEndElement();
            reader.ReadEndElement();
         //   _TransProgress = MySOAP.TerminateTransaction(_Message);
        }

        public CXmlSerializationException(String caption, String message, Exception innerException) : base(message, innerException)
        {
            Data["IATServerException"] = false;
            string[] stackTrace = InnerException.StackTrace.Split('\n');
            for (int ctr = 0; ctr < stackTrace.Length; ctr++)
                StackTrace.Add(stackTrace[ctr]);
//            _TransProgress = MySOAP.TerminateTransaction(message);
            _Message = message;
            _Caption = caption;
            _ErrorType = EErrorType.exception;
        }

        public CXmlSerializationException(String caption, String message, Exception innerException, EErrorType errorType)
            : base(message, innerException)
        {
            Data["IATServerException"] = false;
            string[] stackTrace = InnerException.StackTrace.Split('\n');
            for (int ctr = 0; ctr < stackTrace.Length; ctr++)
                StackTrace.Add(stackTrace[ctr]);
  //          _TransProgress = MySOAP.TerminateTransaction(message);
            _Message = message;
            _Caption = caption;
            _ErrorType = errorType;
        }
    }
}

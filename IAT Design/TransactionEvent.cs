using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace IATClient
{
    class TransactionEvent
    {
        private String _Name;
        private bool IsTopLevel = true;
        private int _MaxProgressValue = -1, _ProgressValue = -1;
        private List<TransactionEvent> _ChildEvents = new List<TransactionEvent>();

        public String Name
        {
            get
            {
                return _Name;
            }
            set
            {
                _Name = value;
            }
        }

        public List<TransactionEvent> ChildEvents {
            get {
                return _ChildEvents;
            }
        }

        public int MaxProgressValue
        {
            get
            {
                return _MaxProgressValue;
            }
            set
            {
                _MaxProgressValue = value;
            }
        }

        public int ProgressValue
        {
            get
            {
                return _ProgressValue;
            }
            set
            {
                _ProgressValue = value;
            }
        }

        public TransactionEvent(String name)
        {
            _Name = name;
        }

        public void AddChildEvent(TransactionEvent dEvent)
        {
            ChildEvents.Add(dEvent);
        }

        public TransactionEvent AddChildEvent(String name)
        {
            TransactionEvent tEvent = new TransactionEvent(name);
            ChildEvents.Add(tEvent);
            return tEvent;
        }

        public SizeF Draw(Graphics g, Font f, PointF ptOrigin)
        {
            String str = Name;
            if (MaxProgressValue > 0)
                str += String.Format(" ({0}/{1})", ProgressValue, MaxProgressValue);

            SizeF szStr = g.MeasureString(str, f, ptOrigin, StringFormat.GenericTypographic);
            g.DrawString(str, f, System.Drawing.Brushes.Black, ptOrigin);
            return szStr;
        }
    }

    class TransactionProgress
    {
        public enum EDeploymentStages
        {
            EstablishEncryption, ShakeHands, VerifyUser, CheckIATExists, QueryDployeAtopOldIAT, VerifyPassword, InitializeEncryptionKeys, RegisterIAT,
            RegisterEncryptionKey, ServerSideDeploymentInitialization, UploadManifest, AwaitingDeploymentVerification
        }
        /*
        static private string[] UploadStageMessages = { Properties.Resources.sUploadEstablishEncryption, Properties.Resources.sUploadShakeHands, Properties.Resources.sUploadVerifyUser,
                                                        Properties.Resources.sUploadCheckIATExists, Properties.Resources.sUploadQueryDeployAtopOldIAT, Properties.Resources.sUploadVerifyPassword,
                                                        Properties.Resources.sUploadInitializeEncryptionKeys, Properties.Resources.UploadRegisterIAT, Properties.Resources.sUploadRegisterEncryptionKey,
                                                        Properties.Resources.sUploadServerSideDeploymentInitialization, Properties.Resources.UploadManifest, Properties.Resources.sAwaitingDeploymentVerification };
        */

        private String _TerminationReason = String.Empty;

        public enum ETransactionType { Activation, IAT, Deletion, DataRetrieval, RetrieveIATList, RetrieveItemSlides }

        private List<TransactionEvent> TransactionEvents = new List<TransactionEvent>();

        public TransactionEvent CurrentEvent 
        {
            get
            {
                return TransactionEvents.Last();
            }
        }

        public void BeginNewEvent(TransactionEvent dEvent)
        {
            TransactionEvents.Add(dEvent);
        }

        private String _IATName;
        private ETransactionType _TransType;

        public ETransactionType TransactionType
        {
            get
            {
                return _TransType;
            }
        }

        public String IATName
        {
            get
            {
                return _IATName;
            }
        }

        public TransactionProgress(ETransactionType type)
        {
            _TransType = type;
        }

        public TransactionProgress(ETransactionType type, String iatName)
        {
            _TransType = type;
            _IATName = iatName;
        }

        public void Terminate(String reason)
        {
            _TerminationReason = reason;
        }

        public float Draw(Graphics g, PointF ptOrigin)
        {
            Font f = System.Drawing.SystemFonts.DialogFont;
            Font boldF = new Font(f, FontStyle.Bold);
            PointF ptDraw = new PointF(ptOrigin.X, ptOrigin.Y);

            for (int ctr = 0; ctr < TransactionEvents.Count; ctr++)
            {
                ptDraw.Y += TransactionEvents[ctr].Draw(g, boldF, ptDraw).Height;
                for (int ctr2 = 0; ctr2 < TransactionEvents[ctr].ChildEvents.Count; ctr2++)
                    ptDraw.Y += TransactionEvents[ctr].ChildEvents[ctr2].Draw(g, f, ptDraw).Height;
            }

            if (_TerminationReason != String.Empty)
            {
                ptDraw.Y += (2 * f.Height);
                float height = g.MeasureString("[Error Encountered]", boldF, ptDraw, StringFormat.GenericTypographic).Height;
                g.DrawString("[Error Encountered]", boldF, System.Drawing.Brushes.Black, ptDraw);
                ptDraw.Y += height;
                height = g.MeasureString(_TerminationReason, boldF, ptDraw, StringFormat.GenericTypographic).Height;
                g.DrawString(_TerminationReason, boldF, System.Drawing.Brushes.Black, ptDraw);
                ptDraw.Y += height;

            }

            return ptDraw.Y - ptOrigin.Y;
        }

        public Size MeasureOutput()
        {
            Size szTotal = new Size(0, 0);
            Size szLine;
            Font measureFont = new Font(System.Drawing.SystemFonts.DialogFont.FontFamily, System.Drawing.SystemFonts.DialogFont.Size / .975F);
            Font boldF = new Font(measureFont, FontStyle.Bold);
            for (int ctr = 0; ctr < TransactionEvents.Count; ctr++)
            {
                szLine = System.Windows.Forms.TextRenderer.MeasureText(TransactionEvents[ctr].Name, boldF);
                if (szLine.Width > szTotal.Width)
                    szTotal.Width = szLine.Width;
                szTotal.Height += szLine.Height;
                for (int ctr2 = 0; ctr2 < TransactionEvents[ctr].ChildEvents.Count; ctr2++)
                {
                    szLine = System.Windows.Forms.TextRenderer.MeasureText(TransactionEvents[ctr].Name, measureFont);
                    if (szLine.Width > szTotal.Width)
                        szTotal.Width = szLine.Width;
                    szTotal.Height += szLine.Height;
                }
            }

            if (_TerminationReason != String.Empty)
            {
                szTotal.Height += System.Drawing.SystemFonts.DialogFont.Height * 2;
                szLine = System.Windows.Forms.TextRenderer.MeasureText("[Error Encountered]", measureFont);
                if (szLine.Width > szTotal.Width)
                    szTotal.Width = szLine.Width;
                szTotal.Height += szLine.Height;
                szTotal.Height += System.Drawing.SystemFonts.DialogFont.Height * 2;
                szLine = System.Windows.Forms.TextRenderer.MeasureText(_TerminationReason, measureFont);
                if (szLine.Width > szTotal.Width)
                    szTotal.Width = szLine.Width;
                szTotal.Height += szLine.Height;

            }
            measureFont.Dispose();
            boldF.Dispose();
            return szTotal;
        }

        public String GetTransactionHistory()
        {
            String str = String.Empty;

            for (int ctr = 0; ctr < TransactionEvents.Count; ctr++)
            {
                str += TransactionEvents[ctr].Name + "\r\n";
                for (int ctr2 = 0; ctr2 < TransactionEvents[ctr].ChildEvents.Count; ctr2++)
                    str += TransactionEvents[ctr].ChildEvents[ctr2].Name + "\r\n";
            }

            return str;
        }
    }
}

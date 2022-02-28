/*using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Threading;
using System.IO;
using System.Xml;
using System.Windows.Forms;
using System.Security.Cryptography;
using System.Net;
using System.Xml.Serialization;

namespace IATClient
{

    class COneStageUploader : ITransmissionOwner
    {
        public delegate bool IncrementProgressHandler(int n);
        private object lockObject = new object();
        private CPacketTransmission Transmission;
        private CIAT IAT;
        private bool bUploadSuccessful = false;
        private IATConfigMainForm MainForm;
        private String ServerURL = Properties.Resources.sDefaultIATServer;
        private bool AbortFlag = false;
        private IATConfigMainForm.SetProgressRangeHandler SetProgressRange;
        private IATConfigMainForm.DisplayYesNoMessageBoxHandler DisplayYesNoMessageBox;
        private IATConfigMainForm.OperationCompleteHandler OperationComplete;
        private IATConfigMainForm.OperationFailedHandler OperationFailed;
        private IATConfigMainForm.ProgressIncrementHandler ProgressIncrement;
        private IATConfigMainForm.ResetProgressHandler ResetProgress;
        private IATConfigMainForm.SetStatusMessageHandler SetProgressMessage;
        private Action<Func<int, bool, bool>, IATConfigMainForm.EProgressBarUses> BeginProgressBarUse;
        private IATConfigMainForm.EndProgressBarUseHandler EndProgressBarUse;
        private IATConfigMainForm.DisplayMessageBoxHandler DisplayMessageBox;
        private IATConfigMainForm.DisplayDataPasswordHandler OnDisplayDataPassword;
        private IATConfigMainForm.SetProgressValueHandler OnSetProgressValue;
        private IATConfigMainForm.ShowFormHandler ShowForm;
        private MemoryStream ConfigFileXML;
        private IATConfigFileNamespace.ConfigFile ConfigFile;
        private List<MemoryStream> Surveys = new List<MemoryStream>(), SASurveys = new List<MemoryStream>();
        private MemoryStream UniqueIDResponses = null;
        private List<String> SurveyNames = new List<String>();
        private bool TransmissionInProgress = false;
        private String _IATName, DataPassword, AdminPassword;
        private int ClientID = -1;
        private CIATSummary IATSummary;
        private ManualResetEvent _UploadComplete = new ManualResetEvent(false);

        public String IATName
        {
            get
            {
                return _IATName;
            }
       }

        public ManualResetEvent UploadComplete
        {
            get
            {
                return _UploadComplete;
            }
        }

        private class IATUploadException : Exception
        {
            private String _Message;
            private String _Caption;

            public String Message
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

            public IATUploadException(String caption, String message)
                : base(message)
            {
                _Message = message;
                _Caption = caption;
            }
        }

        private enum ERegisterIATResult
        {
            Success, IATExists, InsufficientDiskSpace, InsufficientIATs, ClientNotRegistered, DatabaseError
        };

        public enum EDeployResult
        {
            incomplete, successful, cannotBackup, incompatibleResultDescriptors, genericError, deploymentTimerExpired, fileDeploymentError, transformError, databaseError, backupLost
        };

        public bool UploadSuccessful
        {
            get
            {
                return bUploadSuccessful;
            }
        }

        private void CallSOAP(MySOAP.ESoapAction action, INamedXmlSerializable input, INamedXmlSerializable output)
        {
            lock (lockObject)
            {
                if (Aborted)
                    throw new TransactionAbortedException();
                MySOAP.CallSOAP(Properties.Resources.sDataProviderServlet, Convert.ToInt32(Properties.Resources.sDataProviderPort), action, input, output);
            }
        }

        private void IncrementProgress(int n)
        {
            lock (lockObject)
            {
                if (!Aborted)
                    MainForm.Invoke(ProgressIncrement, n);
                else
                    throw new TransactionAbortedException();
            }
        }

        private void SetProgressValue(int n)
        {
            lock (lockObject)
            {
                if (!Aborted)
                    MainForm.Invoke(OnSetProgressValue, n);
                else
                    throw new TransactionAbortedException();
            }
        }

        private void SetStatusMessage(String msg)
        {
            lock (lockObject)
            {
                if (!Aborted)
                    MainForm.Invoke(SetProgressMessage, msg);
                else
                    throw new TransactionAbortedException();
            }
        }

        private void ResetProgressBar()
        {
            lock (lockObject)
            {
                if (!Aborted)
                {
                    MainForm.Invoke(ResetProgress);
                    return;
                }
                throw new TransactionAbortedException();
            }
        }

        private void SetProgressBarRange(int min, int max)
        {
            lock (lockObject)
            {
                if (!Aborted)
                {
                    MainForm.Invoke(SetProgressRange, min, max);
                    return;
                }
                throw new TransactionAbortedException();
            }
        }

        private DialogResult OnDisplayYesNoMessageBox(String message, String caption)
        {
            lock (lockObject)
            {
                if (!Aborted)
                {
                    return (DialogResult)MainForm.Invoke(DisplayYesNoMessageBox, message, caption);
                }
                throw new TransactionAbortedException();
            }
        }

        public bool TryLock(int millis)
        {
            return Monitor.TryEnter(lockObject, millis);
        }

        public void Unlock()
        {
            Monitor.Exit(lockObject);
        }

        private void OnDisplayMessageBox(String message, String caption)
        {
            lock (lockObject)
            {
                if (!Aborted)
                {
                    MainForm.Invoke(DisplayMessageBox, message, caption);
                }
            }
        }

        private void OnOperationFailed(String msg, String caption)
        {
            lock (lockObject)
            {
                if (!Aborted)
                {
                    MainForm.Invoke(OperationFailed, msg, caption);
                    return;
                }
                throw new TransactionAbortedException();
            }
        }

        private void OnEndProgressBarUse()
        {
            lock (lockObject)
            {
                if (!Aborted)
                {
                    MainForm.Invoke(EndProgressBarUse);
                    return;
                }
            }
        }

        private DataPasswordForm.EDataPassword DisplayDataPassword(DataPasswordForm dpf)
        {
            lock (lockObject)
            {
                if (!Aborted)
                    return (DataPasswordForm.EDataPassword)MainForm.Invoke(OnDisplayDataPassword, dpf);
                throw new TransactionAbortedException();
            }
        }

        private void OnOperationComplete(CIATSummary summary)
        {
            lock (lockObject)
            {
                if (!Aborted)
                    MainForm.Invoke(OperationComplete, summary);
                else
                {
                    TransactionRequest outTrans = new TransactionRequest();
                    String serverURL = Properties.Resources.sDataProviderServlet;
                    MySOAP.EstablishEncryption(serverURL);
                    MySOAP.ShakeHands(serverURL, IATName);
                    MySOAP.VerifyPassword(IATName, CPartiallyEncryptedRSAKey.EKeyType.Admin, AdminPassword);
                    TransactionRequest trans = new TransactionRequest();
                    trans.Transaction = TransactionRequest.ETransaction.DeleteIAT;
                    trans.IATName = IATName;
                    trans.IsLastTransaction = true;
                    CallSOAP(MySOAP.ESoapAction.DeleteIAT, trans, outTrans);
                }
            }
        }

        public bool OnCancel(int timeout, bool forceAbort)
        {
            if ((timeout == -1) || (forceAbort == true))
                Monitor.Enter(lockObject);
            else if (!Monitor.TryEnter(lockObject, timeout))
                return false;
            SetAbortFlag();
            MySOAP.AbortCurrentTransaction();
            MySOAP.TerminateConnection(Properties.Resources.sDataProviderServlet);
            MySOAP.TerminateTransaction("User Interruption");
            Monitor.Exit(lockObject);
            return true;
        }

        public bool Aborted
        {
            get
            {
                lock (lockObject)
                {
                    return AbortFlag;
                }
            }
        }

        public void SetAbortFlag()
        {
            lock (lockObject)
            {
                AbortFlag = true;
            }
        }

        public class IATImage
        {
            public String Filename;
            public int FileSize;
            public byte[] ImageData;
        }

        public COneStageUploader(CIAT iat, IATConfigMainForm mainWin)
        {
            AbortFlag = false;
            IAT = iat;
            Transmission = new CPacketTransmission(this);
            SetProgressRange += new IATConfigMainForm.SetProgressRangeHandler(mainWin.SetProgressRange);
            OperationFailed += new IATConfigMainForm.OperationFailedHandler(mainWin.OperationFailed);
            OperationComplete += new IATConfigMainForm.OperationCompleteHandler(mainWin.OperationComplete);
            ProgressIncrement += new IATConfigMainForm.ProgressIncrementHandler(mainWin.ProgressIncrement);
            ResetProgress += new IATConfigMainForm.ResetProgressHandler(mainWin.ResetProgress);
            SetProgressMessage += new IATConfigMainForm.SetStatusMessageHandler(mainWin.SetStatusMessage);
            DisplayYesNoMessageBox += new IATConfigMainForm.DisplayYesNoMessageBoxHandler(mainWin.OnDisplayYesNoMessageBox);
            BeginProgressBarUse += new Action<Func<int, bool, bool>, IATConfigMainForm.EProgressBarUses>(mainWin.BeginProgressBarUse);
            EndProgressBarUse += new IATConfigMainForm.EndProgressBarUseHandler(mainWin.EndProgressBarUse);
            OnSetProgressValue += new IATConfigMainForm.SetProgressValueHandler(mainWin.SetProgressValue);
            DisplayMessageBox += new IATConfigMainForm.DisplayMessageBoxHandler(mainWin.OnDisplayMessageBox);
            ShowForm += new IATConfigMainForm.ShowFormHandler(mainWin.ShowForm);
            MainForm = mainWin;
        }
        /*
        private int FindIdenticalDisplayItem(CDisplayItem di, Point ptOrigin)
        {
            for (int ctr = 0; ctr < IATDisplayItems.Count; ctr++)
                if (IATDisplayItems[ctr].IsIdenticalTo(di, ptOrigin))
                    return ctr;
            return -1;
        }


        /// <summary>
        /// Saves the passed bitmap as an item slide image in the "ItemSlide" directory of the packaged test
        /// </summary>
        /// <param name="ItemSlide">the bitmap to save</param>
        /// <param name="ItemNum">the item number the bitmap represents</param>
        private void SaveItemSlide(Image ItemSlide, int ItemNum, String arg)
        {
            IATImage iatImage = new IATImage();
            iatImage.Filename = String.Format("Item_{0:D4}.jpg", ItemNum);
            MemoryStream memStream = new MemoryStream();
            ItemSlide.Save(memStream, System.Drawing.Imaging.ImageFormat.Jpeg);
            iatImage.FileSize = (int)memStream.Length;
            iatImage.ImageData = new byte[iatImage.FileSize];
            Array.Copy(memStream.ToArray(), iatImage.ImageData, iatImage.FileSize);
            memStream.Dispose();
            ItemSlides.Add(iatImage);
            nItemSlides++;
        }

        private int ProcessIATItem(IATClient.CIATItem item, int blockNum)
        {
            IATDisplayItem IATDisplayItem;
            IATItem PackagedIATItem;
            int stimulusID;

            // grab the stimulus
            CDisplayItem DisplayItem = item.Stimulus;
            Point ptOrigin = CIAT.Layout.StimulusRectangle.Location;
            switch (DisplayItem.Type)
            {
                case CDisplayItem.EType.text:
                    ptOrigin.X += (CIAT.Layout.StimulusRectangle.Width - DisplayItem.ItemSize.Width) >> 1;
                    ptOrigin.Y += CIAT.Layout.TextStimulusPaddingTop;
                    stimulusID = FindIdenticalDisplayItem(DisplayItem, ptOrigin);
                    if (stimulusID == -1)
                    {
                        stimulusID = IATDisplayItems.Count;
                        IATDisplayItem = new IATDisplayItem(stimulusID, DisplayItem, ptOrigin, MainForm.IATName);
                        IATDisplayItems.Add(IATDisplayItem);
                    }
                    break;

                case CDisplayItem.EType.stimulusImage:
                    CStimulusImageItem idi = (CStimulusImageItem)DisplayItem;
                    Size idiSize = idi.IATImage.ImageSize;
                    ptOrigin.Y += (CIAT.Layout.StimulusRectangle.Height - idiSize.Height) >> 1;
                    ptOrigin.X += (CIAT.Layout.StimulusRectangle.Width - idiSize.Width) >> 1;
                    stimulusID = FindIdenticalDisplayItem(DisplayItem, ptOrigin);
                    if (stimulusID == -1)
                    {
                        stimulusID = IATDisplayItems.Count;
                        IATDisplayItem = new IATDisplayItem(stimulusID, DisplayItem, ptOrigin, MainForm.IATName);
                        IATDisplayItems.Add(IATDisplayItem);
                    }
                    break;

                default:
                    throw new Exception("Invalid Display Item type employed as stimulus.");
            }

            // add the IATItem to the event list
            IATItem.EKeyedDir keyedDir = IATItem.EKeyedDir.Left;
            switch (item.KeyedDir)
            {
                case CIATItem.EKeyedDir.Right:
                    keyedDir = IATItem.EKeyedDir.Right;
                    break;

                case CIATItem.EKeyedDir.Left:
                    keyedDir = IATItem.EKeyedDir.Left;
                    break;

                case CIATItem.EKeyedDir.DynamicRight:
                    keyedDir = IATItem.EKeyedDir.DynamicRight;
                    break;

                case CIATItem.EKeyedDir.DynamicLeft:
                    keyedDir = IATItem.EKeyedDir.DynamicLeft;
                    break;
            }
            PackagedIATItem = new IATItem(stimulusID, keyedDir, item.KeySpecifierID, blockNum, item.SpecifierArg, item.OriginatingBlock);
            IATEvents.Add(PackagedIATItem);
            return PackagedIATItem.ItemNum;
        }

        private bool ProcessIATBlock(CIATBlock Block, bool IsPracticeBlock, int blockNum)
        {
            IATDisplayItem IATDisplayItem;
            CDisplayItem DisplayItem;
            IATEvent IATEvent;
            Point ptOrigin = new Point();
            int leftKeyID, rightKeyID, instructionsID;

            // grab the left response key value
            DisplayItem = Block.Key.LeftValue;
            ptOrigin.X = CIAT.Layout.LeftKeyValueRectangle.Left +
                ((CIAT.Layout.KeyValueSize.Width - DisplayItem.ItemSize.Width) >> 1);
            ptOrigin.Y = CIAT.Layout.LeftKeyValueRectangle.Top +
                ((CIAT.Layout.KeyValueSize.Height - DisplayItem.ItemSize.Height) >> 1);
            leftKeyID = IATDisplayItems.Count;
            IATDisplayItem = new IATDisplayItem(leftKeyID, DisplayItem, ptOrigin, MainForm.IATName);
            IATDisplayItems.Add(IATDisplayItem);

            // grab the right response key value
            DisplayItem = Block.Key.RightValue;
            ptOrigin.X = CIAT.Layout.RightKeyValueRectangle.Left +
                ((CIAT.Layout.KeyValueSize.Width - DisplayItem.ItemSize.Width) >> 1);
            ptOrigin.Y = CIAT.Layout.RightKeyValueRectangle.Top +
                ((CIAT.Layout.KeyValueSize.Height - DisplayItem.ItemSize.Height) >> 1);
            rightKeyID = IATDisplayItems.Count;
            IATDisplayItem = new IATDisplayItem(rightKeyID, DisplayItem, ptOrigin, MainForm.IATName);
            IATDisplayItems.Add(IATDisplayItem);

            // grab the instructions display item
            DisplayItem = Block.Instructions;
            ptOrigin = CIAT.Layout.InstructionsRectangle.Location;
            instructionsID = IATDisplayItems.Count;
            IATDisplayItem = new IATDisplayItem(instructionsID, DisplayItem, ptOrigin, MainForm.IATName);
            IATDisplayItems.Add(IATDisplayItem);

            // generate the start block event
            IATEvent = new BeginIATBlock(Block.GetIndex(Block), instructionsID, leftKeyID, rightKeyID);
            ((BeginIATBlock)IATEvent).NumPresentations = Block.NumPresentations;
            ((BeginIATBlock)IATEvent).NumItems = Block.NumItems;
            if (Block.AlternateBlock == null)
                ((BeginIATBlock)IATEvent).AlternatedWith = -1;
            else
                ((BeginIATBlock)IATEvent).AlternatedWith = Block.GetIndex(Block.AlternateBlock);
            IATEvents.Add(IATEvent);

            // process the items
            for (int ctr = 0; ctr < Block.NumItems; ctr++)
            {
                IncrementProgress(1);
                int ItemNum = ProcessIATItem(Block[ctr], blockNum);
                Image img = Block[ctr].GeneratePreviewImage();
                SaveItemSlide(img, ItemNum, String.Empty);
                img.Dispose();
            }

            // generate the end block event
            IATEvent = new EndIATBlock();
            IATEvents.Add(IATEvent);
            return true;
        }


        private bool Package()
        {
            try
            {
                // process the iat
                ResetProgressBar();
                SetProgressBarRange(0, IAT.GetNumItems() + 3);
                SetStatusMessage("Processing IAT Blocks");
                int nBlock = 1;
                bool bResult = false;
                for (int ctr = 0; ctr < IAT.Contents.Count; ctr++)
                {
                    switch (IAT.Contents[ctr].Type)
                    {
                        case ContentsItemType.IATBlock:
                            bResult = ProcessIATBlock((CIATBlock)IAT.Contents[ctr], false, nBlock++);
                            //                     if (RandomizationType == ConfigFile.ERandomizationType.SetNumberOfPresentations)
                            NumIATItems += ((CIATBlock)IAT.Contents[ctr]).NumPresentations;
                            //                   else
                            //                     nIATItems += ((CIATBlock)IAT.Contents[ctr]).NumItems;
                            break;

                        case ContentsItemType.InstructionBlock:
                            bResult = ProcessInstructionBlock((CInstructionBlock)IAT.Contents[ctr]);
                            break;

                        case ContentsItemType.BeforeSurvey:
                            bResult = true;
                            break;

                        case ContentsItemType.AfterSurvey:
                            bResult = true;
                            break;
                    }
                    if (!bResult)
                        return false;
                }
                IncrementProgress(1);
                // generate the error mark
                ErrorMarkID = IATDisplayItems.Count;
                CIAT.Layout.ErrorMarkIATImage.Lock();
                CMemoryImageDisplayItem mdi = new CMemoryImageDisplayItem(CIAT.Layout.ErrorMarkIATImage.ImageSize.Width, CIAT.Layout.ErrorMarkIATImage.ImageSize.Height, false,
                    CIAT.Layout.ErrorMarkIATImage.theImage);
                CIAT.Layout.ErrorMarkIATImage.Unlock();
                IATDisplayItem pdi = new IATDisplayItem(ErrorMarkID, mdi, CIAT.Layout.ErrorRectangle.Location, MainForm.IATName);
                IATDisplayItems.Add(pdi);
                IATItem.ResetItemCounter();
                IncrementProgress(1);

                // generate key outline boxes
                Graphics g;
                Point ptOrigin;
                Brush outlineBrush = new SolidBrush(CIAT.Layout.OutlineColor);
                Brush backBrush = new SolidBrush(CIAT.Layout.BackColor);
                Pen outlinePen = new Pen(outlineBrush, CIAT.Layout.ResponseValueRectMargin >> 1);

                LeftKeyOutlineID = IATDisplayItems.Count;
                CMemoryImageDisplayItem LeftKeyOutline = new CMemoryImageDisplayItem(CIAT.Layout.KeyValueSize.Width + (CIAT.Layout.ResponseValueRectMargin << 1),
                    CIAT.Layout.KeyValueSize.Height + (CIAT.Layout.ResponseValueRectMargin << 1), false);
                g = Graphics.FromImage(LeftKeyOutline.MemoryBmp);
                g.FillRectangle(backBrush, new Rectangle(0, 0, LeftKeyOutline.ItemSize.Width, LeftKeyOutline.ItemSize.Height));
                g.DrawRectangle(outlinePen, new Rectangle(CIAT.Layout.ResponseValueRectMargin >> 1, CIAT.Layout.ResponseValueRectMargin >> 1, CIAT.Layout.KeyValueSize.Width +
                    CIAT.Layout.ResponseValueRectMargin, CIAT.Layout.KeyValueSize.Height + CIAT.Layout.ResponseValueRectMargin));
                g.Dispose();
                ptOrigin = CIAT.Layout.LeftKeyValueRectangle.Location - new Size(CIAT.Layout.ResponseValueRectMargin, CIAT.Layout.ResponseValueRectMargin) +
                    new Size(CIAT.Layout.BorderWidth, CIAT.Layout.BorderWidth);
                pdi = new IATDisplayItem(LeftKeyOutlineID, LeftKeyOutline, ptOrigin, MainForm.IATName);
                IATDisplayItems.Add(pdi);
                IncrementProgress(1);

                RightKeyOutlineID = IATDisplayItems.Count;
                CMemoryImageDisplayItem RightKeyOutline = new CMemoryImageDisplayItem(CIAT.Layout.KeyValueSize.Width + (CIAT.Layout.ResponseValueRectMargin << 1),
                    CIAT.Layout.KeyValueSize.Height + (CIAT.Layout.ResponseValueRectMargin << 1), false);
                g = Graphics.FromImage(RightKeyOutline.MemoryBmp);
                g.FillRectangle(backBrush, new Rectangle(0, 0, LeftKeyOutline.ItemSize.Width, LeftKeyOutline.ItemSize.Height));
                g.DrawRectangle(outlinePen, new Rectangle(CIAT.Layout.ResponseValueRectMargin >> 1, CIAT.Layout.ResponseValueRectMargin >> 1, CIAT.Layout.KeyValueSize.Width +
                    CIAT.Layout.ResponseValueRectMargin, CIAT.Layout.KeyValueSize.Height + CIAT.Layout.ResponseValueRectMargin));
                g.Dispose();
                ptOrigin = CIAT.Layout.RightKeyValueRectangle.Location - new Size(CIAT.Layout.ResponseValueRectMargin, CIAT.Layout.ResponseValueRectMargin) +
                    new Size(CIAT.Layout.BorderWidth, CIAT.Layout.BorderWidth);
                pdi = new IATDisplayItem(RightKeyOutlineID, RightKeyOutline, ptOrigin, MainForm.IATName);
                IATDisplayItems.Add(pdi);
                ResetProgressBar();
                SetStatusMessage("Generating Stimuli");
                SetProgressBarRange(0, IATDisplayItems.Count);
                for (int ctr = 0; ctr < IATDisplayItems.Count; ctr++)
                {
                    IATImage iatImg = new IATImage();
                    IATDisplayItems[ctr].Save(IATName, iatImg);
                    IATDisplayItems[ctr].Filename = iatImg.Filename;
                    DisplayItemImages.Add(iatImg);
                    IncrementProgress(1);
                }
                return true;
            }
            catch (TransactionAbortedException)
            {
                return false;
            }
        }*/
/*
private void AddUniqueIDResponse(int surveyNum, int itemNum, CFixedDigResponseObject responseObject)
{
    bool bFirst = (UniqueIDResponses == null);
    if (bFirst)
        UniqueIDResponses = new MemoryStream();
    BinaryWriter bWriter = new BinaryWriter(UniqueIDResponses);
    bWriter.Write(surveyNum);
    bWriter.Write(itemNum);
    bWriter.Write(responseObject.NumValues);
    for (int ctr = 0; ctr < responseObject.NumValues; ctr++)
        bWriter.Write(responseObject[ctr]);            
    bWriter.Flush();
    NumUniqueIDResponses++;
}
*/
/*
private void ProcessUniqueResponses(CPartiallyEncryptedRSAKey dataKey)
{
    RSACryptoServiceProvider crypt = new RSACryptoServiceProvider();
    crypt.ImportParameters(dataKey.GetRSAParameters());
    MemoryStream memStream = new MemoryStream(UniqueIDResponses.ToArray());
    UniqueIDResponses.Dispose();
    UniqueIDResponses = new MemoryStream();
    XmlWriter xWriter = new XmlTextWriter(UniqueIDResponses, Encoding.UTF8);
    BinaryReader bReader = new BinaryReader(memStream);
    memStream.Seek(0, SeekOrigin.Begin);
    xWriter.WriteStartDocument();
    xWriter.WriteStartElement("UniqueResponseList");
    xWriter.WriteStartElement("NumResponses", NumUniqueIDResponses.ToString());
    for (int ctr = 0; ctr < NumUniqueIDResponses; ctr++)
    {
        xWriter.WriteStartElement("UniqueResponse");
        xWriter.WriteElementString("SurveyNum", bReader.ReadInt32().ToString());
        xWriter.WriteElementString("ItemNum", bReader.ReadInt32().ToString());
        xWriter.WriteStartElement("UniqueResponseValues");
        int nElems = bReader.ReadInt32();
        xWriter.WriteAttributeString("NumElements", nElems.ToString());
        for (int ctr2 = 0; ctr2 < nElems; ctr2++)
        {
            String val = bReader.ReadString();
            byte[] valBytes = System.Text.Encoding.UTF8.GetBytes(val);
            byte[] encValBytes = crypt.Encrypt(valBytes, false);
            xWriter.WriteElementString("ResponseValue", Convert.ToBase64CharArray(encValBytes));
        }
        xWriter.WriteEndElement();
        xWriter.WriteEndElement();
    }
    xWriter.WriteEndElement();
    xWriter.WriteEndDocument();
    UniqueIDResponses.Seek(0, SeekOrigin.Begin);
    memStream.Dispose();
}
*//*
private void ProcessSurveys(String URL, int port)
{
    int UnnamedSurveyCtr = 0;
    for (int ctr1 = 0; ctr1 < IAT.BeforeSurvey.Count; ctr1++)
    {
        // store the schema-less XML used by the XSLT code
        MemoryStream beforeSurveyStream = new MemoryStream();
        XmlTextWriter xmlWriter = new XmlTextWriter(beforeSurveyStream, Encoding.Unicode);
        xmlWriter.WriteStartDocument();
        xmlWriter.WriteStartElement("Survey");
        xmlWriter.WriteAttributeString("IAT", IATName);
        xmlWriter.WriteAttributeString("Type", "Before");
        xmlWriter.WriteAttributeString("FileName", IAT.BeforeSurvey[ctr1].FileNameBase);
        xmlWriter.WriteAttributeString("SurveyName", IAT.BeforeSurvey[ctr1].Name);
        xmlWriter.WriteAttributeString("TimeoutMillis", (IAT.BeforeSurvey[ctr1].Timeout * 60000).ToString());
        xmlWriter.WriteAttributeString("ServerURL", URL);
        xmlWriter.WriteAttributeString("ServerPort", port.ToString());
        xmlWriter.WriteAttributeString("ClientID", ClientID.ToString());
            if (IAT.UniqueResponse.SurveyName == IAT.BeforeSurvey[ctr1].Name)
                xmlWriter.WriteAttributeString("UniqueResponseItem", IAT.UniqueResponse.ItemNum.ToString());
            else
                xmlWriter.WriteAttributeString("UniqueResponseItem", "-1");
        for (int ctr2 = 0; ctr2 < IAT.BeforeSurvey[ctr1].Items.Count; ctr2++)
            IAT.BeforeSurvey[ctr1].Items[ctr2].WriteToXml(xmlWriter);
        xmlWriter.WriteEndElement();
        xmlWriter.WriteEndDocument();
        xmlWriter.Flush();
        Surveys.Add(beforeSurveyStream);

        // store the schema-ed XML used for result file processing
        beforeSurveyStream = new MemoryStream();
        Survey s = new Survey(IAT.BeforeSurvey[ctr1].Name);
        s.Timeout = (int)(IAT.BeforeSurvey[ctr1].Timeout * 60000);
        if (IAT.BeforeSurvey[ctr1].Items[0].IsCaption) {
            s.SetCaption(IAT.BeforeSurvey[ctr1].Items[0]);
            s.HasCaption = true;
            CSurveyItem []surveyItems = new CSurveyItem[IAT.BeforeSurvey[ctr1].Items.Count - 1];
            int itemCtr = 0;
            for (int ctr2 = 1; ctr2 < IAT.BeforeSurvey[ctr1].Items.Count; ctr2++) {
               if (IAT.BeforeSurvey[ctr1].Items[ctr2].Response.ResponseType != CResponse.EResponseType.Instruction)
                   itemCtr++;
                surveyItems[ctr2 - 1] = IAT.BeforeSurvey[ctr1].Items[ctr2];
            }
            s.SetItems(surveyItems);
            s.NumItems = itemCtr;
        } else {
            s.HasCaption = false;
            CSurveyItem []surveyItems = new CSurveyItem[IAT.BeforeSurvey[ctr1].Items.Count];
            int itemCtr = 0;
            for (int ctr2 = 0; ctr2 < IAT.BeforeSurvey[ctr1].Items.Count; ctr2++) {
                if (IAT.BeforeSurvey[ctr1].Items[ctr2].Response.ResponseType != CResponse.EResponseType.Instruction)
                    itemCtr++;
                surveyItems[ctr2] = IAT.BeforeSurvey[ctr1].Items[ctr2];
            }
            s.SetItems(surveyItems);
            s.NumItems = itemCtr;
        }
        XmlSerializer ser = new XmlSerializer(typeof(Survey));
        ser.Serialize(beforeSurveyStream, s);
        SASurveys.Add(beforeSurveyStream);
    }
    for (int ctr1 = 0; ctr1 < IAT.AfterSurvey.Count; ctr1++)
    {
        // store the schema-less XML used by the XSLT code
        MemoryStream afterSurveyStream = new MemoryStream();
        XmlTextWriter xmlWriter = new XmlTextWriter(afterSurveyStream, Encoding.Unicode);
        xmlWriter.WriteStartDocument();
        xmlWriter.WriteStartElement("Survey");
        xmlWriter.WriteAttributeString("IAT", IATName);
        xmlWriter.WriteAttributeString("Type", "After");
        xmlWriter.WriteAttributeString("FileName", IAT.AfterSurvey[ctr1].FileNameBase);
        xmlWriter.WriteAttributeString("SurveyName", IAT.AfterSurvey[ctr1].Name);
        xmlWriter.WriteAttributeString("TimeoutMillis", (IAT.AfterSurvey[ctr1].Timeout * 60000).ToString());
        xmlWriter.WriteAttributeString("ServerURL", URL);
        xmlWriter.WriteAttributeString("ServerPort", port.ToString());
        xmlWriter.WriteAttributeString("ClientID", ClientID.ToString());
        if (IAT.UniqueResponse.SurveyName == IAT.AfterSurvey[ctr1].Name)
            xmlWriter.WriteAttributeString("UniqueResponseItem", IAT.UniqueResponse.ItemNum.ToString());
        else
            xmlWriter.WriteAttributeString("UniqueResponseItem", "-1");
        for (int ctr2 = 0; ctr2 < IAT.AfterSurvey[ctr1].Items.Count; ctr2++)
            IAT.AfterSurvey[ctr1].Items[ctr2].WriteToXml(xmlWriter);
        xmlWriter.WriteEndElement();
        xmlWriter.WriteEndDocument();
        xmlWriter.Flush();
        Surveys.Add(afterSurveyStream);

        // store the schema-ed XML used for result file processing
        afterSurveyStream = new MemoryStream();
        Survey s = new Survey(IAT.AfterSurvey[ctr1].Name);
        s.Timeout = (int)(IAT.AfterSurvey[ctr1].Timeout * 60000);
        if (IAT.AfterSurvey[ctr1].Items[0].IsCaption)
        {
            s.SetCaption(IAT.AfterSurvey[ctr1].Items[0]);
            s.HasCaption = true;
            CSurveyItem[] surveyItems = new CSurveyItem[IAT.AfterSurvey[ctr1].Items.Count - 1];
            int itemCtr = 0;
            for (int ctr2 = 1; ctr2 < IAT.AfterSurvey[ctr1].Items.Count; ctr2++)
            {
                if (IAT.AfterSurvey[ctr1].Items[ctr2].Response.ResponseType != CResponse.EResponseType.Instruction)
                    itemCtr++;
                surveyItems[ctr2 - 1] = IAT.AfterSurvey[ctr1].Items[ctr2];
            }
            s.SetItems(surveyItems);
            s.NumItems = itemCtr;
        }
        else
        {
            s.HasCaption = false;
            CSurveyItem[] surveyItems = new CSurveyItem[IAT.AfterSurvey[ctr1].Items.Count];
            int itemCtr = 0;
            for (int ctr2 = 0; ctr2 < IAT.AfterSurvey[ctr1].Items.Count; ctr2++)
            {
                if (IAT.AfterSurvey[ctr1].Items[ctr2].Response.ResponseType != CResponse.EResponseType.Instruction)
                    itemCtr++;
                surveyItems[ctr2] = IAT.AfterSurvey[ctr1].Items[ctr2];
            }
            s.SetItems(surveyItems);
            s.NumItems = itemCtr;
        }
        XmlSerializer ser = new XmlSerializer(typeof(Survey));
        ser.Serialize(afterSurveyStream, s);
        SASurveys.Add(afterSurveyStream);
    }
}

/*
private void ProcessTextInstructionScreen(IATClient.CTextInstructionScreen screen)
{
    // grab the instructions image
    Point ptOrigin = CIAT.Layout.InstructionScreenTextAreaRectangle.Location;
    int instructionsID = IATDisplayItems.Count;
    IATDisplayItem pdi = new IATDisplayItem(instructionsID, screen.Instructions, ptOrigin, MainForm.IATName);
    IATDisplayItems.Add(pdi);

    // grab the continue instructions image
    CDisplayItem di = screen.ContinueInstructions;
    ptOrigin = CIAT.Layout.ContinueInstructionsRectangle.Location;
    ptOrigin.X += (CIAT.Layout.ContinueInstructionsRectangle.Width - di.ItemSize.Width) >> 1;
    ptOrigin.Y += CIAT.Layout.ContinueInstructionsRectangle.Height - di.ItemSize.Height;
    int continueInstructionsID = IATDisplayItems.Count;
    pdi = new IATDisplayItem(continueInstructionsID, di, ptOrigin, MainForm.IATName);
    IATDisplayItems.Add(pdi);

    // add the instruction screen event
    TextInstructionScreen InstrScr = new TextInstructionScreen(screen.ContinueKeyChar, continueInstructionsID, instructionsID);
    IATEvents.Add(InstrScr);
}

private void ProcessMockItemInstructionScreen(IATClient.CMockItemScreen screen)
{
    IATDisplayItem IATDisplayItem;
    CDisplayItem DisplayItem;
    Point ptOrigin = new Point();
    int leftKeyID, rightKeyID, instructionsID;
    bool outlineLeftResponse, outlineRightResponse;

    // grab the left response key value
    DisplayItem = CIATKey.KeyDictionary[screen.ResponseKeyName].LeftValue;
    ptOrigin.X = CIAT.Layout.LeftKeyValueRectangle.Left +
        ((CIAT.Layout.KeyValueSize.Width - DisplayItem.ItemSize.Width) >> 1);
    ptOrigin.Y = CIAT.Layout.LeftKeyValueRectangle.Top +
        ((CIAT.Layout.KeyValueSize.Height - DisplayItem.ItemSize.Height) >> 1);
    leftKeyID = IATDisplayItems.Count;
    IATDisplayItem = new IATDisplayItem(leftKeyID, DisplayItem, ptOrigin, MainForm.IATName);
    IATDisplayItems.Add(IATDisplayItem);

    // grab the right response key value
    DisplayItem = CIATKey.KeyDictionary[screen.ResponseKeyName].RightValue;
    ptOrigin.X = CIAT.Layout.RightKeyValueRectangle.Left +
        ((CIAT.Layout.KeyValueSize.Width - DisplayItem.ItemSize.Width) >> 1);
    ptOrigin.Y = CIAT.Layout.RightKeyValueRectangle.Top +
        ((CIAT.Layout.KeyValueSize.Height - DisplayItem.ItemSize.Height) >> 1);
    rightKeyID = IATDisplayItems.Count;
    IATDisplayItem = new IATDisplayItem(rightKeyID, DisplayItem, ptOrigin, MainForm.IATName);
    IATDisplayItems.Add(IATDisplayItem);

    // grab the stimulus
    int stimulusID;
    DisplayItem = screen.MockItemStimulus;
    ptOrigin = CIAT.Layout.StimulusRectangle.Location;
    switch (DisplayItem.Type)
    {
        case CDisplayItem.EType.text:
            ptOrigin.X += (CIAT.Layout.StimulusRectangle.Width - DisplayItem.ItemSize.Width) >> 1;
            ptOrigin.Y += CIAT.Layout.TextStimulusPaddingTop;
            stimulusID = FindIdenticalDisplayItem(DisplayItem, ptOrigin);
            if (stimulusID == -1)
            {
                stimulusID = IATDisplayItems.Count;
                IATDisplayItem = new IATDisplayItem(stimulusID, DisplayItem, ptOrigin, MainForm.IATName);
                IATDisplayItems.Add(IATDisplayItem);
            }
            break;

        case CDisplayItem.EType.stimulusImage:
            CStimulusImageItem idi = (CStimulusImageItem)DisplayItem;
            Size idiSize = idi.ItemSize;
            ptOrigin.Y += (CIAT.Layout.StimulusRectangle.Height - idiSize.Height) >> 1;
            ptOrigin.X += (CIAT.Layout.StimulusRectangle.Width - idiSize.Width) >> 1;
            stimulusID = FindIdenticalDisplayItem(DisplayItem, ptOrigin);
            if (stimulusID == -1)
            {
                stimulusID = IATDisplayItems.Count;
                IATDisplayItem = new IATDisplayItem(stimulusID, DisplayItem, ptOrigin, MainForm.IATName);
                IATDisplayItems.Add(IATDisplayItem);
            }
            break;

        default:
            stimulusID = int.MinValue;
            break;
    }

    // grab the instructions display item
    DisplayItem = screen.BriefInstructions;
    ptOrigin = CIAT.Layout.MockItemInstructionsRectangle.Location;
    instructionsID = IATDisplayItems.Count;
    IATDisplayItem = new IATDisplayItem(instructionsID, DisplayItem, ptOrigin, MainForm.IATName);
    IATDisplayItems.Add(IATDisplayItem);

    // grab the continue instructions display item
    DisplayItem = screen.ContinueInstructions;
    ptOrigin = CIAT.Layout.ContinueInstructionsRectangle.Location;
    ptOrigin.X += (CIAT.Layout.ContinueInstructionsRectangle.Width - DisplayItem.ItemSize.Width) >> 1;
    ptOrigin.Y += CIAT.Layout.ContinueInstructionsRectangle.Height - DisplayItem.ItemSize.Height;
    int continueInstructionsID = IATDisplayItems.Count;
    IATDisplayItem = new IATDisplayItem(continueInstructionsID, DisplayItem, ptOrigin, MainForm.IATName);
    IATDisplayItems.Add(IATDisplayItem);

    // determine if responses are to be outlined
    outlineLeftResponse = outlineRightResponse = false;
    if (screen.KeyedDirOutlined)
    {
        if (screen.MockItemKeyedDir == IATClient.CIATItem.EKeyedDir.Left)
            outlineLeftResponse = true;
        else if (screen.MockItemKeyedDir == IATClient.CIATItem.EKeyedDir.Right)
            outlineRightResponse = true;
    }

    // create the mock item screeen event
    MockItemInstructionScreen InstrScr = new MockItemInstructionScreen(screen.ContinueKeyChar, continueInstructionsID, leftKeyID, rightKeyID, stimulusID,
        instructionsID, screen.InvalidResponseFlag, outlineLeftResponse, outlineRightResponse);
    IATEvents.Add(InstrScr);
}

private void ProcessKeyedInstructionScreen(IATClient.CKeyInstructionScreen screen)
{
    // grab the instructions image
    Point ptOrigin = CIAT.Layout.KeyInstructionScreenTextAreaRectangle.Location;
    int instructionsID = IATDisplayItems.Count;
    IATDisplayItem pdi = new IATDisplayItem(instructionsID, screen.Instructions, ptOrigin, MainForm.IATName);
    IATDisplayItems.Add(pdi);

    CDisplayItem DisplayItem;
    IATDisplayItem IATDisplayItem;
    int leftKeyID, rightKeyID;

    // grab the left response key value
    DisplayItem = CIATKey.KeyDictionary[screen.ResponseKeyName].LeftValue;
    ptOrigin.X = CIAT.Layout.LeftKeyValueRectangle.Left +
        ((CIAT.Layout.KeyValueSize.Width - DisplayItem.ItemSize.Width) >> 1);
    ptOrigin.Y = CIAT.Layout.LeftKeyValueRectangle.Top +
        ((CIAT.Layout.KeyValueSize.Height - DisplayItem.ItemSize.Height) >> 1);
    leftKeyID = IATDisplayItems.Count;
    IATDisplayItem = new IATDisplayItem(leftKeyID, DisplayItem, ptOrigin, MainForm.IATName);
    IATDisplayItems.Add(IATDisplayItem);

    // grab the right response key value
    DisplayItem = CIATKey.KeyDictionary[screen.ResponseKeyName].RightValue;
    ptOrigin.X = CIAT.Layout.RightKeyValueRectangle.Left +
        ((CIAT.Layout.KeyValueSize.Width - DisplayItem.ItemSize.Width) >> 1);
    ptOrigin.Y = CIAT.Layout.RightKeyValueRectangle.Top +
        ((CIAT.Layout.KeyValueSize.Height - DisplayItem.ItemSize.Height) >> 1);
    rightKeyID = IATDisplayItems.Count;
    IATDisplayItem = new IATDisplayItem(rightKeyID, DisplayItem, ptOrigin, MainForm.IATName);
    IATDisplayItems.Add(IATDisplayItem);

    // grab the continue instructions image
    CDisplayItem di = screen.ContinueInstructions;
    ptOrigin = CIAT.Layout.ContinueInstructionsRectangle.Location;
    ptOrigin.X += (CIAT.Layout.ContinueInstructionsRectangle.Width - di.ItemSize.Width) >> 1;
    ptOrigin.Y += CIAT.Layout.ContinueInstructionsRectangle.Height - di.ItemSize.Height;
    int continueInstructionsID = IATDisplayItems.Count;
    pdi = new IATDisplayItem(continueInstructionsID, di, ptOrigin, MainForm.IATName);
    IATDisplayItems.Add(pdi);

    // add the instruction screen event
    KeyedInstructionScreen InstrScr = new KeyedInstructionScreen(screen.ContinueKeyChar, continueInstructionsID, instructionsID, rightKeyID, leftKeyID);
    IATEvents.Add(InstrScr);
}


private bool ProcessInstructionBlock(IATClient.CInstructionBlock InstructionBlock)
{
    BeginInstructionBlock beginInstructions = new BeginInstructionBlock();
    if (InstructionBlock.AlternateInstructionBlock == null)
        beginInstructions.AlternatedWith = -1;
    else
        beginInstructions.AlternatedWith = InstructionBlock.GetBlockIndex(InstructionBlock.AlternateInstructionBlock);
    beginInstructions.NumInstructionScreens = InstructionBlock.NumScreens;
    IATEvents.Add(beginInstructions);
    for (int ctr = 0; ctr < InstructionBlock.NumScreens; ctr++)
    {
        IncrementProgress(1);
        if (InstructionBlock[ctr].Type == IATClient.CInstructionScreen.EType.MockItem)
            ProcessMockItemInstructionScreen((CMockItemScreen)InstructionBlock[ctr]);
        else if (InstructionBlock[ctr].Type == IATClient.CInstructionScreen.EType.Text)
            ProcessTextInstructionScreen((IATClient.CTextInstructionScreen)InstructionBlock[ctr]);
        else if (InstructionBlock[ctr].Type == IATClient.CInstructionScreen.EType.Key)
            ProcessKeyedInstructionScreen((IATClient.CKeyInstructionScreen)InstructionBlock[ctr]);
        else
            throw new Exception("Instruction screen of unspecified type encountered during IAT Packaging");
    }
    return true;
}

private void BuildConfigFile(String URL)
{
    ProcessSurveys(URL);

    // calc number of once-only responses
    IATConfigFileNamespace.IATSurvey[] BeforeSurveys = new IATConfigFileNamespace.IATSurvey[IAT.BeforeSurvey.Count];
    IATConfigFileNamespace.IATSurvey[] AfterSurveys = new IATConfigFileNamespace.IATSurvey[IAT.AfterSurvey.Count];

    // build the config file
    IATConfigFile = new ConfigFile(IAT);
    IATConfigFile.NumIATItems = NumIATItems;
    IATConfigFile.Is7Block = IAT.Is7Block;
    IATConfigFile.RedirectOnComplete = IAT.RedirectionURL;
    IATConfigFile.LeftResponseASCIIKeyCodeLower = Encoding.ASCII.GetBytes(IAT.LeftResponseChar.ToLower())[0];
    IATConfigFile.LeftResponseASCIIKeyCodeUpper = Encoding.ASCII.GetBytes(IAT.LeftResponseChar.ToUpper())[0];
    IATConfigFile.RightResponseASCIIKeyCodeLower = Encoding.ASCII.GetBytes(IAT.RightResponseChar.ToLower())[0];
    IATConfigFile.RightResponseASCIIKeyCodeUpper = Encoding.ASCII.GetBytes(IAT.RightResponseChar.ToUpper())[0];
    IATConfigFile.RandomizationType = IATClient.ConfigFile.ERandomizationType.SetNumberOfPresentations;
    IATConfigFile.ErrorMarkID = ErrorMarkID;
    IATConfigFile.LeftKeyOutlineID = LeftKeyOutlineID;
    IATConfigFile.RightKeyOutlineID = RightKeyOutlineID;
    IATConfigFile.EventList.IATEvents.AddRange(IATEvents);
    IATConfigFile.DisplayItems.DisplayItems.AddRange(IATDisplayItems);
    IATConfigFile.Name = IATName;
    IATConfigFile.ServerURL = URL;
    IATConfigFile.PrefixSelfAlternatingSurveys = AlternationGroup.PrefixSelfAlternatingSurveys;
    IATConfigFile.ClientID = ClientID;
    IATConfigFile.BeforeSurveys.AddRange(BeforeSurveys);
    IATConfigFile.AfterSurveys.AddRange(AfterSurveys);

    foreach (DynamicSpecifier ds in DynamicSpecifier.GetAllSpecifiers())
        IATConfigFile.DynamicSpecifiers.Add(ds.GetSerializableSpecifier());
    ConfigFile = new MemoryStream();
    XmlTextWriter xWriter = new XmlTextWriter(ConfigFile, System.Text.Encoding.Unicode);
    xWriter.WriteStartDocument();
    IATConfigFile.WriteXml(xWriter);
    xWriter.WriteEndDocument();
    xWriter.Flush();
    SAConfigFile = new MemoryStream();
    xWriter = new XmlTextWriter(SAConfigFile, System.Text.Encoding.Unicode);
    xWriter.Formatting = Formatting.Indented;
    xWriter.WriteStartDocument();
    IATConfigFile.WriteXml(xWriter);
    xWriter.WriteEndDocument();
    xWriter.Flush();
    IATSummary = new CIATSummary(IATConfigFile);
    IATSummary.Surveys.AddRange(SurveyNames);
}
*//*
protected Manifest GenerateFileManifest()
{
    Manifest manifest = new Manifest();
    ManifestFile configFile = new ManifestFile();
    ManifestFile uniqueResponses = null;
    configFile.Name = IATName + ".cf";
    configFile.Size = ConfigFileXML.Length;

    int nFiles = 2 + ConfigFile.DisplayItems.Count + (Surveys.Count * 2);
    if (UniqueIDResponses != null)
    {
        nFiles++;
        uniqueResponses = new ManifestFile();
        uniqueResponses.Name = "UniqueResponses.dat";
        uniqueResponses.Size = UniqueIDResponses.Length;
    }
    manifest.Contents.Add(configFile);
    if (uniqueResponses != null)
        manifest.Contents.Add(uniqueResponses);
    String surveyFNameBase;
    for (int ctr2 = 0; ctr2 < Surveys.Count; ctr2++)
    {
        ManifestFile SurveyFile = new ManifestFile();
        if (ctr2 < IAT.BeforeSurvey.Count)
            surveyFNameBase = IAT.BeforeSurvey[ctr2].FileNameBase;
        else
            surveyFNameBase = IAT.AfterSurvey[ctr2 - IAT.BeforeSurvey.Count].FileNameBase;
        SurveyFile.Name = String.Format("{0}.xml.survey", surveyFNameBase);
        SurveyFile.Size = Surveys[ctr2].Length;
        manifest.Contents.Add(SurveyFile);
        ManifestFile SurveyFileWithSchema = new ManifestFile();
        SurveyFileWithSchema.Name = String.Format("{0}WithSchema.xml", surveyFNameBase);
        SurveyFileWithSchema.Size = SASurveys[ctr2].Length;
        manifest.Contents.Add(SurveyFileWithSchema);
    }
    manifest.Contents.AddRange(ConfigFile.DisplayItemImages.ConstructFileManifest());
    ManifestDirectory ItemSlideDir = new ManifestDirectory();
    ItemSlideDir.Name = "ItemSlides";
    ItemSlideDir.Contents.Add(new ManifestFile("SlideManifest.xml", (int)ConfigFile.SlideManifestData.Length));
    ItemSlideDir.Contents.AddRange(ConfigFile.ItemSlides.ConstructFileManifest());
    manifest.Contents.Add(ItemSlideDir);
    manifest.IATName = IAT.Name;
    return manifest;
}

private CPacketTransmission.ETransmissionResult SendManifest()
{
    CPacketTransmission transmission = new CPacketTransmission(this);
    transmission.QueueFile(ConfigFileXML);
    if (UniqueIDResponses != null)
        transmission.QueueFile(UniqueIDResponses);
    for (int ctr = 0; ctr < Surveys.Count; ctr++)
    {
        transmission.QueueFile(Surveys[ctr]);
        transmission.QueueFile(SASurveys[ctr]);
    }
    byte[][] ImageData = ConfigFile.DisplayItemImages.GetImageData();
    for (int ctr = 0; ctr < ImageData.Length; ctr++)
        transmission.QueueFile(ImageData[ctr]);
    transmission.QueueFile(ConfigFile.SlideManifestData);
    ImageData = ConfigFile.ItemSlides.GetImageData();
    for (int ctr = 0; ctr < ImageData.Length; ctr++)
        transmission.QueueFile(ImageData[ctr]);
    transmission.BuildPacketList();
    TransactionEvent tEvent = MySOAP.CurrentTransactionEvent.AddChildEvent("Sending Packets");
    tEvent.MaxProgressValue = transmission.NumPackets;
    SetProgressBarRange(0, transmission.NumPackets);
    ResetProgressBar();
    return transmission.Transmit(MainForm, ProgressIncrement, tEvent, Properties.Resources.sDataProviderServlet);
}

private void UploadAbort(String errorMsg)
{
    OnOperationFailed("Upload Failed", errorMsg);
}

private bool DoUpload()
{
    try
    {
        try
        {
            SetStatusMessage("Processing IAT");
            ConfigFile = new IATConfigFileNamespace.ConfigFile(IAT);
            ConfigFile.ServerURL = ServerURL;
            ConfigFile.ServerPort = Convert.ToInt32(Properties.Resources.sDefaultIATServerPort);
            ResetProgressBar();
            TransactionEvent tEvent = MySOAP.BeginNewTransaction(TransactionProgress.ETransactionType.IAT, IATName);
            SetStatusMessage("Negotiating Transmission With Server");
            CPartiallyEncryptedRSAKey adminKey = new CPartiallyEncryptedRSAKey(CPartiallyEncryptedRSAKey.EKeyType.Admin);
            CPartiallyEncryptedRSAKey dataKey = new CPartiallyEncryptedRSAKey(CPartiallyEncryptedRSAKey.EKeyType.Data);
            TransactionRequest inTrans = null;
            lock (lockObject)
            {
                if (Aborted)
                    throw new TransactionAbortedException();
                MySOAP.EstablishEncryption(Properties.Resources.sDataProviderServlet);
                inTrans = MySOAP.ShakeHands(Properties.Resources.sDataProviderServlet, IATName);
            }
            ClientID = inTrans.ClientID;
            ConfigFile.ClientID = inTrans.ClientID;
            ConfigFileXML = new MemoryStream();
            XmlWriter xWriter = new XmlTextWriter(ConfigFileXML, System.Text.Encoding.UTF8);
            xWriter.WriteStartDocument();
            ConfigFile.WriteXml(xWriter);
            xWriter.WriteEndDocument();
            xWriter.Flush();
            ProcessSurveys(ServerURL, Convert.ToInt32(Properties.Resources.sDataProviderPort));
            if (inTrans.Transaction == TransactionRequest.ETransaction.ClientFrozen)
            {
                OnOperationFailed("Your account appears to have been frozen.  Please contact us at admin@iatsoftware.net for details.", "Account Frozen");
                return false;
            }
            else if (inTrans.Transaction == TransactionRequest.ETransaction.ClientDeleted)
            {
                OnOperationFailed("You no longer have an account with IAT Software.", "Account Deleted");
                return false;
            }
            else if (inTrans.Transaction == TransactionRequest.ETransaction.NoSuchClient)
            {
                OnOperationFailed("Your product is not registered with the IATSoftware.net server.  If you believe you are seeing this message in error, " +
                    "please contact us at admin@iatsoftware.net", "Unknown Client");
                return false;
            }
            else if (inTrans.Transaction != TransactionRequest.ETransaction.RequestTransmission)
            {
                OnOperationFailed("Could Not Connect with Server.", "Server Error");
                return false;
            }
            TransactionRequest trans = new TransactionRequest();
            trans.Transaction = TransactionRequest.ETransaction.IATExists;
            trans.IATName = IATName;
            trans.IsLastTransaction = false;
            tEvent = MySOAP.BeginNewTransactionEvent("Initiating IAT Deployment");
            tEvent.AddChildEvent("Checking for an IAT on the server with the same name");
            CallSOAP(MySOAP.ESoapAction.IATExists, trans, inTrans);
            if (inTrans.Transaction == TransactionRequest.ETransaction.IATExists)
            {
                if (OnDisplayYesNoMessageBox(Properties.Resources.sIATExistsMsg, Properties.Resources.sIATExistsCaption) != DialogResult.Yes)
                    return false;
                TransactionRequest.ETransaction transactionResult;
                lock (lockObject)
                    transactionResult = MySOAP.VerifyPassword(IATName, CPartiallyEncryptedRSAKey.EKeyType.Admin, AdminPassword);
                if (transactionResult != TransactionRequest.ETransaction.TransactionSuccess)
                {
                    OnOperationFailed("The supplied password does not match the password supplied when this IAT was initially uploaded.", "Invalid Password");
                    return false;
                }
                /*                    if (!MySOAP.VerifyPassword(Properties.Resources.sDataProviderServlet, IATName, CPartiallyEncryptedRSAKey.EKeyType.Data, DataPassword))
                                    {
                                        if (!MySOAP.VerifyPassword(Properties.Resources.sDataProviderServlet, MainForm.IATName, CPartiallyEncryptedRSAKey.EKeyType.Data, DataPassword))
                                        {
                                            if (DisplayYesNoMessageBox("Passwords Do Not Match", "The data retrieval password you entered does not match the password previously supplied.  Would you like to replace the old password" +
                                                " with the new one?") != DialogResult.Yes)
                                            {
                                                DataPasswordForm dpf = new DataPasswordForm();
                                                DataPasswordForm.EDataPassword dataPassResult = DisplayDataPassword(dpf);
                                                if (dataPassResult == DataPasswordForm.EDataPassword.cancel)
                                                {
                                                    OnEndProgressBarUse();
                                                    return;
                                                }
                                                else if (dataPassResult == DataPasswordForm.EDataPassword.noMatch)
                                                {
                                                    OperationFailed("Wrong Password", "The password you supplied does not match the previous data retrieval password.");
                                                    return;
                                                }
                                            }
                                        }
                                    }
                                    *//*

                MySOAP.CurrentTransactionEvent.AddChildEvent("Retrieving IAT Encryption Data from Server");
                trans.Transaction = TransactionRequest.ETransaction.RequestRSAKey;
                trans.IATName = IATName;
                trans.StringValue = CPartiallyEncryptedRSAKey.EKeyType.Data.ToString();
                trans.IsLastTransaction = false;
                CallSOAP(MySOAP.ESoapAction.RequestRSAKey, trans, dataKey);
                trans.StringValue = CPartiallyEncryptedRSAKey.EKeyType.Admin.ToString();
                CallSOAP(MySOAP.ESoapAction.RequestRSAKey, trans, adminKey);
            }
            else
            {
                MySOAP.CurrentTransactionEvent.AddChildEvent("Generating IAT Encryption Data");
                dataKey = new CPartiallyEncryptedRSAKey(CPartiallyEncryptedRSAKey.EKeyType.Data);
                dataKey.Generate(DataPassword);
                adminKey = new CPartiallyEncryptedRSAKey(CPartiallyEncryptedRSAKey.EKeyType.Admin);
                adminKey.Generate(AdminPassword);
            }
            String URL = Properties.Resources.sDataProviderServlet;
            URL = URL.Substring(0, URL.IndexOf("/IATServer") + "/IATServer".Length);
            Manifest iatManifest = GenerateFileManifest();
            MySOAP.CurrentTransactionEvent.AddChildEvent("Sending IAT File Manifest");
            CallSOAP(MySOAP.ESoapAction.InitiateIATDeployment, iatManifest, inTrans);
            if (inTrans.Transaction == TransactionRequest.ETransaction.InsufficientDiskSpace)
                {
                    OnOperationFailed("You do not have enough disk space alotted to your account to upload this IAT.  If you wish to more or have questions regarding this, please contact us at admin@iatsoftware.net", "Insufficient Disk Space");
                    return false;
                }
                else if (inTrans.Transaction == TransactionRequest.ETransaction.InsufficientIATs)
                {
                    OnOperationFailed("You have already uploaded as many IATs as permitted by your alotted quota.  If you wish to increase your quoata or have questions regarding this, please contact us at admin@iatsoftware.net", "Insufficient IATs");
                    return false;
                }
            CRSAKeyPair keyPair = new CRSAKeyPair(dataKey, adminKey);
            MySOAP.CurrentTransactionEvent.AddChildEvent("Registering Encryption Keys Data");
            CallSOAP(MySOAP.ESoapAction.RecordEncryptionKey, keyPair, inTrans);
            if (inTrans.Transaction != TransactionRequest.ETransaction.TransactionSuccess)
                throw new IATUploadException("Server Error", "The IAT server encountered an error while attempting to register your IAT.  If this problem persists, please contact us at admin@iatsoftware.net.");
            trans = new TransactionRequest();
            trans.Transaction = TransactionRequest.ETransaction.RequestTransmission;
            CallSOAP(MySOAP.ESoapAction.DoIATDeploy, trans, inTrans);
            if (inTrans.Transaction != TransactionRequest.ETransaction.RequestTransmission)
            {
                throw new IATUploadException("Server Error", "The IAT server encountered an error while preparing to receive your IAT.  If this problem persists, please contact us at admin@iatsoftware.net.");
            }
            SetStatusMessage("Uploading IAT");
            TransmissionInProgress = true;
            MySOAP.BeginNewTransactionEvent("Uploading IAT");
            CPacketTransmission.ETransmissionResult transResult = SendManifest();
            TransmissionInProgress = false;
            if (transResult == CPacketTransmission.ETransmissionResult.Cancel)
                throw new TransactionAbortedException();
            else if (transResult == CPacketTransmission.ETransmissionResult.Fail)
            {
                throw new IATUploadException("Packet Transmission Error", "The upload of your IAT failed.  This might be due to internet connectivity issues or a server error.  Please check your internet connection and try again. " +
                    "If this problem persists, please contact us at admin@iatsoftware.net");
            }
            trans = new TransactionRequest();
            trans.IATName = IATName;
            trans.Transaction = TransactionRequest.ETransaction.QueryDeploymentProgress;
            trans.IsLastTransaction = false;
            CDeploymentProgressUpdate deploymentProgress = new CDeploymentProgressUpdate();
            CDeploymentProgressUpdate.EStage progressStage = CDeploymentProgressUpdate.EStage.unset;
            int stageNum = -1;
            tEvent = MySOAP.BeginNewTransactionEvent("Awaiting Deployment Confirmation");
            TransactionEvent childEvent = null;
            do
            {
                CallSOAP(MySOAP.ESoapAction.QueryDeploymentProgress, trans, deploymentProgress);
                if (deploymentProgress.IsUnchanged)
                    continue;
                if (stageNum != deploymentProgress.StageNum)
                {
                    childEvent = tEvent.AddChildEvent(deploymentProgress.StatusMessage);
                    SetStatusMessage(deploymentProgress.StatusMessage);
                    if (deploymentProgress.ProgressMax > 0)
                    {
                        childEvent.MaxProgressValue = deploymentProgress.ProgressMax;
                        childEvent.ProgressValue = deploymentProgress.CurrentProgress;
                        SetProgressBarRange(0, deploymentProgress.ProgressMax);
                        SetProgressValue(deploymentProgress.CurrentProgress);
                    }
                    else
                        ResetProgressBar();
                }
                else if (deploymentProgress.ProgressMax > 0)
                {
                    childEvent.ProgressValue = deploymentProgress.CurrentProgress;
                    SetProgressValue(deploymentProgress.CurrentProgress);
                }
                stageNum = deploymentProgress.StageNum;
                Thread.Sleep(500);
            } while (!deploymentProgress.IsLastUpdate);
            if (deploymentProgress.Stage == CDeploymentProgressUpdate.EStage.failed)
            {
                CDeploymentProgressUpdate failureDPU = deploymentProgress;
                do
                {
                    CallSOAP(MySOAP.ESoapAction.QueryDeploymentProgress, trans, deploymentProgress);
                    Thread.Sleep(500);
                } while (!deploymentProgress.IsLastUpdate);
                if (deploymentProgress.Stage == CDeploymentProgressUpdate.EStage.cleanupFailed)
                    OnOperationFailed(failureDPU.StatusMessage + " " + Properties.Resources.sDeploymentRollbackFailure, "Deployment Failure");
                else
                    OnOperationFailed(failureDPU.StatusMessage, "Deployment Failure");
            }
            /*
            trans = new TransactionRequest();
            trans.Transaction = TransactionRequest.ETransaction.VerifyIATDeployment;
            trans.IsLastTransaction = true;
            trans.IATName = IATName;
            MySOAP.CallSOAP(Properties.Resources.sDataProviderServlet, MySOAP.ESoapAction.VerifyIATDeployment, trans, inTrans);
            if (inTrans.Transaction != TransactionRequest.ETransaction.TransactionSuccess)
            {
                EDeployResult deployResult = (EDeployResult)Enum.Parse(typeof(EDeployResult), inTrans.StringValue);
                String deployErrorMessage;
                switch (deployResult)
                {
                    case EDeployResult.backupLost:
                        deployErrorMessage = "Prior to deploying your IAT, the server made a backup of the existing IAT. The result set generated by your new IAT was of a different format " +
                            "from the result set of the existing IAT. Incongruous result set formats cannot coexist in the database for the same IAT so your attempt to overwrite the existing " +
                            "failed. Furthermore, the server failed to restore the backup it created of your existing IAT. You may redeploy your original IAT without error. If you do not have " +
                            "a copy of your original IAT, please contact us at admin@iatsoftware.net so we can attempt to restore your original IAT.";
                        break;

                    case EDeployResult.cannotBackup:
                        deployErrorMessage = "Prior to attempting the redeployment of your IAT, the server attempted to backup the existing IAT. This backup failed. It is unlikely that your " +
                            "original IAT was lost. However, you should ensure that it still administers correctly. If you encounter an error, please contact us at admin@iatsoftware.net so can " +
                            "attempt to restore your original IAT. It is unwise to attempt to redeploy your original IAT if you encounter an administration error.";
                        break;

                    case EDeployResult.databaseError:
                        deployErrorMessage = "A database error occurred on the server. If this problem persists, please contact us at admin@iatsoftware.net.";
                        break;

                    case EDeployResult.deploymentTimerExpired:
                        deployErrorMessage = "A long period of inactivity occured during IAT deployment and the server presummed deployment had failed. If this was due to internet connectivity issues " +
                            "during deployment, please try again. If this problem persists, please contact us at admin@iatsoftware.net.";
                        break;

                    case EDeployResult.fileDeploymentError:
                        deployErrorMessage = "An error occurred either in the the transfer of your IAT package or the server was unable to process and store your IAT. This could be due " +
                            "to a corrupt package file. Please repackage your IAT and reattempt deployment. If this does not resolve the issue, please contact us at admin@iatsoftware.net.";
                        break;

                    case EDeployResult.genericError:
                        deployErrorMessage = "The server failed to deploy your IAT. Please try again. If this problem persists, please contact us at admin@iatsoftware.net.";
                        break;

                    case EDeployResult.incompatibleResultDescriptors:
                        deployErrorMessage = "The result set format that would be generated by your new IAT is not compatible with the result set format of the existing IAT. The server " +
                            "would be unable to continue recording result data atop the result data already collected and so redeployment was rejected. If you have deleted your result data or if no result data has been collected yet, please delete your IAT as well " +
                            "and try again.";
                        break;

                    case EDeployResult.transformError:
                        deployErrorMessage = "The server encountered an error while processing your IAT. This could be due " +
                            "to a corrupt package file. Please repackage your IAT and reattempt deployment. If this does not resolve the issue, please contact us at admin@iatsoftware.net.";
                        break;

                    default:
                        deployErrorMessage = "A corrupt transmission was recieved from the server while attempting to verify the deployment of your IAT. Please check to see if your IAT " +
                            "administers correctly by copying the following URL into the address bar of your web browser.  Please either bookmark this URL or copy and paste it into a file " +
                            "as it is the location of your IAT on the Internet:\r\n" + ServerURL.Substring(0, ServerURL.LastIndexOf("/") + 1) + String.Format(Properties.Resources.sIATServletURLPart, IATName, inTrans.ClientID) +
                            "\r\nIf your IAT does not administer correctly, attempt redeployment. If this does not resolve the issue, please contact us at admin@iatsoftware.net.";
                        break;

                }
                throw new IATUploadException("IAT Deployment Error", deployErrorMessage);
            }
             * *//*
            IATSummary = new CIATSummary(ConfigFile);
            IATSummary.IATLink = Properties.Resources.sDataProviderServlet.Substring(0, Properties.Resources.sDataProviderServlet.LastIndexOf("/")) + String.Format(Properties.Resources.sIATServletURLPart, IATName, ClientID);
            IATSummary.DataRetrievalPassword = DataPassword;
            IATSummary.AdminPassword = AdminPassword;
            OnOperationComplete(IATSummary);
            MySOAP.EndTransaction();
            return true;
        }
        catch (IATConfigFileNamespace.PackagingException ex)
        {
            return false;
        }
        catch (TransactionAbortedException)
        {
            return false;
        }
        catch (TimeoutException ex)
        {
            throw new CXmlSerializationException("Server Not Responsive", "The server is not responsive.  Please try again later.  If this problem persists, contact us at admin@iatsoftware.net.", ex);
        }
        catch (WebException ex)
        {
            if (((HttpWebResponse)ex.Response).StatusCode == HttpStatusCode.NotAcceptable)
            {
                TransactionRequest trans = new TransactionRequest();
                trans.Transaction = TransactionRequest.ETransaction.QueryDeploymentProgress;
                CDeploymentProgressUpdate deployProgress = new CDeploymentProgressUpdate();
                MySOAP.CallSOAP(Properties.Resources.sDataProviderServlet, Convert.ToInt32(Properties.Resources.sDataProviderPort), MySOAP.ESoapAction.QueryDeploymentProgress, trans, deployProgress);
                throw new IATUploadException("Server Error", deployProgress.StatusMessage);
            }
            throw new CXmlSerializationException("Server Error", ex.Message, ex);
        }
        catch (IOException ex)
        {
            throw new CXmlSerializationException("Error", ex.Message, ex);
        }
        catch (XmlException ex)
        {
            throw new CXmlSerializationException("Server Response Error", ex.Message, ex);
        }
        catch (CryptographicException ex)
        {
            throw new CXmlSerializationException("Error Decrypting Server Response", ex.Message, ex);
        }
        catch (ArgumentException ex)
        {
            throw new CXmlSerializationException("Server Response Stream Not Readable", ex.Message, ex);
        }
        catch (IATUploadException ex)
        {
            throw new CXmlSerializationException(ex.Caption, ex.Message, ex);
        }
    }
    catch (CXmlSerializationException ex)
    {
        if (ex.ErrorType == CXmlSerializationException.EErrorType.terminateProcess)
            ManageFail(ex.DeploymentPhase);
        else if (ex.ErrorType == CXmlSerializationException.EErrorType.fatal)
        {
            ErrorReportDisplay errorDisplay = null;
            errorDisplay = new ErrorReportDisplay(ex.Message, ex);
            lock (lockObject)
            {
                if (!Aborted)
                {
                    MainForm.Invoke(ShowForm, errorDisplay);
                }
            }
        }
        else
        {
            ErrorReportDisplay errorDisplay = null;
            errorDisplay = new ErrorReportDisplay(ex.Message, ex);
            lock (lockObject)
            {
                if (!Aborted)
                {
                    MainForm.Invoke(ShowForm, errorDisplay);
                }
            }
            ManageFail(ex.DeploymentPhase);
        }
        return false;
    }
    finally
    {
        OnEndProgressBarUse();
        MySOAP.EndTransaction();
    }
}

private void ManageFail(CDeploymentProgressUpdate.EStage failReason)
{
    if (failReason == CDeploymentProgressUpdate.EStage.unset)
        return;
    CDeploymentProgressUpdate progressUpdate = new CDeploymentProgressUpdate();
    TransactionRequest trans = new TransactionRequest();
    SetStatusMessage("Cleaning up failed deployment.");
    do
    {
        Thread.Sleep(500);
        MySOAP.CallSOAP(Properties.Resources.sDataProviderServlet, Convert.ToInt32(Properties.Resources.sDataProviderPort), MySOAP.ESoapAction.QueryDeploymentProgress, trans, progressUpdate);
    } while (!progressUpdate.IsLastUpdate);
    if ((progressUpdate.Stage == CDeploymentProgressUpdate.EStage.cleanupComplete) || (progressUpdate.Stage == CDeploymentProgressUpdate.EStage.backupRestored))
        DisplayMessageBox("The partial deployment of your IAT was successfully cleaned up after. Please try uploading again.", "Cleanup Successful");
    else if (progressUpdate.Stage == CDeploymentProgressUpdate.EStage.cleanupFailed)
        DisplayMessageBox("The server could not erase the files from the partial deployment of your IAT. Please attempt to delete your IAT with the password you " +
            "and then try to upload it again.", "IAT Cleanup Failed");
    else if (progressUpdate.Stage == CDeploymentProgressUpdate.EStage.cannotRestoreBackup)
        DisplayMessageBox("The server could not restore the backup of your previous IAT.  Your data is not lost, but you will not be able to redeploy your IAT " +
            "until you contact us at admin@iatsoftware.net", "Cannot Restore Backup");
}


private void run()
{
    DynamicSpecifier.CompactSpecifierDictionary(IAT);
    bUploadSuccessful = DoUpload();
    if (!Aborted)
        MainForm.Invoke(EndProgressBarUse);
    UploadComplete.Set();
}

public void Upload(String iatName, String password)
{
    UploadComplete.Reset();
    DataPassword = password;
    AdminPassword = password;
    _IATName = iatName;
    MainForm.BeginProgressBarUse(OnCancel, IATConfigMainForm.EProgressBarUses.Upload);
    MainForm.AddToolStripCancelButton(OnCancel);
    ThreadStart proc = new ThreadStart(run);
    Thread th = new Thread(proc);
    th.Start();
}

public void Upload()
{
    UploadComplete.Reset();
    DataPassword = MainForm.DataRetrievalPassword;
    AdminPassword = MainForm.AdminPassword;
    _IATName = MainForm.IATName;
    MainForm.BeginProgressBarUse(OnCancel, IATConfigMainForm.EProgressBarUses.Upload);
    MainForm.AddToolStripCancelButton(OnCancel);
    ThreadStart proc = new ThreadStart(run);
    Thread th = new Thread(proc);
    th.Start();
}
}
}
*/
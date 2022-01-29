using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using System.IO;
using System.IO.Packaging;
using System.Security.Cryptography;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;
using Saxon.Api;
using IATClient.ResultData;

namespace IATClient
{

    class CResultDocument
    {
        private IATConfigMainForm MainForm
        {
            get
            {
                return (IATConfigMainForm)Application.OpenForms[Properties.Resources.sMainFormName];
            }
        }

        abstract class ExcelTransform
        {
            protected static object _TransformLock = new object();
            private static byte[] DESKeyBytes = { 0x1C, 0x39, 0xA3, 0xF2, 0x2C, 0x13, 0x45, 0x21 };
            private static byte[] IVBytes = { 0x0A, 0x47, 0x99, 0xF3, 0x1A, 0x42, 0x79, 0x01 };
            protected static DESCryptoServiceProvider desCrypt = new DESCryptoServiceProvider();
            protected static Saxon.Api.Processor XSLTProcessor = new Processor();
            protected static XsltCompiler XSLTCompiler = XSLTProcessor.NewXsltCompiler();
            protected MemoryStream Content = new MemoryStream();

            public ExcelTransform(XmlNode contentNode)
            {
                FromBase64Transform from64 = new FromBase64Transform();
                ICryptoTransform desTrans = desCrypt.CreateDecryptor(DESKeyBytes, IVBytes);
                CryptoStream cStream = new CryptoStream(Content, desTrans, CryptoStreamMode.Write);
                CryptoStream bStream = new CryptoStream(cStream, from64, CryptoStreamMode.Write);
                StreamWriter txtWriter = new StreamWriter(bStream);
                txtWriter.Write(contentNode.InnerText);
                txtWriter.Flush();
                bStream.FlushFinalBlock();
                Content.Seek(0, SeekOrigin.Begin);
            }

            public abstract void Transform(XmlDocument responseDoc, ZipPackage package, Action incProgress, WorkerPool wPool);

            public static object TransformLock
            {
                get
                {
                    return _TransformLock;
                }
            }

        }

        class ExcelAddendumComponent : ExcelTransform
        {
            private XsltExecutable xsltExec;
            private XsltTransformer xsltTrans;

            public ExcelAddendumComponent(XmlNode transNode)
                : base(transNode.SelectSingleNode("Content"))
            {
                xsltExec = XSLTCompiler.Compile(Content);
                xsltTrans = xsltExec.Load();
            }

            public override void Transform(XmlDocument responseDoc, ZipPackage package, Action incrementProgress, WorkerPool wPool)
            {
                XdmNode inputNode;
                TextWriter txtWriter;
                lock (TransformLock)
                {
                    txtWriter = new StringWriter();
                    inputNode = XSLTProcessor.NewDocumentBuilder().Build(responseDoc.DocumentElement);
                    Serializer ser = XSLTProcessor.NewSerializer(txtWriter);
                    xsltTrans.InitialContextNode = inputNode;
                    xsltTrans.Run(ser);
                txtWriter.Flush();
                XmlDocument doc = new XmlDocument();
                doc.Load(new StringReader(txtWriter.ToString()));
                    if (responseDoc.DocumentElement.SelectSingleNode("Addendum") == null)
                        responseDoc.DocumentElement.AppendChild(responseDoc.CreateElement("Addendum"));
                    responseDoc.DocumentElement["Addendum"].AppendChild(responseDoc.ImportNode(doc.DocumentElement, true));
                incrementProgress.Invoke();
                }
            }
        }

        class ExcelCopyComponent : ExcelTransform
        {
            private String outputPath, contentType;

            public ExcelCopyComponent(XmlNode transNode)
                : base(transNode.SelectSingleNode("Content"))
            {
                outputPath = transNode.SelectSingleNode("OutputPath").InnerText;
                contentType = transNode.SelectSingleNode("ContentType").InnerText;
            }

            public override void Transform(XmlDocument responseDoc, ZipPackage package, Action incrementProgress, WorkerPool wPool)
            {
                lock (TransformLock)
                {
                    ZipPackagePart zpp = (ZipPackagePart)package.CreatePart(new Uri(outputPath, UriKind.Relative), contentType, CompressionOption.Normal);
                    zpp.GetStream().Write(Content.ToArray(), 0, (int)Content.Length);
                    zpp.GetStream().Flush();
                    zpp.GetStream().Close();
                    incrementProgress.Invoke();
                }
            }
        }

        class ExcelTransformComponent : ExcelTransform
        {
            private XsltExecutable xsltExec;
            private XsltTransformer xsltTrans;
            private String outputPath, contentType;

            public ExcelTransformComponent(XmlNode transNode)
                : base(transNode.SelectSingleNode("Content"))
            {
                xsltExec = XSLTCompiler.Compile(Content);
                xsltTrans = xsltExec.Load();
                outputPath = transNode.SelectSingleNode("OutputPath").InnerText;
                contentType = transNode.SelectSingleNode("ContentType").InnerText;
            }

            public override void Transform(XmlDocument responseDoc, ZipPackage package, Action incrementProgress, WorkerPool wPool)
            {
                BackgroundWorker worker = new BackgroundWorker();
                worker.DoWork += (o, a) =>
                {
                    MemoryStream memStream = new MemoryStream();
                    TextWriter txtWriter = new StreamWriter(memStream, Encoding.UTF8);
                    Serializer ser = XSLTProcessor.NewSerializer(txtWriter);
                    XdmNode inputNode;
                    lock (TransformLock)  
                    {
                        inputNode = XSLTProcessor.NewDocumentBuilder().Build(responseDoc.DocumentElement);
                        xsltTrans.InitialContextNode = inputNode;
                        xsltTrans.Run(ser);
                        txtWriter.Flush();
                        ZipPackagePart zpp = (ZipPackagePart)package.CreatePart(new Uri(outputPath, UriKind.Relative), contentType, CompressionOption.Normal);
                        zpp.GetStream().Write(memStream.ToArray(), 0, (int)memStream.Length);
                        zpp.GetStream().Close();
                        memStream.Dispose();
                    }
                    incrementProgress.Invoke();
                };
                wPool.QueueWorker(worker, null);
            }
        }

        class ExcelMultiTransformComponent : ExcelTransform
        {
            private XsltExecutable xsltExec;
            private XsltTransformer xsltTrans;
            private String outputPathTemplate, contentType;
            private int templateStartingVal;
            private List<int> ItrCtr;
            private List<int> ItrVal;

            public ExcelMultiTransformComponent(XmlNode transNode, List<int> itrCtr, List<int> itrVal)
                : base(transNode.SelectSingleNode("Content"))
            {
                xsltExec = XSLTCompiler.Compile(Content);
                xsltTrans = xsltExec.Load();
                outputPathTemplate = transNode.SelectSingleNode("OutputTemplate").InnerText;
                contentType = transNode.SelectSingleNode("ContentType").InnerText;
                templateStartingVal = Convert.ToInt32(transNode.SelectSingleNode("OutputTemplate").Attributes["StartingValue"].Value);
                ItrCtr = itrCtr;
                ItrVal = itrVal;
            }

            public override void Transform(XmlDocument responseDoc, ZipPackage package, Action incrementProgress, WorkerPool wPool)
            {
                lock (TransformLock)
                {
                    if (responseDoc.DocumentElement.SelectSingleNode("Addendum/IterationInfo") == null)
                    {
                        XmlNode itrNode = responseDoc.CreateElement("IterationInfo");
                        itrNode.AppendChild(responseDoc.CreateElement("IterationCount"));
                        itrNode.AppendChild(responseDoc.CreateElement("IterationValue"));
                        responseDoc.DocumentElement.SelectSingleNode("Addendum").AppendChild(itrNode);
                    }
                }
                for (int ctr = 0; ctr < ItrCtr.Count(); ctr++)
                {
                    BackgroundWorker worker = new BackgroundWorker();
                    worker.DoWork += (o, a) =>
                    {
                        MemoryStream memStream = new MemoryStream();
                        Int32 ndx = (Int32)a.Argument;
                        XdmNode inputNode;
                        lock (TransformLock) {
                            responseDoc.SelectSingleNode("//IterationCount").InnerText = ItrCtr[ndx].ToString();
                            responseDoc.SelectSingleNode("//IterationValue").InnerText = ItrVal[ndx].ToString();
                            inputNode = XSLTProcessor.NewDocumentBuilder().Build(responseDoc.DocumentElement);
                        TextWriter txtWriter = new StreamWriter(memStream, Encoding.UTF8);
                        Serializer ser = XSLTProcessor.NewSerializer(txtWriter);
                        xsltTrans.InitialContextNode = inputNode;
                            xsltTrans.Run(ser);
                            txtWriter.Flush();
                            ZipPackagePart zpp = (ZipPackagePart)package.CreatePart(new Uri(String.Format(outputPathTemplate, templateStartingVal + ndx), UriKind.Relative), contentType, CompressionOption.Normal);
                            zpp.GetStream().Write(memStream.ToArray(), 0, (int)memStream.Length);
                            zpp.GetStream().Close();
                            memStream.Dispose();
                        }
                        incrementProgress.Invoke();
                    };
                    wPool.QueueWorker(worker, ctr);
                }
            }

        }

        class WorkerPool
        {
            private List<BackgroundWorker> workerThreads = new List<BackgroundWorker>();
            private List<object> workerArgs = new List<object>();
            private Action WorkComplete;
            private bool endQueue = false;
            private const int NumWorkerThreads = 2;
            private object queueLock = new object();
            private int NumRunningWorkers = 0;
            private Semaphore semaphore = new Semaphore(3, 3);
            private bool canceled = false;

            public WorkerPool(Action workComplete)
            {
                this.WorkComplete = workComplete;
            }

            public void CancelJob()
            {
                canceled = true;
            }

            public void EndQueue()
            {
                this.endQueue = true;
            }

            public void QueueWorker(BackgroundWorker worker, object arg)
            {
                semaphore.WaitOne();
                worker.RunWorkerCompleted += (o, a) =>
                {
                    lock (queueLock)
                    {
                        NumRunningWorkers--;
                        if ((this.workerThreads.Count() == 0) && endQueue && (NumRunningWorkers == 0))
                            WorkComplete.Invoke();
                        else if ((this.workerThreads.Count() > 0) && !canceled)
                        {
                            this.workerThreads[0].RunWorkerAsync(workerArgs[0]);
                            NumRunningWorkers++;
                            this.workerThreads.RemoveAt(0);
                            this.workerArgs.RemoveAt(0);
                        }
                        else if (canceled && (NumRunningWorkers == 0))
                            WorkComplete.Invoke();
                    }
                    semaphore.Release();
                };
                lock (queueLock)
                {
                    if (canceled && (NumWorkerThreads == 0))
                    {
                        WorkComplete.Invoke();
                        semaphore.Release();
                    }
                    else if (!canceled)
                    {
                        if (NumRunningWorkers < NumWorkerThreads)
                        {
                            worker.RunWorkerAsync(arg);
                            NumRunningWorkers++;
                        }
                        else
                        {
                            workerThreads.Add(worker);
                            workerArgs.Add(arg);
                        }
                    }
                    else
                        semaphore.Release();
                }
            }

        }


        private XmlDocument ResponseDoc = new XmlDocument();
        private ResultDocument.ResultDocument ResultDocument = new ResultDocument.ResultDocument();
        private MemoryStream ResultXML = new MemoryStream();
        private static List<ExcelTransform> TransformList = new List<ExcelTransform>();
        private Package XLSXPackage = null;
        private List<Image> TitlePageImages = new List<Image>();
        private static List<String> NumberWords = new List<String>();
        private CItemSlideContainer SlideContainer;
        private ManualResetEvent TransformInitEvent, ExportCompleteEvent;
        private static string[] OutputFontFamilies = { "Calibri", "Tahoma" };
        private BackgroundWorker ExportWorker = null;
        private WorkerPool TransformWorkers = null;
        private bool bCompleted = false;

        static CResultDocument()
        {
            string[] digitWords = { "first", "second", "third", "fourth", "fifth", "sixth", "seventh", "eighth", "nineth" };
            string[] teenWords = { "tenth", "eleventh", "twelveth", "thirteenth", "forteenth", "fifteenth", "sixteenth", "seventeenth", "eighteenth" };
            string[] tenMultWords = { "twentieth", "thirtieth", "fourtieth", "fiftieth", "sixtieth", "seventieth", "eightieth", "ninethieth" };
            string[] leadWords = { "twenty", "thirty", "fourty", "fifty", "sixty", "seventy", "eighty", "ninety" };
            NumberWords.AddRange(digitWords);
            NumberWords.AddRange(teenWords);
            for (int ctr = 0; ctr < 8; ctr++)
            {
                NumberWords.Add(tenMultWords[ctr]);
                for (int ctr2 = 0; ctr2 < digitWords.Length; ctr2++)
                    NumberWords.Add(leadWords[ctr] + "-" + digitWords[ctr2]);
            }
            NumberWords.Add("hundreth");


        }

        private static char[] alphabet = { 'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J', 'K', 'L', 'M', 'N', 'O', 'P', 'Q', 'R', 'S', 'T', 'U', 'V', 'W', 'X', 'Y', 'Z' };

        private String colNumToName(int colNum)
        {
            colNum--;
            String colName = String.Empty;
            if (colNum >= 26 * 26)
            {
                colName += alphabet[colNum / (26 * 26)];
                colName += alphabet[(colNum % (26 * 26)) / 26];
                colName += alphabet[colNum % 26];
            }
            else if (colNum >= 26)
            {
                colName += alphabet[(colNum / 26) - 1];
                colName += alphabet[colNum % 26];
            }
            else
                colName += alphabet[colNum];
            return colName;

        }

        private void BuildTitlePage(ResultData.ResultData rd)
        {
            TitlePageImages.Clear();
            Image canvasBmp = new Bitmap(1000, 5000);
            Image titlePageImg;
            Graphics copyG;
            Graphics g = Graphics.FromImage(canvasBmp);
            g.FillRectangle(Brushes.White, new Rectangle(0, 0, canvasBmp.Width, canvasBmp.Height));
            g.DrawImage(Properties.Resources.logo, new Rectangle(100, 100, 800, Properties.Resources.logo.Height * 800 / Properties.Resources.logo.Width));
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAliasGridFit;
            float yOffset = 150 + Properties.Resources.logo.Height * 800 / Properties.Resources.logo.Width;
            Font font = null;
            int fCtr = 0;
            while (font == null)
            {
                if (fCtr == OutputFontFamilies.Length)
                    font = new Font(System.Drawing.SystemFonts.DialogFont.FontFamily, 11F);
                else
                {
                    foreach (FontFamily ff in FontFamily.Families)
                        if (ff.Name == OutputFontFamilies[fCtr])
                        {
                            font = new Font(ff, 11F);
                            break;
                        }
                    fCtr++;
                }
            }
            Font titleFont = new Font(System.Drawing.SystemFonts.DefaultFont.FontFamily, 18F);
            Font headerFont = new Font(System.Drawing.SystemFonts.DefaultFont.FontFamily, 13F);
            SizeF sz = g.MeasureString("Results for " + rd.Descriptor.ConfigFile.Name, titleFont);
            g.DrawString("Results for " + rd.Descriptor.ConfigFile.Name, titleFont, Brushes.DarkBlue, new PointF(500 - (sz.Width / 2), yOffset));
            yOffset += sz.Height + 25;
            sz = g.MeasureString(String.Format("Test Author: {0}", rd.Descriptor.TestAuthor), titleFont);
            g.DrawString(String.Format("Test Author: {0}", rd.Descriptor.TestAuthor), titleFont, Brushes.DarkBlue, new PointF(500 - (sz.Width / 2), yOffset));
            yOffset += sz.Height + 25;
            String str = String.Format("Mean: {0:F6}                  Standard Deviation: {1:F6}", rd.Mean, rd.SD);
            sz = g.MeasureString(str, headerFont);
            g.DrawString(str, headerFont, Brushes.Black, new PointF(500 - (sz.Width / 2), yOffset));
            yOffset += sz.Height + 25;
            sz = g.MeasureString(Properties.Resources.sOutputTitlePageHeading, font, 800, StringFormat.GenericDefault);
            g.DrawString(Properties.Resources.sOutputTitlePageHeading, font, Brushes.Black, new RectangleF(new PointF(100, yOffset), sz));
            yOffset += sz.Height + 50;
            sz = g.MeasureString("Summary Grid Format", headerFont);
            g.DrawString("Summary Grid Format", headerFont, Brushes.Black, new PointF(100, yOffset));
            yOffset += sz.Height + 10;
            int colNum = 1;
            if (rd.Descriptor.TokenType != ETokenType.NONE) { 
                str = String.Format("Column {0}: {1} (test taker token)", colNumToName(colNum++), rd.Descriptor.TokenName);
                sz = g.MeasureString(str, font);
                if (yOffset + sz.Height > canvasBmp.Height)
                {
                    titlePageImg = new Bitmap(1000, ((int)Math.Ceiling(yOffset) % 20 == 0) ? (int)Math.Ceiling(yOffset) : ((int)Math.Ceiling(yOffset) + 20 - ((int)Math.Ceiling(yOffset) % 20)));
                    copyG = Graphics.FromImage(titlePageImg);
                    copyG.FillRectangle(Brushes.White, new Rectangle(0, 0, titlePageImg.Width, titlePageImg.Height));
                    copyG.DrawImage(canvasBmp, new Rectangle(0, 0, titlePageImg.Width, titlePageImg.Height), new Rectangle(0, 0, titlePageImg.Width, titlePageImg.Height), GraphicsUnit.Pixel);
                    copyG.Dispose();
                    TitlePageImages.Add(titlePageImg);
                    g.FillRectangle(Brushes.White, new Rectangle(0, 0, canvasBmp.Width, canvasBmp.Height));
                    yOffset = 0;
                }
                g.DrawString(str, font, Brushes.Black, new PointF(150, yOffset));
                yOffset += 10 + sz.Height;
            }

            for (int ctr = 0; ctr < rd.Descriptor.BeforeSurveys.Count; ctr++)
            {
                if (rd.Descriptor.BeforeSurveys[ctr].NumQuestions == 0)
                    continue;
                Image surveyImg = GenerateSurveySummary(rd.Descriptor.BeforeSurveys[ctr], colNum, font);
                if (surveyImg.Height + yOffset > canvasBmp.Height)
                {
                    titlePageImg = new Bitmap(1000, ((int)Math.Ceiling(yOffset) % 20 == 0) ? (int)Math.Ceiling(yOffset) : ((int)Math.Ceiling(yOffset) + 20 - ((int)Math.Ceiling(yOffset) % 20)));
                    copyG = Graphics.FromImage(titlePageImg);
                    copyG.FillRectangle(Brushes.White, new Rectangle(0, 0, titlePageImg.Width, titlePageImg.Height));
                    copyG.DrawImage(canvasBmp, new Rectangle(0, 0, titlePageImg.Width, titlePageImg.Height), new Rectangle(0, 0, titlePageImg.Width, titlePageImg.Height), GraphicsUnit.Pixel);
                    copyG.Dispose();
                    TitlePageImages.Add(titlePageImg);
                    g.FillRectangle(Brushes.White, new Rectangle(0, 0, canvasBmp.Width, canvasBmp.Height));
                    yOffset = 0;
                }
                g.DrawImage(surveyImg, new PointF(150, yOffset));
                yOffset += 10 + surveyImg.Height;
                surveyImg.Dispose();
                colNum += rd.Descriptor.BeforeSurveys[ctr].NumQuestions;
            }
            str = String.Format("Column {0}: IAT Score", colNumToName(colNum++));
            sz = g.MeasureString(str, font);
            if (yOffset + sz.Height > canvasBmp.Height)
            {
                titlePageImg = new Bitmap(1000, ((int)Math.Ceiling(yOffset) % 20 == 0) ? (int)Math.Ceiling(yOffset) : ((int)Math.Ceiling(yOffset) + 20 - ((int)Math.Ceiling(yOffset) % 20)));
                copyG = Graphics.FromImage(titlePageImg);
                copyG.FillRectangle(Brushes.White, new Rectangle(0, 0, titlePageImg.Width, titlePageImg.Height));
                copyG.DrawImage(canvasBmp, new Rectangle(0, 0, titlePageImg.Width, titlePageImg.Height), new Rectangle(0, 0, titlePageImg.Width, titlePageImg.Height), GraphicsUnit.Pixel);
                copyG.Dispose();
                TitlePageImages.Add(titlePageImg);
                g.FillRectangle(Brushes.White, new Rectangle(0, 0, canvasBmp.Width, canvasBmp.Height));
                yOffset = 0;
            }
            g.DrawString(str, font, Brushes.Black, new PointF(150, yOffset));
            yOffset += sz.Height + 10;
            for (int ctr = 0; ctr < rd.Descriptor.AfterSurveys.Count; ctr++)
            {
                if (rd.Descriptor.AfterSurveys[ctr].NumQuestions == 0)
                    continue;
                Image surveyImg = GenerateSurveySummary(rd.Descriptor.AfterSurveys[ctr], colNum, font);
                if (surveyImg.Height + yOffset > canvasBmp.Height)
                {
                    titlePageImg = new Bitmap(1000, ((int)Math.Ceiling(yOffset) % 20 == 0) ? (int)Math.Ceiling(yOffset) : ((int)Math.Ceiling(yOffset) + 20 - ((int)Math.Ceiling(yOffset) % 20)));
                    copyG = Graphics.FromImage(titlePageImg);
                    copyG.FillRectangle(Brushes.White, new Rectangle(0, 0, titlePageImg.Width, titlePageImg.Height));
                    copyG.DrawImage(canvasBmp, new Rectangle(0, 0, titlePageImg.Width, titlePageImg.Height), new Rectangle(0, 0, titlePageImg.Width, titlePageImg.Height), GraphicsUnit.Pixel);
                    copyG.Dispose();
                    TitlePageImages.Add(titlePageImg);
                    g.FillRectangle(Brushes.White, new Rectangle(0, 0, canvasBmp.Width, canvasBmp.Height));
                    yOffset = 1;
                }
                g.DrawImage(surveyImg, new PointF(150, yOffset));
                yOffset += surveyImg.Height + 10;
                surveyImg.Dispose();
                colNum += rd.Descriptor.AfterSurveys[ctr].NumQuestions;
            }
            titlePageImg = new Bitmap(1000, ((int)Math.Ceiling(yOffset) % 20 == 0) ? (int)Math.Ceiling(yOffset) : ((int)Math.Ceiling(yOffset) + 20 - ((int)Math.Ceiling(yOffset) % 20)));
            copyG = Graphics.FromImage(titlePageImg);
            copyG.FillRectangle(Brushes.White, new Rectangle(0, 0, titlePageImg.Width, titlePageImg.Height));
            copyG.DrawImage(canvasBmp, new Rectangle(0, 0, titlePageImg.Width, titlePageImg.Height), new Rectangle(0, 0, titlePageImg.Width, titlePageImg.Height), GraphicsUnit.Pixel);
            copyG.Dispose();
            TitlePageImages.Add(titlePageImg);
            g.Dispose();
            canvasBmp.Dispose();
            ResultDocument.TitlePage = new ResultDocument.TTitlePage();
            ResultDocument.TitlePage.PageHeights = new int[TitlePageImages.Count];
            for (int ctr = 0; ctr < TitlePageImages.Count; ctr++)
                ResultDocument.TitlePage.PageHeights[ctr] = TitlePageImages[ctr].Height;
        }

        private Image GenerateSurveySummary(Survey survey, int colNum, Font f)
        {
            int ctr2;
            Image surveyImage = new Bitmap(750, 5000);
            Graphics g = Graphics.FromImage(surveyImage);
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAliasGridFit;
            g.FillRectangle(Brushes.White, new Rectangle(new Point(0, 0), new Size(750, 5000)));
            float colLabelRight = g.MeasureString("Column #888", f).Width;
            float yOffset = 0;
            for (int ctr = 0; ctr < survey.SurveyItems.Length; ctr++)
            {
                if (survey.SurveyItems[ctr].Response.ResponseType == ResponseType.None)
                    continue;
                String str = String.Format("Column {0}", colNumToName(colNum++));
                SizeF sz = g.MeasureString(str, f);
                g.DrawString(str, f, Brushes.Black, new PointF(colLabelRight - sz.Width, yOffset));
                str = survey.SurveyItems[ctr].Text;
                sz = g.MeasureString(str, f, (int)(surveyImage.Width - colLabelRight - 15));
                g.DrawString(str, f, Brushes.Black, new PointF(colLabelRight + 15, yOffset));
                yOffset += sz.Height + 5;
                switch (survey.SurveyItems[ctr].Response.ResponseType)
                {
                    case ResponseType.Boolean:
                        str = "A true/false response. 1 represents \"true\" and 0 represents \"false\"";
                        sz = g.MeasureString(str, f);
                        g.DrawString(str, f, Brushes.Black, new PointF(colLabelRight + 15, yOffset));
                        yOffset += sz.Height;
                        break;

                    case ResponseType.BoundedLength:
                        BoundedLength bl = (BoundedLength)survey.SurveyItems[ctr].Response;
                        str = String.Format("A text response between {0} and {1} characters in length.", bl.MinLength, bl.MaxLength);
                        sz = g.MeasureString(str, f);
                        g.DrawString(str, f, Brushes.Black, new PointF(colLabelRight + 15, yOffset));
                        yOffset += sz.Height;
                        break;

                    case ResponseType.BoundedNumber:
                        BoundedNumber bn = (BoundedNumber)survey.SurveyItems[ctr].Response;
                        str = String.Format("A numeric response between {0} and {1}.", bn.MinValue, bn.MaxValue);
                        sz = g.MeasureString(str, f);
                        g.DrawString(str, f, Brushes.Black, new PointF(colLabelRight + 15, yOffset));
                        yOffset += sz.Height;
                        break;

                    case ResponseType.Date:
                        Date d = (Date)survey.SurveyItems[ctr].Response;
                        if (d.HasStartDate && d.HasEndDate)
                            str = String.Format("A date that falls between {0}/{1}/{2} and {3}/{4}/{5} in MM/DD/YYYY format.", d.StartDate.Month, d.StartDate.Day, d.StartDate.Year, d.EndDate.Month, d.EndDate.Day, d.EndDate.Year);
                        else if (d.HasStartDate)
                            str = String.Format("A date that falls after {0}/{1}/{2} in MM/DD/YYYY format.", d.StartDate.Month, d.StartDate.Day, d.StartDate.Year);
                        else if (d.HasEndDate)
                            str = String.Format("A date that falls before {0}/{1}/{2} in MM/DD/YYYY format.", d.EndDate.Month, d.EndDate.Day, d.EndDate.Year);
                        else
                            str = "A date in MM/DD/YYYY format.";
                        sz = g.MeasureString(str, f);
                        g.DrawString(str, f, Brushes.Black, new PointF(colLabelRight + 15, yOffset));
                        yOffset += sz.Height;
                        break;

                    case ResponseType.FixedDigit:
                        FixedDigit fd = (FixedDigit)survey.SurveyItems[ctr].Response;
                        str = String.Format("A response that consists of {0} digits.", fd.NumDigs);
                        sz = g.MeasureString(str, f);
                        g.DrawString(str, f, Brushes.Black, new PointF(colLabelRight + 15, yOffset));
                        yOffset += sz.Height;
                        break;

                    case ResponseType.Likert:
                        Likert l = (Likert)survey.SurveyItems[ctr].Response;
                        if (l.ReverseScored)
                            str = String.Format("An already reverse-scored likert response with {0} options.", l.Choices.Length);
                        else
                            str = String.Format("A likert response with {0} options.", l.Choices.Length);
                        sz = g.MeasureString(str, f);
                        g.DrawString(str, f, Brushes.Black, new PointF(colLabelRight + 15, yOffset));
                        yOffset += sz.Height + 5;
                        for (ctr2 = 0; ctr2 < l.Choices.Length; ctr2++)
                        {
                            str = String.Format("{0}: {1}", l.ReverseScored ? (l.Choices.Length - ctr2) : ctr2 + 1, l.Choices[ctr2]);
                            sz = g.MeasureString(str, f, (int)(surveyImage.Width - colLabelRight - 25));
                            g.DrawString(str, f, Brushes.Black, new RectangleF(new PointF(colLabelRight + 25, yOffset), sz));
                            yOffset += sz.Height + 5;
                        }
                        yOffset -= 5;
                        break;

                    case ResponseType.MultiBoolean:
                        MultiBoolean mb = (MultiBoolean)survey.SurveyItems[ctr].Response;
                        str = "A multiple selection item whose respone is represented by a series of 1's and 0's where a 1 indicate that a choice was selected and a zero indicates that choice was not." +
                            " Each choice is listed below with the correspding digit. If this digit is 1, that choice was selected by the test taker.";
                        sz = g.MeasureString(str, f, (int)(surveyImage.Width - colLabelRight - 15));
                        g.DrawString(str, f, Brushes.Black, new RectangleF(new PointF(colLabelRight + 15, yOffset), sz));
                        yOffset += sz.Height + 5;

                        for (ctr2 = 0; ctr2 < mb.Choices.Length; ctr2++)
                        {
                            str = String.Format("{0} digit: {1}", NumberWords[ctr2], mb.Choices[ctr2]);
                            sz = g.MeasureString(str, f, (int)(surveyImage.Width - colLabelRight - 25));
                            g.DrawString(str, f, Brushes.Black, new RectangleF(new PointF(colLabelRight + 25, yOffset), sz));
                            yOffset += sz.Height + 5;
                        }
                        yOffset -= 5;
                        break;

                    case ResponseType.Multiple:
                        Multiple mlt = (Multiple)survey.SurveyItems[ctr].Response;
                        str = "A multiple choice question with the following options:";
                        sz = g.MeasureString(str, f, (int)(surveyImage.Width - colLabelRight - 15));
                        g.DrawString(str, f, Brushes.Black, new PointF(colLabelRight + 15, yOffset));
                        yOffset += sz.Height;
                        for (ctr2 = 0; ctr2 < mlt.Choices.Length; ctr2++)
                        {
                            yOffset += 5;
                            str = String.Format("({0}) {1}", ctr2 + 1, mlt.Choices[ctr2]);
                            sz = g.MeasureString(str, f, (int)(surveyImage.Width - colLabelRight - 25));
                            g.DrawString(str, f, Brushes.Black, new RectangleF(new PointF(colLabelRight + 25, yOffset), sz));
                            yOffset += sz.Height;
                        }
                        break;

                    case ResponseType.RegEx:
                        str = String.Format("A text response that matches the regular expression, {0}", ((RegEx)survey.SurveyItems[ctr].Response).RegularExpression);
                        sz = g.MeasureString(str, f, (int)(surveyImage.Width - colLabelRight - 15));
                        g.DrawString(str, f, Brushes.Black, new RectangleF(new PointF(colLabelRight + 15, yOffset), sz));
                        yOffset += sz.Height;
                        break;

                    case ResponseType.WeightedMultiple:
                        WeightedMultiple wmr = (WeightedMultiple)survey.SurveyItems[ctr].Response;
                        str = "A multiple choice question where each choice is assigned a weight. The weight value appears, not the ordinal value of the choice.";
                        sz = g.MeasureString(str, f, (int)(surveyImage.Width - colLabelRight - 15));
                        g.DrawString(str, f, Brushes.Black, new RectangleF(new PointF(colLabelRight + 15, yOffset), sz));
                        yOffset += sz.Height;
                        for (ctr2 = 0; ctr2 < wmr.Choices.Length; ctr2++)
                        {
                            yOffset += 5;
                            str = String.Format("({0}) {1}", wmr.Choices[ctr2].Weight, wmr.Choices[ctr2].Text);
                            sz = g.MeasureString(str, f, (int)(surveyImage.Width - colLabelRight - 15));
                            g.DrawString(str, f, Brushes.Black, new RectangleF(new PointF(colLabelRight + 15, yOffset), sz));
                            yOffset += sz.Height;
                        }
                        break;
                }
                yOffset += 10;
            }
            Image retImg = new Bitmap(surveyImage.Width, (int)Math.Ceiling(yOffset));
            g.Dispose();
            g = Graphics.FromImage(retImg);
            g.DrawImage(surveyImage, new RectangleF(0, 0, retImg.Width, retImg.Height), new Rectangle(0, 0, surveyImage.Width, retImg.Height), GraphicsUnit.Pixel);
            g.Dispose();
            return retImg;
        }

        private ResultDocument.TSurveyFormat ParseSurveyFormat(Survey s, int elemNum)
        {
            ResultDocument.TSurveyFormat Survey = new ResultDocument.TSurveyFormat();
            Survey.ElementNum = elemNum;
            if (s.HasCaption)
                Survey.CaptionText = s.Caption.Text;
            else
                Survey.CaptionText = String.Empty;
            Survey.Questions = new ResultDocument.TSurveyQuestionFormat[s.SurveyItems.Length];
            for (int ctr = 0; ctr < s.SurveyItems.Length; ctr++)
            {
                ResultDocument.TSurveyQuestionFormat sqf = new ResultDocument.TSurveyQuestionFormat();
                sqf.ResponseType = (ResultDocument.TResponseType)Enum.Parse(typeof(ResultDocument.TResponseType), s.SurveyItems[ctr].Response.ResponseType.ToString());
                switch (s.SurveyItems[ctr].Response.ResponseType)
                {
                    case ResponseType.None:
                        sqf.QuestionText = s.SurveyItems[ctr].Text;
                        break;

                    case ResponseType.Boolean:
                        sqf.QuestionText = s.SurveyItems[ctr].Text;
                        sqf.ResponseSummary = String.Format("1 : {0}, 0 : {1}", ((ResultData.Boolean)s.SurveyItems[ctr].Response).TrueStatement,
                                ((ResultData.Boolean)s.SurveyItems[ctr].Response).FalseStatement);
                        sqf.Choices = new ResultDocument.TChoiceFormat[2];
                        sqf.Choices[0] = new ResultDocument.TChoiceFormat();
                        sqf.Choices[1] = new ResultDocument.TChoiceFormat();
                        sqf.Choices[0].Text = ((ResultData.Boolean)s.SurveyItems[ctr].Response).TrueStatement;
                        sqf.Choices[1].Text = ((ResultData.Boolean)s.SurveyItems[ctr].Response).FalseStatement;
                        sqf.Choices[0].Value = "1";
                        sqf.Choices[1].Value = "0";
                        break;

                    case ResponseType.BoundedLength:
                        sqf.QuestionText = s.SurveyItems[ctr].Text;
                        sqf.ResponseSummary = String.Format("Between {0} and {1} characters of text.", ((BoundedLength)s.SurveyItems[ctr].Response).MinLength,
                                ((BoundedLength)s.SurveyItems[ctr].Response).MaxLength);
                        break;

                    case ResponseType.BoundedNumber:
                        BoundedNumber bnr = (BoundedNumber)s.SurveyItems[ctr].Response;
                        sqf.QuestionText = s.SurveyItems[ctr].Text;
                        sqf.ResponseSummary = String.Format("A number between {0} and {1}.", bnr.MinValue, bnr.MaxValue);
                        break;

                    case ResponseType.Date:
                        Date dr = (Date)s.SurveyItems[ctr].Response;
                        sqf.QuestionText = s.SurveyItems[ctr].Text;
                        if (dr.HasStartDate && dr.HasEndDate)
                            sqf.ResponseSummary = String.Format("A date between {0}/{1}/{2} and {3}/{4}/{5}.", dr.StartDate.Month, dr.StartDate.Day, dr.StartDate.Year,
                                dr.EndDate.Month, dr.EndDate.Day, dr.EndDate.Year);
                        else if (!dr.HasStartDate && dr.HasEndDate)
                            sqf.ResponseSummary = String.Format("A date before {0}/{1}/{2}", dr.EndDate.Month, dr.EndDate.Day, dr.EndDate.Year);
                        else if (dr.HasStartDate && !dr.HasEndDate)
                            sqf.ResponseSummary = String.Format("A date after {0}/{1}/{2}", dr.StartDate.Month, dr.StartDate.Day, dr.StartDate.Year);
                        else
                            sqf.ResponseSummary = "A date in MM/DD/YYYY format.";
                        break;

                    case ResponseType.FixedDigit:
                        FixedDigit fdr = (FixedDigit)s.SurveyItems[ctr].Response;
                        sqf.QuestionText = s.SurveyItems[ctr].Text;
                        sqf.ResponseSummary = String.Format("A series of {0} digits.", fdr.NumDigs);
                        break;

                    case ResponseType.Likert:
                        Likert lr = (Likert)s.SurveyItems[ctr].Response;
                        sqf.QuestionText = s.SurveyItems[ctr].Text;
                        if (lr.ReverseScored)
                            sqf.ResponseSummary = String.Format("A {0}-point reverse-scored likert scale.", lr.Choices.Length);
                        else
                            sqf.ResponseSummary = String.Format("A {0}-point likert scale.", lr.Choices.Length);
                        int lCtr = lr.ReverseScored ? lr.Choices.Length : 1;
                        sqf.Choices = new ResultDocument.TChoiceFormat[lr.Choices.Length];
                        for (int ctr2 = 0; ctr2 < lr.Choices.Length; ctr2++)
                        {
                            sqf.Choices[ctr2] = new ResultDocument.TChoiceFormat();
                            sqf.Choices[ctr2].Text = lr.Choices[ctr2];
                            sqf.Choices[ctr2].Value = lCtr.ToString();
                            if (lr.ReverseScored)
                                lCtr--;
                            else
                                lCtr++;
                        }
                        break;

                    case ResponseType.MultiBoolean:
                        MultiBoolean mbr = (MultiBoolean)s.SurveyItems[ctr].Response;
                        sqf.QuestionText = s.SurveyItems[ctr].Text;
                        if ((mbr.MinSelections == 0) && (mbr.MaxSelections == mbr.Choices.Length))
                            sqf.ResponseSummary = "A selection of the following.";
                        else if ((mbr.MaxSelections != mbr.Choices.Length) && (mbr.MinSelections == 0))
                            sqf.ResponseSummary = String.Format("A selection of up to {0} of the following.", mbr.MaxSelections);
                        else if ((mbr.MinSelections != 0) && (mbr.MaxSelections == mbr.Choices.Length))
                            sqf.ResponseSummary = String.Format("A selecion of more than {0} of the following.", mbr.MinSelections);
                        else
                            sqf.ResponseSummary = String.Format("A selection of between {0} and {1} of the following.", mbr.MinSelections, mbr.MaxSelections);
                        sqf.Choices = new ResultDocument.TChoiceFormat[mbr.Choices.Length];
                        for (int ctr2 = 0; ctr2 < mbr.Choices.Length; ctr2++)
                        {
                            sqf.Choices[ctr2] = new ResultDocument.TChoiceFormat();
                            sqf.Choices[ctr2].Text = mbr.Choices[ctr2];
                            String val = String.Empty;
                            for (int ctr22 = mbr.Choices.Length - 1; ctr22 > ctr2; ctr22--)
                                val += "0";
                            val += "1";
                            for (int ctr22 = 0; ctr22 < ctr2; ctr22++)
                                val += "0";
                            sqf.Choices[ctr2].Value = val;
                        }
                        break;

                    case ResponseType.Multiple:
                        Multiple mr = (Multiple)s.SurveyItems[ctr].Response;
                        sqf.QuestionText = s.SurveyItems[ctr].Text;
                        sqf.ResponseSummary = "A multiple choice item with the following options.";
                        sqf.Choices = new ResultDocument.TChoiceFormat[mr.Choices.Length];
                        for (int ctr2 = 0; ctr2 < mr.Choices.Length; ctr2++)
                        {
                            sqf.Choices[ctr2] = new ResultDocument.TChoiceFormat();
                            sqf.Choices[ctr2].Text = mr.Choices[ctr2];
                            sqf.Choices[ctr2].Value = (ctr2 + 1).ToString();
                        }
                        break;

                    case ResponseType.RegEx:
                        RegEx rR = (RegEx)s.SurveyItems[ctr].Response;
                        sqf.QuestionText = s.SurveyItems[ctr].Text;
                        sqf.ResponseSummary = String.Format("A text response that fits the form of the regular expression /{0}/", rR.RegularExpression);
                        break;

                    case ResponseType.WeightedMultiple:
                        WeightedMultiple wmR = (WeightedMultiple)s.SurveyItems[ctr].Response;
                        sqf.QuestionText = s.SurveyItems[ctr].Text;
                        sqf.ResponseSummary = String.Format("A multiple choice item with {0} choices where each choice is assigned a numerical value", wmR.Choices.Length);
                        sqf.Choices = new ResultDocument.TChoiceFormat[wmR.Choices.Length];
                        for (int ctr2 = 0; ctr2 < wmR.Choices.Length; ctr2++)
                        {
                            sqf.Choices[ctr2] = new ResultDocument.TChoiceFormat();
                            sqf.Choices[ctr2].Text = wmR.Choices[ctr2].Text;
                            sqf.Choices[ctr2].Value = wmR.Choices[ctr2].Weight.ToString();
                        }
                        break;
                }
                Survey.Questions[ctr] = sqf;
            }
            return Survey;
        }


        private void ParseResultData(ResultData.ResultData resultData)
        {
            if (resultData.Descriptor.TokenType != ETokenType.NONE)
                ResultDocument.TokenName = resultData.Descriptor.TokenName;
            ResultDocument.NumResults = (uint)resultData.IATResults.NumResultSets; ;
            ResultDocument.NumBlockPresentations = new uint[resultData.Descriptor.ConfigFile.NumBlocks];
            ResultDocument.ItemSlideSize = new ResultDocument.TItemSlideSize();
            ResultDocument.ItemSlideSize.NumCols = (int)Math.Floor((1038096F * (float)(SlideContainer.DisplaySize.Width)) / (500F * 120237F));
            ResultDocument.ItemSlideSize.ColOffset = (int)((1038096F * (float)(SlideContainer.DisplaySize.Width) / 500F) - ((float)ResultDocument.ItemSlideSize.NumCols * 120237F));
            ResultDocument.ItemSlideSize.NumRows = (int)Math.Floor((1038096F * (float)(SlideContainer.DisplaySize.Height)) / (500F * 39927F));
            ResultDocument.ItemSlideSize.RowOffset = (int)((1038096F * (float)(SlideContainer.DisplaySize.Height) / 500F) - ((float)ResultDocument.ItemSlideSize.NumRows * 39927F));
            for (int ctr = 0; ctr < ResultDocument.NumBlockPresentations.Length; ctr++)
                ResultDocument.NumBlockPresentations[ctr] = resultData.Descriptor.ConfigFile.GetNumPresentationsInBlock(ctr + 1);
            int nValidResults = 0;
            for (int ctr = 0; ctr < resultData.IATResults.NumResultSets; ctr++)
                if (resultData.IATResults[ctr].IATScore != double.NaN)
                    nValidResults++;
            ResultDocument.NumScoredResults = (uint)nValidResults;
            ResultDocument.NumIATItems = (uint)resultData.IATConfiguration.NumItems;
            ResultDocument.NumPresentations = (uint)resultData.IATConfiguration.NumPresentations;
            ResultDocument.TestAuthor = resultData.Descriptor.TestAuthor;
            DateTime dt = DateTime.Now.ToUniversalTime();
            ResultDocument.RetrievalTime = String.Format("{0:D4}-{1:D2}-{2:D2}T{3:D2}:{4:D2}:{5:D2}Z", dt.Year, dt.Month, dt.Day, dt.Hour, dt.Minute, dt.Second);
            List<Survey> surveyList = new List<Survey>();
            List<int> nonInstSurveys = new List<int>();

            for (int ctr = 0; ctr < resultData.Descriptor.BeforeSurveys.Count; ctr++)
            {
                try
                {
                    int nQuests = 0;
                    for (int ctr2 = 0; ctr2 < resultData.Descriptor.BeforeSurveys[ctr].SurveyItems.Length; ctr2++)
                        if (resultData.Descriptor.BeforeSurveys[ctr].SurveyItems[ctr2].Response.ResponseType != ResponseType.None)
                        {
                            nQuests = 1;
                            break;
                        }
                    if (nQuests > 0)
                    {
                        surveyList.Add(resultData.Descriptor.BeforeSurveys[ctr]);
                        nonInstSurveys.Add(ctr);
                    }
                }
                catch (Exception ex) { }
            }
            int nBeforeQuestionnaires = nonInstSurveys.Count;
            for (int ctr = 0; ctr < resultData.Descriptor.AfterSurveys.Count; ctr++)
            {
                try
                {
                    int nQuests = 0;
                    for (int ctr2 = 0; ctr2 < resultData.Descriptor.AfterSurveys[ctr].SurveyItems.Length; ctr2++)
                        if (resultData.Descriptor.AfterSurveys[ctr].SurveyItems[ctr2].Response.ResponseType != ResponseType.None)
                        {
                            nQuests = 1;
                            break;
                        }
                    if (nQuests > 0)
                    {
                        surveyList.Add(resultData.Descriptor.AfterSurveys[ctr]);
                        nonInstSurveys.Add(ctr + resultData.Descriptor.BeforeSurveys.Count);
                    }
                }
                catch (Exception ex) { }
            }
            ResultDocument.SurveyDesign = new ResultDocument.SurveyFormats();
            ResultDocument.SurveyDesign.SurveyFormat = new ResultDocument.TSurveyFormat[surveyList.Count];
            for (int ctr = 0; ctr < nonInstSurveys.Count; ctr++)
                ResultDocument.SurveyDesign.SurveyFormat[ctr] = ParseSurveyFormat(surveyList[ctr], ctr + 1 + ((ctr >= resultData.Descriptor.BeforeSurveys.Count) ? 1 : 0));

            ResultDocument.TestResult = new ResultDocument.TTestResult[resultData.IATResults.NumResultSets];
            for (int ctr = 0; ctr < resultData.IATResults.NumResultSets; ctr++)
            {
                ResultDocument.TestResult[ctr] = new ResultDocument.TTestResult();
                if (resultData.Descriptor.TokenType == ETokenType.NONE)
                    ResultDocument.TestResult[ctr].Token = null;
                else
                {
                    ResultDocument.TestResult[ctr].Token = new ResultDocument.CDATA();
                    ResultDocument.TestResult[ctr].Token.Content = resultData.IATResults[ctr].Token;
                }
                ResultDocument.TestResult[ctr].IATResult = new ResultDocument.TIATResult();
                ResultDocument.TestResult[ctr].IATResult.ElementNum = resultData.Descriptor.BeforeSurveys.FindAll(s => s.NumQuestions > 0).Count + 1;
                if (resultData.IATResults[ctr].IATScore == Double.NaN)
                    ResultDocument.TestResult[ctr].IATResult.IATScore = double.NaN;
                else
                    ResultDocument.TestResult[ctr].IATResult.IATScore = resultData.IATResults[ctr].IATScore;
                ResultDocument.TestResult[ctr].IATResult.IATResponse = new ResultDocument.TIATResponse[resultData.IATConfiguration.NumPresentations];
                for (int ctr2 = 0; ctr2 < resultData.IATResults[ctr].IATResponse.NumItems; ctr2++)
                {
                    ResultDocument.TestResult[ctr].IATResult.IATResponse[ctr2] = new ResultDocument.TIATResponse();
                    ResultDocument.TestResult[ctr].IATResult.IATResponse[ctr2].Error = resultData.IATResults[ctr].IATResponse[ctr2].Error;
                    ResultDocument.TestResult[ctr].IATResult.IATResponse[ctr2].ItemNum = (uint)resultData.IATResults[ctr].IATResponse[ctr2].ItemNumber;
                    ResultDocument.TestResult[ctr].IATResult.IATResponse[ctr2].Latency = (uint)resultData.IATResults[ctr].IATResponse[ctr2].ResponseTime;
                }
                ResultDocument.TestResult[ctr].SurveyResults = new ResultDocument.TSurveyResponse[nonInstSurveys.Count];
                for (int ctr2 = 0; ctr2 < nonInstSurveys.Count; ctr2++)
                {
                    ResultDocument.TestResult[ctr].SurveyResults[ctr2] = new ResultDocument.TSurveyResponse();
                    ResultDocument.TestResult[ctr].SurveyResults[ctr2].ElementNum = ctr2 + 1 + ((nonInstSurveys[ctr2] < resultData.Descriptor.BeforeSurveys.Count) ? 0 : 1);
                    ResultDocument.TestResult[ctr].SurveyResults[ctr2].Answer = new string[surveyList[ctr2].NumItems];
                    for (int ctr3 = 0; ctr3 < surveyList[ctr2].NumItems; ctr3++)
                    {
                        if (nonInstSurveys[ctr2] < resultData.Descriptor.BeforeSurveys.Count)
                        {
                            ISurveyItemResponse resp = resultData.IATResults[ctr].BeforeSurveys[nonInstSurveys[ctr2]][ctr3];
                            if (resp.IsBlank)
                                ResultDocument.TestResult[ctr].SurveyResults[ctr2].Answer[ctr3] = Properties.Resources.sUnanswered;
                            else if (resp.WasForceSubmitted)
                                ResultDocument.TestResult[ctr].SurveyResults[ctr2].Answer[ctr3] = Properties.Resources.sForceSubmitted;
                            else
                                ResultDocument.TestResult[ctr].SurveyResults[ctr2].Answer[ctr3] = resp.Value;
                        }
                        else
                        {
                            ISurveyItemResponse resp = resultData.IATResults[ctr].AfterSurveys[nonInstSurveys[ctr2] - resultData.Descriptor.BeforeSurveys.Count][ctr3];
                            if (resp.IsBlank)
                                ResultDocument.TestResult[ctr].SurveyResults[ctr2].Answer[ctr3] = Properties.Resources.sUnanswered;
                            else if (resp.WasForceSubmitted)
                                ResultDocument.TestResult[ctr].SurveyResults[ctr2].Answer[ctr3] = Properties.Resources.sForceSubmitted;
                            else
                                ResultDocument.TestResult[ctr].SurveyResults[ctr2].Answer[ctr3] = resp.Value;
                        }
                    }
                }
            }
            var slideNums = (from tr in ResultDocument.TestResult select from ir in tr.IATResult.IATResponse select ir.ItemNum + 1).Aggregate((a1, a2) => a1.Concat(a2)).Cast<int>().Distinct().OrderBy(i => i).ToList();
            ResultDocument.ItemSlide = new ResultDocument.TItemSlide[slideNums.Count()];
            for (int ctr = 0; ctr < ResultDocument.ItemSlide.Length; ctr++)
            {
                ResultDocument.ItemSlide[ctr].SlideNum = slideNums[ctr];
                ResultDocument.ItemSlide[ctr].ItemNum = ctr;
            }
        }

        public CResultDocument()
        {
        }

        public void SetResultData(ResultData.ResultData resultData, CItemSlideContainer slideContainer)
        {
            try
            {
                SlideContainer = slideContainer;
                BuildTitlePage(resultData);
                ParseResultData(resultData);
                XmlSerializer ser = new XmlSerializer(typeof(ResultDocument.ResultDocument));
                ResultXML = new MemoryStream();
                ser.Serialize(ResultXML, ResultDocument);
                ResultXML.Seek(0, SeekOrigin.Begin);
                ResponseDoc.Load(ResultXML);
            }
            catch (Exception ex)
            {
                ErrorReporter.ReportError(new CReportableException("Error parsing result data", ex));
            }
        }

        private void LoadTransforms(List<int> itrCtr, List<int> itrVal)
        {
            XmlDocument transDoc = new XmlDocument();
            transDoc.Load(new StringReader(Properties.Resources.ExcelTrans));
            TransformList.Clear();
            foreach (XmlNode n in transDoc.SelectNodes("//ExcelAddendumComponent"))
                TransformList.Add(new ExcelAddendumComponent(n));
            foreach (XmlNode n in transDoc.SelectNodes("//ExcelCopyComponent"))
                TransformList.Add(new ExcelCopyComponent(n));
            foreach (XmlNode n in transDoc.SelectNodes("//ExcelTransformComponent"))
                TransformList.Add(new ExcelTransformComponent(n));
            foreach (XmlNode n in transDoc.SelectNodes("//ExcelMultiTransformComponent"))
                TransformList.Add(new ExcelMultiTransformComponent(n, itrCtr, itrVal));
        }

        public void WaitOnTransforms()
        {
            //         TransformInitEvent.WaitOne();
        }

        private bool bAbortExport = false;

        private void Abort(object sender, EventArgs e)
        {
            MainForm.BeginInvoke(new Action<String, String>(MainForm.OnDisplayMessageBox), "Canceling export job", String.Empty);
            ExportWorker.CancelAsync();
            if (TransformWorkers != null)
                TransformWorkers.CancelJob();
        }

        public bool BeginWriteToFile(String filename)
        {
            ExportCompleteEvent = new ManualResetEvent(false);
            ExportWorker = new BackgroundWorker();
            ExportWorker.WorkerSupportsCancellation = true;
            ExportWorker.DoWork += (o, a) => { WriteToFile(filename, (BackgroundWorker)o); };
            ExportWorker.RunWorkerCompleted += (o, a) => { ExportCompleteEvent.Set(); };
            ExportWorker.RunWorkerAsync();
            ExportCompleteEvent.WaitOne();
            Action endProgressUse = new Action(MainForm.EndProgressBarUse);
            MainForm.Invoke(endProgressUse);
            return bCompleted;
        }

        private void WriteToFile(String filename, BackgroundWorker worker)
        {
            try
            {
                var itemNums = (from tr in ResultDocument.TestResult select from ir in tr.IATResult.IATResponse select ir.ItemNum).
                    Aggregate((a1, a2) => a1.Concat(a2)).Distinct().Cast<int>().OrderBy(i => i);
                var slideNums = (from resource in SlideContainer.SlideManifest.ResourceReferences select resource)
                    .Where(rr => rr.ReferenceIds.Intersect(itemNums).Count() > 0).Select(rr => rr.ResourceId).Distinct().OrderBy(i => i);
                var indexes = from sn in slideNums select slideNums.ToList().IndexOf(sn);
//                TItemSlideEntry[] slideEntries = SlideContainer.SlideManifest.ItemSlideEntries;
  //              var slideNdxs = from i in Items select from ise in slideEntries select slideEntries.ToList().IndexOf(ise);
    //            var SlideEntries = from i in Items select (from isl in SlideContainer.SlideManifest.ItemSlideEntries where isl.Items.Contains(i) select isl).First();
                bAbortExport = false;
                LoadTransforms(itemNums.ToList(), indexes.ToList());
                ZipPackage outZip = (ZipPackage)Package.Open(filename, FileMode.Create);
                int nTransforms = 0;
                nTransforms = TransformList.Count();
                Action<EventHandler, IATConfigMainForm.EProgressBarUses> del = new Action<EventHandler, IATConfigMainForm.EProgressBarUses>(MainForm.BeginProgressBarUse);
                MainForm.Invoke(del, new EventHandler(Abort), IATConfigMainForm.EProgressBarUses.ExportData);
                Action<int, int> setRangeDel = (Action<int, int>)MainForm.SetProgressRange;
                MainForm.Invoke(setRangeDel, 0, (TransformList.OfType<ExcelMultiTransformComponent>().Count() * (slideNums.Count() - 1) + TransformList.Count()));
                Action incProgress = new Action(() => MainForm.BeginInvoke(new Action<int>(MainForm.ProgressIncrement), 1));
                ManualResetEvent transformEvent = new ManualResetEvent(false);
                TransformWorkers = new WorkerPool(() => { transformEvent.Set(); });
                foreach (ExcelTransform et in TransformList)
                {
                    et.Transform(ResponseDoc, outZip, incProgress, TransformWorkers);
                    if (worker.CancellationPending)
                    {
                        transformEvent.WaitOne();
                        outZip.Close();
                        File.Delete(filename);
                        return;
                    }
                }
                TransformWorkers.EndQueue();
                ZipPackagePart zpp;
                int ctr = 0;
                for (ctr = 0; ctr < TitlePageImages.Count; ctr++)
                {
                    lock (ExcelTransform.TransformLock)
                    {
                        zpp = (ZipPackagePart)outZip.CreatePart(PackUriHelper.CreatePartUri(new Uri(String.Format("/xl/media/image{0}.png", ctr + 1), UriKind.Relative)), "image/png");
                        TitlePageImages[ctr].Save(zpp.GetStream(), System.Drawing.Imaging.ImageFormat.Png);
                        zpp.GetStream().Flush();
                        zpp.GetStream().Close();
                    }
                }
                List<String> savedImgs = new List<String>();
                ctr = TitlePageImages.Count + 1;
                foreach (var sn in slideNums)
                {
                    lock (ExcelTransform.TransformLock)
                    {
                        zpp = (ZipPackagePart)outZip.CreatePart(PackUriHelper.CreatePartUri(new Uri(String.Format("/xl/media/image{0}.jpg", ctr.ToString()), UriKind.Relative)), System.Net.Mime.MediaTypeNames.Image.Jpeg);
                        Image img = SlideContainer.SlideDictionary[sn].FullSizedImage;
                        img.Save(zpp.GetStream(), System.Drawing.Imaging.ImageFormat.Jpeg);
                        zpp.GetStream().Flush();
                        zpp.GetStream().Close();
                    }
                }
                transformEvent.WaitOne();
                outZip.Close();
                if (worker.CancellationPending)
                {
                    File.Delete(filename);
                    return;
                }
                bCompleted = true;
            }
            catch (Exception ex)
            {
                ErrorReporter.ReportError(new CReportableException("Error exporting to Excel", ex));
            }
        }
    }
}

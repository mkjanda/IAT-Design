using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using System.IO;
using IATClient.IATResultSetNamespaceV2;
using Saxon.Api;
using System.IO.Packaging;
using System.Security.Cryptography;
using System.Drawing;

namespace IATClient
{

    class CResultDocument
    {
        class ExcelTransform
        {
            private static byte[] DESKeyBytes = { 0x1C, 0x39, 0xA3, 0xF2, 0x2C, 0x13, 0x45, 0x21 };
            private static byte[] IVBytes = { 0x0A, 0x47, 0x99, 0xF3, 0x1A, 0x42, 0x79, 0x01 };
            private static DESCryptoServiceProvider desCrypt = new DESCryptoServiceProvider();
            public enum EAction { Copy, UnwrapTransform, Transform };
            public EAction Action;
            public String Directory;
            public String FileName;
            public MemoryStream Content = new MemoryStream();
        
            public ExcelTransform(XmlNode transNode)
            {
                FromBase64Transform from64 = new FromBase64Transform();
                ICryptoTransform desTrans = desCrypt.CreateDecryptor(DESKeyBytes, IVBytes);
                CryptoStream cStream = new CryptoStream(Content, desTrans, CryptoStreamMode.Write);
                CryptoStream bStream = new CryptoStream(cStream, from64, CryptoStreamMode.Write);
                StreamWriter txtWriter = new StreamWriter(bStream);
                txtWriter.Write(transNode.InnerText);
                txtWriter.Flush();
                bStream.FlushFinalBlock();
                Content.Seek(0, SeekOrigin.Begin);
                Action = (EAction)Enum.Parse(typeof(EAction), transNode.Attributes["Action"].Value);
                Directory = transNode.Attributes["Directory"].Value;
                if (Action != EAction.UnwrapTransform)
                    FileName = transNode.Attributes["FileName"].Value;
            }


            public void Transform(XmlDocument responseDoc, ZipPackage package)
            {
                ZipPackagePart zpp;
                Processor xsltProc;
                XsltCompiler compiler;
                XsltTransformer trans;
                TextReader txtReader;
                TextWriter txtWriter;
                Serializer ser;
                XdmNode inputNode;
                switch (Action)
                {
                    case EAction.Copy:
                        zpp = (ZipPackagePart)package.CreatePart(new Uri(Directory + FileName, UriKind.Relative), System.Net.Mime.MediaTypeNames.Application.Octet);
                        zpp.GetStream().Write(Content.ToArray(), 0, (int)Content.Length);
                        zpp.GetStream().Flush();
                        break;

                    case EAction.Transform:
                        zpp = (ZipPackagePart)package.CreatePart(PackUriHelper.CreatePartUri(new Uri(Directory  + FileName, UriKind.Relative)), System.Net.Mime.MediaTypeNames.Text.Xml);
                        txtWriter = new StreamWriter(zpp.GetStream(), Encoding.UTF8);
                        xsltProc = new Processor();
                        compiler = xsltProc.NewXsltCompiler();
                        txtReader = new StreamReader(Content);
                        trans = compiler.Compile(txtReader).Load();
                        inputNode = xsltProc.NewDocumentBuilder().Build(responseDoc.DocumentElement);
                        trans.InitialContextNode = inputNode;
                        ser = xsltProc.NewSerializer(txtWriter);
                        trans.Run(ser);
                        txtWriter.Flush();
                        break;

                    case EAction.UnwrapTransform:
                        xsltProc = new Processor();        
                        compiler = xsltProc.NewXsltCompiler();
                        txtReader = new StreamReader(Content, Encoding.UTF8);
                        trans = compiler.Compile(txtReader).Load();
                        txtWriter = new StringWriter();
                        ser = xsltProc.NewSerializer(txtWriter);
                        inputNode = xsltProc.NewDocumentBuilder().Build(responseDoc.DocumentElement);
                        trans.InitialContextNode = inputNode;
                        trans.Run(ser);
                        txtWriter.Flush();
                        XmlDocument xDoc = new XmlDocument();
                        xDoc.Load(new StringReader(txtWriter.ToString()));
                        foreach (XmlNode node in xDoc.DocumentElement.ChildNodes) {
                            MemoryStream memStream = new MemoryStream(Encoding.UTF8.GetBytes(node.InnerXml));
                            zpp = (ZipPackagePart)package.CreatePart(new Uri(node.Attributes["Path"].Value, UriKind.Relative), System.Net.Mime.MediaTypeNames.Text.Xml);
                            zpp.GetStream().Write(memStream.ToArray(), 0, (int)memStream.Length);
                            zpp.GetStream().Flush();
                        }
                        break;
                }
            }
        }
            


        private XmlDocument ResponseDoc = new XmlDocument();
        private IATClient.ResponseDocument.ResultDocument ResultDocument;
        private MemoryStream ResultXML = new MemoryStream();
        private List<ExcelTransform> TransformList = new List<ExcelTransform>();        
        private Package XLSXPackage = null;
        private List<Image> ItemSlides;
        private IATClient.ResponseDocument.TSurveyFormat ParseSurveyFormat(IATSurveyFile.Survey s, int elemNum)
        {
            IATClient.ResponseDocument.TSurveyFormat Survey = new IATClient.ResponseDocument.TSurveyFormat();
            Survey.ElementNum = (uint)elemNum;
            if (s.HasCaption)
                Survey.CaptionText = s.Caption.Text;
            else
                Survey.CaptionText = String.Empty;
            Survey.Questions = new IATClient.ResponseDocument.TSurveyQuestionFormat[s.SurveyItems.Length];
            for (int ctr = 0; ctr < s.SurveyItems.Length; ctr++)
            {
                IATClient.ResponseDocument.TSurveyQuestionFormat sqf = new IATClient.ResponseDocument.TSurveyQuestionFormat();
                sqf.ResponseType = (IATClient.ResponseDocument.TResponseType)Enum.Parse(typeof(IATClient.ResponseDocument.TResponseType), s.SurveyItems[ctr].Response.ResponseType.ToString());
                switch (s.SurveyItems[ctr].Response.ResponseType)
                {
                    case IATClient.IATSurveyFile.ResponseType.None:
                        sqf.QuestionText = s.SurveyItems[ctr].Text;
                        break;

                    case IATClient.IATSurveyFile.ResponseType.Boolean:
                        sqf.QuestionText = s.SurveyItems[ctr].Text;
                        sqf.ResponseSummary = String.Format("1 : {0}, 0 : {1}", ((IATSurveyFile.Boolean)s.SurveyItems[ctr].Response).TrueStatement,
                                ((IATSurveyFile.Boolean)s.SurveyItems[ctr].Response).FalseStatement);
                        sqf.Choices = new IATClient.ResponseDocument.TChoiceFormat[2];
                        sqf.Choices[0] = new IATClient.ResponseDocument.TChoiceFormat();
                        sqf.Choices[1] = new IATClient.ResponseDocument.TChoiceFormat();
                        sqf.Choices[0].Text = ((IATSurveyFile.Boolean)s.SurveyItems[ctr].Response).TrueStatement;
                        sqf.Choices[1].Text = ((IATSurveyFile.Boolean)s.SurveyItems[ctr].Response).FalseStatement;
                        sqf.Choices[0].Value = "1";
                        sqf.Choices[1].Value = "0";
                        break;

                    case IATClient.IATSurveyFile.ResponseType.BoundedLength:
                        sqf.QuestionText = s.SurveyItems[ctr].Text;
                        sqf.ResponseSummary = String.Format("Between {0} and {1} characters of text.", ((IATSurveyFile.BoundedLength)s.SurveyItems[ctr].Response).MinLength,
                                ((IATSurveyFile.BoundedLength)s.SurveyItems[ctr].Response).MaxLength);
                        break;

                    case IATClient.IATSurveyFile.ResponseType.BoundedNum:
                        IATSurveyFile.BoundedNum bnr = (IATSurveyFile.BoundedNum)s.SurveyItems[ctr].Response;
                        sqf.QuestionText = s.SurveyItems[ctr].Text;
                        sqf.ResponseSummary = String.Format("A number between {0} and {1}.", bnr.MinValue, bnr.MaxValue);
                        break;

                    case IATClient.IATSurveyFile.ResponseType.Date:
                        IATSurveyFile.Date dr = (IATSurveyFile.Date)s.SurveyItems[ctr].Response;
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

                    case IATClient.IATSurveyFile.ResponseType.FixedDig:
                        IATSurveyFile.FixedDig fdr = (IATSurveyFile.FixedDig)s.SurveyItems[ctr].Response;
                        sqf.QuestionText = s.SurveyItems[ctr].Text;
                        sqf.ResponseSummary = String.Format("A series of {0} digits.", fdr.NumDigs);
                        break;

                    case IATClient.IATSurveyFile.ResponseType.Likert:
                        IATSurveyFile.Likert lr = (IATSurveyFile.Likert)s.SurveyItems[ctr].Response;
                        sqf.QuestionText = s.SurveyItems[ctr].Text;
                        if (lr.ReverseScored)
                            sqf.ResponseSummary = String.Format("A {0}-point reverse-scored likert scale.", lr.Choices.Length);
                        else
                            sqf.ResponseSummary = String.Format("A {0}-point likert scale.", lr.Choices.Length);
                        int lCtr = lr.ReverseScored ? lr.Choices.Length : 1;
                        sqf.Choices = new IATClient.ResponseDocument.TChoiceFormat[lr.Choices.Length];
                        for (int ctr2 = 0; ctr2 < lr.Choices.Length; ctr2++)
                        {
                            sqf.Choices[ctr2] = new IATClient.ResponseDocument.TChoiceFormat();
                            sqf.Choices[ctr2].Text = lr.Choices[ctr2];
                            sqf.Choices[ctr2].Value = lCtr.ToString();
                            if (lr.ReverseScored)
                                lCtr--;
                            else
                                lCtr++;
                        }
                        break;

                    case IATClient.IATSurveyFile.ResponseType.MultiBoolean:
                        IATSurveyFile.MultiBoolean mbr = (IATSurveyFile.MultiBoolean)s.SurveyItems[ctr].Response;
                        sqf.QuestionText = s.SurveyItems[ctr].Text;
                        if ((mbr.MinSelections == 0) && (mbr.MaxSelections == mbr.Choices.Length))
                            sqf.ResponseSummary = "A selection of the following.";
                        else if ((mbr.MaxSelections != mbr.Choices.Length) && (mbr.MinSelections == 0))
                            sqf.ResponseSummary = String.Format("A selection of up to {0} of the following.", mbr.MaxSelections);
                        else if ((mbr.MinSelections != 0) && (mbr.MaxSelections == mbr.Choices.Length))
                            sqf.ResponseSummary = String.Format("A selecion of more than {0} of the following.", mbr.MinSelections);
                        else
                            sqf.ResponseSummary = String.Format("A selection of between {0} and {1} of the following.", mbr.MinSelections, mbr.MaxSelections);
                        sqf.Choices = new IATClient.ResponseDocument.TChoiceFormat[mbr.Choices.Length];
                        for (int ctr2 = 0; ctr2 < mbr.Choices.Length; ctr2++)
                        {
                            sqf.Choices[ctr2] = new IATClient.ResponseDocument.TChoiceFormat();
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

                    case IATClient.IATSurveyFile.ResponseType.Multiple:
                        IATSurveyFile.Multiple mr = (IATSurveyFile.Multiple)s.SurveyItems[ctr].Response;
                        sqf.QuestionText = s.SurveyItems[ctr].Text;
                        sqf.ResponseSummary = "A multiple choice item with the following options.";
                        sqf.Choices = new IATClient.ResponseDocument.TChoiceFormat[mr.Choices.Length];
                        for (int ctr2 = 0; ctr2 < mr.Choices.Length; ctr2++)
                        {
                            sqf.Choices[ctr2] = new IATClient.ResponseDocument.TChoiceFormat();
                            sqf.Choices[ctr2].Text = mr.Choices[ctr2];
                            sqf.Choices[ctr2].Value = (ctr2 + 1).ToString();
                        }
                        break;

                    case IATClient.IATSurveyFile.ResponseType.RegEx:
                        IATSurveyFile.RegEx rR = (IATSurveyFile.RegEx)s.SurveyItems[ctr].Response;
                        sqf.QuestionText = s.SurveyItems[ctr].Text;
                        sqf.ResponseSummary = String.Format("A text response that fits the form of the regular expression /{0}/", rR.RegEx1);
                        break;

                    case IATClient.IATSurveyFile.ResponseType.WeightedMultiple:
                        IATSurveyFile.WeightedMultiple wmR = (IATSurveyFile.WeightedMultiple)s.SurveyItems[ctr].Response;
                        sqf.QuestionText = s.SurveyItems[ctr].Text;
                        sqf.ResponseSummary = String.Format("A multiple choice item with {0} choices where each choice is assigned a numerical value");
                        sqf.Choices = new IATClient.ResponseDocument.TChoiceFormat[wmR.Choices.Length];
                        for (int ctr2 = 0; ctr2 < wmR.Choices.Length; ctr2++)
                        {
                            sqf.Choices[ctr2] = new IATClient.ResponseDocument.TChoiceFormat();
                            sqf.Choices[ctr2].Text = wmR.Choices[ctr2].Text;
                            sqf.Choices[ctr2].Value = wmR.Choices[ctr2].Weight.ToString();
                        }
                        break;
                }
                Survey.Questions[ctr] = sqf;
            }
            return Survey;
        }


        private void ParseResultData(CResultData resultData)
        {
            ResultDocument = new IATClient.ResponseDocument.ResultDocument();
            ResultDocument.NumResults = (uint)resultData.ResultDescriptor.NumResults;
            int nValidResults = 0;
            for (int ctr = 0; ctr < resultData.ResultDescriptor.NumResults; ctr++)
                if (resultData.IATResults[ctr].IATScore != double.NaN)
                    nValidResults++;
            ResultDocument.NumScoredResults = (uint)nValidResults;
            ResultDocument.NumIATItems = (uint)resultData.ResultDescriptor.ConfigFile.CountIATItems();
            ResultDocument.NumPresentations = (uint)resultData.ResultDescriptor.ConfigFile.TotalPresentations;
            ResultDocument.TestAuthor = resultData.ResultDescriptor.GetMiscValue("TestAuthor");
            DateTime dt = DateTime.Now.ToUniversalTime();
            ResultDocument.RetrievalTime = String.Format("{0}-{1}-{2}T{3}:{4}:{5}Z", dt.Year, dt.Month, dt.Day, dt.Hour, dt.Minute, dt.Second);
            List<IATSurveyFile.Survey> surveyList = new List<IATSurveyFile.Survey>();
            List<int> nonInstSurveys = new List<int>();
            for (int ctr = 0; ctr < resultData.ResultDescriptor.BeforeSurveys.Count; ctr++)
            {
                int nQuests = 0;
                for (int ctr2 = 0; ctr2 < resultData.ResultDescriptor.BeforeSurveys[ctr].SurveyItems.Length; ctr2++)
                    if (resultData.ResultDescriptor.BeforeSurveys[ctr].SurveyItems[ctr2].Response.ResponseType != IATClient.IATSurveyFile.ResponseType.None)
                    {
                        nQuests = 1;
                        break;
                    }
                if (nQuests > 0) {
                    surveyList.Add(resultData.ResultDescriptor.BeforeSurveys[ctr]);
                    nonInstSurveys.Add(ctr);
                }
            }
            for (int ctr = 0; ctr < resultData.ResultDescriptor.AfterSurveys.Count; ctr++)
            {
                int nQuests = 0;
                for (int ctr2 = 0; ctr2 < resultData.ResultDescriptor.AfterSurveys[ctr].SurveyItems.Length; ctr2++)
                    if (resultData.ResultDescriptor.AfterSurveys[ctr].SurveyItems[ctr2].Response.ResponseType != IATClient.IATSurveyFile.ResponseType.None)
                    {
                        nQuests = 1;
                        break;
                    }
                if (nQuests > 0) {
                    surveyList.Add(resultData.ResultDescriptor.AfterSurveys[ctr]);
                    nonInstSurveys.Add(ctr + resultData.ResultDescriptor.BeforeSurveys.Count);
                }
            }
            ResultDocument.SurveyDesign = new IATClient.ResponseDocument.TSurveyFormat[surveyList.Count];
            for (int ctr = 0; ctr < surveyList.Count; ctr++)
                ResultDocument.SurveyDesign[ctr] = ParseSurveyFormat(surveyList[ctr], ctr + 1);

            ResultDocument.TestResult = new IATClient.ResponseDocument.TTestResult[resultData.ResultDescriptor.NumResults];
            for (int ctr = 0; ctr < resultData.ResultDescriptor.NumResults; ctr++) {
                ResultDocument.TestResult[ctr] = new IATClient.ResponseDocument.TTestResult();
                ResultDocument.TestResult[ctr].IATResult = new IATClient.ResponseDocument.TIATResult();
                if (resultData.IATResults[ctr].IATScore == Double.NaN)
                    ResultDocument.TestResult[ctr].IATResult.IATScore = double.NaN;
                else
                    ResultDocument.TestResult[ctr].IATResult.IATScore = resultData.IATResults[ctr].IATScore;
                ResultDocument.TestResult[ctr].IATResult.IATResponse = new IATClient.ResponseDocument.TIATResponse[resultData.ResultDescriptor.ConfigFile.NumIATItems];
                for (int ctr2 = 0; ctr2 < resultData.IATResults[ctr].IATResults.NumItems; ctr2++) {
                    ResultDocument.TestResult[ctr].IATResult.IATResponse[ctr2] = new IATClient.ResponseDocument.TIATResponse();
                    ResultDocument.TestResult[ctr].IATResult.IATResponse[ctr2].Error = resultData.IATResults[ctr][ctr2].Error;
                    ResultDocument.TestResult[ctr].IATResult.IATResponse[ctr2].ItemNum = (uint)resultData.IATResults[ctr].IATResults[ctr2].ItemNumber;
                    ResultDocument.TestResult[ctr].IATResult.IATResponse[ctr2].Latency = (uint)resultData.IATResults[ctr].IATResults[ctr2].ResponseTime;
                }
                ResultDocument.TestResult[ctr].SurveyResults = new IATClient.ResponseDocument.TSurveyResponse[surveyList.Count];
                for (int ctr2 = 0; ctr2 < surveyList.Count; ctr2++) {
                    ResultDocument.TestResult[ctr].SurveyResults[ctr2] = new IATClient.ResponseDocument.TSurveyResponse();
                    ResultDocument.TestResult[ctr].SurveyResults[ctr2].Answer = new string[surveyList[ctr2].NumItems];
                    for (int ctr3 = 0; ctr3 < surveyList[ctr2].NumItems; ctr3++) {
                        if (nonInstSurveys[ctr2] < resultData.ResultDescriptor.BeforeSurveys.Count) 
                            ResultDocument.TestResult[ctr].SurveyResults[ctr2].Answer[ctr3] = resultData.IATResults[ctr].BeforeSurveys[nonInstSurveys[ctr2]].SurveyResults[ctr3];
                        else
                            ResultDocument.TestResult[ctr].SurveyResults[ctr2].Answer[ctr3] = resultData.IATResults[ctr].AfterSurveys[nonInstSurveys[ctr2] = resultData.ResultDescriptor.BeforeSurveys.Count].SurveyResults[ctr3];
                    
                    }
                }
            }
        }

        public CResultDocument(CResultData resultData, List<Image> itemSlides)
        {
            ParseResultData(resultData);
            XmlSerializer ser = new XmlSerializer(typeof(ResponseDocument.ResultDocument));
            ser.Serialize(ResultXML, ResultDocument);
            ResultXML.Seek(0, SeekOrigin.Begin);
            ResponseDoc.Load(ResultXML);
            XmlDocument transDoc = new XmlDocument();
            transDoc.Load(new StringReader(Properties.Resources.ExcelTrans));
            foreach (XmlNode n in transDoc.SelectNodes("//ExcelComponent"))
                TransformList.Add(new ExcelTransform(n));
            ItemSlides = itemSlides;
        }

        public void WriteToFile(String filename)
        {
            ZipPackage outZip = (ZipPackage)Package.Open(filename, FileMode.Create);
            foreach (ExcelTransform et in TransformList)
                et.Transform(ResponseDoc, outZip);
            for (int ctr = 1; ctr <= ItemSlides.Count; ctr++)
            {
                ZipPackagePart zpp = (ZipPackagePart)outZip.CreatePart(PackUriHelper.CreatePartUri(new Uri(String.Format("/xl/media/image{0}", ctr.ToString()), UriKind.Relative)), System.Net.Mime.MediaTypeNames.Image.Jpeg);
                ItemSlides[ctr - 1].Save(zpp.GetStream(), System.Drawing.Imaging.ImageFormat.Jpeg);
                zpp.GetStream().Flush();
            }
            outZip.Close();
        }
    }
}

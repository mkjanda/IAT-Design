using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using System.Net;
using System.Drawing;
using System.Windows.Forms;
using System.Xml.Serialization;
using System.Linq;
using System.Management;

namespace IATClient
{
    public class ErrorReporter
    {
        public class HandshakeException : Exception { }
        static public int Errors = 0, ErrorsReported = 0;
        ErrorReporter Reporter = new ErrorReporter();
        public static Thread ErrorReportThread { get; private set; } = null;
        private static readonly ManualResetEvent IsReportingEvent = new ManualResetEvent(true);
        public ErrorReporter() { }

        private static String ShakeHands()
        {
            WebClient downloader = new WebClient();
            downloader.Headers.Add(HttpRequestHeader.Accept, "text/plain");
            String challenge = downloader.DownloadString(Properties.Resources.sErrorReportURL);
            RSACryptoServiceProvider rsa = new RSACryptoServiceProvider();
            rsa.ImportCspBlob(Convert.FromBase64String(Properties.Resources.ErrorReportCspBlob));
            return Convert.ToBase64String(rsa.Decrypt(Convert.FromBase64String(challenge), false));
        }

        [STAThread]
        public static void ReportError(CReportableException rex)
        {
            IsReportingEvent.WaitOne();
            IsReportingEvent.Reset();
            if (CIAT.SaveFile.IsDisposing)
                return;
            Errors++;
            ErrorsReported++;
            var thread = new Thread(() =>
            {
                try
                {
                    Point ptSplash = (Application.OpenForms[Properties.Resources.sMainFormName] != null) ?
                        Application.OpenForms[Properties.Resources.sMainFormName].Location : new Point(0, 0);
                    ptSplash.Offset(200, 200);
                    ErrorReportSplash spl = new ErrorReportSplash()
                    {
                        Left = ptSplash.X,
                        Top = ptSplash.Y,
                        Topmost = true
                    };
                    spl.Show();
                    LocalStorage.RecordError(new CClientException(rex.Caption, (CReportableException)rex).GetXml());
                    Application.OpenForms[Properties.Resources.sMainFormName]?.Invoke(new Action(Application.OpenForms[Properties.Resources.sMainFormName].Close));
                    String handshakeResponse = ShakeHands();
                    WebClient uploader = new WebClient();
                    uploader.Headers.Add("response", handshakeResponse);
                    uploader.Headers.Add(HttpRequestHeader.ContentType, "text/xml");
                    uploader.Headers.Add(HttpRequestHeader.Accept, "text/xml");
                    byte[] responseBytes = uploader.UploadData(Properties.Resources.sErrorReportURL, System.Text.Encoding.Unicode.GetBytes(new CClientException(rex.Caption, rex).GetXml()));
                    XmlSerializer ser = new XmlSerializer(typeof(ErrorReportResponse));
                    MemoryStream memStream = new MemoryStream(responseBytes);
                    ErrorReportResponse response = ser.Deserialize(memStream) as ErrorReportResponse;
                    switch (response.Response)
                    {
                        case ErrorReportResponse.EResponseCode.success:
                            String recovery = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments, Environment.SpecialFolderOption.Create);
                            recovery += Path.PathSeparator + "recovery" + DateTime.Now.Year.ToString("0000") + "-" + DateTime.Now.Month.ToString("00") + "-" + DateTime.Now.Day.ToString("00") + ".iat";
                            MessageBox.Show(String.Format(Properties.Resources.sErrorReportedMessage, LocalStorage.Activation[LocalStorage.Field.ProductKey], recovery),
                                Properties.Resources.sErrorReportedCaption);
                            CIAT.SaveFile.Save(recovery);
                            break;

                        case ErrorReportResponse.EResponseCode.killFiled:
                            ErrorsReported--;
                            MessageBox.Show(response.Message, response.Caption);
                            break;

                        case ErrorReportResponse.EResponseCode.invalidHandshake:
                            ErrorsReported--;
                            MessageBox.Show(response.Message, response.Caption);
                            break;

                        case ErrorReportResponse.EResponseCode.serverError:
                            MessageBox.Show(response.Message, response.Caption);
                            ErrorsReported--;
                            break;
                    }
                }
                catch (Exception)
                {
                    ErrorsReported--;
                    ErrorReportDisplay errorReport = new ErrorReportDisplay(rex);
                    errorReport.ShowDialog();
                }
                finally
                {
                    IsReportingEvent.Set();
                    Application.OpenForms[Properties.Resources.sMainFormName]?.Invoke(new Action(Application.OpenForms[Properties.Resources.sMainFormName].Close));
                }
            });
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
        }

        private static void CollectActivationKeyOrEmail(ActivationException report, Exception ex)
        {
            ActivationExceptionClientInfoForm requestForm = new ActivationExceptionClientInfoForm(ex);
            bool reported = false;
            WebClient downloader;
            while (!reported)
            {
                requestForm.ShowDialog();
                if (requestForm.DialogResult == DialogResult.OK)
                {
                    downloader = new WebClient();
                    downloader.Headers.Add(HttpRequestHeader.Accept, "text/plain");
                    if (requestForm.ProductKey != String.Empty)
                        downloader.QueryString.Add("ProductKey", requestForm.ProductKey);
                    if (requestForm.Email != String.Empty)
                        downloader.QueryString.Add("Email", requestForm.Email);
                    String verifyResult = downloader.DownloadString(Properties.Resources.sErrorReportDataSubmissionURL);
                    reported = true;
                    if (verifyResult == "Both")
                    {
                        report.ProductKey = requestForm.ProductKey;
                        report.Email = requestForm.Email;
                    }
                    else if (verifyResult == "ProductKey") {
                        report.ProductKey = requestForm.ProductKey;
                        report.Email = null;
                    }
                    else if (verifyResult == "Email") {
                        report.Email = requestForm.Email;
                        report.ProductKey = null;
                    } else
                    {
                        report.Email = null;
                        report.ProductKey = null;
                        reported = false;
                    }
                    if (!reported)
                        MessageBox.Show("Neither the product key nor email you entered does not have a matching entry in the server database.", "No such user", MessageBoxButtons.OK);
                }
                else
                    return;
            }
        }

        [STAThread]
        public static void ReportActivationError(Exception ex)
        {
            IsReportingEvent.WaitOne();
            IsReportingEvent.Set();
            WebClient downloader;
            var scheduler = TaskScheduler.FromCurrentSynchronizationContext();
            var task = new Task(() =>
            {
                Point ptSplash = (Application.OpenForms[Properties.Resources.sMainFormName] != null) ?
                Application.OpenForms[Properties.Resources.sMainFormName].Location : new Point(0, 0);
                ptSplash.Offset(200, 200);
                ErrorReportSplash spl = new ErrorReportSplash()
                {
                    Left = ptSplash.X,
                    Top = ptSplash.Y,
                    Topmost = true
                };
                try
                {
                    ActivationException report = new ActivationException(ex);
                    String challengeResponse = ShakeHands();
                    if (report.ProductKey == null)
                        CollectActivationKeyOrEmail(report, ex);
                    else
                    {
                        downloader = new WebClient();
                        downloader.Headers.Add(HttpRequestHeader.Accept, "text/plain");
                        downloader.Headers.Add("response", challengeResponse);
                        downloader.QueryString.Add("ProductKey", LocalStorage.Activation[LocalStorage.Field.ProductKey]);
                        downloader.QueryString.Add("Email", LocalStorage.Activation[LocalStorage.Field.UserEmail]);
                        String verification = downloader.DownloadString(Properties.Resources.sErrorReportDataSubmissionURL);
                        if (verification == "Neither")
                            CollectActivationKeyOrEmail(report, ex);
                        else if (verification == "ProductKey")
                            report.Email = null;
                        else if (verification == "Email")
                            report.ProductKey = null;
                    }
                    spl.Show();
                    XmlSerializer ser = new XmlSerializer(typeof(ActivationException));
                    StringWriter textWriter = new StringWriter();
                    ser.Serialize(textWriter, report);
                    LocalStorage.RecordError(textWriter.ToString());
                    byte[] exceptionXmlBytes = System.Text.Encoding.Unicode.GetBytes(textWriter.ToString());
                    WebClient uploader = new WebClient();
                    uploader.Headers.Add(HttpRequestHeader.ContentType, "text/xml");
                    uploader.Headers.Add(HttpRequestHeader.Accept, "text/xml");
                    uploader.Headers.Add("response", challengeResponse);
                    byte[] responseBytes = uploader.UploadData(Properties.Resources.sActivationErrorReportURLTesting, exceptionXmlBytes);
                    ser = new XmlSerializer(typeof(ErrorReportResponse));
                    MemoryStream memStream = new MemoryStream(responseBytes);
                    ErrorReportResponse reportResponse = ser.Deserialize(memStream) as ErrorReportResponse;
                    switch (reportResponse.Response)
                    {
                        case ErrorReportResponse.EResponseCode.success:
                            MessageBox.Show(String.Format(Properties.Resources.sErrorReportedMessage, report.ProductKey),
                            Properties.Resources.sErrorReportedCaption);
                            break;

                        case ErrorReportResponse.EResponseCode.killFiled:
                            MessageBox.Show(reportResponse.Message, reportResponse.Caption);
                            break;

                        case ErrorReportResponse.EResponseCode.invalidHandshake:
                            MessageBox.Show(reportResponse.Message, reportResponse.Caption);
                            break;

                        case ErrorReportResponse.EResponseCode.serverError:
                            MessageBox.Show(reportResponse.Message, reportResponse.Caption);
                            break;
                    }
                }
                catch (Exception ex2)
                {
                    MessageBox.Show(Properties.Resources.sErrorReportException, Properties.Resources.sErrorReportExceptionCaption);
                }
                finally
                {
                    if (spl.IsLoaded)
                        spl.Close();
                    IsReportingEvent.Reset();
                }
            });
            task.Start(scheduler);
        }
    }
}

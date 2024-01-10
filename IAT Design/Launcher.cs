using Launcher;
using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;

namespace IATClient
{
    public class Launcher : ILauncher
    {
        private static readonly object errorReportLock = new object();
        private static void OnException(Exception ex)
        {
            if (CIAT.SaveFile == null)
                return;
            if ((CIAT.SaveFile.IsDisposing) || (CIAT.SaveFile.IsDisposed))
                return;
            if (!Monitor.TryEnter(errorReportLock))
                return;
            try
            {
                if (ex is InvalidSaveFileException)
                {
                    XDocument xDoc = new XDocument();
                    xDoc.Add(new XElement("CorruptedSaveFileReport", new XElement("ProductCode", LocalStorage.Activation[LocalStorage.Field.ProductKey]),
                        new XElement("UserName", LocalStorage.Activation[LocalStorage.Field.UserName]),
                        new XElement("UserEmail", LocalStorage.Activation[LocalStorage.Field.UserEmail])));
                    StringWriter sWriter = new StringWriter();
                    xDoc.Save(sWriter);
                    MemoryStream memStream = new MemoryStream();
                    byte[] bytes = Encoding.UTF8.GetBytes(sWriter.ToString());
                    memStream.Write(bytes, 0, bytes.Length);
                    WebClient wClient = new WebClient();
                    wClient.Headers[HttpRequestHeader.ContentType] = "text/xml";
                    wClient.UploadData(Properties.Resources.sInvalidSaveFileReportURL, memStream.ToArray());
                    MessageBox.Show("Invalid or corrupted save file.", "IAT Design");
                }
                else if (LocalStorage.Activation[LocalStorage.Field.ActivationKey] != null)
                {
                    ErrorReporter.ReportError(new CReportableException("General Application Failure", ex));
                    CIAT.SaveFile.Dispose();
                }
                else
                {
                    ErrorReporter.ReportActivationError(ex);
                }
            }
            catch (Exception ex2) { }
            Application.OpenForms[Properties.Resources.sMainFormName]?.Invoke(new Action(Application.OpenForms[Properties.Resources.sMainFormName].Close));
            Monitor.Exit(errorReportLock);
        }

        [STAThread]
        public void Launch()
        {
            Application.EnableVisualStyles();
            Application.ThreadException += new ThreadExceptionEventHandler((o, tExArgs) =>
            {
                OnException(tExArgs.Exception);
            });
            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler((o, uhExArgs) =>
            {
                OnException(uhExArgs.ExceptionObject as Exception);
            });
            TaskScheduler.UnobservedTaskException += (sender, args) => OnException(args.Exception);
            CSurvey.CompileXSLT();
            var mainWin = new IATConfigMainForm();
            mainWin.AutoScaleMode = AutoScaleMode.Dpi;
            Application.Run(mainWin);
            if (SaveFile.SaveThread != null)
                if (SaveFile.SaveThread.IsAlive)
                    SaveFile.SaveThread.Join();
            if (ErrorReporter.ErrorReportThread != null)
                if (ErrorReporter.ErrorReportThread.IsAlive)
                    ErrorReporter.ErrorReportThread.Join();
        }
    }
}


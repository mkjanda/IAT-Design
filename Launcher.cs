using Launcher;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.Threading;
using System.IO;
using System.Windows.Forms;
using System.Xml.Linq;
using System.Net;


namespace IATClient
{
    public class Launcher : ILauncher
    {
        private static readonly object errorReportLock = new object();
        private void OnException(Exception ex)
        {
            if (!Monitor.TryEnter(errorReportLock))
                return;
            try
            {
                if (ex is InvalidSaveFileException)
                {
                    IATConfigMainForm.Halt();
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
                else
                {
                    IATConfigMainForm.ShowErrorReport("General Application Failure", new CReportableException("General Application Failure", ex));
                    IATConfigMainForm.Halt();
                    CIAT.SaveFile.CreateRecovery();
                    MessageBox.Show(Properties.Resources.sRecoveryFileCreated, Properties.Resources.sRecoveryFileCreatedCaption);
                    CIAT.SaveFile.Dispose();
                }
            }
            catch (Exception ex2) { }
            Application.OpenForms[Properties.Resources.sMainFormName]?.Invoke(new Action(Application.OpenForms[Properties.Resources.sMainFormName].Close));
            Monitor.Exit(errorReportLock);
        }

        public void Launch()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.ThreadException += new ThreadExceptionEventHandler((o, tExArgs) =>
            {
                OnException(tExArgs.Exception);
            });
            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler((o, uhExArgs) =>
            {
                OnException(uhExArgs.ExceptionObject as Exception);
            });
            CSurvey.CompileXSLT();
            var mainWin = new IATConfigMainForm();
            mainWin.AutoScaleMode = AutoScaleMode.Dpi;
            Application.Run(mainWin);
            if (SaveFile.SaveThread != null)
                if (SaveFile.SaveThread.IsAlive)
                    SaveFile.SaveThread.Join();
            if (IATConfigMainForm.ErrorReportThread != null)
                if (IATConfigMainForm.ErrorReportThread.IsAlive)
                    IATConfigMainForm.ErrorReportThread.Join();
        }
    }
}


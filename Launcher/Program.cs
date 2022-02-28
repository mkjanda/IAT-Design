using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Linq;
using System.Text;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using System.Reflection;
using System.Xml;
using System.Xml.Serialization;
using Microsoft.Win32;
using System.Windows.Forms;
using System.Xml.Linq;

namespace Launcher
{
    class Program
    {
        private static byte[] DESData = { 0xAD, 0x81, 0x56, 0x1F, 0x59, 0xE1, 0x33, 0x85 };
        private static byte[] IVData = { 0x01, 0x03, 0x05, 0x03, 0x01, 0x09, 0x07, 0x05 };
        private static DownloadSplash downloadSplash = null;
        private static Manifest fileManifest;
        private static String LocalStorageDirectory { get { return Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + Path.DirectorySeparatorChar + "IATSoftware";  } }
        private static String LocalStoragePath { get { return LocalStorageDirectory + Path.DirectorySeparatorChar + "IATDesign.xml"; } }
        private static XDocument LocalStorage = null;

        [STAThread]
        public static void Main(string[] args)
        {
            Application.EnableVisualStyles();
//            Application.SetCompatibleTextRenderingDefault(false);
            if (File.Exists(LocalStoragePath))
            {
                LocalStorage = XDocument.Load(LocalStoragePath);
                if (LocalStorage.Root.Element("Version") == null)
                {
                    LocalStorage.Root.Add(new XElement("Version", Resource1.sVersion));
                    LocalStorage.Save(LocalStoragePath);
                }
            }
            else
            {
                if (!Directory.Exists(LocalStorageDirectory))
                    Directory.CreateDirectory(LocalStorageDirectory);
                LocalStorage = new XDocument();
                LocalStorage.Add(new XElement("IATDesign"));
                LocalStorage.Root.Add(new XElement("Version", Resource1.sVersion));
                LocalStorage.Root.Add(new XElement("Version_1_1_confirmed", true.ToString()));
            }
            if (File.Exists(LocalStoragePath))
                File.Delete(LocalStoragePath);
            LocalStorage.Save(LocalStoragePath);
            Assembly assembly = GetAssembly();
            if (assembly != null)
            {
                var type = assembly.GetTypes().Where(t => typeof(ILauncher).IsAssignableFrom(t)).FirstOrDefault();
                var launcher = Activator.CreateInstance(type) as ILauncher;
                launcher.Launch();
            }
        }

        private static Assembly GetAssembly()
        {
            MemoryStream memStream = null;
            try
            {
                downloadSplash = new DownloadSplash();
                downloadSplash.Show();
                String currentVersion = String.Empty, version = String.Empty;
                WebClient downloader = new WebClient();
                try
                {
                    version = LocalStorage.Root.Element("Version").Value;
                    downloader = new WebClient();
                    currentVersion = downloader.DownloadString(Resource1.sCurrentVersionURL).Trim();
                }
                catch (Exception ex)
                {
                    return GetLocalAssembly();
                }

                if (currentVersion != version)
                {
                    fileManifest = new Manifest();
                    StringReader sReader = new StringReader(downloader.DownloadString(String.Format(Resource1.sCurrentVersionManifestURL, version.Replace('.', '-'))));
                    XmlReader xReader = new XmlTextReader(sReader);
                    fileManifest.ReadXml(xReader);
                    try
                    {
                        memStream = new MemoryStream(downloader.DownloadData(new Uri(String.Format(Resource1.sCurrentVersionDownloadURL, version.Replace('.', '-')))));
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("An error occured during the update process. It is most likely that an update has been issued since you first downloaded " +
                            "the software and you have not yet activated it. Whether this is the case or not, you must download the software again " +
                            "at http://www.iatsoftware.net.", "Download Required");
                        return null;
                    }
                    if (memStream.Length != fileManifest.TotalSize)
                        throw new Exception();
                    for (int n = 0; n < fileManifest.FileCount; n++)
                    {
                        FileEntity fe = fileManifest[n];
                        WriteFileEntity(fe, memStream);
                    }
                    CleanupFiles();
                    LocalStorage.Root.Element("Version").Value = currentVersion;
                    if (File.Exists(LocalStoragePath))
                        File.Delete(LocalStoragePath);
                    LocalStorage.Save(LocalStoragePath);
         
                    downloader.Headers[HttpRequestHeader.Accept] = "text/xml";
                    String notificationXML = downloader.DownloadString(new Uri(String.Format(Resource1.sNotificationURL, version.Replace('.', '-'))));
                    XmlSerializer ser = new XmlSerializer(typeof(UpdateNotification));
                    MemoryStream updateStream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(notificationXML));
                    UpdateNotification uNote = ser.Deserialize(updateStream) as UpdateNotification;
                    updateStream.Dispose();
                    if (uNote.Notification.Count == 0)
                        return GetLocalAssembly();
                    downloadSplash.Close();
                    downloadSplash = null;
                    WebBrowser browser = new WebBrowser();
                    browser.DocumentText = uNote.UpdateNotificationHTML;
                    Form unForm = new Form();
                    unForm.Text = "Update Notification";
                    unForm.Size = new System.Drawing.Size(640, 500);
                    browser.Dock = DockStyle.Fill;
                    unForm.Controls.Add(browser);
                    unForm.ShowDialog();

                    
                }
            }
            catch (Exception ex)
            {
                downloadSplash.Close();
                MessageBox.Show("An update is available but could not be downloaded. Another attempt will be made the next time you run the software. If this error persists, you can download " +
                    "the latest version of the software at http://www.iatsoftware.net.", "Update Failed");
                return GetLocalAssembly();
            }
            finally
            {
                if (memStream != null)
                    memStream.Dispose();
                if (downloadSplash != null)
                    downloadSplash.Close();
            }
            return GetLocalAssembly();
        }

        private static void WriteFileEntity(FileEntity fe, MemoryStream memStream)
        {
            if (fe.FileEntityType == FileEntity.EFileEntityType.Directory)
            {
                if (!Directory.Exists(fe.Path))
                    Directory.CreateDirectory(fe.Path);
                ManifestDirectory md = (ManifestDirectory)fe;
                for (int n = 0; n < md.NumChildren; n++)
                    WriteFileEntity(fe, memStream);
            }
            else
            {
                if (File.Exists(fe.Path))
                {
                    File.SetAttributes(fe.Path, FileAttributes.Normal);
                    File.Move(fe.Path, fe.Path + ".bak");
                }

                FileStream fStream = new FileStream(fe.Path, FileMode.Create);
                byte[] fileData = new byte[fe.Size];
                memStream.Read(fileData, 0, (int)fe.Size);
                fStream.Write(fileData, 0, fileData.Length);
                fStream.Close();
            }
        }

        private static void CleanupFiles()
        {
            for (int ctr = 0; ctr < fileManifest.NumChildren; ctr++)
                CleanupFiles(fileManifest[ctr]);
        }

        private static void CleanupFiles(FileEntity fe)
        {
            if (fe.FileEntityType == FileEntity.EFileEntityType.Directory)
            {
                for (int ctr = 0; ctr < ((ManifestDirectory)fe).NumChildren; ctr++)
                    CleanupFiles(fe);
            }
            else
            {
                if (File.Exists(fe.Path + ".bak"))
                    File.Delete(fe.Path + ".bak");
            }
        }


        private static Assembly GetLocalAssembly()
        {
            try
            {
                return Assembly.Load("DesignBinaries");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Unable to start. Please download again.", "Error");
                return null;
            }
        }

    }
}

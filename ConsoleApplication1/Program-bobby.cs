using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Security.Policy;
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

namespace Launcher
{
    class Program
    {
        private static byte[] DESData = { 0xAD, 0x81, 0x56, 0x1F, 0x59, 0xE1, 0x33, 0x85 };
        private static byte[] IVData = { 0x01, 0x03, 0x05, 0x03, 0x01, 0x09, 0x07, 0x05 };
        private static DownloadSplash downloadSplash = null;
        private static Manifest fileManifest;

        [STAThread]
        public static void Main(string[] args)
        {
            Assembly assembly = GetAssembly();
            if (assembly != null)
            {
                ILauncher launcher = assembly.CreateInstance("IATClient.Launcher") as ILauncher;
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
                String version = GetRegistryValue("Version");
                String currentVersion;
                if (version == String.Empty)
                {
                    return GetLocalAssembly();
                }
                WebClient downloader = new WebClient();
                try
                {
                    currentVersion = downloader.DownloadString(Resource1.sCurrentVersionURL).Trim();
                }
                catch (Exception ex)
                {
                    return GetLocalAssembly();
                }

                if (currentVersion != version)
                {
                    fileManifest = new Manifest();
                    StringReader sReader = new StringReader(downloader.DownloadString(Resource1.sCurrentVersionManifestURL));
                    XmlReader xReader = new XmlTextReader(sReader);
                    fileManifest.ReadXml(xReader);
                    memStream = new MemoryStream(downloader.DownloadData(new Uri(String.Format(Resource1.sCurrentVersionDownloadURL, GetRegistryValue("IATProductCode")))));
                    if (memStream.Length != fileManifest.TotalSize)
                        throw new Exception();
                    for (int n = 0; n < fileManifest.FileCount; n++)
                    {
                        FileEntity fe = fileManifest[n];
                        WriteFileEntity(fe, memStream);
                    }
                    CleanupFiles();
                }
            }
            catch (Exception ex)
            {
                downloadSplash.Close();
                MessageBox.Show("An update is available but could not be downloaded. Another attempt will be made the next time you run the software.", "Update Failed");
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
                byte[] assmBytes = File.ReadAllBytes(Resource1.sDesignAssemblyName);
                return Assembly.Load(assmBytes);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Unable to start. Please download again.", "Error");
                return null;
            }
        }

        private static RegistryKey GetBaseKey()
        {
            RegistryKey key = Registry.CurrentUser.OpenSubKey("Software", true);
            if (key == null)
                return null;
            key = key.OpenSubKey("IATSoftware", true);
            if (key == null)
                return null;
            return key.OpenSubKey("IATClient", true);
        }

        private static String GetKeyValue(RegistryKey key, String valueName)
        {
            String str = (String)key.GetValue(valueName, null);
            if (str == null)
                return null;
            MemoryStream m = new MemoryStream(Convert.FromBase64String(str));
            m.Seek(0, SeekOrigin.Begin);
            DESCryptoServiceProvider DESCrypt = new DESCryptoServiceProvider();
            CryptoStream cStream = new CryptoStream(m, DESCrypt.CreateDecryptor(DESData, IVData), CryptoStreamMode.Read);
            StreamReader reader = new StreamReader(cStream, Encoding.UTF8);
            String result = reader.ReadToEnd();
            cStream.Close();
            return result;
        }

        private static String GetRegistryValue(String subKeyName)
        {
            RegistryKey baseKey = GetBaseKey();
            if (baseKey == null)
                return String.Empty;
            String val = GetKeyValue(baseKey, subKeyName);
            if (val == null)
                return "empty";
            return val;
        }

    }
}

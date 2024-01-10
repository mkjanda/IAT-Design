﻿using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.IO;
using System.IO.Packaging;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.Xml.Linq;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace IATClient
{
    public interface IPackagePart : IDisposable
    {
        Uri URI { get; set; }
        Type BaseType { get; }
        String MimeType { get; }
    }

    public class InvalidSaveFileException : Exception { }

    public class SaveFile : IDisposable
    {
        public class SaveFileMetaData : IPackagePart
        {
            [Serializable]
            public class HistoryEntry
            {
                [XmlElement(ElementName = "TimeOpened", Form = XmlSchemaForm.Unqualified)]
                public String TimeOpened { get; set; } = DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToShortTimeString();
                [XmlElement(ElementName = "ProductKey", Form = XmlSchemaForm.Unqualified)]
                public String ProductKey { get; set; } = "N/A";
                [XmlElement(ElementName = "ErrorCount", Form = XmlSchemaForm.Unqualified)]
                public int ErrorCount { get; set; } = -1;
                [XmlElement(ElementName = "ErrorsReported", Form = XmlSchemaForm.Unqualified)]
                public int ErrorsReported { get; set; } = -1;
                [XmlElement(ElementName = "SaveFileVersion", Form = XmlSchemaForm.Unqualified)]
                public String Version { get; set; } = null;

                public HistoryEntry()
                {
                    Version = LocalStorage.Activation[LocalStorage.Field.Version];
                    ProductKey = LocalStorage.Activation[LocalStorage.Field.ProductKey];
                    ErrorCount = IATConfigMainForm.Errors;
                    ErrorsReported = IATConfigMainForm.ErrorsReported;
                }

                public HistoryEntry(XElement root)
                {
                    if (root.Element("Timestamp") != null)
                        TimeOpened = root.Element("Timestamp").Value;
                    if (root.Element("Version") != null)
                        Version = root.Element("Version").Value;
                    if (root.Element("ErrorCount") != null)
                        ErrorCount = Convert.ToInt32(root.Element("ErrorCount").Value);
                    if (root.Element("ErrorsReported") != null)
                        ErrorsReported = Convert.ToInt32(root.Element("ErrorsReported").Value);
                    if (root.Element("ProductKey") != null)
                        ProductKey = root.Element("ProductKey").Value;
                }

                public void AddToXml(XElement parent)
                {
                    parent.Add(new XElement("HistoryEntry", new XElement("Timestamp", TimeOpened), new XElement("Version", Version), new XElement("ErrorCount", ErrorCount),
                        new XElement("ErrorsReported", ErrorsReported.ToString()), new XElement("ProductKey", ProductKey)));
                }
            }


            public DateTime TimeOpened { get; private set; } = DateTime.UtcNow;
            public ConcurrentDictionary<String, List<int>> UriCounters { get; private set; } = new ConcurrentDictionary<String, List<int>>();
            public List<HistoryEntry> History { get; private set; } = new List<HistoryEntry>();
            public Uri URI { get; set; }
            public Type BaseType { get { return typeof(SaveFileMetaData); } }
            public String MimeType { get { return "text/xml+" + typeof(SaveFileMetaData).ToString(); } }
            public String IATRelId { get; set; } = String.Empty;
            private SaveFile SaveFile { get; set; } = null;
            public CVersion Version { get; private set; } = new CVersion();
            public SaveFileMetaData(SaveFile saveFile)
            {
                URI = PackUriHelper.CreatePartUri(new Uri(typeof(SaveFileMetaData).ToString() + "/" + typeof(SaveFileMetaData).ToString() + "1.xml", UriKind.Relative));
                saveFile.SavePackage.CreatePart(URI, MimeType);
                UriCounters[typeof(SaveFileMetaData).ToString()] = new List<int>(new int[] { 1 });
                saveFile.CreatePackageLevelRelationship(URI, typeof(SaveFileMetaData));
                SaveFile = saveFile;
            }

            public SaveFileMetaData(SaveFile saveFile, Uri u)
            {
                SaveFile = saveFile;
                this.URI = u;
                UriCounters[typeof(SaveFileMetaData).ToString()] = new List<int>(new int[] { 1 });
                Load(SaveFile.SavePackage);
            }


            public void Save()
            {
                XDocument xDoc = new XDocument();
                xDoc.Add(new XElement("MetaData"));
                xDoc.Root.Add(new XElement("IATRelId", IATRelId));
                new HistoryEntry().AddToXml(xDoc.Root);
                foreach (HistoryEntry hist in History)
                    hist.AddToXml(xDoc.Root);
                XElement xElem = new XElement("UriCounters");
                foreach (String t in UriCounters.Keys)
                {
                    XElement uriElem = new XElement("UriCounter", new XAttribute("Type", t.ToString()));
                    foreach (int i in UriCounters[t])
                        uriElem.Add(new XElement("ConsumedValue", i.ToString()));
                    xElem.Add(uriElem);
                }
                xDoc.Root.Add(xElem);
                Stream s = Stream.Synchronized(SaveFile.GetWriteStream(this));
                try
                {
                    xDoc.Save(s);
                    s.Dispose();
                }
                finally
                {
                    SaveFile.ReleaseWriteStreamLock();
                }
            }

            public void Load(Package savePackage)
            {
                Stream s = Stream.Synchronized(SaveFile.GetReadStream(this));
                XDocument xDoc = null;
                try
                {
                    xDoc = XDocument.Load(s);
                    s.Dispose();
                }
                finally
                {
                    SaveFile.ReleaseReadStreamLock();
                }
                IATRelId = xDoc.Root.Element("IATRelId").Value;
                foreach (XElement elem in xDoc.Root.Elements("HistoryEntry"))
                    History.Add(new HistoryEntry(elem));
                Version = new CVersion(History.First().Version);
                foreach (XElement elem in xDoc.Root.Element("UriCounters").Elements())
                {
                    UriCounters[elem.Attribute("Type").Value] = new List<int>();
                    foreach (XElement consumedVal in elem.Elements("ConsumedValue"))
                        UriCounters[elem.Attribute("Type").Value].Add(Convert.ToInt32(consumedVal.Value));
                }
            }

            public void Dispose()
            {
                UriCounters.Clear();
            }
        }

        public static Thread SaveThread { get; private set; } = null;
        private SaveSplash SaveSplash { get; set; } = null;
        private bool IsDisposed = false;
        private Package SavePackage;
        private readonly CompressionOption Compression = CompressionOption.NotCompressed;
        private CIAT _IAT = null;
        private CIATLayout _Layout = null;
        private FontPreferences _FontPreferences;
        private ConcurrentDictionary<Uri, CIATKey> Keys = new ConcurrentDictionary<Uri, CIATKey>();
        private ConcurrentDictionary<Uri, CIATItem> IATItems = new ConcurrentDictionary<Uri, CIATItem>();
        private ConcurrentDictionary<Uri, CIATBlock> IATBlocks = new ConcurrentDictionary<Uri, CIATBlock>();
        private ConcurrentDictionary<Uri, CInstructionScreen> InstructionScreens = new ConcurrentDictionary<Uri, CInstructionScreen>();
        private ConcurrentDictionary<Uri, CInstructionBlock> InstructionBlocks = new ConcurrentDictionary<Uri, CInstructionBlock>();
        private ConcurrentDictionary<Uri, CSurvey> Surveys = new ConcurrentDictionary<Uri, CSurvey>();
        private ConcurrentDictionary<Uri, AlternationGroup> AlternationGroups = new ConcurrentDictionary<Uri, AlternationGroup>();
        private ConcurrentDictionary<Uri, Images.IImage> IImages = new ConcurrentDictionary<Uri, Images.IImage>();
        private ConcurrentDictionary<Uri, ObservableUri> ObservableUris = new ConcurrentDictionary<Uri, ObservableUri>();
        private ConcurrentDictionary<Uri, CFontFile.FontItem> FontItems = new ConcurrentDictionary<Uri, CFontFile.FontItem>();
        private ConcurrentDictionary<Uri, CSurveyItem> SurveyItems = new ConcurrentDictionary<Uri, CSurveyItem>();
        private readonly ManualResetEvent DisposingEvent = new ManualResetEvent(true), ResizingLayoutEvent = new ManualResetEvent(true);
        private ConcurrentDictionary<Uri, DIBase> DIs = new ConcurrentDictionary<Uri, DIBase>();
        private int ReadLockCount = 0;
        private bool FrozenForSave { get; set; }
        private System.Threading.Timer CacheTimer = null;
        private const int AutoSaveInterval = 30000;
        private readonly object ReadLocked = new object(), WriteLocked = new object();
        public readonly SaveFileMetaData MetaData;
        private ManualResetEvent PackageStreamOpen = new ManualResetEvent(true);
        public Images.ImageManager ImageManager { get; private set; }
        private readonly object layoutLock = new object();
        private readonly object saveLock = new object();
        public bool IsLoading { get; private set; } = false;
        private readonly ReaderWriterLockSlim ioLock = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);
        public CVersion Version { get { return MetaData.Version; } }
        public List<SaveFileMetaData.HistoryEntry> History { get { return MetaData.History; } }
        private void AutoSaveFlush(object state)
        {
            if (!Monitor.TryEnter(saveLock))
                return;
            Flush(true);
            Monitor.Exit(saveLock);
        }

        public SaveFile(String fileName, bool compressed, bool hidden)
        {
            IATConfigMainForm.Errors = 0;
            IATConfigMainForm.ErrorsReported = 0;
            DisposingEvent.WaitOne();
            FileInfo fi = new FileInfo(fileName);
            fi.Attributes &= ~FileAttributes.ReadOnly;
            bool hashVerified = false;
            try
            {
                byte[] signedHash = new byte[512];
                using (Stream workingFS = File.Open(WorkingSaveFilename, FileMode.Create, FileAccess.ReadWrite))
                {
                    using (FileStream saveFS = File.Open(fileName, FileMode.Open, FileAccess.Read))
                    {
                        long nBytes = fi.Length - 512;
                        byte[] buff = new byte[65536];
                        long nBytesRead = 0;
                        while (nBytesRead < nBytes)
                        {
                            int bytes = saveFS.Read(buff, 0, (nBytes - nBytesRead < 65536) ? (int)(nBytes - nBytesRead) : 65536);
                            workingFS.Write(buff, 0, bytes);
                            nBytesRead += bytes;
                        }
                        saveFS.Read(signedHash, 0, 512);
                    }
                }
                SHA256 sha = SHA256.Create();
                byte[] hash;
                using (Stream workingFS = File.Open(WorkingSaveFilename, FileMode.Open, FileAccess.Read))
                    hash = sha.ComputeHash(workingFS);
                RSACryptoServiceProvider rsaCrypt = new RSACryptoServiceProvider();
                rsaCrypt.ImportCspBlob(Convert.FromBase64String(Properties.Resources.sig));
                if (rsaCrypt.VerifyHash(hash, CryptoConfig.MapNameToOID("SHA256"), signedHash))
                    hashVerified = true;
                SavePackage = Package.Open(WorkingSaveFilename, FileMode.Open, FileAccess.ReadWrite);
            }
            catch (Exception ex)
            {
                if (File.Exists(WorkingSaveFilename))
                    File.Delete(WorkingSaveFilename);
                File.Copy(fileName, WorkingSaveFilename);
                SavePackage = Package.Open(WorkingSaveFilename, FileMode.Open, FileAccess.ReadWrite);
            }
            MetaData = new SaveFileMetaData(this, GetPackageLevelRelationship(typeof(SaveFileMetaData)).TargetUri);
            if (CVersion.Compare(new CVersion("1.1.1.13"), Version) < 0)
                RebuildSaveFileUris();
            if (CVersion.Compare(Version, new CVersion("1.1.0.29")) < 0)
                if (!hashVerified)
                {
                    InvalidSaveFileReport.Report();
                    throw new InvalidSaveFileException();
                }
            ImageManager = new Images.ImageManager();
        }

        public SaveFile()
        {
            DisposingEvent.WaitOne();
            SavePackage = Package.Open(WorkingSaveFilename, FileMode.Create, FileAccess.ReadWrite);
            MetaData = new SaveFileMetaData(this);
            ImageManager = new Images.ImageManager();
        }

        private String _WorkingSaveFilename = null;
        public String WorkingSaveFilename
        {
            get
            {
                if (_WorkingSaveFilename != null)
                    return _WorkingSaveFilename;
                int ctr = 0;
                while (File.Exists(String.Format(Properties.Resources.sWorkingSaveFileName, ++ctr)))
                {
                    try
                    {
                        File.Delete(String.Format(Properties.Resources.sWorkingSaveFileName, ctr));
                    }
                    catch (Exception) { }
                }
                ctr = 0;
                FileStream fs = null;
                while (_WorkingSaveFilename == null)
                {
                    try
                    {
                        _WorkingSaveFilename = String.Format(Properties.Resources.sWorkingSaveFileName, ++ctr);
                        fs = File.Open(_WorkingSaveFilename, FileMode.OpenOrCreate, FileAccess.Read);
                    }
                    catch (Exception ex)
                    {
                        _WorkingSaveFilename = null;
                    }
                    finally
                    {
                        if (fs != null)
                            fs.Close();
                    }
                }
                return _WorkingSaveFilename;
            }
        }

        private void RebuildSaveFileUris()
        {
            ioLock.EnterUpgradeableReadLock();
            Uri u;
            try
            {
                MetaData.UriCounters.Clear();
                Regex objTypeExp = new Regex(@"^/IATClient\.[^/]+/(IATClient\..*?)([1-9][0-9]*).*?(?!=\.rel)");
                PackagePartCollection ppc = SavePackage.GetParts();
                foreach (PackagePart pp in ppc)
                {
                    u = pp.Uri;
                    if (!objTypeExp.IsMatch(u.ToString()))
                        continue;
                    String objType = objTypeExp.Match(u.ToString()).Groups[1].Value;
                    int ndx = Convert.ToInt32(objTypeExp.Match(u.ToString()).Groups[2].Value);
                    if (!MetaData.UriCounters.ContainsKey(objType))
                        MetaData.UriCounters[objType] = new List<int>();
                    MetaData.UriCounters[objType].Add(ndx);
                }
            }
            catch (Exception ex)
            {
                IATConfigMainForm.ShowErrorReport("Error upgrading save file Uris", new CReportableException(ex.Message, ex));
            }
            finally
            {
                ioLock.ExitUpgradeableReadLock();
            }
        }

        public List<CFontFile.FontItem> CheckForMissingFonts()
        {
            List<CFontFile.FontItem> utilizedFonts = new List<CFontFile.FontItem>();
            List<Uri> uris = GetRelationshipsByType(IAT.URI, typeof(CIAT), typeof(CFontFile.FontItem)).Select(pr => pr.TargetUri).ToList();
            foreach (Uri u in uris)
                utilizedFonts.Add(new CFontFile.FontItem(u));
            return utilizedFonts.Where(fi => CFontFile.AvailableFonts.Where(fd => fd.FamilyName == fi.FamilyName).Count() == 0).ToList();
        }
        private readonly object iatLock = new object();
        public CIAT IAT
        {
            get
            {
                if (_IAT != null)
                    return _IAT;
                lock (iatLock)
                {
                    if (MetaData.IATRelId != String.Empty)
                    {
                        _IAT = new CIAT(GetPackageLevelRelationship(MetaData.IATRelId).TargetUri);
                        foreach (Uri u in GetRelationshipsByType(_IAT.URI, typeof(CIAT), typeof(AlternationGroup)).Select(pr => pr.TargetUri))
                            new AlternationGroup(u);
                    }
                    else
                    {
                        try
                        {
                            IsLoading = true;
                            _IAT = new CIAT(GetPackageLevelRelationship(typeof(CIAT)).TargetUri);
                        }
                        catch (ArgumentException ex)
                        {
                            _IAT = new CIAT();
                            foreach (Uri u in GetRelationshipsByType(_IAT.URI, typeof(CIAT), typeof(AlternationGroup)).Select(pr => pr.TargetUri))
                                new AlternationGroup(u);
                            MetaData.IATRelId = CreatePackageLevelRelationship(_IAT.URI, typeof(CIAT));
                        }
                        finally
                        {
                            IsLoading = false;
                        }
                    }
                    return _IAT;
                }
            }
            set
            {
                lock (iatLock)
                {
                    if (value.URI != null)
                    {
                        try
                        {
                            DeleteRelationships(value.URI);
                        }
                        catch (InvalidOperationException) { }
                        MetaData.IATRelId = CreatePackageLevelRelationship(value.URI, typeof(CIAT));
                    }
                    else
                    {
                        try
                        {
                            PackageRelationship pr = SavePackage.GetRelationshipsByType(typeof(CIAT).ToString()).First();
                            value.URI = pr.TargetUri;
                            MetaData.IATRelId = pr.Id;
                        }
                        catch (InvalidOperationException)
                        {
                            value.URI = CreatePart(value.BaseType, value.GetType(), value.MimeType, ".xml");
                            MetaData.IATRelId = CreatePackageLevelRelationship(value.URI, typeof(CIAT));
                        }
                    }
                    _IAT = value;
                }
            }
        }

        public CIATLayout Layout
        {
            get
            {
                if (_Layout != null)
                    return _Layout;
                lock (layoutLock)
                {
                    if (_Layout != null) 
                        return _Layout;
                    try
                    {
                        _Layout = new CIATLayout(GetPackageLevelRelationship(typeof(CIATLayout)).TargetUri);
                        _Layout.Activate();
                    }
                    catch (Exception ex)
                    {
                        _Layout = new CIATLayout();
                        _Layout.Activate();
                        _Layout.Save();
                        CreatePackageLevelRelationship(_Layout.URI, typeof(CIATLayout));
                    }
                    return _Layout;
                }
            }
            set
            {
                if (_Layout == value)
                    return;
                if (_Layout != null)
                    DeletePackageLevelRelationship(_Layout.BaseType);
                lock (layoutLock)
                {
                    if (_Layout != null)
                    {
                        value.Activate();
                        value.Save();
                        _Layout.Dispose();
                        _Layout = value;
                        CreatePackageLevelRelationship(value.URI, typeof(CIATLayout));
                        Task.Run(() => { ResizeToNewLayout(); });
                    }
                    else
                    {
                        _Layout = value;
                        CreatePackageLevelRelationship(value.URI, typeof(CIATLayout));
                        value.Activate();
                        value.Save();
                    }
                    _Layout.Save();
                }
            }
        }

        public FontPreferences FontPreferences
        {
            get
            {
                if (_FontPreferences != null)
                    return _FontPreferences;
                ioLock.EnterUpgradeableReadLock();
                try
                {
                    _FontPreferences = new FontPreferences(SavePackage.GetRelationshipsByType(typeof(FontPreferences).ToString()).First().TargetUri);
                }
                catch (Exception ex)
                {
                    _FontPreferences = new FontPreferences();
                }
                finally
                {
                    ioLock.ExitUpgradeableReadLock();
                }
                return _FontPreferences;
            }
            set
            {
                if (_FontPreferences != null)
                {
                    ioLock.ExitUpgradeableReadLock();
                    try
                    {
                        SavePackage.DeleteRelationship(SavePackage.GetRelationshipsByType(typeof(FontPreferences).ToString()).First().Id);
                        _FontPreferences = value;
                        _FontPreferences.Save();
                        SavePackage.CreateRelationship(_FontPreferences.URI, TargetMode.Internal, typeof(FontPreferences).ToString());
                    }
                    finally
                    {
                        ioLock.ExitUpgradeableReadLock();
                    }
                }
            }
        }

        public Uri CreatePart(Type baseType, Type objType, String mimeType, String ext)
        {
            ioLock.EnterWriteLock();
            try
            {
                String objTypeName = objType.ToString();
                if (!MetaData.UriCounters.ContainsKey(objTypeName))
                    MetaData.UriCounters[objTypeName] = new List<int>();
                MetaData.UriCounters[objTypeName].Sort();
                List<int> preCtrs = MetaData.UriCounters[objTypeName].TakeWhile((elem, ndx) => elem == ndx + 1).ToList();
                List<int> postCtrs = MetaData.UriCounters[objTypeName].SkipWhile((elem, ndx) => elem == ndx + 1).ToList();
                Uri uri = PackUriHelper.CreatePartUri(new Uri(baseType.ToString() + "/" + objType.ToString() + ((preCtrs.Count == 0) ? 1 : (preCtrs.Max() + 1)).ToString() + ext, UriKind.Relative));
                MetaData.UriCounters[objTypeName].Clear();
                MetaData.UriCounters[objTypeName].AddRange(preCtrs);
                MetaData.UriCounters[objTypeName].Add((preCtrs.Count == 0) ? 1 : (preCtrs.Max() + 1));
                MetaData.UriCounters[objTypeName].AddRange(postCtrs);
                return SavePackage.CreatePart(uri, mimeType, Compression).Uri;
            }
            finally
            {
                ioLock.ExitWriteLock();
            }
        }

        private PackagePart GetPart(IPackagePart p)
        {
            if (p.URI != null)
            {
                ioLock.EnterUpgradeableReadLock();
                try
                {
                    return SavePackage.GetPart(p.URI);
                }
                catch (Exception ex)
                {
                    return null;
                }
                finally
                {
                    ioLock.ExitUpgradeableReadLock();
                }
            }
            return null;
        }

        public PackagePart GetPart(Uri uri)
        {
            ioLock.EnterUpgradeableReadLock();
            try
            {
                return SavePackage.GetPart(uri);
            }
            catch (InvalidOperationException ex)
            {
                return null;
            }
            finally
            {
                ioLock.ExitUpgradeableReadLock();
            }
        }

        public String GetTypeName(Uri uri)
        {
            PackagePart part = GetPart(uri);
            return (new Regex("\\+(.*)")).Match(part.ContentType).Groups[1].Value;
        }

        public String GetMimeType(Uri uri)
        {
            PackagePart part = GetPart(uri);
            return part.ContentType;
        }

        public bool PartExists(Uri u)
        {
            ioLock.EnterUpgradeableReadLock();
            bool b = SavePackage.PartExists(u);
            ioLock.ExitUpgradeableReadLock();
            return b;
        }

        public PackageRelationship GetRelationship(IPackagePart p, String rId)
        {
            ioLock.EnterUpgradeableReadLock();
            try
            {
                return SavePackage.GetPart(p.URI).GetRelationship(rId);
            }
            finally
            {
                ioLock.ExitUpgradeableReadLock();
            }
        }


        public String GetRelationship(IPackagePart src, IPackagePart target)
        {
            ioLock.EnterUpgradeableReadLock();
            try
            {
                PackagePart p = SavePackage.GetPart(src.URI);
                return p.GetRelationships().Where(pr => pr.TargetUri.Equals(target.URI)).Select(pr => pr.Id).First();
            }
            catch (Exception)
            {
                return null;
            }
            finally
            {
                ioLock.ExitUpgradeableReadLock();
            }
        }

        public String GetRelationship(Uri srcUri, Uri targetUri)
        {
            ioLock.EnterUpgradeableReadLock();
            try
            {
                return SavePackage.GetPart(srcUri).GetRelationships().Where(pr => pr.TargetUri.Equals(targetUri)).Select(pr => pr.Id).FirstOrDefault();
            }
            finally
            {
                ioLock.ExitUpgradeableReadLock();
            }
        }

        public String GetRelationship(IPackagePart src, Uri targetUri)
        {
            ioLock.EnterUpgradeableReadLock();
            try
            {
                PackagePart p = SavePackage.GetPart(src.URI);
                return p.GetRelationships().Where(pr => pr.TargetUri.Equals(targetUri)).Select(pr => pr.Id).FirstOrDefault();
            }
            catch (Exception ex)
            {
                return null;
            }
            finally
            {
                ioLock.ExitUpgradeableReadLock();
            }
        }

        public PackageRelationship GetPackageLevelRelationship(String rId)
        {
            ioLock.EnterUpgradeableReadLock();
            try
            {
                return SavePackage.GetRelationship(rId);
            }
            catch (Exception ex)
            {
                return null;
            }
            finally
            {
                ioLock.ExitUpgradeableReadLock();
            }
        }

        public PackageRelationship GetPackageLevelRelationship(Type targetType)
        {
            ioLock.EnterUpgradeableReadLock();
            try
            {
                return SavePackage.GetRelationshipsByType(targetType.ToString()).First();
            }
            catch (Exception)
            {
                return null;
            }
            finally
            {
                ioLock.ExitUpgradeableReadLock();
            }
        }


        public void DeleteRelationships(Uri srcUri)
        {
            ioLock.EnterWriteLock();
            try
            {
                PackagePart part = SavePackage.GetPart(srcUri);
                List<String> rids = part.GetRelationships().Select(pr => pr.Id).ToList();
                foreach (String rid in rids)
                    part.DeleteRelationship(rid);
            }
            finally
            {
                ioLock.ExitWriteLock();
            }
        }

        public void DeleteRelationshipsByType(Uri srcUri, Type srcType, Type targetType)
        {
            ioLock.EnterWriteLock();
            try
            {
                PackagePart part = SavePackage.GetPart(srcUri);
                List<String> rids = part.GetRelationshipsByType(String.Format(Properties.Resources.sRelationshipTemplate, srcType.ToString(), targetType.ToString())).Select(rel => rel.Id).ToList();
                foreach (String rid in rids)
                    part.DeleteRelationship(rid);
            }
            finally
            {
                ioLock.ExitWriteLock();
            }
        }

        public String CreatePackageLevelRelationship(Uri targetUri, Type targetType)
        {
            ioLock.EnterWriteLock();
            try
            {
                if (SavePackage.GetRelationshipsByType(targetType.ToString()).Count() > 0)
                    throw new InvalidOperationException("A package level relationship of that type already exists");
                return SavePackage.CreateRelationship(targetUri, TargetMode.Internal, targetType.ToString()).Id;
            }
            finally
            {
                ioLock.ExitWriteLock();
            }
        }

        public String CreateRelationship(Type srcType, Type targetType, Uri srcUri, Uri targetUri)
        {
            ioLock.EnterWriteLock();
            try
            {
                PackagePart srcPart = SavePackage.GetPart(srcUri);
                return srcPart.CreateRelationship(targetUri, TargetMode.Internal, String.Format(Properties.Resources.sRelationshipTemplate, srcType.ToString(), targetType.ToString())).Id;
            }
            finally
            {
                ioLock.ExitWriteLock();
            }
        }

        public String CreateRelationship(Type srcType, Type targetType, Uri srcUri, Uri targetUri, String additionalData)
        {
            ioLock.EnterWriteLock();
            try
            {
                PackagePart srcPart = SavePackage.GetPart(srcUri);
                return srcPart.CreateRelationship(targetUri, TargetMode.Internal, String.Format(Properties.Resources.sRelationshipTemplate, srcType.ToString(), targetType.ToString()) + "+" + additionalData).Id;
            }
            finally
            {
                ioLock.ExitWriteLock();
            }
        }

        public List<PackageRelationship> GetRelationshipsByType(Uri srcUri, Type srcType, Type targetType)
        {
            ioLock.EnterUpgradeableReadLock();
            try
            {
                return SavePackage.GetPart(srcUri).GetRelationshipsByType(String.Format(Properties.Resources.sRelationshipTemplate, srcType.ToString(), targetType.ToString())).ToList();
            }
            finally
            {
                ioLock.ExitUpgradeableReadLock();
            }
        }

        public List<PackageRelationship> GetRelationshipsByType(Uri srcUri, Type srcType, Type targetType, String additionalData)
        {
            ioLock.EnterUpgradeableReadLock();
            try
            {
                return SavePackage.GetPart(srcUri).GetRelationshipsByType(String.Format(Properties.Resources.sRelationshipTemplate, srcType.ToString(), targetType.ToString()) + "+" + additionalData).ToList();
            }
            finally
            {
                ioLock.ExitUpgradeableReadLock();
            }
        }

        public void DeleteRelationshipsByType(Uri srcUri, Type srcType, Type targetType, String additionalData)
        {
            ioLock.EnterWriteLock();
            try
            {
                PackagePart pp = GetPart(srcUri);
                List<PackageRelationship> prList = pp.GetRelationshipsByType(String.Format(Properties.Resources.sRelationshipTemplate, srcType.ToString(), targetType.ToString()) + "+" + additionalData).ToList();
                foreach (String rId in prList.Select(pr => pr.Id))
                    pp.DeleteRelationship(rId);
            }
            finally
            {
                ioLock.ExitWriteLock();
            }
        }

        public void DeletePackageLevelRelationship(Type relType)
        {
            ioLock.EnterWriteLock();
            try
            {
                SavePackage.DeleteRelationship(SavePackage.GetRelationshipsByType(relType.ToString()).First().Id);
            }
            finally
            {
                ioLock.ExitWriteLock();
            }
        }

        public String DeleteRelationship(Uri srcUri, Uri targetUri)
        {
            ioLock.EnterWriteLock();
            try
            {
                PackagePart part = SavePackage.GetPart(srcUri);
                String rId = part.GetRelationships().Where(pr => pr.TargetUri.Equals(targetUri)).First().Id;
                part.DeleteRelationship(rId);
                return rId;
            }
            catch (Exception)
            {
                return String.Empty;
            }
            finally
            {
                ioLock.ExitWriteLock();
            }
        }

        public String DeleteRelationship(Uri URI, String rid)
        {
            ioLock.EnterWriteLock();
            try
            {
                PackagePart p = GetPart(URI);
                GetPart(URI).DeleteRelationship(rid);
                return rid;
            }
            finally
            {
                ioLock.ExitWriteLock();
            }
        }

        private ManualResetEventSlim ReadStreamFree = new ManualResetEventSlim(true), WriteStreamFree = new ManualResetEventSlim(true);

        public Stream GetReadStream(IPackagePart p)
        {
            PackagePart part = GetPart(p);
            ioLock.EnterUpgradeableReadLock();
            return part.GetStream(FileMode.Open, FileAccess.Read);
        }

        public Stream GetWriteStream(IPackagePart p)
        {
            ioLock.EnterWriteLock();
            PackagePart part = GetPart(p);
            return part.GetStream(FileMode.Create, FileAccess.Write);
        }

        public void ReleaseReadStreamLock()
        {
            ioLock.ExitUpgradeableReadLock();
        }

        public void ReleaseWriteStreamLock()
        {
            ioLock.ExitWriteLock();
        }

        public Uri Register(DIBase di)
        {
            Uri u = di.URI;
            if (u == null)
                u = CreatePart(di.BaseType, di.GetType(), di.MimeType, ".xml");
            if (!DIs.TryAdd(u, di))
                return u;
            return u;
        }

        public void Replace(DIBase newDI, DIBase oldDI)
        {
            ioLock.EnterWriteLock();
            try
            {
                List<PackageRelationship> oldRels = GetPart(newDI).GetRelationships().ToList();
                foreach (PackageRelationship rel in oldRels)
                {
                    GetPart(newDI).DeleteRelationship(rel.Id);
                }
                List<PackageRelationship> deletedRels = GetPart(oldDI).GetRelationships().ToList();
                foreach (PackageRelationship rel in deletedRels)
                {
                    GetPart(oldDI).DeleteRelationship(rel.Id);
                }
                if (!DIs.TryRemove(oldDI.URI, out DIBase dummy))
                    throw new ArgumentException("Attempt to replace a display item not in the dictionary.");
                DeletePart(oldDI.URI);
                newDI.URI = oldDI.URI;
                CreatePart(typeof(DIBase), newDI.GetType(), newDI.MimeType, ".xml");
                DIs.TryAdd(newDI.URI, newDI);
            }
            finally
            {
                ioLock.ExitWriteLock();
            }
        }

        public Uri Register(CIATKey key)
        {
            Uri u = key.URI;
            if (u == null)
                u = CreatePart(key.BaseType, key.GetType(), key.MimeType, ".xml");
            if (!Keys.TryAdd(u, key))
                return u;
            return u;
        }

        public Uri Register(CIATItem i)
        {
            Uri u = i.URI;
            if (u == null)
                u = CreatePart(i.BaseType, i.GetType(), i.MimeType, ".xml");
            if (!IATItems.TryAdd(u, i))
                return u;
            return u;
        }

        public Uri Register(CIATBlock b)
        {
            Uri u = b.URI;
            if (u == null)
                u = CreatePart(b.BaseType, b.GetType(), b.MimeType, ".xml");
            if (!IATBlocks.TryAdd(u, b))
                return u;
            return u;
        }

        public Uri Register(CInstructionScreen b)
        {
            Uri u = b.URI;
            if (u == null)
                u = CreatePart(b.BaseType, b.GetType(), b.MimeType, ".xml");
            if (!InstructionScreens.TryAdd(u, b))
                return u;
            return u;
        }

        public Uri Register(CInstructionBlock b)
        {
            Uri u = b.URI;
            if (u == null)
                u = CreatePart(b.BaseType, b.GetType(), b.MimeType, ".xml");
            if (!InstructionBlocks.TryAdd(u, b))
                return u;
            return u;
        }

        public Uri Register(CSurvey s)
        {
            Uri u = s.URI;
            if (u == null)
                u = CreatePart(s.BaseType, s.GetType(), s.MimeType, ".xml");
            if (!Surveys.TryAdd(u, s))
                return u;
            return u;
        }

        public Uri Register(AlternationGroup g)
        {
            Uri u = g.URI;
            if (u == null)
                u = CreatePart(g.BaseType, g.GetType(), g.MimeType, ".xml");
            if (!AlternationGroups.TryAdd(u, g))
                return u;
            return u;
        }

        public Uri Register(Images.IImage ii)
        {
            Uri u = ii.URI;
            if (u == null)
                throw new NullReferenceException();
            if (!IImages.TryAdd(u, ii))
                return u;
            return u;
        }

        public Uri Register(ObservableUri ou)
        {
            Uri u = ou.URI;
            if (u == null)
                u = CreatePart(ou.BaseType, ou.GetType(), ou.MimeType, ".xml");
            if (!ObservableUris.TryAdd(u, ou))
                return u;
            return u;
        }

        public Uri Register(CFontFile.FontItem fi)
        {
            Uri u = fi.URI;
            if (u == null)
                u = CreatePart(typeof(CFontFile.FontItem), fi.GetType(), fi.MimeType, ".xml");
            if (!FontItems.TryAdd(u, fi))
                return u;
            return u;
        }

        public Uri Register(CSurveyItem si)
        {
            Uri u = si.URI;
            if (u == null)
                u = CreatePart(typeof(CSurveyItem), si.GetType(), si.MimeType, ".xml");
            SurveyItems.TryAdd(u, si);
            return u;
        }

        public void DeletePart(Uri uri)
        {
            ioLock.EnterWriteLock();
            try
            {
                Regex exp = new Regex(@"IATClient\..+/(IATClient\..*?)([1-9][0-9]*)");
                String objTypeName = exp.Match(uri.ToString()).Groups[1].Value;
                int ndx = Convert.ToInt32(exp.Match(uri.ToString()).Groups[2].Value);
                MetaData.UriCounters[objTypeName].Remove(ndx);
                Keys.TryRemove(uri, out CIATKey key);
                DIs.TryRemove(uri, out DIBase di);
                IATItems.TryRemove(uri, out CIATItem i);
                IATBlocks.TryRemove(uri, out CIATBlock b);
                InstructionScreens.TryRemove(uri, out CInstructionScreen iScrn);
                InstructionBlocks.TryRemove(uri, out CInstructionBlock iBlock);
                IImages.TryRemove(uri, out Images.IImage ii);
                Surveys.TryRemove(uri, out CSurvey s);
                ObservableUris.TryRemove(uri, out ObservableUri oUri);
                FontItems.TryRemove(uri, out CFontFile.FontItem fi);
                SurveyItems.TryRemove(uri, out CSurveyItem si);
                DeleteRelationships(uri);
                SavePackage.DeletePart(uri);
            }
            finally
            {
                ioLock.ExitWriteLock();
            }
        }

        public List<Uri> GetPartsOfType(String contentType)
        {
            ioLock.EnterUpgradeableReadLock();
            try
            {
                return SavePackage.GetParts().Where(p => p.ContentType == contentType).Select(p => p.Uri).ToList();
            }
            finally
            {
                ioLock.ExitUpgradeableReadLock();
            }
        }

        public List<Uri> GetAllIATKeyUris()
        {
            ioLock.EnterUpgradeableReadLock();
            try
            {
                PackagePart pp = GetPart(IAT);
                return pp.GetRelationshipsByType(String.Format(Properties.Resources.sRelationshipTemplate, typeof(CIAT).ToString(), typeof(CIATKey).ToString())).Select(pr => pr.TargetUri).ToList();
            }
            finally
            {
                ioLock.ExitUpgradeableReadLock();
            }
        }

        public List<DIPreview> GetPreviews()
        {
            ioLock.EnterUpgradeableReadLock();
            try
            {
                var previews = new List<DIPreview>();
                foreach (var part in SavePackage.GetParts().Where(p => p.ContentType == "text/xml+display-item+" + DIType.Preview.ToString()))
                    previews.Add(GetDI(part.Uri) as DIPreview);
                return previews;
            }
            finally
            {
                ioLock.ExitUpgradeableReadLock();
            }
        }

        public void ResizeToNewLayout()
        {
            ResizingLayoutEvent.Reset();
            List<DIBase> dis = new List<DIBase>();
            List<DIPreview> composites = new List<DIPreview>();
            ioLock.EnterUpgradeableReadLock();
            try
            {
                Regex exp = new Regex(@"^text/xml\+display\-item\+(.*)");
                List<PackagePart> parts = SavePackage.GetParts().Where(p => exp.IsMatch(p.ContentType)).ToList();
                foreach (var p in parts)
                {
                    if (exp.Match(p.ContentType).Groups[1].Value != DIType.SurveyImage.ToString())
                    {
                        DIType diType = DIType.FromString(exp.Match(p.ContentType).Groups[1].Value);
                        if (diType == DIType.Preview)
                            composites.Add(GetDI(p.Uri) as DIPreview);
                        else if (diType != DIType.Null)
                            dis.Add(GetDI(p.Uri));
                    }
                }
            }
            finally
            {
                ioLock.ExitUpgradeableReadLock();
            }
            var genDiUris = dis.AsQueryable().Where(di => di != null).Select(di => di.URI).Distinct<Uri>().Select(u => CIAT.SaveFile.GetDI(u)).Cast<DIBase>();
            ImageManager.InvalidateImageBags(false);
            List<WaitHandle> waiters = new List<WaitHandle>();
            waiters.AddRange(genDiUris.Where(di => di != null).Select(di => di.InvalidationWaitHandle).ToList());
            foreach (var di in genDiUris)
                if (di != null)
                    di.ScheduleInvalidation();
            int n = 0;
            while (n < genDiUris.Count()) {
                var waits = genDiUris.Skip(n).Select(d => d.InvalidationWaitHandle).Take(genDiUris.Count() - n > 64 ? 64 : (genDiUris.Count() - n)).ToArray();
                WaitHandle.WaitAll(waits);
                n += waits.Count();
            }
            ResizingLayoutEvent.Set();
        }
    

        public DIBase GetDI(Uri URI)
        {
            ioLock.EnterUpgradeableReadLock();
            try
            {
                String mimeType = String.Empty;
                if (DIs.ContainsKey(URI))
                    return DIs[URI];
                if (!SavePackage.PartExists(URI))
                    return null;
                PackagePart part = SavePackage.GetPart(URI);
                mimeType = part.ContentType;
                Regex exp = new Regex(@"display\-item\+(.*)$");
                String dispItemType = exp.Match(mimeType).Groups[1].Value;
                DIType type;
                if (dispItemType == "Composite")
                    type = DIType.Preview;
                else
                    type = DIType.FromString(dispItemType);
                if (type == DIType.LambdaGenerated)
                    return null;
                DIBase di = type.Create(URI);
                DIs.TryAdd(URI, di);
                return di;
            }
            finally
            {
                ioLock.ExitUpgradeableReadLock();
            }
        }


        public CIATKey GetIATKey(Uri uri)
        {
            bool bWasWriting = ioLock.IsWriteLockHeld;
            bool bWasReading = ioLock.IsReadLockHeld;
            ioLock.EnterUpgradeableReadLock();
            try
            {
                if (Keys.ContainsKey(uri))
                    return Keys[uri];
                if (!PartExists(uri))
                    throw new KeyNotFoundException("The supplied uri does not exist in the package file");
                PackagePart part = SavePackage.GetPart(uri);
                String mimeType = part.ContentType;
                Regex exp = new Regex("\\+(.*)$");
                Keys[uri] = IATKeyType.FromTypeName(exp.Match(mimeType).Groups[1].Value).Create(uri);
                return Keys[uri];
            }
            finally
            {
                ioLock.ExitUpgradeableReadLock();
            }
        }

        public CIATItem GetIATItem(Uri uri)
        {
            if (IATItems.ContainsKey(uri))
                return IATItems[uri];
            if (!PartExists(uri))
                throw new KeyNotFoundException("The supplied uri does not exist in the package file");
            IATItems[uri] = new CIATItem(uri);
            return IATItems[uri];
        }

        public CIATBlock GetIATBlock(Uri uri)
        {
            if (IATBlocks.ContainsKey(uri))
                return IATBlocks[uri];
            if (!PartExists(uri))
                throw new KeyNotFoundException("The supplied uri does not exist in the package file");
            IATBlocks[uri] = new CIATBlock(IAT, uri);
            return IATBlocks[uri];
        }

        public CInstructionScreen GetInstructionScreen(Uri uri)
        {
            if (InstructionScreens.ContainsKey(uri))
                return InstructionScreens[uri];
            if (!PartExists(uri))
                throw new KeyNotFoundException("The supplied uri does not exist in the package file");
            PackagePart part = GetPart(uri);
            String mimeType = part.ContentType;
            Regex exp = new Regex("\\+(.*)");
            InstructionScreens[uri] = InstructionScreenType.FromTypeName(exp.Match(mimeType).Groups[1].Value).Create(uri);
            return InstructionScreens[uri];
        }

        public CInstructionBlock GetInstructionBlock(Uri uri)
        {
            if (InstructionBlocks.ContainsKey(uri))
                return InstructionBlocks[uri];
            if (!PartExists(uri))
                throw new KeyNotFoundException("The supplied uri does not exist in the package file");
            InstructionBlocks[uri] = new CInstructionBlock(IAT, uri);
            return InstructionBlocks[uri];
        }

        public CSurvey GetSurvey(Uri uri)
        {
            if (Surveys.ContainsKey(uri))
                return Surveys[uri];
            if (!PartExists(uri))
                throw new KeyNotFoundException("The supplied uri does not exist in the package file.");
            Surveys[uri] = new CSurvey(uri);
            return Surveys[uri];
        }

        public AlternationGroup GetAlternationGroup(Uri uri)
        {
            if (AlternationGroups.ContainsKey(uri))
                return AlternationGroups[uri];
            if (PartExists(uri))
                throw new KeyNotFoundException("The supplied uri does not exist in the package file.");
            AlternationGroups[uri] = new AlternationGroup(uri);
            return AlternationGroups[uri];
        }

        public ObservableUri GetObservableUri(Uri u)
        {
            if (ObservableUris.ContainsKey(u))
                return ObservableUris[u];
            if (!PartExists(u))
                throw new KeyNotFoundException("The supplied uri does not exist in the package file.");
            ObservableUris[u] = new ObservableUri(u);
            return ObservableUris[u];
        }

        public CFontFile.FontItem GetFontItem(Uri u)
        {
            if (FontItems.ContainsKey(u))
                return FontItems[u];
            if (!PartExists(u))
                throw new KeyNotFoundException("The supplied uri does not exist in the package file.");
            FontItems[u] = new CFontFile.FontItem(u);
            return FontItems[u];
        }


        public CSurveyItem GetSurveyItem(Uri u)
        {
            if (SurveyItems.ContainsKey(u))
                return SurveyItems[u];
            if (!PartExists(u))
                throw new KeyNotFoundException("The supplied uri does not exist in the package file.");
            PackagePart pp = SavePackage.GetPart(u);
            Regex exp = new Regex(@"text/xml\+(.+)");
            SurveyItemType t = SurveyItemType.FromTypeName(exp.Match(pp.ContentType).Groups[1].Value);
            if (t == SurveyItemType.Caption)
                SurveyItems[u] = new CSurveyCaption(u);
            else if (t == SurveyItemType.SurveyImage)
                SurveyItems[u] = new CSurveyItemImage(u);
            else
                SurveyItems[u] = new CSurveyItem(u);
            return SurveyItems[u];
        }

        public Images.IImage GetIImage(Uri u)
        {
            if (IImages.ContainsKey(u))
                return IImages[u];
            if (!PartExists(u))
                throw new KeyNotFoundException("The supplied uri does not exist in the package file.");
            IImages[u] = new Images.ImageManager.Image(u);
            return IImages[u];
        }

        private readonly ManualResetEventSlim SaveEvent = new ManualResetEventSlim(true);
        public void Save(String filename)
        {
            Package package = null;
            ThreadStart thStart = new ThreadStart(new Action(() =>
            {
                SaveSplash spl = new SaveSplash();
                spl.Show();
                Flush(false);
                ioLock.EnterWriteLock();
                try
                {
                    SavePackage.Close();
                    if ((WorkingSaveFilename != filename) && (File.Exists(filename)))
                        File.Delete(filename);
                    File.Copy(WorkingSaveFilename, filename);
                    SavePackage = Package.Open(WorkingSaveFilename, FileMode.Open, FileAccess.ReadWrite);
                    package = Package.Open(filename, FileMode.Open, FileAccess.ReadWrite);
                    List<PackagePart> metaDataParts = package.GetParts().Where(pp => pp.ContentType.Contains("text/xml+" + typeof(Images.ImageManager.Image.ImageMetaData).ToString())).ToList();
                    List<PackagePart> previewImageParts = metaDataParts.SelectMany(pp => pp.GetRelationshipsByType(String.Format(Properties.Resources.sRelationshipTemplate, typeof(Images.ImageManager.Image.ImageMetaData).ToString(), typeof(Images.ImageManager.ImageMedia).ToString()))).Select(pr => pr.Package.GetPart(pr.TargetUri)).Where(pp => pp.ContentType.Contains(Images.ImageMediaType.FullWindow.MimeType)).ToList();
                    List<Uri> metaDataPreviewUris = previewImageParts.SelectMany(pp => pp.GetRelationshipsByType(String.Format(Properties.Resources.sRelationshipTemplate, typeof(Images.ImageManager.ImageMedia).ToString(), typeof(Images.ImageManager.Image.ImageMetaData).ToString()))).Select(pr => pr.TargetUri).ToList();
                    foreach (Uri metaUri in metaDataPreviewUris.Distinct())
                    {
                        foreach (Uri u in package.GetPart(metaUri).GetRelationshipsByType(String.Format(Properties.Resources.sRelationshipTemplate, typeof(Images.ImageManager.Image.ImageMetaData).ToString(), typeof(Images.ImageManager.ImageMedia).ToString())).Select(pr => pr.TargetUri))
                            package.DeletePart(u);
                        package.DeletePart(metaUri);
                    }
                    PackagePart pPart = package.CreatePart(PackUriHelper.CreatePartUri(new Uri("signature.bin", UriKind.Relative)), "application/octet-stream");
                    byte[] data = new byte[32];
                    Stream s = pPart.GetStream(FileMode.Create, FileAccess.ReadWrite);
                    s.Write(data, 0, 32);
                    package.DeletePart(pPart.Uri);
                    package.Close();
                    RSACryptoServiceProvider rsaCrypt = new RSACryptoServiceProvider();
                    rsaCrypt.ImportCspBlob(Convert.FromBase64String(Properties.Resources.sig));
                    SHA256 sha = SHA256.Create();
                    byte[] hash = null;
                    using (FileStream fs = new FileStream(filename, FileMode.Open, FileAccess.Read))
                        hash = sha.ComputeHash(fs);
                    byte[] signedHash = rsaCrypt.SignHash(hash, CryptoConfig.MapNameToOID("SHA256"));
                    using (FileStream fs = new FileStream(filename, FileMode.Append, FileAccess.Write))
                        fs.Write(signedHash, 0, signedHash.Length);
                }
                finally
                {
                    ioLock.ExitWriteLock();
                    spl.Close();
                    SaveEvent.Set();
                }
            }));
            SaveEvent.Reset();
            SaveThread = new Thread(thStart);
            SaveThread.SetApartmentState(ApartmentState.STA);
            SaveThread.Start();
        }

        public bool Flush(bool optional)
        {
            DisposingEvent.WaitOne();
            ResizingLayoutEvent.WaitOne(1000);
            if (IsDisposed)
                return false;
            CIAT iat = IAT;
            FrozenForSave = true;
            int ctr = 0;
            List<Uri> uris = Keys.Keys.ToList();
            foreach (Uri u in uris)
            {
                GetIATKey(u)?.Save();
            }
            uris = IATItems.Keys.ToList();
            foreach (Uri u in uris)
            {
                GetIATItem(u)?.Save();
            }
            uris = InstructionScreens.Keys.ToList();
            foreach (Uri u in uris)
            {
                GetInstructionScreen(u)?.Save();
            }
            uris = IATBlocks.Keys.ToList();
            foreach (Uri u in uris)
            {
                GetIATBlock(u)?.Save();
            }
            uris = InstructionBlocks.Keys.ToList();
            foreach (Uri u in uris)
            {
                GetInstructionBlock(u)?.Save();
            }
            uris = Surveys.Keys.ToList();
            foreach (Uri u in uris)
            {
                GetSurvey(u)?.Save();
            }
            uris = AlternationGroups.Keys.ToList();
            foreach (Uri u in uris)
            {
                GetAlternationGroup(u)?.Save();
            }
            uris = IImages.Keys.ToList();
            foreach (Uri u in uris)
            {
                GetIImage(u)?.Save();
            }
            uris = DIs.Keys.ToList();
            foreach (Uri u in uris)
            {
                GetDI(u)?.Save();
            }
            uris = ObservableUris.Keys.ToList();
            foreach (Uri u in uris)
            {
                GetObservableUri(u).Save();
            }
            FontPreferences.Save();
            Layout.Save();
            iat.Save();
            MetaData.Save();
            ImageManager.FlushCache();
            FrozenForSave = false;
            return true;
        }

        static public String RecoveryFilePath
        {
            get
            {
                return Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData, Environment.SpecialFolderOption.Create) + Path.DirectorySeparatorChar +
                    "IATSoftware" + Path.DirectorySeparatorChar + "Recovery.iat";
            }
        }

        public void CreateRecovery()
        {
            if (File.Exists(RecoveryFilePath))
                File.Delete(RecoveryFilePath);
            File.Copy(WorkingSaveFilename, RecoveryFilePath);
            Save(RecoveryFilePath);
        }

        public void ClearDIDictionary()
        {
            DIs.Clear();
        }

        public void Dispose()
        {
            try
            {
                ImageManager.Dispose();
                lock (saveLock)
                {
                    foreach (var k in Keys.Values.Where(key => key is CIATDualKey))
                        k.Dispose();
                    foreach (var k in Keys.Values.Where(key => key is CIATReversedKey))
                        k.Dispose();
                    foreach (var k in Keys.Values.Where(key => key is CIATKey))
                        k.Dispose();
                    foreach (var b in IATBlocks.Values)
                        b.Dispose();
                    foreach (var ib in InstructionBlocks.Values)
                        ib.Dispose();
                    foreach (var s in Surveys.Values)
                        s.Dispose();
                    foreach (var i in IImages.Values)
                        i.Dispose();
                    foreach (var ou in ObservableUris.Values)
                        ou.Dispose();
                    foreach (var di in DIs.Values)
                        di.Dispose();
                }
                DisposingEvent.Reset();
            }
            finally
            {
                IsDisposed = true;
                SavePackage.Close();
                try
                {
                    File.Delete(WorkingSaveFilename);
                }
                catch (Exception ex)
                {
                }
            }
            DisposingEvent.Set();
        }
    }
}

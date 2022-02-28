using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace IATClient
{
    public class ImageMetaDataDocument : IPackagePart
    {
        public readonly Dictionary<String, ImageMetaData> Entries = new Dictionary<String, ImageMetaData>();
        public Uri URI { get; set; }
        public Type BaseType { get { return typeof(ImageMetaDataDocument); } }
        public String MimeType { get { return "text/xml+" + this.GetType(); } }

        public ImageMetaDataDocument()
        {
            URI = CIAT.SaveFile.CreatePart(BaseType, this.GetType(), MimeType, ".xml");
            CIAT.SaveFile.CreatePackageLevelRelationship(URI, BaseType);
        }
        public ImageMetaDataDocument(Uri u)
        {
            URI = u;
            Load();
        }

        public void CleanPackageForSave()
        {
            var l = Entries.Values.Where(kv => Images.ImageMediaType.FromDIType(kv.DIType) == Images.ImageMediaType.FullWindow).ToList();
            foreach (var md in l)
                md.Image.Dispose();
        }

        public void RemoveEntry(Images.IImage iImage)
        {
            var pr = CIAT.SaveFile.GetRelationship(this, iImage);
            if (pr == null)
                return;
            Entries.Remove(pr);
            CIAT.SaveFile.DeleteRelationship(URI, pr);
        }

        public void Save()
        {
            XDocument xDoc = new XDocument();
            xDoc.Add(new XElement(GetType().Name));
            foreach (var md in Entries.Values)
                md.Append(xDoc.Root);
            Stream s = Stream.Synchronized(CIAT.SaveFile.GetWriteStream(this));
            try
            {
                xDoc.Save(s);
            }
            finally
            {
                s.Dispose();
                CIAT.SaveFile.ReleaseWriteStreamLock();
            }
        }

        public void Load()
        {
            Stream s = Stream.Synchronized(CIAT.SaveFile.GetReadStream(this));
            XDocument xDoc;
            try
            {
                xDoc = XDocument.Load(s);
            }
            finally
            {
                s.Dispose();
                CIAT.SaveFile.ReleaseReadStreamLock();
            }
            foreach (var meta in xDoc.Root.Elements(typeof(ImageMetaData).Name))
            {
                var data = new ImageMetaData(this, meta);
                Entries[data.ImageRelId] = data;
            }
        }

        public void Dispose()
        {
            foreach (var md in Entries)
            {
                var img = md.Value.Image;
                CIAT.SaveFile.DeleteRelationship(URI, md.Key);
            }
            Entries.Clear();
            CIAT.SaveFile.DeletePackageLevelRelationship(BaseType);
        }
    }
}


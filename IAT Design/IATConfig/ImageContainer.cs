using IATClient.Messages;
using net.sf.saxon.@event;
using net.sf.saxon.om;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.Drawing;
using System.Linq;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Linq;

namespace IATClient.IATConfig
{
    public class ImageContainer
    {
        private static readonly int NUM_WORKER_THREADS = 4;
        private static readonly int WORKER_INTERVAL = 500;
        private int idCtr = 0;
        private int numImagesProcessed = 0;
        private ConcurrentDictionary<System.Timers.Timer, ManualResetEvent> TimerEvents = new ConcurrentDictionary<System.Timers.Timer, ManualResetEvent>();
        private readonly ConcurrentQueue<IATImage> ImageBases = new ConcurrentQueue<IATImage>();
        private readonly List<System.Timers.Timer> Timers = new List<System.Timers.Timer>();
        private readonly List<object> TimerLocks = new List<object>();
        private List<IATImage> ImageList = new List<IATImage>();
        private SHA512Managed SHA512 = new SHA512Managed();
        private readonly object ImageListLock = new object(), queueLock = new object();
        private readonly ManualResetEvent QueueEvent = new ManualResetEvent(false);
        private readonly ManualResetEvent ImagesProcessed;
        public ImageContainer(Func<DIBase, IATImage> generateImage, ManualResetEvent imagesProcessed)
        {
            ImagesProcessed = imagesProcessed;
            object incLock = new object();
            int numThreadsFinished = 0;
            for (int ctr = 0; ctr < NUM_WORKER_THREADS; ctr++)
            {
                System.Timers.Timer timer = new System.Timers.Timer(100);
                timer.Elapsed += (sender, evt) =>
                {
                    IATImage iatImage;
                    lock (queueLock)
                    {
                        if (!ImageBases.TryDequeue(out iatImage))
                            return;
                    }
                    if (iatImage == null)
                    {
                        lock (incLock)
                        {
                            if (++numThreadsFinished == NUM_WORKER_THREADS)
                                imagesProcessed.Set();
                            ImageBases.Enqueue(null);
                            timer.Stop();
                            return;
                        }
                    }
                    else
                    {
                        var image = generateImage(CIAT.SaveFile.GetDI(iatImage.SourceUris[0]));
                        var sha = image.SHA;
                        lock (ImageListLock)
                        {
                            var duplicate = ImageList.Where(i => (i.SHA == sha) && iatImage.Bounds.Equals(i.Bounds)).FirstOrDefault();
                            if (duplicate == null)
                            {
                                image.SourceUris.AddRange(iatImage.SourceUris);
                                image.Bounds = iatImage.Bounds;
                                image.Id = ImageList.Count + 1;
                                ImageList.Add(image);
                            }
                            else if (duplicate != null)
                                duplicate.SourceUris.AddRange(iatImage.SourceUris);
                        }
                    }
                };
                timer.Start();
            }
        }


        public int NumImages
        {
            get
            {
                return ImageList.Count;
            }
        }

        public void AddDI(DIBase di, Rectangle rect)
        {
            if (di == null)
            {
                ImageBases.Enqueue(null);
                return;
            }
            var iatImage = new IATImage();
            iatImage.Bounds = rect;
            iatImage.SourceUris.Add(di.URI);
            ImageBases.Enqueue(iatImage);
        }


        public IATImage GetImage(Uri u)
        {
            return ImageList.First(tup => tup.SourceUris.Contains(u));
        }

        public List<IATImage> GetImages(Uri u)
        {
            return ImageList.Where(i => i.SourceUris.Contains(u)).ToList();
        }

        public void WriteXml(XmlWriter xWriter)
        {
            ImagesProcessed.WaitOne();
            xWriter.WriteStartElement("DisplayItemList");
            xWriter.WriteAttributeString("NumDisplayItems", ImageList.Count.ToString());
            ImageList.ForEach((t) => t.WriteXml(xWriter));
            xWriter.WriteEndElement();
        }


        public ManifestFile[] ConstructFileManifest(ManifestFile.EResourceType resourceType)
        {
            ImagesProcessed.WaitOne();
            ManifestFile[] fileManifest = new ManifestFile[ImageList.Count];
            for (int ctr = 0; ctr < ImageList.Count; ctr++)
            {
                fileManifest[ctr] = new ManifestFile();
                if ((ImageList[ctr].SourceUris[0].Equals(CIAT.SaveFile.Layout.ErrorMark.URI)) && (resourceType == ManifestFile.EResourceType.image))
                    fileManifest[ctr].ResourceType = ManifestFile.EResourceType.errorMark;
                else
                    fileManifest[ctr].ResourceType = resourceType;
                fileManifest[ctr].Name = ImageList[ctr].FileName;
                fileManifest[ctr].Size = ImageList[ctr].ImageData.Length;
                fileManifest[ctr].ResourceId = ImageList[ctr].Id;
                fileManifest[ctr].ReferenceIds.AddRange(ImageList[ctr].Indexes);
            }
            return fileManifest;
        }

        public byte[][] GetImageData()
        {
            byte[][] data = new byte[ImageList.Count][];
            for (int ctr = 0; ctr < ImageList.Count; ctr++)
                data[ctr] = ImageList[ctr].ImageData;
            return data;
        }
    }
}

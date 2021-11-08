using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Security.Cryptography;
using System.Threading;
using System.Xml;
using System.Collections.Concurrent;

namespace IATClient.IATConfig
{
    public class ImageContainer
    {
        private static readonly int NUM_WORKER_THREADS = 4;
        private static readonly int WORKER_INTERVAL = 500;
        private int idCtr = 0;
        private ConcurrentQueue<DIBase> ImageBases = new ConcurrentQueue<DIBase>();
        private readonly List<System.Timers.Timer> Timers = new List<System.Timers.Timer>();
        private readonly List<object> TimerLocks = new List<object>();
        private List<IATImage> ImageList = new List<IATImage>();
        private SHA512Managed SHA512 = new SHA512Managed();
        private readonly object ImageListLock = new object();
      
        public ImageContainer(Func<DIBase, IATImage> generateImage, ManualResetEvent imagesProcessed)
        {
            for (int ctr = 0; ctr < NUM_WORKER_THREADS; ctr++)
            {
                TimerLocks.Add(new object());
                var t = new System.Timers.Timer(WORKER_INTERVAL);
                t.Elapsed += (sender, args) =>
                {
                    if (!Monitor.TryEnter(TimerLocks[ctr]))
                        return;
                    while (ImageBases.TryDequeue(out DIBase result))
                    {
                        if (result == null)
                        {
                            ImageBases.Enqueue(null);
                            Monitor.Exit(TimerLocks[ctr]);
                            imagesProcessed.Set();
                            t.Stop();
                            return;
                        }
                        var iImg = generateImage(result);
                        lock (ImageListLock)
                        {
                            IATImage duplicate = ImageList.FirstOrDefault<IATImage>(i => i.SHA == iImg.SHA);
                            if (duplicate == null)
                            {
                                iImg.Id = ++idCtr;
                                iImg.SourceUris.Add(result.URI);
                                ImageList.Add(iImg);
                            }
                            else
                                duplicate.SourceUris.Add(result.URI);
                        }
                    };
                    Monitor.Exit(TimerLocks[ctr]);
                };
                Timers.Add(t);
                t.Start();
            }
        }


        public int NumImages
        {
            get
            {
                return ImageList.Count;
            }
        }

        public void AddDI(DIBase di)
        {
            ImageBases.Enqueue(di);
        }

        public IATImage GetImage(Uri u)
        {
            return ImageList.First(i => i.SourceUris.Contains(u));
        }

        public void ReadXml(XmlReader xReader)
        {
            int nDisplayItems = Convert.ToInt32(xReader["NumDisplayItems"]);
            xReader.ReadStartElement("DisplayItemList");
            for (int ctr = 0; ctr < nDisplayItems; ctr++)
            {
                var iImg = new IATImage();
                iImg.ReadXml(xReader);
                ImageList.Add(iImg);
            }
            xReader.ReadEndElement();
        }

        public void WriteXml(XmlWriter xWriter)
        {
            xWriter.WriteStartElement("DisplayItemList");
            xWriter.WriteAttributeString("NumDisplayItems", ImageList.Count.ToString());
            ImageList.ForEach((i) => i.WriteXml(xWriter));
            xWriter.WriteEndElement();
        }

        public ManifestFile[] ConstructFileManifest()
        {
            ManifestFile[] fileManifest = new ManifestFile[ImageList.Count];
            for (int ctr = 0; ctr < ImageList.Count; ctr++)
            {
                fileManifest[ctr] = new ManifestFile();
                fileManifest[ctr].Name = ImageList[ctr].FileName;
                fileManifest[ctr].Size = ImageList[ctr].ImageData.Length;
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

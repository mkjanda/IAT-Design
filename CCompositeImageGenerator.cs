using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Timers;
using System.Threading;
using System.Threading.Tasks;

namespace IATClient
{
    class CCompositeImageGenerator
    {
        static private System.Timers.Timer GenerationTimer = new System.Timers.Timer(100);
        static private Dictionary<CCompositeImage, int> ImageDictionary = new Dictionary<CCompositeImage, int>();
        static private object listLockObj = new object();
        static private object generatorLock = new object();
        static private bool halting = false;
        static private ManualResetEvent halted = new ManualResetEvent(false);
        static private System.Threading.Timer ImageGenerationTimer = null;
        static CCompositeImageGenerator()
        {
        }

        static public void StartGeneration()
        {
            halted = new ManualResetEvent(false);
            halting = false;
            ImageGenerationTimer = new System.Threading.Timer((n) =>
            {
                if (Monitor.TryEnter(generatorLock))
                {
                    List<CCompositeImage> staleImages = new List<CCompositeImage>();
                    lock (listLockObj)
                    {
                        foreach (CCompositeImage ci in ImageDictionary.Keys)
                            if (!ci.IsValid)
                                staleImages.Add(ci);
                    }
                    foreach (CCompositeImage ci in staleImages)
                        ci.TryGenerate(false);
                    if (halting)
                        halted.Set();
                    Monitor.Exit(generatorLock);
                }
            }, null, 0, 100);
        }

        static public void EndGeneration()
        {
            if (ImageGenerationTimer == null)
                return;
            halting = true;
            halted.WaitOne();
            ImageGenerationTimer.Dispose();
            ImageGenerationTimer = null;

        }

        static private void OnTimerTick(object sender, ElapsedEventArgs e)
        {
            if (!Monitor.TryEnter(generatorLock))
                return;
            Monitor.Exit(generatorLock);
        }

        static public void AddCompositeImage(CCompositeImage ci)
        {
            lock (listLockObj)
            {
                if (!ImageDictionary.Keys.Contains(ci))
                    ImageDictionary[ci] = 1;
                else
                    ImageDictionary[ci] = ImageDictionary[ci] + 1;
                ci.Invalidate();
            }
        }

        static public void RemoveImage(CCompositeImage ci)
        {
            lock (listLockObj)
            {
                ImageDictionary[ci] = ImageDictionary[ci] - 1;
                if (ImageDictionary[ci] == 0)
                    ImageDictionary.Remove(ci);
            }
        }

        static public void ClearImageList()
        {
            lock (listLockObj)
            {
                ImageDictionary.Clear();
            }
        }
    }
}

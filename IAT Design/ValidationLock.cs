using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace IATClient
{
    public class ValidationLock
    {
        private const int LockWaitPeriod = 2000;

        private readonly ConcurrentDictionary<DIBase, CancellationTokenSource> TokenSourceDictionary = new ConcurrentDictionary<DIBase, CancellationTokenSource>();
        public readonly ManualResetEvent ValidationEvent = new ManualResetEvent(false);
        public readonly ManualResetEvent InvalidationEvent = new ManualResetEvent(true);
        private ConcurrentDictionary<DIBase, ManualResetEvent> DIValidationDictionary = new ConcurrentDictionary<DIBase, ManualResetEvent>();
        public bool DoInvalidation(DIBase di)
        {
            if (!TokenSourceDictionary.TryGetValue(di, out CancellationTokenSource value))
                return false;
            try
            {
                return Task<bool>.Run(() =>
                {
                    if (InvalidationEvent.WaitOne(LockWaitPeriod))
                        return true;
                    return false;
                }, value.Token).ContinueWith<bool>((t) =>
                {
                    var result = t.Result && !t.IsCanceled && !t.IsFaulted;
                    if (DIValidationDictionary.TryRemove(di, out ManualResetEvent evt))
                        evt.Set();
                    if (TokenSourceDictionary.TryRemove(di, out CancellationTokenSource source))
                        source.Cancel();
                    return result;
                }).Result;
            }
            catch (OperationCanceledException ex)
            {
                if (DIValidationDictionary.TryRemove(di, out ManualResetEvent evt))
                    evt.Set();
                return false;
            }
        }

        public ValidationLock(DIBase[] dis)
        {
            Array.ForEach(dis, (di) =>
            {
                di.LockValidation(this);
                TokenSourceDictionary[di] = new CancellationTokenSource();
                DIValidationDictionary[di] = new ManualResetEvent(false);
            });
            Task.Run(() =>
            {
                WaitHandle.WaitAll(DIValidationDictionary.Values.ToArray());
                ValidationEvent.Set();
            });

        }

        public void Validate(DIBase di)
        {
            if (!DIValidationDictionary.TryRemove(di, out ManualResetEvent evt))
                return;
            if (!TokenSourceDictionary.TryRemove(di, out CancellationTokenSource source))
                return;
            source.Cancel();
            evt.Set();
        }
    }
}

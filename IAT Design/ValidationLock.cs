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
        private readonly CancellationTokenSource cancellationSource = new CancellationTokenSource();
        public readonly ManualResetEvent ValidationEvent = new ManualResetEvent(false);
        public readonly ManualResetEvent InvalidationEvent = new ManualResetEvent(true);
        private readonly ConcurrentDictionary<DIBase, ManualResetEvent> DIValidationDictionary = new ConcurrentDictionary<DIBase, ManualResetEvent>();
        public bool DoInvalidation(DIBase di)
        {
            try
            {
                return Task<bool>.Run(() =>
                {
                    if (InvalidationEvent.WaitOne(LockWaitPeriod))
                        return true;
                    return false;
                }, cancellationSource.Token).ContinueWith<bool>((t) =>
                {
                    var result = t.Result && !t.IsCanceled;
                    if (DIValidationDictionary.TryGetValue(di, out ManualResetEvent evt))
                        evt.Set();
                    return result;
                }).Result;
            }
            catch (OperationCanceledException)
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
                DIValidationDictionary[di] = new ManualResetEvent(false);
            });
            Task.Run(() =>
            {
                if (!WaitHandle.WaitAll(DIValidationDictionary.Values.ToArray(), LockWaitPeriod))
                    cancellationSource.Cancel();
                else
                    ValidationEvent.Set();
            });

        }

        public void Validate(DIBase di)
        {
            if (!DIValidationDictionary.TryRemove(di, out ManualResetEvent evt))
                return;
            evt.Set();
        }
    }
}

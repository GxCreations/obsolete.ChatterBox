using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace ChatterBox.Client.Voip.Utils
{
    internal class Lock : IDisposable
    {
        private readonly SemaphoreSlim _sem;
        private SemaphoreSlim _dispose;

        public Lock(SemaphoreSlim sem)
        {
            _sem = sem;
            _sem.WaitAsync();
        }

        public Task WaitAsync()
        {
            if (null != _dispose) return Task.Run(() => { });
            _dispose = _sem;
            var result = _dispose.WaitAsync();
            Debug.WriteLine("Lock - Semaphore - Got");
            return result;
        }

        public void Dispose()
        {
            if (null != _dispose)
            {
                Debug.WriteLine("Lock - Semaphore - Release");
                _dispose.Release();
                _dispose = null;
            }
        }
    }

}

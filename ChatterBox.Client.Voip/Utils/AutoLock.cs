using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace ChatterBox.Client.Voip.Utils
{
    internal class AutoLock : IDisposable
    {
        private readonly SemaphoreSlim _sem;
        private bool _isLocked;

        public AutoLock(SemaphoreSlim sem)
        {
            _sem = sem;
        }

        public Task WaitAsync()
        {
            if (_isLocked) return Task.Run(() => { });
            _isLocked = true;
            var result = _sem.WaitAsync();
            return result;
        }

        public void Dispose()
        {
            if (_isLocked)
            {
                _sem.Release();
            }
        }
    }

}

namespace BlurMask;

/// <summary>
/// Prevents more than one BlurMask process in the current OS user/session.
/// Duplicate launches intentionally exit silently.
/// </summary>
internal sealed class SingleInstanceGuard : IDisposable
{
    // Keep the name short and portable: named Mutex works on Windows, Linux and macOS.
    private const string MutexName = "Xekep.BlurMask.SingleInstance.v1";

    private Mutex? _mutex;
    private bool _ownsMutex;

    private SingleInstanceGuard(Mutex mutex)
    {
        _mutex = mutex;
        _ownsMutex = true;
    }

    public static SingleInstanceGuard? TryAcquire()
    {
        Mutex? mutex = null;

        try
        {
            mutex = new Mutex(initiallyOwned: true, MutexName, out var createdNew);
            if (!createdNew)
            {
                mutex.Dispose();
                return null;
            }

            return new SingleInstanceGuard(mutex);
        }
        catch
        {
            // Failing to create the guard must not make the utility unusable.
            // This is deliberately silent: the app continues as a normal instance.
            mutex?.Dispose();
            return new SingleInstanceGuard(new Mutex());
        }
    }

    public void Dispose()
    {
        var mutex = Interlocked.Exchange(ref _mutex, null);
        if (mutex is null)
            return;

        if (_ownsMutex)
        {
            try
            {
                mutex.ReleaseMutex();
            }
            catch (ApplicationException)
            {
                // The mutex is no longer owned by this thread/process. Nothing to report.
            }

            _ownsMutex = false;
        }

        mutex.Dispose();
    }
}

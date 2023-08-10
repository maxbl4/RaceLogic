using System;
using System.Reactive.Disposables;
using System.Threading;

namespace maxbl4.Race.Logic.Extensions;

public class SyncLock
{
    private readonly object sync = new();
    
    public IDisposable Use()
    {
        Monitor.Enter(sync);
        return Disposable.Create((Action) (() => Monitor.Exit(sync)));
    }
}
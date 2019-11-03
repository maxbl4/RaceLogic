using System;
using System.Reactive.Disposables;
using Easy.MessageHub;

namespace maxbl4.RfidCheckpointService.Ext
{
    public static class MessageHubExt
    {
        public static IDisposable SubscribeDisposable<T>(this IMessageHub messageHub, Action<T> action)
        {
            var subId = messageHub.Subscribe(action);
            return Disposable.Create(() => messageHub.Unsubscribe(subId));
        }
    }
}
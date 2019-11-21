using System;
using Google.Cloud.PubSub.V1;

namespace EverStore.Messaging
{
    internal class DisposableSubscriber : IDisposable
    {
        private SubscriberClient _subscriberClient;

        public DisposableSubscriber(SubscriberClient subscriberClient)
        {
            _subscriberClient = subscriberClient;
        }

        public void Dispose()
        {
            _subscriberClient?.StopAsync(TimeSpan.FromSeconds(1));
            _subscriberClient = null;
        }
    }
}
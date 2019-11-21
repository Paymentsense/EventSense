using System;
using System.Threading;
using System.Threading.Tasks;
using EverStore.Contract;
using EverStore.Domain;
using EverStore.Tracing;
using Google.Cloud.PubSub.V1;
using OpenTracing;

namespace EverStore.Messaging
{
    internal class EventStreamSubscriber: IEventStreamSubscriber
    {
        private readonly SubscriberClient.Settings _subscriptionSettings;
        private readonly IEventStreamHandler _eventStreamHandler;
        private readonly ITracer _tracer;
        private SubscriberClient _subscriber;

        public EventStreamSubscriber(SubscriberClient.Settings subscriptionSettings, IEventStreamHandler eventStreamHandler, ITracer tracer)
        {
            _subscriptionSettings = subscriptionSettings;
            _eventStreamHandler = eventStreamHandler;
            _tracer = tracer;
        }
        public async Task SubscribeAsync(EventStreamSubscription subscription,
            Action<CatchUpSubscription, ResolvedEvent> eventAppeared,
            Action<CatchUpSubscription> liveProcessingStarted = null,
            Action<CatchUpSubscription, Exception> subscriptionDropped = null)
        {
            _subscriber = await SubscriberClient.CreateAsync(subscription.SubscriptionName, settings: _subscriptionSettings);

#pragma warning disable 4014
            //Disable warning because this task is fire and forget. Its handled internally by the PubSub library, please see the dispose for how to stop/cleanup.
            _subscriber.StartAsync(
#pragma warning restore 4014
                async (PubsubMessage message, CancellationToken cancel) =>
                {
                    if (cancel.IsCancellationRequested)
                    {
                        return await Task.FromResult(SubscriberClient.Reply.Nack);
                    }

                    if (!subscription.HasSubscribeToAllStream && string.Equals(message.Attributes[EventStreamAttributes.Stream], subscription.Stream, StringComparison.InvariantCulture))
                    {
                        return await Task.FromResult(SubscriberClient.Reply.Ack);
                    }

                    var span = _tracer.StartNewSpanChildFrom(message, subscription.SubscriptionName.SubscriptionId);
                    _tracer.ScopeManager.Activate(span, true);

                    try
                    {
                        var @event = message.ToModel();
                        var result = _eventStreamHandler.Handle(@event, subscription, eventAppeared, liveProcessingStarted);
                        return await Task.FromResult(result);
                    }
                    catch (Exception exception)
                    {
#pragma warning disable 4014
                        Task.Run(() =>
#pragma warning restore 4014
                        {
                            _subscriber.StopAsync(cancel);
                            subscriptionDropped?.Invoke(subscription.CatchUpSubscription, exception);
                        }, cancel);
                        
                        return await Task.FromResult(SubscriberClient.Reply.Nack);
                    }
                    finally
                    {
                        span.Finish();
                    }
                });
        }
    }
}

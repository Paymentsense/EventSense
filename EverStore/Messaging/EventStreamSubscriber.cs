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
        private readonly IEventSequencer _eventSequencer;
        private readonly ITracer _tracer;
        private SubscriberClient _subscriber;

        public EventStreamSubscriber(SubscriberClient.Settings subscriptionSettings, IEventSequencer eventSequencer, ITracer tracer)
        {
            _subscriptionSettings = subscriptionSettings;
            _eventSequencer = eventSequencer;
            _tracer = tracer;
        }
        public async Task SubscribeAsync(string subscribedStream,
            CatchUpSubscription catchUpSubscription,
            long? lastCheckpoint,
            SubscriptionName streamSubscription,
            Action<CatchUpSubscription, ResolvedEvent> eventAppeared,
            Action<CatchUpSubscription> liveProcessingStarted = null,
            Action<CatchUpSubscription, Exception> subscriptionDropped = null)
        {
            _subscriber = await SubscriberClient.CreateAsync(streamSubscription, settings: _subscriptionSettings);

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

                    var hasSubscribedToAllStream = string.Equals(subscribedStream, Stream.All, StringComparison.InvariantCulture);
                    if (!hasSubscribedToAllStream && string.Equals(message.Attributes[EventStreamAttributes.Stream], subscribedStream, StringComparison.InvariantCulture))
                    {
                        return await Task.FromResult(SubscriberClient.Reply.Ack);
                    }

                    var span = _tracer.StartNewSpanChildFrom(message, streamSubscription.SubscriptionId);
                    _tracer.ScopeManager.Activate(span, true);

                    try
                    {
                        var @event = message.ToModel();

                        var eventSequence = _eventSequencer.GetEventSequence(@event, lastCheckpoint, hasSubscribedToAllStream);
                        if (!eventSequence.IsInSequence)
                        {
                            if (eventSequence.IsInPast)
                            {
                                return await Task.FromResult(SubscriberClient.Reply.Ack);
                            }

                            return await Task.FromResult(SubscriberClient.Reply.Nack);
                        }

                        if (_eventSequencer.IsFirstEvent())
                        {
                            liveProcessingStarted?.Invoke(catchUpSubscription);
                        }

                        eventAppeared(catchUpSubscription, @event.ToDto());

                        _eventSequencer.IncrementEventSequence();

                        return await Task.FromResult(SubscriberClient.Reply.Ack);
                    }
                    catch (Exception exception)
                    {
                        await Task.Run(() => _subscriber.StopAsync(cancel), cancel);

                        subscriptionDropped?.Invoke(catchUpSubscription, exception);
                        
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

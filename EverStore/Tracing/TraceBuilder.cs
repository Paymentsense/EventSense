using System.Collections.Generic;
using System.Linq;
using Google.Cloud.PubSub.V1;
using OpenTracing;
using OpenTracing.Propagation;

namespace EverStore.Tracing
{
    public static class TraceBuilder
    {
        public static ISpan CreateSpan(this ITracer tracer, string message, string topic)
        {
            var builder = tracer.BuildSpan($"Publishing to: {topic}");
            builder.AsChildOf(tracer.ActiveSpan);
            var publisherSpan = builder.Start();
            publisherSpan.SetTag("message.data", message);
            return publisherSpan;
        }

        public static Dictionary<string, string> CreateSpanInformationCarrier(this ITracer tracer)
        {
            var spanInformationCarrier = new Dictionary<string, string>();
            var span = tracer.ActiveSpan;
            tracer.Inject(span?.Context, BuiltinFormats.TextMap, new TextMapInjectAdapter(spanInformationCarrier));
            return spanInformationCarrier;
        }

        public static ISpan StartNewSpanChildFrom(this ITracer tracer, PubsubMessage message, string subscription)
        {
            var spanInformationCarrier = message.Attributes.ToDictionary(pair => pair.Key, x => x.Value);
            var callingHeaders = new TextMapExtractAdapter(spanInformationCarrier);
            var callingSpanContext = tracer.Extract(BuiltinFormats.TextMap, callingHeaders);

            var operationName = $"Subscriber: {subscription}";
            var builder = tracer.BuildSpan(operationName);
            builder.AsChildOf(callingSpanContext);
            builder.WithTag("message.data", message.Data.ToStringUtf8());
            var span = builder.Start();
            return span;
        }
    }
}

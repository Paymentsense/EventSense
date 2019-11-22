using System;

namespace EverStore.Domain
{
    public static class Stream
    {
        public static string All => "$all";

        public static void Parse(string stream, out string streamAggregate, out string streamId)
        {
            if (string.IsNullOrWhiteSpace(stream))
            {
                throw new ArgumentException("Stream is empty", nameof(stream));
            }

            if (stream.Contains("-"))
            {
                throw new ArgumentException($"Stream cannot have hyphens '-' in it [{stream}], MongoDb will throw a duplicate key exception due to tokenization of hyphen in the text index.", nameof(stream));
            }

            var splitStream = stream.Split('_');

            if (splitStream.Length != 2)
            {
                throw new ArgumentException($"Stream is not formed correctly, expected: aggregate_id but got:[{stream}]", nameof(stream));
            }

            streamAggregate = splitStream[0];
            streamId = splitStream[1];

            if (string.IsNullOrWhiteSpace(streamAggregate))
            {
                throw new ArgumentException($"StreamAggregate is empty: {stream}", nameof(stream));
            }

            if (string.IsNullOrWhiteSpace(streamId))
            {
                throw new ArgumentException($"StreamId is empty: {stream}", nameof(stream));
            }
        }
    }
}

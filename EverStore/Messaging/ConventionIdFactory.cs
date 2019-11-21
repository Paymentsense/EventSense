namespace EverStore.Messaging
{
    public class ConventionIdFactory: IConventionIdFactory
    {
        private readonly string _prefix;
        private readonly string _postfix;
        private readonly string _subscriptionIdentifier;

        public ConventionIdFactory(string prefix, string postfix, string subscriptionIdentifier)
        {
            _prefix = prefix;
            _postfix = postfix;
            _subscriptionIdentifier = subscriptionIdentifier;
        }

        public string GetTopicId(string streamAggregate)
        {
            string topic = streamAggregate;

            if (!string.IsNullOrEmpty(_prefix))
            {
                topic = $"{_prefix}_{streamAggregate}";
            }
            
            if (!string.IsNullOrEmpty(_postfix))
            {
                topic = $"{topic}_{_postfix}";
            }

            return topic;
        }
        
        public string GetSubscriptionId(string streamAggregate)
        {
            string subscription = $"{_subscriptionIdentifier}_{streamAggregate}";

            if (!string.IsNullOrEmpty(_prefix))
            {
                subscription = $"{_prefix}_{subscription}";
            }
            
            if (!string.IsNullOrEmpty(_postfix))
            {
                subscription = $"{subscription}_{_postfix}";
            }

            return subscription;
        }
    }
}
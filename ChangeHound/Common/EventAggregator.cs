namespace ChangeHound.Common {
    public static class EventAggregator {
        private static readonly Dictionary<Type, List<object>> _subscribers = new();

        public static void Subscribe<T>(Action<T> action) {
            Type? messageType = typeof(T);
            if (!_subscribers.ContainsKey(messageType)) {
                _subscribers[messageType] = new List<object>();
            }
            _subscribers[messageType].Add(action);
        }

        public static void Publish<T>(T message) {
            Type? messageType = typeof(T);
            if (_subscribers.ContainsKey(messageType)) {
                foreach (object? subscriber in _subscribers[messageType]) {
                    (subscriber as Action<T>)?.Invoke(message);
                }
            }
        }
    }
}

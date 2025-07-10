namespace Sokio
{
    public class EventCallbackManager
    {
        private Dictionary<string, List<EventCallback>> _eventCallbacks;
        public EventCallbackManager()
        {
            _eventCallbacks = new Dictionary<string, List<EventCallback>>();
        }

        public void register(string eventName, EventCallback eventCallback)
        {
            if (!_eventCallbacks.ContainsKey(eventName))
            {
                _eventCallbacks[eventName] = [eventCallback];
            }
            _eventCallbacks[eventName].Add(eventCallback);
        }

        public void Execute(string eventName, MessageEventArgs msg)
        {
            if (_eventCallbacks.ContainsKey(eventName))
            {
                var callbacks = _eventCallbacks[eventName];
                foreach (var cb in callbacks)
                {
                    cb.Callback.Invoke(msg);
                }
            }
        }
    }
}
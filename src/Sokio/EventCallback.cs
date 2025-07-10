namespace Sokio
{
    public class EventCallback
    {
        private string _eventName;
        private Func<MessageEventArgs, Task> _callback;

        public EventCallback(string eventName, Func<MessageEventArgs, Task> callback)
        {

            _eventName = eventName;
            _callback = callback;
        }

        public Func<MessageEventArgs, Task> Callback
        {
            get { return _callback; }
        }

        public string EventName
        {
            get { return _eventName; }
        }






    }
}
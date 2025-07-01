namespace Sokio
{

	public abstract class Event
	{

		private string _eventName;

		public string EventName
		{
			get { return _eventName; }
			set { _eventName = value; }
		}

		public Event(string eventName, string data)
		{
			_eventName = eventName;
		}




	}
}

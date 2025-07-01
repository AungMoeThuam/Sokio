namespace Sokio
{
    public interface IEventTargetable
    {
        public void Emit(string eventName, object data);
    }
}
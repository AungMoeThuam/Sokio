
namespace Sokio
{
	public interface IEmitable
	{


		public void Emit(string eventName, object data);
	}
}

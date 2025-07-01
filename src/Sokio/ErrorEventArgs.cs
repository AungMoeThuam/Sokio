
namespace Sokio
{
    public class ErrorEventArgs : EventArgs
    {
        public Exception Exception { get; }
        public string Message { get; }

        public ErrorEventArgs(Exception exception)
        {
            Exception = exception;
            Message = exception.Message;
        }
    }

}
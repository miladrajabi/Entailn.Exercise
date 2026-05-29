namespace Entain.Application.Common.Exception;

public class CallGroupException : System.Exception
{
    public CallGroupException()
    {
    }

    public CallGroupException(string message)
        : base(message)
    {
    }

    public CallGroupException(string message, System.Exception innerException)
        : base(message, innerException)
    {
    }
}
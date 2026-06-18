namespace QRCodeManager.Application.Exceptions;

public class QrProcessingException : Exception
{
    public QrProcessingException(string message) : base(message)
    {
    }

    public QrProcessingException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}

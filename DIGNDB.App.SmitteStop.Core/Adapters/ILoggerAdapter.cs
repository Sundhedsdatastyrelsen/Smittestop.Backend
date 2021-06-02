namespace DIGNDB.App.SmitteStop.Core.Adapters
{
    public interface ILoggerAdapter<T>
    {
        void LogInformation(string message);
        void LogError(string message);
        void LogWarning(string message);

    }
}

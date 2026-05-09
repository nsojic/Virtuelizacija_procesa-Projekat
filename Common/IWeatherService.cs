using System.ServiceModel;


namespace Common
{
    [ServiceContract]
    public interface IWeatherService
    {
        [OperationContract]
        string StartSession(string meta);

        [OperationContract]
        string PushSample(WeatherSample sample);

        [OperationContract]
        string EndSession();
    }
}

using System.ServiceModel;


namespace Common
{
    [ServiceContract]
    public interface IWeatherService
    {
        [OperationContract]
        string StartSession(string meta);

        [OperationContract]
        [FaultContract(typeof(ValidationFault))]
        [FaultContract(typeof(DataFormatFault))]
        string PushSample(WeatherSample sample);

        [OperationContract]
        string EndSession();

    }
}

using System.ServiceModel;

namespace Common
{
    [ServiceContract]
    public interface IWeatherService
    {
        [OperationContract]
        ServiceResponse StartSession(SessionMetadata meta);

        [OperationContract]
        [FaultContract(typeof(ValidationFault))]
        [FaultContract(typeof(DataFormatFault))]
        ServiceResponse PushSample(WeatherSample sample);

        [OperationContract]
        ServiceResponse EndSession();

    }
}

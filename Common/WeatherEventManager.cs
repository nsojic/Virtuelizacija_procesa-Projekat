using Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service
{
    public class WeatherEventManager
    {
        public event EventHandler<WeatherEventArgs> OnTransferStarted;

        public event EventHandler<WeatherEventArgs> OnSampleReceived;

        public event EventHandler<WeatherEventArgs> OnTransferCompleted;

        public event EventHandler<WeatherEventArgs> OnWarningRaised;

        public void RaiseTransferStarted(string message)
        {
            OnTransferStarted?.Invoke(
                this,
                new WeatherEventArgs(message));
        }

        public void RaiseSampleReceived(string message)
        {
            OnSampleReceived?.Invoke(
                this,
                new WeatherEventArgs(message));
        }

        public void RaiseTransferCompleted(string message)
        {
            OnTransferCompleted?.Invoke(
                this,
                new WeatherEventArgs(message));
        }

        public void RaiseWarning(string message)
        {
            OnWarningRaised?.Invoke(
                this,
                new WeatherEventArgs(message));
        }
    }
}

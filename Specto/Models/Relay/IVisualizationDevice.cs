using System;  

namespace Specto.Relay
{
    public interface IVisualizationDevice : IDisposable, IVisualizationDataReceiver
    {
        bool IsActive { get; set; }
        string Name { get; }
        string SerialNumber { get; } 
        bool IsSerial { get; }
        bool IsWiFi { get; }
    }
}

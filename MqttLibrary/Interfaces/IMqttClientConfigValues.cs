using System.Collections.Generic;

namespace MqttLibrary.Interfaces
{
    interface IMqttClientConfigValues
    {
        string ClientId { get; set; }
        string URL { get; set; }
        int Port { get; set; }
        string User { get; set; }
        byte[] Password { get; set; }
        List<string> Topics { get; set; }
    }
}

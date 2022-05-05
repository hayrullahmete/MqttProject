using MqttLibrary.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MqttLibrary.Models
{
    public class MqttClientConfigValues : IMqttClientConfigValues
    {
        private static MqttClientConfigValues instance = null;
        private static readonly object padlock = new();
        public static MqttClientConfigValues Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (padlock)
                    {
                        if (instance == null)
                        {
                            instance = new MqttClientConfigValues();
                        }
                    }
                }
                return instance;
            }
        }
        public string ClientId { get; set; } = Guid.NewGuid().ToString("N");
        public string URL { get; set; }
        public int Port { get; set; }
        public string User { get; set; }
        public byte[] Password { get; set; }
        public List<string> Topics { get; set; } = new List<string>();
    }
}

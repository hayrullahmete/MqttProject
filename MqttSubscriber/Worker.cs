using MqttLibrary.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Client.Options;
using System;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MqttSubscriber
{
    public class Worker : BackgroundService
    {
        public static MqttClientConfigValues mqttClientConfigValues = new();
        public static MqttTopicFilter[] mqttTopicFilters = Array.Empty<MqttTopicFilter>();
        public static MqttClientOptions clientOptions = new();
        public IMqttClient mqttClient = null;
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                /// Baglanti saglandiginda yapilacak isler icin handler olusturuluyor.
                /// 
                #region UseConnectedHandler

                mqttClient.UseConnectedHandler(e =>
                {
                    Program.logger.LogInformation($"ClientId: {mqttClientConfigValues.ClientId} | Connected. | IsConnected: {mqttClient.IsConnected}");

                    mqttClientConfigValues.Topics.ForEach(t =>
                    {
                        //if (t.StartsWith(projectRootName))
                        {
                            Array.Resize(ref mqttTopicFilters, mqttTopicFilters.Length + 1);
                            mqttTopicFilters[mqttTopicFilters.Length - 1] = new MqttTopicFilter() { Topic = t };
                        }
                    });

                    /// Belirlenen topic listesine abone olunmasi saglaniyor.
                    mqttClient.SubscribeAsync(mqttTopicFilters);
                });

                #endregion UseConnectedHandler

                /// Mesaj geldiginde yapilacak isler icin handler olusturuluyor.
                /// 
                #region UseApplicationMessageReceivedHandler

                mqttClient.UseApplicationMessageReceivedHandler(e =>
                {
                    //JsonSerializer.Deserialize<object>(Encoding.UTF8.GetString(e.ApplicationMessage.Payload))
                    Program.logger.LogInformation($"ClientId: {mqttClientConfigValues.ClientId} | Received. | Topic: {e.ApplicationMessage.Topic} | QoS: {e.ApplicationMessage.QualityOfServiceLevel.ToString()} | Message: {e.ApplicationMessage.ConvertPayloadToString()}");
                    e.AcknowledgeAsync(stoppingToken);
                });

                #endregion UseApplicationMessageReceivedHandler

                /// Baglanti kapatildiginda yapilacak isler icin handler olusturuluyor.
                /// 
                #region UseDisconnectedHandler

                mqttClient.UseDisconnectedHandler(e =>
                {
                    Program.logger.LogInformation($"ClientId: {mqttClientConfigValues.ClientId} | Disconnected. | IsConnected: {mqttClient.IsConnected}");
                });

                #endregion UseDisconnectedHandler

                await mqttClient.ConnectAsync(clientOptions, stoppingToken);
            }
            catch (Exception ex)
            {
                Program.logger.LogError($"{Program.sourceName} | EXCEPTION: {ex.Message} | {ex.InnerException} | {ex.StackTrace}");
            }
        }
        public override async Task StartAsync(CancellationToken cancellationToken)
        {
            #region PrepareClient

            mqttClientConfigValues = new()
            {
                URL = Program.configuration.GetValue<string>("MQTT:Subscriber:URL"),
                Port = Program.configuration.GetValue<int>("MQTT:Subscriber:Port"),
                User = Program.configuration.GetValue<string>("MQTT:Subscriber:User"),
                Password = Encoding.ASCII.GetBytes(Program.configuration.GetValue<string>("MQTT:Subscriber:Password")),
                Topics = Program.configuration.GetValue<string>("MQTT:Subscriber:Topics").Split(';').ToList()
            };

            mqttClient = new MqttFactory().CreateMqttClient();

            clientOptions = (MqttClientOptions)(new MqttClientOptionsBuilder()
                .WithClientId(mqttClientConfigValues.ClientId)
                .WithTcpServer(mqttClientConfigValues.URL, mqttClientConfigValues.Port)
                .WithCredentials(new MqttClientCredentials() { Username = mqttClientConfigValues.User, Password = mqttClientConfigValues.Password })
                .WithCleanSession()
                .Build());

            #endregion PrepareClient

            // DO YOUR STUFF HERE
            await base.StartAsync(cancellationToken);
        }
        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            /// Belirlenen ve abone olunmus topic listesinden cikis yapiliyor.
            foreach (var topicFilter in mqttTopicFilters)
            {
                mqttClient.UnsubscribeAsync(topicFilter.Topic).Wait();
                Program.logger.LogInformation($"ClientId: {mqttClientConfigValues.ClientId} | Unsubscribed. | {topicFilter.Topic}");
            }

            await mqttClient.DisconnectAsync(cancellationToken);
        }
    }
}

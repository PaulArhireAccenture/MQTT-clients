using System;
using System.Threading.Tasks;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Client.Options;
using Newtonsoft.Json;

namespace MQTTpublisher
{
    class DTO
    {
        public string data;
        public int number;
        public DTO(string data, int nr)
        {
            this.data = data;
            this.number = nr;
        }
    }
    class Publisher
    {
        static async Task Main(string[] args)
        {
            var watch = new System.Diagnostics.Stopwatch();

            var client = new MqttFactory().CreateMqttClient();
            var options = new MqttClientOptionsBuilder().WithClientId(Guid.NewGuid().ToString())
                                                        .WithTcpServer("test.mosquitto.org", 1883)
                                                        .WithCleanSession().Build();

            client.UseConnectedHandler(e => {
                Console.WriteLine("Connected");
            });

            client.UseApplicationMessageReceivedHandler(e =>
            {
                var recv = System.Text.Encoding.UTF8.GetString(e.ApplicationMessage.Payload);
                Console.WriteLine($"Recived message: {JsonConvert.DeserializeObject<DTO>(recv).data} \n" +
                    $"Recived number: {JsonConvert.DeserializeObject<DTO>(recv).number}");
                watch.Stop();
                Console.WriteLine($"Time elapsed: {watch.ElapsedMilliseconds} ms");
            });


            await client.ConnectAsync(options);

            var dto = new DTO("Publisher msg", 32);
            
            var message = new MqttApplicationMessageBuilder().WithTopic("PaulMqttTest")
                                                             .WithPayload(JsonConvert.SerializeObject(dto))
                                                             .WithAtLeastOnceQoS()
                                                             .Build();
            if (client.IsConnected) {

                await client.PublishAsync(message);

                watch.Start();

                var topicFilter = new TopicFilterBuilder().WithTopic("PaulMqttTest").Build();
                await client.SubscribeAsync(topicFilter);

                Console.ReadLine();

                await client.DisconnectAsync();

                Console.ReadLine();
            }
        }
    }
}

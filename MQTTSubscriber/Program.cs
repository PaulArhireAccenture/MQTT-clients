using System;
using System.Text;
using System.Threading.Tasks;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Client.Options;
using Newtonsoft.Json;

namespace MQTTSubscrier
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

    class Subscriber
    {
        static async Task Main(string[] args)
        {
            var client = new MqttFactory().CreateMqttClient();
            var options = new MqttClientOptionsBuilder().WithClientId(Guid.NewGuid().ToString())
                                                        .WithTcpServer("test.mosquitto.org", 1883)
                                                        .WithCleanSession().Build();
            
         

            client.UseConnectedHandler(async e => {
                Console.WriteLine("Connected");
                var topicFilter = new MqttTopicFilterBuilder().WithTopic("PaulMqttTest").Build();
                await client.SubscribeAsync(topicFilter);
                
                
                

            });

            client.UseApplicationMessageReceivedHandler(async e =>
            {
                string recv = Encoding.UTF8.GetString(e.ApplicationMessage.Payload);
                Console.WriteLine($"Recived message: {JsonConvert.DeserializeObject<DTO>(recv).data} \n" +
                    $"Recived number: {JsonConvert.DeserializeObject<DTO>(recv).number}");
                await client.UnsubscribeAsync("PaulMqttTest");
                var dto = new DTO("Subscriber msg", 24);
                var message = new MqttApplicationMessageBuilder().WithTopic("PaulMqttTest")
                                                                .WithPayload(JsonConvert.SerializeObject(dto))
                                                                .WithAtLeastOnceQoS()
                                                                .Build();

                await client.PublishAsync(message);
            });

            await client.ConnectAsync(options);


            if (client.IsConnected)
            {
                Console.ReadLine();
                await client.DisconnectAsync();
            }
        }
    }
}

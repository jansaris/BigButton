using System;
using System.Text;
using System.Threading.Tasks;
using Microsoft.ServiceBus.Messaging;

namespace iot2eventhub
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                string connectionString = "HostName=ButtonIotHub.azure-devices.net;SharedAccessKeyName=iothubowner;SharedAccessKey=KvRHqYFFbp8NazRg8fRf/myvpJIq0QcbsOYmRUcWtLs=";
                string hub = "devices/BigButtonDevice";
                string iotHubD2cEndpoint = "messages/events";

                var eventHubClient = EventHubClient.CreateFromConnectionString(connectionString, iotHubD2cEndpoint);

                var d2cPartitions = eventHubClient.GetRuntimeInformation().PartitionIds;
                foreach (string partition in d2cPartitions)
                {
                    var receiver = eventHubClient.GetDefaultConsumerGroup().CreateReceiver(partition, DateTime.Now);
                    ReceiveMessagesFromDeviceAsync(receiver);
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
            Console.ReadLine();
        }

        async static Task ReceiveMessagesFromDeviceAsync(EventHubReceiver receiver)
        {
            while (true)
            {
                EventData eventData = await receiver.ReceiveAsync();
                if (eventData == null) continue;

                string data = Encoding.UTF8.GetString(eventData.GetBytes());
                Console.WriteLine("Message received: '{0}'", data);
                SendMessageToEventHub(data);
            }
        }
        
        static void SendMessageToEventHub(string message)
        {
            var connectionString = "Endpoint=sb://bigbuttoneventshub.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=j5w5I9/FDcpDnV4acDpzbYQaj97K0bifHyEzwwWfQPQ=";
            var eventHubName = "rawmessages";
            var eventHubClient = EventHubClient.CreateFromConnectionString(connectionString, eventHubName);
            try
            {
                eventHubClient.Send(new EventData(Encoding.UTF8.GetBytes(message)));
                Console.WriteLine("{0} > Sended message: {1}", DateTime.Now, message);
            }
            catch (Exception exception)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("{0} > Exception: {1}", DateTime.Now, exception.Message);
                Console.ResetColor();
            }
        }
    }
}

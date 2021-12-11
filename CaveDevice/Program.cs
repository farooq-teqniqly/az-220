using System;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.Devices.Client;
using Newtonsoft.Json;

namespace CaveDevice
{
    internal class Program
    {
        private static DeviceClient deviceClient;
        private static readonly string connectionString = "HostName=iot-az220-training-fm211211.azure-devices.net;DeviceId=sensor-th-0001;SharedAccessKey=tTOmVRMRlykzF1K10AhyQf0Kqw/0t8N4UUn+PztrzEw=";

        private static async Task Main()
        {
            deviceClient = DeviceClient.CreateFromConnectionString(connectionString, TransportType.Amqp);
            await SendDeviceToCloudMessagesAsync();
            Console.ReadLine();
        }

        private static async Task SendDeviceToCloudMessagesAsync()
        {
            var sensor = new EnvironmentSensor();

            while (true)
            {
                var currentTemp = sensor.ReadTemp();
                var currentHumidity = sensor.ReadHumidity();
                var messageString = CreateMessageString(currentTemp, currentHumidity);
                
                var message = new Message(Encoding.ASCII.GetBytes(messageString));
                message.Properties.Add("tempAlert", currentTemp > 30 ? "true" : "false");

                await deviceClient.SendEventAsync(message);
                Console.WriteLine($"[{DateTime.Now}]\t{messageString}");

                await Task.Delay(TimeSpan.FromSeconds(1));
            }
        }

        private static string CreateMessageString(double currentTemp, double currentHumidity)
        {
            var telemetry = new
            {
                temp = currentTemp,
                humidity = currentHumidity
            };

            return JsonConvert.SerializeObject(telemetry);
        }
    }

    internal class EnvironmentSensor
    {
        private static readonly double tempBias = 20.0;
        private static readonly double humidityBias = 60.0;
        private static readonly Random rand = new();

        public double ReadTemp()
        {
            return rand.NextDouble() + tempBias;
        }

        public double ReadHumidity()
        {
            return rand.NextDouble() + humidityBias;
        }
    }
}

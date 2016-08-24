using Microsoft.Azure.Devices.Client;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using voicelight.core.AzureSuite;
using Windows.ApplicationModel.Core;
using Windows.Devices.Geolocation;
using Windows.UI.Core;
using Windows.UI.Popups;

namespace voicelight.core
{
    namespace AzureSuite
    {
        [DataContract]
        public class DeviceProperties
        {
            [DataMember]
            internal string DeviceID;

            [DataMember]
            internal bool HubEnabledState;

            [DataMember]
            internal string CreatedTime;

            [DataMember]
            internal string DeviceState;

            [DataMember]
            internal string UpdatedTime;

            [DataMember]
            internal string Manufacturer;

            [DataMember]
            internal string ModelNumber;

            [DataMember]
            internal string SerialNumber;

            [DataMember]
            internal string FirmwareVersion;

            [DataMember]
            internal string Platform;

            [DataMember]
            internal string Processor;

            [DataMember]
            internal string InstalledRAM;

            [DataMember]
            internal double Latitude;

            [DataMember]
            internal double Longitude;

        }

        [DataContract]
        public class CommandParameter
        {
            [DataMember]
            internal string Name;

            [DataMember]
            internal string Type;
        }

        [DataContract]
        public class Command
        {
            [DataMember]
            internal string Name;

            [DataMember]
            internal CommandParameter[] Parameters;
        }

        [DataContract]
        public class Thermostat
        {
            [DataMember]
            internal DeviceProperties DeviceProperties;

            [DataMember]
            internal Command[] Commands;

            [DataMember]
            internal bool IsSimulatedDevice;

            [DataMember]
            internal string Version;

            [DataMember]
            internal string ObjectType;
        }

        [DataContract]
        public class TelemetryData
        {
            [DataMember]
            internal string DeviceId;

            [DataMember]
            internal double Temperature;

            [DataMember]
            internal double Humidity;

            [DataMember]
            internal double ExternalTemperature;

            [DataMember]
            internal double PM25;

            [DataMember]
            internal double PM10;

        }

        public class AzureWorker
        {
            static DeviceClient deviceClient;
            string iotHubUri = "eyesonair.azure-devices.net";
            string deviceId = "Air_RaspBerry01";
            string deviceKey = "yaVTkbMCVgvqTPyXbmPrgA==";

            public AzureWorker(string uri, string id, string key)
            {
                iotHubUri = uri;
                deviceId = id;
                deviceKey = key;
            }
            public Boolean Connect()
            {
                try
                {
                    deviceClient = DeviceClient.Create(iotHubUri, new DeviceAuthenticationWithRegistrySymmetricKey(deviceId, deviceKey));
                    sendDeviceMetaData();
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                }
                return true;
            }

            public async Task Disconnect()
            {
               await deviceClient.CloseAsync();
            } 
            private byte[] Serialize(object obj)
            {
                string json = JsonConvert.SerializeObject(obj);
                return Encoding.UTF8.GetBytes(json);

            }

            private dynamic DeSerialize(byte[] data)
            {
                string text = Encoding.UTF8.GetString(data);
                return JsonConvert.DeserializeObject(text);
            }

            private async void sendDeviceMetaData()
            {
                DeviceProperties device = new DeviceProperties();
                Thermostat thermostat = new Thermostat();

                thermostat.ObjectType = "DeviceInfo";
                thermostat.IsSimulatedDevice = false;
                thermostat.Version = "1.0";

                device.HubEnabledState = true;
                device.DeviceID = deviceId;
                device.Manufacturer = "DigiGirlz";
                device.ModelNumber = "RaspBerryPi3";
                device.SerialNumber = "10111011";
                device.FirmwareVersion = "10";
                device.Platform = "Windows 10";
                device.Processor = "SnapDragon";
                device.InstalledRAM = "3GB";
                device.DeviceState = "normal";

                try
                {
                    Geolocator geolocator = new Geolocator();
                    Geoposition pos = await geolocator.GetGeopositionAsync();

                    device.Latitude = (float)pos.Coordinate.Point.Position.Latitude;
                    device.Longitude = (float)pos.Coordinate.Point.Position.Longitude;
                }
                catch
                {
                    device.Latitude = (float)31;
                    device.Longitude = (float)121;
                }
                thermostat.DeviceProperties = device;

                Command TriggerAlarm = new Command();
                TriggerAlarm.Name = "TriggerAlarm";
                CommandParameter param = new CommandParameter();
                param.Name = "Message";
                param.Type = "String";
                TriggerAlarm.Parameters = new CommandParameter[] { param };

                Command Power = new Command();
                Power.Name = "Power";

                CommandParameter port = new CommandParameter();
                port.Name = "port";
                port.Type = "int";

                CommandParameter paramPower = new CommandParameter();
                paramPower.Name = "status";
                paramPower.Type = "Boolean";
                Power.Parameters = new CommandParameter[] { port, paramPower };

                thermostat.Commands = new Command[] { TriggerAlarm, Power };

                try
                {
                    var msg = new Message(Serialize(thermostat));
                    if (deviceClient != null)
                    {
                        await deviceClient.SendEventAsync(msg);
                    }
                }
                catch (System.Exception e)
                {
                    Debug.Write("Exception while sending device meta data :\n" + e.Message.ToString());
                }

            }

            public async Task receiveDataFromAzure()
            {

                Message message = await deviceClient.ReceiveAsync();

                if (message != null)
                {
                    try
                    {
                        dynamic command = DeSerialize(message.GetBytes());

                        if (command.Name == "TriggerAlarm")
                        {

                            // Received a new message, display it
                            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
                            async () =>
                            {
                                var dialogbox = new MessageDialog("Received message from Azure IoT Hub: " + command.Parameters.Message.ToString());
                                await dialogbox.ShowAsync();
                            });
                            // We received the message, indicate IoTHub we treated it

                        }

                        if (command.Name == "Power")
                        {
                            //Boolean bl = true;

                            //Boolean.TryParse(command.Parameters.status.ToString(), out bl);

                            GpioWorker.Process(JsonConvert.SerializeObject(command.Parameters));

                        }
                        await deviceClient.CompleteAsync(message);
                    }
                    catch
                    {
                        await deviceClient.RejectAsync(message);
                    }
                }
            }
        }
    }
}

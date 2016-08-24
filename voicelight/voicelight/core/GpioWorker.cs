using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Data.Json;
using Windows.Devices.Gpio;

namespace voicelight
{
   
    class GpioWorker
    {
        static GpioController controller;
        static Dictionary<int, GpioPin> num2pin;
        internal class Command
        {
            [JsonProperty("port")]
            public int port
            {
                get;
                set;
            }

            [JsonProperty("status")]
            public Boolean status
            {
                get;
                set;
            }
        }
        public static void InitGPIO()
        {
            if (controller == null)
                controller = GpioController.GetDefault();

            if (num2pin == null)
                num2pin = new Dictionary<int, GpioPin>();

            
        }

        public static void DestroyGPIO()
        {
            foreach (var pair in num2pin)
            {
                pair.Value.Dispose();
            }

            num2pin.Clear();
        }

        public static void Process(String data)
        {
            List<Command> cmds = null;
            try
            {
                cmds = JsonConvert.DeserializeObject<List<Command>>("["+data+"]");
            }
            catch (Exception e)
            {
                // System.Diagnostics.Debug.WriteLine(e.Message);
                return;
            }

            foreach (var cmd in cmds)
            {
                try
                {
                    var pin = GetPin(cmd.port);
                    if (pin != null)
                    {
                        pin.Write((cmd.status == true) ? GpioPinValue.Low : GpioPinValue.High);
                        pin.SetDriveMode(GpioPinDriveMode.Output);
                        System.Diagnostics.Debug.WriteLine("set port " + cmd.port + " " + cmd.status);
                    }
                }
                catch (Exception e)
                {
                     System.Diagnostics.Debug.WriteLine(e.Message);
                }
            }
        }

        public static GpioPin GetPin(int num)
        {
            if (num2pin.ContainsKey(num))
            {
                return num2pin[num];
            }

            var pin = controller.OpenPin(num);
            if (pin == null)
                throw new Exception("Invalid Pin : " + num);

            num2pin[num] = pin;
            
            return pin;
        }
    }
}

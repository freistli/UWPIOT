﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.Data.Json;
using Windows.Devices.Gpio;
using Windows.UI.Xaml;

namespace voicelight
{

    class GpioWorker
    {
        static GpioController controller;
        static Dictionary<int, GpioPin> num2pin;
        static Dictionary<int, GpioPinValue> num2pinValue;
        static Timer timer ;
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

            if (num2pinValue == null)
                num2pinValue = new Dictionary<int, GpioPinValue>();

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
                cmds = JsonConvert.DeserializeObject<List<Command>>("[" + data + "]");
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
                        GpioPinValue gpv = (cmd.status == true) ? GpioPinValue.Low : GpioPinValue.High;
                        pin.Write(gpv);
                        pin.SetDriveMode(GpioPinDriveMode.Output);
                        num2pinValue[cmd.port] = gpv;
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

        public static GpioPinValue GetPortStatus(int port)
        {
            if (num2pinValue.ContainsKey(port))
                return (num2pinValue[port]);
            else
                return GpioPinValue.High;
        }
        public static void flash ()
        {
            timer = new Timer(
                (x) => Timer_Tick(x),null,10000,1000
                );
            return;
        }
        public static void stopFlash()
        {
            if (timer != null)
                timer.Dispose();
            return;
        }
        public static void Timer_Tick(object e)
        {
            GpioPinValue gpv = GetPortStatus(5);
            GpioPin pin;
            pin = GetPin(5);
            if (pin != null)
            {
                gpv = (gpv != GpioPinValue.Low) ? GpioPinValue.Low : GpioPinValue.High;
                pin.Write(gpv);
                pin.SetDriveMode(GpioPinDriveMode.Output);
                num2pinValue[5] = gpv;
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.Media.SpeechRecognition;
using Windows.UI.Core;
using Windows.Networking;
using Windows.Networking.Connectivity;
using Windows.Devices.Gpio;
using System.Threading;
using Windows.ApplicationModel.Core;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace voicelight
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        Timer timer;

        public MainPage()
        {
            this.InitializeComponent();

            foreach (HostName localHostName in NetworkInformation.GetHostNames())
            {
                if (localHostName.IPInformation != null)
                {
                    if (localHostName.Type == HostNameType.Ipv4)
                    {
                        LocalIP.Text = "Device Local IP is: " + localHostName.ToString();
                        break;
                    }
                }
            }

            timer = new Timer(
                async (x) =>
                {                 
                    await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(
                    CoreDispatcherPriority.Normal, () =>
                     {
                         LED.Text = "LED 5 is " + ((GpioWorker.GetPortStatus(5) == GpioPinValue.Low) ? "on" : "off");
                     });      
                },
                null,
                0,
                500
            );

           
        }
    }
}

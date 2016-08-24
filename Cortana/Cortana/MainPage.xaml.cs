using Microsoft.Azure.Devices;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;


// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace Cortana
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
        }

        private SolidColorBrush blueBrush = new SolidColorBrush(Windows.UI.Colors.Blue);
        private SolidColorBrush grayBrush = new SolidColorBrush(Windows.UI.Colors.LightGray);

        private Boolean status = false;
        static string iotHubUri = "eyesonair.azure-devices.net";
        static string deviceId = "Air_RaspBerry01";
        static string deviceKey = "ls0x0E2svbdpq0I/ruG5hYibzO24uUpIiIbIaXJJccg=";
        static string connectionString = "HostName=" + iotHubUri + ";DeviceId=" + deviceId + ";SharedAccessKeyName=iothubowner;SharedAccessKey=" + deviceKey;

       
        private async void StackPanel_Tapped(object sender, TappedRoutedEventArgs e)
        {
            status = status == false ? true : false;

            string messageString = "[]";
            if (status)
                messageString = "{\"Name\":\"Power\",\"Parameters\":{\"port\":5,\"status\":true}}";
            else
                messageString = "{\"Name\":\"Power\",\"Parameters\":{\"port\":5,\"status\":false}}";

            var serviceClient = ServiceClient.CreateFromConnectionString(connectionString);
            System.Diagnostics.Debug.WriteLine(messageString);
            txtStatus.Text = "Connected to Azure: " + connectionString;
            byte[] bts = System.Text.Encoding.UTF8.GetBytes(messageString);
            Message message = new Message(bts);
            await serviceClient.SendAsync(deviceId, message);            
            await serviceClient.CloseAsync();
            txtStatus.Text = "Message was sent to device through Azure: " + messageString;
            if (status)
                LED.Fill = blueBrush;
            else
                LED.Fill = grayBrush;
        }
    }
}

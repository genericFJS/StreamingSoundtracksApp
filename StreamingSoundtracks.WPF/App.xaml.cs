using System.Diagnostics;
using System.Windows;

namespace StreamingSoundtracks
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {

        public static void OpenHyperlink(string uri)
        {
            Process.Start(new ProcessStartInfo(uri));
        }
    }
}

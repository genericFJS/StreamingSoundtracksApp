using StreamingSoundtracks.Core;
using System.Windows;

namespace StreamingSoundtracks
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class DebugWindow : Window
    {
        private StreamPlayer streamPlayer;

        public DebugWindow(StreamPlayer dataContext) : this()
        {
            streamPlayer = dataContext;
        }

        public void StreamPlayer_StreamingStarted(object sender, System.EventArgs e)
        {
            DataContext = streamPlayer.BufferedStream;
        }

        public DebugWindow()
        {
            InitializeComponent();
        }
    }
}

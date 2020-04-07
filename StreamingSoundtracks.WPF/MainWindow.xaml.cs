using StreamingSoundtracks.Core;
using System.ComponentModel;
using System.Windows;

namespace StreamingSoundtracks
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        private double imageHeight = 999;
        private bool autoplay = Properties.Settings.Default.Autoplay;
        private bool showQueueHistoryEstimates = Properties.Settings.Default.ShowQueueHistoryEstimates;
        private bool showQueueHistoryHeader = Properties.Settings.Default.ShowQueueHistoryHeader;
        public double ImageHeight
        {
            get { return imageHeight; }
            set
            {
                imageHeight = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ImageHeight)));
            }
        }

        public bool Autoplay
        {
            get
            {
                return autoplay;
            }
            set
            {
                autoplay = value;
                Properties.Settings.Default.Autoplay = Autoplay;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Autoplay)));
            }
        }
        public bool ShowQueueHistoryEstimates
        {
            get
            {
                return showQueueHistoryEstimates;
            }
            set
            {
                showQueueHistoryEstimates = value;
                Properties.Settings.Default.ShowQueueHistoryEstimates = ShowQueueHistoryEstimates;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ShowQueueHistoryEstimates)));
            }
        }
        public bool ShowQueueHistoryHeader
        {
            get
            {
                return showQueueHistoryHeader;
            }
            set
            {
                showQueueHistoryHeader = value;
                Properties.Settings.Default.ShowQueueHistoryHeader = ShowQueueHistoryHeader;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ShowQueueHistoryHeader)));
            }
        }

        private Coordinator Coordinator { get; set; } = new Coordinator();
        private CurrentPlayingControl CurrentPlayingControl { get; set; } = new CurrentPlayingControl();
        private SettingsControl SettingsControl { get; set; } = new SettingsControl();

        public event PropertyChangedEventHandler PropertyChanged;

        public MainWindow()
        {
            InitializeComponent();

            Queue.DataContext = Coordinator.Playlist;
            History.DataContext = Coordinator.Playlist;
            PlaybackControls.DataContext = Coordinator.StreamPlayer;

            CurrentPlayingControl.DataContext = Coordinator.Playlist.CurrentPlaying;
            SettingsControl.DataContext = this;
            SettingsControl.BackButton.Click += SettingsButton_Click;

            CenterControl.Navigate(CurrentPlayingControl);

            if (Autoplay)
                Coordinator.StartAudioStream();

            Window_SizeChanged(null, null);
        }

        private void StartStopButton_Click(object sender, RoutedEventArgs e)
        {
            if (Coordinator.StreamPlayer.IsPlaying)
                Coordinator.StopAudioStream();
            else
                Coordinator.StartAudioStream();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Coordinator.StopAudioStream();
            Properties.Settings.Default.Save();
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            Properties.Settings.Default.WindowHeight = Window.Height;
            Properties.Settings.Default.WindowWidth = Window.Width;
            if (Queue.ActualHeight < Window.ActualWidth / 10)
            {
                ImageHeight = Window.ActualWidth / 10;
            }
            else
            {
                ImageHeight = 0;
            }
        }

        private void MuteButton_Click(object sender, RoutedEventArgs e)
        {
            Coordinator.StreamPlayer.IsMuted = !Coordinator.StreamPlayer.IsMuted;
        }

        private void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            if (CenterControl.Content.GetType() == typeof(SettingsControl))
                CenterControl.Navigate(CurrentPlayingControl);
            else
                CenterControl.Navigate(SettingsControl);
        }
    }
}

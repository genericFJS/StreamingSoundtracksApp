using System.IO;
using Vlc.DotNet.Core;

namespace StreamingSoundtracks.Core
{
    public class Coordinator
    {
        public Playlist Playlist { get; set; }
        public StreamPlayer StreamPlayer { get; set; }

        private long lastUpdatedTimeToCurrentPlaying = 0;
        private long lastUpdatedPlaybackPosition = 0;

        public Coordinator(DirectoryInfo libVlcDirectory)
        {
            StreamPlayer = new StreamPlayer(libVlcDirectory);
            Playlist = new Playlist(StreamPlayer);
        }

        private void VlcMediaPlayer_Stopped(object sender, VlcMediaPlayerStoppedEventArgs e)
        {
            Playlist.StopQueuedUpdate();
        }

        private void VlcMediaPlayer_TimeChanged(object sender, Vlc.DotNet.Core.VlcMediaPlayerTimeChangedEventArgs e)
        {
            if (lastUpdatedPlaybackPosition + 1000 < e.NewTime)
            {
                Playlist.CurrentPlaying.UpdatePlaybackPosition();
                lastUpdatedPlaybackPosition = e.NewTime;
            }
            if (lastUpdatedTimeToCurrentPlaying + 10000 < e.NewTime)
            {
                Playlist.UpdateTimeToCurrentPlaying();
                lastUpdatedTimeToCurrentPlaying = e.NewTime;
            }
        }

        ~Coordinator()
        {
            Properties.Settings.Default.Save();
        }

        public void StartAudioStream()
        {
            lastUpdatedTimeToCurrentPlaying = 0;
            lastUpdatedPlaybackPosition = 0;
            StreamPlayer.StartStreaming();
            StreamPlayer.VlcMediaPlayer.TimeChanged += VlcMediaPlayer_TimeChanged;
            StreamPlayer.VlcMediaPlayer.Stopped += VlcMediaPlayer_Stopped;
            Playlist.UpdateAllPlaylists();
        }

        public void StopAudioStream()
        {
            StreamPlayer.StopStreaming();
        }
    }
}

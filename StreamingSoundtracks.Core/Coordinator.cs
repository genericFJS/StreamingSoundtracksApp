namespace StreamingSoundtracks.Core
{
    public class Coordinator
    {
        public Playlist Playlist { get; set; } = new Playlist();
        public StreamPlayer StreamPlayer { get; set; } = new StreamPlayer();

        public Coordinator()
        {
            StreamPlayer.NextTrackStarting += StreamPlayer_NextTrackStarting;
            StreamPlayer.PlaybackSecondElapsed += StreamPlayer_PlaybackSecondElapsed;
            StreamPlayer.PlaybackTenSecondsElapsed += StreamPlayer_PlaybackTenSecondsElapsed;
        }

        ~Coordinator()
        {
            Properties.Settings.Default.Save();
        }

        private void StreamPlayer_PlaybackTenSecondsElapsed(object sender, System.EventArgs e)
        {
            Playlist.UpdateTimeToCurrentPlaying();
        }

        private void StreamPlayer_PlaybackSecondElapsed(object sender, System.EventArgs e)
        {
            Playlist.CurrentPlaying.UpdatePlaybackPosition();
        }

        private void StreamPlayer_NextTrackStarting(object sender, System.EventArgs e)
        {
            Playlist.TryUpdatePlaylists();
        }

        public void StartAudioStream()
        {
            Playlist.UpdatePlaylists();
            StreamPlayer.StartStreaming();
        }

        public void StopAudioStream()
        {
            StreamPlayer.StopStreaming();
        }
    }
}

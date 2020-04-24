using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace StreamingSoundtracks.Core
{
    public class Playlist
    {
        private const string CURRENT_PLAYING_URL = "http://www.streamingsoundtracks.com/soap/FM24sevenJSON.php?action=GetCurrentlyPlaying";
        private const string HISTORY_URL = "http://www.streamingsoundtracks.com/soap/FM24sevenJSON.php?action=GetHistory";
        private const string QUEUE_URL = "http://www.streamingsoundtracks.com/soap/FM24sevenJSON.php?action=GetQueue";

        public int MaxPlaylistLength { get; set; } = 10;

        public CurrentPlaying CurrentPlaying { get; set; } = new CurrentPlaying();
        public ObservableCollection<PlaybackEntry> History { get; private set; } = new ObservableCollection<PlaybackEntry>();
        public ObservableCollection<PlaybackEntry> Queue { get; private set; } = new ObservableCollection<PlaybackEntry>();

        private StreamPlayer StreamPlayer { get; }
        private bool PlaybackUpdateQueued { get; set; } = false;

        private readonly TaskScheduler guiDispatcher;
        private CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

        public Playlist(StreamPlayer streamPlayer)
        {
            StreamPlayer = streamPlayer;
            if (SynchronizationContext.Current == null)
                SynchronizationContext.SetSynchronizationContext(new SynchronizationContext());
            guiDispatcher = TaskScheduler.FromCurrentSynchronizationContext();
            UpdateAllPlaylists(true);
        }

        public void UpdateAllPlaylists(bool forceUpdate = false)
        {
            if (!StreamPlayer.IsPlaying && !forceUpdate)
                return;
            PlaybackUpdateQueued = false;
            using (WebClient webClient = new WebClient())
            {
                UpdatePlaylist(webClient, HISTORY_URL, History);
                UpdatePlaylist(webClient, QUEUE_URL, Queue);

                var currentPlayingJSON = webClient.DownloadString(CURRENT_PLAYING_URL);
                var currentPlaying = JsonConvert.DeserializeObject<CurrentPlaying>(currentPlayingJSON);
                CurrentPlaying.Update(currentPlaying);

                UpdateTimeToCurrentPlaying();
            }
            if (cancellationTokenSource.IsCancellationRequested)
                cancellationTokenSource = new CancellationTokenSource();
            _ = Task.Run(async () =>
            {
                var ms = Convert.ToInt32(CurrentPlaying.PlaybackPositionFromEnd.TotalMilliseconds);
                try
                {
                    await Task.Delay(ms, cancellationTokenSource.Token);
                    await Task.Factory.StartNew(() => { UpdateAllPlaylists(); }, CancellationToken.None, TaskCreationOptions.None, guiDispatcher);
                }
                catch (TaskCanceledException) { }
            });
        }

        public void StopQueuedUpdate()
        {
            cancellationTokenSource.Cancel();
        }

        private void UpdatePlaylist(WebClient webClient, string url, ObservableCollection<PlaybackEntry> playlist)
        {
            var json = webClient.DownloadString(url);
            var newPlaylist = JsonConvert.DeserializeObject<List<PlaybackEntry>>(json);
            foreach (var entry in newPlaylist)
            {
                if (!playlist.Contains(entry))
                {
                    playlist.Add(entry);
                    if (playlist.Count > MaxPlaylistLength)
                        playlist.RemoveAt(0);
                }
            }
        }

        internal void UpdateTimeToCurrentPlaying()
        {
            var timeSum = TimeSpan.Zero;
            var previousTrackLength = TimeSpan.Zero;
            foreach (var entry in Queue)
            {
                if (timeSum == TimeSpan.Zero)
                    timeSum = CurrentPlaying.PlaybackPositionFromEnd;
                else
                    timeSum += previousTrackLength;
                previousTrackLength = entry.Length;

                entry.TimeToCurrentPlaying = timeSum;
            }

            timeSum = TimeSpan.Zero;
            foreach (var entry in History)
            {
                if (timeSum == TimeSpan.Zero)
                    timeSum = CurrentPlaying.PlaybackPositionFromStart;
                else
                    timeSum += previousTrackLength;
                previousTrackLength = entry.Length;

                entry.TimeToCurrentPlaying = timeSum;
            }
        }
    }
}

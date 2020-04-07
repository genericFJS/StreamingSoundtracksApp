using System;
using System.Web;

namespace StreamingSoundtracks.Core
{
    public class CurrentPlaying : PlaybackEntry
    {

        private string artist;
        private DateTime playStart;
        private DateTime systemTime;
        private int listenerCount;
        private readonly int OFFSET = 15;

        public string Artist
        {
            get
            {
                return artist;
            }
            set
            {
                artist = HttpUtility.HtmlDecode(value);
                RaisePropertyChanged(nameof(Artist));
            }
        }
        public DateTime PlayStart
        {
            get
            {
                return playStart;
            }
            set
            {
                playStart = value;
                RaisePropertyChanged(nameof(PlayStart));
            }
        }
        public DateTime SystemTime
        {
            get
            {
                return systemTime;
            }
            set
            {
                systemTime = value;
                RaisePropertyChanged(nameof(SystemTime));
            }
        }
        public int ListenerCount
        {
            get
            {
                return listenerCount;
            }
            set
            {
                listenerCount = value;
                RaisePropertyChanged(nameof(ListenerCount));
            }
        }

        public TimeSpan PlaybackPositionFromStart
        {
            get
            {
                var value = DateTime.Now - SystemTimeDifference - PlayStart;
                return value.TotalSeconds < 0 ? TimeSpan.FromSeconds(0) : value;
            }
        }
        public TimeSpan PlaybackPositionFromEnd
        {
            get
            {
                var value = Length - PlaybackPositionFromStart;
                return value > Length ? Length : value;
            }
        }
        private TimeSpan SystemTimeDifference { get; set; }

        public void UpdatePlaybackPosition()
        {
            RaisePropertyChanged(nameof(PlaybackPositionFromStart));
            RaisePropertyChanged(nameof(PlaybackPositionFromEnd));
        }


        public void Update(CurrentPlaying currentPlaying)
        {
            var difference = DateTime.Now - currentPlaying.SystemTime;
            SystemTimeDifference = difference + TimeSpan.FromSeconds(OFFSET);

            foreach (var property in GetType().GetProperties())
                if (property.CanWrite)
                    property.SetValue(this, GetType().GetProperty(property.Name).GetValue(currentPlaying));
        }
    }
}

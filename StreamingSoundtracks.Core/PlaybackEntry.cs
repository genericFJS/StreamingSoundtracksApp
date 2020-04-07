using Newtonsoft.Json;
using System;
using System.ComponentModel;
using System.Web;

namespace StreamingSoundtracks.Core
{
    public partial class PlaybackEntry : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private string album;
        private string track;
        private TimeSpan length;
        private string coverLink;
        private string thumbnailLink;
        private string siteLink;
        private string requestedBy;
        private string message;
        private TimeSpan timeToCurrentPlaying;

        public string Album
        {
            get
            {
                return album;
            }
            set
            {
                album = HttpUtility.HtmlDecode(value);
                RaisePropertyChanged(nameof(Album));
            }
        }
        public string Track
        {
            get
            {
                return track;
            }
            set
            {
                track = HttpUtility.HtmlDecode(value);
                RaisePropertyChanged(nameof(Track));
            }
        }
        [JsonConverter(typeof(TimeSpanJsonConverter))]
        public TimeSpan Length
        {
            get
            {
                return length;
            }
            set
            {
                length = value;
                RaisePropertyChanged(nameof(Length));
            }
        }
        public string CoverLink
        {
            get
            {
                return coverLink;
            }
            set
            {
                coverLink = value;
                RaisePropertyChanged(nameof(CoverLink));
            }
        }
        public string ThumbnailLink
        {
            get
            {
                return thumbnailLink;
            }
            set
            {
                thumbnailLink = value;
                RaisePropertyChanged(nameof(ThumbnailLink));
            }
        }
        public string SiteLink
        {
            get
            {
                return siteLink;
            }
            set
            {
                siteLink = HttpUtility.HtmlDecode(value);
                RaisePropertyChanged(nameof(SiteLink));
            }
        }
        public string RequestedBy
        {
            get
            {
                return requestedBy;
            }
            set
            {
                requestedBy = HttpUtility.HtmlDecode(value);
                RaisePropertyChanged(nameof(RequestedBy));
            }
        }
        public string Message
        {
            get
            {
                return message;
            }
            set
            {
                if (!(value is null))
                    value = value.Replace("\\'", "'").Replace("\\\"", "\"");
                message = HttpUtility.HtmlDecode(HttpUtility.HtmlDecode(value));
                RaisePropertyChanged(nameof(Message));
            }
        }
        public TimeSpan TimeToCurrentPlaying
        {
            get
            {
                return timeToCurrentPlaying;
            }
            set
            {
                timeToCurrentPlaying = value;
                RaisePropertyChanged(nameof(TimeToCurrentPlaying));
            }

        }

        protected void RaisePropertyChanged(string property)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
        }

        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return ToString().GetHashCode();
        }

        public override string ToString()
        {
            return $"{Track} ({Album}) {Length.ToString(@"mm\:ss")} - {RequestedBy} {Message}";
        }
    }
}

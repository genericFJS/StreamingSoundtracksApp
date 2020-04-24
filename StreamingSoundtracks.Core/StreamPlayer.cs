using System;
using System.ComponentModel;
using System.IO;
using Vlc.DotNet.Core;

namespace StreamingSoundtracks.Core
{
    public class StreamPlayer : INotifyPropertyChanged
    {
        private string[] Urls { get; } = new string[] {
            "http://hi5.streamingsoundtracks.com/",
            "http://hi.streamingsoundtracks.com/",
            "http://hi1.streamingsoundtracks.com:8000/"
        };

        public bool IsPlaying
        {
            get
            {
                return isOpening || VlcMediaPlayer.IsPlaying();
            }
        }
        private bool isMute = Properties.Settings.Default.IsMute;
        public bool IsMute
        {
            get
            {
                return isMute;
            }
            set
            {
                isMute = value;
                Properties.Settings.Default.IsMute = IsMute;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsMute)));
            }
        }

        private int volume = Properties.Settings.Default.Volume;
        public int Volume
        {
            get
            {
                return volume;
            }
            set
            {
                volume = value;
                IsMute = false;
                Properties.Settings.Default.Volume = Volume;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Volume)));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public VlcMediaPlayer VlcMediaPlayer { get; private set; }
        private DirectoryInfo LibVlcDirectory { get; }
        private bool isOpening = false;

        public StreamPlayer(DirectoryInfo libVlcDirectory)
        {
            LibVlcDirectory = libVlcDirectory;
            ResetPlayer();
        }

        /// <summary>
        /// Resetting player is necessary as stopping and starting may cause issues when the volume has been changed.
        /// https://github.com/ZeBobo5/Vlc.DotNet/issues/130
        /// https://github.com/ZeBobo5/Vlc.DotNet/issues/381
        /// </summary>
        public void ResetPlayer()
        {
            if (VlcMediaPlayer != null)
                VlcMediaPlayer.Dispose();
            VlcMediaPlayer = new VlcMediaPlayer(LibVlcDirectory, new string[] { "--aout=directsound" });
            VlcMediaPlayer.Stopped += VlcMediaPlayer_Stopped;
            VlcMediaPlayer.TimeChanged += VlcMediaPlayer_TimeChanged;
            VlcMediaPlayer.Playing += VlcMediaPlayer_Playing;
            VlcMediaPlayer.SetMedia(new Uri(Urls[0]));
            VlcMediaPlayer.Audio.Volume = Volume;
            VlcMediaPlayer.Audio.IsMute = IsMute;

        }

        private void VlcMediaPlayer_Playing(object sender, VlcMediaPlayerPlayingEventArgs e)
        {
            isOpening = false;
        }

        public void StartStreaming()
        {
            if (isOpening)
                return;
            ResetPlayer();
            isOpening = true;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsPlaying)));
            VlcMediaPlayer.Play();
        }

        public void StopStreaming()
        {
            isOpening = false;
            if (VlcMediaPlayer.IsPlaying())
                VlcMediaPlayer.Stop();
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsPlaying)));
        }

        private void VlcMediaPlayer_TimeChanged(object sender, VlcMediaPlayerTimeChangedEventArgs e)
        {
            var m = sender as VlcMediaPlayer;
            var a = m.Audio;
            if (IsMute && !VlcMediaPlayer.Audio.IsMute)
                VlcMediaPlayer.Audio.IsMute = true;
            if (!IsMute && VlcMediaPlayer.Audio.IsMute)
                VlcMediaPlayer.Audio.IsMute = false;
            if (VlcMediaPlayer.Audio.Volume != Volume)
                VlcMediaPlayer.Audio.Volume = Volume;
        }

        private void VlcMediaPlayer_Stopped(object sender, VlcMediaPlayerStoppedEventArgs e)
        {
            StopStreaming();
        }
    }
}

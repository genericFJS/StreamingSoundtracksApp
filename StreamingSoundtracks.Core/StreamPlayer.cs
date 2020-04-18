using DevHost.Shoutcast;
using NAudio.Wave;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace StreamingSoundtracks.Core
{
    public class StreamPlayer : INotifyPropertyChanged
    {

        private readonly int BUFFER_LENGTH = 65536;
        private long NextTrackPosition { get; set; } = 0;
        private string[] Urls { get; } = new string[] {
            "http://hi5.streamingsoundtracks.com/",
            "http://hi.streamingsoundtracks.com/",
            "http://hi1.streamingsoundtracks.com:8000/"
        };
        public Stream BufferedStream { get; private set; }

        private Thread FetchingThread { get; set; }
        private Thread PlaybackThread { get; set; }

        private bool isPlaying = false;
        public bool IsPlaying
        {
            get
            {
                return isPlaying;
            }
            set
            {
                isPlaying = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsPlaying)));
            }
        }

        private bool isMuted = Properties.Settings.Default.IsMuted;
        public bool IsMuted
        {
            get
            {
                return isMuted;
            }
            set
            {
                isMuted = value;
                Properties.Settings.Default.IsMuted = IsMuted;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsMuted)));
            }
        }

        private float volume = Properties.Settings.Default.Volume;
        public float Volume
        {
            get
            {
                return volume;
            }
            set
            {
                volume = value;
                IsMuted = false;
                Properties.Settings.Default.Volume = Volume;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Volume)));
            }
        }

        public event EventHandler NextTrackStarting;
        public event EventHandler PlaybackSecondElapsed;
        public event EventHandler PlaybackTenSecondsElapsed;
        public event EventHandler StreamingStarted;
        public event PropertyChangedEventHandler PropertyChanged;

        private readonly TaskScheduler guiDispatcher;

        public StreamPlayer()
        {
            if (SynchronizationContext.Current == null)
                SynchronizationContext.SetSynchronizationContext(new SynchronizationContext());
            guiDispatcher = TaskScheduler.FromCurrentSynchronizationContext();
        }

        public void StartStreaming()
        {

            IsPlaying = true;
            if (!(PlaybackThread is null))
                PlaybackThread.Abort();
            if (!(FetchingThread is null))
                FetchingThread.Abort();
            FetchingThread = new Thread(FetchOnlineStream);
            PlaybackThread = new Thread(PlayBufferedStream);
            FetchingThread.Start();
            PlaybackThread.Start();
        }

        public void StopStreaming()
        {
            IsPlaying = false;
        }

        private void FetchOnlineStream()
        {
            BufferedStream = new CircularStream(BUFFER_LENGTH * 6);
            Task.Factory.StartNew(() => { StreamingStarted?.Invoke(this, EventArgs.Empty); }, CancellationToken.None, TaskCreationOptions.None, guiDispatcher);
            using (var stream = new ShoutcastStream(Urls[0], "StreamingSoundtracksApp"))
            {
                stream.StreamTitleChanged += new EventHandler(delegate (object e, EventArgs a)
                {
                    NextTrackPosition = BufferedStream.Position;
                });
                byte[] buffer = new byte[BUFFER_LENGTH];
                int read;
                while ((read = stream.Read(buffer, 0, buffer.Length)) > 0)
                {
                    if (!IsPlaying)
                        return;
                    var currentPosition = BufferedStream.Position;
                    BufferedStream.Position = BufferedStream.Length;
                    BufferedStream.Write(buffer, 0, read);
                    BufferedStream.Position = currentPosition;
                }
            }
        }

        private void PlayBufferedStream()
        {
            while (BufferedStream is null || BufferedStream.Length < BUFFER_LENGTH * 3)
                Thread.Sleep(100);
            BufferedStream.Position = 0;
            while (true)
            {
                try
                {
                    using (WaveStream blockAlignedStream = new BlockAlignReductionStream(WaveFormatConversionStream.CreatePcmStream(new Mp3FileReader(BufferedStream))))
                    {
                        using (WaveOut waveOut = new WaveOut(WaveCallbackInfo.FunctionCallback()))
                        {
                            waveOut.Init(blockAlignedStream);
                            waveOut.Play();
                            var tick = 0;
                            while (waveOut.PlaybackState == PlaybackState.Playing)
                            {
                                if (!IsPlaying)
                                    return;
                                if (IsMuted && waveOut.Volume != 0)
                                    waveOut.Volume = 0;
                                if (!IsMuted && waveOut.Volume != Volume)
                                    waveOut.Volume = Volume;
                                if (NextTrackPosition != 0 && BufferedStream.Position >= NextTrackPosition)
                                {
                                    Task.Factory.StartNew(() => { NextTrackStarting?.Invoke(this, EventArgs.Empty); }, CancellationToken.None, TaskCreationOptions.None, guiDispatcher);
                                    NextTrackPosition = 0;
                                }
                                if (tick % 10 == 0)
                                    Task.Factory.StartNew(() => { PlaybackSecondElapsed?.Invoke(this, EventArgs.Empty); }, CancellationToken.None, TaskCreationOptions.None, guiDispatcher);
                                if (tick == 0)
                                    Task.Factory.StartNew(() => { PlaybackTenSecondsElapsed?.Invoke(this, EventArgs.Empty); }, CancellationToken.None, TaskCreationOptions.None, guiDispatcher);
                                tick = (tick + 1) % 100;
                                Thread.Sleep(100);
                            }
                        }
                    }
                }
                catch (InvalidOperationException e)
                {
                    Debug.WriteLine(e.Message);
                    Thread.Sleep(100);
                }
            }
        }
    }
}

using DevHost.Shoutcast;
using NAudio.Wave;
using StreamingSoundtracks.Core;
using System;
using System.IO;
using System.Reflection;
using System.Threading;

namespace TestConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            //var coord = new Coordinator();
            //Trace.Listeners.Add(new ConsoleTraceListener());
            //coord.StreamPlayer.Volume = 0.1f;
            //coord.StreamPlayer.StartStreaming();
            //Experiment2();
            ExperimentVLC();
        }

        private static void Experiment1()
        {
            long switchPos = 0;
            string title = "";
            // https://stackoverflow.com/questions/184683/play-audio-from-a-stream-using-c-sharp
            var url = "http://hi5.streamingsoundtracks.com";
            using (var ms = new MemoryStream())
            {
                Console.WriteLine("Start buffering");
                new Thread(delegate (object o)
                {
                    using (var stream = new ShoutcastStream(url))
                    {
                        stream.StreamTitleChanged += new EventHandler(delegate (object e, EventArgs a)
                        {
                            switchPos = ms.Position;
                            title = stream.StreamTitle;
                        });
                        byte[] buffer = new byte[65536]; // 64KB chunks
                        int read;
                        while ((read = stream.Read(buffer, 0, buffer.Length)) > 0)
                        {
                            var pos = ms.Position;
                            ms.Position = ms.Length;
                            ms.Write(buffer, 0, read);
                            ms.Position = pos;
                            //Console.WriteLine(stream.StreamTitle);
                        }
                    }
                }).Start();

                var playlist = new Playlist();
                playlist.UpdatePlaylists();
                foreach (var entry in playlist.History)
                {
                    Console.WriteLine(entry);
                }



                // Pre-buffering some data to allow NAudio to start playing
                while (ms.Length < 65536)
                {
                    //Console.WriteLine($"Waiting {ms.Length / 1024} < {65536 * 10 / 1024}");
                    Thread.Sleep(1000);
                }

                Console.WriteLine("Start playing");
                ms.Position = 0;
                using (WaveStream blockAlignedStream = new BlockAlignReductionStream(WaveFormatConversionStream.CreatePcmStream(new Mp3FileReader(ms))))
                {
                    using (WaveOut waveOut = new WaveOut(WaveCallbackInfo.FunctionCallback()))
                    {
                        waveOut.Init(blockAlignedStream);
                        waveOut.Play();
                        while (waveOut.PlaybackState == PlaybackState.Playing)
                        {
                            if (switchPos >= ms.Position)
                                Console.WriteLine(title);
                            System.Threading.Thread.Sleep(100);
                        }
                    }
                }
            }
        }

        private static void Experiment2()
        {
            long switchPos = 0;
            string title = "";
            // https://stackoverflow.com/questions/184683/play-audio-from-a-stream-using-c-sharp
            var url = "http://hi5.streamingsoundtracks.com";
            using (var ms = new MemoryStream())
            {
                Console.WriteLine("Start buffering");
                new Thread(delegate (object o)
                {
                    using (var stream = new ShoutcastStream(url))
                    {
                        stream.StreamTitleChanged += new EventHandler(delegate (object e, EventArgs a)
                        {
                            switchPos = ms.Position;
                            title = stream.StreamTitle;
                        });
                        byte[] buffer = new byte[65536]; // 64KB chunks
                        int read;
                        while ((read = stream.Read(buffer, 0, buffer.Length)) > 0)
                        {
                            var pos = ms.Position;
                            ms.Position = ms.Length;
                            ms.WriteAsync(buffer, 0, read);
                            ms.Position = pos;
                        }
                    }
                }).Start();

                // Pre-buffering some data to allow NAudio to start playing
                while (ms.Length < 65536 * 3)
                {
                    //Console.WriteLine($"Waiting {ms.Length / 1024} < {65536 * 10 / 1024}");
                    Thread.Sleep(1000);
                }

                Console.WriteLine("Start playing");
                ms.Position = 0;

                using (WaveStream blockAlignedStream = new BlockAlignReductionStream(WaveFormatConversionStream.CreatePcmStream(new Mp3FileReader(ms))))
                {
                    using (WaveOut waveOut = new WaveOut(WaveCallbackInfo.FunctionCallback()))
                    {
                        waveOut.Init(blockAlignedStream);
                        waveOut.Play();
                        while (waveOut.PlaybackState == PlaybackState.Playing)
                        {
                            if (switchPos >= ms.Position)
                                Console.WriteLine(title);
                            Thread.Sleep(100);
                        }
                    }
                }
            }
        }

        private static void ExperimentVLC()
        {
            var currentDirectory = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
            // Default installation path of VideoLAN.LibVLC.Windows
            var libDirectory = new DirectoryInfo(Path.Combine(currentDirectory, "libvlc", IntPtr.Size == 4 ? "win-x86" : "win-x64"));

            using (var mediaPlayer = new Vlc.DotNet.Core.VlcMediaPlayer(libDirectory))
            {

                //var mediaOptions = new[]
                //{
                //    //":sout=#rtp{sdp=rtsp://127.0.0.1:554/}",
                //    //":sout-keep"
                //};

                mediaPlayer.SetMedia(new Uri("http://hls1.addictradio.net/addictrock_aac_hls/playlist.m3u8"));

                mediaPlayer.Play();

                for (int i = 0; i <= 20; i++)
                {
                    Thread.Sleep(500);
                    mediaPlayer.Audio.Volume = i * 10;
                }

                Console.WriteLine("Streaming on rtsp://127.0.0.1:554/");
                Console.WriteLine("Press any key to exit");
                Console.ReadKey();
            }
        }

        private static void Stream_StreamTitleChanged(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }
    }
}

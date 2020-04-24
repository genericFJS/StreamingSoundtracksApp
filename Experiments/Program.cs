using StreamingSoundtracks.Core;
using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;

namespace TestConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            var currentDirectory = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
            // Default installation path of VideoLAN.LibVLC.Windows
            var libDirectory = new DirectoryInfo(Path.Combine(currentDirectory, "libvlc", IntPtr.Size == 4 ? "win-x86" : "win-x64"));
            var coord = new Coordinator(libDirectory);
            Trace.Listeners.Add(new ConsoleTraceListener());
            coord.StreamPlayer.StartStreaming();
            //ExperimentVLC();
            Console.ReadKey();
        }

        private static void ExperimentVLC()
        {
            var currentDirectory = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
            // Default installation path of VideoLAN.LibVLC.Windows
            var libDirectory = new DirectoryInfo(Path.Combine(currentDirectory, "libvlc", IntPtr.Size == 4 ? "win-x86" : "win-x64"));

            Trace.WriteLine($"VLC: {libDirectory}");
            using (var mediaPlayer = new Vlc.DotNet.Core.VlcMediaPlayer(libDirectory, new string[] { "-v" }))
            {

                //var mediaOptions = new[]
                //{
                //    //":sout=#rtp{sdp=rtsp://127.0.0.1:554/}",
                //    //":sout-keep"
                //};

                Console.WriteLine("Setting Media");
                mediaPlayer.SetMedia(new Uri("http://www.streamingsoundtracks.com/modules/Listen/MP3-hi.pls"));
                //mediaPlayer.SetMedia(new Uri("http://hi.streamingsoundtracks.com"));
                //mediaPlayer.SetMedia(new Uri("http://hls1.addictradio.net/addictrock_aac_hls/playlist.m3u8"));
                //mediaPlayer.SetMedia(new FileInfo(@"D:\jonatan\Music\Library\Adam Taylor\The Handmaid's Tale\01 Escapes Within (Elisabeth Moss Narration).mp3"));

                Console.WriteLine("Playing");
                mediaPlayer.Play();

                //Console.WriteLine("Fading in");
                //for (int i = 5; i <= 10; i++)
                //{
                //    Thread.Sleep(500);
                //    mediaPlayer.Audio.Volume = i * 10;
                //}

                Console.ReadKey();
                mediaPlayer.Stop();
            }
        }

        private static void Stream_StreamTitleChanged(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }
    }
}

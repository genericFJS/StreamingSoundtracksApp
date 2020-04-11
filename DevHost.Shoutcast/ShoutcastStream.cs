using System;
using System.IO;
using System.Net;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

/// <summary>
/// https://www.codeproject.com/Articles/19125/ShoutcastStream-Class
/// </summary>
namespace DevHost.Shoutcast
{
    /// <summary>
    /// Provides the functionality to receive a shoutcast media stream
    /// </summary>
    public class ShoutcastStream : Stream
    {
        private readonly int metaInt;
        private readonly Stream netStream;
        private int receivedBytes;
        private bool connected = false;

        /// <summary>
        /// Gets the title of the stream
        /// </summary>
        public string StreamTitle { get; private set; }

        /// <summary>
        /// Is fired, when a new StreamTitle is received
        /// </summary>
        public event EventHandler StreamTitleChanged;

        /// <summary>
        /// Creates a new ShoutcastStream and connects to the specified Url
        /// </summary>
        /// <param name="url">Url of the Shoutcast stream</param>
        public ShoutcastStream(string url, string userAgent = "VLC media player")
        {
            SetAllowUnsafeHeaderParsing20();

            HttpWebResponse response;

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.Headers.Clear();
            request.Headers.Add("Icy-MetaData", "1");
            request.KeepAlive = false;
            request.UserAgent = userAgent;

            response = (HttpWebResponse)request.GetResponse();

            metaInt = int.Parse(response.Headers["Icy-MetaInt"]);
            receivedBytes = 0;

            netStream = response.GetResponseStream();

            connected = true;
        }

        /// <summary>
        /// Parses the received Meta Info
        /// </summary>
        /// <param name="metaInfo"></param>
        private void ParseMetaInfo(byte[] metaInfo)
        {
            string metaString = Encoding.ASCII.GetString(metaInfo);

            string newStreamTitle = Regex.Match(metaString, "(StreamTitle=')(.*)(';StreamUrl)").Groups[2].Value.Trim();
            if (!newStreamTitle.Equals(StreamTitle))
            {
                StreamTitle = newStreamTitle;
                StreamTitleChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        /// <summary>
        /// Reads data from the ShoutcastStream.
        /// </summary>
        /// <param name="buffer">An array of bytes to store the received data from the ShoutcastStream.</param>
        /// <param name="offset">The location in the buffer to begin storing the data to.</param>
        /// <param name="count">The number of bytes to read from the ShoutcastStream.</param>
        /// <returns>The number of bytes read from the ShoutcastStream.</returns>
        public override int Read(byte[] buffer, int offset, int count)
        {
            try
            {
                if (receivedBytes == metaInt)
                {
                    int metaLen = netStream.ReadByte();
                    if (metaLen > 0)
                    {
                        byte[] metaInfo = new byte[metaLen * 16];
                        int len = 0;
                        while ((len += netStream.Read(metaInfo, len, metaInfo.Length - len)) < metaInfo.Length)
                            continue;
                        ParseMetaInfo(metaInfo);
                    }
                    receivedBytes = 0;
                }

                int bytesLeft = ((metaInt - receivedBytes) > count) ? count : (metaInt - receivedBytes);
                int result = netStream.Read(buffer, offset, bytesLeft);
                receivedBytes += result;
                return result;
            }
            catch (Exception e)
            {
                connected = false;
                Console.WriteLine(e.Message);
                return -1;
            }
        }

        /// <summary>
        /// Allows unsafe header parsing for Shoutcast headers.
        /// See https://social.msdn.microsoft.com/Forums/en-US/ff098248-551c-4da9-8ba5-358a9f8ccc57/how-do-i-enable-useunsafeheaderparsing-from-code-net-20.
        /// </summary>
        /// <returns>Success of enabeling unsafe header parsing.</returns>
        public static bool SetAllowUnsafeHeaderParsing20()
        {
            //Get the assembly that contains the internal class
            Assembly aNetAssembly = Assembly.GetAssembly(typeof(System.Net.Configuration.SettingsSection));
            if (aNetAssembly != null)
            {
                //Use the assembly in order to get the internal type for the internal class
                Type aSettingsType = aNetAssembly.GetType("System.Net.Configuration.SettingsSectionInternal");
                if (aSettingsType != null)
                {
                    //Use the internal static property to get an instance of the internal settings class.
                    //If the static instance isn't created allready the property will create it for us.
                    object anInstance = aSettingsType.InvokeMember("Section", BindingFlags.Static | BindingFlags.GetProperty | BindingFlags.NonPublic, null, null, new object[] { });

                    if (anInstance != null)
                    {
                        //Locate the private bool field that tells the framework is unsafe header parsing should be allowed or not
                        FieldInfo aUseUnsafeHeaderParsing = aSettingsType.GetField("useUnsafeHeaderParsing", BindingFlags.NonPublic | BindingFlags.Instance);
                        if (aUseUnsafeHeaderParsing != null)
                        {
                            aUseUnsafeHeaderParsing.SetValue(anInstance, true);
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// Closes the ShoutcastStream.
        /// </summary>
        public override void Close()
        {
            connected = false;
            netStream.Close();
        }

        /// <summary>
        /// Gets a value that indicates whether the ShoutcastStream supports reading.
        /// </summary>
        public override bool CanRead { get { return connected; } }

        /// <summary>
        /// Gets a value that indicates whether the ShoutcastStream supports seeking.
        /// This property will always be false.
        /// </summary>
        public override bool CanSeek { get; } = false;

        /// <summary>
        /// Gets a value that indicates whether the ShoutcastStream supports writing.
        /// This property will always be false.
        /// </summary>
        public override bool CanWrite { get; } = false;

        /// <summary>
        /// Flushes data from the stream.
        /// This method is currently not supported
        /// </summary>
        public override void Flush() { return; }

        /// <summary>
        /// Gets the length of the data available on the Stream.
        /// This property is not currently supported and always thows a <see cref="NotSupportedException"/>.
        /// </summary>
        public override long Length
        {
            get { throw new NotSupportedException(); }
        }

        /// <summary>
        /// Gets or sets the current position in the stream.
        /// This property is not currently supported and always thows a <see cref="NotSupportedException"/>.
        /// </summary>
        public override long Position
        {
            get
            {
                throw new NotSupportedException();
            }
            set
            {
                throw new NotSupportedException();
            }
        }

        /// <summary>
        /// Sets the current position of the stream to the given value.
        /// This Method is not currently supported and always throws a <see cref="NotSupportedException"/>.
        /// </summary>
        /// <param name="offset"></param>
        /// <param name="origin"></param>
        /// <returns></returns>
        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Sets the length of the stream.
        /// This Method always throws a <see cref="NotSupportedException"/>.
        /// </summary>
        /// <param name="value"></param>
        public override void SetLength(long value)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Writes data to the ShoutcastStream.
        /// This method is not currently supported and always throws a <see cref="NotSupportedException"/>.
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="count"></param>
        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotSupportedException();
        }
    }
}

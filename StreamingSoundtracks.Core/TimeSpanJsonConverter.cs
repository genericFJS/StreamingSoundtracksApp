using Newtonsoft.Json;
using System;

namespace StreamingSoundtracks.Core
{
    public partial class PlaybackEntry
    {
        public class TimeSpanJsonConverter : JsonConverter
        {

            public override bool CanConvert(Type objectType)
            {
                return objectType == typeof(TimeSpan);
            }

            public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
            {
                return TimeSpan.FromMilliseconds(double.Parse((string)reader.Value));
            }

            public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
            {
                throw new NotImplementedException();
            }
        }
    }
}

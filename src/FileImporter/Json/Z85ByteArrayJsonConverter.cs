using System;
using Newtonsoft.Json;

namespace FileImporter.Json
{
    /// <summary>
    /// JsonConverter to convert byte[] to arrays containing the integer value for each byte.
    /// </summary>
    /// <remarks>http://stackoverflow.com/questions/15226921/how-to-serialize-byte-as-simple-json-array-and-not-as-base64-in-json-net</remarks>
    public sealed class Z85ByteArrayJsonConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (value == null)
            {
                writer.WriteNull();
                return;
            }

            var data = (byte[])value;

            var encodedData = CoenM.Encoding.Z85Extended.Encode(data);
            writer.WriteValue(encodedData);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            do
            {
                switch (reader.TokenType)
                {
                    case JsonToken.String:
                        var encodedData = reader.Value.ToString();
                        return CoenM.Encoding.Z85Extended.Decode(encodedData);

                    case JsonToken.Null:
                        return null;

                    case JsonToken.Comment:
                        // skip
                        break;

                    default:
                        throw new Exception($"Unexpected token when reading bytes: {reader.TokenType}");
                }
            } while (reader.Read());

            throw new Exception("Unexpected end when reading bytes.");
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(byte[]);
        }
    }
}
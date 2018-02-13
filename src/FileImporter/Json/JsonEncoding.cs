using System.Diagnostics;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace EagleEye.FileImporter.Json
{
    public static class JsonEncoding
    {
        private static readonly JsonSerializerSettings JsonSerializerSettings;

        private static readonly JsonSerializer Serializer;

        static JsonEncoding()
        {
            JsonSerializerSettings = new JsonSerializerSettings();
            JsonSerializerSettings.Converters.Add(new StringEnumConverter());
            JsonSerializerSettings.Converters.Add(new Z85ByteArrayJsonConverter());

            Serializer = JsonSerializer.Create(JsonSerializerSettings);
            Serializer.Formatting = Formatting.Indented;
        }

        public static string Serialize(object input)
        {
            return JsonConvert.SerializeObject(input, Formatting.Indented, JsonSerializerSettings);
        }

        public static T Deserialize<T>(string input)
        {
            return JsonConvert.DeserializeObject<T>(input, JsonSerializerSettings);
        }

        public static T ReadFromFile<T>(string filename)
        {
            T result;
            using (var file = File.OpenText(filename))
            {
                result = (T)Serializer.Deserialize(file, typeof(T));
            }
            return result;
        }

        public static void WriteDataToJsonFile(object obj, string filename)
        {
            Debug.Assert(string.IsNullOrWhiteSpace(filename) == false);
            Debug.Assert(obj != null);

            using (var streamWriter = File.CreateText(filename))
            {
                Serializer.Serialize(streamWriter, obj);
            }
        }
    }
}
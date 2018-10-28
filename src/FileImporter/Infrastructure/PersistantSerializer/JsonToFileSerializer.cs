namespace EagleEye.FileImporter.Infrastructure.PersistantSerializer
{
    using System;

    using EagleEye.FileImporter.Json;

    public class JsonToFileSerializer<T> : IPersistantSerializer<T> where T : new()
    {
        private readonly string filename;

        public JsonToFileSerializer(string filename)
        {
            this.filename = filename;
        }

        public T Load()
        {
            try
            {
                var result = JsonEncoding.ReadFromFile<T>(filename);
                if (result == null)
                    return new T();
                return result;
            }
            catch (Exception)
            {
                return new T();
            }
        }

        public void Save(T data)
        {
            Json.JsonEncoding.WriteDataToJsonFile(data, filename);
        }
    }
}
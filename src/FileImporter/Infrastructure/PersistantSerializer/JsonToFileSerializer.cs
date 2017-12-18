using System;
using FileImporter.Json;

namespace FileImporter.Infrastructure.PersistantSerializer
{
    public class JsonToFileSerializer<T> : IPersistantSerializer<T> where T : new()
    {
        private readonly string _filename;

        public JsonToFileSerializer(string filename)
        {
            _filename = filename;
        }

        public T Load()
        {
            try
            {
                var result = JsonEncoding.ReadFromFile<T>(_filename);
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
            Json.JsonEncoding.WriteDataToJsonFile(data, _filename);
        }
    }
}
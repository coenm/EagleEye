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
            return Json.JsonEncoding.ReadFromFile<T>(_filename);
        }

        public void Save(T data)
        {
            Json.JsonEncoding.WriteDataToJsonFile(data, _filename);
        }
    }
}
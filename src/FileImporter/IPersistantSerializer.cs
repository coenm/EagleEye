namespace EagleEye.FileImporter
{
    public interface IPersistantSerializer<T> where T : new()
    {
        T Load();

        void Save(T data);
    }
}
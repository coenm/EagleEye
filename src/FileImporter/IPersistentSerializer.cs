namespace EagleEye.FileImporter
{
    public interface IPersistentSerializer<T>
        where T : new()
    {
        T Load();

        void Save(T data);
    }
}

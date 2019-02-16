namespace EagleEye.Photo.Domain.Services
{
    internal interface IFilenameRepository
    {
        bool Contains(string filename);

        void Add(string filename);
    }
}

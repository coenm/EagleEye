namespace EagleEye.Photo.Domain.Services
{
    internal interface IMediaFilenameRepository
    {
        bool Contains(string filename);

        void Add(string filename);
    }
}

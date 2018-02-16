namespace EagleEye.Picasa
{
    using System;
    using System.Threading.Tasks;

    using EagleEye.Picasa.Picasa;

    public interface IPicasaService : IDisposable
    {
        Task<FileWithPersons> GetDataAsync(string filename);
    }
}